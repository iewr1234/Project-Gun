using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("---Access Script---")]
    [SerializeField] private Weapon weapon;
    private CharacterController target;

    [Header("---Access Component---")]
    private TrailRenderer trail;
    private Rigidbody bulletRb;
    private Collider bulletCd;
    private List<MeshRenderer> meshRdrs = new List<MeshRenderer>();

    [Header("--- Assignment Variable---")]
    [SerializeField] private float speed = 30f;

    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private bool isHit;
    private bool isCheck;
    private float timer;

    private readonly float startWidth = 0.01f;
    private readonly float destroyTime = 1f;

    public void SetComponents(Weapon _weapon, CharacterController _target, bool _isHit)
    {
        weapon = _weapon;
        target = _target;

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
        bulletRb.velocity = transform.forward * speed;

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

        isHit = _isHit;
        if (isHit)
        {
            targetLayer = LayerMask.GetMask("Node") | LayerMask.GetMask("BodyParts");
        }
        else
        {
            targetLayer = LayerMask.GetMask("Node") | LayerMask.GetMask("Cover");
        }
        isCheck = false;
    }

    void FixedUpdate()
    {
        if (isCheck) return;

        var hits = Physics.SphereCastAll(transform.position, 0.1f, transform.forward, 0f, targetLayer);
        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            var charCtr = hit.collider.GetComponentInParent<CharacterController>();
            if (charCtr != null && charCtr == target && isHit)
            {
                charCtr.OnHit(transform.forward, weapon);
                isHit = false;
                Debug.Log($"{charCtr.name}: Hit");
            }
            HitBullet();
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

    private void HitBullet()
    {
        if (isCheck) return;

        for (int j = 0; j < meshRdrs.Count; j++)
        {
            var meshRdr = meshRdrs[j];
            meshRdr.enabled = false;
        }
        bulletRb.velocity = Vector3.zero;
        bulletRb.constraints = RigidbodyConstraints.FreezeAll;
        bulletRb.isKinematic = true;
        bulletCd.enabled = false;
        isCheck = true;
    }
}
