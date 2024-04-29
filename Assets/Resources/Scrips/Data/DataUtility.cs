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

    public static float GetDistance(Vector3 posA, Vector3 posB)
    {
        var distance = Mathf.Sqrt((posA - posB).sqrMagnitude);
        return Mathf.Round(distance * 100) / 100;
    }

    public static void SetMeshsMaterial(List<MeshRenderer> meshs, string shaderName)
    {
        for (int i = 0; i < meshs.Count; i++)
        {
            var mesh = meshs[i];
            mesh.material.shader = Shader.Find(shaderName);
        }
    }

    public static void SetMeshsMaterial(CharacterOwner ownerType, List<MeshRenderer> meshs)
    {
        for (int i = 0; i < meshs.Count; i++)
        {
            var mesh = meshs[i];
            if (mesh.CompareTag("Glass")) continue;

            mesh.material = new Material(mesh.material);
            mesh.material.shader = Shader.Find("Draw/AlwaysVisible");
            switch (ownerType)
            {
                case CharacterOwner.Player:
                    mesh.material.SetColor("_PhantomColor", color_Player);
                    break;
                case CharacterOwner.Enemy:
                    mesh.material.SetColor("_PhantomColor", color_Enemy);
                    break;
                default:
                    break;
            }
        }
    }

    public static void SetMeshsMaterial(List<SkinnedMeshRenderer> sMeshs, string shaderName)
    {
        for (int i = 0; i < sMeshs.Count; i++)
        {
            var sMesh = sMeshs[i];
            sMesh.material.shader = Shader.Find(shaderName);
        }
    }

    public static void SetMeshsMaterial(CharacterOwner ownerType, List<SkinnedMeshRenderer> sMeshs)
    {
        for (int i = 0; i < sMeshs.Count; i++)
        {
            var sMesh = sMeshs[i];
            sMesh.material = new Material(sMesh.material);
            sMesh.material.shader = Shader.Find("Draw/AlwaysVisible");
            switch (ownerType)
            {
                case CharacterOwner.Player:
                    sMesh.material.SetColor("_PhantomColor", color_Player);
                    break;
                case CharacterOwner.Enemy:
                    sMesh.material.SetColor("_PhantomColor", color_Enemy);
                    break;
                default:
                    break;
            }
        }
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