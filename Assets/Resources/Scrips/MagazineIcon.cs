using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MagazineIcon : MonoBehaviour
{
    [Header("---Access Component---")]
    [SerializeField] private Slider slider;

    private Image frameImage;
    private Image backImage;

    public void SetComponents()
    {
        slider = transform.Find("Slider").GetComponent<Slider>();

        frameImage = transform.Find("Frame").GetComponent<Image>();
        backImage = slider.transform.GetComponent<Image>();

        gameObject.SetActive(false);
    }

    public void SetMagazineSlider(int maxValue, int value)
    {
        slider.maxValue = maxValue;
        slider.value = value;
    }

    public void SetImageScale(bool zoom)
    {
        switch (zoom)
        {
            case true:
                frameImage.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
                backImage.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
                break;
            case false:
                frameImage.transform.localScale = Vector3.one;
                backImage.transform.localScale = Vector3.one;
                break;
        }
    }

    public void Button_MagazineIcon()
    {

    }
}
