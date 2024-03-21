using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Cover : MonoBehaviour
{
    [Header("---Access Script---")]
    public FieldNode node;

    public void SetComponents(FieldNode _node)
    {
        node = _node;

        node.cover = this;
        node.canMove = false;
        node.ReleaseAdjacentNodes();
    }
}
