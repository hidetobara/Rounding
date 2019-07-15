using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Rounding
{
	using Hash = Dictionary<string, System.Object>;

	public class CharacterController : MonoBehaviour
	{
		private string _ID = null;
		private int _Life = 0;
		private GameObject _Character;
		private Rigidbody2D _Rigidbody;
		private SpriteRenderer _Renderer;
		private Vector2 _Direction;
		private NetConnection _Connection;
		private int _Count = 5;
		public Camera CharacterCamera;
		public Vector2 CameraPosition;
		public Sprite SpriteUp, SpriteDown, SpriteLeft, SpriteRight;

		void Start()
		{
			_Character = this.gameObject;
			_Rigidbody = GetComponent<Rigidbody2D>();
			_Renderer = GetComponent<SpriteRenderer>();
			_Direction = new Vector2(0, -1);
			_Life = 100;

			_Connection = NetConnection.Singleton();
			_Connection.Connect("ws://localhost:12345/chat");
			_Connection.OnReceive += OnReceive;
		}

		// Update is called once per frame
		void Update()
		{
			// key
			if (Input.GetKey(KeyCode.LeftArrow))
			{
				MoveCharacter(new Vector2(-1, 0));
				_Renderer.sprite = SpriteLeft;
				_Direction = new Vector2(-1, 0);
			}
			if (Input.GetKey(KeyCode.RightArrow))
			{
				MoveCharacter(new Vector2(1, 0));
				_Renderer.sprite = SpriteRight;
				_Direction = new Vector2(1, 0);
			}
			if (Input.GetKey(KeyCode.UpArrow))
			{
				MoveCharacter(new Vector2(0, 1));
				_Renderer.sprite = SpriteUp;
				_Direction = new Vector2(0, 1);
			}
			if (Input.GetKey(KeyCode.DownArrow))
			{
				MoveCharacter(new Vector2(0, -1));
				_Renderer.sprite = SpriteDown;
				_Direction = new Vector2(0, -1);
			}
			if (Input.GetKeyDown(KeyCode.Space))
			{
				var o = SwordController.Create();
				o.Fire(this.transform.position, _Direction);
			}
			// camera
			CameraPosition = (CameraPosition * 3 + new Vector2(this.transform.position.x, this.transform.position.y)) / 4;
			CharacterCamera.transform.position = new Vector3(CameraPosition.x, CameraPosition.y, CharacterCamera.transform.position.z);
			// connection
			Send();
		}

		private void MoveCharacter(Vector2 v)
		{
			_Rigidbody.velocity += v * 0.5f;
		}

		private void Send()
		{
			if (_ID == null) return;
			_Count--;
			if (_Count > 0) return;
			Dictionary<string, object> hash = new Dictionary<string, object>();
			hash["id"] = _ID;
			hash["life"] = _Life;




			hash["pos"] = ToArray(this.transform.position);
			var json = Json.Serialize(hash);
			_Connection.Enqueue(json);
			_Count = 30;
		}

		private double[] ToArray(Vector3 v)
		{
			return new double[3] { v.x, v.y, v.z };
		}

		private void OnReceive(Hash hash)
		{
			if (hash.ContainsKey("_id")) _ID = hash["_id"].ToString();
		}
	}
}