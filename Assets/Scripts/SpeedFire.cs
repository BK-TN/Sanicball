using UnityEngine;
using System.Collections;
using Sanicball.Gameplay;

namespace Sanicball {
	public class SpeedFire : MonoBehaviour {
		private Ball ball;
		Rigidbody rb;
		MeshRenderer mr;
		AudioSource asrc;

		float rot = 0;

		public void Init(Ball ball) {

			this.ball = ball;
			rb = ball.GetComponent<Rigidbody>();
			transform.localScale = ball.transform.localScale;
		}

		// Use this for initialization
		void Start () {
			mr = GetComponent<MeshRenderer>();
			asrc = GetComponent<AudioSource>();
		}
		
		// Update is called once per frame
		void Update () {
			if (!ball) {
				Destroy(gameObject);
				return;
			}

			float power = Mathf.InverseLerp(120, 500, rb.velocity.magnitude);
			power = power * power;

			rot += Time.deltaTime * 1000;
			transform.position = ball.transform.position;
			Vector3 look = rb.velocity;
			if (look == Vector3.zero)
			{
				look = Vector3.forward;
			}
			Quaternion q = Quaternion.LookRotation(look);
			q = Quaternion.AngleAxis(Random.Range(0,360), rb.velocity) * q;
			transform.rotation = q;

			mr.material.color = new Color(1,1,1,power);
			asrc.volume = Mathf.Lerp(0,0.4f,power);
			asrc.pitch = Mathf.Lerp(1.5f,7f,power);
		}
	}
}