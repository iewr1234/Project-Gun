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
    Wait,
    Move,
    TakeCover,
    LeaveCover,
    CoverAim,
    BackCover,
    Shoot,
    Reload,
}

public enum TargetDirection
{
    Front,
    Right,
    Left,
}

public class CharacterCommand
{
    public CommandType type;
    public float time;
    public List<FieldNode> passList;
    public FieldNode cover;
    public Transform lookAt;
    public TargetInfo targetInfo;
}

[System.Serializable]
public struct TargetInfo
{
    public CharacterController target;
    public FieldNode shooterNode;
    public FieldNode shooterCover;
    public FieldNode targetNode;
    public FieldNode targetCover;
    public bool isRight;
    public bool targetRight;
}

public class CharacterController : MonoBehaviour
{
    [Header("---Access Script---")]
    [SerializeField] private GameManager gameMgr;
    public Weapon weapon;

    [Header("---Access Component---")]
    public Animator animator;

    private RigBuilder rigBdr;
    public Transform aimPoint;
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

    public FieldNode currentNode;
    [HideInInspector] public Cover cover;
    [HideInInspector] public List<TargetInfo> targetList = new List<TargetInfo>();
    [HideInInspector] public int targetIndex;

    private LayerMask nodeLayer;
    private LayerMask coverLayer;

    private List<CharacterCommand> commandList = new List<CharacterCommand>();

    private float timer;

    private bool moving;
    private readonly float moveSpeed = 7f;
    private readonly float closeDistance = 0.05f;

    private bool covering;
    private Vector3 coverPos;
    private readonly float coverSpeed = 1f;
    private readonly float coverInterval = 0.2f;
    private readonly float coverAimSpeed = 3f;

    private Transform aimTf;
    private Vector3 aimInterval;
    private bool endAim;
    private readonly float aimSpeed = 25f;
    private readonly float aimOffTime = 0.4f;

    private bool targetingMove;
    private Vector3 targetingPos;
    private readonly float targetingMoveSpeed = 1f;

    private bool reloading;

    /// <summary>
    /// 구성요소 설정
    /// </summary>
    /// <param name="_gameMgr"></param>
    /// <param name="_ownerType"></param>
    /// <param name="_currentNode"></param>
    public void SetComponents(GameManager _gameMgr, CharacterOwner _ownerType, FieldNode _currentNode, Transform aimPointTf)
    {
        gameMgr = _gameMgr;
        animator = GetComponent<Animator>();

        rigBdr = GetComponent<RigBuilder>();
        rigBdr.enabled = false;
        aimPoint = transform.Find("AimPoint");
        aimPoint.name = $"AimPoint_{transform.name}";
        aimPoint.SetParent(aimPointTf);
        aimPoint.gameObject.SetActive(false);
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
        var charList = ownerType == CharacterOwner.Player ? gameMgr.playerList : gameMgr.enemyList;
        var name = transform.name.Split(' ', '(', ')')[0];
        transform.name = $"{name}_{charList.Count}";
        charList.Add(this);
        DataUtility.SetMeshsMaterial(ownerType, transform.GetComponentsInChildren<MeshRenderer>().ToList());
        DataUtility.SetMeshsMaterial(ownerType, transform.GetComponentsInChildren<SkinnedMeshRenderer>().ToList());

        health = maxHealth;
        nodeLayer = LayerMask.GetMask("Node");
        coverLayer = LayerMask.GetMask("Cover");

        currentNode = _currentNode;
        currentNode.charCtr = this;
        currentNode.canMove = false;
    }

    private void OnDrawGizmos()
    {
        DrawShootingPath();
        DrawWeaponRange();
    }

    /// <summary>
    /// 사격경로 표시
    /// </summary>
    private void DrawShootingPath()
    {
        if (targetList.Count == 0) return;

        Gizmos.color = Color.red;
        var height = 1f;
        var pos = new Vector3(currentNode.transform.position.x, height, currentNode.transform.position.z);
        for (int i = 0; i < targetList.Count; i++)
        {
            var target = targetList[i].target;
            var targetPos = new Vector3(target.currentNode.transform.position.x, height, target.currentNode.transform.position.z);
            Gizmos.DrawLine(pos, targetPos);
        }
    }

    /// <summary>
    /// 무기 사거리 범위 표시
    /// </summary>
    private void DrawWeaponRange()
    {
        if (weapon == null) return;

        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.color = Color.yellow;
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
        SetAimPosition();
        MoveTargetingPosition();
        CommandApplication();
    }

    private void SetAimPosition()
    {
        var canMove = animator.GetCurrentAnimatorStateInfo(0).IsTag("Aim")
                   && !animator.GetCurrentAnimatorStateInfo(1).IsTag("Hit")
                   && aimPoint.gameObject.activeSelf;
        if (!canMove) return;

        var aimPos = aimTf.position + aimInterval;
        if (aimPoint.position != aimPos)
        {
            aimPoint.position = Vector3.MoveTowards(aimPoint.position, aimPos, aimSpeed * Time.deltaTime);
        }

        if (aimPoint.position == aimPos && endAim)
        {
            rigBdr.enabled = false;
            aimPoint.gameObject.SetActive(false);
            endAim = false;
        }
    }

    private void MoveTargetingPosition()
    {
        if (!targetingMove) return;

        if (transform.position != targetingPos)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetingPos, targetingMoveSpeed * Time.deltaTime);
        }
        else
        {
            targetingMove = false;
        }
    }

    /// <summary>
    /// 커맨드리스트의 커맨드를 실행
    /// </summary>
    private void CommandApplication()
    {
        var runCommand = commandList.Count > 0 && !targetingMove;
        if (!runCommand) return;

        var command = commandList[0];
        switch (command.type)
        {
            case CommandType.Wait:
                WaitProcess(command);
                break;
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

    private void WaitProcess(CharacterCommand command)
    {
        timer += Time.deltaTime;
        if (timer > command.time)
        {
            commandList.Remove(command);
            timer = 0f;
        }
    }

    /// <summary>
    /// 캐릭터 이동 처리
    /// </summary>
    /// <param name="command"></param>
    private void MoveProcess(CharacterCommand command)
    {
        if (targetList.Count > 0)
        {
            targetList.Clear();
        }

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
            if (command.cover != null)
            {
                coverNode = command.cover;
            }
            else
            {
                var findCover = FindTargetDirectionCover();
                if (findCover != null)
                {
                    coverNode = findCover;
                }
                else if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, DataUtility.nodeSize, coverLayer))
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

        FieldNode FindTargetDirectionCover()
        {
            var targetList = ownerType != CharacterOwner.Player ? gameMgr.playerList : gameMgr.enemyList;
            var closeList = targetList.FindAll(x => DataUtility.GetDistance(transform.position, x.transform.position) < weapon.range);
            if (closeList.Count > 0)
            {
                var closeTarget = closeList.OrderBy(x => DataUtility.GetDistance(transform.position, x.transform.position)).ToList()[0];
                var dir = closeTarget.transform.position - transform.position;
                if (Physics.Raycast(transform.position, dir, out RaycastHit hit, DataUtility.nodeSize, coverLayer))
                {
                    var cover = hit.collider.GetComponentInParent<Cover>();
                    return currentNode.onAxisNodes.Find(x => x == cover.node) != null ? cover.node : null;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 엄폐 후 캐릭터 방향 찾기
    /// </summary>
    /// <param name="coverNode"></param>
    private void FindDirectionForCover(FieldNode coverNode)
    {
        transform.LookAt(coverNode.transform);
        var rightHit = false;
        var leftHit = false;
        if (Physics.Raycast(transform.position, transform.right, DataUtility.nodeSize, nodeLayer))
        {
            rightHit = true;
        }
        if (Physics.Raycast(transform.position, -transform.right, DataUtility.nodeSize, nodeLayer))
        {
            leftHit = true;
        }

        var targetDir = FindCloseTargetDirection();
        if (rightHit || leftHit)
        {
            var rightCover = false;
            var leftCover = false;
            var pos = transform.position + (transform.right * DataUtility.nodeSize);
            if (Physics.Raycast(pos, transform.forward, DataUtility.nodeSize, coverLayer))
            {
                rightCover = true;
            }
            pos = transform.position + (-transform.right * DataUtility.nodeSize);
            if (Physics.Raycast(pos, transform.forward, DataUtility.nodeSize, coverLayer))
            {
                leftCover = true;
            }

            if (rightHit && !rightCover && targetDir == TargetDirection.Right)
            {
                animator.SetBool("isRight", true);
            }
            else if (leftHit && !leftCover && targetDir == TargetDirection.Left)
            {
                animator.SetBool("isRight", false);
            }
            else if (rightCover && leftCover)
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
            animator.SetBool("isRight", true);
            Debug.LogError("not Found Node");
            return;
        }

        cover = coverNode.cover;
        coverPos = transform.position + (transform.forward * coverInterval);
        covering = true;
        animator.SetBool("isCover", true);

        TargetDirection FindCloseTargetDirection()
        {
            var targetList = ownerType != CharacterOwner.Player ? gameMgr.playerList : gameMgr.enemyList;
            var closeList = targetList.FindAll(x => DataUtility.GetDistance(transform.position, x.transform.position) < weapon.range);
            if (closeList.Count > 0)
            {
                var closeTarget = closeList.OrderBy(x => DataUtility.GetDistance(transform.position, x.transform.position)).ToList()[0];
                var dir = closeTarget.transform.position - transform.position;
                var angleInDegrees = Vector3.Angle(dir, transform.forward);
                var cross = Vector3.Cross(dir, transform.forward);

                return cross.y <= 0f ? TargetDirection.Right : TargetDirection.Left;
            }
            else
            {
                return TargetDirection.Right;
            }
        }
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
            if (command.lookAt != null)
            {
                transform.LookAt(command.lookAt);
            }
            commandList.Remove(command);
        }
    }

    /// <summary>
    /// 캐릭터 엄폐사격 처리
    /// </summary>
    /// <param name="command"></param>
    private void CoverAimProcess(CharacterCommand command)
    {
        if (!covering && !animator.GetBool("coverAim"))
        {
            animator.SetBool("isRight", command.targetInfo.isRight);
            animator.SetBool("coverAim", true);
            coverPos = command.targetInfo.shooterNode.transform.position;
            covering = true;
            weapon.firstShot = true;
            var value = Random.Range(0, 100);
            weapon.isHit = value < weapon.hitAccuracy ? true : false;
            aimTf = command.targetInfo.target.transform;
            if (!weapon.isHit)
            {
                var dir = System.Convert.ToBoolean(Random.Range(0, 2)) ? transform.right : -transform.right;
                var errorInterval = 1f;
                aimInterval = dir * errorInterval;
                aimInterval.y += DataUtility.aimPointY;
            }
            else
            {
                aimInterval = new Vector3(0f, DataUtility.aimPointY, 0f);
            }
            aimPoint.position = DataUtility.GetAimPosition(transform, command.targetInfo.isRight);
        }
        else if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Aim"))
        {
            rigBdr.enabled = true;
            aimPoint.gameObject.SetActive(true);
            animator.SetBool("isAim", true);

            if (transform.position != coverPos)
            {
                transform.position = Vector3.MoveTowards(transform.position, coverPos, coverAimSpeed * Time.deltaTime);
            }
            else
            {
                animator.SetInteger("shootNum", weapon.bulletsPerShot);
                coverPos = currentNode.transform.position + (transform.forward * coverInterval);
                commandList.Remove(command);
                covering = false;
            }
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
            transform.LookAt(command.targetInfo.target.transform);
            rigBdr.enabled = true;
            animator.SetBool("isAim", true);
            animator.SetInteger("shootNum", weapon.bulletsPerShot);

            weapon.firstShot = true;
            var value = Random.Range(0, 100);
            weapon.isHit = value < weapon.hitAccuracy ? true : false;
            Debug.Log($"{transform.name}: {value}/{weapon.hitAccuracy} = {weapon.isHit}");

            aimTf = command.targetInfo.target.transform;
            if (!weapon.isHit)
            {
                var dir = System.Convert.ToBoolean(Random.Range(0, 2)) ? transform.right : -transform.right;
                var errorInterval = 1f;
                aimInterval = dir * errorInterval;
                aimInterval.y += DataUtility.aimPointY;
            }
            else
            {
                aimInterval = new Vector3(0f, DataUtility.aimPointY, 0f);
            }
            aimPoint.transform.position = aimTf.position + aimInterval;
            aimPoint.gameObject.SetActive(true);
        }

        var shootNum = animator.GetInteger("shootNum");
        if (shootNum == 0) return;

        if (animator.GetCurrentAnimatorStateInfo(1).IsTag("Aim"))
        {
            weapon.FireBullet(command.targetInfo.target);
            shootNum--;
            animator.SetInteger("shootNum", shootNum);
            if (shootNum == 0)
            {
                var targetInfo = command.targetInfo;
                targetInfo.target.SetTargeting(false);
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
    /// 사격 가능한 타겟 찾음
    /// </summary>
    public void FindTargets()
    {
        targetList.Clear();
        var _targetList = ownerType != CharacterOwner.Player ? gameMgr.playerList : gameMgr.enemyList;
        for (int i = 0; i < _targetList.Count; i++)
        {
            var target = _targetList[i];
            var pos = currentNode.transform.position;
            var targetPos = target.currentNode.transform.position;
            var distance = DataUtility.GetDistance(pos, targetPos);
            if (distance < weapon.range)
            {
                FieldNode RN = null;
                FieldNode LN = null;
                FieldNode targetRN = null;
                FieldNode targetLN = null;
                var cover = FindCoverNode(this, target);
                var targetCover = FindCoverNode(target, this);
                if (cover != null && targetCover != null)
                {
                    RN = CheckTheCanMoveNode(pos, cover, TargetDirection.Right);
                    LN = CheckTheCanMoveNode(pos, cover, TargetDirection.Left);
                    if (RN == null && LN == null)
                    {
                        Debug.Log($"{transform.name}: 사격할 공간이 없음(=> {target.name})");
                        continue;
                    }
                    targetRN = CheckTheCanMoveNode(targetPos, targetCover, TargetDirection.Right);
                    targetLN = CheckTheCanMoveNode(targetPos, targetCover, TargetDirection.Left);
                    if (targetRN == null && targetLN == null)
                    {
                        Debug.Log($"{transform.name}: 적이 나올 공간이 없음(=> {target.name})");
                        continue;
                    }

                    if (!FindNodeOfShooterAndTarget(target, cover, RN, LN, targetCover, targetRN, targetLN))
                    {
                        Debug.Log($"{transform.name}: 사격 경로가 막힘(=> {target.name})");
                    }
                }
                else if (cover != null)
                {
                    RN = CheckTheCanMoveNode(pos, cover, TargetDirection.Right);
                    LN = CheckTheCanMoveNode(pos, cover, TargetDirection.Left);
                    if (RN == null && LN == null)
                    {
                        Debug.Log($"{transform.name}: 사격할 공간이 없음(=> {target.name})");
                        continue;
                    }

                    if (!FindNodeOfShooterAndTarget(target, cover, RN, LN, targetCover, targetRN, targetLN))
                    {
                        Debug.Log($"{transform.name}: 사격 경로가 막힘(=> {target.name})");
                    }
                }
                else if (targetCover != null)
                {
                    targetRN = CheckTheCanMoveNode(targetPos, targetCover, TargetDirection.Right);
                    targetLN = CheckTheCanMoveNode(targetPos, targetCover, TargetDirection.Left);
                    if (targetRN == null && targetLN == null)
                    {
                        Debug.Log($"{transform.name}: 적이 나올 공간이 없음(=> {target.name})");
                        continue;
                    }

                    if (!FindNodeOfShooterAndTarget(target, cover, RN, LN, targetCover, targetRN, targetLN))
                    {
                        Debug.Log($"{transform.name}: 사격 경로가 막힘(=> {target.name})");
                    }
                }
                else
                {
                    if (CheckTheCoverAlongPath(pos, targetPos))
                    {
                        var targetInfo = new TargetInfo
                        {
                            target = target,
                            shooterNode = currentNode,
                            targetNode = target.currentNode,
                        };
                        targetList.Add(targetInfo);
                    }
                    else
                    {
                        Debug.Log($"{transform.name}: 사격 경로가 막힘(=> {target.name})");
                    }
                }
            }
            else
            {
                Debug.Log($"{transform.name}: 사거리가 닿지 않음(=> {target.name})");
            }
        }

        FieldNode FindCoverNode(CharacterController shooter, CharacterController target)
        {
            var dir = Vector3.Normalize(target.transform.position - shooter.transform.position);
            if (Physics.Raycast(shooter.transform.position, dir, out RaycastHit hit, DataUtility.nodeSize, coverLayer))
            {
                var coverNode = hit.collider.GetComponentInParent<FieldNode>();
                if (shooter.currentNode.onAxisNodes.Find(x => x == coverNode) != null)
                {
                    return coverNode;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        FieldNode CheckTheCanMoveNode(Vector3 pos, FieldNode coverNode, TargetDirection targetDir)
        {
            FieldNode node = null;
            var frontDir = Vector3.Normalize(coverNode.transform.position - pos);
            var dir = targetDir == TargetDirection.Right ? Quaternion.Euler(0, 90f, 0) * frontDir : Quaternion.Euler(0, -90f, 0) * frontDir;
            if (Physics.Raycast(pos, dir, out RaycastHit hit, DataUtility.nodeSize, nodeLayer))
            {
                node = hit.collider.GetComponentInParent<FieldNode>();
                if (!node.canMove)
                {
                    node = null;
                }
            }

            return node;
        }

        bool FindNodeOfShooterAndTarget(CharacterController target, FieldNode cover, FieldNode RN, FieldNode LN, FieldNode targetCover, FieldNode targetRN, FieldNode targetLN)
        {
            FieldNode shooterNode = null;
            FieldNode targetNode = null;
            bool isRight = false;
            bool targetRight = false;
            Vector3 pos;
            Vector3 targetPos;
            var distance = 999999f;

            if (cover == null)
            {
                if (targetRN != null)
                {
                    CheckNodes(currentNode, targetRN, false, true);
                }
                if (targetLN != null)
                {
                    CheckNodes(currentNode, targetLN, false, false);
                }
            }
            else
            {
                if (RN != null)
                {
                    if (target.cover == null)
                    {
                        CheckNodes(RN, target.currentNode, true, false);
                    }
                    else
                    {
                        if (targetRN != null)
                        {
                            CheckNodes(RN, targetRN, true, true);
                        }
                        if (targetLN != null)
                        {
                            CheckNodes(RN, targetLN, true, false);
                        }
                    }
                }
                if (LN != null)
                {
                    if (target.cover == null)
                    {
                        CheckNodes(LN, target.currentNode, false, false);
                    }
                    else
                    {
                        if (targetRN != null)
                        {
                            CheckNodes(LN, targetRN, false, true);
                        }
                        if (targetLN != null)
                        {
                            CheckNodes(LN, targetLN, false, false);
                        }
                    }
                }
            }

            if (shooterNode != null && targetNode != null)
            {
                var targetInfo = new TargetInfo
                {
                    target = target,
                    shooterNode = shooterNode,
                    shooterCover = cover,
                    targetNode = targetNode,
                    targetCover = targetCover,
                    targetRight = targetRight,
                    isRight = isRight,
                };
                targetList.Add(targetInfo);
                return true;
            }
            else
            {
                return false;
            }

            void CheckNodes(FieldNode _shooterNode, FieldNode _targetNode, bool _isRight, bool _targetRight)
            {
                pos = _shooterNode.transform.position;
                targetPos = _targetNode.transform.position;
                if (CheckTheCoverAlongPath(pos, targetPos))
                {
                    var dist = DataUtility.GetDistance(pos, targetPos);
                    if (dist < distance)
                    {
                        shooterNode = _shooterNode;
                        targetNode = _targetNode;
                        isRight = _isRight;
                        targetRight = _targetRight;
                        distance = dist;
                    }
                }
            }

        }

        bool CheckTheCoverAlongPath(Vector3 pos, Vector3 targetPos)
        {
            bool canShoot;
            var dir = Vector3.Normalize(targetPos - pos);
            if (Physics.Raycast(pos, dir, weapon.range, coverLayer))
            {
                canShoot = false;
            }
            else
            {
                canShoot = true;
            }

            return canShoot;
        }
    }

    /// <summary>
    /// 타겟을 설정
    /// </summary>
    public bool SetTarget()
    {
        if (targetList.Count == 0)
        {
            return false;
        }
        else
        {
            targetIndex = 0;
            var targetInfo = targetList[targetIndex];
            targetInfo.target.SetTargeting(true, targetInfo.targetRight);
            CameraState camState;
            if (cover == null)
            {
                camState = CameraState.FrontAim;
            }
            else if (targetInfo.isRight)
            {
                camState = CameraState.RightAim;
            }
            else
            {
                camState = CameraState.LeftAim;
            }
            gameMgr.camMgr.SetCameraState(camState, transform, targetInfo.target.transform);
            return true;
        }
    }

    /// <summary>
    /// 다음 타겟을 설정
    /// </summary>
    public void SetNextTarget()
    {
        if (targetList.Count < 2) return;

        var prevTargetInfo = targetList[targetIndex];
        prevTargetInfo.target.SetTargeting(false);
        targetIndex++;
        if (targetIndex == targetList.Count)
        {
            targetIndex = 0;
        }

        var targetInfo = targetList[targetIndex];
        targetInfo.target.SetTargeting(true, targetInfo.targetRight);
        CameraState camState;
        if (cover == null)
        {
            camState = CameraState.FrontAim;
        }
        else if (targetInfo.isRight)
        {
            camState = CameraState.RightAim;
        }
        else
        {
            camState = CameraState.LeftAim;
        }
        gameMgr.camMgr.SetCameraState(camState, transform, targetInfo.target.transform);
    }

    /// <summary>
    /// 커맨드 추가
    /// </summary>
    /// <param name="type"></param>
    /// <param name="passList"></param>
    public void AddCommand(CommandType type, float time)
    {
        switch (type)
        {
            case CommandType.Wait:
                var waitCommand = new CharacterCommand
                {
                    type = CommandType.Wait,
                    time = time,
                };
                commandList.Add(waitCommand);
                break;
            default:
                break;
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
    /// <param name="passList"></param>
    public void AddCommand(CommandType type, FieldNode cover)
    {
        switch (type)
        {
            case CommandType.TakeCover:
                var takeCoverCommand = new CharacterCommand
                {
                    type = CommandType.TakeCover,
                    cover = cover,
                };
                commandList.Add(takeCoverCommand);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 커맨드 추가
    /// </summary>
    /// <param name="type"></param>
    /// <param name="passList"></param>
    public void AddCommand(CommandType type, Transform lookAt)
    {
        switch (type)
        {
            case CommandType.LeaveCover:
                var leaveCoverCommand = new CharacterCommand
                {
                    type = CommandType.LeaveCover,
                    lookAt = lookAt,
                };
                commandList.Add(leaveCoverCommand);
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
            case CommandType.CoverAim:
                var coverAimCommand = new CharacterCommand
                {
                    type = CommandType.CoverAim,
                    targetInfo = targetList[targetIndex],
                };
                commandList.Add(coverAimCommand);
                break;
            case CommandType.Shoot:
                var shootCommand = new CharacterCommand
                {
                    type = CommandType.Shoot,
                    targetInfo = targetList[targetIndex],
                };
                commandList.Add(shootCommand);
                break;
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
    /// 조준상태로 설정
    /// </summary>
    /// <param name="value"></param>
    public void SetTargeting(bool value, bool isRight)
    {
        if (!animator.GetBool("isCover")) return;

        animator.SetBool("isRight", isRight);
        switch (value)
        {
            case true:
                var dir = animator.GetBool("isRight") ? transform.right : -transform.right;
                var moveDist = 0.7f;
                targetingPos = transform.position + (dir * moveDist);
                animator.SetTrigger("targeting");
                break;
            case false:
                break;
        }
        targetingMove = true;
    }

    /// <summary>
    /// 조준상태로 설정
    /// </summary>
    /// <param name="value"></param>
    public void SetTargeting(bool value)
    {
        if (!animator.GetBool("isCover")) return;

        switch (value)
        {
            case true:
                break;
            case false:
                targetingPos = currentNode.transform.position + (transform.forward * coverInterval);
                animator.SetTrigger("unTargeting");
                break;
        }
        targetingMove = true;
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
        else if (health > 0 && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Hit"))
        {
            animator.SetTrigger("isHit");
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

        if (!animator.GetBool("isCover"))
        {
            rigBdr.enabled = false;
            aimPoint.gameObject.SetActive(false);
        }
        else
        {
            aimTf = transform;
            aimInterval = transform.forward * DataUtility.aimPointZ;
            aimInterval.y += DataUtility.aimPointY;
            endAim = true;
        }
        commandList.Remove(command);
        animator.SetBool("isAim", false);
        animator.SetBool("coverAim", false);
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
