﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlanePilot : MonoBehaviour {

	public float speed = 280.0f;
	public float dragCoefficient = 1.3f;
	public float roll;
	public float rollRatio;
	public float flaps1SpeedDecrease = 0f;

	private float terrainHeightWhereWeAre;

	public GameObject AU;
	public AudioSource VO;
	public AudioClip VOclip;

	private bool touchDown = false;
	private bool splashDownPlayed = false;
	private bool LowAltitudeWarningLoopPlaying = false;
	private bool ATC1 = false;

	public float timeElapsed;

	public float WingAngle;
	public float NoseAngle;
	public float landingSpeed;

	private Text AirspeedText;
	private Text AltitudeText;
	private Text WingAngleText;
	private Text PitchText;

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
	}

	// Update is called once per frame
	void Update () 
	{

		timeElapsed += Time.deltaTime;
		//Camera Controls
		//float bias = 0.5f;
		//Vector3 moveCamTo = transform.position - transform.forward * 500.0f + Vector3.up * 100.0f;
		//Camera.main.transform.position = Camera.main.transform.position * bias + moveCamTo * (1.0f - bias);//Spring function
		//Camera.main.transform.LookAt (transform.position + transform.forward * 30.0f);

		//Speed Calcs
		if (Input.GetKey(KeyCode.F)) 
		{
			flaps1SpeedDecrease += 3f * Time.deltaTime;
		}
		else
		{
			flaps1SpeedDecrease = 0f;
		}
		speed -= ((transform.forward.y + flaps1SpeedDecrease) * Time.deltaTime)*dragCoefficient;

		//Rotational Control
		transform.Rotate (Input.GetAxis ("Vertical"), Input.GetAxis("Yaw"), -Input.GetAxis ("Horizontal"));

		//Vertical descent
		transform.position += (transform.forward * Time.deltaTime * speed);
		transform.position = new Vector3 (transform.position.x,
			transform.position.y - (9.8f / 2f * Time.deltaTime),
			transform.position.z);

		//Altitude Conditions
		terrainHeightWhereWeAre = Terrain.activeTerrain.SampleHeight (transform.position);

		if (transform.position.y < 1500f && !LowAltitudeWarningLoopPlaying) 
		{
			PlayLowAltitudeLoop ();
			LowAltitudeWarningLoopPlaying = true;
		}

		if (terrainHeightWhereWeAre > transform.position.y) 
		{
			
			touchDown = true;
			if (touchDown && !splashDownPlayed) 
			{
				SnapShotOfLanding ();
				splashDownPlayed = true;
				PlaySplashDown ();
			}
			speed -= 80f * Time.deltaTime;
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
			Debug.Log ("Pitch adjust");
			pitchAngle = transform.rotation.eulerAngles.x - 360f;
		}
		pitchAngle = pitchAngle * -1.0f;

		AirspeedText.text = "Airspeed: " + speed.ToString();
		WingAngleText.text = "Wing Angle: " + wingAngle.ToString();
		PitchText.text = "Pitch: " + pitchAngle.ToString();
		AltitudeText.text = "Altitude: " + (transform.position.y - terrainHeightWhereWeAre).ToString();
	}
	void SnapShotOfLanding()
	{
		landingSpeed = speed;
		NoseAngle = transform.rotation.x;
		WingAngle = transform.rotation.z;

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
}
