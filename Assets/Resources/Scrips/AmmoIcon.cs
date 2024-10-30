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
    Chamber,
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

    private class Chamber
    {
        public RectTransform rect;
        public Image iconImage;
    }

    [Header("---Access Component---")]
    private RectTransform rect;
    private Image raycastImage;

    private Magazine mag;
    private Grenade grd;
    private Bullet bullet;
    private Chamber chamber;

    [Header("--- Assignment Variable---")]
    public AmmoIconType type;
    public ItemHandler item;
    public int maxValue;
    public int value;

    private readonly Vector3 activeScale = new Vector3(1.5f, 1.5f, 1f);

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

        chamber = new Chamber()
        {
            rect = transform.Find("Chamber").GetComponent<RectTransform>(),
            iconImage = transform.Find("Chamber/Image").GetComponent<Image>(),
        };
        chamber.rect.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }

    public void SetAmmoIcon(AmmoIconType _type, ItemHandler _item)
    {
        Initialize();
        type = _type;
        item = _item;
        switch (type)
        {
            case AmmoIconType.Magazine:
                mag.rect.gameObject.SetActive(true);
                mag.slider.maxValue = item.magData.magSize;
                mag.slider.value = item.magData.loadedBullets.Count;
                break;
            case AmmoIconType.Grenade:
                grd.rect.gameObject.SetActive(true);
                break;
            case AmmoIconType.Bullet:
                bullet.rect.gameObject.SetActive(true);
                if (item.bulletData.pelletNum == 0)
                {
                    bullet.iconImage.sprite = Resources.Load<Sprite>("Sprites/Icon_PistolBullet");
                }
                else
                {
                    bullet.iconImage.sprite = Resources.Load<Sprite>("Sprites/Icon_ShotgunCell");
                }
                bullet.upArrowImage.enabled = false;
                bullet.downArrowImage.enabled = false;
                maxValue = item.TotalCount;
                value = 1;
                bullet.numText.enabled = false;
                bullet.numText.text = $"{value}";
                break;
            case AmmoIconType.Chamber:
                chamber.rect.gameObject.SetActive(true);
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
                mag.rect.localScale = Vector3.one;
                mag.rect.gameObject.SetActive(false);
                break;
            case AmmoIconType.Grenade:
                grd.rect.localScale = Vector3.one;
                grd.rect.gameObject.SetActive(false);
                break;
            case AmmoIconType.Bullet:
                bullet.rect.localScale = Vector3.one;
                bullet.rect.gameObject.SetActive(false);
                break;
            case AmmoIconType.Chamber:
                chamber.rect.localScale = Vector3.one;
                chamber.rect.gameObject.SetActive(false);
                break;
            default:
                break;
        }
        type = AmmoIconType.None;
        item = null;
        maxValue = 0;
        value = 0;
        gameObject.SetActive(false);
    }

    public void SetActiveIcon(bool zoom)
    {
        switch (type)
        {
            case AmmoIconType.Magazine:
                mag.rect.localScale = zoom ? activeScale : Vector3.one;
                break;
            case AmmoIconType.Grenade:
                grd.rect.localScale = zoom ? activeScale : Vector3.one;
                break;
            case AmmoIconType.Bullet:
                bullet.rect.localScale = zoom ? activeScale : Vector3.one;
                bullet.upArrowImage.enabled = zoom;
                bullet.downArrowImage.enabled = zoom;
                bullet.numText.enabled = zoom;
                break;
            case AmmoIconType.Chamber:
                chamber.rect.localScale = zoom ? activeScale : Vector3.one;
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
                if (nextValue < 1) return;

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
            case AmmoIconType.Chamber:
                break;
            default:
                break;
        }
    }
}
