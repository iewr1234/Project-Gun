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
    private Rigidbody bulletRb;
    private Collider bulletCd;
    [SerializeField] private List<GameObject> meshs = new List<GameObject>();

    [Space(5f)][SerializeField] public ParticleSystem fx_tracerSmoke;
    public ParticleSystem[] fx_impactList;

    [Header("--- Assignment Variable---")]
    [SerializeField] private ImpactType impactType;
    [Tooltip("���")] public int propellant;
    [Tooltip("���ط�")] public int damage;
    [Tooltip("����")] public int penetrate;
    [Tooltip("�� �ջ�")] public int armorBreak;
    [Tooltip("����ȭ")] public int critical;

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
        hitCheck = false;
        resultCheck = false;

        timer = 0f;
        hitTime = DataUtility.GetDistance(transform.position, target.GetAimTarget()) / speed; // �ð� = �Ÿ� �� �ӷ�
        destroyTime = _meshType == 0 ? destroyTime_bullet : destroyTime_pellet;
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
            float dist = speed * destroyTime; // �Ÿ� = �ӷ� x �ð�
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

        bulletRb.velocity = Vector3.zero;
        bulletRb.constraints = RigidbodyConstraints.FreezeAll;
        bulletRb.isKinematic = true;
        bulletCd.enabled = false;
        for (int i = 0; i < meshs.Count; i++)
        {
            var mesh = meshs[i];
            mesh.SetActive(false);
        }

        fx_tracerSmoke.Stop();
        if (impactType != ImpactType.None) fx_impactList[(int)impactType].Play();
        hitCheck = true;
    }
}
