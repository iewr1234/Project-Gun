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
    public static readonly float nodeSize = 1.2f;
    public static readonly float nodeInterval = 0.1f;

    public static readonly Color color_Player = new Color(90 / 255f, 130 / 255f, 192 / 255f);
    public static readonly Color color_Enemy = new Color(192 / 255f, 94 / 255f, 90 / 255f);

    public static readonly float aimPointY = 0.9f;
    public static readonly float aimPointZ = 5f;

    public static readonly float lineInterval = 0.5f;

    public static Quaternion GetSetRotation(TargetDirection setDirection)
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