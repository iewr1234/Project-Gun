using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI aSpeedText;

    [Space(5f)][SerializeField] private Animator char_before;
    [SerializeField] private Animator char_after;
    [SerializeField] private GameObject[] weapons;
    private int weaponIndex;

    private readonly float maxAnimatorSpeed = 2f;
    private readonly float minAnimatorSpeed = 0.1f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            bool isMove = !char_before.GetBool("isMove");
            char_before.SetBool("isMove", isMove);
            char_after.SetBool("isMove", isMove);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            bool isAim = !char_before.GetBool("isAim");
            char_before.SetBool("isAim", isAim);
            char_after.SetBool("isAim", isAim);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            if (char_before.GetBool("fullCover")) return;

            bool isCover = !char_before.GetBool("isCover");
            char_before.SetBool("isCover", isCover);
            char_before.SetBool("fullCover", false);
            char_after.SetBool("isCover", isCover);
            char_after.SetBool("fullCover", false);
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            if (!char_before.GetBool("isCover"))
            {
                char_before.SetBool("isCover", true);
                char_before.SetBool("fullCover", true);
                char_before.SetBool("isRight", false);
                char_after.SetBool("isCover", true);
                char_after.SetBool("fullCover", true);
                char_after.SetBool("isRight", false);
            }
            else if (!char_before.GetBool("isRight"))
            {
                char_before.SetBool("isRight", true);
                char_after.SetBool("isRight", true);
            }
            else
            {
                char_before.SetBool("isCover", false);
                char_before.SetBool("fullCover", false);
                char_after.SetBool("isCover", false);
                char_after.SetBool("fullCover", false);
            }
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            char_before.SetBool("reload", true);
            char_before.SetBool("loadChamber", true);
            char_after.SetBool("reload", true);
            char_after.SetBool("loadChamber", true);
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            if (!char_before.GetBool("isCover")) return;

            bool targeting = !char_before.GetCurrentAnimatorStateInfo(3).IsTag("Targeting");
            char_before.SetTrigger(targeting ? "targeting" : "unTargeting");
            char_after.SetTrigger(targeting ? "targeting" : "unTargeting");
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

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            weapons[weaponIndex].SetActive(false);
            weaponIndex++;
            if (weaponIndex == weapons.Length) weaponIndex = 0;

            weapons[weaponIndex].SetActive(true);
        }
    }
}
