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
    private Vector3 posCam;

    private void Start()
    {
        posCam = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }

    void FixedUpdate()
    {
        Cursor.lockState = CursorLockMode.Locked;

        // rotation
        Vector3 rotCam = transform.rotation.eulerAngles;
        rotCam.x -= Input.GetAxis("Mouse Y") * sensitivity;
        rotCam.y += Input.GetAxis("Mouse X") * sensitivity;
        transform.rotation = Quaternion.Euler(rotCam);

        // translation
        posCam = posCam + new Vector3(Input.GetAxis("XX"), Input.GetAxis("YY"), Input.GetAxis("ZZ")) * Time.deltaTime * speed;
        transform.position  = posCam;
    }

}
