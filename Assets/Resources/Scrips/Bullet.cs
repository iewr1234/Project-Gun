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

    [SerializeField] private LayerMask targetLayer;
    private bool isHit;
    private float timer;

    private readonly float startWidth = 0.01f;
    private readonly float destroyTime = 1f;

    public void SetComponents(Weapon _weapon, bool _isHit)
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

        isHit = !_isHit;
        if (!isHit)
        {
            targetLayer = LayerMask.GetMask("Node") | LayerMask.GetMask("BodyParts") | LayerMask.GetMask("Cover");
        }
        else
        {
            targetLayer = LayerMask.GetMask("Node") | LayerMask.GetMask("BodyParts");
        }
    }

    void FixedUpdate()
    {
        if (isHit) return;

        var hitCds = Physics.OverlapSphere(transform.position, 0.1f, targetLayer).ToList();
        for (int i = 0; i < hitCds.Count; i++)
        {
            var hitCd = hitCds[i];
            var charCtr = hitCd.GetComponentInParent<CharacterController>();
            if (charCtr != null)
            {
                charCtr.OnHit(transform.forward, weapon);
            }
            HitBullet();
            break;
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
        for (int j = 0; j < meshRdrs.Count; j++)
        {
            var meshRdr = meshRdrs[j];
            meshRdr.enabled = false;
        }
        bulletRb.velocity = Vector3.zero;
        bulletRb.constraints = RigidbodyConstraints.FreezeAll;
        bulletRb.isKinematic = true;
        bulletCd.enabled = false;
        isHit = true;
    }
}
