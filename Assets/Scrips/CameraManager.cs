using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("---Access Component---")]
    public Transform pivotPoint;
    public Camera mainCam;

    [Header("--- Assignment Variable---")]
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private float rotSpeed = 150f;

    private float currentRot;

    public void SetComponents()
    {
        pivotPoint = transform.Find("PivotPoint");
        mainCam = Camera.main;

        currentRot = 0f;
    }

    private void Update()
    {
        CameraMove();
        CameraRotate();
    }

    private void CameraMove()
    {
        //Vector3 dir;
        //var pos = pivotPoint.transform.position;
        //if (Input.GetKey(KeyCode.W))
        //{
        //    dir = mainCam.transform.forward;
        //    dir.y = mainCam.transform.position.y;
        //    pos += dir * (moveSpeed * Time.deltaTime);
        //}
        //else if (Input.GetKey(KeyCode.S))
        //{
        //    dir = mainCam.transform.forward;
        //    dir.y = mainCam.transform.position.y;
        //    pos -= dir * (moveSpeed * Time.deltaTime);
        //}
        //if (Input.GetKey(KeyCode.A))
        //{
        //    pos -= mainCam.transform.right * (moveSpeed * Time.deltaTime);
        //}
        //else if (Input.GetKey(KeyCode.D))
        //{
        //    pos += mainCam.transform.right * (moveSpeed * Time.deltaTime);
        //}
        //pos.y = pivotPoint.transform.position.y;
        //pivotPoint.transform.position = pos;

        var dir = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            dir += mainCam.transform.forward;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            dir -= mainCam.transform.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            dir -= mainCam.transform.right;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            dir += mainCam.transform.right;
        }
        dir.y = 0f;
        var newPos = pivotPoint.transform.position + dir * (moveSpeed * Time.deltaTime);
        newPos.y = pivotPoint.transform.position.y;
        pivotPoint.transform.position = newPos;
    }

    private void CameraRotate()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            currentRot += 45f;
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            currentRot -= 45f;
        }

        var rotDiff = Mathf.DeltaAngle(pivotPoint.transform.eulerAngles.y, currentRot);
        var rotDir = Mathf.Sign(rotDiff);

        var rotStep = rotSpeed * Time.deltaTime;
        if (Mathf.Abs(rotDiff) < rotStep)
        {
            rotStep = Mathf.Abs(rotDiff);
        }

        pivotPoint.transform.Rotate(Vector3.up * rotDir * rotStep, Space.Self);
    }
}
