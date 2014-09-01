using UnityEngine;
using System.Collections;

public class RotateMe : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(null != gameObject)
		{
			gameObject.transform.Rotate(Vector3.forward, 30.0f * Time.deltaTime) ;
			gameObject.transform.Rotate(Vector3.up, -30.0f * Time.deltaTime) ;
			gameObject.transform.Rotate(Vector3.left, 30.0f * Time.deltaTime) ;
		}
	}
}
