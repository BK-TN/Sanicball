using UnityEngine;

public class SpeedEffect : MonoBehaviour
{
    //float timer = 0.05f;
    //const float timerMax = 1.0f;

    //Vector3 targetScale;
    //Quaternion targetRotation;

    // Use this for initialization
    private void Start()
    {
        //targetScale = transform.localScale;
        //targetRotation = transform.localRotation;
    }

    // Update is called once per frame
    private void Update()
    {
        transform.Rotate(new Vector3(0, 0, 1000f * Time.deltaTime));

        /*timer -= Time.deltaTime;
		if (timer <= 0) {
			timer += timerMax;

			float scale = Random.Range (1.0f,1.1f);
			float rotation = Random.Range (0f,360f);
			targetScale = new Vector3(scale,scale,scale);
			targetRotation = Quaternion.Euler(0,0,rotation);
		}

		transform.localScale = Vector3.Lerp(transform.localScale,targetScale,0.01f);
		transform.localRotation = Quaternion.Lerp(transform.localRotation,targetRotation,0.01f);*/
    }
}
