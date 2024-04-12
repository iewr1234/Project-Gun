using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawRange : MonoBehaviour
{
    [Header("---Access Component---")]
    [SerializeField] private FanMesh fanA;
    [SerializeField] private FanMesh fanB;

    [Header("--- Assignment Variable---")]
    public int angle;
    public float outRadius;
    public float inRadius;
    public float delayRadius;

    public void SetComponents()
    {
        fanA = transform.Find("FanMesh_A").GetComponent<FanMesh>();
        fanA.SetComponents();
        fanB = transform.Find("FanMesh_B").GetComponent<FanMesh>();
        fanB.SetComponents();
    }

    public void SetRange(CharacterController charCtr, FieldNode node)
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        transform.position = charCtr.currentNode.transform.position;
        angle = charCtr.weapon.watchAngle;
        var range = DataUtility.GetDistance(charCtr.currentNode.transform.position, node.transform.position);
        if (range > charCtr.weapon.range)
        {
            range = charCtr.weapon.range;
        }
        outRadius = range;
        inRadius = DataUtility.lineInterval;
        delayRadius = DataUtility.lineInterval;
        fanA.CreateMesh(angle, outRadius, inRadius);
        fanB.CreateMesh(angle, delayRadius, inRadius);
    }
}
