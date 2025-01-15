using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ErrorUI : MonoBehaviour
{
    [Header("---Access Script---")]
    private DataManager dataMgr;

    [Header("---Access Component---")]
    public Animator animator;
    public TextMeshProUGUI errorText;

    private void Awake()
    {
        dataMgr = FindAnyObjectByType<DataManager>();

        animator = GetComponent<Animator>();
        errorText = transform.Find("GameError/Text").GetComponent<TextMeshProUGUI>();

        DontDestroyOnLoad(gameObject);
    }

    public void ShowError(string ErrorID)
    {
        ErrorCodeDataInfo errorCode = dataMgr.errorCodeData.errorCodeInfos.Find(x => x.errorID == ErrorID);
        if (errorCode == null)
        {
            errorText.text = "에러코드를 불러오지 못하였습니다.";
        }
        else
        {
            errorText.text = errorCode.errorText;
        }
        animator.SetTrigger("warning");
    }
}
