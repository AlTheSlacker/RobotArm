using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * take a look around!
 * 
*/

public class FreeCam : MonoBehaviour
{

    [SerializeField] private float speed = 1;
    [SerializeField] private float sensitivity = 1;

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
