using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCam : MonoBehaviour
{

    [SerializeField] private float speed = 10.0f;
    [SerializeField] private float sensitivity = 1.0f;

    void FixedUpdate()
    {
        Cursor.lockState = CursorLockMode.Locked;

        // rotation
        Vector3 rotCam = transform.rotation.eulerAngles;
        rotCam.x -= Input.GetAxis("Mouse Y") * sensitivity;
        rotCam.y += Input.GetAxis("Mouse X") * sensitivity;
        transform.rotation = Quaternion.Euler(rotCam);

        // translation
        Vector3 transCam = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        transCam = transCam * Time.deltaTime * speed;
        transform.Translate(transCam);
    }

}
