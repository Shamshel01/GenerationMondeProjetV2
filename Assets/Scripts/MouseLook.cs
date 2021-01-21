using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{

    public float sensitivity = 100.0f;
    public Transform body;
    public float xRotation = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X")*sensitivity*Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y")*sensitivity*Time.deltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -75.0f, 75.0f);

        transform.localRotation = Quaternion.Euler(xRotation, 0.0f, 0.0f);
        body.Rotate(Vector3.up*mouseX);
    }
}
