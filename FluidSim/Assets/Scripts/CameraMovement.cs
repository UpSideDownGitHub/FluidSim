using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using UnityEngine;

/// <summary>
/// Controls the movement of the camera based on user input.
/// </summary>
public class CameraMovement : MonoBehaviour
{
    // The target object around which the camera rotates.
    public GameObject target;
    // The speed of rotation when the user moves the mouse.
    public float rotationSpeed;
    // The speed of zooming when the user scrolls the mouse wheel.
    public float zoomSpeed;

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    public void Update()
    {
        // Check if the left mouse button is pressed
        if (Input.GetMouseButton(0))
        {
            // Get the amount of mouse movement in X and Y direction
            var mouseX = Input.GetAxis("Mouse X");
            var mouseY = Input.GetAxis("Mouse Y");

            // Rotate the camera around the target based on mouse movement
            transform.RotateAround(target.transform.position, Vector3.up, mouseX * rotationSpeed);
            transform.RotateAround(target.transform.position, transform.right, -mouseY * rotationSpeed);

            // Make the camera look at the target
            transform.LookAt(target.transform.position);
        }

        // Zoom in/out based on mouse scroll wheel movement
        transform.position += -transform.forward * Input.mouseScrollDelta.y * zoomSpeed;
    }
}