using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
    None,
    Move,
    Shoot,
    Reload,
    Watch,
    Inventory,
}

public enum ScheduleState
{
    None,
    Check,
    Shoot,
    End,
}

[System.Serializable]
public struct MovePass
{
    public string indexName;
    public List<FieldNode> passNodes;
    public int moveNum;
}

[System.Serializable]
public class AttackSchedule
{
    public string indexName;
    public CoverState type;
    public TargetInfo targetInfo;
}

public class GameManager : MonoBehaviour
{
    [Header("---Access Script---")]
    public DataManager dataMgr;
    public SceneHandler sceneHlr;
    public CameraManager camMgr;
    public GameUIManager uiMgr;
    public MapEditor mapEdt;
    public InventoryManager invenMgr;

    [Header("---Access Component---")]
    [SerializeField] private ArrowPointer arrowPointer;
    [SerializeField] private Collider targetCheck;

    private Transform characterTf;

    private Transform linePoolTf;
    private Transform rangePoolTf;
    private Transform bulletsPoolTf;
    private Transform warningPoolTf;
    private Transform passPointPoolTf;
    private Transform floatTextPoolTf;

    [Header("--- Assignment Variable---")]
    public GameState gameState;
    [HideInInspector] public List<FieldNode> fieldNodes = new List<FieldNode>();

    [Header("[Move]")]
    [SerializeField] private bool addPass;
    private int expectedAction;
    private int expectedStamina;

    [SerializeField] private List<FieldNode> movableNodes = new List<FieldNode>();
    [SerializeField] private List<FieldNode> openNodes = new List<FieldNode>();
    [SerializeField] private List<FieldNode> closeNodes = new List<FieldNode>();
    [SerializeField] private List<MovePass> passList = new List<MovePass>();
    [SerializeField] private int moveNum;
    private LineRenderer moveLine;

    [Header("[Reload]")]
    private List<ItemHandler> rigMags = new List<ItemHandler>();
    private int magIndex;

    [Header("[Character]")]
    public List<CharacterController> playerList;
    public List<CharacterController> enemyList;
    public CharacterController selectChar;
    [Space(5f)]

    [SerializeField] private List<AttackSchedule> scheduleList;
    [SerializeField] private ScheduleState scheduleState;
    private CoverState targetState;
    private bool firstSchedule;
    private float timer;

    private readonly float scheduleWaitTime = 0.5f;

    [Header("[FieldNode]")]
    [SerializeField] private FieldNode targetNode;

    private DrawRange currentRange;
    [HideInInspector] public List<LineRenderer> linePool = new List<LineRenderer>();
    [HideInInspector] public List<DrawRange> rangePool = new List<DrawRange>();
    [HideInInspector] public List<Bullet> bulletPool = new List<Bullet>();
    [HideInInspector] public List<FireWarning> warningPool = new List<FireWarning>();
    [HideInInspector] public List<MeshRenderer> passPointPool = new List<MeshRenderer>();
    [HideInInspector] public List<FloatText> floatTextPool = new List<FloatText>();

    [HideInInspector] public LayerMask nodeLayer;
    [HideInInspector] public LayerMask coverLayer;
    [HideInInspector] public LayerMask watchLayer;

    private readonly int linePoolMax = 15;
    private readonly int bulletPoolMax = 30;
    private readonly int warningPoolMax = 30;
    private readonly int passPointPoolMax = 30;
    private readonly int floatTextPoolMax = 150;

    public void Start()
    {
        dataMgr = FindAnyObjectByType<DataManager>();
        sceneHlr = FindAnyObjectByType<SceneHandler>();
        camMgr = FindAnyObjectByType<CameraManager>();
        camMgr.SetComponents(this);
        uiMgr = FindAnyObjectByType<GameUIManager>();
        uiMgr.SetComponents(this);
        mapEdt = FindAnyObjectByType<MapEditor>();
        mapEdt.SetComponents(this);

        arrowPointer = FindAnyObjectByType<ArrowPointer>();
        arrowPointer.SetComponents();
        arrowPointer.gameObject.SetActive(false);
        targetCheck = GameObject.FindGameObjectWithTag("TargetCheck").GetComponent<Collider>();
        targetCheck.gameObject.SetActive(false);

        characterTf = GameObject.FindGameObjectWithTag("Characters").transform;

        var objectPool = GameObject.FindGameObjectWithTag("ObjectPool").transform;
        linePoolTf = objectPool.transform.Find("LinePool");
        rangePoolTf = objectPool.transform.Find("RangePool");
        bulletsPoolTf = objectPool.transform.Find("BulletPool");
        warningPoolTf = objectPool.transform.Find("WarningPool");
        passPointPoolTf = objectPool.transform.Find("PassPointPool");
        floatTextPoolTf = objectPool.transform.Find("FloatTextPool");

        nodeLayer = LayerMask.GetMask("Node");
        coverLayer = LayerMask.GetMask("Cover");
        watchLayer = LayerMask.GetMask("Cover") | LayerMask.GetMask("Character");

        CreateLines();
        CreateBullets();
        CreateWarnings();
        CreatePassPoint();
        CreateFloatText();

        invenMgr = FindAnyObjectByType<InventoryManager>();
        invenMgr.SetComponents(this);

        MapLoad();

        void MapLoad()
        {
            if (!dataMgr.gameData.mapLoad) return;

            var loadName = dataMgr.gameData.mapName;
            var mapData = dataMgr.LoadMapData(loadName);
            if (mapData != null)
            {
                StartCoroutine(mapEdt.Coroutine_MapLoad(mapData, false));
                dataMgr.gameData.mapLoad = false;
            }
        }
    }

    /// <summary>
    /// 캐릭터 생성
    /// </summary>
    public void CreateCharacter(CharacterOwner ownerType, Vector2 nodePos, string charID)
    {
        CharacterController charCtr = null;
        switch (ownerType)
        {
            case CharacterOwner.Player:
                charCtr = CreatePlayer();
                break;
            case CharacterOwner.Enemy:
                charCtr = CreateEnemy();
                break;
            default:
                break;
        }

        for (int i = 0; i < charCtr.weapons.Count; i++)
        {
            var weapon = charCtr.weapons[i];
            if (charCtr.currentWeapon == null)
            {
                weapon.EquipWeapon();
                weapon.WeaponSwitching("Right");
                charCtr.currentWeapon = weapon;
            }
            else
            {
                weapon.WeaponSwitching("Holster");
            }
        }
        charCtr.SetOutlinable();

        //// Set Armor
        //if (charData.armorID != "None")
        //{
        //    var armorData = dataMgr.armorData.armorInfos.Find(x => x.ID == charData.armorID);
        //    charCtr.armor = new Armor(armorData);
        //}

        // Set CharacterUI
        var charUI = Instantiate(Resources.Load<CharacterUI>("Prefabs/Character/CharacterUI"));
        charUI.transform.SetParent(characterTf, false);
        charUI.SetComponents(charCtr);
        CreateRange();
        uiMgr.SetMagNum(charCtr);

        CharacterController CreatePlayer()
        {
            var playerData = dataMgr.playerData.playerInfos.Find(x => x.ID == charID);
            var charCtr = Instantiate(Resources.Load<CharacterController>($"Prefabs/Character/{playerData.prefabName}"));
            charCtr.transform.SetParent(characterTf, false);
            var node = fieldNodes.Find(x => x.nodePos == nodePos);
            charCtr.transform.position = node.transform.position;
            charCtr.SetComponents(this, ownerType, playerData, node);

            var weapons = invenMgr.allEquips.FindAll(x => x.item != null && (x.type == EquipType.MainWeapon || x.type == EquipType.SubWeapon));
            for (int i = 0; i < weapons.Count; i++)
            {
                var weaponData = weapons[i].item.weaponData;
                if (weaponData.type == WeaponType.None) continue;

                var weapon = charCtr.GetWeapon(weaponData.weaponName);
                weapon.SetComponets(charCtr, weaponData);
            }

            return charCtr;
        }

        CharacterController CreateEnemy()
        {
            var enemyData = dataMgr.enemyData.enemyInfos.Find(x => x.ID == charID);
            var charCtr = Instantiate(Resources.Load<CharacterController>($"Prefabs/Character/{enemyData.prefabName}"));
            charCtr.transform.SetParent(characterTf, false);
            var node = fieldNodes.Find(x => x.nodePos == nodePos);
            charCtr.transform.position = node.transform.position;
            charCtr.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            charCtr.SetComponents(this, ownerType, enemyData, node);

            var weaponIDs = new string[3] { enemyData.mainWeapon1_ID, enemyData.mainWeapon2_ID, enemyData.subWeapon_ID };
            var bulletsIDs = new string[3] { enemyData.mainBullet1_ID, enemyData.mainBullet2_ID, enemyData.subBullet_ID };
            for (int i = 0; i < weaponIDs.Length; i++)
            {
                if (weaponIDs[i] == "None") continue;

                var weaponData = dataMgr.weaponData.weaponInfos.Find(x => x.ID == weaponIDs[i]).CopyData(dataMgr);
                if (weaponData != null)
                {
                    Weapon weapon = null;
                    switch (weaponData.type)
                    {
                        case WeaponType.Pistol:
                            weapon = Instantiate(Resources.Load<Weapon>($"Prefabs/Weapon/Pistol/{weaponData.prefabName}"));
                            break;
                        case WeaponType.Rifle:
                            weapon = Instantiate(Resources.Load<Weapon>($"Prefabs/Weapon/Rifle/{weaponData.prefabName}"));
                            break;
                        default:
                            break;
                    }
                    weapon.SetComponets(charCtr, weaponData);
                    weapon.magMax = weapon.weaponData.equipMag.magSize;
                    weapon.loadedNum = weapon.magMax;

                    var bulletData = dataMgr.bulletData.bulletInfos.Find(x => x.ID == bulletsIDs[i]).CopyData();
                    weapon.useBullet = bulletData;
                }
            }

            return charCtr;
        }
    }

    /// <summary>
    /// 라인 생성
    /// </summary>
    private void CreateLines()
    {
        var width = 0.03f;
        moveLine = Instantiate(Resources.Load<LineRenderer>("Prefabs/DrawLine"));
        moveLine.transform.SetParent(linePoolTf, false);
        moveLine.startWidth = width;
        moveLine.material = Resources.Load<Material>("Materials/Line/DrawLine_Move");
        moveLine.enabled = false;

        for (int i = 0; i < linePoolMax; i++)
        {
            var line = Instantiate(Resources.Load<LineRenderer>("Prefabs/DrawLine"));
            line.transform.SetParent(linePoolTf, false);
            line.startWidth = width;
            line.enabled = false;
            linePool.Add(line);
        }
    }

    /// <summary>
    /// 범위 생성
    /// </summary>
    private void CreateRange()
    {
        var range = Instantiate(Resources.Load<DrawRange>("Prefabs/DrawRange"));
        range.transform.SetParent(rangePoolTf, false);
        range.SetComponents();
        range.gameObject.SetActive(false);
        rangePool.Add(range);
    }

    /// <summary>
    /// 탄환 생성
    /// </summary>
    private void CreateBullets()
    {
        for (int i = 0; i < bulletPoolMax; i++)
        {
            var bullet = Instantiate(Resources.Load<Bullet>("Prefabs/Weapon/Bullet"));
            bullet.transform.SetParent(bulletsPoolTf, false);
            bullet.gameObject.SetActive(false);
            bulletPool.Add(bullet);
        }
    }

    private void CreateWarnings()
    {
        for (int i = 0; i < warningPoolMax; i++)
        {
            var fireWarning = Instantiate(Resources.Load<FireWarning>("Prefabs/FireWarning"));
            fireWarning.transform.SetParent(warningPoolTf, false);
            fireWarning.SetComponents();
            fireWarning.gameObject.SetActive(false);
            warningPool.Add(fireWarning);
        }
    }

    private void CreatePassPoint()
    {
        for (int i = 0; i < passPointPoolMax; i++)
        {
            var passPoint = Instantiate(Resources.Load<MeshRenderer>("Prefabs/PassPoint"));
            passPoint.transform.SetParent(passPointPoolTf, false);
            passPoint.gameObject.SetActive(false);
            passPointPool.Add(passPoint);
        }
    }

    private void CreateFloatText()
    {
        for (int i = 0; i < floatTextPoolMax; i++)
        {
            var floatText = Instantiate(Resources.Load<FloatText>("Prefabs/FloatText"));
            floatText.transform.SetParent(floatTextPoolTf, false);
            floatText.SetComponents(this);
            floatTextPool.Add(floatText);
        }
    }

    public void Update()
    {
        if (gameState == GameState.Inventory) return;

        KeyboardInput();
        MouseInput();
        PointerUpEvent();
        ScheduleProcess();
        CreatePlayer();
        CreateEnemy();
    }

    /// <summary>
    /// 키보드 입력
    /// </summary>
    private void KeyboardInput()
    {
        switch (gameState)
        {
            case GameState.Move:
                if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Space))
                {
                    ShootingAction_Move();
                }
                else if (Input.GetKeyDown(KeyCode.R))
                {
                    ReloadAction_Move();
                }
                else if (Input.GetKeyDown(KeyCode.X))
                {
                    if (selectChar.commandList.Count > 0) return;
                    if (selectChar.weapons.Count < 2) return;
                    if (!selectChar.animator.GetCurrentAnimatorStateInfo(selectChar.upperIndex).IsTag("None")) return;

                    selectChar.AddCommand(CommandType.ChangeWeapon);
                }
                else if (Input.GetKeyDown(KeyCode.T))
                {
                    RemoveTargetNode();
                    ClearLine();
                    SwitchMovableNodes(false);
                    currentRange = rangePool.Find(x => !x.gameObject.activeSelf);
                    gameState = GameState.Watch;
                }
                else if (Input.GetKeyDown(KeyCode.Escape) && passList.Count > 0)
                {
                    ClearPassPoint();
                    ShowMovableNodes(selectChar);
                    ResultNodePass(selectChar, targetNode);
                    DrawMoveLine();
                }

                if (Input.GetKeyDown(KeyCode.LeftControl))
                {
                    addPass = true;
                }
                else if (Input.GetKeyUp(KeyCode.LeftControl))
                {
                    addPass = false;
                }
                break;
            case GameState.Shoot:
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    selectChar.SetNextTargetOn();
                }
                else if (Input.GetKeyDown(KeyCode.Q))
                {
                    uiMgr.SetFireRateGauge(selectChar);
                }
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    uiMgr.SetSightGauge(selectChar);
                }
                else if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Space))
                {
                    var weapon = selectChar.currentWeapon;
                    var totalCost = weapon.weaponData.actionCost + selectChar.fiarRate + selectChar.sightRate;
                    if (totalCost > selectChar.action)
                    {
                        Debug.Log($"{selectChar.name}: 사용할 행동력이 현재 행동력보다 많음");
                        return;
                    }
                    var shootNum = DataUtility.GetShootNum(weapon.weaponData.RPM, selectChar.fiarRate);
                    var loadedAmmo = weapon.weaponData.equipMag.loadedBullets.Count;
                    //if (weapon.weaponData.isChamber) loadedAmmo++;

                    if (shootNum > loadedAmmo)
                    {
                        Debug.Log($"{selectChar.name}: 발사할 총알 수가 장전된 총알 수보다 많음");
                        return;
                    }

                    selectChar.animator.SetInteger("shootNum", shootNum);
                    if (selectChar.animator.GetBool("isCover"))
                    {
                        selectChar.AddCommand(CommandType.Aim);
                        selectChar.AddCommand(CommandType.Shoot);
                        selectChar.AddCommand(CommandType.BackCover);
                    }
                    else
                    {
                        selectChar.AddCommand(CommandType.Aim);
                        selectChar.AddCommand(CommandType.Shoot);
                    }
                    SwitchMovableNodes(false);
                    SwitchCharacterUI(true);
                    selectChar.SetTargetOff();
                    selectChar.SetAction(-totalCost);
                    camMgr.SetCameraState(CameraState.None);
                    uiMgr.SetActionPoint_Bottom(selectChar);
                    uiMgr.SetActiveAimUI(selectChar, false);
                    uiMgr.SetMagNum(selectChar, weapon.weaponData.equipMag.loadedBullets.Count - shootNum);
                    selectChar = null;
                    gameState = GameState.None;
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    var targetInfo = selectChar.targetList[selectChar.targetIndex];
                    targetInfo.target.AddCommand(CommandType.Targeting, false, transform);
                    selectChar.SetTargetOff();
                    camMgr.SetCameraState(CameraState.None);
                    uiMgr.SetUsedActionPoint_Bottom(selectChar, 0);
                    selectChar = null;
                    SwitchCharacterUI(true);
                    gameState = GameState.None;
                }
                break;
            case GameState.Reload:
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
                {
                    if (rigMags.Count < 2) return;

                    uiMgr.magIconList[magIndex].SetImageScale(false);
                    if (Input.GetKeyDown(KeyCode.A))
                    {
                        magIndex--;
                        if (magIndex < 0)
                        {
                            magIndex = rigMags.Count - 1;
                        }
                    }
                    else
                    {
                        magIndex++;
                        if (magIndex == rigMags.Count)
                        {
                            magIndex = 0;
                        }
                    }
                    uiMgr.magIconList[magIndex].SetImageScale(true);
                }
                else if (Input.GetKeyDown(KeyCode.R))
                {
                    ReloadAction_Reload();
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    uiMgr.SetActiveMagazineIcon(false);
                    uiMgr.reloadButton.SetActiveButton(false);
                    ShowMovableNodes(selectChar);
                    rigMags.Clear();
                    gameState = GameState.Move;
                }
                break;
            case GameState.Watch:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    selectChar.SetWatch();
                    currentRange = null;
                    selectChar.state = CharacterState.Watch;
                    selectChar = null;
                    gameState = GameState.None;
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    currentRange.gameObject.SetActive(false);
                    currentRange = null;
                    selectChar.state = CharacterState.None;
                    selectChar = null;
                    gameState = GameState.None;
                }
                break;
            default:
                break;
        }

        if (Input.GetKeyDown(KeyCode.End))
        {
            TurnEnd();
        }
    }

    public void ShootingAction_Move()
    {
        if (selectChar == null || selectChar.currentWeapon == null) return;

        var weapon = selectChar.currentWeapon;
        if (!weapon.weaponData.isMag /*&& !weapon.weaponData.isChamber*/)
        {
            Debug.Log($"{selectChar.name}: 무기에 장전된 총알이 없음");
            return;
        }
        if (selectChar.action < selectChar.currentWeapon.weaponData.actionCost)
        {
            Debug.Log($"{selectChar.name}: 사격에 사용할 행동력 부족");
            return;
        }

        ClearLine();
        selectChar.FindTargets(selectChar.currentNode, false);
        if (selectChar.SetTargetOn())
        {
            RemoveTargetNode();
            SwitchMovableNodes(false);
            SwitchCharacterUI(false);
            gameState = GameState.Shoot;
        }
    }

    public void ReloadAction_Move()
    {
        if (selectChar == null || selectChar.ownerType != CharacterOwner.Player) return;

        gameState = GameState.Reload;
        uiMgr.reloadButton.SetActiveButton(true);
        uiMgr.SetUsedActionPoint_Bottom(selectChar, 0);
        SwitchMovableNodes(false);
        ClearLine();
        RemoveTargetNode();
        if (selectChar.currentWeapon == null) return;

        rigMags = invenMgr.activeItem.FindAll(x => x.itemSlots.Count > 0 && x.itemSlots[0].myStorage != null
                                                && x.itemSlots[0].myStorage.type == MyStorageType.Rig
                                                && x.itemData.type == ItemType.Magazine
                                                && x.magData.compatModel.Contains(selectChar.currentWeapon.weaponData.model))
                                     .OrderByDescending(x => x.magData.loadedBullets.Count).ToList();

        if (rigMags.Count == 0) return;

        magIndex = 0;
        uiMgr.SetActiveMagazineIcon(true);
        for (int i = 0; i < rigMags.Count; i++)
        {
            var rigMag = rigMags[i];
            var magIcon = uiMgr.magIconList[i];
            magIcon.gameObject.SetActive(true);
            magIcon.SetMagazineSlider(rigMag.magData.magSize, rigMag.magData.loadedBullets.Count);
            magIcon.SetImageScale(i == 0);
        }
    }

    public void ReloadAction_Reload()
    {
        if (rigMags.Count == 0) return;

        var weapon = selectChar.currentWeapon;
        if (weapon == null) return;

        var weaponItem = invenMgr.activeItem.Find(x => x.equipSlot != null
                                                    && (x.itemData.type == ItemType.MainWeapon || x.itemData.type == ItemType.SubWeapon)
                                                    && x.weaponData.ID == weapon.weaponData.ID);
        var rigMag = rigMags[magIndex];
        if (weapon.weaponData.isMag)
        {
            var equipMag = weapon.weaponData.equipMag;
            invenMgr.SetItemInStorage(equipMag);
        }
        invenMgr.QuickEquip(weaponItem, rigMag);

        selectChar.AddCommand(CommandType.Reload);
        selectChar = null;

        uiMgr.SetActiveMagazineIcon(false);
        uiMgr.reloadButton.SetActiveButton(false);
        rigMags.Clear();
        gameState = GameState.None;
    }

    private void SwitchCharacterUI(bool value)
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            var player = playerList[i];
            player.charUI.gameObject.SetActive(value);
        }
        for (int i = 0; i < enemyList.Count; i++)
        {
            var enemy = enemyList[i];
            enemy.charUI.gameObject.SetActive(value);
        }
    }

    /// <summary>
    /// 마우스 입력
    /// </summary>
    private void MouseInput()
    {
        if (uiMgr.onButton) return;

        if (Input.GetMouseButtonDown(0))
        {
            var ray = camMgr.mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, nodeLayer))
            {
                var node = hit.collider.GetComponentInParent<FieldNode>();
                switch (gameState)
                {
                    case GameState.None:
                        if (node.charCtr != null && selectChar == null)
                        {
                            selectChar = node.charCtr;
                            if (selectChar.state == CharacterState.Watch)
                            {
                                selectChar.AimOff();
                                if (selectChar.animator.GetBool("isCover"))
                                {
                                    selectChar.AddCommand(CommandType.BackCover);
                                }
                                selectChar.watchInfo.drawRang.gameObject.SetActive(false);
                                selectChar.state = CharacterState.None;
                            }
                            ShowMovableNodes(selectChar);
                            gameState = GameState.Move;
                        }
                        break;
                    case GameState.Move:
                        if (node.charCtr != null && selectChar != null && node.charCtr != null && node.charCtr == selectChar)
                        {
                            DeselectCharacter();
                        }
                        else if (node == targetNode && node.canMove && selectChar != null)
                        {
                            if (selectChar.commandList.Count > 0) return;

                            CharacterMove(selectChar, node);
                            DeselectCharacter();
                        }
                        break;
                    case GameState.Watch:
                        selectChar.SetWatch();
                        currentRange = null;
                        selectChar.state = CharacterState.Watch;
                        selectChar = null;
                        gameState = GameState.None;
                        break;
                    default:
                        break;
                }
            }
        }
        else if (Input.GetMouseButtonDown(1) && selectChar != null)
        {
            switch (gameState)
            {
                case GameState.Move:
                    if (addPass)
                    {
                        if (passList.Count > 0 && passList[0].passNodes[0] == targetNode) return;

                        var passPoint = passPointPool.Find(x => !x.gameObject.activeSelf);
                        var pos = closeNodes[0].transform.position;
                        pos.y += 0.1f;
                        passPoint.transform.position = pos;
                        passPoint.gameObject.SetActive(true);
                        var movePass = new MovePass()
                        {
                            indexName = $"PassPoint_{closeNodes[0].name}",
                            passNodes = new List<FieldNode>(closeNodes),
                            moveNum = moveNum,
                        };
                        closeNodes.Clear();
                        passList.Insert(0, movePass);
                        ShowMovableNodes(selectChar, passList[0].passNodes[0]);
                    }
                    break;
                case GameState.Shoot:
                    break;
                case GameState.Watch:
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 포인터 업 이벤트
    /// </summary>
    private void PointerUpEvent()
    {
        if (selectChar == null) return;

        var ray = camMgr.mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, nodeLayer))
        {
            var node = hit.collider.GetComponentInParent<FieldNode>();
            switch (gameState)
            {
                case GameState.Move:
                    if (targetNode != node && node == selectChar.currentNode && passList.Count == 0)
                    {
                        arrowPointer.gameObject.SetActive(false);
                        moveLine.enabled = false;
                        ClearFireWarning();
                        RemoveTargetNode();
                        selectChar.FindTargets(node, false);
                        DrawAimLine(node);
                        node.CheckCoverNode(true);
                        targetNode = node;
                        uiMgr.SetUsedActionPoint_Bottom(selectChar, 0);
                        return;
                    }

                    if (targetNode != node && movableNodes.Contains(node))
                    {
                        RemoveTargetNode();
                        ResultNodePass(selectChar, node);
                        DrawMoveLine();

                        selectChar.FindTargets(node, false);
                        DrawAimLine(node);
                        node.CheckCoverNode(true);
                        targetNode = node;
                    }
                    //else if (!movableNodes.Contains(node))
                    //{
                    //    ClearLine();
                    //    RemoveTargetNode();
                    //}
                    break;
                case GameState.Watch:
                    if (targetNode != node)
                    {
                        if (node != null && node != selectChar.currentNode)
                        {
                            currentRange.SetRange(selectChar, node);
                            currentRange.transform.LookAt(node.transform);
                        }
                        targetNode = node;
                    }
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// TargetNode 제거
    /// </summary>
    private void RemoveTargetNode()
    {
        if (targetNode != null)
        {
            targetNode.CheckCoverNode(false);
        }
        targetNode = null;
    }

    public void DeselectCharacter()
    {
        if (selectChar == null) return;

        SwitchMovableNodes(false);
        ClearLine();
        RemoveTargetNode();
        selectChar = null;
        gameState = GameState.None;
    }

    /// <summary>
    /// 이동가능 노드 표시
    /// </summary>
    /// <param name="charCtr"></param>
    private void ShowMovableNodes(CharacterController charCtr)
    {
        SwitchMovableNodes(false);
        Queue<FieldNode> queue = new Queue<FieldNode>();
        HashSet<FieldNode> visited = new HashSet<FieldNode>();

        queue.Enqueue(charCtr.currentNode);
        visited.Add(charCtr.currentNode);

        var action = charCtr.action;
        if (charCtr.stamina < action * 5)
        {
            action = (int)(charCtr.stamina * 0.2f);
        }
        charCtr.maxMoveNum = (int)(charCtr.Mobility * action);

        int remainingShootCost = 0;
        if (charCtr.currentWeapon != null)
        {
            remainingShootCost = action - charCtr.currentWeapon.weaponData.actionCost;
            if (remainingShootCost > 0)
            {
                charCtr.shootMoveNum = (int)(charCtr.Mobility * (action - charCtr.currentWeapon.weaponData.actionCost));
            }
        }

        int moveRange = 0;
        while (queue.Count > 0 && moveRange <= charCtr.maxMoveNum)
        {
            int nodesInCurrentRange = queue.Count;
            for (int i = 0; i < nodesInCurrentRange; i++)
            {
                FieldNode node = queue.Dequeue();
                node.moveCost = DataUtility.GetMoveCost(moveRange, charCtr.Mobility);
                movableNodes.Add(node); // 이동 가능 노드로 추가

                foreach (FieldNode onAxisNode in node.onAxisNodes)
                {
                    if (onAxisNode != null && onAxisNode.canMove && !visited.Contains(onAxisNode))
                    {
                        onAxisNode.canShoot = remainingShootCost > 0 && remainingShootCost > moveRange;
                        queue.Enqueue(onAxisNode);
                        visited.Add(onAxisNode);
                    }
                }
            }
            moveRange++;
        }

        if (charCtr.ownerType == CharacterOwner.Player)
        {
            SwitchMovableNodes(true);
        }
    }

    private void ShowMovableNodes(CharacterController charCtr, FieldNode currentNode)
    {
        SwitchMovableNodes(false);
        Queue<FieldNode> queue = new Queue<FieldNode>();
        HashSet<FieldNode> visited = new HashSet<FieldNode>();

        queue.Enqueue(currentNode);
        visited.Add(currentNode);

        var passMoveNum = passList.Sum(x => x.moveNum);
        var maxMoveNum = charCtr.maxMoveNum - passMoveNum;
        int moveRange = 0;
        while (queue.Count > 0 && moveRange <= maxMoveNum)
        {
            int nodesInCurrentRange = queue.Count;
            for (int i = 0; i < nodesInCurrentRange; i++)
            {
                FieldNode node = queue.Dequeue();
                movableNodes.Add(node); // 이동 가능 노드로 추가

                foreach (FieldNode onAxisNode in node.onAxisNodes)
                {
                    if (onAxisNode != null && onAxisNode.canMove && !visited.Contains(onAxisNode))
                    {
                        onAxisNode.canShoot = charCtr.shootMoveNum > passMoveNum + moveRange;
                        queue.Enqueue(onAxisNode);
                        visited.Add(onAxisNode);
                    }
                }
            }
            moveRange++;
        }
        SwitchMovableNodes(true);
    }

    /// <summary>
    /// 이동가능 노드 표시변경
    /// </summary>
    /// <param name="value"></param>
    private void SwitchMovableNodes(bool value)
    {
        switch (value)
        {
            case true:
                for (int i = 0; i < movableNodes.Count; i++)
                {
                    var movableNode = movableNodes[i];
                    movableNode.SetNodeOutline(movableNodes);
                }
                break;
            case false:
                for (int i = 0; i < movableNodes.Count; i++)
                {
                    var movableNode = movableNodes[i];
                    movableNode.SetNodeOutline(false);
                }
                movableNodes.Clear();
                break;
        }
    }

    /// <summary>
    /// 캐릭터 이동명령
    /// </summary>
    /// <param name="charCtr"></param>
    /// <param name="targetNode"></param>
    private void CharacterMove(CharacterController charCtr, FieldNode targetNode)
    {
        //for (int i = 0; i < movableNodes.Count; i++)
        //{
        //    var movableNode = movableNodes[i];
        //    movableNode.SetNodeOutline(false);
        //}

        var movePass = closeNodes;
        if (passList.Count > 0)
        {
            foreach (var pass in passList)
            {
                var newPass = movePass.Concat(pass.passNodes).Distinct().ToList();
                movePass = newPass;
            }
            ClearPassPoint();
        }

        if (charCtr.animator.GetCurrentAnimatorStateInfo(charCtr.baseIndex).IsTag("Cover"))
        {
            charCtr.AddCommand(CommandType.LeaveCover);
        }
        charCtr.AddCommand(CommandType.Move, movePass);
        //charCtr.AddCommand(CommandType.TakeCover);
    }

    /// <summary>
    /// 이동경로 계산
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="endNode"></param>
    private void ResultNodePass(CharacterController charCtr, FieldNode endNode)
    {
        openNodes.Clear();
        closeNodes.Clear();

        //var startNode = charCtr.currentNode;
        var startNode = passList.Count > 0 ? passList[0].passNodes[0] : charCtr.currentNode;
        startNode.G = 0f;
        startNode.H = DataUtility.GetDistance(startNode.transform.position, endNode.transform.position);
        openNodes.Add(startNode);
        while (openNodes.Count > 0)
        {
            var currentNode = GetNodeWithLowestF();
            openNodes.Remove(currentNode);
            closeNodes.Add(currentNode);
            if (currentNode == endNode)
            {
                ConstructNodePath();
                break;
            }

            for (int i = 0; i < currentNode.allAxisNodes.Count; i++)
            {
                var axisNode = currentNode.allAxisNodes[i];
                if (closeNodes.Contains(axisNode)) continue;
                if (!movableNodes.Contains(axisNode)) continue;

                var _G = currentNode.G + DataUtility.GetDistance(currentNode.transform.position, axisNode.transform.position);
                if (!openNodes.Contains(axisNode) || _G < axisNode.G)
                {
                    axisNode.parentNode = currentNode;
                    axisNode.G = _G;
                    axisNode.H = DataUtility.GetDistance(axisNode.transform.position, endNode.transform.position);

                    if (!openNodes.Contains(axisNode)) openNodes.Add(axisNode);
                }
            }
        }

        FieldNode GetNodeWithLowestF()
        {
            var lowestNode = openNodes[0];
            for (int i = 0; i < openNodes.Count; i++)
            {
                var node = openNodes[i];
                if (node.F < lowestNode.F)
                {
                    lowestNode = node;
                }
            }
            return lowestNode;
        }

        void ConstructNodePath()
        {
            var passList = new List<FieldNode>();
            var currentNode = closeNodes[^1];
            while (currentNode != startNode)
            {
                passList.Add(currentNode);
                currentNode = currentNode.parentNode;
            }
            passList.Add(startNode);
            closeNodes = passList;
        }
    }

    /// <summary>
    /// 이동경로 라인 그리기
    /// </summary>
    private void DrawMoveLine()
    {
        ClearFireWarning();
        moveNum = 0;
        moveLine.enabled = true;
        moveLine.positionCount = closeNodes.Count + passList.Sum(x => x.passNodes.Count - 1);
        var height = 0.1f;
        var passNodes = new List<FieldNode>();
        for (int i = 0; i < closeNodes.Count; i++)
        {
            var node = closeNodes[i];
            moveNum += RanderAndCheck(node, i, closeNodes);
            passNodes.Add(node);
        }
        foreach (var movePass in passList)
        {
            for (int i = 0; i < movePass.passNodes.Count; i++)
            {
                var node = movePass.passNodes[i];
                RanderAndCheck(node, i, movePass.passNodes);

                if (i > 0) passNodes.Add(node);
            }
        }
        for (int i = 0; i < passNodes.Count; i++)
        {
            var passNode = passNodes[i];
            moveLine.SetPosition(i, passNode.transform.position + new Vector3(0f, height, 0f));
        }

        arrowPointer.gameObject.SetActive(true);
        arrowPointer.transform.position = closeNodes[0].transform.position + new Vector3(0f, 0.5f, 0f);
        var moveCost = (int)Mathf.Ceil((moveNum + passList.Sum(x => x.moveNum)) / selectChar.Mobility);
        arrowPointer.SetMoveCost(moveCost);
        uiMgr.SetUsedActionPoint_Bottom(selectChar, moveCost);

        int RanderAndCheck(FieldNode node, int index, List<FieldNode> nodes)
        {
            var moveNum = 0;
            var pos = node.transform.position;
            if (index + 1 < nodes.Count)
            {
                var nextNode = nodes[index + 1];
                if (node.nodePos.x != nextNode.nodePos.x && node.nodePos.y != nextNode.nodePos.y)
                {
                    moveNum += 2;
                }
                else
                {
                    moveNum++;
                }

                if (selectChar.ownerType == CharacterOwner.Player)
                {
                    selectChar.CheckWatcher(node);
                }
            }
            return moveNum;
        }
    }

    /// <summary>
    /// 조준경로 라인 그리기
    /// </summary>
    /// <param name="node"></param>
    private void DrawAimLine(FieldNode node)
    {
        var height = 0.75f;
        for (int i = 0; i < linePool.Count; i++)
        {
            var line = linePool[i];
            if (i < selectChar.targetList.Count)
            {
                var targetInfo = selectChar.targetList[i];
                var pos = node.transform.position;
                pos.y += height;
                var targetPos = targetInfo.target.currentNode.transform.position;
                targetPos.y += height;
                var dir = Vector3.Normalize(targetPos - pos);
                pos += dir * DataUtility.lineInterval;
                targetPos -= dir * DataUtility.lineInterval;

                line.enabled = true;
                line.positionCount = 2;
                line.SetPosition(0, pos);
                line.SetPosition(1, targetPos);
                if (targetInfo.targetCover == null)
                {
                    line.material = Resources.Load<Material>("Materials/Line/DrawLine_Shoot_NonCover");
                }
                else
                {
                    line.material = Resources.Load<Material>("Materials/Line/DrawLine_Shoot_Cover");
                }
            }
            else
            {
                if (!line.enabled)
                {
                    break;
                }
                else
                {
                    line.enabled = false;
                }
            }
        }
    }

    /// <summary>
    /// 라인 제거
    /// </summary>
    private void ClearLine()
    {
        arrowPointer.gameObject.SetActive(false);
        moveLine.enabled = false;
        ClearFireWarning();
        for (int i = 0; i < linePool.Count; i++)
        {
            var line = linePool[i];
            if (!line.enabled)
            {
                break;
            }
            else
            {
                line.enabled = false;
            }
        }
    }

    private void ClearPassPoint()
    {
        passList.Clear();
        for (int i = 0; i < passPointPool.Count; i++)
        {
            var passPoint = passPointPool[i];
            if (passPoint.gameObject.activeSelf)
            {
                passPoint.gameObject.SetActive(false);
            }
            else
            {
                break;
            }
        }
    }

    public void SetFloatText(Vector3 pos, string text, Color color)
    {
        var floatText = floatTextPool.Find(x => !x.gameObject.activeSelf);
        floatText.transform.SetAsFirstSibling();
        floatText.ShowFloatText(pos, text, color);
    }

    public void SetFireWarning(FieldNode node)
    {
        var fireWarning = warningPool.Find(x => !x.gameObject.activeSelf);
        fireWarning.gameObject.SetActive(true);
        fireWarning.transform.position = node.transform.position;
    }

    public void ClearFireWarning()
    {
        for (int i = 0; i < warningPool.Count; i++)
        {
            var fireWarning = warningPool[i];
            fireWarning.gameObject.SetActive(false);
        }
    }

    private void CreatePlayer()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            var ray = camMgr.mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, nodeLayer))
            {
                var node = hit.collider.GetComponentInParent<FieldNode>();
                if (node.canMove)
                {
                    CreateCharacter(CharacterOwner.Player, node.nodePos, "P0001");
                }
            }
        }
    }

    /// <summary>
    /// 적 캐릭터 생성
    /// </summary>
    private void CreateEnemy()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            var ray = camMgr.mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, nodeLayer))
            {
                var node = hit.collider.GetComponentInParent<FieldNode>();
                if (node.canMove)
                {
                    CreateCharacter(CharacterOwner.Enemy, node.nodePos, "E0001");
                }
            }
        }
    }

    /// <summary>
    /// 턴 종료
    /// </summary>
    public void TurnEnd()
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            var player = playerList[i];
            Reset(player);
        }
        for (int i = 0; i < enemyList.Count; i++)
        {
            var enemy = enemyList[i];
            Reset(enemy);
            if (enemy.currentWeapon.loadedNum == 0)
            {
                EnemyAI_Reload(enemy);
            }
            EnemyAI_Move(enemy);
        }

        void Reset(CharacterController charCtr)
        {
            var newStamina = 30 + (charCtr.action * 10);
            charCtr.SetAction(charCtr.maxAction);
            charCtr.SetStamina(newStamina);

            switch (charCtr.state)
            {
                case CharacterState.Watch:
                    charCtr.AimOff();
                    if (charCtr.animator.GetBool("isCover"))
                    {
                        charCtr.AddCommand(CommandType.BackCover);
                    }
                    charCtr.watchInfo.drawRang.gameObject.SetActive(false);
                    charCtr.state = CharacterState.None;
                    break;
                default:
                    break;
            }
        }
    }

    #region AI
    /// <summary>
    /// AI행동 처리
    /// </summary>
    /// <param name="enemy"></param>
    public void EnemyAI_Move(CharacterController enemy)
    {
        ShowMovableNodes(enemy);
        for (int i = 0; i < movableNodes.Count; i++)
        {
            var node = movableNodes[i];
            node.aiScore = enemy.aiData.score_move - node.moveCost;

            var coverScore = 99999;
            var shootScore = -99999;
            enemy.FindTargets(node, true);
            if (enemy.targetList.Count == 0)
            {
                coverScore = enemy.aiData.score_noneCover;
                shootScore = 0;
            }
            else
            {
                for (int j = 0; j < enemy.targetList.Count; j++)
                {
                    var targetInfo = enemy.targetList[j];
                    if (targetInfo.shooterCover == null)
                    {
                        coverScore = enemy.aiData.score_noneCover;
                    }
                    else if (coverScore != enemy.aiData.score_noneCover)
                    {
                        switch (targetInfo.shooterCover.coverType)
                        {
                            case CoverType.Half:
                                if (coverScore > enemy.aiData.score_halfCover) coverScore = enemy.aiData.score_halfCover;
                                break;
                            case CoverType.Full:
                                if (coverScore > enemy.aiData.score_fullCover) coverScore = enemy.aiData.score_fullCover;
                                break;
                            default:
                                break;
                        }
                    }

                    var shooterPos = targetInfo.shooterNode.transform.position;
                    var targetPos = targetInfo.targetNode.transform.position;
                    var dist = DataUtility.GetDistance(shooterPos, targetPos);
                    if (!enemy.CheckTheCoverAlongPath(enemy.currentWeapon.weaponData.range, shooterPos, targetPos, false))
                    {
                        shootScore = 0;
                        continue;
                    }

                    if (targetInfo.targetCover == null)
                    {
                        shootScore = enemy.aiData.score_noneShoot;
                    }
                    else if (shootScore != enemy.aiData.score_noneShoot)
                    {
                        switch (targetInfo.targetCover.coverType)
                        {
                            case CoverType.Half:
                                if (shootScore < enemy.aiData.score_halfShoot) shootScore = enemy.aiData.score_halfShoot;
                                break;
                            case CoverType.Full:
                                if (shootScore < enemy.aiData.score_fullShoot) shootScore = enemy.aiData.score_fullShoot;
                                break;
                            default:
                                break;
                        }
                    }

                    if (coverScore == enemy.aiData.score_noneCover && shootScore == enemy.aiData.score_noneShoot) break;
                }
            }
            node.aiScore += coverScore;
            node.aiScore += node.canShoot ? shootScore * 2 : shootScore;

            node.PosText.text = $"{node.aiScore}";
        }

        var maxScoreNode = movableNodes.OrderByDescending(n => n.aiScore).FirstOrDefault();
        if (playerList.Count > 0 && maxScoreNode.aiScore < 20)
        {
            CharacterController closeTarget;
            if (playerList.Count > 1)
            {
                var pos = enemy.currentNode.transform.position;
                closeTarget = playerList.OrderBy(x => DataUtility.GetDistance(x.currentNode.transform.position, pos)).FirstOrDefault();
            }
            else
            {
                closeTarget = playerList[0];
            }
            maxScoreNode = movableNodes.OrderBy(x => DataUtility.GetDistance(x.transform.position, closeTarget.currentNode.transform.position)).FirstOrDefault();
        }

        if (maxScoreNode != enemy.currentNode)
        {
            ResultNodePass(enemy, maxScoreNode);
            CharacterMove(enemy, maxScoreNode);
        }
        else
        {
            EnemyAI_Shoot(enemy);
        }
        enemy.targetList.Clear();

        //if (maxScoreNode.aiScore <= 40)
        //{
        //    var canWatchNode = movableNodes.FindAll(x => x.canShoot).OrderByDescending(n => n.aiScore).FirstOrDefault();
        //    if (canWatchNode != enemy.currentNode)
        //    {
        //        ResultNodePass(enemy, canWatchNode);
        //        CharacterMove(enemy, canWatchNode);
        //        enemy.state = CharacterState.Watch;
        //    }
        //    else
        //    {
        //        EnemyAI_Watch(enemy, enemy.currentNode);
        //    }
        //}
    }

    public void EnemyAI_Watch(CharacterController enemy, FieldNode currentNode)
    {
        enemy.FindTargets(currentNode, true);
        if (enemy.targetList.Count == 0) return;

        var targetNode = enemy.targetList.OrderBy(x => DataUtility.GetDistance(currentNode.transform.position, x.targetNode.transform.position)).FirstOrDefault().targetNode;
        var range = rangePool.Find(x => !x.gameObject.activeSelf);
        range.SetRange(enemy, targetNode);
        range.transform.LookAt(targetNode.transform);
        enemy.SetWatch();
        enemy.state = CharacterState.Watch;
    }

    public void EnemyAI_Shoot(CharacterController enemy)
    {
        enemy.FindTargets(enemy.currentNode, false);
        if (enemy.aiData.actionType == UseActionType.Rest || enemy.targetList.Count == 0)
        {
            enemy.AddCommand(CommandType.TakeCover);
        }
        else
        {
            enemy.targetIndex = Random.Range(0, enemy.targetList.Count);
            var targetInfo = enemy.targetList[enemy.targetIndex];
            if (enemy.action < enemy.currentWeapon.weaponData.actionCost)
            {
                enemy.AddCommand(CommandType.TakeCover, targetInfo.shooterCover, targetInfo.isRight);
            }
            else
            {
                enemy.SetTargeting(targetInfo, CharacterOwner.Enemy);
                enemy.AddCommand(CommandType.Aim, targetInfo);

                var schedule = new AttackSchedule
                {
                    type = GetScheduleType(targetInfo),
                    targetInfo = targetInfo,
                };
                schedule.indexName = $"{schedule.targetInfo.target.name}: {schedule.type}";
                scheduleList.Add(schedule);

                var totalCost = enemy.currentWeapon.weaponData.actionCost;
                var remCost = enemy.action - totalCost;
                if (remCost > 0)
                {
                    switch (enemy.aiData.actionType)
                    {
                        case UseActionType.Shoot:
                            enemy.fiarRate = remCost;
                            if (enemy.fiarRate > DataUtility.shootRateMax)
                            {
                                enemy.fiarRate = DataUtility.shootRateMax;
                            }
                            totalCost += enemy.fiarRate;
                            break;
                        case UseActionType.Aim:
                            enemy.sightRate = remCost;
                            if (enemy.sightRate > DataUtility.shootRateMax)
                            {
                                enemy.sightRate = DataUtility.shootRateMax;
                            }
                            totalCost += enemy.sightRate;
                            break;
                        default:
                            break;
                    }
                }

                var shootNum = DataUtility.GetShootNum(enemy.currentWeapon.weaponData.RPM, enemy.fiarRate);
                enemy.animator.SetInteger("shootNum", shootNum);
                enemy.SetAction(-totalCost);
                //if (targetInfo.shooterCover != null)
                //{
                //    enemy.AddCommand(CommandType.Aim);
                //    enemy.AddCommand(CommandType.Shoot);
                //    enemy.AddCommand(CommandType.BackCover);
                //}
                //else
                //{
                //    enemy.AddCommand(CommandType.Aim);
                //    enemy.AddCommand(CommandType.Shoot);
                //}
            }
        }
    }

    public void EnemyAI_Reload(CharacterController enemy)
    {
        var weapon = enemy.currentWeapon;
        weapon.loadedNum = weapon.magMax;
        enemy.AddCommand(CommandType.Reload);
    }
    #endregion

    #region Attack Schedule
    public void SetPositionOfAI(CharacterOwner ownerType)
    {
        if (ownerType == CharacterOwner.Player) return;

        var endEnemys = enemyList.FindAll(x => x.commandList.Count == 0 || x.commandList.Find(x => x.type == CommandType.Reload) != null);
        if (endEnemys.Count != enemyList.Count) return;

        scheduleList = scheduleList.OrderByDescending(x => (int)x.type).ToList();
        targetState = CoverState.None;
        scheduleState = ScheduleState.Check;
        firstSchedule = true;
    }

    private void ScheduleProcess()
    {
        if (scheduleList.Count == 0) return;
        if (scheduleState == ScheduleState.None) return;

        switch (scheduleState)
        {
            case ScheduleState.Check:
                Check();
                break;
            case ScheduleState.Shoot:
                Shoot();
                break;
            case ScheduleState.End:
                End();
                break;
            default:
                break;
        }

        void Check()
        {
            var schedule = scheduleList[0];
            var targetInfo = schedule.targetInfo;
            if (targetInfo.shooter.commandList.Count > 0) return;

            if (firstSchedule || targetState != schedule.type)
            {
                if (targetState != CoverState.None)
                {
                    targetInfo.target.AddCommand(CommandType.Targeting, false, targetInfo.target.transform);
                }
                targetInfo.shooter.SetTargeting(targetInfo, CharacterOwner.Player);
                targetState = schedule.type;
                firstSchedule = false;
            }
            scheduleState = ScheduleState.Shoot;
        }

        void Shoot()
        {
            var targetInfo = scheduleList[0].targetInfo;
            var canShoot = targetInfo.shooter.commandList.Count == 0 && targetInfo.target.commandList.Count == 0
                        && targetInfo.shooter.animator.GetCurrentAnimatorStateInfo(targetInfo.shooter.upperIndex).IsTag("Aim")
                        && (targetInfo.target.animator.GetCurrentAnimatorStateInfo(targetInfo.target.baseIndex).IsTag("Idle")
                        || targetInfo.target.animator.GetCurrentAnimatorStateInfo(targetInfo.target.baseIndex).IsTag("Targeting"));
            if (!canShoot) return;

            targetInfo.shooter.AddCommand(CommandType.Shoot, targetInfo);
            if (targetInfo.shooterCover != null)
            {
                targetInfo.shooter.AddCommand(CommandType.BackCover);
            }
            scheduleState = ScheduleState.End;
        }

        void End()
        {
            timer += Time.deltaTime;
            if (timer > scheduleWaitTime)
            {
                var target = scheduleList[0].targetInfo.target;
                scheduleList.RemoveAt(0);
                scheduleState = scheduleList.Count > 0 ? ScheduleState.Check : ScheduleState.None;
                if (scheduleList.Count > 0)
                {
                    scheduleState = ScheduleState.Check;
                }
                else
                {
                    target.AddCommand(CommandType.Targeting, false, target.transform);
                    scheduleState = ScheduleState.None;
                }
                timer = 0f;
            }
        }

        //if (scheduleState == ScheduleState.None) return;
        //if (attackSchedule.Count == 0) return;

        //var schedule = attackSchedule[0];
        //switch (scheduleState)
        //{
        //    case ScheduleState.Aim:
        //        Aim();
        //        break;
        //    case ScheduleState.Shoot:
        //        Shoot();
        //        break;
        //    case ScheduleState.End:
        //        End();
        //        break;
        //    default:
        //        break;
        //}

        //void Aim()
        //{
        //    schedule.shooter.SetTargeting(schedule, CharacterOwner.All);
        //    scheduleState = ScheduleState.Shoot;
        //}

        //void Shoot()
        //{
        //    var canShot = schedule.target.commandList.Count == 0
        //               && schedule.target.animator.GetCurrentAnimatorStateInfo(schedule.target.baseIndex).IsTag("Targeting");
        //    if (!canShot) return;

        //    schedule.shooter.AddCommand(CommandType.Shoot, schedule);
        //    if (schedule.shooterCover != null)
        //    {
        //        schedule.shooter.AddCommand(CommandType.BackCover);
        //    }
        //    scheduleState = ScheduleState.End;
        //}

        //void End()
        //{
        //    var endAction = schedule.shooter.commandList.Count == 0
        //               && (schedule.target.animator.GetCurrentAnimatorStateInfo(schedule.target.baseIndex).IsTag("Idle")
        //               || schedule.target.animator.GetCurrentAnimatorStateInfo(schedule.target.baseIndex).IsTag("Cover"));
        //    if (!endAction) return;

        //    attackSchedule.RemoveAt(0);
        //    scheduleState = attackSchedule.Count > 0 ? ScheduleState.Aim : ScheduleState.None;
        //}
    }

    private CoverState GetScheduleType(TargetInfo targetInfo)
    {
        if (targetInfo.targetCover != null)
        {
            switch (targetInfo.targetCover.coverType)
            {
                case CoverType.Half:
                    return CoverState.Half;
                case CoverType.Full:
                    return targetInfo.targetRight ? CoverState.FullRight : CoverState.FullLeft;
                default:
                    return CoverState.None;
            }
        }
        else
        {
            return CoverState.None;
        }
    }
    #endregion
}
