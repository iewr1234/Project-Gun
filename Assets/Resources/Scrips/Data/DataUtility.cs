using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public enum TargetDirection
{
    None = -1,
    Left,
    Front,
    Back,
    Right,
}

public static class DataUtility
{
    public static readonly string mapDataPath = "/MapData/";

    public static readonly float nodeSize = 1.2f;
    public static readonly float nodeInterval = 0.1f;
    private static readonly Vector3 outlinePos_Left = new Vector3(-0.65f, 0f, 0f);
    private static readonly Quaternion outlineRot_Left = Quaternion.identity;
    private static readonly Vector3 outlinePos_Front = new Vector3(0f, 0f, -0.65f);
    private static readonly Quaternion outlineRot_Front = Quaternion.Euler(0f, 90f, 0f);
    private static readonly Vector3 outlinePos_Back = new Vector3(0f, 0f, 0.65f);
    private static readonly Quaternion outlineRot_Back = Quaternion.Euler(0f, 90f, 0f);
    private static readonly Vector3 outlinePos_Right = new Vector3(0.65f, 0f, 0f);
    private static readonly Quaternion outlineRot_Right = Quaternion.identity;

    public static readonly Color color_Player = new Color(90 / 255f, 130 / 255f, 192 / 255f);
    public static readonly Color color_Enemy = new Color(192 / 255f, 94 / 255f, 90 / 255f);

    public static readonly float aimPointY = 0.9f;
    public static readonly float aimPointZ = 5f;

    public static readonly float lineInterval = 0.5f;

    public static readonly int itemSize = 70;
    public static readonly Color slot_noItemColor = new Color(135 / 255f, 135 / 255f, 135 / 255f, 1f);
    public static readonly Color slot_onItemColor = new Color(190 / 255f, 200 / 255f, 1f, 1f);
    public static readonly Color slot_moveColor = new Color(180 / 255f, 1f, 145 / 255f, 1f);
    public static readonly Color slot_unMoveColor = new Color(1f, 145 / 255f, 145 / 255f, 1f);

    public static readonly Color equip_defaultColor = new Color(25 / 255f, 25 / 255f, 25 / 255f);

    public static readonly Vector3Int popUp_defaultPos = new Vector3Int(0, 350, -50);
    public static readonly Vector3Int popUp_defaultPos_split = new Vector3Int(0, 150, -50);

    public static float GetDistance(Vector3 posA, Vector3 posB)
    {
        var distance = Mathf.Sqrt((posA - posB).sqrMagnitude);
        return Mathf.Round(distance * 100) / 100;
    }

    public static Vector3 GetPositionOfNodeOutline(TargetDirection setDirection)
    {
        switch (setDirection)
        {
            case TargetDirection.Left:
                return outlinePos_Left;
            case TargetDirection.Front:
                return outlinePos_Front;
            case TargetDirection.Back:
                return outlinePos_Back;
            case TargetDirection.Right:
                return outlinePos_Right;
            default:
                return Vector3.zero;
        }
    }

    public static Quaternion GetRotationOfNodeOutline(TargetDirection setDirection)
    {
        switch (setDirection)
        {
            case TargetDirection.Left:
                return outlineRot_Left;
            case TargetDirection.Front:
                return outlineRot_Front;
            case TargetDirection.Back:
                return outlineRot_Back;
            case TargetDirection.Right:
                return outlineRot_Right;
            default:
                return Quaternion.identity;
        }
    }

    public static Quaternion GetSetRotationOfObject(TargetDirection setDirection)
    {
        var rot = Vector3.zero;
        switch (setDirection)
        {
            case TargetDirection.Left:
                rot = new Vector3(0f, 90f, 0f);
                break;
            case TargetDirection.Front:
                rot = new Vector3(0f, 0f, 0f);
                break;
            case TargetDirection.Back:
                rot = new Vector3(0f, 180f, 0f);
                break;
            case TargetDirection.Right:
                rot = new Vector3(0f, 270f, 0f);
                break;
            default:
                break;
        }

        return Quaternion.Euler(rot);
    }

    /// <summary>
    /// 조준 시 스테미나 사용량 계산
    /// </summary>
    /// <returns></returns>
    public static int GetAimStaminaCost(CharacterController charCtr)
    {
        var rebound = charCtr.currentWeapon.weaponData.GetWeaponRebound();
        var weight = charCtr.currentWeapon.weaponData.GetWeaponWeight();
        var strength = charCtr.strength;
        var result = 3 + (rebound * weight * 0.01f) * (1 / (1 + strength * 0.01f));

        return Mathf.CeilToInt(result);
    }

    /// <summary>
    /// 명중률 계산
    /// </summary>
    /// <param name="charCtr"></param>
    /// <param name="targetInfo"></param>
    /// <returns></returns>
    public static float GetHitAccuracy(CharacterController charCtr, TargetInfo targetInfo)
    {
        var pos = targetInfo.shooterNode.transform.position;
        var targetPos = targetInfo.targetNode.transform.position;
        var dist = GetDistance(pos, targetPos);
        var weapon = charCtr.currentWeapon;
        var reboundCheck = 0;
        if (charCtr.stamina == 0)
        {
            reboundCheck++;
        }
        var value = Random.Range(0, 100);
        var shooterHit = charCtr.aiming - (weapon.weaponData.MOA * dist) + (15 / (dist / 3)) - (weapon.weaponData.rebound * reboundCheck);
        if (shooterHit < 0f)
        {
            shooterHit = 0f;
        }
        var coverBonus = GetCoverBonus();
        var reactionBonus = GetReactionBonus();
        var targetEvasion = coverBonus + (targetInfo.target.reaction * reactionBonus);
        return Mathf.Floor((shooterHit - targetEvasion) * 100f) / 100f;

        int GetCoverBonus()
        {
            if (targetInfo.targetCover == null)
            {
                return 0;
            }
            else
            {
                return targetInfo.targetCover.coverType == CoverType.Full ? 40 : 20;
            }
        }

        float GetReactionBonus()
        {
            if (targetInfo.targetCover == null)
            {
                return 0.1f;
            }
            else
            {
                return targetInfo.targetCover.coverType == CoverType.Full ? 0.4f : 0.2f;
            }
        }
    }

    /// <summary>
    /// 반동 명중 감소량 계산
    /// </summary>
    /// <param name="charCtr"></param>
    /// <param name="dist"></param>
    /// <returns></returns>
    public static float GetHitAccuracyReduction(CharacterController charCtr, float dist)
    {
        var rebound = charCtr.currentWeapon.weaponData.GetWeaponRebound();
        var result = Mathf.Pow(2, dist / 5) * (rebound * 0.1f);

        return GetFloorValue(result, 2);
    }

    public static Vector3 GetAimPosition(Transform charTf, bool isRight)
    {
        var pos = charTf.position;
        if (isRight)
        {
            pos += charTf.right * aimPointZ;
        }
        else
        {
            pos -= charTf.right * aimPointZ;
        }
        pos.y = aimPointY;

        return pos;
    }

    public static float GetFloorValue(float value, int _decimalPoint)
    {
        var decimalPoint = (int)Mathf.Pow(10, _decimalPoint);

        return Mathf.Floor(value * decimalPoint) / decimalPoint;
    }
}