﻿using UnityEngine;
using System.Collections;

public class Doors : MonoBehaviour
{

	public float yValue;

	// Use this for initialization
	void Start () 
	{
	
	}

	public void OpenDoor()
	{
		//iTween.MoveBy (gameObject, iTween.Hash ("y", yValue, "oncomplete", "OnDoorOpenFunction", "delay", 0.5));
		// TODO: play particle effect
		Destroy (this.gameObject);
	}


	void OnDoorOpenFunction()
	{
		Debug.Log ("calling this fn");
		Destroy (this.gameObject);
	}
	// Update is called once per frame
	 
}
