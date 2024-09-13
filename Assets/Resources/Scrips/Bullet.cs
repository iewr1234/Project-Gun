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
    [SerializeField] private List<GameObject> meshs = new List<GameObject>();

    [Header("--- Assignment Variable---")]
    [Tooltip("장약")] public int propellant;
    [Tooltip("피해량")] public int damage;
    [Tooltip("관통")] public int penetrate;
    [Tooltip("방어구 손상")] public int armorBreak;
    [Tooltip("파편화")] public int critical;
    [Space(5f)]

    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private float speed = 30f;
    [SerializeField] private bool isHit;
    [SerializeField] private bool isMiss;
    private bool isCheck;
    private float timer;
    private float destroyTime;

    private readonly float startWidth = 0.01f;
    private readonly float destroyTime_bullet = 1f;
    private readonly float destroyTime_pellet = 0.3f;

    public void SetComponents()
    {
        trail = GetComponent<TrailRenderer>();
        trail.startWidth = startWidth;
        trail.endWidth = 0f;
        trail.enabled = false;

        bulletRb = GetComponent<Rigidbody>();
        bulletRb.constraints = RigidbodyConstraints.FreezeAll;
        bulletRb.isKinematic = true;

        bulletCd = GetComponent<Collider>();
        bulletCd.enabled = false;

        meshs.Add(transform.Find("Mesh1").gameObject);
        meshs.Add(transform.Find("Mesh2").gameObject);
        for (int i = 0; i < meshs.Count; i++)
        {
            var mesh = meshs[i];
            mesh.SetActive(false);
        }

        gameObject.SetActive(false);
    }

    public void SetBullet(CharacterController _shooter, CharacterController _target, int _meshType, bool _isHit)
    {
        shooter = _shooter;
        target = _target;

        trail.enabled = true;
        bulletCd.enabled = true;
        bulletRb.constraints = RigidbodyConstraints.None;
        bulletRb.isKinematic = false;
        bulletRb.velocity = transform.forward * speed;
        meshs[_meshType].SetActive(true);

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

        if (_meshType == 0)
        {
            destroyTime = destroyTime_bullet;
        }
        else
        {
            destroyTime = destroyTime_pellet;
        }
        timer = 0f;
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
            trail.Clear();
            trail.enabled = false;
            gameObject.SetActive(false);
        }
    }

    private void HitBullet()
    {
        if (isCheck) return;

        bulletRb.velocity = Vector3.zero;
        bulletRb.constraints = RigidbodyConstraints.FreezeAll;
        bulletRb.isKinematic = true;
        bulletCd.enabled = false;
        for (int i = 0; i < meshs.Count; i++)
        {
            var mesh = meshs[i];
            mesh.SetActive(false);
        }
        isCheck = true;

        if (!isHit)
        {
            target.GameMgr.SetFloatText(target.charUI.transform.position, "Miss", Color.red);
        }
    }
}
