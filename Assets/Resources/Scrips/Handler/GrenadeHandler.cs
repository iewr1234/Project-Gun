using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeHandler : MonoBehaviour
{
    [Header("---Access Component---")]
    public LineRenderer lineRdr;
    public Collider rangeCdr;
    private List<GameObject> grenades = new List<GameObject>();

    [Header("--- Assignment Variable---")]
    public GameObject curGrenade;

    public void SetComponents()
    {
        lineRdr = GetComponent<LineRenderer>();
        lineRdr.positionCount = 60;
        lineRdr.startWidth = 0.03f;
        lineRdr.enabled = false;
        rangeCdr = Instantiate(Resources.Load<Collider>("Prefabs/ExplosionRange"));
        rangeCdr.name = "ExplosionRange";
        rangeCdr.transform.SetParent(transform, false);
        rangeCdr.gameObject.SetActive(false);

        var grenadesTf = transform.Find("Grenades");
        for (int i = 0; i < grenadesTf.childCount; i++)
        {
            var grenade = transform.GetChild(i).gameObject;
            grenades.Add(grenade);
            grenade.SetActive(false);
        }
    }
}
