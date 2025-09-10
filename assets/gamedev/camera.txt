using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform player; // Assign Player in Inspector
    public float distance = 5f; // Default camera distance
    public float minDistance = 2f; // Min zoom (for walls)
    public float maxDistance = 6f; // Max zoom
    public float height = 2f; // Camera height
    public float sensitivity = 1.2f; // Mouse sensitivity
    public float zoomSpeed = 2f; // Scroll zoom speed
    public LayerMask obstacleMask; // Walls & objects to detect
    public float positionSmoothTime = 0.1f;

    public float currentRotationX = 0f;
    private float currentRotationY = 0f;
    private Vector3 positionVelocity;

    void Start()
    {
        // Lock cursor for better camera control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        obstacleMask = LayerMask.GetMask("Obstacles");
    }

    void LateUpdate()
    {
        if (!player) return;

        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        // Rotate camera
        currentRotationX += mouseX;
        currentRotationY -= mouseY;
        currentRotationY = Mathf.Clamp(currentRotationY, -30f, 60f); // Limit vertical rotation

        Quaternion rotation = Quaternion.Euler(currentRotationY, currentRotationX, 0);

        // Camera collision check
        float desiredDistance = distance;
        RaycastHit hit;
        if (Physics.Raycast(player.position, rotation * -Vector3.forward, out hit, maxDistance, obstacleMask))
        {
            desiredDistance = Mathf.Clamp(hit.distance, minDistance, maxDistance);
        }

        // Update camera position
        Vector3 targetPosition = player.position - (rotation * Vector3.forward * desiredDistance) + Vector3.up * height;
        transform.position = Vector3.SmoothDamp(transform.position,targetPosition,ref positionVelocity,positionSmoothTime);
        transform.rotation = rotation;

        // Scroll to zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }
}
