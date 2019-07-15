using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Rounding
{
	public class SwordController : MonoBehaviour
	{
		static public SwordController Create()
		{
			GameObject o = Instantiate(Resources.Load("Sword")) as GameObject;
			//print("sword:" + o);
			return o.GetComponent<SwordController>();
		}

		// Start is called before the first frame update
		void Start()
		{
			StartCoroutine(Starting());
		}

		public void Fire(Vector3 position, Vector2 direction)
		{
			print("fire:" + position + " " + direction);
			float bias = 1.0f;
			this.transform.position = position + new Vector3(direction.x, direction.y, 0) * bias;
			Rigidbody2D r = GetComponent<Rigidbody2D>();
			r.velocity = direction;
			if (direction.x < -0.1f) this.transform.rotation = Quaternion.Euler(0, 0, -90);
			else if (direction.x > 0.1f) this.transform.rotation = Quaternion.Euler(0, 0, 90);
			else if (direction.y < -0.1f) this.transform.rotation = Quaternion.Euler(0, 0, 0);
			else if (direction.y > 0.1f) this.transform.rotation = Quaternion.Euler(0, 0, 180);
		}

		private IEnumerator Starting()
		{
			yield return new WaitForSeconds(3.0f);
			Destroy(this.gameObject);
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			print("tag:" + collision.gameObject.tag);
			if (collision.gameObject.tag == "Player") return;
			Destroy(this.gameObject);
		}
	}
}