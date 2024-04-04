using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowPointer : MonoBehaviour
{
    [SerializeField] private float speed = 0.5f;

    private void Update()
    {
        var rotVal = Time.unscaledDeltaTime * speed;
        var newRot = new Vector3(0f, rotVal, 0f);

        transform.Rotate(newRot);
    }
}
