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
    [Tooltip("���")] public int propellant;
    [Tooltip("���ط�")] public int damage;
    [Tooltip("����")] public int penetrate;
    [Tooltip("�� �ջ�")] public int armorBreak;
    [Tooltip("����ȭ")] public int critical;
    [Space(5f)]

    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private float speed = 30f;
    [SerializeField] private float radius = 0.03f;
    [SerializeField] private bool isHit;
    [SerializeField] private int hitNum;
    private bool isMiss;
    private bool isCheck;
    private float timer;
    private float destroyTime;
    private Vector3 lastPosition;

    private readonly float startWidth = 0.01f;
    private readonly float destroyTime_bullet = 1f;
    private readonly float destroyTime_pellet = 0.3f;
    private const int MAX_HITS = 10; // �ִ� �浹 ���� �� ����

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

    public void SetBullet(CharacterController _shooter, CharacterController _target, int _meshType, Vector3 aimPos, bool _isHit, bool _isMiss, int _hitNum)
    {
        shooter = _shooter;
        target = _target;

        trail.enabled = true;
        bulletCd.enabled = true;
        bulletRb.constraints = RigidbodyConstraints.None;
        bulletRb.isKinematic = false;
        //bulletRb.velocity = transform.forward * speed;
        meshs[_meshType].SetActive(true);

        propellant = shooter.propellant;
        damage = shooter.damage;
        penetrate = shooter.penetrate;
        armorBreak = shooter.armorBreak;
        critical = shooter.critical;

        isHit = _isHit;
        isMiss = _isMiss;
        if (isHit)
        {
            targetLayer = LayerMask.GetMask("Node") | LayerMask.GetMask("BodyParts");
            hitNum = _hitNum;
        }
        else
        {
            targetLayer = LayerMask.GetMask("Node") | LayerMask.GetMask("Cover");
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
        lastPosition = transform.position;
    }

    private void Update()
    {
        HitProcess();
        DelayDestroy();
    }

    private void HitProcess()
    {
        if (isCheck) return;

        Vector3 currentPosition = transform.position;
        if (!CheckCollision(lastPosition, currentPosition))
        {
            // �Ѿ� �̵�
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            lastPosition = currentPosition;
        }
    }

    private bool CheckCollision(Vector3 fromPos, Vector3 toPos)
    {
        float moveDistance = Vector3.Distance(fromPos, toPos);
        if (moveDistance < Mathf.Epsilon) return false;

        Vector3 moveDirection = (toPos - fromPos).normalized;
        RaycastHit[] hitBuffer = new RaycastHit[MAX_HITS];
        int hitCount = Physics.SphereCastNonAlloc(
            fromPos,
            radius,
            moveDirection,
            hitBuffer,
            moveDistance,
            targetLayer
        );

        // ���� ����� �浹 ã��
        float closestDistance = float.MaxValue;
        RaycastHit? closestHit = null;

        for (int i = 0; i < hitCount; i++)
        {
            float distance = hitBuffer[i].distance;
            if (distance < closestDistance)
            {
                var charCtr = hitBuffer[i].collider.GetComponentInParent<CharacterController>();

                // CharacterController�� �ִ� ���
                if (charCtr != null)
                {
                    // Ư�� Ÿ���� �����Ǿ� �ְ�, �� Ÿ���� �ƴ� ��� ��ŵ
                    if (target != null && charCtr != target) continue;

                    closestDistance = distance;
                    closestHit = hitBuffer[i];
                }
                // CharacterController�� ���� ��쵵 �浹 ó��
                else
                {
                    closestDistance = distance;
                    closestHit = hitBuffer[i];
                }
            }
        }

        // �浹 ó��
        if (closestHit.HasValue)
        {
            var hit = closestHit.Value;
            var charCtr = hit.collider.GetComponentInParent<CharacterController>();

            // CharacterController�� �ִ� ���
            if (charCtr != null && charCtr == target && isHit)
            {
                charCtr.OnHit(shooter, moveDirection, this, hitNum);
                //Debug.Log($"{charCtr.name}: Character Hit");
            }
            // �Ϲ� ������Ʈ�� ���
            else
            {
                //HandleNormalCollision(hit, moveDirection);
                //Debug.Log($"{hit.collider.name}: Object Hit");
            }

            isHit = false;
            HitBullet();
        }

        return isCheck;
    }

    private void DelayDestroy()
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

        if (!isHit && isMiss)
        {
            target.GameMgr.SetFloatText(target.charUI.transform.position, "Miss", Color.red);
            isMiss = false;
        }
    }
}
