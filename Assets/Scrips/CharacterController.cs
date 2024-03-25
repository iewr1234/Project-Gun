using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public enum CharacterOwner
{
    None,
    Player,
    Enemy,
}

public enum CommandType
{
    None,
    Move,
    TakeCover,
    LeaveCover,
    CoverAim,
    BackCover,
    Shoot,
    Reload,
}

public class CharacterCommand
{
    public CommandType type;
    public List<FieldNode> passList;
    public CharacterController target;
}

public class CharacterController : MonoBehaviour
{
    [Header("---Access Script---")]
    [SerializeField] private GameManager gameMgr;
    public Weapon weapon;

    [HideInInspector] public FieldNode currentNode;
    [HideInInspector] public Cover cover;

    [Header("---Access Component---")]
    public Animator animator;

    private RigBuilder rigBdr;
    private Transform aimPoint;
    [HideInInspector] public Transform aimTarget;

    [HideInInspector] public Transform rightHandTf;
    [HideInInspector] public Transform leftHandTf;

    private List<Collider> ragdollCds = new List<Collider>();
    private List<Rigidbody> ragdollRbs = new List<Rigidbody>();

    [Header("--- Assignment Variable---")]
    public CharacterOwner ownerType;
    public int mobility;
    public int maxHealth;
    public int health;
    [Space(5f)]

    [SerializeField] private List<CharacterController> targetList = new List<CharacterController>();

    private LayerMask nodeLayer;
    private LayerMask coverLayer;

    private List<CharacterCommand> commandList = new List<CharacterCommand>();
    private bool moving;
    private bool reloading;
    private bool covering;
    private Vector3 coverPos;

    private readonly float moveSpeed = 7f;
    private readonly float closeDistance = 0.05f;
    private readonly float coverSpeed = 1f;
    private readonly float coverInterval = 0.2f;
    private readonly float coverAimSpeed = 3f;

    private readonly float aimPointY = 0.9f;
    private readonly float aimOffTime = 0.3f;

    /// <summary>
    /// 구성요소 설정
    /// </summary>
    /// <param name="_gameMgr"></param>
    /// <param name="_ownerType"></param>
    /// <param name="_currentNode"></param>
    public void SetComponents(GameManager _gameMgr, CharacterOwner _ownerType, FieldNode _currentNode)
    {
        gameMgr = _gameMgr;
        animator = GetComponent<Animator>();

        rigBdr = GetComponent<RigBuilder>();
        rigBdr.enabled = false;
        aimPoint = transform.Find("AimPoint");
        aimTarget = transform.Find("Root/Hips/Spine_01/Spine_02/Spine_03");

        rightHandTf = transform.Find("Root/Hips/Spine_01/Spine_02/Spine_03/Clavicle_R/Shoulder_R/Elbow_R/Hand_R");
        leftHandTf = transform.Find("Root/Hips/Spine_01/Spine_02/Spine_03/Clavicle_L/Shoulder_L/Elbow_L/Hand_L");

        ragdollCds = transform.Find("Root").GetComponentsInChildren<Collider>().ToList();
        for (int i = 0; i < ragdollCds.Count; i++)
        {
            var cd = ragdollCds[i];
            cd.gameObject.layer = LayerMask.NameToLayer("BodyParts");
            cd.isTrigger = true;
        }
        ragdollRbs = transform.Find("Root").GetComponentsInChildren<Rigidbody>().ToList();
        for (int i = 0; i < ragdollRbs.Count; i++)
        {
            var rb = ragdollRbs[i];
            rb.isKinematic = true;
        }

        ownerType = _ownerType;
        switch (ownerType)
        {
            case CharacterOwner.Player:
                gameMgr.playerList.Add(this);
                break;
            case CharacterOwner.Enemy:
                gameMgr.enemyList.Add(this);
                break;
            default:
                break;
        }
        health = maxHealth;
        nodeLayer = LayerMask.GetMask("Node");
        coverLayer = LayerMask.GetMask("Cover");

        currentNode = _currentNode;
        currentNode.charCtr = this;
        currentNode.canMove = false;
    }

    private void OnDrawGizmos()
    {
        DrawWeaponRange();
    }

    /// <summary>
    /// 무기 사거리 범위 표시
    /// </summary>
    private void DrawWeaponRange()
    {
        if (weapon == null) return;

        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.color = Color.red;
        var height = 1f;
        var segments = 30f;
        var angleStep = 360f / segments;
        var angle = 0 * angleStep * Mathf.Deg2Rad;
        var startPos = new Vector3(Mathf.Cos(angle) * weapon.range, height, Mathf.Sin(angle) * weapon.range);
        for (int i = 0; i <= segments; i++)
        {
            angle = i * angleStep * Mathf.Deg2Rad;
            var endPos = new Vector3(Mathf.Cos(angle) * weapon.range, height, Mathf.Sin(angle) * weapon.range);
            Gizmos.DrawLine(startPos, endPos);
            startPos = endPos;
        }
    }

    private void Update()
    {
        CommandApplication();
    }

    /// <summary>
    /// 커맨드리스트의 커맨드를 실행
    /// </summary>
    private void CommandApplication()
    {
        if (commandList.Count == 0) return;

        var command = commandList[0];
        switch (command.type)
        {
            case CommandType.Move:
                MoveProcess(command);
                break;
            case CommandType.TakeCover:
                TakeCoverProcess(command);
                break;
            case CommandType.LeaveCover:
                LeaveCoverProcess(command);
                break;
            case CommandType.CoverAim:
                CoverAimProcess(command);
                break;
            case CommandType.BackCover:
                BackCoverProcess(command);
                break;
            case CommandType.Shoot:
                ShootProcess(command);
                break;
            case CommandType.Reload:
                ReloadPrecess(command);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 캐릭터 이동 처리
    /// </summary>
    /// <param name="command"></param>
    private void MoveProcess(CharacterCommand command)
    {
        var targetNode = command.passList[^1];
        if (!animator.GetBool("isMove") && targetNode == currentNode)
        {
            command.passList.Remove(targetNode);
            if (command.passList.Count == 0)
            {
                commandList.RemoveAt(0);
            }
        }
        else
        {
            if (!animator.GetBool("isMove"))
            {
                animator.SetBool("isAim", false);
                animator.SetBool("isMove", true);
                currentNode.canMove = true;
                currentNode.charCtr = null;
                currentNode = command.passList[0];
                currentNode.canMove = false;
                currentNode.charCtr = this;
            }

            var canLook = animator.GetCurrentAnimatorStateInfo(0).IsTag("Idle") || animator.GetCurrentAnimatorStateInfo(0).IsTag("Move");
            if (canLook && !moving)
            {
                transform.LookAt(targetNode.transform);
                moving = true;
            }
            var direction = Vector3.Normalize(targetNode.transform.position - transform.position);
            var distance = DataUtility.GetDistance(transform.position, targetNode.transform.position);
            if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Move") && distance > closeDistance)
            {
                transform.position += direction * (moveSpeed * Time.deltaTime);
            }
            else if (distance <= closeDistance)
            {
                transform.position = targetNode.transform.position;
                moving = false;
                command.passList.Remove(targetNode);
                if (command.passList.Count == 0)
                {
                    animator.SetBool("isMove", false);
                    commandList.Remove(command);
                }
            }
        }
    }

    /// <summary>
    /// 캐릭터 엄폐실행 처리
    /// </summary>
    /// <param name="command"></param>
    private void TakeCoverProcess(CharacterCommand command)
    {
        if (!covering)
        {
            FieldNode coverNode = null;
            var rotY = transform.rotation.y;
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, DataUtility.nodeSize, coverLayer))
            {
                var cover = hit.collider.GetComponentInParent<Cover>();
                var find = currentNode.onAxisNodes.Find(x => x == cover.node);
                if (find != null)
                {
                    coverNode = cover.node;
                }
                else
                {
                    var node = currentNode.onAxisNodes.Find(x => x.cover != null);
                    if (node != null)
                    {
                        coverNode = node;
                    }
                }
            }
            else
            {
                var node = currentNode.onAxisNodes.Find(x => x.cover != null);
                if (node != null)
                {
                    coverNode = node;
                }
            }

            if (coverNode != null)
            {
                FindDirectionForCover(coverNode);
            }
            else
            {
                commandList.Remove(command);
            }
        }
        else
        {
            MoveCoverPosition(command);
        }
    }

    /// <summary>
    /// 엄폐 후 캐릭터 방향 찾기
    /// </summary>
    /// <param name="coverNode"></param>
    private void FindDirectionForCover(FieldNode coverNode)
    {
        transform.LookAt(coverNode.transform);
        RaycastHit hit;
        var rightHit = false;
        var leftHit = false;
        if (Physics.Raycast(transform.position, transform.right, out hit, DataUtility.nodeSize, nodeLayer))
        {
            rightHit = true;
        }
        if (Physics.Raycast(transform.position, -transform.right, out hit, DataUtility.nodeSize, nodeLayer))
        {
            leftHit = true;
        }

        if (rightHit || leftHit)
        {
            var rightCover = false;
            var leftCover = false;
            var pos = transform.position + (transform.right * DataUtility.nodeSize);
            if (Physics.Raycast(pos, transform.forward, out hit, DataUtility.nodeSize, coverLayer))
            {
                rightCover = true;
            }
            pos = transform.position + (-transform.right * DataUtility.nodeSize);
            if (Physics.Raycast(pos, transform.forward, out hit, DataUtility.nodeSize, coverLayer))
            {
                leftCover = true;
            }

            if (rightCover && leftCover)
            {
                animator.SetBool("isRight", rightHit);
            }
            else if (rightCover && leftHit)
            {
                animator.SetBool("isRight", false);
            }
            else if (leftCover && rightHit)
            {
                animator.SetBool("isRight", true);
            }
            else
            {
                animator.SetBool("isRight", rightHit);
            }
            Debug.Log($"{transform.name}: RN={rightHit}, LN={leftHit}, RC={rightCover}, LC={leftCover}");
        }
        else
        {
            Debug.LogError("not Found Node");
            return;
        }

        cover = coverNode.cover;
        coverPos = transform.position + (transform.forward * coverInterval);
        covering = true;
        animator.SetBool("isCover", true);
    }

    /// <summary>
    /// 캐릭터 엄폐해제 처리
    /// </summary>
    /// <param name="command"></param>
    private void LeaveCoverProcess(CharacterCommand command)
    {
        if (!covering)
        {
            cover = null;
            coverPos = currentNode.transform.position;
            covering = true;
            animator.SetBool("isCover", false);
        }
        else
        {
            MoveCoverPosition(command);
        }
    }

    /// <summary>
    /// 엄폐중 캐릭터 이동 적용
    /// </summary>
    /// <param name="command"></param>
    private void MoveCoverPosition(CharacterCommand command)
    {
        if (transform.position != coverPos)
        {
            transform.position = Vector3.MoveTowards(transform.position, coverPos, coverSpeed * Time.deltaTime);
        }
        else
        {
            covering = false;
            commandList.Remove(command);
        }
    }

    /// <summary>
    /// 캐릭터 엄폐사격 처리
    /// </summary>
    /// <param name="command"></param>
    private void CoverAimProcess(CharacterCommand command)
    {
        if (!covering && !animator.GetBool("isAim"))
        {
            rigBdr.enabled = true;
            FindCoverAimNode();
            animator.SetBool("isAim", true);
        }
        else
        {
            var targetPos = command.target.transform.position;
            aimPoint.transform.position = new Vector3(targetPos.x, targetPos.y + aimPointY, targetPos.z);
            if (transform.position != coverPos)
            {
                transform.position = Vector3.MoveTowards(transform.position, coverPos, coverAimSpeed * Time.deltaTime);
            }
            else
            {
                weapon.firstShot = true;
                animator.SetInteger("shootNum", weapon.bulletsPerShot);
                coverPos = currentNode.transform.position + (transform.forward * coverInterval);
                commandList.Remove(command);
                covering = false;
            }
        }
    }

    /// <summary>
    /// 엄폐사격을 위해 이동할 노드를 찾음
    /// </summary>
    private void FindCoverAimNode()
    {
        Vector3 dir;
        if (animator.GetBool("isRight"))
        {
            dir = transform.right;
        }
        else
        {
            dir = -transform.right;
        }

        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, DataUtility.nodeSize, nodeLayer))
        {
            var node = hit.collider.GetComponentInParent<FieldNode>();
            Debug.Log($"CoverAim: {currentNode.name} => {node.name}");
            coverPos = node.transform.position;
            covering = true;
        }
        else
        {
            //Debug.LogError("not Found Node");
            //commandList.Clear();
        }
    }

    /// <summary>
    /// 엄폐사격 후 재엄폐 처리
    /// </summary>
    /// <param name="command"></param>
    private void BackCoverProcess(CharacterCommand command)
    {
        if (transform.position != coverPos)
        {
            transform.position = Vector3.MoveTowards(transform.position, coverPos, coverAimSpeed * Time.deltaTime);
        }
        else
        {
            commandList.Remove(command);
        }
    }

    /// <summary>
    /// 캐릭터 사격 처리
    /// </summary>
    /// <param name="command"></param>
    private void ShootProcess(CharacterCommand command)
    {
        if (!animator.GetBool("isAim"))
        {
            weapon.firstShot = true;
            transform.LookAt(command.target.transform);
            rigBdr.enabled = true;
            var targetPos = command.target.transform.position;
            aimPoint.transform.position = new Vector3(targetPos.x, targetPos.y + aimPointY, targetPos.z);
            animator.SetBool("isAim", true);
            animator.SetInteger("shootNum", weapon.bulletsPerShot);
        }

        var shootNum = animator.GetInteger("shootNum");
        if (shootNum == 0) return;

        if (animator.GetCurrentAnimatorStateInfo(1).IsTag("Aim"))
        {
            weapon.FireBullet(command.target);
            shootNum--;
            animator.SetInteger("shootNum", shootNum);
            if (shootNum == 0)
            {
                StartCoroutine(Coroutine_AimOff(command));
            }
        }
    }

    /// <summary>
    /// 캐릭터 재장전 처리
    /// </summary>
    /// <param name="command"></param>
    private void ReloadPrecess(CharacterCommand command)
    {
        if (!reloading && animator.GetCurrentAnimatorStateInfo(1).IsTag("None"))
        {
            animator.SetTrigger("reload");
            weapon.Reload();
            reloading = true;
        }
    }

    /// <summary>
    /// 커맨드 추가
    /// </summary>
    /// <param name="type"></param>
    /// <param name="passList"></param>
    public void AddCommand(CommandType type, List<FieldNode> passList)
    {
        switch (type)
        {
            case CommandType.Move:
                var moveCommand = new CharacterCommand
                {
                    type = CommandType.Move,
                    passList = new List<FieldNode>(passList),
                };
                commandList.Add(moveCommand);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 커맨드 추가
    /// </summary>
    /// <param name="type"></param>
    /// <param name="target"></param>
    public void AddCommand(CommandType type, CharacterController target)
    {
        switch (type)
        {
            case CommandType.CoverAim:
                var coverAimCommand = new CharacterCommand
                {
                    type = CommandType.CoverAim,
                    target = target,
                };
                commandList.Add(coverAimCommand);
                break;
            case CommandType.Shoot:
                var shootCommand = new CharacterCommand
                {
                    type = CommandType.Shoot,
                    target = target,
                };
                commandList.Add(shootCommand);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 커맨드 추가
    /// </summary>
    /// <param name="type"></param>
    public void AddCommand(CommandType type)
    {
        switch (type)
        {
            default:
                var command = new CharacterCommand
                {
                    type = type,
                };
                commandList.Add(command);
                break;
        }
    }

    /// <summary>
    /// 사격 가능한 타겟 찾음
    /// </summary>
    public void FindTargets()
    {

    }

    /// <summary>
    /// 캐릭터 피격
    /// </summary>
    public void OnHit(int damage)
    {
        health -= damage;
        if (health < 0)
        {
            health = 0;
        }

        if (health == 0)
        {

        }
    }

    #region Coroutine
    /// <summary>
    /// (코루틴)조준해제
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    private IEnumerator Coroutine_AimOff(CharacterCommand command)
    {
        yield return new WaitForSeconds(aimOffTime);

        rigBdr.enabled = false;
        commandList.Remove(command);
        animator.SetBool("isAim", false);
    }
    #endregion

    #region Animation Event
    /// <summary>
    /// (애니메이션 이벤트)무기 위치 변경
    /// </summary>
    /// <param name="switchPos"></param>
    public void Event_WeaponSwitching(string switchPos)
    {
        weapon.WeaponSwitching(switchPos);
    }

    /// <summary>
    /// (애니메이션 이벤트)재장전 완료
    /// </summary>
    public void Event_ReloadEnd()
    {
        if (commandList.Count == 0)
        {
            Debug.LogError("No Command in the CommanderList");
            return;
        }
        else if (commandList[0].type != CommandType.Reload)
        {
            Debug.LogError("CommandType is not Reload");
            return;
        }

        commandList.RemoveAt(0);
        reloading = false;
    }
    #endregion

    public GameManager GameMgr
    {
        private set { gameMgr = value; }
        get { return gameMgr; }
    }
}
