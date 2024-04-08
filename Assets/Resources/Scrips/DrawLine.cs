using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public float density = 10f; // 점선 밀도 조절

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void SetLine()
    {
        // 카메라의 뷰 프러스텀 내에 있는 모든 렌더러 가져오기
        Renderer[] renderers = FindObjectsOfType<Renderer>();

        // 선의 시작점과 끝점 가져오기
        Vector3 startPos = lineRenderer.GetPosition(0);
        Vector3 endPos = lineRenderer.GetPosition(lineRenderer.positionCount - 1);

        // 선 렌더링을 위한 위치 배열 초기화
        lineRenderer.positionCount = 0;

        bool isOccluded = false;
        Vector3 prevPos = startPos;

        for (float ratio = 0f; ratio <= 1f; ratio += 1f / density)
        {
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, ratio);

            // 현재 위치와 이전 위치 사이에 오브젝트가 있는지 검사
            isOccluded = CheckOcclusion(prevPos, currentPos, renderers);

            if (isOccluded)
            {
                // 가려진 부분은 점선으로 렌더링
                lineRenderer.positionCount = lineRenderer.positionCount + 2;
                lineRenderer.SetPosition(lineRenderer.positionCount - 2, prevPos);
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, currentPos);
            }
            else
            {
                // 가려지지 않은 부분은 실선으로 렌더링
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, currentPos);
            }

            prevPos = currentPos;
        }
    }

    private bool CheckOcclusion(Vector3 start, Vector3 end, Renderer[] renderers)
    {
        Ray ray = new Ray(start, end - start);
        RaycastHit[] hits = new RaycastHit[10]; // RaycastHit 배열 선언, 크기는 상황에 따라 조정
        int hitCount = 0;

        // 레이캐스트를 사용하여 오브젝트 간섭 검사
        hitCount = Physics.RaycastNonAlloc(ray, hits, Vector3.Distance(start, end), ~0, QueryTriggerInteraction.Ignore);

        if (hitCount > 0)
        {
            return true;
        }

        return false;
    }
}
