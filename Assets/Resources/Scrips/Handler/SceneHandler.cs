using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    private Image fade;

    public readonly float fadeDuration = 1f;
    public readonly float minimumLoadingTime = 1.5f;

    private void Awake()
    {
        var find = FindObjectsOfType<DataManager>();
        if (find.Length == 1)
        {
            DontDestroyOnLoad(gameObject);
            SetComponents();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetComponents()
    {
        fade = transform.Find("Fade").GetComponent<Image>();
    }

    public void StartLoadScene(string sceneName)
    {
        StartCoroutine(Coroutine_LoadAsyncScene(sceneName));
    }

    public void EndLoadScene()
    {
        fade.raycastTarget = false;
        StartCoroutine(Fade(0f));
    }

    private IEnumerator Coroutine_LoadAsyncScene(string sceneName)
    {
        fade.raycastTarget = true;
        yield return StartCoroutine(Fade(1f));

        //var startTime = Time.time;
        var asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            Debug.Log(asyncLoad.progress);
            //if (asyncLoad.progress >= 0.9f && Time.time - startTime >= minimumLoadingTime)
            //{
            //    asyncLoad.allowSceneActivation = true; // 최소 로딩 시간이 지나면 씬 활성화
            //}
            yield return null;
        }

        //fade.raycastTarget = false;
        //yield return StartCoroutine(Fade(0f));
    }

    private IEnumerator Fade(float finalAlpha)
    {
        float fadeSpeed = Mathf.Abs(fade.color.a - finalAlpha) / fadeDuration;
        while (!Mathf.Approximately(fade.color.a, finalAlpha))
        {
            var newAlpha = Mathf.MoveTowards(fade.color.a, finalAlpha, fadeSpeed * Time.deltaTime);
            fade.color = new Color(0f, 0f, 0f, newAlpha);
            yield return null;
        }
    }
}
