using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeOutline : MonoBehaviour
{
    [Header("---Access Component---")]
    public Cover lineCover;

    private List<MeshRenderer> meshs = new List<MeshRenderer>();
    [HideInInspector] public MeshRenderer unableMove;

    public void SetComponents()
    {
        meshs = transform.Find("Meshs").GetComponentsInChildren<MeshRenderer>().ToList();
        for (int i = 0; i < meshs.Count; i++)
        {
            var mesh = meshs[i];
            mesh.material = new Material(mesh.material);
            mesh.enabled = false;
        }
        unableMove = transform.Find("UnableMove").GetComponent<MeshRenderer>();
        unableMove.enabled = false;
    }

    public void SetActiveLine(bool value)
    {
        for (int i = 0; i < meshs.Count; i++)
        {
            var mesh = meshs[i];
            mesh.enabled = value;
            mesh.material.color = Color.white;
        }
    }

    public void SetActiveLine(bool value, Color color)
    {
        for (int i = 0; i < meshs.Count; i++)
        {
            var mesh = meshs[i];
            mesh.enabled = value;
            mesh.material.color = color;
        }
    }
}
