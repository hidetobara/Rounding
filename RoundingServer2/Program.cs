using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.WebSockets;


namespace RoundingServer2
{
	using Hash = Dictionary<string, Object>;

	class Program
	{
#if DEBUG
		const string LISTEN_URI = "http://localhost:12345/";
#else
		const string LISTEN_URI = "http://+:12345/";
#endif
		const int BUFFER_MAX = 1024;
		static GameManager _Manager;

		static void Main(string[] args)
		{
			_Manager = new GameManager();

			StartServer();
			Console.WriteLine("{0}:Server start.\nPress any key to exit.", DateTime.Now.ToString());
			Console.ReadKey();
			Parallel.ForEach(_Manager.GetSockets(), p =>
			{
				if (p.State == WebSocketState.Open) p.CloseAsync(WebSocketCloseStatus.NormalClosure, "", System.Threading.CancellationToken.None);
			});
		}

		/// <summary>
		/// WebSocketサーバースタート
		/// </summary>
		static async void StartServer()
		{
			/// httpListenerで待ち受け
			var httpListener = new HttpListener();
			httpListener.Prefixes.Add(LISTEN_URI);
			httpListener.Start();

			while (true)
			{
				/// 接続待機
				var listenerContext = await httpListener.GetContextAsync();
				if (listenerContext.Request.IsWebSocketRequest)
				{
					/// httpのハンドシェイクがWebSocketならWebSocket接続開始
					ProcessRequest(listenerContext);
				}
				else
				{
					/// httpレスポンスを返す
					listenerContext.Response.StatusCode = 400;
					listenerContext.Response.Close();
				}
			}
		}

		/// <summary>
		/// WebSocket接続毎の処理
		/// </summary>
		/// <param name="listenerContext"></param>
		static async void ProcessRequest(HttpListenerContext listenerContext)
		{
			Console.WriteLine("{0}:NewSession from={1}", DateTime.Now.ToString(), listenerContext.Request.RemoteEndPoint.Address.ToString());

			var buffer = new ArraySegment<byte>(new byte[BUFFER_MAX]);
			string json;

			/// WebSocketの接続完了を待機してWebSocketオブジェクトを取得する
			var ws = (await listenerContext.AcceptWebSocketAsync(subProtocol: null)).WebSocket;

			/// 新規クライアントを追加
			var myUser = _Manager.Create(ws);
			json = Json.Serialize(myUser.ToCreatedHash());
			await ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(json)), WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);

			/// WebSocketの送受信ループ
			while (ws.State == WebSocketState.Open)
			{
				try
				{
					/// 受信待機
					var ret = await ws.ReceiveAsync(buffer, System.Threading.CancellationToken.None);

					/// テキスト
					if (ret.MessageType == WebSocketMessageType.Text)
					{
						Console.WriteLine("{0}:Received:{1}", DateTime.Now.ToString(), listenerContext.Request.RemoteEndPoint.Address.ToString());
						json = Encoding.UTF8.GetString(buffer.Take(ret.Count).ToArray());
						Console.WriteLine("Json={0}", json);
						var hash = Json.Deserialize(json) as Hash;
						_Manager.Update(myUser.ID, hash);

						/// 各クライアントへ配信
						json = Json.Serialize(_Manager.ToHash());
						await ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(json)), WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
					}
					else if (ret.MessageType == WebSocketMessageType.Close) /// クローズ
					{
						Console.WriteLine("{0}:Session Close:{1}", DateTime.Now.ToString(), listenerContext.Request.RemoteEndPoint.Address.ToString());
						break;
					}
				}
				catch(Exception ex)
				{
					/// 例外 クライアントが異常終了
					Console.WriteLine("{0}:Session Abort:{1}\n\t{2}", DateTime.Now.ToString(), listenerContext.Request.RemoteEndPoint.Address.ToString(), ex.Message);
					break;
				}
			}

			/// クライアントを除外する
			_Manager.Destroy(myUser.ID);
			ws.Dispose();

		}
	}
}