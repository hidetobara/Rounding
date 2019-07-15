using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;


namespace Rounding
{
	using Hash = Dictionary<string, System.Object>;
	public delegate void HashEventHandler(Hash hash);

	public class NetConnection : MonoBehaviour
	{
		private static NetConnection _Instance = null;
		public static NetConnection Singleton()
		{
			if (_Instance == null) _Instance = FindObjectOfType<NetConnection>();
			if (_Instance == null)
			{
				var o = new GameObject();
				o.name = "NetConnection";
				_Instance = o.AddComponent<NetConnection>();
			}
			return _Instance;
		}

		private WebSocket _WebSocket = null;
		private Queue<string> _Messages = new Queue<string>();

		public HashEventHandler OnReceive;

		public void Connect(string uri)
		{
			if (_WebSocket != null) _WebSocket.Close();
			print("connecting... " + uri);
			try
			{
				_WebSocket = new WebSocket(uri);
				_WebSocket.OnOpen += OnOpen;
				_WebSocket.OnMessage += OnMessage;
				_WebSocket.OnError += OnError;
				_WebSocket.OnClose += OnClose;
				_WebSocket.Connect();
			}
			catch(Exception ex)
			{
				print(ex.Message + "@" + ex.StackTrace);
			}
		}

		public void Enqueue(string m)
		{
			lock (_Messages) { _Messages.Enqueue(m); }
		}

		private void OnOpen(System.Object o, EventArgs e)
		{
			print("OPEN");
		}

		private void OnMessage(System.Object o, MessageEventArgs e)
		{
			print("\t" + e.Data);
			try
			{
				Hash hash = Json.Deserialize(e.Data) as Hash;
				if (hash != null && OnReceive != null) OnReceive(hash);
			}
			catch(Exception ex)
			{
				Debug.LogWarning(ex.Message);
			}
		}

		private void OnError(System.Object o, ErrorEventArgs e)
		{
			print("ERROR=" + e.Message);
		}

		private void OnClose(System.Object o, CloseEventArgs e)
		{
			print("CLOSE=" + e.Reason);
		}

		void Update()
		{
			if (_WebSocket == null || _WebSocket.ReadyState != WebSocketState.Open) return;
			lock (_Messages)
			{
				if (_Messages.Count == 0) return;
				_WebSocket.Send(_Messages.Dequeue());
			}
		}
	}
}