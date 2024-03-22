using System.Collections;
using System.Collections.Generic;
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
    Shoot,
    Reload,
}

public class CharacterCommand
{
    public CommandType type;

    //Move
    public List<FieldNode> passList;

    //Shoot
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

    [Header("--- Assignment Variable---")]
    public CharacterOwner ownerType;
    public int mobility;
    public int maxHealth;
    public int health;

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

    private readonly float aimPointY = 0.9f;
    private readonly float aimOffTime = 0.3f;

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
        coverLayer = LayerMask.GetMask("Cover");

        currentNode = _currentNode;
        currentNode.charCtr = this;
        currentNode.canMove = false;
    }

    private void OnDrawGizmos()
    {
        DrawWeaponRange();
    }

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

    private void FindDirectionForCover(FieldNode coverNode)
    {
        transform.LookAt(coverNode.transform);
        var pos = transform.position + (transform.right * DataUtility.nodeSize);
        if (Physics.Raycast(pos, transform.forward, out RaycastHit hit, DataUtility.nodeSize, coverLayer))
        {
            animator.SetBool("isRight", false);
        }
        else
        {
            animator.SetBool("isRight", true);
        }
        cover = coverNode.cover;
        coverPos = transform.position + (transform.forward * coverInterval);
        covering = true;
        animator.SetBool("isCover", true);
    }

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

    private void ShootProcess(CharacterCommand command)
    {
        if (!animator.GetBool("isAim"))
        {
            transform.LookAt(command.target.transform);
            weapon.firstShot = true;
            rigBdr.enabled = true;
            var targetPos = command.target.transform.position;
            aimPoint.transform.position = new Vector3(targetPos.x, aimPointY, targetPos.z);
            animator.SetBool("isAim", true);
            animator.SetInteger("shootNum", weapon.bulletsPerShot);
        }

        var shootNum = animator.GetInteger("shootNum");
        if (animator.GetCurrentAnimatorStateInfo(1).IsTag("Aim") && shootNum > 0)
        {
            weapon.FireBullet(command.target);
            shootNum--;
            animator.SetInteger("shootNum", shootNum);
            if (shootNum == 0)
            {
                StartCoroutine(WaitAimOff(command));
            }
        }
    }

    private void ReloadPrecess(CharacterCommand command)
    {
        if (!reloading && animator.GetCurrentAnimatorStateInfo(1).IsTag("None"))
        {
            animator.SetTrigger("reload");
            weapon.Reload();
            reloading = true;
        }
    }

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

    public void AddCommand(CommandType type, CharacterController target)
    {
        switch (type)
        {
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

    #region Coroutine
    private IEnumerator WaitAimOff(CharacterCommand command)
    {
        yield return new WaitForSeconds(aimOffTime);

        rigBdr.enabled = false;
        commandList.Remove(command);
        animator.SetBool("isAim", false);
    }
    #endregion

    #region Animation Event
    public void Event_WeaponSwitching(string switchPos)
    {
        weapon.WeaponSwitching(switchPos);
    }

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
