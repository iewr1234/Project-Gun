using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("---Access Script---")]
    [SerializeField] private Weapon weapon;

    [Header("---Access Component---")]
    [HideInInspector] public TrailRenderer trail;
    [HideInInspector] public Rigidbody bulletRb;
    private Collider bulletCd;
    private List<MeshRenderer> meshRdrs = new List<MeshRenderer>();

    [Header("--- Assignment Variable---")]
    public float speed = 150f;
    public float damage;

    private float timer;

    private readonly float startWidth = 0.01f;
    private readonly float destroyTime = 1f;

    public void SetComponents(Weapon _weapon)
    {
        weapon = _weapon;

        if (trail == null)
        {
            trail = GetComponent<TrailRenderer>();
        }
        trail.enabled = true;
        trail.startWidth = startWidth;
        trail.endWidth = 0f;

        if (bulletRb == null)
        {
            bulletRb = GetComponent<Rigidbody>();
        }
        bulletRb.constraints = RigidbodyConstraints.None;
        bulletRb.isKinematic = false;

        if (bulletCd == null)
        {
            bulletCd = GetComponent<Collider>();
        }
        bulletCd.enabled = true;

        if (meshRdrs.Count == 0)
        {
            meshRdrs = GetComponentsInChildren<MeshRenderer>().ToList();
        }
        for (int i = 0; i < meshRdrs.Count; i++)
        {
            var meshRdr = meshRdrs[i];
            meshRdr.enabled = true;
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > destroyTime)
        {
            timer = 0f;
            trail.Clear();
            trail.enabled = false;
            gameObject.SetActive(false);
        }
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    Debug.Log("collision");
    //    bulletRb.velocity = Vector3.zero;
    //    bulletRb.constraints = RigidbodyConstraints.FreezeAll;
    //    bulletRb.isKinematic = true;
    //    bulletCd.enabled = false;
    //    for (int i = 0; i < meshRdrs.Count; i++)
    //    {
    //        var meshRdr = meshRdrs[i];
    //        meshRdr.enabled = false;
    //    }
    //    CheckHitObject(collision.gameObject);
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("BodyParts"))
        {
            var charCtr = other.GetComponentInParent<CharacterController>();
            charCtr.OnHit(weapon.damage);
        }

        for (int i = 0; i < meshRdrs.Count; i++)
        {
            var meshRdr = meshRdrs[i];
            meshRdr.enabled = false;
        }
        bulletRb.velocity = Vector3.zero;
        bulletRb.constraints = RigidbodyConstraints.FreezeAll;
        bulletRb.isKinematic = true;
        bulletCd.enabled = false;
    }

    private void CheckHitObject(GameObject hitObject)
    {
        //if (hitObject.layer == LayerMask.NameToLayer("HitObject"))
        //{
        //    var target = hitObject.GetComponentInParent<Target>();
        //    if (target != null && hitObject.CompareTag("WeakPoint"))
        //    {
        //        target.OnHit(damage, transform.position, true);
        //    }
        //    else if (target != null)
        //    {
        //        target.OnHit(damage, transform.position, false);
        //    }
        //}
    }
}
