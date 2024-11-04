using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;

public enum CameraState
{
    None = -1,
    FrontAim,
    RightAim,
    LeftAim,
    Reload,
}

public class CameraManager : MonoBehaviour
{
    [Header("---Access Script---")]
    private GameManager gameMgr;

    [Header("---Access Component---")]
    public Camera mainCam;
    public Camera subCam;
    public Transform pivotPoint;
    private CinemachineBrain cambrain;

    private List<CinemachineVirtualCamera> virCams;

    [Header("--- Assignment Variable---")]
    public CameraState state;
    public bool lockCam;

    [Space(5f)]
    [SerializeField] private CinemachineVirtualCamera currrentActionCam;
    private CinemachineTransposer reloadTransposer;
    private CinemachineComposer reloadComposer;

    private readonly float moveSpeed = 15f;
    private readonly Vector3 defaultPos = new Vector3(12.5f, 15f, -12.5f);

    private float currentRot;
    private readonly float rotSpeed = 150f;

    private Vector3 camDirection;
    private float camDistance = 25f;
    private readonly float zoomSpeed = 20f;
    private readonly float zoomMin = 10f;
    private readonly float zoomMax = 45f;

    private readonly Vector3 reloadCamBody_Idle = new Vector3(2.5f, 2f, 2.5f);
    private readonly Vector3 reloadCamBody_half = new Vector3(3.5f, 1.25f, -0.25f);
    private readonly Vector3 reloadCamBody_fullRight = new Vector3(3.5f, 2f, -2f);
    private readonly Vector3 reloadCamBody_fullLeft = new Vector3(-3.5f, 2f, -2f);
    private readonly Vector3 reloadCamAim_noneHalf = new Vector3(0f, 1f, 0f);
    private readonly Vector3 reloadCamAim_half = new Vector3(0f, 0.5f, 0f);

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
        reloadTransposer = virCams[(int)CameraState.Reload].GetCinemachineComponent<CinemachineTransposer>();
        reloadComposer = virCams[(int)CameraState.Reload].GetCinemachineComponent<CinemachineComposer>();

        lockCam = false;
    }

    private void Update()
    {
        if (gameMgr != null)
        {
            var canOperation = !lockCam && gameMgr.nodeList.Count > 0;
            if (!canOperation) return;

            //var canMoveCam = gameMgr.gameState == GameState.None
            //              || gameMgr.gameState == GameState.Move
            //              || gameMgr.gameState == GameState.Watch
            //              || gameMgr.gameState == GameState.Base;
            //if (!canMoveCam) return;
        }

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
            if (Mathf.Abs(rotDiff) < rotStep) rotStep = Mathf.Abs(rotDiff);
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
        Debug.Log($"Set Camera: {_state}");
        switch (_state)
        {
            case CameraState.None:
                if (currrentActionCam != null) currrentActionCam.enabled = false;
                currrentActionCam = null;
                mainCam.transform.localPosition = defaultPos;
                mainCam.transform.LookAt(pivotPoint);
                lockCam = false;
                break;
            default:
                break;
        }
        state = _state;
    }

    public void SetCameraState(CameraState _state, CharacterController charCtr)
    {
        if (_state == CameraState.None)
        {
            Debug.LogError($"invalid call: SetCameraState(CameraState.None), {charCtr}");
            return;
        }
        Debug.Log($"Set Camera: {_state}");

        if (currrentActionCam != null) currrentActionCam.enabled = false;
        currrentActionCam = virCams[(int)_state];
        state = _state;
        switch (state)
        {
            case CameraState.None:
                break;
            case CameraState.Reload:
                SetVirtualCamera(charCtr.transform, charCtr.transform);
                if (charCtr.animator.GetBool("isCover"))
                {
                    if (charCtr.animator.GetBool("fullCover"))
                    {
                        reloadTransposer.m_FollowOffset = charCtr.animator.GetBool("isRight") ? reloadCamBody_fullRight : reloadCamBody_fullLeft;
                        reloadComposer.m_TrackedObjectOffset = reloadCamAim_noneHalf;
                    }
                    else
                    {
                        reloadTransposer.m_FollowOffset = reloadCamBody_half;
                        reloadComposer.m_TrackedObjectOffset = reloadCamAim_half;
                    }
                }
                else
                {
                    reloadTransposer.m_FollowOffset = reloadCamBody_Idle;
                    reloadComposer.m_TrackedObjectOffset = reloadCamAim_noneHalf;
                }
                break;
            default:
                SetVirtualCamera(charCtr.transform, charCtr.transform);
                break;
        }
    }

    public void SetCameraState(CameraState _state, CharacterController charCtr_follow, CharacterController charCtr_lookAt)
    {
        if (_state == CameraState.None)
        {
            Debug.LogError($"invalid call: SetCameraState(CameraState.None), {charCtr_follow}, {charCtr_lookAt}");
            return;
        }
        Debug.Log($"Set Camera: {_state}");

        if (currrentActionCam != null) currrentActionCam.enabled = false;
        currrentActionCam = virCams[(int)_state];
        state = _state;
        switch (state)
        {
            case CameraState.None:
                break;
            default:
                SetVirtualCamera(charCtr_follow.transform, charCtr_lookAt.transform);
                break;
        }
    }

    private void SetVirtualCamera(Transform follow, Transform lookAt)
    {
        currrentActionCam.enabled = true;
        currrentActionCam.Follow = follow;
        currrentActionCam.LookAt = lookAt;
        lockCam = true;
    }
}
