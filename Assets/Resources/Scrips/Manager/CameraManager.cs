using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;
using UnityEngine.Windows.WebCam;

public enum CameraState
{
    None = -1,
    FrontAim,
    RightAim,
    LeftAim,
}

public class CameraManager : MonoBehaviour
{
    [Header("---Access Script---")]
    private GameManager gameMgr;

    [Header("---Access Component---")]
    public Camera mainCam;
    public Camera subCam;
    private Transform pivotPoint;
    private CinemachineBrain cambrain;

    private List<CinemachineVirtualCamera> virCams;

    [Header("--- Assignment Variable---")]
    public CameraState state;

    private CinemachineVirtualCamera currrentActionCam;
    [SerializeField] private bool actionCam;

    private readonly float moveSpeed = 15f;
    private readonly Vector3 defaultPos = new Vector3(12.5f, 15f, -12.5f);

    private float currentRot;
    private readonly float rotSpeed = 150f;

    private Vector3 camDirection;
    private float camDistance = 25f;
    private readonly float zoomSpeed = 20f;
    private readonly float zoomMin = 10f;
    private readonly float zoomMax = 45f;

    public void SetComponents(GameManager _gameMgr)
    {
        gameMgr = _gameMgr;

        pivotPoint = transform.Find("PivotPoint");
        mainCam = Camera.main;
        subCam = mainCam.transform.Find("SubCamera").GetComponent<Camera>();
        camDirection = Vector3.Normalize(defaultPos - pivotPoint.position);
        mainCam.transform.localPosition = camDirection * camDistance;
        mainCam.transform.LookAt(pivotPoint);

        cambrain = mainCam.GetComponent<CinemachineBrain>();
        virCams = GetComponentsInChildren<CinemachineVirtualCamera>().ToList();

        actionCam = false;
    }

    private void Update()
    {
        var canOperation = !actionCam && gameMgr.fieldNodes.Count > 0;
        if (!canOperation) return;

        var canMoveCam = gameMgr.gameState == GameState.None
                      || gameMgr.gameState == GameState.Move
                      || gameMgr.gameState == GameState.Watch;
        if (!canMoveCam) return;

        CameraMove();
        CameraRotate();
        CameraZoom();
    }

    private void CameraMove()
    {
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
        var rotDiff = Mathf.DeltaAngle(pivotPoint.transform.eulerAngles.y, currentRot);
        if (rotDiff == 0f)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                currentRot += 45f;
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                currentRot -= 45f;
            }
        }
        else
        {
            var rotDir = Mathf.Sign(rotDiff);
            var rotStep = rotSpeed * Time.deltaTime;
            if (Mathf.Abs(rotDiff) < rotStep)
            {
                rotStep = Mathf.Abs(rotDiff);
            }
            pivotPoint.transform.Rotate(Vector3.up * rotDir * rotStep, Space.Self);
            mainCam.transform.LookAt(pivotPoint);
        }
    }

    private void CameraZoom()
    {
        var scroll = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        if (scroll != 0f)
        {
            camDistance -= scroll;
            if (camDistance < zoomMin)
            {
                camDistance = zoomMin;
            }
            else if (camDistance > zoomMax)
            {
                camDistance = zoomMax;
            }
        }

        mainCam.transform.localPosition = camDirection * camDistance;
    }

    public void SetCameraState(CameraState _state)
    {
        switch (_state)
        {
            case CameraState.None:
                currrentActionCam.enabled = false;
                currrentActionCam = null;
                mainCam.transform.localPosition = defaultPos;
                mainCam.transform.LookAt(pivotPoint);
                actionCam = false;
                break;
            default:
                break;
        }
        state = _state;
    }

    public void SetCameraState(CameraState _state, Transform follow, Transform lookAt)
    {
        if (_state == CameraState.None)
        {
            Debug.LogError($"invalid call: SetCameraState(CameraState.None), {follow}, {lookAt}");
            return;
        }

        if (currrentActionCam != null)
        {
            currrentActionCam.enabled = false;
        }
        currrentActionCam = virCams[(int)_state];
        switch (_state)
        {
            case CameraState.FrontAim:
                SetVirtualCamera(follow, lookAt);
                break;
            case CameraState.RightAim:
                SetVirtualCamera(follow, lookAt);
                break;
            case CameraState.LeftAim:
                SetVirtualCamera(follow, lookAt);
                break;
            default:
                break;
        }
        state = _state;
    }

    private void SetVirtualCamera(Transform follow, Transform lookAt)
    {
        currrentActionCam.enabled = true;
        currrentActionCam.Follow = follow;
        currrentActionCam.LookAt = lookAt;
        actionCam = true;
    }
}
