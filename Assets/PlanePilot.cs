using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlanePilot : MonoBehaviour {

	public GameObject GameOverPanel;
	public GameObject ATC2;

	public float speed = 280.0f;
	public float dragCoefficient = 1.1f;
	public float roll;
	public float rollRatio;
	public float flaps1SpeedDecrease = 0f;

	public float stallPenalty = 0f;

	private float terrainHeightWhereWeAre;

	public GameObject AU;
	public AudioSource VO;
	public AudioClip VOclip;

	private bool touchDown = false;
	private bool splashDownPlayed = false;
	private bool LowAltitudeWarningLoopPlaying = false;
	private bool ATC1 = false;
	private bool ATC2bool = false;
	public float timeElapsed;
	private Text AirspeedText;
	private Text AltitudeText;
	private Text WingAngleText;
	private Text PitchText;

	public Text OutcomeText;

	private GameObject gameOverPanel;

	// Use this for initialization
	void Start () 
	{
		Debug.Log ("Plane pilot script attached to: " + gameObject.name);
		timeElapsed = 0;
		AU = Instantiate(new GameObject());
		AU.AddComponent<AudioSource> ();
		AU.GetComponent<AudioSource> ().playOnAwake = true;
		PlayMayDay ();

		AirspeedText = GameObject.Find ("AirspeedText").GetComponent<Text>();
		AltitudeText = GameObject.Find ("AltitudeText").GetComponent<Text>();
		WingAngleText = GameObject.Find ("WingAngleText").GetComponent<Text> ();
		PitchText = GameObject.Find ("PitchText").GetComponent<Text> ();

		GameOverPanel.SetActive (false);
		//OutcomeText = GameObject.Find ("OutcomeText").GetComponent<Text>();
	}

	// Update is called once per frame
	void Update () 
	{
		
		timeElapsed += Time.deltaTime;

		if (timeElapsed > 50.3f && !ATC2bool) {
			PlayATC2 ();
			ATC2bool = true;
		}
		//Camera Controls
		//float bias = 0.5f;
		//Vector3 moveCamTo = transform.position - transform.forward * 500.0f + Vector3.up * 100.0f;
		//Camera.main.transform.position = Camera.main.transform.position * bias + moveCamTo * (1.0f - bias);//Spring function
		//Camera.main.transform.LookAt (transform.position + transform.forward * 30.0f);

		//Speed Calcs
		if (Input.GetKey(KeyCode.F)) 
		{
			flaps1SpeedDecrease += 1f * Time.deltaTime;
		}
		else
		{
			flaps1SpeedDecrease = 0f;
		}
		speed -= (((transform.forward.y + flaps1SpeedDecrease) * Time.deltaTime) * dragCoefficient);
		if (getPitchAngle() >= 0) 
		{
			speed -= dragCoefficient * Time.deltaTime;
		}
		speed -= getPitchAngle() * Time.deltaTime * Time.deltaTime;

		//Stall Condition
		if (speed < 200f || getPitchAngle() > 18f) 
		{
			stallPenalty += 10f * Time.deltaTime;
		}
		if (speed < 150f || getPitchAngle() > 24f) 
		{
			stallPenalty += 12f * Time.deltaTime;
		}
		if (speed < 130f || getPitchAngle() > 30f) 
		{
			stallPenalty += 15f * Time.deltaTime;
		}
		//Rotational Control
		transform.Rotate (Input.GetAxis ("Vertical"), Input.GetAxis("Yaw"), -Input.GetAxis ("Horizontal"));

		//Vertical descent
		transform.position += (transform.forward * Time.deltaTime * speed);
		transform.position = new Vector3 (transform.position.x,
			transform.position.y - (9.8f * Time.deltaTime) - (getRollAngle() * Time.deltaTime) - (getPitchAngle() * Time.deltaTime) - (stallPenalty*Time.deltaTime),
			transform.position.z);

		//Altitude Conditions
		terrainHeightWhereWeAre = Terrain.activeTerrain.SampleHeight (transform.position);

		/*
		if (transform.position.y < 1000f && !LowAltitudeWarningLoopPlaying) 
		{
			PlayLowAltitudeLoop ();
			LowAltitudeWarningLoopPlaying = true;
		}
		*/

		if (terrainHeightWhereWeAre > transform.position.y) 
		{
			
			touchDown = true;
			if (touchDown && !splashDownPlayed) 
			{
				SnapShotOfLanding ();
				splashDownPlayed = true;
				PlaySplashDown ();
			}
			speed -= 30f * Time.deltaTime;
			speed = Mathf.Clamp (speed, 0f, 1000f);
			transform.position = new Vector3 (transform.position.x, terrainHeightWhereWeAre, transform.position.z);
		}
		if (timeElapsed > AU.GetComponent<AudioSource>().clip.length && !ATC1) 
		{
			ATC1 = true;
			PlayATC1();
		}
		AssignHUDValues ();

	}
	void AssignHUDValues()
	{
		float pitchAngle = transform.rotation.eulerAngles.x;
		float wingAngle = transform.rotation.eulerAngles.z;

		//Calculate wing angle as pos
		if (transform.rotation.eulerAngles.z > 180f) 
		{
			wingAngle = Mathf.Abs (360 - transform.rotation.eulerAngles.z);
		}
		if (transform.rotation.eulerAngles.x > 180f) 
		{
			pitchAngle = transform.rotation.eulerAngles.x - 360f;
		}
		pitchAngle = pitchAngle * -1.0f;

		AirspeedText.text = "Airspeed: " + speed.ToString();
		WingAngleText.text = "Wing Angle: " + wingAngle.ToString();
		PitchText.text = "Pitch: " + pitchAngle.ToString();
		AltitudeText.text = "Altitude: " + (transform.position.y - terrainHeightWhereWeAre).ToString();
	}
	float getPitchAngle()
	{
		float pitchAngle = transform.rotation.eulerAngles.x;
		if (transform.rotation.eulerAngles.x > 180f) 
		{
			pitchAngle = transform.rotation.eulerAngles.x - 360f;
		}
		pitchAngle = pitchAngle * -1.0f;
		return pitchAngle;
	}
	float getRollAngle()
	{
		float wingAngle = transform.rotation.eulerAngles.z;

		//Calculate wing angle as pos
		if (transform.rotation.eulerAngles.z > 180f) 
		{
			wingAngle = Mathf.Abs (360 - transform.rotation.eulerAngles.z);
		}
		return wingAngle;
	}
	void SnapShotOfLanding()
	{

		float landingSpeed;
		float WingAngle;
		float PitchAngle; 

		landingSpeed = speed;
		WingAngle = getRollAngle();
		PitchAngle = getPitchAngle();

		Debug.Log (landingSpeed + " " + WingAngle + " " + PitchAngle);


		if (landingSpeed < 160f && landingSpeed > 100f && PitchAngle > 8f && PitchAngle < 12f && WingAngle < 2f) {
			OutcomeText.text = "Success! You landed the plane safely in the Hudson River.";
		}
		if(landingSpeed < 100f) 
		{
			OutcomeText.text = "You landed too slowly causing the plane to bounce on the surface of the water. The plane tumbles and disintegrates.";
		}
		if(landingSpeed > 160f) 
		{
			OutcomeText.text = "You landed too fast causing the plane to dig into the water, destroying the structural integrity of the body.";
		}
		if (PitchAngle < 8f) 
		{
			OutcomeText.text = OutcomeText.text + "\nThe shallow pitch angle caused the engines to dig into the water, ripping apart the plane.";
		}
		if (PitchAngle > 12f) 
		{
			OutcomeText.text = OutcomeText.text + "\nThe sharp pitch angle caused the rear of the plane to absorb the bulk of the impact, destroying the tail section.";
		}
		if (WingAngle > 2f)
		{
			OutcomeText.text = OutcomeText.text + "\nThe bank angle at landing cause one wing to clip the water's surface prematurely, effectively whipping the plane into oblivion.";
		}
		GameOverPanel.SetActive(true);
	}
	void PlayMayDay()
	{
		AU.GetComponent<AudioSource>().clip = (AudioClip)Resources.Load ("Audio/MayDay");
		AU.GetComponent<AudioSource>().PlayOneShot (AU.GetComponent<AudioSource>().clip);
	}
	void PlayATC1()
	{
		Debug.Log ("in atc1");
		AU.GetComponent<AudioSource>().clip = (AudioClip)Resources.Load ("Audio/ATC1");
		AU.GetComponent<AudioSource>().PlayOneShot (AU.GetComponent<AudioSource>().clip);
	}
	void PlaySplashDown()
	{
		AU.GetComponent<AudioSource>().clip = (AudioClip)Resources.Load ("Audio/SplashDown");
		AU.GetComponent<AudioSource>().PlayOneShot (AU.GetComponent<AudioSource>().clip);
	}
	void PlayLowAltitudeLoop()
	{
		AU.AddComponent<AudioSource>();
		AU.GetComponent<AudioSource>().clip = (AudioClip)Resources.Load ("Audio/LowAltitudeBeepLoop");
		AU.GetComponent<AudioSource>().playOnAwake = true;
		AU.GetComponent<AudioSource>().loop = true;
		AU.GetComponent<AudioSource>().Play();
	}
	void PlayATC2()
	{
		ATC2.GetComponent<AudioSource> ().Play ();
	}
}
