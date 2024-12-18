using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private enum ImpactType
    {
        None = -1,
        Body,
        Stone,
    }

    [Header("---Access Script---")]
    private CharacterController shooter;
    private CharacterController target;

    [Header("---Access Component---")]
    public TrailRenderer trail;

    [Space(5f)][SerializeField] public ParticleSystem fx_tracerSmoke;
    public ParticleSystem[] fx_impactList;

    [Header("--- Assignment Variable---")]
    [SerializeField] private ImpactType impactType;
    [Tooltip("장약")] public int propellant;
    [Tooltip("피해량")] public int damage;
    [Tooltip("관통")] public int penetrate;
    [Tooltip("방어구 손상")] public int armorBreak;
    [Tooltip("파편화")] public int critical;

    [Space(5f)][SerializeField] private LayerMask targetLayer;
    [SerializeField] private bool isHit;
    [SerializeField] private float speed = 30f;
    [SerializeField] private int hitNum;

    private bool hitCheck;
    private bool resultCheck;
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

        gameObject.SetActive(false);
    }

    public void SetBullet(CharacterController _shooter, CharacterController _target, bool _isHit, bool _isPellet)
    {
        shooter = _shooter;
        target = _target;

        trail.enabled = true;

        propellant = shooter.propellant;
        damage = shooter.damage;
        penetrate = shooter.penetrate;
        armorBreak = shooter.armorBreak;
        critical = shooter.critical;

        isHit = _isHit;
        targetLayer = isHit ? LayerMask.GetMask("Node") | LayerMask.GetMask("BodyParts") : LayerMask.GetMask("Node") | LayerMask.GetMask("Cover");
        hitCheck = false;
        resultCheck = false;

        timer = 0f;
        hitTime = DataUtility.GetDistance(transform.position, target.GetAimTarget()) / speed; // 시간 = 거리 ÷ 속력
        destroyTime = _isPellet ? destroyTime_pellet : destroyTime_bullet;
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, speed * destroyTime, targetLayer))
        {
            targetPos = hit.point;
            switch (LayerMask.LayerToName(hit.collider.gameObject.layer))
            {
                case "BodyParts":
                    impactType = ImpactType.Body;
                    break;
                case "Cover":
                    impactType = ImpactType.Stone;
                    break;
                default:
                    impactType = ImpactType.None;
                    break;
            }
        }
        else
        {
            float dist = speed * destroyTime; // 거리 = 속력 x 시간
            targetPos = transform.position + (transform.forward * dist);
            impactType = ImpactType.None;
        }
        fx_tracerSmoke.Play();
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
        if (!resultCheck && timer > hitTime)
        {
            if (isHit)
            {
                target.OnHit(shooter, this);
            }
            else
            {
                target.GameMgr.SetFloatText(target.charUI, "Miss", Color.red);
            }
            resultCheck = true;
        }
    }

    private void DelayDestroy()
    {
        if (timer > destroyTime)
        {
            trail.Clear();
            trail.enabled = false;
            fx_tracerSmoke.Stop();
            gameObject.SetActive(false);
        }
    }

    private void HitBullet()
    {
        if (hitCheck) return;

        fx_tracerSmoke.Stop();
        if (impactType != ImpactType.None) fx_impactList[(int)impactType].Play();
        hitCheck = true;
    }
}
