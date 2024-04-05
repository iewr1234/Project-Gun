using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeOutline : MonoBehaviour
{
    private List<MeshRenderer> meshs = new List<MeshRenderer>();

    public void SetComponents()
    {
        meshs = GetComponentsInChildren<MeshRenderer>().ToList();
        for (int i = 0; i < meshs.Count; i++)
        {
            var mesh = meshs[i];
            mesh.material = new Material(mesh.material);
            mesh.enabled = false;
        }
    }

    public void SetActiveLine(bool value)
    {
        for (int i = 0; i < meshs.Count; i++)
        {
            var mesh = meshs[i];
            mesh.enabled = value;
        }
    }
}
