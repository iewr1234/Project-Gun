using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NodeOutline : MonoBehaviour
{
    [Header("---Access Component---")]
    public Cover lineCover;

    private List<MeshRenderer> meshs = new List<MeshRenderer>();
    [HideInInspector] public MeshRenderer unableMove;

    [Header("--- Assignment Variable---")]
    public Vector2 linePos;

    public void SetComponents(int index, FieldNode node)
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

        var lineDir = (TargetDirection)index;
        transform.position = node.transform.position + DataUtility.GetPositionOfNodeOutline(lineDir);
        transform.rotation = DataUtility.GetRotationOfNodeOutline(lineDir);
        node.outlines[index] = this;
        switch (lineDir)
        {
            case TargetDirection.Left:
                linePos = new Vector2(node.nodePos.x - 0.5f, node.nodePos.y);
                break;
            case TargetDirection.Front:
                linePos = new Vector2(node.nodePos.x, node.nodePos.y - 0.5f);
                break;
            case TargetDirection.Back:
                linePos = new Vector2(node.nodePos.x, node.nodePos.y + 0.5f);
                break;
            case TargetDirection.Right:
                linePos = new Vector2(node.nodePos.x + 0.5f, node.nodePos.y);
                break;
            default:
                break;
        }
        transform.name = $"Outline_X{linePos.x}/Y{linePos.y}";
    }

    public void SetActiveLine(bool value)
    {
        for (int i = 0; i < meshs.Count; i++)
        {
            var mesh = meshs[i];
            mesh.enabled = value;
            mesh.material.color = DataUtility.canShotLineColor;
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
