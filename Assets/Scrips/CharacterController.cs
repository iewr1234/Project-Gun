using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [Header("---Access Script---")]
    [SerializeField] private GameManager gameMgr;

    [Header("--- Assignment Variable---")]
    public FieldNode currentNode;

    public void SetComponents(GameManager _gameMgr, FieldNode _currentNode)
    {
        gameMgr = _gameMgr;
        currentNode = _currentNode;

        currentNode.NodeColor = Color.white;
    }
}
