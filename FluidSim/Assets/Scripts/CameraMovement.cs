using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public GameObject target;
    public float rotationSpeed;
    public float zoomSpeed;

    public void Update()
    {

        if (Input.GetMouseButton(0))
        {
            var mouseX = Input.GetAxis("Mouse X");
            var mouseY = Input.GetAxis("Mouse Y");
            transform.RotateAround(target.transform.position, Vector3.up, mouseX * rotationSpeed);
            transform.RotateAround(target.transform.position, transform.right, -mouseY * rotationSpeed);
            transform.LookAt(target.transform.position);
        }
        transform.position += -transform.forward * Input.mouseScrollDelta.y * zoomSpeed;
        //transform.position = new Vector3(transform.position.x + Input.mouseScrollDelta.y * zoomSpeed, transform.position.y, transform.position.z);
    }
}