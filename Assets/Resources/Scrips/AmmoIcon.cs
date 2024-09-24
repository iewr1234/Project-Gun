using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        public RectTransform rect;
        public Slider slider;
        public Image frameImage;
        public Image backImage;
    }

    private class Grenade
    {
        public RectTransform rect;
        public Image iconImage;
    }

    private class Bullet
    {
        public RectTransform rect;
        public Image iconImage;
        public Image upArrowImage;
        public Image downArrowImage;
        public TextMeshProUGUI numText;
    }

    [Header("---Access Component---")]
    private RectTransform rect;
    private Image raycastImage;

    private Magazine mag;
    private Grenade grd;
    private Bullet bullet;

    [Header("--- Assignment Variable---")]
    public AmmoIconType type;
    public int maxValue;
    public int value;

    private readonly Vector3 activeScale = new Vector3(1.3f, 1.3f, 1f);

    public void SetComponents()
    {
        rect = GetComponent<RectTransform>();
        raycastImage = GetComponent<Image>();

        mag = new Magazine()
        {
            rect = transform.Find("Magazine").GetComponent<RectTransform>(),
            slider = transform.Find("Magazine/Slider").GetComponent<Slider>(),
            frameImage = transform.Find("Magazine/Frame").GetComponent<Image>(),
            backImage = transform.Find("Magazine/Slider").GetComponent<Image>(),
        };
        mag.rect.gameObject.SetActive(false);

        grd = new Grenade()
        {
            rect = transform.Find("Grenade").GetComponent<RectTransform>(),
            iconImage = transform.Find("Grenade/Image").GetComponent<Image>(),
        };
        grd.rect.gameObject.SetActive(false);

        bullet = new Bullet()
        {
            rect = transform.Find("Bullet").GetComponent<RectTransform>(),
            iconImage = transform.Find("Bullet/Image").GetComponent<Image>(),
            upArrowImage = transform.Find("Bullet/UpArrow").GetComponent<Image>(),
            downArrowImage = transform.Find("Bullet/DownArrow").GetComponent<Image>(),
            numText = transform.Find("Bullet/NumText").GetComponent<TextMeshProUGUI>(),
        };
        bullet.rect.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }

    public void SetAmmoIcon(AmmoIconType _type, ItemHandler _item)
    {
        Initialize();
        type = _type;
        switch (type)
        {
            case AmmoIconType.Magazine:
                mag.rect.gameObject.SetActive(true);
                mag.slider.maxValue = _item.magData.magSize;
                mag.slider.value = _item.magData.loadedBullets.Count;
                break;
            case AmmoIconType.Grenade:
                grd.rect.gameObject.SetActive(true);
                break;
            case AmmoIconType.Bullet:
                bullet.rect.gameObject.SetActive(true);
                if (_item.bulletData.pelletNum == 0)
                {
                    bullet.iconImage.sprite = Resources.Load<Sprite>("Sprites/Icon_PistolBullet");
                }
                else
                {
                    bullet.iconImage.sprite = Resources.Load<Sprite>("Sprites/Icon_ShotgunCell");
                }
                bullet.upArrowImage.enabled = false;
                bullet.downArrowImage.enabled = false;
                maxValue = _item.TotalCount;
                value = 0;
                bullet.numText.enabled = false;
                bullet.numText.text = $"{value}";
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
                mag.rect.transform.localScale = Vector3.one;
                mag.rect.gameObject.SetActive(false);
                break;
            case AmmoIconType.Grenade:
                grd.rect.transform.localScale = Vector3.one;
                grd.rect.gameObject.SetActive(false);
                break;
            case AmmoIconType.Bullet:
                bullet.rect.transform.localScale = Vector3.one;
                bullet.rect.gameObject.SetActive(false);
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
                mag.rect.transform.localScale = zoom ? activeScale : Vector3.one;
                break;
            case AmmoIconType.Grenade:
                grd.rect.transform.localScale = zoom ? activeScale : Vector3.one;
                break;
            case AmmoIconType.Bullet:
                bullet.rect.transform.localScale = zoom ? activeScale : Vector3.one;
                bullet.upArrowImage.enabled = zoom;
                bullet.downArrowImage.enabled = zoom;
                bullet.numText.enabled = zoom;
                break;
            default:
                break;
        }
    }

    public void SetAmmoValue(int reloadMax, bool upValue)
    {
        int nextValue;
        switch (upValue)
        {
            case true:
                nextValue = value + 1;
                if (nextValue > reloadMax || nextValue > maxValue) return;

                value = nextValue;
                break;
            case false:
                nextValue = value - 1;
                if (nextValue < 0) return;

                value = nextValue;
                break;
        }
        bullet.numText.text = $"{value}";
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
