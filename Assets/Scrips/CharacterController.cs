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

    [Header("---Access Component---")]
    public Animator animator;

    private RigBuilder rigBdr;
    private Transform aimPoint;
    [HideInInspector] public Transform aimTarget;

    [Header("--- Assignment Variable---")]
    public CharacterOwner ownerType;
    public int mobility;
    public int maxHealth;
    public int health;

    private LayerMask coverLayer;
    [HideInInspector] public FieldNode currentNode;

    private List<CharacterCommand> commandList = new List<CharacterCommand>();
    private bool moving;
    private bool covering;
    private Vector3 coverPos;

    private readonly float moveSpeed = 7f;
    private readonly float closeDistance = 0.05f;
    private readonly float coverSpeed = 1f;
    private readonly float coverInterval = 0.2f;

    public void SetComponents(GameManager _gameMgr, CharacterOwner _ownerType, FieldNode _currentNode)
    {
        gameMgr = _gameMgr;
        animator = GetComponent<Animator>();

        rigBdr = GetComponent<RigBuilder>();
        rigBdr.enabled = false;
        aimPoint = transform.Find("AimPoint");
        aimTarget = transform.Find("Root/Hips/Spine_01/Spine_02/Spine_03");

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
        coverPos = transform.position + (transform.forward * coverInterval);
        covering = true;
        animator.SetBool("isCover", true);
    }

    private void LeaveCoverProcess(CharacterCommand command)
    {
        if (!covering)
        {
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
            animator.SetBool("isAim", true);
            animator.SetInteger("shootNum", weapon.bulletsPerShot);
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Aim"))
        {
            var shootNum = animator.GetInteger("shootNum");
            weapon.FireBullet(command.target);
            shootNum--;
            animator.SetInteger("shootNum", shootNum);
            if (shootNum == 0)
            {
                animator.SetBool("isAim", false);
                commandList.Remove(command);
            }
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

    public GameManager GameMgr
    {
        private set { gameMgr = value; }
        get { return gameMgr; }
    }
}
