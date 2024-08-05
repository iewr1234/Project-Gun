using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Unity.VisualScripting;
using EPOOutline;

public enum CharacterOwner
{
    None,
    Player,
    Enemy,
    All,
}

public enum CharacterState
{
    None,
    Watch,
    Dead,
}

public enum CommandType
{
    None,
    Wait,
    Move,
    TakeCover,
    LeaveCover,
    BackCover,
    Targeting,
    Aim,
    Watch,
    Shoot,
    Reload,
    ChangeWeapon,
}

[System.Serializable]
public class CharacterCommand
{
    public string indexName;
    public CommandType type;

    [Header("[Wait]")]
    public float time;

    [Header("[Move]")]
    public List<FieldNode> movePass;

    [Header("[Cover]")]
    public Cover cover;
    public bool isRight;

    [Header("[Targeting]")]
    public bool targeting;
    public Transform lookAt;

    [Header("[Shoot]")]
    public TargetInfo targetInfo;
}

[System.Serializable]
public struct TargetInfo
{
    public CharacterController shooter;
    public CharacterController target;
    public FieldNode shooterNode;
    public Cover shooterCover;
    public FieldNode targetNode;
    public Cover targetCover;
    public bool isRight;
    public bool targetRight;
}

[System.Serializable]
public struct WatchInfo
{
    public DrawRange drawRang;
    public FieldNode targetNode;
    public FieldNode watchNode;
    public Cover shooterCover;
    public bool isRight;
    public float minAngle;
    public float maxAngle;
}

public struct LineInfo
{
    public Vector3 startPos;
    public Vector3 endPos;
}

public class CharacterController : MonoBehaviour
{
    [Header("---Access Script---")]
    [SerializeField] private GameManager gameMgr;
    [HideInInspector] public CharacterUI charUI;
    public List<Weapon> weapons;
    public Armor armor;

    [Header("---Access Component---")]
    public Animator animator;
    private Outlinable outlinable;
    [SerializeField] private Collider cd;

    [HideInInspector] public Transform aimPoint;
    private MultiAimConstraint headRig;
    private MultiAimConstraint chestRig;

    [HideInInspector] public Transform mainHolsterTf;
    [HideInInspector] public Transform subHolsterTf;
    [HideInInspector] public Transform rightHandTf;
    [HideInInspector] public Transform leftHandTf;

    [HideInInspector] public List<MeshRenderer> meshs = new List<MeshRenderer>();
    [HideInInspector] public List<SkinnedMeshRenderer> sMeshs = new List<SkinnedMeshRenderer>();
    private List<Collider> ragdollCds = new List<Collider>();
    private List<Rigidbody> ragdollRbs = new List<Rigidbody>();

    private Transform weaponPoolTf;
    private List<Weapon> weaponPool = new List<Weapon>();

    [Header("--- Assignment Variable---")]
    [Tooltip("사용자 타입")] public CharacterOwner ownerType;
    [Tooltip("캐릭터 상태")] public CharacterState state;

    [Header("[Status]")]
    [Tooltip("힘")] public int strength;
    [Tooltip("활력")] public int vitality;
    [Tooltip("지능")] public int intellect;
    [Tooltip("지혜")] public int wisdom;
    [Tooltip("민첩")] public int agility;
    [Tooltip("솜씨")] public int dexterity;
    [Tooltip("이동력")] public float Mobility => DataUtility.GetFloorValue(1 + 2.5f * ((float)agility / (agility + 100)), 2);
    public float mobility;
    [HideInInspector] public int maxMoveNum;
    [HideInInspector] public int shootMoveNum;

    [Header("[Physical]")]
    [Tooltip("최대 행동력")] public int maxAction;
    [Tooltip("행동력")] public int action;
    [Tooltip("최대 체력")] public int maxHealth;
    [Tooltip("체력")] public int health;
    [Tooltip("최대 기력")] public int maxStamina;
    [Tooltip("기력")] public int stamina;
    [Tooltip("시야")] public float sight;
    [Tooltip("조준")] public int aiming;
    [Tooltip("반응")] public int reaction;

    [HideInInspector] public int baseIndex;
    [HideInInspector] public int upperIndex;

    [Space(5f)]
    public Weapon currentWeapon;
    [HideInInspector] public int fiarRate;
    [HideInInspector] public int sightRate;

    public FieldNode currentNode;
    private FieldNode prevNode;
    [HideInInspector] public Cover cover;
    [HideInInspector] public bool isCopy;

    [Space(5f)]
    public AIDataInfo aiData;
    //private List<FieldNode> visibleNodes = new List<FieldNode>();

    [Space(5f)]
    public List<TargetInfo> targetList = new List<TargetInfo>();
    [HideInInspector] public int targetIndex;

    [Space(5f)]
    public WatchInfo watchInfo;
    [Space(5f)]

    public List<CharacterCommand> commandList = new List<CharacterCommand>();

    [HideInInspector] public bool pause;
    private float timer;

    private bool moving;
    private readonly float moveSpeed = 7f;
    private readonly float jumpSpeed = 2f;

    private bool covering;
    private Vector3 coverPos;
    private readonly float coverSpeed = 1f;
    private readonly float coverInterval = 0.2f;
    private readonly float coverAimSpeed = 3f;

    private Transform aimTf;
    private Vector3 aimInterval;
    private bool headAim;
    private bool chestAim;
    private readonly float aimSpeed = 2f;
    private readonly float aimTime = 0.25f;

    private bool canTargeting;
    private bool targetingMove;
    private Vector3 targetingPos;
    private readonly float targetingMoveSpeed_Pistol = 1.5f;
    private readonly float targetingMoveSpeed_Rifle = 1f;

    private bool reloading;

    private bool changing;
    private int changeIndex;

    /// <summary>
    /// 구성요소 설정
    /// </summary>
    /// <param name="_gameMgr"></param>
    /// <param name="_ownerType"></param>
    /// <param name="playerData"></param>
    /// <param name="_currentNode"></param>
    public void SetComponents(GameManager _gameMgr, CharacterOwner _ownerType, PlayerDataInfo playerData, FieldNode _currentNode)
    {
        gameMgr = _gameMgr;
        animator = GetComponent<Animator>();
        outlinable = this.AddComponent<Outlinable>();
        cd = GetComponent<Collider>();

        aimPoint = transform.Find("AimPoint");
        headRig = transform.Find("Rig/HeadAim").GetComponent<MultiAimConstraint>();
        headRig.weight = 0f;
        chestRig = transform.Find("Rig/ChestAim").GetComponent<MultiAimConstraint>();
        chestRig.weight = 0f;

        mainHolsterTf = transform.Find("Root/Hips/Spine_01/Spine_02");
        subHolsterTf = transform.Find("Root/Hips/UpperLeg_R/Holster");
        rightHandTf = transform.Find("Root/Hips/Spine_01/Spine_02/Spine_03/Clavicle_R/Shoulder_R/Elbow_R/Hand_R");
        leftHandTf = transform.Find("Root/Hips/Spine_01/Spine_02/Spine_03/Clavicle_L/Shoulder_L/Elbow_L/Hand_L");

        ownerType = _ownerType;
        meshs = transform.GetComponentsInChildren<MeshRenderer>().ToList();
        sMeshs = transform.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
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

        weaponPoolTf = transform.Find("WeaponPool");
        weaponPool = weaponPoolTf.GetComponentsInChildren<Weapon>().ToList();
        for (int i = 0; i < weaponPool.Count; i++)
        {
            var weapon = weaponPool[i];
            weapon.gameObject.SetActive(false);
        }

        var name = transform.name.Split(' ', '(', ')')[0];
        transform.name = $"{name}_{gameMgr.playerList.Count}";
        gameMgr.playerList.Add(this);

        strength = playerData.strength;
        vitality = playerData.vitality;
        intellect = playerData.intellect;
        wisdom = playerData.wisdom;
        agility = playerData.agility;
        dexterity = playerData.dexterity;
        mobility = Mobility;

        maxAction = playerData.maxAction;
        action = maxAction;
        maxHealth = playerData.maxHealth;
        health = maxHealth;
        maxStamina = playerData.maxStamina;
        stamina = maxStamina;
        sight = playerData.sight;
        aiming = playerData.aiming;
        reaction = playerData.reaction;

        baseIndex = 1;
        upperIndex = 2;

        fiarRate = 0;
        sightRate = 0;

        currentNode = _currentNode;
        currentNode.charCtr = this;
        currentNode.canMove = false;
        //ShowVisibleNodes(sight, currentNode);
    }

    /// <summary>
    /// 구성요소 설정
    /// </summary>
    /// <param name="_gameMgr"></param>
    /// <param name="_ownerType"></param>
    /// <param name="enemyData"></param>
    /// <param name="_currentNode"></param>
    public void SetComponents(GameManager _gameMgr, CharacterOwner _ownerType, EnemyDataInfo enemyData, FieldNode _currentNode)
    {
        gameMgr = _gameMgr;
        animator = GetComponent<Animator>();
        outlinable = this.AddComponent<Outlinable>();
        cd = GetComponent<Collider>();

        aimPoint = transform.Find("AimPoint");
        headRig = transform.Find("Rig/HeadAim").GetComponent<MultiAimConstraint>();
        headRig.weight = 0f;
        chestRig = transform.Find("Rig/ChestAim").GetComponent<MultiAimConstraint>();
        chestRig.weight = 0f;

        mainHolsterTf = transform.Find("Root/Hips/Spine_01/Spine_02");
        subHolsterTf = transform.Find("Root/Hips/UpperLeg_R/Holster");
        rightHandTf = transform.Find("Root/Hips/Spine_01/Spine_02/Spine_03/Clavicle_R/Shoulder_R/Elbow_R/Hand_R");
        leftHandTf = transform.Find("Root/Hips/Spine_01/Spine_02/Spine_03/Clavicle_L/Shoulder_L/Elbow_L/Hand_L");

        ownerType = _ownerType;
        meshs = transform.GetComponentsInChildren<MeshRenderer>().ToList();
        sMeshs = transform.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
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

        weaponPoolTf = transform.Find("WeaponPool");
        weaponPool = weaponPoolTf.GetComponentsInChildren<Weapon>().ToList();
        for (int i = 0; i < weaponPool.Count; i++)
        {
            var weapon = weaponPool[i];
            weapon.gameObject.SetActive(false);
        }

        var name = transform.name.Split(' ', '(', ')')[0];
        transform.name = $"{name}_{gameMgr.enemyList.Count}";
        gameMgr.enemyList.Add(this);

        strength = enemyData.strength;
        vitality = enemyData.vitality;
        intellect = enemyData.intellect;
        wisdom = enemyData.wisdom;
        agility = enemyData.agility;
        dexterity = enemyData.dexterity;
        mobility = Mobility;

        maxAction = enemyData.maxAction;
        action = maxAction;
        maxHealth = enemyData.maxHealth;
        health = maxHealth;
        maxStamina = enemyData.maxStamina;
        stamina = maxStamina;
        sight = enemyData.sight;
        aiming = enemyData.aiming;
        reaction = enemyData.reaction;

        aiData = gameMgr.dataMgr.aiData.aiInfos.Find(x => x.ID == enemyData.aiID);

        baseIndex = 1;
        upperIndex = 2;

        currentNode = _currentNode;
        currentNode.charCtr = this;
        currentNode.canMove = false;
    }

    /// <summary>
    /// Rig 설정
    /// </summary>
    /// <param name="type"></param>
    public void SetRig(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Pistol:
                chestRig.data.offset = new Vector3(-10f, 0f, 10f);
                break;
            case WeaponType.Rifle:
                chestRig.data.offset = new Vector3(-40f, 0f, 0f);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 외곽선 설정
    /// </summary>
    public void SetOutlinable()
    {
        outlinable.RenderStyle = RenderStyle.FrontBack;
        outlinable.BackParameters.DilateShift = 0f;
        outlinable.BackParameters.BlurShift = 0f;
        outlinable.BackParameters.FillPass.Shader = Resources.Load<Shader>("Shaders/Outline/Fills/Interlaced");
        outlinable.BackParameters.FillPass.SetColor("_PublicColor", Color.gray);
        outlinable.BackParameters.FillPass.SetFloat("_PublicSize", 2f);
        outlinable.BackParameters.FillPass.SetFloat("_PublicSpeed", 0f);
        outlinable.BackParameters.FillPass.SetFloat("_PublicAngle", 45f);
        switch (ownerType)
        {
            case CharacterOwner.Player:
                outlinable.BackParameters.Color = DataUtility.color_Player;
                outlinable.BackParameters.FillPass.SetColor("_PublicGapColor", DataUtility.color_Player);
                break;
            case CharacterOwner.Enemy:
                outlinable.BackParameters.Color = DataUtility.color_Enemy;
                outlinable.BackParameters.FillPass.SetColor("_PublicGapColor", DataUtility.color_Enemy);
                break;
            default:
                break;
        }
        outlinable.FrontParameters.Enabled = false;
        outlinable.FrontParameters.DilateShift = 0f;
        outlinable.FrontParameters.BlurShift = 0f;
        outlinable.FrontParameters.Color = Color.red;
        outlinable.AddAllChildRenderersToRenderingList();
    }

    /// <summary>
    /// 외곽선 활성화 설정
    /// </summary>
    /// <param name="value"></param>
    public void SetActiveOutline(bool value)
    {
        if (value)
        {
            outlinable.BackParameters.Color = Color.red;
        }
        else
        {
            switch (ownerType)
            {
                case CharacterOwner.Player:
                    outlinable.BackParameters.Color = DataUtility.color_Player;
                    break;
                case CharacterOwner.Enemy:
                    outlinable.BackParameters.Color = DataUtility.color_Enemy;
                    break;
                default:
                    break;
            }
        }
        var newDilate = value ? 1f : 0f;
        outlinable.BackParameters.DilateShift = newDilate;
        outlinable.FrontParameters.Enabled = value;
        outlinable.FrontParameters.DilateShift = newDilate;
    }

    private void OnDrawGizmos()
    {
        if (state == CharacterState.Dead) return;

        //DrawShootingPath();
        DrawWeaponRange();
    }

    //private void DrawShootingPath()
    //{
    //    if (lineInfos.Count == 0) return;

    //    Gizmos.color = Color.red;
    //    var height = 1f;
    //    for (int i = 0; i < lineInfos.Count; i++)
    //    {
    //        var lineInfo = lineInfos[i];
    //        var startPos = lineInfo.startPos + new Vector3(0f, height, 0f);
    //        var endPos = lineInfo.endPos + new Vector3(0f, height, 0f);
    //        Gizmos.DrawLine(startPos, endPos);
    //    }

    //    //if (targetList.Count == 0) return;

    //    //Gizmos.color = Color.red;
    //    //var height = 1f;
    //    //var pos = new Vector3(currentNode.transform.position.x, height, currentNode.transform.position.z);
    //    //for (int i = 0; i < targetList.Count; i++)
    //    //{
    //    //    var target = targetList[i].target;
    //    //    var targetPos = new Vector3(target.currentNode.transform.position.x, height, target.currentNode.transform.position.z);
    //    //    Gizmos.DrawLine(pos, targetPos);
    //    //}
    //}

    /// <summary>
    /// 무기 사거리 범위 표시
    /// </summary>
    private void DrawWeaponRange()
    {
        if (currentWeapon == null) return;

        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.color = Color.yellow;
        var height = 1f;
        var segments = 30f;
        var angleStep = 360f / segments;
        var angle = 0 * angleStep * Mathf.Deg2Rad;
        var startPos = new Vector3(Mathf.Cos(angle) * currentWeapon.weaponData.range, height, Mathf.Sin(angle) * currentWeapon.weaponData.range);
        for (int i = 0; i <= segments; i++)
        {
            angle = i * angleStep * Mathf.Deg2Rad;
            var endPos = new Vector3(Mathf.Cos(angle) * currentWeapon.weaponData.range, height, Mathf.Sin(angle) * currentWeapon.weaponData.range);
            Gizmos.DrawLine(startPos, endPos);
            startPos = endPos;
        }
    }

    private void Update()
    {
        if (state == CharacterState.Dead) return;

        AimProcess();
        CommandApplication();

        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            animator.Play("Idle");
        }
    }

    /// <summary>
    /// 조준관련 처리
    /// </summary>
    private void AimProcess()
    {
        SetAimWeight(headRig, headAim);
        SetAimWeight(chestRig, chestAim);

        if (aimTf != null)
        {
            aimPoint.position = aimTf.position + aimInterval;
        }

        void SetAimWeight(MultiAimConstraint rig, bool value)
        {
            switch (value)
            {
                case true:
                    if (rig.weight != 1f)
                    {
                        rig.weight += aimSpeed * Time.deltaTime;
                    }
                    break;
                case false:
                    if (rig.weight != 0f)
                    {
                        rig.weight -= aimSpeed * Time.deltaTime;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// 커맨드리스트의 커맨드를 실행
    /// </summary>
    private void CommandApplication()
    {
        var runCommand = commandList.Count > 0 && !pause;
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
            case CommandType.BackCover:
                BackCoverProcess(command);
                break;
            case CommandType.Targeting:
                TargetingProcess(command);
                break;
            case CommandType.Aim:
                AimAndWatchProcess(command);
                break;
            case CommandType.Watch:
                AimAndWatchProcess(command);
                break;
            case CommandType.Shoot:
                ShootProcess(command);
                break;
            case CommandType.Reload:
                ReloadPrecess(command);
                break;
            case CommandType.ChangeWeapon:
                ChangeWeaponProcess(command);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 캐릭터 대기 처리
    /// </summary>
    /// <param name="command"></param>
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
            var tagetInfo = targetList.OrderBy(x => DataUtility.GetDistance(x.shooterNode.transform.position, x.targetNode.transform.position)).FirstOrDefault();
            if (tagetInfo.shooterCover)
            {
                AddCommand(CommandType.TakeCover, tagetInfo.shooterCover, tagetInfo.isRight);
            }
            targetList.Clear();
        }

        var targetNode = command.movePass[^1];
        if (!animator.GetBool("isMove") && targetNode == currentNode)
        {
            prevNode = targetNode;
            command.movePass.Remove(targetNode);
            if (command.movePass.Count == 0)
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
                currentNode = command.movePass[0];
            }

            var canLook = animator.GetCurrentAnimatorStateInfo(baseIndex).IsTag("Idle") || animator.GetCurrentAnimatorStateInfo(baseIndex).IsTag("Move");
            if (canLook && !moving)
            {
                CheckLineCover();
                transform.LookAt(targetNode.transform);
                moving = true;
            }

            var pos = transform.position;
            var targetPos = targetNode.transform.position;
            if (animator.GetCurrentAnimatorStateInfo(baseIndex).IsTag("Move") && pos != targetPos)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            }
            else if (animator.GetCurrentAnimatorStateInfo(baseIndex).IsTag("Jump") && pos != targetPos)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, jumpSpeed * Time.deltaTime);
            }
            else if (pos == targetPos)
            {
                moving = false;
                prevNode = targetNode;
                command.movePass.Remove(targetNode);
                //ShowVisibleNodes(sight, targetNode);
                switch (ownerType)
                {
                    case CharacterOwner.Player:
                        if (targetNode.hitNode)
                        {
                            targetNode.watcher.AddCommand(CommandType.Shoot, this, targetNode);
                            targetNode.hitNode = false;
                            targetNode.watcher = null;
                        }
                        break;
                    case CharacterOwner.Enemy:
                        CheckWatcher(targetNode);
                        break;
                    default:
                        break;
                }

                if (command.movePass.Count == 0)
                {
                    animator.SetBool("isMove", false);
                    commandList.Remove(command);
                    if (commandList.Count == 0)
                    {
                        switch (ownerType)
                        {
                            case CharacterOwner.Player:
                                AddCommand(CommandType.TakeCover);
                                break;
                            case CharacterOwner.Enemy:
                                gameMgr.EnemyAI_Shoot(this);
                                //if (state == CharacterState.Watch)
                                //{
                                //    gameMgr.EnemyAI_Watch(this, currentNode);
                                //}
                                //else
                                //{
                                //    gameMgr.EnemyAI_Shoot(this);
                                //}
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        void CheckLineCover()
        {
            if (targetNode.nodePos.x == prevNode.nodePos.x || targetNode.nodePos.y == prevNode.nodePos.y)
            {
                TargetDirection nextDir;
                if (targetNode.nodePos.x > prevNode.nodePos.x)
                {
                    nextDir = TargetDirection.Right;
                }
                else if (targetNode.nodePos.x < prevNode.nodePos.x)
                {
                    nextDir = TargetDirection.Left;
                }
                else if (targetNode.nodePos.y > prevNode.nodePos.y)
                {
                    nextDir = TargetDirection.Back;
                }
                else
                {
                    nextDir = TargetDirection.Front;
                }

                if (prevNode.outlines[(int)nextDir].lineCover != null)
                {
                    animator.SetTrigger("jump");
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
        var pos = currentNode.transform.position;
        if (!covering)
        {
            Cover cover = null;
            if (command.cover != null)
            {
                cover = command.cover;
                transform.LookAt(cover.transform);
                this.cover = cover;
                animator.SetBool("isCover", true);
                switch (cover.coverType)
                {
                    case CoverType.Half:
                        animator.SetBool("fullCover", false);
                        commandList.Remove(command);
                        gameMgr.SetPositionOfAI(ownerType);
                        break;
                    case CoverType.Full:
                        animator.SetBool("fullCover", true);
                        animator.SetBool("isRight", command.isRight);
                        coverPos = pos + (transform.forward * coverInterval);
                        covering = true;
                        break;
                    default:
                        break;
                }
                return;
            }
            else
            {
                var findCover = FindTargetDirectionCover();
                if (findCover != null)
                {
                    cover = findCover;
                }
                else if (Physics.Raycast(pos, transform.forward, out RaycastHit hit, DataUtility.nodeSize, gameMgr.coverLayer))
                {
                    cover = hit.collider.GetComponentInParent<Cover>();
                    if (cover == null || (cover != null && !currentNode.onAxisNodes.Contains(cover.coverNode)))
                    {
                        cover = SearchCoverOfOnAxisNode();
                    }
                }
                else
                {
                    cover = SearchCoverOfOnAxisNode();
                }
            }

            if (cover != null)
            {
                switch (cover.coverType)
                {
                    case CoverType.Half:
                        transform.LookAt(cover.transform);
                        animator.SetBool("isCover", true);
                        animator.SetBool("fullCover", false);
                        commandList.Remove(command);
                        gameMgr.SetPositionOfAI(ownerType);
                        break;
                    case CoverType.Full:
                        FindDirectionForCover(cover);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                commandList.Remove(command);
                gameMgr.SetPositionOfAI(ownerType);
            }
        }
        else
        {
            MoveCoverPosition(command);
        }

        Cover FindTargetDirectionCover()
        {
            var targetList = ownerType != CharacterOwner.Player ? gameMgr.playerList : gameMgr.enemyList;
            var closeList = targetList.FindAll(x => DataUtility.GetDistance(pos, x.currentNode.transform.position) < currentWeapon.weaponData.range);
            if (closeList.Count > 0)
            {
                var closeTarget = closeList.OrderBy(x => DataUtility.GetDistance(pos, x.currentNode.transform.position)).ToList()[0];
                var dir = closeTarget.currentNode.transform.position - pos;
                if (Physics.Raycast(pos, dir, out RaycastHit hit, DataUtility.nodeSize, gameMgr.coverLayer))
                {
                    var cover = hit.collider.GetComponentInParent<Cover>();
                    if (cover != null && currentNode.onAxisNodes.Contains(cover.coverNode))
                    {
                        return cover;
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
            else
            {
                return null;
            }
        }

        Cover SearchCoverOfOnAxisNode()
        {
            Cover cover = null;
            for (int i = 0; i < currentNode.onAxisNodes.Count; i++)
            {
                var onAxisNode = currentNode.onAxisNodes[i];
                if (onAxisNode == null) continue;

                if (onAxisNode.cover != null)
                {
                    cover = onAxisNode.cover;
                    break;
                }
                else if (currentNode.outlines[i].lineCover != null)
                {
                    cover = currentNode.outlines[i].lineCover;
                    break;
                }
            }

            return cover;
        }
    }

    /// <summary>
    /// 엄폐 후 캐릭터 방향 찾기
    /// </summary>
    /// <param name="coverNode"></param>
    private void FindDirectionForCover(Cover _cover)
    {
        transform.LookAt(_cover.transform);
        var pos = currentNode.transform.position;
        var rightHit = false;
        var leftHit = false;
        if (Physics.Raycast(pos, transform.right, DataUtility.nodeSize, gameMgr.nodeLayer))
        {
            rightHit = true;
        }
        if (Physics.Raycast(pos, -transform.right, DataUtility.nodeSize, gameMgr.nodeLayer))
        {
            leftHit = true;
        }

        var targetDir = FindCloseTargetDirection();
        if (rightHit || leftHit)
        {
            var rightCover = false;
            var leftCover = false;
            pos = currentNode.transform.position + (transform.right * DataUtility.nodeSize);
            if (Physics.Raycast(pos, transform.forward, DataUtility.nodeSize, gameMgr.coverLayer))
            {
                rightCover = true;
            }
            pos = currentNode.transform.position + (-transform.right * DataUtility.nodeSize);
            if (Physics.Raycast(pos, transform.forward, DataUtility.nodeSize, gameMgr.coverLayer))
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

        cover = _cover;
        coverPos = currentNode.transform.position + (transform.forward * coverInterval);
        covering = true;
        animator.SetBool("isCover", true);
        animator.SetBool("fullCover", true);

        TargetDirection FindCloseTargetDirection()
        {
            var targetList = ownerType != CharacterOwner.Player ? gameMgr.playerList : gameMgr.enemyList;
            var closeList = targetList.FindAll(x => DataUtility.GetDistance(pos, x.currentNode.transform.position) < currentWeapon.weaponData.range);
            if (closeList.Count > 0)
            {
                var closeTarget = closeList.OrderBy(x => DataUtility.GetDistance(pos, x.currentNode.transform.position)).ToList()[0];
                var dir = closeTarget.currentNode.transform.position - pos;
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
            //gameMgr.SetPositionOfAI(ownerType);
        }
    }

    /// <summary>
    /// 엄폐사격 후 재엄폐 처리
    /// </summary>
    /// <param name="command"></param>
    private void BackCoverProcess(CharacterCommand command)
    {
        if (cover.coverType != CoverType.Full)
        {
            commandList.Remove(command);
            return;
        }

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
    /// 캐릭터 조준상태 처리
    /// </summary>
    /// <param name="command"></param>
    private void TargetingProcess(CharacterCommand command)
    {
        if (!canTargeting)
        {
            switch (command.targeting)
            {
                case true:
                    if (animator.GetCurrentAnimatorStateInfo(baseIndex).IsTag("Idle") || animator.GetCurrentAnimatorStateInfo(baseIndex).IsTag("Cover"))
                    {
                        canTargeting = true;
                    }
                    break;
                case false:
                    if (animator.GetCurrentAnimatorStateInfo(baseIndex).IsTag("Idle") || animator.GetCurrentAnimatorStateInfo(baseIndex).IsTag("Targeting"))
                    {
                        canTargeting = true;
                    }
                    break;
            }
            return;
        }

        if (animator.GetBool("isCover") && !animator.GetBool("fullCover"))
        {
            switch (command.targeting)
            {
                case true:
                    aimTf = command.lookAt;
                    headAim = true;
                    animator.SetTrigger("targeting");
                    break;
                case false:
                    aimTf = null;
                    headAim = false;
                    animator.SetTrigger("unTargeting");
                    break;
            }
            canTargeting = false;
            commandList.Remove(command);
        }
        else if (animator.GetBool("isCover"))
        {
            if (!targetingMove)
            {
                switch (command.targeting)
                {
                    case true:
                        aimTf = command.lookAt;
                        headAim = true;
                        var moveDir = animator.GetBool("isRight") ? transform.right : -transform.right;
                        var moveDist = GetDistance();
                        targetingPos = transform.position + (moveDir * moveDist);
                        animator.SetTrigger("targeting");
                        break;
                    case false:
                        aimTf = null;
                        headAim = false;
                        targetingPos = currentNode.transform.position + (transform.forward * coverInterval);
                        animator.SetTrigger("unTargeting");
                        break;
                }
                targetingMove = true;
            }
            else
            {
                if (transform.position != targetingPos)
                {
                    var speed = GetSpeed();
                    transform.position = Vector3.MoveTowards(transform.position, targetingPos, speed * Time.deltaTime);
                }
                else
                {
                    canTargeting = false;
                    targetingMove = false;
                    commandList.Remove(command);
                }
            }
        }
        else
        {
            if (command.targeting)
            {
                transform.LookAt(command.lookAt);
            }
            canTargeting = false;
            commandList.Remove(command);
        }

        float GetDistance()
        {
            switch (currentWeapon.weaponData.type)
            {
                case WeaponType.Pistol:
                    return 0.45f;
                case WeaponType.Rifle:
                    return 0.7f;
                default:
                    return 0f;
            }
        }

        float GetSpeed()
        {
            switch (currentWeapon.weaponData.type)
            {
                case WeaponType.Pistol:
                    return targetingMoveSpeed_Pistol;
                case WeaponType.Rifle:
                    return targetingMoveSpeed_Rifle;
                default:
                    return 0f;
            }
        }
    }

    /// <summary>
    /// 조준 및 경계 처리
    /// </summary>
    /// <param name="command"></param>
    private void AimAndWatchProcess(CharacterCommand command)
    {
        switch (command.type)
        {
            case CommandType.Aim:
                if (command.targetInfo.shooterCover != null)
                {
                    CoverAim();
                }
                else
                {
                    NoneCoverAim();
                }
                break;
            case CommandType.Watch:
                if (watchInfo.shooterCover != null)
                {
                    CoverAim();
                }
                else
                {
                    NoneCoverAim();
                }
                break;
            default:
                break;
        }

        void CoverAim()
        {
            if (!covering)
            {
                animator.SetBool("coverAim", true);
                switch (command.type)
                {
                    case CommandType.Aim:
                        animator.SetBool("isRight", command.targetInfo.isRight);
                        //animator.SetInteger("shootNum", currentWeapon.GetShootBulletNumber());
                        coverPos = command.targetInfo.shooterNode.transform.position;
                        SetAiming(command.targetInfo);
                        break;
                    case CommandType.Watch:
                        animator.SetBool("isRight", watchInfo.isRight);
                        coverPos = watchInfo.watchNode.transform.position;
                        aimTf = watchInfo.targetNode.transform;
                        aimInterval = new Vector3(0f, DataUtility.aimPointY, 0f);
                        aimPoint.transform.position = aimTf.position + aimInterval;
                        break;
                    default:
                        break;
                }
                covering = true;
            }
            else if (animator.GetCurrentAnimatorStateInfo(baseIndex).IsTag("Aim"))
            {
                animator.SetBool("isAim", true);
                chestAim = true;
                if (transform.position != coverPos)
                {
                    transform.position = Vector3.MoveTowards(transform.position, coverPos, coverAimSpeed * Time.deltaTime);
                }
                else
                {
                    coverPos = currentNode.transform.position + (transform.forward * coverInterval);
                    commandList.Remove(command);
                    covering = false;
                    gameMgr.SetPositionOfAI(ownerType);
                }
            }
        }

        void NoneCoverAim()
        {
            animator.SetBool("isAim", true);
            switch (command.type)
            {
                case CommandType.Aim:
                    //animator.SetInteger("shootNum", currentWeapon.GetShootBulletNumber());
                    transform.LookAt(command.targetInfo.target.transform);
                    SetAiming(command.targetInfo);
                    break;
                case CommandType.Watch:
                    transform.LookAt(watchInfo.targetNode.transform);
                    aimTf = watchInfo.targetNode.transform;
                    break;
                default:
                    break;
            }
            aimInterval = new Vector3(0f, DataUtility.aimPointY, 0f);
            aimPoint.transform.position = aimTf.position + aimInterval;
            chestAim = true;
            chestRig.weight = 1f;
            commandList.Remove(command);
            gameMgr.SetPositionOfAI(ownerType);
        }
    }

    /// <summary>
    /// 조준점 설정
    /// </summary>
    /// <param name="targetInfo"></param>
    private void SetAiming(TargetInfo targetInfo)
    {
        aimTf = targetInfo.target.transform;
        if (currentWeapon.CheckHitBullet(targetInfo, animator.GetInteger("shootNum")))
        {
            var dir = System.Convert.ToBoolean(Random.Range(0, 2)) ? transform.right : -transform.right;
            var errorInterval = 1f;
            aimInterval = dir * errorInterval;
            aimInterval.y += DataUtility.aimPointY;
            //if (targetInfo.target.animator.GetCurrentAnimatorStateInfo(baseIndex).IsTag("Targeting"))
            //{
            //    targetInfo.target.AddCommand(CommandType.Targeting, false, transform);
            //}
        }
        else
        {
            aimInterval = new Vector3(0f, DataUtility.aimPointY, 0f);
        }
        aimPoint.transform.position = aimTf.position + aimInterval;
    }

    /// <summary>
    /// 캐릭터 사격 처리
    /// </summary>
    /// <param name="command"></param>
    private void ShootProcess(CharacterCommand command)
    {
        var shootNum = animator.GetInteger("shootNum");
        if (shootNum == 0) return;

        timer += Time.deltaTime;
        if (timer > aimTime && animator.GetCurrentAnimatorStateInfo(upperIndex).IsTag("Aim"))
        {
            if (!animator.GetBool("fireTrigger"))
            {
                animator.SetBool("fireTrigger", true);
            }
            //weapon.FireBullet();
            shootNum--;
            if (shootNum == 0)
            {
                //animator.SetBool("fireTrigger", false);
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
        if (!reloading && animator.GetCurrentAnimatorStateInfo(upperIndex).IsTag("None"))
        {
            animator.SetTrigger("reload");
            //currentWeapon.Reload();
            reloading = true;
        }
    }

    /// <summary>
    /// 캐릭터 무기교체 처리
    /// </summary>
    /// <param name="command"></param>
    private void ChangeWeaponProcess(CharacterCommand command)
    {
        if (!changing)
        {
            changeIndex = weapons.IndexOf(currentWeapon);
            changeIndex++;
            if (changeIndex == weapons.Count)
            {
                changeIndex = 0;
            }
            var nextWeapon = weapons[changeIndex];
            if (currentWeapon.weaponData.type != nextWeapon.weaponData.type)
            {
                animator.SetBool("otherType", true);
            }
            else
            {
                animator.SetBool("otherType", false);
            }
            animator.SetTrigger("change");
            changing = true;
        }
    }

    #region 시야 코드
    ///// <summary>
    ///// 시야에 있는 노드를 표시
    ///// </summary>
    ///// <param name="sight"></param>
    ///// <param name="node"></param>
    //public void ShowVisibleNodes(float sight, FieldNode node)
    //{
    //    if (ownerType != CharacterOwner.Player) return;

    //    SwitchVisibleNode(false);
    //    visibleNodes.Clear();
    //    var findNodes = gameMgr.fieldNodes.FindAll(x => DataUtility.GetDistance(x.transform.position, node.transform.position) < sight);
    //    for (int i = 0; i < findNodes.Count; i++)
    //    {
    //        var findNode = findNodes[i];
    //        var pos = node.transform.position;
    //        var targetPos = findNode.transform.position;
    //        if (!CheckSight())
    //        {
    //            visibleNodes.Add(findNode);
    //            continue;
    //        }

    //        for (int j = 0; j < node.onAxisNodes.Count; j++)
    //        {
    //            var onAxisNode = node.onAxisNodes[j];
    //            if (onAxisNode != null && onAxisNode.canMove)
    //            {
    //                pos = onAxisNode.transform.position;
    //                if (!CheckSight())
    //                {
    //                    CheckEnemy(findNode);
    //                    break;
    //                }
    //            }
    //        }

    //        bool CheckSight()
    //        {
    //            var dir = Vector3.Normalize(targetPos - pos);
    //            var dist = DataUtility.GetDistance(pos, targetPos);
    //            if (Physics.Raycast(pos, dir, out RaycastHit hit, dist, gameMgr.coverLayer))
    //            {
    //                var coverNode = hit.collider.GetComponentInParent<FieldNode>();
    //                if (coverNode != null && visibleNodes.Find(x => x == coverNode) == null)
    //                {
    //                    CheckEnemy(coverNode);
    //                }
    //                return true;
    //            }
    //            else
    //            {
    //                return false;
    //            }
    //        }
    //    }
    //    SwitchVisibleNode(true);

    //    void CheckEnemy(FieldNode node)
    //    {
    //        visibleNodes.Add(node);
    //        for (int i = 0; i < node.onAxisNodes.Count; i++)
    //        {
    //            var onAxisNode = node.onAxisNodes[i];
    //            if (onAxisNode == null) continue;

    //            if (onAxisNode.charCtr != null && onAxisNode.charCtr.ownerType != ownerType)
    //            {
    //                visibleNodes.Add(onAxisNode);
    //            }
    //        }
    //    }

    //    void SwitchVisibleNode(bool value)
    //    {
    //        for (int i = 0; i < visibleNodes.Count; i++)
    //        {
    //            var visibleNode = visibleNodes[i];
    //            visibleNode.SetVisibleNode(value);
    //        }
    //    }
    //}
    #endregion

    /// <summary>
    /// 행동력 설정
    /// </summary>
    /// <param name="value"></param>
    public void SetAction(int value)
    {
        action += value;
        if (action < 0)
        {
            action = 0;
        }
        else if (action > maxAction)
        {
            action = maxAction;
        }
        gameMgr.uiMgr.SetActionPoint_Bottom(this);
    }

    /// <summary>
    /// 방어구 내구값 설정
    /// </summary>
    /// <param name="value"></param>
    public void SetArmor(int value)
    {
        armor.durability += value;
        if (armor.durability < 0)
        {
            armor.durability = 0;
        }
        else if (armor.durability > armor.maxDurability)
        {
            armor.durability = armor.maxDurability;
        }
        charUI.SetCharacterValue();
    }

    /// <summary>
    /// 체력값 설정
    /// </summary>
    /// <param name="value"></param>
    public void SetHealth(int value)
    {
        health += value;
        if (health < 0)
        {
            health = 0;
        }
        else if (health > maxHealth)
        {
            health = maxHealth;
        }
        charUI.SetCharacterValue();
    }

    /// <summary>
    /// 활력값 설정
    /// </summary>
    /// <param name="value"></param>
    public void SetStamina(int value)
    {
        stamina += value;
        if (stamina < 0)
        {
            stamina = 0;
        }
        else if (stamina > maxStamina)
        {
            stamina = maxStamina;
        }
        charUI.SetCharacterValue();
    }

    public void PayTheMoveCost(int useAction)
    {
        SetAction(-useAction);
        SetStamina(-useAction * 5);
    }

    /// <summary>
    /// 경계자의 위치를 찾음
    /// </summary>
    /// <param name="targetNode"></param>
    public void SetWatchInfo(FieldNode targetNode, DrawRange drawRange)
    {
        FieldNode watchNode;
        var isRight = false;
        var shooterCover = FindCoverNode(currentNode, targetNode);
        if (shooterCover != null && shooterCover.coverType == CoverType.Full)
        {
            var RN = CheckTheCanMoveNode(currentNode, shooterCover, TargetDirection.Right);
            var LN = CheckTheCanMoveNode(currentNode, shooterCover, TargetDirection.Left);
            if (RN == null && LN == null)
            {
                watchNode = currentNode;
            }
            else if (LN == null)
            {
                watchNode = RN;
                isRight = true;
            }
            else if (RN == null)
            {
                watchNode = LN;
            }
            else if (DataUtility.GetDistance(RN.transform.position, targetNode.transform.position)
                  <= DataUtility.GetDistance(LN.transform.position, targetNode.transform.position))
            {
                watchNode = RN;
                isRight = true;
            }
            else
            {
                watchNode = LN;
            }

            var dir = Vector3.Normalize(targetNode.transform.position - watchNode.transform.position);
            if (Physics.Raycast(watchNode.transform.position, dir, DataUtility.nodeSize, gameMgr.coverLayer))
            {
                watchNode = currentNode;
            }
        }
        else
        {
            watchNode = currentNode;
        }

        var pos = watchNode.transform.position;
        var nodeAngleRad = Mathf.Atan2(targetNode.transform.position.x - pos.x, targetNode.transform.position.z - pos.z);
        var nodeAngle = (nodeAngleRad * Mathf.Rad2Deg + 360) % 360;
        var halfAngle = currentWeapon.weaponData.watchAngle / 2f;
        watchInfo = new WatchInfo()
        {
            drawRang = drawRange,
            targetNode = targetNode,
            watchNode = watchNode,
            shooterCover = shooterCover,
            isRight = isRight,
            minAngle = DataUtility.GetFloorValue((nodeAngle - halfAngle + 360f) % 360f, 2),
            maxAngle = DataUtility.GetFloorValue((nodeAngle + halfAngle) % 360f, 2),
        };
    }

    /// <summary>
    /// 경계자 체크
    /// </summary>
    /// <param name="currentNode"></param>
    public void CheckWatcher(FieldNode currentNode)
    {
        var watchers = ownerType != CharacterOwner.Player
                     ? gameMgr.playerList.FindAll(x => x.state == CharacterState.Watch)
                     : gameMgr.enemyList.FindAll(x => x.state == CharacterState.Watch);
        if (watchers.Count == 0) return;

        var pos = currentNode.transform.position;
        var interval = new Vector3(0f, 1f, 0f);
        for (int i = 0; i < watchers.Count; i++)
        {
            var watcher = watchers[i];
            var watchInfo = watcher.watchInfo;
            var angle = GetAngle(watchInfo);
            var angleRad = angle * Mathf.Deg2Rad;
            var dir = new Vector3(Mathf.Sin(angleRad), 0f, Mathf.Cos(angleRad)).normalized;
            var watchPos = watchInfo.watchNode.transform.position + interval;
            var range = watchInfo.drawRang.radius;
            var hits = Physics.RaycastAll(watchPos, dir, range, gameMgr.watchLayer).ToList();
            hits.Reverse();

            var shoot = false;
            for (int j = 0; j < hits.Count; j++)
            {
                var cover = hits[j].collider.GetComponentInParent<Cover>();
                if (cover != null)
                {
                    shoot = false;
                    break;
                }

                var check = hits[j].collider.GetComponentInParent<FieldNode>();
                if (check != null && check == currentNode)
                {
                    shoot = true;
                }
            }

            if (shoot)
            {
                switch (ownerType)
                {
                    case CharacterOwner.Player:
                        gameMgr.SetFireWarning(currentNode);
                        currentNode.hitNode = true;
                        currentNode.watcher = watcher;
                        break;
                    case CharacterOwner.Enemy:
                        watcher.AddCommand(CommandType.Shoot, this, currentNode);
                        break;
                    default:
                        break;
                }
            }
        }

        float GetAngle(WatchInfo watchInfo)
        {
            var watcherPos = watchInfo.watchNode.transform.position + interval;
            var angleRad = Mathf.Atan2(pos.x - watcherPos.x, pos.z - watcherPos.z);
            var angle = angleRad * Mathf.Rad2Deg + 360;
            if (watchInfo.minAngle > watchInfo.maxAngle)
            {
                if (angle >= watchInfo.minAngle && angle <= watchInfo.maxAngle + 360)
                {
                    angle %= 360;
                }
                else if (angle > watchInfo.maxAngle + 360)
                {
                    angle = watchInfo.maxAngle;
                }
                else
                {
                    angle = watchInfo.minAngle;
                }
            }
            else
            {
                angle %= 360;
                if (angle >= watchInfo.minAngle && angle <= watchInfo.maxAngle)
                {
                    return angle;
                }
                else if (angle > watchInfo.maxAngle)
                {
                    angle = watchInfo.maxAngle;
                }
                else
                {
                    angle = watchInfo.minAngle;
                }
            }

            return angle;
        }
    }

    /// <summary>
    /// 사격 가능한 타겟 찾음
    /// </summary>
    public void FindTargets(FieldNode node, bool noRange)
    {
        targetList.Clear();
        if (currentWeapon == null) return;

        var currentNode = node;
        var _targetList = ownerType != CharacterOwner.Player ? gameMgr.playerList.FindAll(x => x.state != CharacterState.Dead)
                                                             : gameMgr.enemyList.FindAll(x => x.state != CharacterState.Dead);
        for (int i = 0; i < _targetList.Count; i++)
        {
            var target = _targetList[i];
            var pos = currentNode.transform.position;
            var targetPos = target.currentNode.transform.position;
            var distance = DataUtility.GetDistance(pos, targetPos);
            FieldNode RN = null;
            FieldNode LN = null;
            FieldNode targetRN = null;
            FieldNode targetLN = null;
            var shooterCover = FindCoverNode(node, target.currentNode);
            var targetCover = FindCoverNode(target.currentNode, node);
            if (shooterCover != null && shooterCover.coverType == CoverType.Full
             && targetCover != null && targetCover.coverType == CoverType.Full)
            {
                RN = CheckTheCanMoveNode(node, shooterCover, TargetDirection.Right);
                LN = CheckTheCanMoveNode(node, shooterCover, TargetDirection.Left);
                if (RN == null && LN == null)
                {
                    Debug.Log($"{transform.name}: 사격할 공간이 없음(=> {target.name})");
                    continue;
                }
                targetRN = CheckTheCanMoveNode(target.currentNode, targetCover, TargetDirection.Right);
                targetLN = CheckTheCanMoveNode(target.currentNode, targetCover, TargetDirection.Left);
                if (targetRN == null && targetLN == null)
                {
                    Debug.Log($"{transform.name}: 적이 나올 공간이 없음(=> {target.name})");
                    continue;
                }

                if (!FindNodeOfShooterAndTarget())
                {
                    Debug.Log($"{transform.name}: 사격 불가(=> {target.name})");
                }
            }
            else if (shooterCover != null && shooterCover.coverType == CoverType.Full)
            {
                RN = CheckTheCanMoveNode(node, shooterCover, TargetDirection.Right);
                LN = CheckTheCanMoveNode(node, shooterCover, TargetDirection.Left);
                if (RN == null && LN == null)
                {
                    Debug.Log($"{transform.name}: 사격할 공간이 없음(=> {target.name})");
                    continue;
                }

                if (!FindNodeOfShooterAndTarget())
                {
                    Debug.Log($"{transform.name}: 사격 불가(=> {target.name})");
                }
            }
            else if (targetCover != null && targetCover.coverType == CoverType.Full)
            {
                targetRN = CheckTheCanMoveNode(target.currentNode, targetCover, TargetDirection.Right);
                targetLN = CheckTheCanMoveNode(target.currentNode, targetCover, TargetDirection.Left);
                if (targetRN == null && targetLN == null)
                {
                    Debug.Log($"{transform.name}: 적이 나올 공간이 없음(=> {target.name})");
                    continue;
                }

                if (!FindNodeOfShooterAndTarget())
                {
                    Debug.Log($"{transform.name}: 사격 불가(=> {target.name})");
                }
            }
            else
            {
                if (CheckTheCoverAlongPath(currentWeapon.weaponData.range, pos, targetPos, noRange))
                {
                    var targetInfo = new TargetInfo
                    {
                        shooter = this,
                        target = target,
                        shooterNode = currentNode,
                        shooterCover = shooterCover,
                        targetNode = target.currentNode,
                        targetCover = targetCover,
                    };
                    targetList.Add(targetInfo);
                }
                else
                {
                    Debug.Log($"{transform.name}: 사격 불가(=> {target.name})");
                }
            }

            bool FindNodeOfShooterAndTarget()
            {
                FieldNode shooterNode = null;
                FieldNode targetNode = null;
                bool isRight = false;
                bool targetRight = false;
                Vector3 pos;
                Vector3 targetPos;
                var distance = 999999f;

                if (shooterCover != null && shooterCover.coverType == CoverType.Full)
                {
                    if (RN != null)
                    {
                        if (targetCover != null && targetCover.coverType == CoverType.Full)
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
                        else
                        {
                            CheckNodes(RN, target.currentNode, true, false);
                        }
                    }
                    if (LN != null)
                    {
                        if (targetCover != null && targetCover.coverType == CoverType.Full)
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
                        else
                        {
                            CheckNodes(LN, target.currentNode, false, false);
                        }
                    }
                }
                else
                {
                    if (targetCover != null && targetCover.coverType == CoverType.Full)
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
                        CheckNodes(currentNode, target.currentNode, true, false);
                    }
                }


                if (shooterNode != null && targetNode != null)
                {
                    var targetInfo = new TargetInfo
                    {
                        shooter = this,
                        target = target,
                        shooterNode = shooterNode,
                        shooterCover = shooterCover,
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
                    if (CheckTheCoverAlongPath(currentWeapon.weaponData.range, pos, targetPos, noRange))
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
        }
    }

    /// <summary>
    /// 엄폐물 노드를 찾음
    /// </summary>
    /// <param name="shooterNode"></param>
    /// <param name="targetNode"></param>
    /// <returns></returns>
    private Cover FindCoverNode(FieldNode shooterNode, FieldNode targetNode)
    {
        Cover cover = null;
        var shooterPos = shooterNode.transform.position;
        var endPos = targetNode.transform.position;
        var dir = Vector3.zero;
        RayCastOfCoverLayer();

        var rightDir = Quaternion.Euler(0f, 90f, 0f) * dir;
        var interval = rightDir * 0.4f;
        shooterPos = shooterNode.transform.position + interval;
        endPos = targetNode.transform.position + interval;
        RayCastOfCoverLayer();

        var leftDir = Quaternion.Euler(0f, -90f, 0f) * dir;
        interval = leftDir * 0.4f;
        shooterPos = shooterNode.transform.position + interval;
        endPos = targetNode.transform.position + interval;
        dir = Vector3.Normalize(endPos - shooterPos);
        RayCastOfCoverLayer();

        return cover;

        void RayCastOfCoverLayer()
        {
            if (cover != null) return;

            dir = Vector3.Normalize(endPos - shooterPos);
            if (Physics.Raycast(shooterPos, dir, out RaycastHit hit, DataUtility.nodeSize, gameMgr.coverLayer))
            {
                var _cover = hit.collider.GetComponentInParent<Cover>();
                if (_cover == null) return;
                //if (_cover.coverType == CoverType.Half)
                //{
                //    cover = _cover;
                //    return;
                //}

                //switch (_cover.formType)
                //{
                //    case CoverForm.Node:
                //        if (!shooterNode.onAxisNodes.Contains(_cover.coverNode)) return;

                //        for (int i = 0; i < _cover.coverNode.allAxisNodes.Count; i++)
                //        {
                //            var axisNode = _cover.coverNode.allAxisNodes[i];
                //            if (axisNode == null) continue;
                //            if (axisNode == shooterNode) continue;

                //            if (axisNode.nodePos.x != shooterNode.nodePos.x && axisNode.nodePos.y != shooterNode.nodePos.y
                //             && axisNode.cover != null) return;
                //        }
                //        break;
                //    case CoverForm.Line:
                //        if (!shooterNode.outlines.Find(x => x.lineCover == _cover)) return;

                //        var _coverNode = _cover.frontNode != shooterNode ? _cover.frontNode : _cover.backNode;
                //        for (int i = 0; i < _coverNode.allAxisNodes.Count; i++)
                //        {
                //            var axisNode = _coverNode.allAxisNodes[i];
                //            if (axisNode == null) continue;
                //            if (axisNode == shooterNode) continue;

                //            if (axisNode.nodePos.x != shooterNode.nodePos.x && axisNode.nodePos.y != shooterNode.nodePos.y
                //             && axisNode.cover != null) return;
                //        }
                //        break;
                //    default:
                //        break;
                //}
                //cover = _cover;

                if (shooterNode.onAxisNodes.Contains(_cover.coverNode) || shooterNode.outlines.Find(x => x.lineCover == _cover))
                {
                    cover = _cover;
                }
            }
        }
    }

    /// <summary>
    /// 이동가능한 노드를 체크
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="coverNode"></param>
    /// <param name="targetDir"></param>
    /// <returns></returns>
    private FieldNode CheckTheCanMoveNode(FieldNode node, Cover cover, TargetDirection targetDir)
    {
        FieldNode moveNode = null;
        var frontDir = Vector3.Normalize(cover.transform.position - node.transform.position);
        var dir = targetDir == TargetDirection.Right ? Quaternion.Euler(0, 90f, 0) * frontDir : Quaternion.Euler(0, -90f, 0) * frontDir;
        RaycastHit hit;
        if (Physics.Raycast(node.transform.position, dir, out hit, DataUtility.nodeSize, gameMgr.nodeLayer))
        {
            moveNode = hit.collider.GetComponentInParent<FieldNode>();
            if (!moveNode.canMove)
            {
                moveNode = null;
            }
        }
        if (moveNode == null) return null;

        if (Physics.Raycast(moveNode.transform.position, frontDir, out hit, DataUtility.nodeSize, gameMgr.coverLayer))
        {
            var _cover = hit.collider.GetComponentInParent<Cover>();
            if (_cover.coverType == CoverType.Full) return null;
        }
        //switch (cover.formType)
        //{
        //    case CoverForm.Node:

        //        for (int i = 0; i < moveNode.offAxisNodes.Count; i++)
        //        {
        //            var axisNode = moveNode.offAxisNodes[i];
        //            if (axisNode == null) continue;

        //            var checkNode = cover.coverNode.allAxisNodes.Find(x => x != node && x.allAxisNodes.Contains(moveNode));
        //            if (checkNode != null && checkNode.cover != null) return null;
        //        }
        //        break;
        //    case CoverForm.Line:
        //        var _node = cover.frontNode != node ? cover.frontNode : cover.backNode;
        //        for (int i = 0; i < _node.allAxisNodes.Count; i++)
        //        {
        //            var axisNode = _node.offAxisNodes[i];
        //            if (axisNode == null) continue;

        //            var checkNode = cover.coverNode.allAxisNodes.Find(x => x != _node && x.allAxisNodes.Contains(axisNode));
        //            if (checkNode != null && checkNode.cover != null) return null;
        //        }
        //        break;
        //    default:
        //        break;
        //}

        return moveNode;
    }

    /// <summary>
    /// 사격경로 상의 장애물을 체크
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="targetPos"></param>
    /// <returns></returns>
    public bool CheckTheCoverAlongPath(float range, Vector3 pos, Vector3 targetPos, bool noRange)
    {
        var dist = DataUtility.GetDistance(pos, targetPos);
        var isRange = noRange || dist <= range;
        if (!isRange) return false;

        bool canShoot;
        var interval = new Vector3(0f, 1f, 0f);
        pos += interval;
        targetPos += interval;
        var dir = Vector3.Normalize(targetPos - pos);
        if (Physics.Raycast(pos, dir, dist, gameMgr.coverLayer))
        {
            canShoot = false;
        }
        else
        {
            canShoot = true;
        }

        return canShoot;
    }

    /// <summary>
    /// 타겟을 설정
    /// </summary>
    public bool SetTargetOn()
    {
        if (targetList.Count == 0)
        {
            Debug.Log($"{transform.name}: No Target");
            return false;
        }
        else
        {
            targetIndex = 0;
            fiarRate = 0;
            sightRate = 0;
            var targetInfo = targetList[targetIndex];
            targetInfo.target.SetActiveOutline(true);
            SetTargeting(targetInfo, CharacterOwner.All);
            CameraState camState;
            if (targetInfo.shooterCover == null)
            {
                camState = CameraState.RightAim;
            }
            else if (targetInfo.shooterCover.coverType == CoverType.Half)
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
            gameMgr.uiMgr.SetUsedActionPoint_Bottom(this, currentWeapon.weaponData.actionCost);
            gameMgr.uiMgr.SetActiveAimUI(this, true);
            gameMgr.uiMgr.SetTargetInfo(targetInfo);
            return true;
        }
    }

    /// <summary>
    /// 다음 타겟을 설정
    /// </summary>
    public void SetNextTargetOn()
    {
        if (targetList.Count < 2) return;

        var prevTargetInfo = targetList[targetIndex];
        prevTargetInfo.target.SetActiveOutline(false);
        prevTargetInfo.target.AddCommand(CommandType.Targeting, false, transform);
        targetIndex++;
        if (targetIndex == targetList.Count)
        {
            targetIndex = 0;
        }
        //ChangeTargetShader();

        var targetInfo = targetList[targetIndex];
        targetInfo.target.SetActiveOutline(true);
        SetTargeting(targetInfo, CharacterOwner.All);
        CameraState camState;
        if (cover == null)
        {
            camState = CameraState.RightAim;
        }
        else if (cover.coverType == CoverType.Half)
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
        gameMgr.uiMgr.SetTargetInfo(targetInfo);
    }

    /// <summary>
    /// 조준상태로 설정
    /// </summary>
    /// <param name="value"></param>
    public void SetTargeting(TargetInfo targetInfo, CharacterOwner ownerType)
    {
        var shooter = targetInfo.shooter;
        var target = targetInfo.target;

        if (ownerType == CharacterOwner.All || target.ownerType == ownerType)
        {
            if (target.cover != null)
            {
                if (targetInfo.targetCover == null)
                {
                    target.AddCommand(CommandType.LeaveCover);
                }
                else if (targetInfo.targetCover != null && targetInfo.targetCover != target.cover)
                {
                    target.AddCommand(CommandType.LeaveCover);
                    target.AddCommand(CommandType.TakeCover, targetInfo.targetCover, targetInfo.targetRight);
                }
                else
                {
                    //target.animator.SetBool("isRight", targetInfo.targetRight);
                    target.AddCommand(CommandType.TakeCover, targetInfo.targetCover, targetInfo.targetRight);
                }
            }
            else if (targetInfo.targetCover != null)
            {
                target.AddCommand(CommandType.TakeCover, targetInfo.targetCover, targetInfo.targetRight);
            }
            else
            {
                target.transform.LookAt(shooter.transform);
            }
            target.AddCommand(CommandType.Targeting, true, shooter.transform);
        }

        if (ownerType == CharacterOwner.All || shooter.ownerType == ownerType)
        {
            if (shooter.cover != null)
            {
                if (targetInfo.shooterCover == null)
                {
                    shooter.AddCommand(CommandType.LeaveCover);
                }
                else if (targetInfo.shooterCover != null && targetInfo.shooterCover != shooter.cover)
                {
                    shooter.AddCommand(CommandType.LeaveCover);
                    shooter.AddCommand(CommandType.TakeCover, targetInfo.shooterCover, targetInfo.isRight);
                }
                else
                {
                    //shooter.animator.SetBool("isRight", targetInfo.isRight);
                    shooter.AddCommand(CommandType.TakeCover, targetInfo.shooterCover, targetInfo.isRight);
                }
            }
            else if (targetInfo.shooterCover != null)
            {
                shooter.AddCommand(CommandType.TakeCover, targetInfo.shooterCover, targetInfo.isRight);
            }
            else
            {
                shooter.transform.LookAt(target.transform);
            }
        }
    }

    /// <summary>
    /// 조준상태를 해제
    /// </summary>
    public void SetTargetOff()
    {
        targetList[targetIndex].target.SetActiveOutline(false);
        gameMgr.uiMgr.SetActiveAimUI(this, false);
    }

    /// <summary>
    /// 경계상태로 설정
    /// </summary>
    public void SetWatch()
    {
        if (currentWeapon == null) return;
        if (action < currentWeapon.weaponData.actionCost) return;

        SetAction(-currentWeapon.weaponData.actionCost);
        if (animator.GetBool("isCover"))
        {
            if (watchInfo.shooterCover == null)
            {
                AddCommand(CommandType.LeaveCover);
            }
            else if (watchInfo.shooterCover != null && watchInfo.shooterCover != cover)
            {
                AddCommand(CommandType.LeaveCover);
                AddCommand(CommandType.TakeCover, watchInfo.shooterCover, watchInfo.isRight);
            }
            AddCommand(CommandType.Watch);
        }
        else
        {
            if (watchInfo.shooterCover != null)
            {
                AddCommand(CommandType.TakeCover, watchInfo.shooterCover, watchInfo.isRight);
            }
            AddCommand(CommandType.Watch);
        }
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
                    indexName = $"{type}",
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
    /// <param name="movePass"></param>
    public void AddCommand(CommandType type, List<FieldNode> movePass)
    {
        switch (type)
        {
            case CommandType.Move:
                currentNode.canMove = true;
                currentNode.charCtr = null;
                var targetNode = movePass[0];
                targetNode.canMove = false;
                targetNode.charCtr = this;
                PayTheMoveCost(targetNode.moveCost);

                var moveCommand = new CharacterCommand
                {
                    indexName = $"{type}",
                    type = CommandType.Move,
                    movePass = new List<FieldNode>(movePass),
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
    public void AddCommand(CommandType type, Cover cover, bool isRight)
    {
        switch (type)
        {
            case CommandType.TakeCover:
                var takeCoverCommand = new CharacterCommand
                {
                    indexName = $"{type}",
                    type = CommandType.TakeCover,
                    cover = cover,
                    isRight = isRight,
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
    /// <param name="targeting"></param>
    /// <param name="lookAt"></param>
    public void AddCommand(CommandType type, bool targeting, Transform lookAt)
    {
        switch (type)
        {
            case CommandType.Targeting:
                var targetingCommand = new CharacterCommand
                {
                    indexName = $"{type}",
                    type = CommandType.Targeting,
                    targeting = targeting,
                    lookAt = lookAt,
                };
                commandList.Add(targetingCommand);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 커맨드 추가
    /// </summary>
    /// <param name="target"></param>
    public void AddCommand(CommandType type, CharacterController target, FieldNode targetNode)
    {
        switch (type)
        {
            case CommandType.Shoot:
                //if (currentWeapon.weaponData.chamberBullet == null || currentWeapon.weaponData.chamberBullet.level == 0) return;
                if (currentWeapon == null) return;
                if (!currentWeapon.weaponData.isMag) return;
                if (currentWeapon.weaponData.equipMag.loadedBullets.Count == 0) return;

                target.animator.speed = 0f;
                target.pause = true;
                var targetInfo = new TargetInfo()
                {
                    shooter = this,
                    target = target,
                    shooterNode = watchInfo.watchNode,
                    shooterCover = watchInfo.shooterCover,
                    targetNode = targetNode,
                    targetCover = null,
                    isRight = watchInfo.isRight,
                };
                //animator.SetInteger("shootNum", currentWeapon.GetShootBulletNumber());
                SetAiming(targetInfo);

                var shootCommand = new CharacterCommand
                {
                    indexName = $"{type}",
                    type = CommandType.Shoot,
                    targetInfo = targetInfo,
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
            case CommandType.Aim:
                var coverAimCommand = new CharacterCommand
                {
                    indexName = $"{type}",
                    type = CommandType.Aim,
                    targetInfo = targetList[targetIndex],
                };
                commandList.Add(coverAimCommand);
                break;
            case CommandType.Shoot:
                var shootCommand = new CharacterCommand
                {
                    indexName = $"{type}",
                    type = CommandType.Shoot,
                    targetInfo = targetList[targetIndex],
                };
                commandList.Add(shootCommand);
                break;
            default:
                var command = new CharacterCommand
                {
                    indexName = $"{type}",
                    type = type,
                };
                commandList.Add(command);
                break;
        }
    }

    public void AddCommand(CommandType type, TargetInfo targetInfo)
    {
        switch (type)
        {
            case CommandType.Aim:
                var coverAimCommand = new CharacterCommand
                {
                    indexName = $"{type}",
                    type = CommandType.Aim,
                    targetInfo = targetInfo,
                };
                commandList.Add(coverAimCommand);
                break;
            case CommandType.Shoot:
                var shootCommand = new CharacterCommand
                {
                    indexName = $"{type}",
                    type = CommandType.Shoot,
                    targetInfo = targetInfo,
                };
                commandList.Add(shootCommand);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 캐릭터 피격
    /// </summary>
    public void OnHit(Vector3 dir, BulletDataInfo _bulletData)
    {
        if (state == CharacterState.Dead) return;

        bool isPenetrate;
        float bulletproof;
        if (armor != null && armor.durability > 0)
        {
            var value = Random.Range(0, 100);
            armor.bulletproof = Mathf.Floor(((121 - (5000 / (45 + ((float)armor.durability / armor.maxDurability) * 200))) / 100 * armor.bulletproof) * 10f) / 10f;
            if (armor.bulletproof < 0f)
            {
                armor.bulletproof = 0f;
            }

            var penetrateRate = _bulletData.penetrate <= armor.bulletproof ? (int)Mathf.Floor(0.9f - (0.1f * (armor.bulletproof - _bulletData.penetrate)) * 100f) : 95;
            isPenetrate = value < penetrateRate;

            var armorDamage = (int)Mathf.Floor(_bulletData.damage * (_bulletData.armorBreak / 100f));
            SetArmor(-armorDamage);
            bulletproof = armor.bulletproof;
        }
        else
        {
            isPenetrate = true;
            bulletproof = 0f;
        }

        var damage = (int)Mathf.Floor(_bulletData.damage * (_bulletData.penetrate / (_bulletData.penetrate + bulletproof)));
        if (isPenetrate)
        {
            SetHealth(-damage);
            gameMgr.SetFloatText(charUI.transform.position, $"{damage}", Color.white);
        }
        else
        {
            SetStamina(-damage);
        }

        if (health == 0)
        {
            CharacterDead();
        }
        else if (health > 0 && !animator.GetCurrentAnimatorStateInfo(baseIndex).IsTag("Hit") && !animator.GetBool("isMove"))
        {
            animator.SetTrigger("isHit");
        }

        void CharacterDead()
        {
            //DataUtility.SetMeshsMaterial(meshs, "Standard");
            //DataUtility.SetMeshsMaterial(sMeshs, "Standard");
            //DataUtility.SetMeshsMaterial(currentWeapon.meshs, "Standard");
            animator.enabled = false;
            cd.enabled = false;
            headAim = false;
            chestAim = false;
            headRig.weight = 0f;
            chestRig.weight = 0f;
            currentNode.charCtr = null;
            currentNode.canMove = true;
            var charList = ownerType == CharacterOwner.Player ? gameMgr.playerList : gameMgr.enemyList;
            //charList.Remove(this);

            var force = 500f;
            for (int i = 0; i < ragdollCds.Count; i++)
            {
                var cd = ragdollCds[i];
                cd.isTrigger = false;
            }
            for (int i = 0; i < ragdollRbs.Count; i++)
            {
                var rb = ragdollRbs[i];
                rb.isKinematic = false;
                rb.AddForce(dir * force, ForceMode.Force);
            }
            state = CharacterState.Dead;
            charUI.gameObject.SetActive(false);
            //Destroy(charUI.gameObject);

            if (charList.Find(x => x.state != CharacterState.Dead) == null)
            {
                StartCoroutine(gameMgr.Coroutine_GameEnd());
            }
        }
    }

    /// <summary>
    /// 조준 해제
    /// </summary>
    public void AimOff()
    {
        aimTf = null;
        chestAim = false;
        animator.SetBool("isAim", false);
        animator.SetBool("coverAim", false);
        timer = 0f;
    }

    public void AddWeapon(ItemHandler item)
    {
        var weapon = weaponPool.Find(x => x.name == item.weaponData.weaponName);
        weapon.SetComponets(this, item.weaponData);
        if (currentWeapon == null)
        {
            weapon.WeaponSwitching("Right");
            weapon.EquipWeapon();
            currentWeapon = weapon;
        }
        else
        {
            weapon.WeaponSwitching("Holster");
        }
    }

    public void RemoveWeapon(string weaponName)
    {
        var weapon = weaponPool.Find(x => x.name == weaponName);
        if (currentWeapon == weapon)
        {
            currentWeapon = null;
        }
        weapon.transform.SetParent(weaponPoolTf, false);
        weapon.Initialize();
    }

    public Weapon GetWeapon(string weaponName)
    {
        return weaponPool.Find(x => x.name == weaponName);
    }

    #region Coroutine
    /// <summary>
    /// (코루틴)조준 해제
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    private IEnumerator Coroutine_AimOff(CharacterCommand command)
    {
        yield return new WaitForSeconds(aimTime);

        animator.SetBool("fireTrigger", false);
        var target = command.targetInfo.target;
        switch (state)
        {
            case CharacterState.None:
                if (target.ownerType != CharacterOwner.Player && target.animator.GetCurrentAnimatorStateInfo(baseIndex).IsTag("Targeting"))
                {
                    target.AddCommand(CommandType.Targeting, false, transform);
                }
                AimOff();
                break;
            case CharacterState.Watch:
                aimTf = command.targetInfo.targetNode.transform;
                var find = commandList.Find(x => x.type == CommandType.Shoot && x != command);
                if (find == null)
                {
                    var charList = ownerType != CharacterOwner.Player ? gameMgr.playerList : gameMgr.enemyList;
                    for (int i = 0; i < charList.Count; i++)
                    {
                        var charCtr = charList[i];
                        if (!charCtr.pause) continue;

                        charCtr.animator.speed = 1f;
                        charCtr.pause = false;
                    }
                }
                break;
            default:
                break;
        }
        commandList.Remove(command);
    }
    #endregion

    #region Animation Event
    /// <summary>
    /// (애니메이션 이벤트)무기 사격
    /// </summary>
    public void Event_FireWeapon()
    {
        var shootCommand = commandList[0];
        if (shootCommand.type != CommandType.Shoot)
        {
            Debug.LogError("Event_FireWeapon: shootCommand is not [CommandType.Shoot]");
            return;
        }

        var target = shootCommand.targetInfo.target;
        currentWeapon.FireBullet(target);
        var shootNum = animator.GetInteger("shootNum");
        shootNum--;
        animator.SetInteger("shootNum", shootNum);
    }

    /// <summary>
    /// (애니메이션 이벤트)무기 위치 변경
    /// </summary>
    /// <param name="switchPos"></param>
    public void Event_WeaponSwitching(string switchPos)
    {
        currentWeapon.WeaponSwitching(switchPos);
    }

    public void Event_WeaponSwitching_Rifle(string switchPos)
    {
        if (!animator.GetBool("otherType")) return;

        currentWeapon.WeaponSwitching(switchPos);
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
        gameMgr.SetPositionOfAI(ownerType);
    }

    public void Event_WeaponChange()
    {
        currentWeapon.WeaponSwitching("Holster");
        currentWeapon = weapons[changeIndex];
        currentWeapon.WeaponSwitching("Right");
    }

    public void Event_WeaponChange_OrderType()
    {
        if (!animator.GetBool("otherType")) return;

        currentWeapon = weapons[changeIndex];
        currentWeapon.EquipWeapon();
        animator.SetTrigger("change");
        animator.SetTrigger("isCut");
    }

    public void Event_ChangeEnd()
    {
        if (commandList.Count == 0)
        {
            Debug.LogError("No Command in the CommanderList");
            return;
        }
        else if (commandList[0].type != CommandType.ChangeWeapon)
        {
            Debug.LogError("CommandType is not ChangeWeapon");
            return;
        }

        commandList.RemoveAt(0);
        changing = false;
    }
    #endregion

    public GameManager GameMgr
    {
        private set { gameMgr = value; }
        get { return gameMgr; }
    }
}
