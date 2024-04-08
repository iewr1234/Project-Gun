using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public float density = 10f; // ���� �е� ����

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void SetLine()
    {
        // ī�޶��� �� �������� ���� �ִ� ��� ������ ��������
        Renderer[] renderers = FindObjectsOfType<Renderer>();

        // ���� �������� ���� ��������
        Vector3 startPos = lineRenderer.GetPosition(0);
        Vector3 endPos = lineRenderer.GetPosition(lineRenderer.positionCount - 1);

        // �� �������� ���� ��ġ �迭 �ʱ�ȭ
        lineRenderer.positionCount = 0;

        bool isOccluded = false;
        Vector3 prevPos = startPos;

        for (float ratio = 0f; ratio <= 1f; ratio += 1f / density)
        {
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, ratio);

            // ���� ��ġ�� ���� ��ġ ���̿� ������Ʈ�� �ִ��� �˻�
            isOccluded = CheckOcclusion(prevPos, currentPos, renderers);

            if (isOccluded)
            {
                // ������ �κ��� �������� ������
                lineRenderer.positionCount = lineRenderer.positionCount + 2;
                lineRenderer.SetPosition(lineRenderer.positionCount - 2, prevPos);
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, currentPos);
            }
            else
            {
                // �������� ���� �κ��� �Ǽ����� ������
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, currentPos);
            }

            prevPos = currentPos;
        }
    }

    private bool CheckOcclusion(Vector3 start, Vector3 end, Renderer[] renderers)
    {
        Ray ray = new Ray(start, end - start);
        RaycastHit[] hits = new RaycastHit[10]; // RaycastHit �迭 ����, ũ��� ��Ȳ�� ���� ����
        int hitCount = 0;

        // ����ĳ��Ʈ�� ����Ͽ� ������Ʈ ���� �˻�
        hitCount = Physics.RaycastNonAlloc(ray, hits, Vector3.Distance(start, end), ~0, QueryTriggerInteraction.Ignore);

        if (hitCount > 0)
        {
            return true;
        }

        return false;
    }
}
