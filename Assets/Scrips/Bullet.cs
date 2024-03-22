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
    private List<MeshRenderer> meshRenderers;

    [Header("--- Assignment Variable---")]
    public float speed = 150f;
    public float damage;

    private float timer;

    private readonly float startWidth = 0.01f;
    private readonly float destroyTime = 1f;

    public void SetComponents(Weapon _weapon)
    {
        weapon = _weapon;

        trail = GetComponent<TrailRenderer>();
        bulletRb = GetComponent<Rigidbody>();
        bulletCd = GetComponent<Collider>();
        meshRenderers = GetComponentsInChildren<MeshRenderer>().ToList();

        trail.enabled = true;
        trail.startWidth = startWidth;
        trail.endWidth = 0f;
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

    private void OnCollisionEnter(Collision collision)
    {
        for (int i = 0; i < meshRenderers.Count; i++)
        {
            meshRenderers[i].enabled = false;
        }
        bulletRb.velocity = Vector3.zero;
        bulletRb.constraints = RigidbodyConstraints.FreezeAll;
        bulletRb.isKinematic = true;
        CheckHitObject(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        for (int i = 0; i < meshRenderers.Count; i++)
        {
            meshRenderers[i].enabled = false;
        }
        bulletRb.velocity = Vector3.zero;
        bulletRb.constraints = RigidbodyConstraints.FreezeAll;
        bulletRb.isKinematic = true;
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
