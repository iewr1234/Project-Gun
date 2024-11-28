using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI aSpeedText;

    [Space(5f)][SerializeField] private Animator char_before;
    [SerializeField] private Animator char_after;

    private readonly float maxAnimatorSpeed = 2f;
    private readonly float minAnimatorSpeed = 0.1f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            char_before.SetBool("isMove", !char_before.GetBool("isMove"));
            char_after.SetBool("isMove", !char_after.GetBool("isMove"));
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            if (!char_before.GetBool("isCover"))
            {
                char_before.SetBool("isCover", true);
                char_before.SetBool("fullCover", false);
                char_after.SetBool("isCover", true);
                char_after.SetBool("fullCover", false);
            }
            else
            {
                char_before.SetBool("isCover", false);
                char_after.SetBool("isCover", false);
            }
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            float curSpeed = char_before.speed;
            if (curSpeed == minAnimatorSpeed) curSpeed = 0f;

            curSpeed += 0.5f;
            if (curSpeed > maxAnimatorSpeed) curSpeed = maxAnimatorSpeed;

            char_before.speed = curSpeed;
            char_after.speed = curSpeed;
            aSpeedText.text = $"애니메이션 속도: {curSpeed:F1}";
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            float curSpeed = char_before.speed;
            curSpeed -= 0.5f;
            if (curSpeed < minAnimatorSpeed) curSpeed = minAnimatorSpeed;

            char_before.speed = curSpeed;
            char_after.speed = curSpeed;
            aSpeedText.text = $"애니메이션 속도: {curSpeed:F1}";
        }
    }
}
