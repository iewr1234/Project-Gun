using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetDirection
{
    None = -1,
    Left,
    Front,
    Back,
    Right,
}

public struct WeaponGripInfo
{
    public Vector3 pivotPos;
    public Quaternion pivotRot;
    public Vector3 gripPos;
    public Quaternion gripRot;
}

public static class DataUtility
{
    public static readonly string mapDataPath = "/Resources/MapData/";

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
    public static readonly Color color_PlayerMarker = new Color(90 / 255f, 130 / 255f, 192 / 255f);
    public static readonly Color color_EnemyMarker = new Color(150 / 255f, 35 / 255f, 35 / 255f);
    public static readonly Color color_baseMarker = new Color(220 / 255f, 220 / 255f, 0f);

    public static readonly int minHitAccuracy = 5;
    public static readonly float aimPointY = 0.2f;
    public static readonly float aimPointZ = 5f;
    public static readonly Vector3 aimInterval = new Vector3(0f, 0.2f, 0f);

    public static readonly float lineInterval = 0.5f;

    public static readonly int itemSize = 70;
    public static readonly Color slot_noItemColor = new Color(135 / 255f, 135 / 255f, 135 / 255f, 1f);
    public static readonly Color slot_onItemColor = new Color(190 / 255f, 200 / 255f, 1f, 1f);
    public static readonly Color slot_moveColor = new Color(180 / 255f, 1f, 145 / 255f, 1f);
    public static readonly Color slot_unMoveColor = new Color(1f, 145 / 255f, 145 / 255f, 1f);

    public static readonly Color equip_defaultColor = new Color(25 / 255f, 25 / 255f, 25 / 255f);

    public static readonly Vector3Int popUp_defaultPos = new Vector3Int(0, 350, -150);
    public static readonly Vector3Int popUp_defaultPos_split = new Vector3Int(0, 150, -150);

    public static readonly int shootRateMax = 4;
    public static readonly int sModeMax = 2;

    public static readonly Vector2Int floorSlotSize = new Vector2Int(7, 17);

    public static readonly Vector3 weaponPivot_pos_HG_1 = new Vector3(0.14f, 0.034f, -0.045f);
    public static readonly Quaternion weaponPivot_rot_HG_1 = Quaternion.Euler(-8.375f, 89f, -90.246f);
    public static readonly Vector3 weaponPivot_pos_HG_2 = new Vector3(0.1f, 0.034f, -0.037f);
    public static readonly Quaternion weaponPivot_rot_HG_2 = Quaternion.Euler(-8.375f, 89f, -90.246f);
    public static readonly Vector3 weaponPivot_pos_SMG_ns = new Vector3(0.082f, 0.034f, -0.037f);
    public static readonly Quaternion weaponPivot_rot_SMG_ns = Quaternion.Euler(-8.375f, 89f, -90.246f);
    public static readonly Vector3 weaponPivot_pos_SMG = new Vector3(0.113f, 0.033f, -0.04f);
    public static readonly Quaternion weaponPivot_rot_SMG = Quaternion.Euler(-8f, 98.92f, -97f);
    public static readonly Vector3 weaponPivot_pos_AR = new Vector3(0.113f, 0.033f, -0.05f);
    public static readonly Quaternion weaponPivot_rot_AR = Quaternion.Euler(-12.9f, 98.92f, -97f);
    public static readonly Vector3 weaponPivot_pos_SR = new Vector3(0.092f, 0.024f, -0.048f);
    public static readonly Quaternion weaponPivot_rot_SR = Quaternion.Euler(5.1f, 95.7f, -89.67f);
    public static readonly Vector3 weaponPivot_pos_SG_PA = new Vector3(0.05f, 0.033f, -0.045f);
    public static readonly Quaternion weaponPivot_rot_SG_PA = Quaternion.Euler(-10.7f, 99f, -97f);
    public static readonly Quaternion weaponPivot_defaultRot = Quaternion.Euler(0f, 90f, -90f);

    public static readonly Vector3 leftPivot_pos_HG = new Vector3(-0.083f, -0.059f, -0.047f);
    public static readonly Quaternion leftPivot_rot_HG = Quaternion.Euler(-19.074f, -32.39f, -91.302f);

    public static readonly Color maxMoveLineColor = Color.white;
    public static readonly Color canShotLineColor = new Color(97f / 255, 197f / 255, 1f);

    public static float GetDistance(Vector3 posA, Vector3 posB)
    {
        var distance = Mathf.Sqrt((posA - posB).sqrMagnitude);
        return Mathf.Round(distance * 100) / 100;
    }

    public static Vector3[] GetParabolaPoints(int length, Vector3 startPos, Vector3 endPos)
    {
        var points = new Vector3[length];
        var center = (startPos + endPos) * 0.5f;
        center.y -= 3;
        startPos -= center;
        endPos -= center;
        for (int i = 0; i < points.Length; i++)
        {
            var point = Vector3.Slerp(startPos, endPos, i / (float)(points.Length - 1));
            point += center;
            points[i] = point;
        }

        return points;
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
    /// 이동칸 수에 따른 행동력 소모 계산
    /// </summary>
    /// <param name="moveRange">이동칸 수</param>
    /// <param name="mobility">이동력</param>
    /// <returns></returns>
    public static int GetMoveCost(int moveRange, float mobility)
    {
        return Mathf.CeilToInt(moveRange / mobility);
    }

    /// <summary>
    /// 조준 시 스테미나 사용량 계산
    /// </summary>
    /// <returns></returns>
    public static int GetAimingStaminaCost(CharacterController charCtr)
    {
        var weaponData = charCtr.currentWeapon.weaponData;
        var rebound = charCtr.rebound;
        var weight = weaponData.GetWeaponWeight();
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
    public static int GetHitAccuracy(TargetInfo targetInfo, float distance, int shootNum)
    {
        var shooter = targetInfo.shooter;
        var target = targetInfo.target;
        int sModeValue;
        switch (targetInfo.shooter.sMode)
        {
            case ShootingMode.PointShot:
                sModeValue = targetInfo.shooter.shootingMode_point;
                break;
            case ShootingMode.AimShot:
                sModeValue = targetInfo.shooter.shootingMode_aim;
                break;
            case ShootingMode.SightShot:
                sModeValue = targetInfo.shooter.shootingMode_sight;
                break;
            default:
                sModeValue = 0;
                break;
        }

        // 사격자 명중률 계산
        //var shooterHit = sModeValue * (1 + shooter.aiming * 0.01f) * (1 / (1 + target.reaction * 0.01f))
        //               * (1 - GetCoverBonus() * 0.01f - 30 * ((Mathf.Pow(distance, 2) - 40) / (Mathf.Pow(distance, 2) + 80)) * 0.01f);
        var shooterHit = sModeValue * (1 + shooter.aiming * 0.01f) * (1 - 30 * ((Mathf.Pow(distance, 2) - 40) / (Mathf.Pow(distance, 2) + 80)) * 0.01f)
                         - GetCoverBonus() * (1 + target.reaction * 0.01f);
        var hitAccuracy = Mathf.FloorToInt(shooterHit);

        // 연사 명중 보정
        //hitAccuracy += Mathf.RoundToInt(30 * (shootNum - 1) / (shootNum - 1 + 25));

        // 최소, 최대값 보정
        if (hitAccuracy > 100)
        {
            hitAccuracy = 100;
        }
        else if (hitAccuracy < minHitAccuracy)
        {
            hitAccuracy = minHitAccuracy;
        }

        return hitAccuracy;

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
    }

    /// <summary>
    /// 반동 명중 감소량 계산
    /// </summary>
    /// <param name="charCtr"></param>
    /// <param name="dist"></param>
    /// <returns></returns>
    public static float GetHitAccuracyReduction(CharacterController charCtr, float dist)
    {
        var weaponData = charCtr.currentWeapon.weaponData;
        var rebound = weaponData.GetWeaponRebound(charCtr.propellant);
        var result = Mathf.Pow(2, dist / 5) * (rebound * 0.1f);

        return GetFloorValue(result, 2);
    }

    /// <summary>
    /// 발사수 계산
    /// </summary>
    /// <param name="RPM"></param>
    /// <param name="fireRate"></param>
    /// <returns></returns>
    public static int GetShootNum(int RPM, int fireRate)
    {
        var result = (int)(((float)RPM / 200) * (fireRate + 1));
        return result == 0 ? 1 : result;
    }

    public static float GetFloorValue(float value, int _decimalPoint)
    {
        var decimalPoint = (int)Mathf.Pow(10, _decimalPoint);

        return Mathf.Floor(value * decimalPoint) / decimalPoint;
    }

    public static string GetScriptText(string textInfo, int value)
    {
        var textData = textInfo.Split('{', '}');
        string scriptText = null;
        for (int i = 0; i < textData.Length; i++)
        {
            var text = textData[i];
            if (text == "value")
            {
                scriptText += $"{value}";
            }
            else
            {
                scriptText += text;
            }
        }

        return scriptText;
    }

    public static WeaponGripInfo GetWeaponGripInfo(WeaponGripType type)
    {
        WeaponGripInfo gripInfo;
        switch (type)
        {
            case WeaponGripType.Handgun_1:
                gripInfo = new WeaponGripInfo()
                {
                    pivotPos = weaponPivot_pos_HG_1,
                    pivotRot = weaponPivot_defaultRot,
                };
                break;
            case WeaponGripType.Handgun_2:
                gripInfo = new WeaponGripInfo()
                {
                    pivotPos = weaponPivot_pos_HG_2,
                    pivotRot = weaponPivot_defaultRot,
                };
                break;
            case WeaponGripType.SubMachineGun_noStock:
                gripInfo = new WeaponGripInfo()
                {
                    pivotPos = weaponPivot_pos_SMG,
                    pivotRot = weaponPivot_defaultRot,
                };
                break;
            case WeaponGripType.SubMachineGun:
                gripInfo = new WeaponGripInfo()
                {
                    pivotPos = weaponPivot_pos_SMG,
                    pivotRot = weaponPivot_defaultRot,
                };
                break;
            case WeaponGripType.AssaultRifle:
                gripInfo = new WeaponGripInfo()
                {
                    pivotPos = weaponPivot_pos_AR,
                    pivotRot = weaponPivot_defaultRot,
                };
                break;
            case WeaponGripType.SniperRifle_BoltAction:
                gripInfo = new WeaponGripInfo()
                {
                    pivotPos = weaponPivot_pos_SR,
                    pivotRot = weaponPivot_defaultRot,
                };
                break;
            case WeaponGripType.SniperRifle_SemiAuto:
                gripInfo = new WeaponGripInfo()
                {
                    pivotPos = weaponPivot_pos_SR,
                    pivotRot = weaponPivot_defaultRot,
                };
                break;
            case WeaponGripType.Shotgun_PumpAction:
                gripInfo = new WeaponGripInfo()
                {
                    pivotPos = weaponPivot_pos_SG_PA,
                    pivotRot = weaponPivot_defaultRot,
                };
                break;
            case WeaponGripType.Shotgun_SemiAuto:
                gripInfo = new WeaponGripInfo()
                {
                    pivotPos = weaponPivot_pos_AR,
                    pivotRot = weaponPivot_defaultRot,
                };
                break;
            default:
                gripInfo = new WeaponGripInfo();
                break;
        }

        return gripInfo;
    }
}