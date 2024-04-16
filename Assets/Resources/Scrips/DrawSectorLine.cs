using UnityEngine;
using UnityEngine.Animations.Rigging;

public class DrawSectorLine : MonoBehaviour
{
    public int numPoints = 20;
    public float radius = 5f;
    public float angle = 90f;

    private LineRenderer lineRdr;

    void Start()
    {
        lineRdr = gameObject.AddComponent<LineRenderer>();
        lineRdr.startWidth = 0.03f;
        lineRdr.positionCount = numPoints + 3; // �߰��� 2���� ���� ���
        lineRdr.useWorldSpace = false;
        DrawSector();
    }

    void DrawSector()
    {
        float angleStep = angle / numPoints;
        Vector3 origin = Vector3.zero;

        // ��ȣ ���������� �������� �� �׸���
        lineRdr.SetPosition(0, origin);
        lineRdr.SetPosition(1, new Vector3(Mathf.Cos(0) * radius, 0f, Mathf.Sin(0) * radius));

        for (int i = 0; i <= numPoints; i++)
        {
            float currentAngle = i * angleStep * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(currentAngle) * radius, 0f, Mathf.Sin(currentAngle) * radius);
            lineRdr.SetPosition(i + 2, pos);
        }

        // ��ȣ �������� �������� �� �׸���
        lineRdr.SetPosition(numPoints + 2, origin);
    }

    void Update()
    {
        DrawSector();
    }
}