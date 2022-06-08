using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float minFov = 5f;
    private float maxFov = 90f;
    private Vector3 camOrigin;
    public float sensitivity = 10f;
    public float moveSpeed = 1f;
    public Camera cam;
 
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
        if (Input.GetMouseButtonDown(0))
        {
            camOrigin = Input.mousePosition;
            return;
        }

        if (!Input.GetMouseButton(0)) return;

        Vector3 pos = cam.ScreenToViewportPoint(Input.mousePosition - camOrigin);
        Vector3 move = new Vector3(-(pos.x) * moveSpeed, -(pos.y) * moveSpeed, 0);

        transform.Translate(move, Space.World);
    }
}
