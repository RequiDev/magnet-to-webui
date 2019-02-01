using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Windows.Forms;
using Microsoft.Win32;
using RestSharp;
using Newtonsoft.Json;

namespace magnet_to_webui
{
	internal class TorrentSettings
	{
		public string WebUrl { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string SavePath { get; set; }
	}

	internal class Program
	{
		private static TorrentSettings _torrentSettings = new TorrentSettings();

		private static void Main(string[] args)
		{
			if (args.Length == 0 || !args[0].StartsWith("magnet"))
			{
				if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
				{
					var p = new Process
					{
						StartInfo =
						{
							FileName = System.Reflection.Assembly.GetEntryAssembly().Location,
							UseShellExecute = true,
							Verb = "runas"
						}
					};
					p.Start();
					return;
				}

				var key = Registry.ClassesRoot.CreateSubKey("magnet");
				key?.SetValue("", "URL:magnet");
				key?.SetValue("URL Protocol", "");
				key?.SetValue("FriendlyTypeName", "Magnet URL");
				key?.CreateSubKey("shell")?.CreateSubKey("open")?.CreateSubKey("command")?.SetValue("", $"\"{System.Reflection.Assembly.GetEntryAssembly().Location}\" \"%1\"");
				return;
			}

			if (!File.Exists("Torrent.json"))
			{
				File.WriteAllText("Torrent.json", JsonConvert.SerializeObject(_torrentSettings, Formatting.Indented));
				return;
			}

			_torrentSettings = JsonConvert.DeserializeObject<TorrentSettings>(File.ReadAllText("Torrent.json"));

			var magnetUrl = args[0];

			var client = new RestClient();
			var request = new RestRequest();
			var cookies = new CookieContainer();
			client.CookieContainer = cookies;

			client.BaseUrl = new Uri($"{_torrentSettings.WebUrl}/login");
			request.Method = Method.POST;
			request.AddParameter("username", _torrentSettings.Username);
			request.AddParameter("password", _torrentSettings.Password);

			client.Execute(request);
			request.Parameters.Clear();

			client.BaseUrl = new Uri($"{_torrentSettings.WebUrl}/command/download");
			request.Method = Method.POST;
			request.AddParameter("urls", magnetUrl);
			request.AddParameter("savepath", $"{_torrentSettings.SavePath}");

			client.Execute(request);
			request.Parameters.Clear();

			// gotta do it clean, you know?
			client.BaseUrl = new Uri($"{_torrentSettings.WebUrl}/logout");
			request.Method = Method.POST;

			client.Execute(request);
		}
	}
}
