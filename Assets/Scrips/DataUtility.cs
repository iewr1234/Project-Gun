using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DataUtility
{
    public static readonly float nodeSize = 1.2f;
    public static readonly float nodeInterval = 0.1f;

    public static readonly Color pMaterialColor = new Color(90 / 255f, 130 / 255f, 192 / 255f);
    public static readonly Color eMaterialColor = new Color(192 / 255f, 94 / 255f, 90 / 255f);

    public static float GetDistance(Vector3 posA, Vector3 posB)
    {
        var distance = Mathf.Sqrt((posA - posB).sqrMagnitude);
        return Mathf.Round(distance * 100) / 100;
    }

    public static void SetMeshsMaterial(CharacterOwner ownerType, List<MeshRenderer> meshs)
    {
        for (int i = 0; i < meshs.Count; i++)
        {
            var mesh = meshs[i];
            var mtName = mesh.material.name.Split(' ')[0];
            var mt = Resources.Load<Material>($"Materials/AlwaysVisible/{mtName}(AV)");
            if (mt != null)
            {
                mesh.material = new Material(mt);
                switch (ownerType)
                {
                    case CharacterOwner.Player:
                        mesh.material.SetColor("_PhantomColor", pMaterialColor);
                        break;
                    case CharacterOwner.Enemy:
                        mesh.material.SetColor("_PhantomColor", eMaterialColor);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public static void SetMeshsMaterial(CharacterOwner ownerType, List<SkinnedMeshRenderer> sMeshs)
    {
        for (int i = 0; i < sMeshs.Count; i++)
        {
            var sMesh = sMeshs[i];
            var mtName = sMesh.material.name.Split(' ')[0];
            var mt = Resources.Load<Material>($"Materials/AlwaysVisible/{mtName}(AV)");
            if (mt != null)
            {
                sMesh.material = new Material(mt);
                switch (ownerType)
                {
                    case CharacterOwner.Player:
                        sMesh.material.SetColor("_PhantomColor", pMaterialColor);
                        break;
                    case CharacterOwner.Enemy:
                        sMesh.material.SetColor("_PhantomColor", eMaterialColor);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}