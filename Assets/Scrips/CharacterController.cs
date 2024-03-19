using System.Collections;
using System.Collections.Generic;
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

    [HideInInspector] public FieldNode currentNode;

    private List<CharacterCommand> commandList = new List<CharacterCommand>();
    private bool moving;

    private readonly float moveSpeed = 0.045f;

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
        currentNode = _currentNode;
    }

    private void Update()
    {
        CommandProcess();
    }

    private void CommandProcess()
    {
        if (commandList.Count == 0) return;

        var command = commandList[0];
        switch (command.type)
        {
            case CommandType.Move:
                CharacterMove(command);
                break;
            default:
                break;
        }
    }

    private void CharacterMove(CharacterCommand command)
    {
        var targetNode = command.passList[0];
        if (!animator.GetBool("isMove") && targetNode == currentNode)
        {
            command.passList.RemoveAt(0);
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
                currentNode = command.passList[^1];
            }
            if (!moving)
            {
                transform.LookAt(targetNode.transform);
            }

            var direction = Vector3.Normalize(targetNode.transform.position - transform.position);
            var distance = DataUtility.GetDistance(transform.position, targetNode.transform.position);
            if (distance > 0.05f)
            {
                transform.position += direction * (moveSpeed * Time.timeScale);
            }
            else
            {
                transform.position = targetNode.transform.position;
                moving = false;
                command.passList.RemoveAt(0);
                if (command.passList.Count == 0)
                {
                    animator.SetBool("isMove", false);
                    commandList.RemoveAt(0);
                }
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
}
