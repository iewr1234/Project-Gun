using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public enum AmmoIconType
{
    None,
    Magazine,
    Grenade,
    Bullet,
}

public class AmmoIcon : MonoBehaviour
{
    private class Magazine
    {
        public GameObject component;
        public Slider slider;
        public Image frameImage;
        public Image backImage;
    }

    private class Grenade
    {
        public GameObject component;
        public Image iconImage;
    }

    private class Bullet
    {
        public GameObject component;
    }

    [Header("---Access Component---")]
    private RectTransform rect;
    private Image raycastImage;

    private Magazine mag;
    private Grenade grd;
    private Bullet bullet;

    [Header("--- Assignment Variable---")]
    public AmmoIconType type;

    private readonly Vector3 activeScale = new Vector3(1.3f, 1.3f, 1f);

    public void SetComponents()
    {
        rect = GetComponent<RectTransform>();
        raycastImage = GetComponent<Image>();

        mag = new Magazine()
        {
            component = transform.Find("Magazine").gameObject,
            slider = transform.Find("Magazine/Slider").GetComponent<Slider>(),
            frameImage = transform.Find("Magazine/Frame").GetComponent<Image>(),
            backImage = transform.Find("Magazine/Slider").GetComponent<Image>(),
        };
        mag.component.SetActive(false);

        grd = new Grenade()
        {
            component = transform.Find("Grenade").gameObject,
            iconImage = transform.Find("Grenade/Image").GetComponent<Image>(),
        };
        grd.component.SetActive(false);

        bullet = new Bullet()
        {
            component = transform.Find("Grenade").gameObject,
        };
        bullet.component.SetActive(false);

        gameObject.SetActive(false);
    }

    public void SetAmmoIcon(AmmoIconType _type, ItemHandler _item)
    {
        Initialize();
        type = _type;
        switch (type)
        {
            case AmmoIconType.Magazine:
                mag.component.SetActive(true);
                mag.slider.maxValue = _item.magData.magSize;
                mag.slider.value = _item.magData.loadedBullets.Count;
                break;
            case AmmoIconType.Grenade:
                grd.component.SetActive(true);
                break;
            case AmmoIconType.Bullet:
                bullet.component.SetActive(true);
                break;
            default:
                break;
        }
        gameObject.SetActive(true);
    }

    public void Initialize()
    {
        if (type == AmmoIconType.None) return;

        switch (type)
        {
            case AmmoIconType.Magazine:
                mag.frameImage.transform.localScale = Vector3.one;
                mag.backImage.transform.localScale = Vector3.one;
                mag.component.SetActive(false);
                break;
            case AmmoIconType.Grenade:
                grd.iconImage.transform.localScale = Vector3.one;
                grd.component.SetActive(false);
                break;
            case AmmoIconType.Bullet:
                bullet.component.SetActive(false);
                break;
            default:
                break;
        }
        type = AmmoIconType.None;
        gameObject.SetActive(false);
    }

    public void SetActiveIcon(bool zoom)
    {
        switch (type)
        {
            case AmmoIconType.Magazine:
                mag.frameImage.transform.localScale = zoom ? activeScale : Vector3.one;
                mag.backImage.transform.localScale = zoom ? activeScale : Vector3.one;
                break;
            case AmmoIconType.Grenade:
                grd.iconImage.transform.localScale = zoom ? activeScale : Vector3.one;
                break;
            default:
                break;
        }
    }

    public void Button_AmmoIcon()
    {
        switch (type)
        {
            case AmmoIconType.Magazine:
                break;
            case AmmoIconType.Grenade:
                break;
            case AmmoIconType.Bullet:
                break;
            default:
                break;
        }
    }
}
