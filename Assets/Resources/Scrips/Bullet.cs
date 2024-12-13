using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

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

    private LayerMask targetLayer;
    [Space(5f)][SerializeField] private bool isHit;
    [SerializeField] private float speed = 30f;
    [SerializeField] private int hitNum;

    private bool isCheck;
    private Vector3 targetPos;

    private float timer;
    private float hitTime;
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
        meshs[_meshType].SetActive(true);

        propellant = shooter.propellant;
        damage = shooter.damage;
        penetrate = shooter.penetrate;
        armorBreak = shooter.armorBreak;
        critical = shooter.critical;

        isHit = _isHit;
        targetLayer = isHit ? LayerMask.GetMask("Node") | LayerMask.GetMask("BodyParts") : LayerMask.GetMask("Node") | LayerMask.GetMask("Cover");
        isCheck = false;
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, speed * destroyTime, targetLayer))
        {
            targetPos = hit.point;
        }
        else
        {
            float dist = speed * destroyTime; // 거리 = 속력 x 시간
            targetPos = transform.position + (transform.forward * dist);
        }

        timer = 0f;
        hitTime = DataUtility.GetDistance(transform.position, target.GetAimTarget()) / speed; // 시간 = 거리 ÷ 속력
        destroyTime = _meshType == 0 ? destroyTime_bullet : destroyTime_pellet;
    }

    private void Update()
    {
        HitProcess();
        DelayDestroy();
    }

    private void HitProcess()
    {
        if (transform.position != targetPos)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
        }
        else
        {
            HitBullet();
        }

        timer += Time.deltaTime;
        if (!isCheck && timer > hitTime)
        {
            if (isHit)
            {
                target.OnHit(shooter, this);
            }
            else
            {
                target.GameMgr.SetFloatText(target.charUI, "Miss", Color.red);
            }
            isCheck = true;
        }
    }

    private void DelayDestroy()
    {
        if (timer > destroyTime)
        {
            trail.Clear();
            trail.enabled = false;
            gameObject.SetActive(false);
        }
    }

    private void HitBullet()
    {
        //if (isCheck) return;

        bulletRb.velocity = Vector3.zero;
        bulletRb.constraints = RigidbodyConstraints.FreezeAll;
        bulletRb.isKinematic = true;
        bulletCd.enabled = false;
        for (int i = 0; i < meshs.Count; i++)
        {
            var mesh = meshs[i];
            mesh.SetActive(false);
        }
        //isCheck = true;

        //if (!isHit && isMiss)
        //{
        //    target.GameMgr.SetFloatText(target.charUI.transform.position, "Miss", Color.red);
        //    isMiss = false;
        //}
    }
}
