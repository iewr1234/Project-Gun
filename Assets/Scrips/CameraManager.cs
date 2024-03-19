using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("---Access Component---")]
    public Camera mainCam;

    [Header("--- Assignment Variable---")]
    [SerializeField] private float moveSpeed;

    public void SetComponents()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        MoveCamera();
    }

    private void MoveCamera()
    {
        var pos = mainCam.transform.position;
        if (Input.GetKey(KeyCode.W))
        {
            pos.z += moveSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            pos.z -= moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            pos.x -= moveSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            pos.x += moveSpeed * Time.deltaTime;
        }
        mainCam.transform.position = pos;
    }
}
