using System;
using WebSocketSharp;
using WebSocketSharp.Server;


namespace RoundingServer
{
	public class Echo : WebSocketBehavior
	{
		protected override void OnMessage(MessageEventArgs e)
		{
			var name = Context.QueryString["id"];
			var pos = Context.QueryString["pos"];
			Console.WriteLine(name + " " + pos);
			//Send(!name.IsNullOrEmpty() ? String.Format("\"{0}\" to {1}", e.Data, name) : e.Data);
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World !");
			var wssv = new WebSocketServer(System.Net.IPAddress.Any, 12345);

			wssv.AddWebSocketService<Echo>("/echo");

			wssv.Start();
			if (wssv.IsListening)
			{
				Console.WriteLine("Listening on port {0}, and providing WebSocket services:", wssv.Port);
				
				foreach (var path in wssv.WebSocketServices.Paths)
					Console.WriteLine("- {0}", path);
			}

			Console.WriteLine("\nPress Enter key to stop the server...");
			Console.ReadLine();

			wssv.Stop();
		}
	}
}
