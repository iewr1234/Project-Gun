using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using TMPro;

public class InputFilter : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;

    public readonly int maxDigits = 4; // �ִ� �ڸ���

    private void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(string newText)
    {
        string filteredText = Regex.Replace(newText, "[^0-9]", ""); // ���� �̿��� ���� ����
        if (filteredText != newText)
        {
            inputField.text = filteredText;
        }

        // �ִ� �ڸ��� ����
        if (filteredText.Length > maxDigits)
        {
            inputField.text = filteredText.Substring(0, maxDigits); // �ִ� �ڸ������� �ڸ���
        }
    }
}