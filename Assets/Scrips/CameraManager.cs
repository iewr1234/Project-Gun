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
        Vector3 dir; 
        var pos = mainCam.transform.position;
        if (Input.GetKey(KeyCode.W))
        {
            dir = mainCam.transform.forward;
            dir.y = mainCam.transform.position.y;
            pos += dir * (moveSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            dir = mainCam.transform.forward;
            dir.y = mainCam.transform.position.y;
            pos -= dir * (moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            pos -= mainCam.transform.right * (moveSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            pos += mainCam.transform.right * (moveSpeed * Time.deltaTime);
        }
        pos.y = mainCam.transform.position.y;
        mainCam.transform.position = pos;
    }
}
