using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Bullet : MonoBehaviour
{
    [Header("---Access Script---")]
    [SerializeField] private Weapon weapon;

    [Header("---Access Component---")]
    public Rigidbody rigidBody;
    public Collider bulletCdr;
    public TrailRenderer trail;
    public List<MeshRenderer> meshRenderers;

    [Header("--- Assignment Variable---")]
    public float speed = 150f;
    public float damage;

    private float timer;

    private readonly float startWidth = 0.01f;
    private readonly float destroyTime = 1f;

    private void Start()
    {
        trail.startWidth = startWidth;
        trail.endWidth = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > destroyTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        for (int i = 0; i < meshRenderers.Count; i++)
        {
            meshRenderers[i].enabled = false;
        }
        rigidBody.velocity = Vector3.zero;
        rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        rigidBody.isKinematic = true;
        CheckHitObject(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        for (int i = 0; i < meshRenderers.Count; i++)
        {
            meshRenderers[i].enabled = false;
        }
        rigidBody.velocity = Vector3.zero;
        rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        rigidBody.isKinematic = true;
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
