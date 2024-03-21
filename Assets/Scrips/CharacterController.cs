using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

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
}

public class CharacterCommand
{
    public CommandType type;

    //Move
    public List<FieldNode> passList;
}

public class CharacterController : MonoBehaviour
{
    [Header("---Access Script---")]
    [SerializeField] private GameManager gameMgr;

    [Header("---Access Component---")]
    public Animator animator;

    [Header("--- Assignment Variable---")]
    public CharacterOwner ownerType;
    public int mobility;

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
                animator.SetBool("isMove", true);
                currentNode.charCtr = null;
                currentNode = command.passList[0];
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
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, DataUtility.nodeSize, coverLayer))
            {
                var cover = hit.collider.GetComponentInParent<Cover>();
                coverNode = cover.node;
            }
            else
            {
                var node = currentNode.onAxisNodes.Find(x => x.cover != null);
                if (node != null)
                {
                    Debug.Log(node.name);
                    coverNode = node;

                }
            }

            if (coverNode != null)
            {
                transform.LookAt(coverNode.transform);
                coverPos = transform.position + (transform.forward * coverInterval);
                covering = true;
                animator.SetBool("isCover", true);
            }
            else
            {
                commandList.Remove(command);
            }
        }
        else
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

    public void AddCommand(CommandType type)
    {
        switch (type)
        {
            case CommandType.TakeCover:
                var takeCoverCommand = new CharacterCommand
                {
                    type = CommandType.TakeCover,
                };
                commandList.Add(takeCoverCommand);
                break;
            case CommandType.LeaveCover:
                var leaveCoverCommand = new CharacterCommand
                {
                    type = CommandType.LeaveCover,
                };
                commandList.Add(leaveCoverCommand);
                break;
            default:
                break;
        }
    }
}
