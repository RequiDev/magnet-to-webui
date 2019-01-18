using System;
using System.Diagnostics;
using System.Net;
using System.Security.Principal;
using Microsoft.Win32;
using RestSharp;

namespace magnet_to_webui
{
	internal class Program
	{
		private const string WebUrl = "YOUR_WEBUI_URL";
		private const string Username = "YOUR_USERNAME";
		private const string Password = "YOUR_PASSWORD";
		private const string SavePath = "YOUR_SAVE_PATH";

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
				key?.CreateSubKey("shell")?.CreateSubKey("open")?.CreateSubKey("command")?.SetValue("", $"\"{System.Reflection.Assembly.GetEntryAssembly().Location}\" \"%1\"");
				return;
			}

			var magnetUrl = args[0];

			var client = new RestClient();
			var request = new RestRequest();
			var cookies = new CookieContainer();
			client.CookieContainer = cookies;

			client.BaseUrl = new Uri($"{WebUrl}/login");
			request.Method = Method.POST;
			request.AddParameter("username", Username);
			request.AddParameter("password", Password);

			client.Execute(request);
			request.Parameters.Clear();

			client.BaseUrl = new Uri($"{WebUrl}/command/download");
			request.Method = Method.POST;
			request.AddParameter("urls", magnetUrl);

			request.AddParameter("savepath", SavePath);

			client.Execute(request);
			request.Parameters.Clear();

			// gotta do it clean, you know?
			client.BaseUrl = new Uri($"{WebUrl}/logout");
			request.Method = Method.POST;

			client.Execute(request);
		}
	}
}
