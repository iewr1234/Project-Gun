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
    Base,
}

public enum CommandType
{
    None,
    Wait,
    Move,
    MoveChange,
    TakeCover,
    LeaveCover,
    BackCover,
    Targeting,
    Aim,
    Watch,
    ThrowAim,
    Shoot,
    Reload,
    ChangeWeapon,
    Throw,
}

public enum ShootingMode
{
    PointShot,
    AimShot,
    SightShot,
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
    public FieldNode endNode;
    public FieldNode targetNode;

    [Header("[Cover]")]
    public Cover cover;
    public bool isRight;

    [Header("[Targeting]")]
    public bool targeting;
    public Transform lookAt;

    [Header("[Shoot]")]
    public TargetInfo targetInfo;

    [Header("[Reload]")]
    public bool isReload;
    public bool loadChamber;
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

[System.Serializable]
public struct ThrowInfo
{
    public GameObject grenade;
    public FieldNode targetNode;
    public FieldNode throwNode;
    public Cover throwerCover;
    public bool isRight;
    public List<CharacterController> targetList;
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
    public GrenadeHandler grenadeHlr;
    public Armor armor;
    [Space(5f)]

    public DropTableDataInfo dropTableData;
    public ItemDataInfo uniqueItemData;

    [Header("---Access Component---")]
    public Animator animator;
    public Outlinable outlinable;
    [SerializeField] private Collider cd;

    private RigBuilder rigBdr;
    private Rig rig;
    private MultiAimConstraint headRig;
    private MultiAimConstraint chestRig;
    [HideInInspector] public ChainIKConstraint chainIK;

    public Transform aimPoint;
    public Transform aimTf;

    [HideInInspector] public Transform mainHolsterPivot;
    [HideInInspector] public Transform subHolsterPivot;
    [Space(5f)] public Transform rightHandPivot;
    public Transform leftHandPivot;
    //public Transform gripPivot;

    [HideInInspector] public List<MeshRenderer> meshs = new List<MeshRenderer>();
    [HideInInspector] public List<SkinnedMeshRenderer> sMeshs = new List<SkinnedMeshRenderer>();
    private List<Collider> ragdollCds = new List<Collider>();
    private List<Rigidbody> ragdollRbs = new List<Rigidbody>();

    //[Space(5f)] public Transform weaponPoolTf;
    //public List<Weapon> weaponPool = new List<Weapon>();

    [Header("--- Assignment Variable---")]
    [Tooltip("사용자 타입")] public CharacterOwner ownerType;
    [Tooltip("캐릭터 상태")] public CharacterState state;
    [SerializeField] private bool turnEnd;

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

    [Header("[Ability]")]
    public int shootingMode_point;
    public int shootingMode_aim;
    public int shootingMode_sight;
    [Tooltip("발사속도")] public int RPM;
    [Tooltip("사거리")] public float range;
    [Tooltip("경계각")] public int watchAngle;
    [Tooltip("정확도")] public float MOA;
    [Tooltip("안정성")] public int stability;
    [Tooltip("반동")] public int rebound;
    [Tooltip("장약")] public int propellant;
    [Tooltip("피해량")] public int damage;
    [Tooltip("관통")] public int penetrate;
    [Tooltip("방어구 손상")] public int armorBreak;
    [Tooltip("파편화")] public int critical;
    public Ability ability = new Ability();
    public Ability addAbility = new Ability();

    [HideInInspector] public int baseIndex;
    [HideInInspector] public int upperIndex;

    [Space(5f)]
    public Weapon currentWeapon;
    [HideInInspector] public int fiarRate;
    [HideInInspector] public ShootingMode sMode;

    public FieldNode currentNode;
    private FieldNode prevNode;
    public Cover currentCover;
    [HideInInspector] public bool isCopy;

    [Space(5f)]
    public AIDataInfo aiData;
    //private List<FieldNode> visibleNodes = new List<FieldNode>();

    [Space(5f)]
    public List<TargetInfo> targetList = new List<TargetInfo>();
    [HideInInspector] public int targetIndex;

    [Space(5f)]
    public WatchInfo watchInfo;
    public ThrowInfo throwInfo;
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


    private Vector3 aimInterval;
    private bool headAim;
    private bool chestAim;
    private readonly float aimSpeed = 2f;
    private readonly float aimTime = 0.25f;

    private bool canTargeting;
    private bool targetingMove;
    private Vector3 targetingPos;
    private readonly float targetingMoveSpeed_main = 1.5f;
    private readonly float targetingMoveSpeed_sub = 1.5f;

    private bool reloading;
    private bool changing;
    private int changeIndex;

    private bool throwing;

    private Coroutine curCoroutine;

    //private bool weightCheck;
    //private bool runInstantly;
    [HideInInspector] public bool moveGripPivot;
    [HideInInspector] public float targetWeight;

    /// <summary>
    /// 구성요소 설정
    /// </summary>
    /// <param name="_gameMgr"></param>
    /// <param name="_ownerType"></param>
    /// <param name="charData"></param>
    /// <param name="_currentNode"></param>
    public void SetComponents(GameManager _gameMgr, CharacterOwner _ownerType, object charData, FieldNode _currentNode)
    {
        gameMgr = _gameMgr;

        // Grenade Pool 설정
        var grenadePool = Instantiate(Resources.Load<GrenadeHandler>("Prefabs/Weapon/GrenadePool"));
        grenadePool.transform.SetParent(transform, false);
        grenadeHlr = grenadePool.GetComponent<GrenadeHandler>();
        grenadeHlr.SetComponents(this);

        // Animator, Outlinable, Collider 설정
        animator = GetComponent<Animator>();
        outlinable = this.AddComponent<Outlinable>();
        cd = GetComponent<Collider>();

        // Aim과 IK 설정
        aimPoint = transform.Find("AimPoint");

        rigBdr = GetComponent<RigBuilder>();
        rig = transform.Find("Rig").GetComponent<Rig>();
        headRig = transform.Find("Rig/HeadAim").GetComponent<MultiAimConstraint>();
        headRig.weight = 0f;
        chestRig = transform.Find("Rig/ChestAim").GetComponent<MultiAimConstraint>();
        chestRig.weight = 0f;
        chainIK = transform.Find("Rig/Chain_IK").GetComponent<ChainIKConstraint>();

        mainHolsterPivot = transform.Find("Root/Hips/Spine_01/Spine_02");
        subHolsterPivot = transform.Find("Root/Hips/UpperLeg_R/Holster");

        ownerType = _ownerType;

        // Mesh와 Ragdoll 설정
        meshs = transform.GetComponentsInChildren<MeshRenderer>().ToList();
        sMeshs = transform.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
        ragdollCds = transform.Find("Root").GetComponentsInChildren<Collider>().ToList();
        foreach (var cd in ragdollCds)
        {
            cd.gameObject.layer = LayerMask.NameToLayer("BodyParts");
            cd.isTrigger = true;
        }
        ragdollRbs = transform.Find("Root").GetComponentsInChildren<Rigidbody>().ToList();
        foreach (var rb in ragdollRbs)
        {
            rb.isKinematic = true;
        }

        // 이름 설정
        var name = transform.name.Split(' ', '(', ')')[0];
        if (_ownerType == CharacterOwner.Player)
        {
            transform.name = $"{name}_{gameMgr.playerList.Count}";
            gameMgr.playerList.Add(this);
        }
        else
        {
            transform.name = $"{name}_{gameMgr.enemyList.Count}";
            gameMgr.enemyList.Add(this);
        }

        // 데이터 설정
        if (charData is PlayerDataInfo playerData)
        {
            // PlayerDataInfo 속성 설정
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

            ability.charCtr = this;
            addAbility.charCtr = this;
            ability.SetAbility(playerData);
        }
        else if (charData is EnemyDataInfo enemyData)
        {
            // EnemyDataInfo 속성 설정
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

            ability.charCtr = this;
            addAbility.charCtr = this;
            addAbility.ResetShootingModeInfos();
            ability.SetAbility(enemyData);

            // 적 전용 속성 설정
            if (enemyData.dropTableID != "None")
                dropTableData = gameMgr.dataMgr.dropTableData.dropTableInfo.Find(x => x.ID == enemyData.dropTableID);
            if (enemyData.uniqueItemID != "None")
                uniqueItemData = gameMgr.dataMgr.itemData.itemInfos.Find(x => x.ID == enemyData.uniqueItemID);

            aiData = gameMgr.dataMgr.aiData.aiInfos.Find(x => x.ID == enemyData.AI_ID);
        }

        baseIndex = 1;
        upperIndex = 2;

        // FieldNode 설정
        currentNode = _currentNode;
        currentNode.charCtr = this;
        currentNode.canMove = false;
    }

    /// <summary>
    /// Rig 설정
    /// </summary>
    /// <param name="type"></param>
    public void SetRig(Weapon weapon)
    {
        switch (weapon.weaponData.weaponType)
        {
            case WeaponType.Pistol:
                chestRig.data.offset = new Vector3(-10.7f, 0f, 17.7f);
                break;
            case WeaponType.Revolver:
                chestRig.data.offset = new Vector3(-10.7f, 0f, 17.7f);
                break;
            case WeaponType.SubMachineGun:
                chestRig.data.offset = new Vector3(-36.17f, 0f, 4.5f);
                break;
            case WeaponType.AssaultRifle:
                chestRig.data.offset = new Vector3(-35.9f, 0f, 5f);
                break;
            case WeaponType.SniperRifle:
                chestRig.data.offset = new Vector3(-49.75f, 0f, 16.27f);
                break;
            case WeaponType.Shotgun:
                chestRig.data.offset = new Vector3(-36.3f, 0f, 5f);
                break;
            default:
                break;
        }
        moveGripPivot = true;
        targetWeight = 1f;
    }

    public void SetWeaponPivot(WeaponGripInfo gripInfo)
    {
        rightHandPivot.SetLocalPositionAndRotation(gripInfo.pivotPos, gripInfo.pivotRot);
        leftHandPivot.SetLocalPositionAndRotation(-gripInfo.pivotPos, gripInfo.pivotRot);
        //gripPivot.SetLocalPositionAndRotation(gripInfo.gripPos, gripInfo.gripRot);
    }

    /// <summary>
    /// 외곽선 설정
    /// </summary>
    public void SetOutlinable()
    {
        outlinable.ComplexMaskingMode = ComplexMaskingMode.ObstaclesMode;
        outlinable.RenderStyle = RenderStyle.FrontBack;
        outlinable.BackParameters.DilateShift = 0.5f;
        outlinable.BackParameters.BlurShift = 0f;
        //outlinable.BackParameters.FillPass.Shader = Resources.Load<Shader>("Shaders/Outline/Fills/Interlaced");
        //outlinable.BackParameters.FillPass.SetColor("_PublicColor", Color.gray);
        //outlinable.BackParameters.FillPass.SetFloat("_PublicSize", 2f);
        //outlinable.BackParameters.FillPass.SetFloat("_PublicSpeed", 0f);
        //outlinable.BackParameters.FillPass.SetFloat("_PublicAngle", 45f);
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
        if (state == CharacterState.Dead) return;

        outlinable.enabled = value;
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
        //var newDilate = value ? 1f : 0.5f;
        outlinable.BackParameters.DilateShift = value ? 1f : 0.5f;
        outlinable.FrontParameters.Enabled = value;
        outlinable.FrontParameters.DilateShift = value ? 1f : 0f;
    }

    private void OnDrawGizmos()
    {
        if (state == CharacterState.Dead) return;

        //DrawShootingPath();
        DrawFireRay();
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
    /// 사선 표시
    /// </summary>
    private void DrawFireRay()
    {
        if (currentWeapon == null) return;

        var endRayPos = currentWeapon.firePoint.position + (currentWeapon.firePoint.forward * range);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(currentWeapon.firePoint.position, endRayPos);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(aimPoint.position, 0.025f);
    }

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
        var startPos = new Vector3(Mathf.Cos(angle) * range, height, Mathf.Sin(angle) * range);
        for (int i = 0; i <= segments; i++)
        {
            angle = i * angleStep * Mathf.Deg2Rad;
            var endPos = new Vector3(Mathf.Cos(angle) * range, height, Mathf.Sin(angle) * range);
            Gizmos.DrawLine(startPos, endPos);
            startPos = endPos;
        }
    }

    private void Update()
    {
        if (state == CharacterState.Dead) return;

        AimProcess();
        CommandApplication();

        //if (Input.GetKeyDown(KeyCode.F1)) rigBdr.Build();
    }

    /// <summary>
    /// 조준관련 처리
    /// </summary>
    private void AimProcess()
    {
        SetAimWeight(headRig, headAim);
        SetAimWeight(chestRig, chestAim);
        SetAimTransform();

        //if (aimTf != null) aimPoint.position = aimTf.position + aimInterval;

        void SetAimWeight(MultiAimConstraint rig, bool value)
        {
            switch (value)
            {
                case true:
                    if (rig.weight != 1f) rig.weight += aimSpeed * Time.deltaTime;
                    break;
                case false:
                    if (rig.weight != 0f) rig.weight -= aimSpeed * Time.deltaTime;
                    break;
            }
        }

        void SetAimTransform()
        {
            if (aimTf == null) return;

            float dist = 5f;
            Vector3 pos = GetAimTarget();
            Vector3 targetPos = aimTf.position + DataUtility.aimInterval;
            Vector3 dir = Vector3.Normalize(targetPos - pos);
            aimPoint.position = pos + (dir * dist) + aimInterval;
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
            case CommandType.ThrowAim:
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
            case CommandType.Throw:
                ThrowProcess(command);
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
            if (tagetInfo.shooterCover != null)
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
                var _targetNode = commandList[0].targetNode;
                commandList.RemoveAt(0);
                if (currentNode.markerType == MarkerType.Base)
                {
                    gameMgr.BaseEvent(_targetNode);
                }
            }
        }
        else
        {
            if (currentNode != command.movePass[0]) currentNode = command.movePass[0];
            if (!animator.GetBool("isMove"))
            {
                animator.SetBool("isAim", false);
                animator.SetBool("isMove", true);
                //currentNode = command.movePass[0];
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
                if (gameMgr.gameState != GameState.Base)
                {
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
                }

                var moveChangeCommand = commandList.Find(x => x.type == CommandType.MoveChange);
                if (moveChangeCommand != null)
                {
                    commandList.Remove(command);
                    currentNode.charCtr = null;
                    currentNode.canMove = true;
                    currentNode = targetNode;

                    gameMgr.CharacterMoveChange(this, moveChangeCommand);
                    commandList.Remove(moveChangeCommand);
                }
                else if (command.movePass.Count == 0)
                {
                    animator.SetBool("isMove", false);
                    var _targetNode = command.targetNode;
                    commandList.Remove(command);
                    if (commandList.Count == 0)
                    {
                        switch (ownerType)
                        {
                            case CharacterOwner.Player:
                                if (gameMgr.eventActive)
                                {
                                    gameMgr.BaseEvent(_targetNode);
                                }
                                else
                                {
                                    AddCommand(CommandType.TakeCover);
                                }
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
                currentCover = cover;
                animator.SetBool("isCover", true);
                switch (cover.coverType)
                {
                    case CoverType.Half:
                        animator.SetBool("fullCover", false);
                        commandList.Remove(command);
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
                currentCover = cover;
                switch (currentCover.coverType)
                {
                    case CoverType.Half:
                        transform.LookAt(cover.transform);
                        animator.SetBool("isCover", true);
                        animator.SetBool("fullCover", false);
                        commandList.Remove(command);
                        break;
                    case CoverType.Full:
                        FindDirectionForCover();
                        break;
                    default:
                        break;
                }
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

        Cover FindTargetDirectionCover()
        {
            var targetList = ownerType != CharacterOwner.Player ? gameMgr.playerList : gameMgr.enemyList;
            var closeList = targetList.FindAll(x => DataUtility.GetDistance(pos, x.currentNode.transform.position) < range);
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
    private void FindDirectionForCover()
    {
        transform.LookAt(currentCover.transform);
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

        //currentCover = _cover;
        coverPos = currentNode.transform.position + (transform.forward * coverInterval);
        covering = true;
        animator.SetBool("isCover", true);
        animator.SetBool("fullCover", true);

        TargetDirection FindCloseTargetDirection()
        {
            var targetList = ownerType != CharacterOwner.Player ? gameMgr.playerList : gameMgr.enemyList;
            var closeList = targetList.FindAll(x => DataUtility.GetDistance(pos, x.currentNode.transform.position) < range);
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
            currentCover = null;
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
        if (currentCover == null || currentCover.coverType == CoverType.None)
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
        if (animator.GetCurrentAnimatorStateInfo(baseIndex).IsTag("Switching")) return;

        if (!canTargeting)
        {
            switch (command.targeting)
            {
                case true:
                    if (animator.GetCurrentAnimatorStateInfo(baseIndex).IsTag("Idle")
                     || animator.GetCurrentAnimatorStateInfo(baseIndex).IsTag("Cover"))
                    {
                        canTargeting = true;
                    }
                    break;
                case false:
                    if (animator.GetCurrentAnimatorStateInfo(baseIndex).IsTag("Cover"))
                    {
                        EndTargeting();
                    }
                    else if (animator.GetCurrentAnimatorStateInfo(baseIndex).IsTag("Idle")
                          || animator.GetCurrentAnimatorStateInfo(baseIndex).IsTag("Targeting"))
                    {
                        canTargeting = true;
                    }
                    break;
            }
            return;
        }

        if (currentCover != null)
        {
            switch (currentCover.coverType)
            {
                case CoverType.Half:
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
                    EndTargeting();
                    break;
                case CoverType.Full:
                    if (!targetingMove)
                    {
                        switch (command.targeting)
                        {
                            case true:
                                aimTf = command.lookAt;
                                headAim = true;
                                var moveDir = animator.GetBool("isRight") ? transform.right : -transform.right;
                                var moveDist = GetDistance();
                                targetingPos = currentNode.transform.position + (moveDir * moveDist);
                                animator.ResetTrigger("unTargeting");
                                animator.SetTrigger("targeting");
                                break;
                            case false:
                                aimTf = null;
                                headAim = false;
                                targetingPos = currentNode.transform.position + (transform.forward * coverInterval);
                                animator.ResetTrigger("targeting");
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
                            targetingMove = false;
                            EndTargeting();
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        else
        {
            if (command.targeting)
            {
                transform.LookAt(command.lookAt);
                if (ownerType == CharacterOwner.Player)
                {
                    gameMgr.ReceiveScheduleSignal();
                }
            }
            EndTargeting();
        }

        float GetDistance()
        {
            if (currentWeapon.weaponData.isMain)
            {
                return 0.7f;
            }
            else
            {
                return 0.45f;
            }
        }

        float GetSpeed()
        {
            if (currentWeapon.weaponData.isMain)
            {
                return targetingMoveSpeed_main;
            }
            else
            {
                return targetingMoveSpeed_sub;
            }
        }

        void EndTargeting()
        {
            canTargeting = false;
            //if (!command.targeting) currentWeapon.MoveLocalPosition(false);
            commandList.Remove(command);
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
            case CommandType.ThrowAim:
                if (throwInfo.throwerCover != null)
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
                        coverPos = command.targetInfo.shooterNode.transform.position;
                        SetAiming(command.targetInfo);
                        break;
                    case CommandType.Watch:
                        animator.SetBool("isRight", watchInfo.isRight);
                        coverPos = watchInfo.watchNode.transform.position;
                        aimTf = watchInfo.targetNode.transform;
                        aimInterval = Vector3.zero;
                        //aimPoint.position = aimTf.position + aimInterval;
                        break;
                    case CommandType.ThrowAim:
                        animator.SetBool("isRight", throwInfo.isRight);
                        coverPos = throwInfo.throwNode.transform.position;
                        aimTf = throwInfo.targetNode.transform;
                        aimInterval = Vector3.zero;
                        //aimPoint.position = aimTf.position + aimInterval;
                        break;
                    default:
                        break;
                }
                covering = true;
            }
            else if (animator.GetCurrentAnimatorStateInfo(baseIndex).IsTag("Aim"))
            {
                AdjustWeapon();
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

                    if (command.type == CommandType.Aim && ownerType == CharacterOwner.Enemy) SetTurnEnd(true);
                }
            }
        }

        void NoneCoverAim()
        {
            AdjustWeapon();
            animator.SetBool("isAim", true);
            switch (command.type)
            {
                case CommandType.Aim:
                    transform.LookAt(command.targetInfo.target.transform);
                    SetAiming(command.targetInfo);
                    break;
                case CommandType.Watch:
                    var watchTarget = watchInfo.targetNode.transform;
                    transform.LookAt(watchTarget);
                    aimTf = watchTarget;
                    aimInterval = Vector3.zero;
                    break;
                case CommandType.ThrowAim:
                    var throwTarget = throwInfo.targetNode.transform;
                    transform.LookAt(throwTarget);
                    aimTf = throwTarget;
                    aimInterval = Vector3.zero;
                    break;
                default:
                    break;
            }
            //aimPoint.transform.position = aimTf.position + aimInterval;
            chestAim = true;
            chestRig.weight = 1f;
            commandList.Remove(command);

            if (command.type == CommandType.Aim && ownerType == CharacterOwner.Enemy) SetTurnEnd(true);
        }

        void AdjustWeapon()
        {
            if (animator.GetBool("isAim")) return;

        }
    }

    /// <summary>
    /// 조준점 설정
    /// </summary>
    /// <param name="targetInfo"></param>
    private void SetAiming(TargetInfo targetInfo)
    {
        aimTf = targetInfo.target.mainHolsterPivot;
        if (currentWeapon.CheckHitBullet(targetInfo, animator.GetInteger("shootNum"), true))
        {
            var dir = System.Convert.ToBoolean(Random.Range(0, 2)) ? transform.right : -transform.right;
            var errorInterval = 1f;
            aimInterval = dir * errorInterval;
            //aimInterval.y += DataUtility.aimPointY;
            //if (targetInfo.target.animator.GetCurrentAnimatorStateInfo(baseIndex).IsTag("Targeting"))
            //{
            //    targetInfo.target.AddCommand(CommandType.Targeting, false, transform);
            //}
        }
        else
        {
            aimInterval = Vector3.zero;
        }
        //aimPoint.transform.position = aimTf.position + aimInterval;
    }

    /// <summary>
    /// 캐릭터 사격 처리
    /// </summary>
    /// <param name="command"></param>
    private void ShootProcess(CharacterCommand command)
    {
        var shootNum = animator.GetInteger("shootNum");
        if (shootNum == 0 && animator.GetCurrentAnimatorStateInfo(upperIndex).IsTag("Aim"))
        {
            if (curCoroutine == null) curCoroutine = StartCoroutine(Coroutine_AimOff(command));
        }
        else
        {
            timer += Time.deltaTime;
            if (timer > aimTime && chestRig.weight == 1f
             && animator.GetCurrentAnimatorStateInfo(upperIndex).IsTag("Aim"))
            {
                switch (ownerType)
                {
                    case CharacterOwner.Player:
                        switch (gameMgr.uiMgr.aimGauge.state)
                        {
                            case AimGauge.State.None:
                                gameMgr.uiMgr.aimGauge.SetAimGauge(true, currentWeapon);
                                return;
                            case AimGauge.State.Done:
                                break;
                            default:
                                return;
                        }
                        break;
                    case CharacterOwner.Enemy:
                        switch (charUI.aimGauge.state)
                        {
                            case AimGauge.State.None:
                                charUI.aimGauge.SetAimGauge(true, currentWeapon);
                                return;
                            case AimGauge.State.Done:
                                break;
                            default:
                                return;
                        }
                        break;
                    default:
                        break;
                }

                if (!animator.GetBool("fireTrigger")) animator.SetBool("fireTrigger", true);

                //weapon.FireBullet();
                //shootNum--;
                //if (shootNum == 0)
                //{
                //    //animator.SetBool("fireTrigger", false);
                //    StartCoroutine(Coroutine_AimOff(command));
                //}
            }
        }
    }

    /// <summary>
    /// 캐릭터 재장전 처리
    /// </summary>
    /// <param name="command"></param>
    private void ReloadPrecess(CharacterCommand command)
    {
        if (!reloading/* && animator.GetCurrentAnimatorStateInfo(upperIndex).IsTag("None")*/)
        {
            if (!command.isReload && !command.loadChamber)
            {
                commandList.Remove(command);
                return;
            }

            animator.SetBool("reload", command.isReload);
            animator.SetBool("loadChamber", command.loadChamber);
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
            if (changeIndex == weapons.Count) changeIndex = 0;

            animator.SetBool("otherType", true);
            animator.SetTrigger("change");
            changing = true;
        }
    }

    private void ThrowProcess(CharacterCommand command)
    {
        if (!throwing)
        {
            animator.SetTrigger("throw");
            throwing = true;
        }
    }

    //private void LateUpdate()
    //{
    //    ControlGripPivot();
    //    ControlChainWeaight();
    //}

    //private void ControlGripPivot()
    //{
    //    if (currentWeapon == null) return;
    //    if (currentWeapon.gripTf == null) return;

    //    gripPivot.position = currentWeapon.gripTf.position;
    //}

    //private void ControlChainWeaight()
    //{
    //    if (chainIK.weight == targetWeight) return;

    //    if (runInstantly)
    //    {
    //        chainIK.weight = targetWeight;
    //    }
    //    else
    //    {

    //    }
    //}

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
    /// 능력치 설정
    /// </summary>
    public void SetAbility()
    {
        addAbility.SetAddAbility();
        shootingMode_point = ability.sModeInfos[(int)ShootingMode.PointShot].value + addAbility.sModeInfos[(int)ShootingMode.PointShot].value;
        shootingMode_aim = ability.sModeInfos[(int)ShootingMode.AimShot].value + addAbility.sModeInfos[(int)ShootingMode.AimShot].value;
        shootingMode_sight = ability.sModeInfos[(int)ShootingMode.SightShot].value + addAbility.sModeInfos[(int)ShootingMode.SightShot].value;
        RPM = ability.RPM + addAbility.RPM;
        range = ability.range + addAbility.range;
        watchAngle = ability.watchAngle + addAbility.watchAngle;
        MOA = ability.MOA + addAbility.MOA;
        stability = ability.stability + addAbility.stability;
        rebound = ability.rebound + addAbility.rebound;
        propellant = ability.propellant + addAbility.propellant;
        damage = ability.damage + addAbility.damage;
        penetrate = ability.penetrate + addAbility.penetrate;
        armorBreak = ability.armorBreak + addAbility.armorBreak;
        critical = ability.critical + addAbility.critical;
    }

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

    public void PayTheShotCost(int curStamina)
    {
        stamina = curStamina;
        charUI.SetCharacterValue();
    }

    public void SetTurnEnd(bool turnEnd)
    {
        if (this.turnEnd == turnEnd) return;

        this.turnEnd = turnEnd;
        switch (turnEnd)
        {
            case true:
                if (ownerType == CharacterOwner.Enemy) gameMgr.SetPositionOfAI(ownerType);
                break;
            case false:
                var newStamina = 30 + (action * 10);
                SetAction(maxAction);
                SetStamina(newStamina);
                break;
        }
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
            var interval = new Vector3(0f, 1f, 0f);
            if (Physics.Raycast(watchNode.transform.position + interval, dir, DataUtility.nodeSize, gameMgr.coverLayer))
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
        var halfAngle = watchAngle / 2f;
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

    public void SetGrenadeInfo(FieldNode targetNode)
    {
        FieldNode throwNode;
        var isRight = false;
        var interval = new Vector3(0f, 1f, 0f);
        var throwerCover = FindCoverNode(currentNode, targetNode);
        if (throwerCover != null && throwerCover.coverType == CoverType.Full)
        {
            var RN = CheckTheCanMoveNode(currentNode, throwerCover, TargetDirection.Right);
            var LN = CheckTheCanMoveNode(currentNode, throwerCover, TargetDirection.Left);
            if (RN == null && LN == null)
            {
                throwNode = currentNode;
            }
            else if (LN == null)
            {
                throwNode = RN;
                isRight = true;
            }
            else if (RN == null)
            {
                throwNode = LN;
            }
            else if (DataUtility.GetDistance(RN.transform.position, targetNode.transform.position)
                  <= DataUtility.GetDistance(LN.transform.position, targetNode.transform.position))
            {
                throwNode = RN;
                isRight = true;
            }
            else
            {
                throwNode = LN;
            }

            var dir = Vector3.Normalize(targetNode.transform.position - throwNode.transform.position);
            if (Physics.Raycast(throwNode.transform.position + interval, dir, DataUtility.nodeSize, gameMgr.coverLayer))
            {
                throwNode = currentNode;
            }
        }
        else
        {
            throwNode = currentNode;
        }

        if (!CheckTheCoverAlongPath(throwNode.transform.position, targetNode.transform.position)) return;

        // 포물선
        grenadeHlr.lineRdr.enabled = true;
        var startPos = throwNode.transform.position + interval;
        var endPos = targetNode.transform.position;
        grenadeHlr.lineRdr.SetPositions(DataUtility.GetParabolaPoints(grenadeHlr.lineRdr.positionCount, startPos, endPos));
        grenadeHlr.rangeMr.transform.position = grenadeHlr.lineRdr.GetPosition(grenadeHlr.lineRdr.positionCount - 1);
        grenadeHlr.rangeMr.gameObject.SetActive(true);


        // 범위 검색
        SetOffThrowTargets();
        throwInfo.targetList.Clear();
        var targetList = new List<CharacterController>();
        var hits = Physics.SphereCastAll(endPos, grenadeHlr.blastRange * 0.5f, grenadeHlr.curGrenade.transform.forward, 0f, gameMgr.charLayer);
        if (hits.Length > 0)
        {
            var explosionPos = targetNode.transform.position + interval;
            for (int i = 0; i < hits.Length; i++)
            {
                var charCtr = hits[i].collider.GetComponent<CharacterController>();
                if (charCtr == null) continue;

                var targetPos = charCtr.currentNode.transform.position + interval;
                var dist = DataUtility.GetDistance(targetPos, explosionPos);
                var dir = Vector3.Normalize(targetPos - explosionPos);
                if (!Physics.Raycast(explosionPos, dir, dist, gameMgr.coverLayer))
                {
                    charCtr.SetActiveOutline(true);
                    targetList.Add(charCtr);
                }
            }
        }

        throwInfo = new ThrowInfo()
        {
            targetNode = targetNode,
            throwNode = throwNode,
            throwerCover = throwerCover,
            isRight = isRight,
            targetList = targetList,
        };
    }

    public void SetOffThrowTargets()
    {
        if (throwInfo.targetList.Count == 0) return;

        for (int i = 0; i < throwInfo.targetList.Count; i++)
        {
            var target = throwInfo.targetList[i];
            target.SetActiveOutline(false);
        }
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
            var shooterCover = FindCoverNode(currentNode, target.currentNode);
            Cover targetCover = null;
            FieldNode RN = null;
            FieldNode LN = null;
            FieldNode targetRN = null;
            FieldNode targetLN = null;
            if (shooterCover != null && shooterCover.coverType == CoverType.Full)
            {
                RN = CheckTheCanMoveNode(node, shooterCover, TargetDirection.Right);
                LN = CheckTheCanMoveNode(node, shooterCover, TargetDirection.Left);
                if (RN == null && LN == null)
                {
                    shooterCover = null;
                }
                FindNodeOfShooterAndTarget();
            }
            else
            {
                FindNodeOfShooterAndTarget();
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
                        var _targetCover = FindCoverNode(target.currentNode, RN);
                        if (_targetCover != null && _targetCover.coverType == CoverType.Full)
                        {
                            targetRN = CheckTheCanMoveNode(target.currentNode, _targetCover, TargetDirection.Right);
                            targetLN = CheckTheCanMoveNode(target.currentNode, _targetCover, TargetDirection.Left);
                            if (targetRN != null)
                            {
                                CheckNodes(RN, _targetCover, targetRN, true, true);
                            }
                            if (targetLN != null)
                            {
                                CheckNodes(RN, _targetCover, targetLN, true, false);
                            }
                        }
                        else
                        {
                            CheckNodes(RN, _targetCover, target.currentNode, true, false);
                        }
                    }
                    if (LN != null)
                    {
                        var _targetCover = FindCoverNode(target.currentNode, LN);
                        if (_targetCover != null && _targetCover.coverType == CoverType.Full)
                        {
                            targetRN = CheckTheCanMoveNode(target.currentNode, _targetCover, TargetDirection.Right);
                            targetLN = CheckTheCanMoveNode(target.currentNode, _targetCover, TargetDirection.Left);
                            if (targetRN != null)
                            {
                                CheckNodes(LN, _targetCover, targetRN, false, true);
                            }
                            if (targetLN != null)
                            {
                                CheckNodes(LN, _targetCover, targetLN, false, false);
                            }
                        }
                        else
                        {
                            CheckNodes(LN, _targetCover, target.currentNode, false, false);
                        }
                    }
                }
                else
                {
                    var _targetCover = FindCoverNode(target.currentNode, currentNode);
                    if (_targetCover != null && _targetCover.coverType == CoverType.Full)
                    {
                        targetRN = CheckTheCanMoveNode(target.currentNode, _targetCover, TargetDirection.Right);
                        targetLN = CheckTheCanMoveNode(target.currentNode, _targetCover, TargetDirection.Left);
                        if (targetRN != null)
                        {
                            CheckNodes(currentNode, _targetCover, targetRN, false, true);
                        }
                        if (targetLN != null)
                        {
                            CheckNodes(currentNode, _targetCover, targetLN, false, false);
                        }
                    }
                    else
                    {
                        CheckNodes(currentNode, _targetCover, target.currentNode, true, false);
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

                void CheckNodes(FieldNode _shooterNode, Cover _targetCover, FieldNode _targetNode, bool _isRight, bool _targetRight)
                {
                    pos = _shooterNode.transform.position;
                    targetPos = _targetNode.transform.position;
                    if (CheckTheCoverAlongPath(range, pos, targetPos, noRange))
                    {
                        var dist = DataUtility.GetDistance(pos, targetPos);
                        if (dist < distance)
                        {
                            if (targetCover != null && _targetCover != null && targetCover.coverType < _targetCover.coverType) return;

                            shooterNode = _shooterNode;
                            targetCover = _targetCover;
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

        if (cover != null) return cover;

        var rightDir = Quaternion.Euler(0f, 90f, 0f) * dir;
        var interval = rightDir * 0.4f;
        shooterPos = shooterNode.transform.position + interval;
        endPos = targetNode.transform.position + interval;
        RayCastOfCoverLayer();

        if (cover != null) return cover;

        var leftDir = Quaternion.Euler(0f, -90f, 0f) * dir;
        interval = leftDir * 0.4f;
        shooterPos = shooterNode.transform.position + interval;
        endPos = targetNode.transform.position + interval;
        dir = Vector3.Normalize(endPos - shooterPos);
        RayCastOfCoverLayer();

        return cover;

        void RayCastOfCoverLayer()
        {
            dir = Vector3.Normalize(endPos - shooterPos);
            if (Physics.Raycast(shooterPos, dir, out RaycastHit hit, DataUtility.nodeSize, gameMgr.coverLayer))
            {
                var _cover = hit.collider.GetComponentInParent<Cover>();
                if (_cover == null) return;

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

        return moveNode;
    }

    /// <summary>
    /// 경로 상의 장애물을 체크
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
    /// 경로 상의 장애물을 체크
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="targetPos"></param>
    /// <returns></returns>
    public bool CheckTheCoverAlongPath(Vector3 pos, Vector3 targetPos)
    {
        bool canShoot;
        var dist = DataUtility.GetDistance(pos, targetPos);
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
            sMode = ShootingMode.PointShot;
            gameMgr.SetEnemyOutlinable(false);
            var targetInfo = targetList[targetIndex];
            targetInfo.target.SetActiveOutline(true);
            SetTargeting(targetInfo, CharacterOwner.All);
            CameraState camState;
            if (targetInfo.shooterCover != null && targetInfo.shooterCover.coverType == CoverType.Full && !targetInfo.isRight)
            {
                camState = CameraState.LeftAim;
            }
            else
            {
                camState = CameraState.RightAim;
            }
            gameMgr.SwitchCharacterUI(false);
            targetInfo.target.charUI.components.SetActive(true);

            gameMgr.camMgr.SetCameraState(camState, this, targetInfo.target);
            gameMgr.uiMgr.SetUsedActionPoint_Bottom(this, currentWeapon.weaponData.actionCost);
            gameMgr.uiMgr.SetActiveAimUI(this, true);
            gameMgr.uiMgr.aimGauge.components.SetActive(true);
            gameMgr.uiMgr.SetHitAccuracy(targetInfo.shooter);
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
        prevTargetInfo.target.charUI.components.SetActive(false);
        prevTargetInfo.target.SetActiveOutline(false);
        prevTargetInfo.target.AddCommand(CommandType.Targeting, false, transform);
        targetIndex++;
        if (targetIndex == targetList.Count)
        {
            targetIndex = 0;
        }
        //ChangeTargetShader();

        var targetInfo = targetList[targetIndex];
        targetInfo.target.charUI.components.SetActive(true);
        targetInfo.target.SetActiveOutline(true);
        SetTargeting(targetInfo, CharacterOwner.All);
        CameraState camState;
        if (targetInfo.shooterCover != null && targetInfo.shooterCover.coverType == CoverType.Full && !targetInfo.isRight)
        {
            camState = CameraState.LeftAim;
        }
        else
        {
            camState = CameraState.RightAim;
        }
        gameMgr.camMgr.SetCameraState(camState, this, targetInfo.target);
        gameMgr.uiMgr.SetHitAccuracy(targetInfo.shooter);
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
            if (target.currentCover != null)
            {
                if (targetInfo.targetCover == null)
                {
                    target.AddCommand(CommandType.LeaveCover);
                }
                else if (targetInfo.targetCover != null && targetInfo.targetCover != target.currentCover)
                {
                    target.AddCommand(CommandType.LeaveCover);
                    target.AddCommand(CommandType.TakeCover, targetInfo.targetCover, targetInfo.targetRight);
                }
                else if (targetInfo.targetRight != target.animator.GetBool("isRight"))
                {
                    //target.animator.SetBool("isRight", targetInfo.targetRight);
                    target.AddCommand(CommandType.TakeCover, targetInfo.targetCover, targetInfo.targetRight);
                }
            }
            else if (targetInfo.targetCover != null)
            {
                target.AddCommand(CommandType.TakeCover, targetInfo.targetCover, targetInfo.targetRight);
            }
            //else
            //{
            //    target.transform.LookAt(shooter.transform);
            //}
            target.AddCommand(CommandType.Targeting, true, shooter.transform);
        }

        if (ownerType == CharacterOwner.All || shooter.ownerType == ownerType)
        {
            if (shooter.currentCover != null)
            {
                if (targetInfo.shooterCover == null)
                {
                    shooter.AddCommand(CommandType.LeaveCover, target.transform);
                }
                else if (targetInfo.shooterCover != null && targetInfo.shooterCover != shooter.currentCover)
                {
                    shooter.AddCommand(CommandType.LeaveCover);
                    shooter.AddCommand(CommandType.TakeCover, targetInfo.shooterCover, targetInfo.isRight);
                }
                else if (targetInfo.isRight != shooter.animator.GetBool("isRight"))
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
        gameMgr.SetEnemyOutlinable(true);
        gameMgr.uiMgr.SetActiveAimUI(this, false);
        gameMgr.uiMgr.aimGauge.components.SetActive(false);
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
            else if (watchInfo.shooterCover != null && watchInfo.shooterCover != currentCover)
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

    public void SetThrower()
    {
        SetOffThrowTargets();
        grenadeHlr.lineRdr.enabled = false;
        grenadeHlr.rangeMr.gameObject.SetActive(false);
        if (animator.GetBool("isCover"))
        {
            if (throwInfo.throwerCover == null)
            {
                AddCommand(CommandType.LeaveCover);
            }
            else if (throwInfo.throwerCover != null && throwInfo.throwerCover != currentCover)
            {
                AddCommand(CommandType.LeaveCover);
                AddCommand(CommandType.TakeCover, throwInfo.throwerCover, throwInfo.isRight);
            }
        }
        else
        {
            if (throwInfo.throwerCover != null)
            {
                AddCommand(CommandType.TakeCover, throwInfo.throwerCover, throwInfo.isRight);
            }
        }
        AddCommand(CommandType.ThrowAim);
        AddCommand(CommandType.Throw);
        AddCommand(CommandType.BackCover);
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
                var aimCommand = new CharacterCommand
                {
                    indexName = $"{type}",
                    type = CommandType.Aim,
                    targetInfo = targetList[targetIndex],
                };
                commandList.Add(aimCommand);
                break;
            case CommandType.Shoot:
                SetRig(currentWeapon);
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

    public void AddCommand(CommandType type, FieldNode endNode, FieldNode targetNode)
    {
        switch (type)
        {
            case CommandType.MoveChange:
                var moveChangeCommand = new CharacterCommand
                {
                    indexName = $"{type}",
                    type = CommandType.MoveChange,
                    endNode = endNode,
                    targetNode = targetNode,
                };
                commandList.Add(moveChangeCommand);
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
    public void AddCommand(CommandType type, List<FieldNode> movePass, FieldNode targetNode)
    {
        switch (type)
        {
            case CommandType.Move:
                currentNode.canMove = true;
                currentNode.charCtr = null;
                targetNode.canMove = false;
                targetNode.charCtr = this;
                PayTheMoveCost(targetNode.moveCost);

                if (ownerType == CharacterOwner.Player) gameMgr.gameMenuMgr.otherStorage.DeactiveTabButtons();
                var moveCommand = new CharacterCommand
                {
                    indexName = $"{type}",
                    type = CommandType.Move,
                    movePass = new List<FieldNode>(movePass),
                    targetNode = targetNode,
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
    /// <param name="lookAt"></param>
    public void AddCommand(CommandType type, Transform lookAt)
    {
        switch (type)
        {
            case CommandType.LeaveCover:
                var targetingCommand = new CharacterCommand
                {
                    indexName = $"{type}",
                    type = CommandType.LeaveCover,
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
    /// <param name="type"></param>
    /// <param name="targeting"></param>
    /// <param name="lookAt"></param>
    public void AddCommand(CommandType type, bool targeting, Transform lookAt)
    {
        switch (type)
        {
            case CommandType.Targeting:
                CancelTargeting();
                var targetingCommand = new CharacterCommand
                {
                    indexName = $"{type}_{targeting}",
                    type = CommandType.Targeting,
                    targeting = targeting,
                    lookAt = lookAt,
                };
                commandList.Add(targetingCommand);
                break;
            default:
                break;
        }

        void CancelTargeting()
        {
            if (ownerType == CharacterOwner.Player) return;

            var targetingCommand = commandList.Find(x => x.type == CommandType.Targeting);
            if (targetingCommand == null) return;

            animator.ResetTrigger("unTargeting");
            animator.ResetTrigger("targeting");
            canTargeting = false;
            targetingMove = false;
            commandList.Remove(targetingCommand);
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
                if (currentWeapon.weaponData.magType != MagazineType.Cylinder && !currentWeapon.weaponData.isChamber) return;
                if (currentWeapon.weaponData.magType == MagazineType.Cylinder && currentWeapon.weaponData.equipMag.loadedBullets.Count == 0) return;
                //if (!currentWeapon.weaponData.isMag) return;
                //if (currentWeapon.weaponData.equipMag.loadedBullets.Count == 0) return;

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
    /// <param name="targetInfo"></param>
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
                SetRig(currentWeapon);
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
    /// <param name="loadChamber"></param>
    public void AddCommand(CommandType type, bool isReload, bool loadChamber)
    {
        switch (type)
        {
            case CommandType.Reload:
                var reloadCommand = new CharacterCommand
                {
                    indexName = $"{type}",
                    type = CommandType.Reload,
                    isReload = isReload,
                    loadChamber = loadChamber,
                };
                commandList.Add(reloadCommand);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 커맨드 추가
    /// </summary>
    /// <param name="type"></param>
    /// <param name="reloadNum"></param>
    public void AddCommand(CommandType type, int reloadNum, bool isReload, bool loadChamber)
    {
        switch (type)
        {
            case CommandType.Reload:
                var reloadCommand = new CharacterCommand
                {
                    indexName = $"{type}",
                    type = CommandType.Reload,
                    isReload = isReload,
                    loadChamber = loadChamber,
                };
                animator.SetInteger("reloadNum", reloadNum);
                commandList.Add(reloadCommand);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 캐릭터 피격
    /// </summary>
    public void OnHit(CharacterController shooter, Bullet bullet)
    {
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

            var penetrateRate = bullet.penetrate <= armor.bulletproof ? (int)Mathf.Floor(0.9f - (0.1f * (armor.bulletproof - bullet.penetrate)) * 100f) : 95;
            isPenetrate = value < penetrateRate;

            var armorDamage = (int)Mathf.Floor(bullet.damage * (bullet.armorBreak / 100f));
            SetArmor(-armorDamage);
            bulletproof = armor.bulletproof;
        }
        else
        {
            isPenetrate = true;
            bulletproof = 0f;
        }

        var penetrate = bulletproof - bullet.penetrate < 0 ? 0 : bulletproof - bullet.penetrate;
        var damage = (int)Mathf.Floor(bullet.damage * (1 + penetrate * 0.01f));
        if (isPenetrate)
        {
            SetHealth(-damage);
            Debug.Log($"{transform.name}: 공격자 = {shooter.name}, 피해량 = {damage}, 체력 = {health}/{maxHealth}");
            gameMgr.SetFloatText(charUI, $"{damage}", Color.white);
        }
        else
        {
            SetStamina(-damage);
        }

        if (state == CharacterState.Dead) return;
        if (health == 0)
        {
            var force = 500f;
            CharacterDead(bullet.transform.forward, force);
        }
        else if (health > 0 && !animator.GetCurrentAnimatorStateInfo(baseIndex).IsTag("Hit") && !animator.GetBool("isMove"))
        {
            animator.SetTrigger("isHit");
        }
    }

    public void OnHit(Vector3 dir, int damage)
    {
        SetHealth(-damage);
        gameMgr.SetFloatText(charUI, $"{damage}", Color.white);

        if (health == 0)
        {
            var force = 1000f;
            CharacterDead(dir, force);
        }
        else if (health > 0 && !animator.GetCurrentAnimatorStateInfo(baseIndex).IsTag("Hit") && !animator.GetBool("isMove"))
        {
            animator.SetTrigger("isHit");
        }
    }

    private void CharacterDead(Vector3 dir, float force)
    {
        animator.enabled = false;
        outlinable.enabled = false;
        cd.enabled = false;
        headAim = false;
        chestAim = false;
        headRig.weight = 0f;
        chestRig.weight = 0f;
        currentNode.charCtr = null;
        currentNode.canMove = true;
        var charList = ownerType == CharacterOwner.Player ? gameMgr.playerList : gameMgr.enemyList;
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

        if (charList.Find(x => x.state != CharacterState.Dead) == null)
        {
            Time.timeScale = 0.2f;
            Time.fixedDeltaTime = Time.timeScale * 0.02f;
            gameMgr.gameState = GameState.Result;
            StartCoroutine(gameMgr.Coroutine_GameEnd());
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

    public void AddWeapon(ItemHandler item, EquipSlot equipSlot)
    {
        var find = weapons.Find(x => x.weaponData.ID == item.weaponData.ID && x.equipSlot.type == equipSlot.type);
        if (find != null) return;

        var weapon = GetWeapon(item.weaponData);
        weapon.SetComponets(this, equipSlot, item.weaponData);
        if (currentWeapon == null)
        {
            weapon.WeaponSwitching("Right");
            currentWeapon = weapon;
            weapon.EquipWeapon();
        }
        else
        {
            weapon.WeaponSwitching("Holster");
        }
    }

    public void RemoveWeapon(string weaponID, EquipSlot equipSlot)
    {
        var prevWeapon = weapons.Find(x => x.weaponData.ID == weaponID && x.equipSlot.type == equipSlot.type);
        if (prevWeapon == null) return;

        //weapon.transform.SetParent(weaponPoolTf, false);
        //weapon.Initialize();
        weapons.Remove(prevWeapon);
        if (currentWeapon == prevWeapon)
        {
            if (weapons.Count > 0)
            {
                var weapon = weapons[0];
                weapon.WeaponSwitching("Right");
                currentWeapon = weapon;
                weapon.EquipWeapon();
            }
            else
            {
                currentWeapon = null;
            }
        }
        Destroy(prevWeapon.gameObject);
    }

    public Weapon GetWeapon(WeaponDataInfo weaponData)
    {
        string prefabPath = "Prefabs/Weapon/";
        switch (weaponData.weaponType)
        {
            case WeaponType.Pistol:
                prefabPath += "Handgun/";
                break;
            case WeaponType.Revolver:
                prefabPath += "Handgun/";
                break;
            default:
                prefabPath += $"{weaponData.weaponType}/";
                break;
        }
        Weapon weapon = Instantiate(Resources.Load<Weapon>(prefabPath + $"{weaponData.prefabName}"));
        weapon.transform.localScale = Vector3.one;
        return weapon;

        //if (weaponPool.Count == 0) return null;

        //var weaponName = prefabName;
        //switch (equipType)
        //{
        //    case EquipType.MainWeapon1:
        //        weaponName += "_A";
        //        break;
        //    case EquipType.MainWeapon2:
        //        weaponName += "_B";
        //        break;
        //    default:
        //        break;
        //}

        //var weapon = weaponPool.Find(x => x.name == weaponName);
        //return weapon;
    }

    public Weapon GetWeapon(EnemyGearDataInfo.WeaponInfo weaponInfo)
    {
        string prefabPath = "Prefabs/Weapon/";
        switch (weaponInfo.weaponType)
        {
            case WeaponType.Pistol:
                prefabPath += "Handgun/";
                break;
            case WeaponType.Revolver:
                prefabPath += "Handgun/";
                break;
            default:
                prefabPath += $"{weaponInfo.weaponType}/";
                break;
        }
        Weapon weapon = Instantiate(Resources.Load<Weapon>(prefabPath + $"{weaponInfo.prefabName}"));
        weapon.transform.localScale = Vector3.one;
        return weapon;
    }

    public Vector3 GetAimTarget()
    {
        return mainHolsterPivot.position + DataUtility.aimInterval;
    }

    public void EnterTheBase()
    {
        charUI.gameObject.SetActive(false);
        state = CharacterState.Base;
    }

    private void ReloadEnd()
    {
        if (ownerType == CharacterOwner.Player)
        {
            gameMgr.camMgr.SetCameraState(CameraState.None);
            gameMgr.camMgr.lockCam = false;
        }
        animator.SetBool("reload", false);
        animator.SetBool("loadChamber", false);
        //currentWeapon.WeaponSwitching("Right");
        commandList.RemoveAt(0);
        reloading = false;
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

        if (ownerType == CharacterOwner.Player)
        {
            charUI.components.SetActive(true);
            gameMgr.SetEnemyOutlinableAndUI(true);
            gameMgr.camMgr.SetCameraState(CameraState.None);
            gameMgr.camMgr.lockCam = false;
            gameMgr.uiMgr.aimGauge.SetAimGauge(false);
        }
        else
        {
            charUI.aimGauge.SetAimGauge(false);
        }
        commandList.Remove(command);
        curCoroutine = null;
    }
    #endregion

    #region Animation Event
    /// <summary>
    /// (애니메이션 이벤트)무기 사격
    /// </summary>
    public void Event_FireWeapon()
    {
        if (commandList.Count == 0) return;

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
        if (currentWeapon == null)
        {
            Debug.Log(rightHandPivot.transform.position);

            Weapon weapon = switchPos == "Right" ? leftHandPivot.GetComponentInChildren<Weapon>() : rightHandPivot.GetComponentInChildren<Weapon>();
            if (weapon == null) return;

            switch (switchPos)
            {
                case "Right":
                    weapon.transform.SetParent(rightHandPivot, false);
                    break;
                case "Left":
                    leftHandPivot.transform.position = rightHandPivot.transform.position;
                    leftHandPivot.transform.rotation = rightHandPivot.transform.rotation;
                    weapon.transform.SetParent(leftHandPivot, false);
                    break;
                default:
                    break;
            }
            //animator.SetBool("reload", false);
            //animator.SetBool("loadChamber", false);
        }
        else
        {
            currentWeapon.WeaponSwitching(switchPos);
        }
    }

    public void Event_WeaponSwitching_Rifle(string switchPos)
    {
        if (!animator.GetBool("otherType")) return;

        currentWeapon.WeaponSwitching(switchPos);
    }

    public void Event_InsertBullet()
    {
        var reloadNum = animator.GetInteger("reloadNum") - 1;
        animator.SetInteger("reloadNum", reloadNum);
        if (reloadNum > 0) return;

        if (currentWeapon == null) return;
        if (currentWeapon.weaponData.magType == MagazineType.Cylinder) return;

        //if (commandList.Count > 0 && commandList[0].type == CommandType.Reload && !commandList[0].loadChamber)
        //{
        //    animator.SetBool("reload", false);
        //    currentWeapon.WeaponSwitching("Right");
        //    StartCoroutine(Coroutine_ReloadEnd());
        //}
    }

    public void Event_UnequipMagazine()
    {
        if (currentWeapon == null) return;

        currentWeapon.SetAllParts("Magazine", false);
    }

    public void Event_EquipMagazine()
    {
        if (currentWeapon == null) return;

        if (currentWeapon.weaponData.equipMag != null)
        {
            currentWeapon.SetParts(currentWeapon.weaponData.equipMag.prefabName, true);
        }

        //if (commandList.Count > 0 && commandList[0].type == CommandType.Reload && !commandList[0].loadChamber)
        //{
        //    animator.SetBool("reload", false);
        //    currentWeapon.WeaponSwitching("Right");
        //    StartCoroutine(Coroutine_ReloadEnd());
        //}
    }

    /// <summary>
    /// (애니메이션 이벤트)재장전 완료
    /// </summary>
    public void Event_ReloadEnd()
    {
        if (commandList.Count == 0) return;
        if (commandList[0].type != CommandType.Reload) return;

        ReloadEnd();
    }

    public void Event_WeaponChange()
    {
        Debug.Log("Event_WeaponChange");
        //SetWeaponAbility(false, currentWeapon.weaponData);
        currentWeapon.WeaponSwitching("Holster");
        //currentWeapon.UnequipWeapon();
        weapons[changeIndex].EquipWeapon();
        currentWeapon.WeaponSwitching("Right");
    }

    public void Event_WeaponChange_OrderType()
    {
        if (!animator.GetBool("otherType")) return;

        //currentWeapon.UnequipWeapon();
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

        changing = false;
        commandList.RemoveAt(0);
        gameMgr.RecheckTarget();
    }

    public void Event_SetGrenade()
    {
        grenadeHlr.curGrenade.transform.SetParent(leftHandPivot, false);
        var pos = new Vector3(-0.047f, -0.051f, 0f);
        var rot = new Vector3(-15.6f, 90f, 90f);
        grenadeHlr.curGrenade.transform.SetLocalPositionAndRotation(pos, Quaternion.Euler(rot));
        grenadeHlr.curGrenade.SetActive(true);
    }

    public void Event_ThrowGrenade()
    {
        grenadeHlr.ThrowGrenade(throwInfo.targetNode.transform.position);
    }

    public void Event_ThrowEnd()
    {
        if (commandList.Count == 0)
        {
            Debug.LogError("No Command in the CommanderList");
            return;
        }
        else if (commandList[0].type != CommandType.Throw)
        {
            Debug.LogError("CommandType is not ChangeWeapon");
            return;
        }

        AimOff();
        commandList.RemoveAt(0);
        throwing = false;
    }

    public void Event_TargetingOn()
    {
        if (ownerType != CharacterOwner.Player) return;

        gameMgr.ReceiveScheduleSignal();
    }
    #endregion

    [System.Serializable]
    public class Ability
    {
        [HideInInspector] public CharacterController charCtr;

        public List<ShootingModeInfo> sModeInfos = new List<ShootingModeInfo>();
        [Tooltip("발사속도")] public int RPM;
        [Tooltip("사거리")] public float range;
        [Tooltip("경계각")] public int watchAngle;
        [Tooltip("정확도")] public float MOA;
        [Tooltip("안정성")] public int stability;
        [Tooltip("반동")] public int rebound;
        [Tooltip("장약")] public int propellant;
        [Tooltip("피해량")] public int damage;
        [Tooltip("관통")] public int penetrate;
        [Tooltip("방어구 손상")] public int armorBreak;
        [Tooltip("파편화")] public int critical;

        public void ResetShootingModeInfos()
        {
            sModeInfos = new List<ShootingModeInfo>
            {
                new ShootingModeInfo()
                {
                    indexName = $"{ShootingMode.PointShot}: 0",
                    modeType = ShootingMode.PointShot,
                    value = 0,
                },
                new ShootingModeInfo()
                {
                    indexName = $"{ShootingMode.AimShot}: 0",
                    modeType = ShootingMode.AimShot,
                    value = 0,
                },
                new ShootingModeInfo()
                {
                    indexName = $"{ShootingMode.SightShot}: 0",
                    modeType = ShootingMode.SightShot,
                    value = 0,
                },
            };
        }

        public void SetAbility(PlayerDataInfo playerData)
        {
            ResetShootingModeInfos();
            sModeInfos = playerData.sModeInfos;
            RPM = playerData.RPM;
            range = playerData.range;
            watchAngle = playerData.watchAngle;
            MOA = playerData.MOA;
            stability = playerData.stability;
            rebound = playerData.rebound;
            propellant = playerData.propellant;
            damage = playerData.damage;
            penetrate = playerData.penetrate;
            armorBreak = playerData.armorBreak;
            critical = playerData.critical;
        }

        public void SetAbility(EnemyDataInfo enemyData)
        {
            sModeInfos = enemyData.sModeInfos;
            RPM = enemyData.RPM;
            range = enemyData.range;
            watchAngle = enemyData.watchAngle;
            MOA = enemyData.MOA;
            stability = enemyData.stability;
            rebound = enemyData.rebound;
            propellant = enemyData.propellant;
            damage = enemyData.damage;
            penetrate = enemyData.penetrate;
            armorBreak = enemyData.armorBreak;
            critical = enemyData.critical;
        }

        public void SetAddAbility()
        {
            if (charCtr.ownerType != CharacterOwner.Player) return;

            ResetShootingModeInfos();
            RPM = 0;
            range = 0;
            watchAngle = 0;
            MOA = 0;
            stability = 0;
            rebound = 0;
            propellant = 0;
            damage = 0;
            penetrate = 0;
            armorBreak = 0;
            critical = 0;

            var weaponData = charCtr.currentWeapon != null ? charCtr.currentWeapon.weaponData : null;
            var bulletData = weaponData != null && weaponData.isChamber ? weaponData.chamberBullet : null;
            ApllyAbilitys(this, weaponData, bulletData);
        }

        public Ability GetAddAbility(int bulletIndex)
        {
            var addAbility = new Ability();
            addAbility.ResetShootingModeInfos();

            var weaponData = charCtr.currentWeapon != null ? charCtr.currentWeapon.weaponData : null;
            var bulletData = weaponData != null && weaponData.isMag && weaponData.equipMag.loadedBullets.Count > 0
                         && (weaponData.equipMag.loadedBullets.Count - 1) - bulletIndex >= 0
                           ? weaponData.equipMag.loadedBullets[(weaponData.equipMag.loadedBullets.Count - 1) - bulletIndex] : null;
            ApllyAbilitys(addAbility, weaponData, bulletData);

            return addAbility;
        }

        private void ApllyAbilitys(Ability ability, WeaponDataInfo weaponData, BulletDataInfo bulletData)
        {
            if (weaponData != null)
            {
                int[] modeValues =
                {
                    weaponData.sModeInfos[(int)ShootingMode.PointShot].value,
                    weaponData.sModeInfos[(int)ShootingMode.AimShot].value,
                    weaponData.sModeInfos[(int)ShootingMode.SightShot].value
                };
                for (int i = 0; i < modeValues.Length; i++)
                {
                    var sModeInfo = ability.sModeInfos[i];
                    var newValue = sModeInfo.value + modeValues[i];
                    sModeInfo.indexName = $"{sModeInfo.modeType}: {newValue}";
                    sModeInfo.value = newValue;
                }
                ability.RPM += weaponData.RPM;
                ability.range += weaponData.range;
                ability.watchAngle += weaponData.watchAngle;
                ability.MOA += weaponData.MOA;
                ability.stability += weaponData.stability;
                ability.rebound += weaponData.rebound;
                ability.propellant += weaponData.propellant;

                if (bulletData != null)
                {
                    ability.MOA += bulletData.MOA;
                    ability.stability += bulletData.stability;
                    ability.rebound += bulletData.rebound;
                    ability.propellant += bulletData.propellant;
                    ability.damage += bulletData.damage;
                    ability.penetrate += bulletData.penetrate;
                    ability.armorBreak += bulletData.armorBreak;
                    ability.critical += bulletData.critical;
                }
            }
        }
    }

    public GameManager GameMgr
    {
        private set { gameMgr = value; }
        get { return gameMgr; }
    }

    public bool TurnEnd
    {
        private set { turnEnd = value; }
        get { return turnEnd; }
    }

    #region Old Code
    ///// <summary>
    ///// 사격 가능한 타겟 찾음
    ///// </summary>
    //public void FindTargets(FieldNode node, bool noRange)
    //{
    //    targetList.Clear();
    //    if (currentWeapon == null) return;

    //    var currentNode = node;
    //    var _targetList = ownerType != CharacterOwner.Player ? gameMgr.playerList.FindAll(x => x.state != CharacterState.Dead)
    //                                                         : gameMgr.enemyList.FindAll(x => x.state != CharacterState.Dead);
    //    for (int i = 0; i < _targetList.Count; i++)
    //    {
    //        var target = _targetList[i];
    //        var pos = currentNode.transform.position;
    //        var targetPos = target.currentNode.transform.position;
    //        var distance = DataUtility.GetDistance(pos, targetPos);
    //        var shooterCover = FindCoverNode(currentNode, target.currentNode);
    //        Cover targetCover = null;
    //        FieldNode RN = null;
    //        FieldNode LN = null;
    //        FieldNode targetRN = null;
    //        FieldNode targetLN = null;
    //        //if (shooterCover != null && shooterCover.coverType == CoverType.Full
    //        // && targetCover != null && targetCover.coverType == CoverType.Full)
    //        //{
    //        //    RN = CheckTheCanMoveNode(node, shooterCover, TargetDirection.Right);
    //        //    LN = CheckTheCanMoveNode(node, shooterCover, TargetDirection.Left);
    //        //    if (RN == null && LN == null)
    //        //    {
    //        //        //Debug.Log($"{transform.name}: 사격할 공간이 없음(=> {target.name})");
    //        //        continue;
    //        //    }
    //        //    //targetRN = CheckTheCanMoveNode(target.currentNode, targetCover, TargetDirection.Right);
    //        //    //targetLN = CheckTheCanMoveNode(target.currentNode, targetCover, TargetDirection.Left);
    //        //    //if (targetRN == null && targetLN == null)
    //        //    //{
    //        //    //    //Debug.Log($"{transform.name}: 적이 나올 공간이 없음(=> {target.name})");
    //        //    //    continue;
    //        //    //}

    //        //    if (!FindNodeOfShooterAndTarget())
    //        //    {
    //        //        //Debug.Log($"{transform.name}: 사격 불가(=> {target.name})");
    //        //    }
    //        //}
    //        //else if (shooterCover != null && shooterCover.coverType == CoverType.Full)
    //        if (shooterCover != null && shooterCover.coverType == CoverType.Full)
    //        {
    //            RN = CheckTheCanMoveNode(node, shooterCover, TargetDirection.Right);
    //            LN = CheckTheCanMoveNode(node, shooterCover, TargetDirection.Left);
    //            if (RN == null && LN == null)
    //            {
    //                //Debug.Log($"{transform.name}: 사격할 공간이 없음(=> {target.name})");
    //                //continue;
    //                shooterCover = null;
    //            }

    //            if (!FindNodeOfShooterAndTarget())
    //            {
    //                //Debug.Log($"{transform.name}: 사격 불가(=> {target.name})");
    //            }
    //        }
    //        else /*if (targetCover != null && targetCover.coverType == CoverType.Full)*/
    //        {
    //            //targetRN = CheckTheCanMoveNode(target.currentNode, targetCover, TargetDirection.Right);
    //            //targetLN = CheckTheCanMoveNode(target.currentNode, targetCover, TargetDirection.Left);
    //            //if (targetRN == null && targetLN == null)
    //            //{
    //            //    Debug.Log($"{transform.name}: 적이 나올 공간이 없음(=> {target.name})");
    //            //    continue;
    //            //}

    //            if (!FindNodeOfShooterAndTarget())
    //            {
    //                //Debug.Log($"{transform.name}: 사격 불가(=> {target.name})");
    //            }
    //        }
    //        //else
    //        //{
    //        //    if (CheckTheCoverAlongPath(Range, pos, targetPos, noRange))
    //        //    {
    //        //        var targetInfo = new TargetInfo
    //        //        {
    //        //            shooter = this,
    //        //            target = target,
    //        //            shooterNode = currentNode,
    //        //            shooterCover = shooterCover,
    //        //            targetNode = target.currentNode,
    //        //            targetCover = targetCover,
    //        //        };
    //        //        targetList.Add(targetInfo);
    //        //    }
    //        //    //else
    //        //    //{
    //        //    //    Debug.Log($"{transform.name}: 사격 불가(=> {target.name})");
    //        //    //}
    //        //}

    //        bool FindNodeOfShooterAndTarget()
    //        {
    //            FieldNode shooterNode = null;
    //            FieldNode targetNode = null;
    //            bool isRight = false;
    //            bool targetRight = false;
    //            Vector3 pos;
    //            Vector3 targetPos;
    //            var distance = 999999f;
    //            if (shooterCover != null && shooterCover.coverType == CoverType.Full)
    //            {
    //                if (RN != null)
    //                {
    //                    targetCover = FindCoverNode(target.currentNode, RN);
    //                    if (targetCover != null && targetCover.coverType == CoverType.Full)
    //                    {
    //                        targetRN = CheckTheCanMoveNode(target.currentNode, targetCover, TargetDirection.Right);
    //                        targetLN = CheckTheCanMoveNode(target.currentNode, targetCover, TargetDirection.Left);
    //                        if (targetRN != null)
    //                        {
    //                            CheckNodes(RN, targetRN, true, true);
    //                        }
    //                        if (targetLN != null)
    //                        {
    //                            CheckNodes(RN, targetLN, true, false);
    //                        }
    //                    }
    //                    else
    //                    {
    //                        CheckNodes(RN, target.currentNode, true, false);
    //                    }
    //                }
    //                if (LN != null)
    //                {
    //                    targetCover = FindCoverNode(target.currentNode, LN);
    //                    if (targetCover != null && targetCover.coverType == CoverType.Full)
    //                    {
    //                        targetRN = CheckTheCanMoveNode(target.currentNode, targetCover, TargetDirection.Right);
    //                        targetLN = CheckTheCanMoveNode(target.currentNode, targetCover, TargetDirection.Left);
    //                        if (targetRN != null)
    //                        {
    //                            CheckNodes(LN, targetRN, false, true);
    //                        }
    //                        if (targetLN != null)
    //                        {
    //                            CheckNodes(LN, targetLN, false, false);
    //                        }
    //                    }
    //                    else
    //                    {
    //                        CheckNodes(LN, target.currentNode, false, false);
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                targetCover = FindCoverNode(target.currentNode, currentNode);
    //                if (targetCover != null && targetCover.coverType == CoverType.Full)
    //                {
    //                    targetRN = CheckTheCanMoveNode(target.currentNode, targetCover, TargetDirection.Right);
    //                    targetLN = CheckTheCanMoveNode(target.currentNode, targetCover, TargetDirection.Left);
    //                    if (targetRN != null)
    //                    {
    //                        CheckNodes(currentNode, targetRN, false, true);
    //                    }
    //                    if (targetLN != null)
    //                    {
    //                        CheckNodes(currentNode, targetLN, false, false);
    //                    }
    //                }
    //                else
    //                {
    //                    CheckNodes(currentNode, target.currentNode, true, false);
    //                }
    //            }


    //            if (shooterNode != null && targetNode != null)
    //            {
    //                var targetInfo = new TargetInfo
    //                {
    //                    shooter = this,
    //                    target = target,
    //                    shooterNode = shooterNode,
    //                    shooterCover = shooterCover,
    //                    targetNode = targetNode,
    //                    targetCover = targetCover,
    //                    targetRight = targetRight,
    //                    isRight = isRight,
    //                };
    //                targetList.Add(targetInfo);
    //                return true;
    //            }
    //            else
    //            {
    //                return false;
    //            }

    //            void CheckNodes(FieldNode _shooterNode, FieldNode _targetNode, bool _isRight, bool _targetRight)
    //            {
    //                pos = _shooterNode.transform.position;
    //                targetPos = _targetNode.transform.position;
    //                if (CheckTheCoverAlongPath(Range, pos, targetPos, noRange))
    //                {
    //                    var dist = DataUtility.GetDistance(pos, targetPos);
    //                    if (dist < distance)
    //                    {
    //                        shooterNode = _shooterNode;
    //                        targetNode = _targetNode;
    //                        isRight = _isRight;
    //                        targetRight = _targetRight;
    //                        distance = dist;
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}

    ///// <summary>
    ///// 엄폐물 노드를 찾음
    ///// </summary>
    ///// <param name="shooterNode"></param>
    ///// <param name="targetNode"></param>
    ///// <returns></returns>
    //private Cover FindCoverNode(FieldNode shooterNode, FieldNode targetNode)
    //{
    //    Cover cover = null;
    //    var shooterPos = shooterNode.transform.position;
    //    var endPos = targetNode.transform.position;
    //    var dir = Vector3.zero;
    //    RayCastOfCoverLayer();

    //    var rightDir = Quaternion.Euler(0f, 90f, 0f) * dir;
    //    var interval = rightDir * 0.4f;
    //    shooterPos = shooterNode.transform.position + interval;
    //    endPos = targetNode.transform.position + interval;
    //    RayCastOfCoverLayer();

    //    var leftDir = Quaternion.Euler(0f, -90f, 0f) * dir;
    //    interval = leftDir * 0.4f;
    //    shooterPos = shooterNode.transform.position + interval;
    //    endPos = targetNode.transform.position + interval;
    //    dir = Vector3.Normalize(endPos - shooterPos);
    //    RayCastOfCoverLayer();

    //    return cover;

    //    void RayCastOfCoverLayer()
    //    {
    //        if (cover != null) return;

    //        dir = Vector3.Normalize(endPos - shooterPos);
    //        if (Physics.Raycast(shooterPos, dir, out RaycastHit hit, DataUtility.nodeSize, gameMgr.coverLayer))
    //        {
    //            var _cover = hit.collider.GetComponentInParent<Cover>();
    //            if (_cover == null) return;
    //            //if (_cover.coverType == CoverType.Half)
    //            //{
    //            //    cover = _cover;
    //            //    return;
    //            //}

    //            //switch (_cover.formType)
    //            //{
    //            //    case CoverForm.Node:
    //            //        if (!shooterNode.onAxisNodes.Contains(_cover.coverNode)) return;

    //            //        for (int i = 0; i < _cover.coverNode.allAxisNodes.Count; i++)
    //            //        {
    //            //            var axisNode = _cover.coverNode.allAxisNodes[i];
    //            //            if (axisNode == null) continue;
    //            //            if (axisNode == shooterNode) continue;

    //            //            if (axisNode.nodePos.x != shooterNode.nodePos.x && axisNode.nodePos.y != shooterNode.nodePos.y
    //            //             && axisNode.cover != null) return;
    //            //        }
    //            //        break;
    //            //    case CoverForm.Line:
    //            //        if (!shooterNode.outlines.Find(x => x.lineCover == _cover)) return;

    //            //        var _coverNode = _cover.frontNode != shooterNode ? _cover.frontNode : _cover.backNode;
    //            //        for (int i = 0; i < _coverNode.allAxisNodes.Count; i++)
    //            //        {
    //            //            var axisNode = _coverNode.allAxisNodes[i];
    //            //            if (axisNode == null) continue;
    //            //            if (axisNode == shooterNode) continue;

    //            //            if (axisNode.nodePos.x != shooterNode.nodePos.x && axisNode.nodePos.y != shooterNode.nodePos.y
    //            //             && axisNode.cover != null) return;
    //            //        }
    //            //        break;
    //            //    default:
    //            //        break;
    //            //}
    //            //cover = _cover;

    //            if (shooterNode.onAxisNodes.Contains(_cover.coverNode) || shooterNode.outlines.Find(x => x.lineCover == _cover))
    //            {
    //                cover = _cover;
    //            }
    //        }
    //    }
    //}

    ///// <summary>
    ///// 이동가능한 노드를 체크
    ///// </summary>
    ///// <param name="pos"></param>
    ///// <param name="coverNode"></param>
    ///// <param name="targetDir"></param>
    ///// <returns></returns>
    //private FieldNode CheckTheCanMoveNode(FieldNode node, Cover cover, TargetDirection targetDir)
    //{
    //    FieldNode moveNode = null;
    //    var frontDir = Vector3.Normalize(cover.transform.position - node.transform.position);
    //    var dir = targetDir == TargetDirection.Right ? Quaternion.Euler(0, 90f, 0) * frontDir : Quaternion.Euler(0, -90f, 0) * frontDir;
    //    RaycastHit hit;
    //    if (Physics.Raycast(node.transform.position, dir, out hit, DataUtility.nodeSize, gameMgr.nodeLayer))
    //    {
    //        moveNode = hit.collider.GetComponentInParent<FieldNode>();
    //        if (!moveNode.canMove)
    //        {
    //            moveNode = null;
    //        }
    //    }
    //    if (moveNode == null) return null;

    //    if (Physics.Raycast(moveNode.transform.position, frontDir, out hit, DataUtility.nodeSize, gameMgr.coverLayer))
    //    {
    //        var _cover = hit.collider.GetComponentInParent<Cover>();
    //        if (_cover.coverType == CoverType.Full) return null;
    //    }
    //    //switch (cover.formType)
    //    //{
    //    //    case CoverForm.Node:

    //    //        for (int i = 0; i < moveNode.offAxisNodes.Count; i++)
    //    //        {
    //    //            var axisNode = moveNode.offAxisNodes[i];
    //    //            if (axisNode == null) continue;

    //    //            var checkNode = cover.coverNode.allAxisNodes.Find(x => x != node && x.allAxisNodes.Contains(moveNode));
    //    //            if (checkNode != null && checkNode.cover != null) return null;
    //    //        }
    //    //        break;
    //    //    case CoverForm.Line:
    //    //        var _node = cover.frontNode != node ? cover.frontNode : cover.backNode;
    //    //        for (int i = 0; i < _node.allAxisNodes.Count; i++)
    //    //        {
    //    //            var axisNode = _node.offAxisNodes[i];
    //    //            if (axisNode == null) continue;

    //    //            var checkNode = cover.coverNode.allAxisNodes.Find(x => x != _node && x.allAxisNodes.Contains(axisNode));
    //    //            if (checkNode != null && checkNode.cover != null) return null;
    //    //        }
    //    //        break;
    //    //    default:
    //    //        break;
    //    //}

    //    return moveNode;
    //}
    #endregion
}
