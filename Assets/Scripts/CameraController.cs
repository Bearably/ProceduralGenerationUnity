using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	//Defines the minimum and maxiumum Field of View, sensitivity and Camera move speed values. It also defines the camera transform used and the 
    private float minFov = 5f;
    private float maxFov = 90f;
    public float sensitivity = 10f;
    public float moveSpeed = 10f;
    public Camera cam;

    void Update()
    {
		//Calls the Zoom and Move functions
        Zoom();
        Move();
    }

    void Zoom()
    {
		//Sets a new float field of view value to the camera transform's field of view.
        float fov = cam.fieldOfView;
		//Decreases or increases the field of view based on scroll wheel input.
        fov += -(Input.GetAxis("Mouse ScrollWheel")) * sensitivity;
		//Clamps the field of view to the minimum and maximum values set.
        fov = Mathf.Clamp(fov, minFov, maxFov);
		//Sets the camera transform's field of view to the new field of view value.
        cam.fieldOfView = fov;
    }

    void Move()
    {
		//Checks if the W key has been pressed
        if (Input.GetKey(KeyCode.W))
        {
			//Moves the camera transform's position up based on the movespeed value.
            cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y + (0.01f * moveSpeed), cam.transform.position.z);
        }
		//Checks if the S key has been pressed
        if (Input.GetKey(KeyCode.S))
        {
			//Moves the camera transform's position down based on the movespeed value.
            cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y - (0.01f * moveSpeed), cam.transform.position.z);
        }
		//Checks if the A key has been pressed
        if (Input.GetKey(KeyCode.A))
        {
			//Moves the camera transform's position left based on the movespeed value.
            cam.transform.position = new Vector3(cam.transform.position.x - (0.01f * moveSpeed), cam.transform.position.y, cam.transform.position.z);
        }
		//Checks if the D key has been pressed
        if (Input.GetKey(KeyCode.D))
        {
			//Moves the camera transform's position right based on the movespeed value.
            cam.transform.position = new Vector3(cam.transform.position.x + (0.01f * moveSpeed), cam.transform.position.y, cam.transform.position.z);
        }
    }
}
