using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawRange : MonoBehaviour
{
    [Header("---Access Component---")]
    [SerializeField] private LineRenderer lineRdr;

    [Header("--- Assignment Variable---")]
    public float angle;
    public float radius;

    private readonly int numPoints = 20;

    public void SetComponents()
    {
        lineRdr = GetComponentInChildren<LineRenderer>();
        lineRdr.material = new Material(lineRdr.material);
        lineRdr.startWidth = 0.02f;
        lineRdr.positionCount = numPoints + 3; // 추가된 2개의 점을 고려
        lineRdr.useWorldSpace = false;
    }

    public void SetRange(CharacterController charCtr, FieldNode targetNode)
    {
        charCtr.SetWatchInfo(targetNode, this);
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        transform.position = charCtr.watchInfo.watchNode.transform.position;
        angle = charCtr.WatchAngle;
        switch (charCtr.ownerType)
        {
            case CharacterOwner.Player:
                var range = DataUtility.GetDistance(transform.position, targetNode.transform.position);
                if (range > charCtr.Range)
                {
                    range = charCtr.Range;
                }
                radius = range;
                break;
            case CharacterOwner.Enemy:
                radius = charCtr.Range;
                break;
            default:
                break;
        }
        DrawFan();
    }

    private void DrawFan()
    {
        lineRdr.transform.localRotation = Quaternion.Euler(0f, -angle * 0.5f, 0f);

        var angleStep = angle / numPoints;
        Vector3 origin = Vector3.zero;

        // 원호 시작점에서 원점까지 선 그리기
        lineRdr.SetPosition(0, origin);
        lineRdr.SetPosition(1, new Vector3(Mathf.Sin(0) * radius, 0f, Mathf.Cos(0) * radius));

        for (int i = 0; i <= numPoints; i++)
        {
            var currentAngle = i * angleStep * Mathf.Deg2Rad;
            var pos = new Vector3(Mathf.Sin(currentAngle) * radius, 0f, Mathf.Cos(currentAngle) * radius);
            lineRdr.SetPosition(i + 2, pos);
        }

        // 원호 끝점에서 원점까지 선 그리기
        lineRdr.SetPosition(numPoints + 2, origin);
    }
}
