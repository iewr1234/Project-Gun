using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using TMPro;

public class InputFilter : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;

    public readonly int maxDigits = 4; // 최대 자릿수

    private void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(string newText)
    {
        string filteredText = Regex.Replace(newText, "[^0-9]", ""); // 숫자 이외의 문자 제거
        if (filteredText != newText)
        {
            inputField.text = filteredText;
        }

        // 최대 자릿수 제한
        if (filteredText.Length > maxDigits)
        {
            inputField.text = filteredText.Substring(0, maxDigits); // 최대 자릿수까지 자르기
        }
    }
}