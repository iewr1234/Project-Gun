using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestCode : MonoBehaviour
{
    //public Transform startPoint;
    //public Transform endPoint;
    //public LineRenderer line;
    //[Space(5f)]

    //public Vector3 vel;

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        PredictTrajectory(startPoint.position, vel);
    //    }
    //}

    //void PredictTrajectory(Vector3 startPos, Vector3 vel)
    //{
    //    var _points = new List<Vector3>();
    //    float deltaTime = Time.fixedDeltaTime;
    //    Vector3 gravity = Physics.gravity;
    //    Vector3 position = startPos;
    //    Vector3 velocity = vel;
    //    while (position.y >= 0f)
    //    {
    //        position += velocity * deltaTime + 0.5f * gravity * deltaTime * deltaTime;
    //        velocity += gravity * deltaTime;
    //        _points.Add(position);
    //    }

    //    line.positionCount = _points.Count;
    //    line.SetPositions(_points.ToArray());
    //    line.gameObject.SetActive(true);
    //}

    public GameObject ball;
    public LineRenderer line;
    [Space(5f)]

    public Transform m_Target;
    public float m_Speed = 20;
    public float m_HeightArc = 3;
    private Vector3 m_StartPosition;

    private List<Vector3> points = new List<Vector3>();
    private bool m_IsStart;
    private Vector3 nextPoint;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // 출발
        {
            ResultParabola();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            line.gameObject.SetActive(false);
            m_IsStart = true;
            nextPoint = points[0];
        }

        if (m_IsStart)
        {
            if (points.Count == 0)
            {
                m_IsStart = false;
                return;
            }
            ball.transform.position = Vector3.MoveTowards(ball.transform.position, nextPoint, m_Speed * Time.deltaTime);

            if (DataUtility.GetDistance(ball.transform.position, nextPoint) < 0.01f)
            {
                points.RemoveAt(0);
                if (points.Count > 0)
                {
                    nextPoint = points[0];
                }
            }
        }
    }

    private void ResultParabola()
    {
        m_StartPosition = ball.transform.position;
        points.Clear();
        points.Add(m_StartPosition);

        var curPoint = m_StartPosition;
        var targetPosition = m_Target.position;

        while (true)
        {
            var x0 = m_StartPosition.x;
            var x1 = targetPosition.x;
            var z0 = m_StartPosition.z;
            var z1 = targetPosition.z;

            var distance = x1 - x0;
            var nextX = Mathf.MoveTowards(curPoint.x, x1, m_Speed * Time.deltaTime);

            // 보간을 통해 y좌표와 z좌표를 결정합니다.
            var baseY = Mathf.Lerp(m_StartPosition.y, targetPosition.y, (nextX - x0) / distance);
            var arc = m_HeightArc * (nextX - x0) * (nextX - x1) / (-0.25f * distance * distance);

            // z좌표를 보간하여 업데이트합니다.
            var nextZ = Mathf.Lerp(z0, z1, (nextX - x0) / distance);

            var nextPosition = new Vector3(nextX, baseY + arc, nextZ);
            points.Add(nextPosition);
            curPoint = nextPosition;

            // 현재 위치가 목표 위치에 도달했는지 확인합니다.
            if (DataUtility.GetDistance(nextPosition, targetPosition) < 0.01f)
            {
                //Debug.Log("도착");
                line.positionCount = points.Count;
                line.SetPositions(points.ToArray());
                line.gameObject.SetActive(true);
                break;
            }
        }
    }

    //Quaternion LookAt2D(Vector2 forward)
    //{
    //    return Quaternion.Euler(0, 0, Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg);
    //}

    //private void ResultParabola()
    //{
    //    m_StartPosition = ball.transform.position;
    //    points.Clear();
    //    points.Add(m_StartPosition);

    //    curPoint = m_StartPosition;
    //    while (true)
    //    {
    //        float x0 = m_StartPosition.x;
    //        float x1 = m_Target.position.x;
    //        float distance = x1 - x0;
    //        float nextX = Mathf.MoveTowards(curPoint.x, x1, m_Speed * Time.deltaTime);
    //        float baseY = Mathf.Lerp(m_StartPosition.y, m_Target.position.y, (nextX - x0) / distance);
    //        float arc = m_HeightArc * (nextX - x0) * (nextX - x1) / (-0.25f * distance * distance);
    //        Vector3 nextPosition = new Vector3(nextX, baseY + arc, curPoint.z);
    //        points.Add(nextPosition);
    //        curPoint = nextPosition;

    //        if (nextPosition == m_Target.position)
    //        {
    //            Arrived();
    //            break;
    //        }
    //    }
    //}
}
