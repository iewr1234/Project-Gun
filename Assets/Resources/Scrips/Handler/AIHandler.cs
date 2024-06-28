using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIType
{
    None,
    A,
    B,
    C,
}

public class AIHandler : MonoBehaviour
{
    [Header("---Access Script---")]
    private CharacterController charCtr;

    [Header("--- Assignment Variable---")]
    public AIType type;

    public void SetComponents(CharacterController _charCtr, AIType _type)
    {
        charCtr = _charCtr;
        charCtr.aiHlr = this;

        type = _type;
    }
}
