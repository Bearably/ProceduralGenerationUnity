using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float minFov = 5f;
    private float maxFov = 90f;
    public float sensitivity = 10f;
    public float moveSpeed = 10f;
    public Camera cam;
    private Vector3 dragOrigin;

    void Update()
    {
        Zoom();
        Move();
    }

    void Zoom()
    {
        float fov = cam.fieldOfView;
        fov += -(Input.GetAxis("Mouse ScrollWheel")) * sensitivity;
        fov = Mathf.Clamp(fov, minFov, maxFov);
        cam.fieldOfView = fov;
    }

    void Move()
    {
        if (Input.GetKey(KeyCode.W))
        {
            cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y + (0.01f * moveSpeed), cam.transform.position.z);
        }
        if (Input.GetKey(KeyCode.S))
        {
            cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y - (0.01f * moveSpeed), cam.transform.position.z);
        }
        if (Input.GetKey(KeyCode.A))
        {
            cam.transform.position = new Vector3(cam.transform.position.x - (0.01f * moveSpeed), cam.transform.position.y, cam.transform.position.z);
        }
        if (Input.GetKey(KeyCode.D))
        {
            cam.transform.position = new Vector3(cam.transform.position.x + (0.01f * moveSpeed), cam.transform.position.y, cam.transform.position.z);
        }
    }
}
