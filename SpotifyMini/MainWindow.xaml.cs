namespace SpotifyMini
{
	using System;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls.Primitives;
	using System.Windows.Media.Imaging;
	using SpotifyAPI.Web;
	using SpotifyAPI.Web.Auth;
	using SpotifyAPI.Web.Enums;
	using SpotifyAPI.Web.Models;

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public const string ClientId = null;
		public const string ClientSecret = null;

		private bool isPlaying;
		private SpotifyWebAPI api;

		public MainWindow()
		{
			InitializeComponent();

			this.Controls.Opacity = 0.0;

			AuthorizationCodeAuth auth = new AuthorizationCodeAuth(
				ClientId,
				ClientSecret,
				"http://localhost:4002",
				"http://localhost:4002",
				Scope.UserReadPlaybackState | Scope.UserModifyPlaybackState | Scope.UserReadCurrentlyPlaying);

			auth.AuthReceived += AuthOnAuthReceived;
			auth.Start();
			auth.OpenBrowser();
		}

		private async void AuthOnAuthReceived(object sender, AuthorizationCode payload)
		{
			AuthorizationCodeAuth auth = (AuthorizationCodeAuth)sender;
			auth.Stop();

			Token token = await auth.ExchangeCode(payload.Code);

			this.api = new SpotifyWebAPI
			{
				AccessToken = token.AccessToken,
				TokenType = token.TokenType
			};

			while (true)
			{
				await Task.Delay(1000);

				try
				{
					if (token.IsExpired())
					{
						token = await auth.ExchangeCode(payload.Code);
					}

					PlaybackContext playing = await api.GetPlayingTrackAsync();

					if (playing == null || playing.Item == null)
						continue;

					if (Application.Current == null)
						return;

					Application.Current.Dispatcher.Invoke(() =>
					{
						this.isPlaying = playing.IsPlaying;

						if (playing.Item.Album.Images.Count > 0)
						{
							this.Background.ImageSource = new BitmapImage(new Uri(playing.Item.Album.Images[0].Url));
							this.Background.Opacity = 1;
						}
						else
						{
							this.Background.Opacity = 0;
						}

						this.AlbumName.Text = playing.Item.Name + " - " + playing.Item.Album.Name;
						this.PauseIcon.Visibility = this.isPlaying ? Visibility.Visible : Visibility.Hidden;
						this.PlayIcon.Visibility = this.isPlaying ? Visibility.Hidden : Visibility.Visible;
					});
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
					Console.WriteLine(ex);
				}
			}
		}

		private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			this.DragMove();
		}

		private void Border_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			this.Controls.Animate(Window.OpacityProperty, 1.0, 250);
		}

		private void Border_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			this.Controls.Animate(Window.OpacityProperty, 0.0, 250);
		}

		private void OnClose(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void OnPause(object sender, RoutedEventArgs e)
		{
			if (this.isPlaying)
			{
				this.api.PausePlayback();
			}
			else
			{
				this.api.ResumePlayback("", "", null, (int?)null, 0);
			}

			this.isPlaying = !this.isPlaying;
			this.PauseIcon.Visibility = this.isPlaying ? Visibility.Visible : Visibility.Hidden;
			this.PlayIcon.Visibility = this.isPlaying ? Visibility.Hidden : Visibility.Visible;
		}

		private void OnSkipForward(object sender, RoutedEventArgs e)
		{
			this.api.SkipPlaybackToNext();
		}

		private void OnSkipBack(object sender, RoutedEventArgs e)
		{
			this.api.SkipPlaybackToPrevious();
		}

		private void OnHeart(object sender, RoutedEventArgs e)
		{
		}

		private void OnPin(object sender, RoutedEventArgs e)
		{
			this.Topmost = !this.Topmost;
		}

		private void OnResizeDrag(object sender, DragDeltaEventArgs e)
		{
			double scale = this.WindowScale.ScaleX;

			double delta = Math.Max(e.HorizontalChange / 1024, e.VerticalChange / 576);
			scale += delta;

			scale = Math.Clamp(scale, 0.5, 2.0);
			this.WindowScale.ScaleX = scale;
			this.WindowScale.ScaleY = scale;
		}
	}
}
