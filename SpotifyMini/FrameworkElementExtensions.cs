namespace System.Windows
{
	using System;
	using System.Windows.Media;
	using System.Windows.Media.Animation;

	public static class FrameworkElementExtensions
	{
		public static void Animate(this FrameworkElement self, DependencyProperty property, double to, int durationMs)
		{
			Storyboard story = new Storyboard();
			DoubleAnimation anim = new DoubleAnimation(to, new Duration(TimeSpan.FromMilliseconds(durationMs)));
			anim.EasingFunction = new QuadraticEase();
			Storyboard.SetTarget(anim, self);
			Storyboard.SetTargetProperty(anim, new PropertyPath(property));
			story.Children.Add(anim);
			story.Begin();
		}

		public static void Animate(this Animatable self, DependencyProperty property, double from, double to, int durationMs)
		{
			DoubleAnimation anim = new DoubleAnimation(from, to, new Duration(TimeSpan.FromMilliseconds(durationMs)));
			anim.EasingFunction = new QuadraticEase();
			self.BeginAnimation(property, anim);
		}
	}
}
