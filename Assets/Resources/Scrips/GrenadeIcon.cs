using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrenadeIcon : MonoBehaviour
{
    public Image iconImage;

    private readonly Vector3 activeScale = new Vector3(1.3f, 1.3f, 1f);

    public void SetComponents()
    {
        iconImage = transform.Find("Image").GetComponent<Image>();
        gameObject.SetActive(false);
    }

    public void SetImageScale(bool value)
    {
        iconImage.transform.localScale = value ? activeScale : Vector3.one;
    }
}
