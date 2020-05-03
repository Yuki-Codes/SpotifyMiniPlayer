using System;
using System.IO;
using System.Text.Json;

namespace SpotifyMini
{
	[Serializable]
	public class Settings
	{
		private const string path = "settings.json";

		public double WindowScale { get; set; } = 1.0;

		public static Settings Load()
		{
			if (!File.Exists(path))
				return new Settings();
			
			string json = File.ReadAllText(path);
			return JsonSerializer.Deserialize<Settings>(json);
		}

		public void Save()
		{
			string json = JsonSerializer.Serialize(this);
			File.WriteAllText(path, json);
		}
	}
}
