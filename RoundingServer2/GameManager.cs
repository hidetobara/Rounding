using System;
using System.Collections.Generic;
using System.Text;
using System.Net.WebSockets;


namespace RoundingServer2
{
	using Hash = Dictionary<string, Object>;

	class GameManager
	{
		Dictionary<string, GameUser> _Users = new Dictionary<string, GameUser>();
		int _Passed = 0;

		public GameUser Create(WebSocket socket)
		{
			GameUser u = null;
			for(int life = 3; life >= 0; life--)
			{
				Guid g = System.Guid.NewGuid();
				string id = g.ToString("N").Substring(0, 4);
				if (_Users.ContainsKey(id)) continue;
				u = new GameUser() { Socket = socket, ID = id };
				_Users[id] = u;
				break;
			}
			return u;
		}

		public void Clear() { _Users.Clear(); _Passed = 0; }
		public void Destroy(string id) { _Users.Remove(id); }

		public void Update(string id, Hash h)
		{
			UpdatePosition(id, h["pos"]);
			UpdateLife(id, h["life"]);
		}

		public void UpdatePosition(string id, Object o)
		{
			if (!_Users.ContainsKey(id)) return;
			var u = _Users[id];
			var pos = o as List<System.Object>;
			if (u == null || pos == null) return;
			u.pos[0] = (double)pos[0]; u.pos[1] = (double)pos[1]; u.pos[2] = (double)pos[2];
			_Passed += 1;
		}

		public void UpdateLife(string id, Object o)
		{
			if (!_Users.ContainsKey(id)) return;
			var u = _Users[id];
			var life = (long)o;
			if (u == null) return;
			u.life = (int)life;
			_Passed += 1;
		}

		public Hash ToHash()
		{
			Hash h = new Hash();
			h["_passed"] = _Passed;
			foreach(var pair in _Users)
			{
				h[pair.Key] = pair.Value.ToIntervalHash();
			}
			return h;
		}

		public List<WebSocket> GetSockets()
		{
			List<WebSocket> list = new List<WebSocket>();
			foreach (var s in _Users.Values) if (s.Socket != null) list.Add(s.Socket);
			return list;
		}
	}

	class GameUser
	{
		public WebSocket Socket;
		public string ID;
		public double[] pos = new double[3];
		public int life;
		public Hash ToCreatedHash()
		{
			Hash h = new Hash();
			h["_id"] = ID;
			return h;
		}
		public Hash ToIntervalHash()
		{
			Hash h = new Hash();
			h["pos"] = new List<double>(pos);
			h["life"] = life;
			return h;
		}
	}
}
