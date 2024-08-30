using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("---Access Script---")]
    private CharacterController shooter;
    private CharacterController target;

    [Header("---Access Component---")]
    private TrailRenderer trail;
    private Rigidbody bulletRb;
    private Collider bulletCd;
    private List<MeshRenderer> meshRdrs = new List<MeshRenderer>();

    [Header("--- Assignment Variable---")]
    //[SerializeField] private BulletDataInfo bulletData;
    [SerializeField] private float speed = 30f;
    [Tooltip("장약")] public int propellant;
    [Tooltip("피해량")] public int damage;
    [Tooltip("관통")] public int penetrate;
    [Tooltip("방어구 손상")] public int armorBreak;
    [Tooltip("파편화")] public int critical;

    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private bool isHit;
    [SerializeField] private bool isMiss;
    private bool isCheck;
    private float timer;

    private readonly float startWidth = 0.01f;
    private readonly float destroyTime = 1f;

    public void SetComponents(CharacterController _shooter, CharacterController _target, bool _isHit)
    {
        shooter = _shooter;
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

        propellant = shooter.propellant;
        damage = shooter.damage;
        penetrate = shooter.penetrate;
        armorBreak = shooter.armorBreak;
        critical = shooter.critical;

        isHit = _isHit;
        if (isHit)
        {
            targetLayer = LayerMask.GetMask("Node") | LayerMask.GetMask("BodyParts");
            isMiss = false;
        }
        else
        {
            targetLayer = LayerMask.GetMask("Node") | LayerMask.GetMask("Cover");
            isMiss = true;
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
                charCtr.OnHit(transform.forward, this);
                isHit = false;
                Debug.Log($"{charCtr.name}: Hit");
            }
            HitBullet();
        }

        if (!isHit && isMiss)
        {
            var dist = DataUtility.GetDistance(target.transform.position, transform.position);
            if (dist < 1.5f)
            {
                target.GameMgr.SetFloatText(target.transform.position + new Vector3(0f, 2f, 0f), "Miss", Color.red);
                isMiss = false;
            }
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

        if (!isHit)
        {
            target.GameMgr.SetFloatText(target.charUI.transform.position, "Miss", Color.red);
        }
    }
}
