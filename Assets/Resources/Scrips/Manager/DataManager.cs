using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using TMPro;
using Unity.VisualScripting;

[System.Serializable]
public struct ObjectData
{
    public MapEditorType objectType;
    public string itemName;
    public TargetDirection setDir;
}

[System.Serializable]
public class NodeData
{
    public Vector2 pos;

    [Header("[Floor]")]
    public bool isMesh;
    public string floorItemName;
    public Quaternion floorRot;

    [Header("[NodeCover]")]
    public bool isNodeCover;
    public FindNodeType nCoverType;

    [Header("[LineCover]")]
    public bool isLineCover;
    public TargetDirection[] lCoverDirs;
    public FindNodeType[] lCoverTypes;

    [Header("[Object]")]
    public bool isObject;
    public ObjectData[] objectDatas;

    [Header("[Marker]")]
    public bool isMarker;
    public MarkerType markerType;
    public EnemyMarker enemyType;
    public BaseCampMarker baseType;
}

[System.Serializable]
public class MapData
{
    public Vector2 mapSize;
    public NodeData[] nodeDatas;
}

public class DataManager : MonoBehaviour
{
    private void Awake()
    {
        if (gameData == null) gameData = Resources.Load<GameData>("ScriptableObjects/GameData");

        if (gameData.dataMgr == null)
        {
            DontDestroyOnLoad(gameObject);
            SetComponents();
            gameData.dataMgr = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetComponents()
    {
        //if (gameData == null) gameData = Resources.Load<GameData>("ScriptableObjects/GameData");

        if (playerData == null) playerData = Resources.Load<PlayerData>("ScriptableObjects/PlayerData");

        if (enemyData == null) enemyData = Resources.Load<EnemyData>("ScriptableObjects/EnemyData");

        if (aiData == null) aiData = Resources.Load<AIData>("ScriptableObjects/AIData");

        if (stageData == null) stageData = Resources.Load<StageData>("ScriptableObjects/StageData");

        if (itemData == null) itemData = Resources.Load<ItemData>("ScriptableObjects/ItemData");

        if (weaponData == null) weaponData = Resources.Load<WeaponData>("ScriptableObjects/WeaponData");

        if (partsData == null) partsData = Resources.Load<WeaponPartsData>("ScriptableObjects/WeaponPartsData");

        if (magData == null) magData = Resources.Load<MagazineData>("ScriptableObjects/MagazineData");

        if (grenadeData == null) grenadeData = Resources.Load<GrenadeData>("ScriptableObjects/GrenadeData");

        if (armorData == null) armorData = Resources.Load<ArmorData>("ScriptableObjects/ArmorData");

        if (rigData == null) rigData = Resources.Load<RigData>("ScriptableObjects/RigData");

        if (backpackData == null) backpackData = Resources.Load<BackpackData>("ScriptableObjects/BackpackData");

        if (itemOptionData == null) itemOptionData = Resources.Load<ItemOptionData>("ScriptableObjects/ItemOptionData");

        if (optionSheetData == null) optionSheetData = Resources.Load<OptionSheetData>("ScriptableObjects/OptionSheetData");

        if (dropTableData == null) dropTableData = Resources.Load<DropTableData>("ScriptableObjects/DropTableData");
    }

    #region GameData
    [HideInInspector] public GameData gameData;

    #endregion

    #region MapEditor
    public void SaveMapData(string saveName, Vector2 _mapSize, List<FieldNode> _fieldNodes)
    {
        var mapData = new MapData();
        mapData.mapSize = _mapSize;
        var nodeDatas = new List<NodeData>();
        for (int i = 0; i < _fieldNodes.Count; i++)
        {
            var node = _fieldNodes[i];
            var nodeData = new NodeData();
            nodeData.pos = node.nodePos;

            // Floor Data
            nodeData.isMesh = node.Mesh.enabled;
            if (nodeData.isMesh)
            {
                nodeData.floorItemName = node.Mesh.material.name.Split(' ')[0];
                nodeData.floorRot = node.Mesh.transform.localRotation;
            }

            // NodeCover Data
            nodeData.isNodeCover = node.cover != null || node.unableMove.enabled;
            if (nodeData.isNodeCover)
            {
                if (node.unableMove.enabled)
                {
                    nodeData.nCoverType = FindNodeType.SetUnableMove;
                }
                else if (node.cover != null)
                {
                    switch (node.cover.coverType)
                    {
                        case CoverType.Half:
                            nodeData.nCoverType = FindNodeType.SetHalfCover;
                            break;
                        case CoverType.Full:
                            nodeData.nCoverType = FindNodeType.SetFullCover;
                            break;
                        default:
                            break;
                    }
                }
            }

            // LineCover Data
            nodeData.isLineCover = (node.outlines.Find(x => x.lineCover != null) != null) || (node.outlines.Find(x => x.unableMove.enabled) != null);
            if (nodeData.isLineCover)
            {
                nodeData.lCoverDirs = new TargetDirection[4];
                nodeData.lCoverTypes = new FindNodeType[4];
                for (int j = 0; j < node.outlines.Count; j++)
                {
                    var outline = node.outlines[j];
                    if (outline.unableMove.enabled)
                    {
                        nodeData.lCoverDirs[j] = (TargetDirection)j;
                        nodeData.lCoverTypes[j] = FindNodeType.SetUnableMove;
                    }
                    else if ((outline.lineCover != null))
                    {
                        nodeData.lCoverDirs[j] = (TargetDirection)j;
                        switch (outline.lineCover.coverType)
                        {
                            case CoverType.Half:
                                nodeData.lCoverTypes[j] = FindNodeType.SetHalfCover;
                                break;
                            case CoverType.Full:
                                nodeData.lCoverTypes[j] = FindNodeType.SetFullCover;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            // Object Data
            nodeData.isObject = node.setObjects.Count > 0 && node.setObjects.Find(x => x.setNode == node) != null;
            if (nodeData.isObject)
            {
                var objectDatas = new List<ObjectData>();
                for (int j = 0; j < node.setObjects.Count; j++)
                {
                    var _object = node.setObjects[j];
                    if (_object.setNode != node) continue;

                    var objectData = new ObjectData
                    {
                        objectType = _object.type,
                        itemName = _object.setObject.name,
                        setDir = _object.setDir,
                    };
                    objectDatas.Add(objectData);
                }
                nodeData.objectDatas = objectDatas.ToArray();
            }

            // Marker Data
            nodeData.isMarker = node.Marker.activeSelf;
            if (nodeData.isMarker)
            {
                nodeData.markerType = node.markerType;
                switch (nodeData.markerType)
                {
                    case MarkerType.Enemy:
                        nodeData.enemyType = node.enemyType;
                        break;
                    case MarkerType.Base:
                        nodeData.baseType = node.baseType;
                        break;
                    default:
                        break;
                }
            }
            nodeDatas.Add(nodeData);
        }
        mapData.nodeDatas = nodeDatas.ToArray();

        var saveData = JsonUtility.ToJson(mapData, true);
        var folderPath = Application.dataPath + DataUtility.mapDataPath;
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, $"{saveName}.json");
        File.WriteAllText(filePath, saveData);
    }

    public void ReadMapLoadIndex(TMP_Dropdown dropdown)
    {
        dropdown.ClearOptions();
        var folderPath = Application.dataPath + DataUtility.mapDataPath;
        if (Directory.Exists(folderPath))
        {
            string[] fileNames = Directory.GetFiles(folderPath, "*.json");
            var options = new List<TMP_Dropdown.OptionData>();
            for (int i = 0; i < fileNames.Length; i++)
            {
                var fileName = Path.GetFileName(fileNames[i]).Split('.')[0];
                var option = new TMP_Dropdown.OptionData(fileName);
                options.Add(option);
            }
            dropdown.AddOptions(options);
        }
    }

    public MapData LoadMapData(string loadName)
    {
        var folderPath = Application.dataPath + DataUtility.mapDataPath;
        var filePath = Path.Combine(folderPath, $"{loadName}.json");
        if (File.Exists(filePath))
        {
            var jsonData = File.ReadAllText(filePath);
            var mapData = JsonUtility.FromJson<MapData>(jsonData);
            return mapData;
        }
        else
        {
            Debug.Log("Not Found");
            return null;
        }
    }
    #endregion

    #region Player Data
    [HideInInspector] public PlayerData playerData;
    private readonly string playerDB = "https://docs.google.com/spreadsheets/d/1K4JDpojMJeJPpvA-u_sOK591Y16PBG45T77HCHyn_9w/export?format=tsv&gid=676425891&range=A3:AC";
    private enum PlayerVariable
    {
        ID,
        PrefabName,
        PlayerName,
        Strength,
        Vitality,
        Intellect,
        Wisdom,
        Agility,
        Dexterity,
        MaxAction,
        MaxHealth,
        MaxStamina,
        Sight,
        Aiming,
        Reaction,
        AimShot_point,
        AimShot_aim,
        AimShot_sight,
        RPM,
        Range,
        WatchAngle,
        MOA,
        Stability,
        Rebound,
        Propellant,
        Damage,
        Penetrate,
        ArmorBreak,
        Critical,
    }

    public void UpdatePlayerData()
    {
        if (playerData == null) playerData = Resources.Load<PlayerData>("ScriptableObjects/PlayerData");
        if (playerData.playerInfos.Count > 0) playerData.playerInfos.Clear();

        StartCoroutine(ReadPlayerData());

        IEnumerator ReadPlayerData()
        {
            UnityWebRequest www = UnityWebRequest.Get(playerDB);
            yield return www.SendWebRequest();

            var text = www.downloadHandler.text;
            var datas = text.Split('\n');
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i].Split('\t');
                var playerInfo = new PlayerDataInfo
                {
                    indexName = $"{data[(int)PlayerVariable.ID]}: {data[(int)PlayerVariable.PlayerName]}",
                    ID = data[(int)PlayerVariable.ID],
                    prefabName = data[(int)PlayerVariable.PrefabName],
                    charName = data[(int)PlayerVariable.PlayerName],
                    strength = int.Parse(data[(int)PlayerVariable.Strength]),
                    vitality = int.Parse(data[(int)PlayerVariable.Vitality]),
                    intellect = int.Parse(data[(int)PlayerVariable.Intellect]),
                    wisdom = int.Parse(data[(int)PlayerVariable.Wisdom]),
                    agility = int.Parse(data[(int)PlayerVariable.Agility]),
                    dexterity = int.Parse(data[(int)PlayerVariable.Dexterity]),
                    maxAction = int.Parse(data[(int)PlayerVariable.MaxAction]),
                    maxHealth = int.Parse(data[(int)PlayerVariable.MaxHealth]),
                    maxStamina = int.Parse(data[(int)PlayerVariable.MaxStamina]),
                    sight = float.Parse(data[(int)PlayerVariable.Sight]),
                    aiming = int.Parse(data[(int)PlayerVariable.Aiming]),
                    reaction = int.Parse(data[(int)PlayerVariable.Reaction]),
                    aimShot_point = int.Parse(data[(int)PlayerVariable.AimShot_point]),
                    aimShot_aim = int.Parse(data[(int)PlayerVariable.AimShot_aim]),
                    aimShot_sight = int.Parse(data[(int)PlayerVariable.AimShot_sight]),
                    RPM = int.Parse(data[(int)PlayerVariable.RPM]),
                    range = float.Parse(data[(int)PlayerVariable.Range]),
                    watchAngle = int.Parse(data[(int)PlayerVariable.WatchAngle]),
                    MOA = float.Parse(data[(int)PlayerVariable.MOA]),
                    stability = int.Parse(data[(int)PlayerVariable.Stability]),
                    rebound = int.Parse(data[(int)PlayerVariable.Rebound]),
                    propellant = int.Parse(data[(int)PlayerVariable.Propellant]),
                    damage = int.Parse(data[(int)PlayerVariable.Damage]),
                    penetrate = int.Parse(data[(int)PlayerVariable.Penetrate]),
                    armorBreak = int.Parse(data[(int)PlayerVariable.ArmorBreak]),
                    critical = int.Parse(data[(int)PlayerVariable.Critical]),
                };
                playerData.playerInfos.Add(playerInfo);
            }
            Debug.Log("Update Player Data");
        }
    }
    #endregion

    #region Enemy Data
    [HideInInspector] public EnemyData enemyData;
    private readonly string enemyDB = "https://docs.google.com/spreadsheets/d/1K4JDpojMJeJPpvA-u_sOK591Y16PBG45T77HCHyn_9w/export?format=tsv&gid=437348199&range=A4:AS";
    private enum EnemyVariable
    {
        ID,
        PrefabName,
        EnemyName,
        AI_ID,
        DropTableID,
        UniqueItemID,
        Strength,
        Vitality,
        Intellect,
        Wisdom,
        Agility,
        Dexterity,
        MaxAction,
        MaxHealth,
        MaxStamina,
        Sight,
        Aiming,
        Reaction,
        AimShot,
        RPM,
        Range,
        WatchAngle,
        MOA,
        Stability,
        Rebound,
        Propellant,
        Damage,
        Penetrate,
        ArmorBreak,
        Critical,
        MainWeapon1_type,
        MainWeapon1_prefabName,
        MainWeapon1_meshType,
        MainWeapon1_pelletNum,
        MainWeapon1_magMax,
        MainWeapon2_type,
        MainWeapon2_prefabName,
        MainWeapon2_meshType,
        MainWeapon2_pelletNum,
        MainWeapon2_magMax,
        SubWeapon_type,
        SubWeapon_prefabName,
        SubWeapon_meshType,
        SubWeapon_pelletNum,
        SubWeapon_magMax,
    }

    public void UpdateEnemyData()
    {
        if (enemyData == null) enemyData = Resources.Load<EnemyData>("ScriptableObjects/EnemyData");
        if (enemyData.enemyInfos.Count > 0) enemyData.enemyInfos.Clear();

        StartCoroutine(ReadEnemyData());

        IEnumerator ReadEnemyData()
        {
            UnityWebRequest www = UnityWebRequest.Get(enemyDB);
            yield return www.SendWebRequest();

            var text = www.downloadHandler.text;
            var datas = text.Split('\n');
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i].Split('\t');
                var enemyInfo = new EnemyDataInfo
                {
                    indexName = $"{data[(int)EnemyVariable.ID]}: {data[(int)EnemyVariable.EnemyName]}",
                    ID = data[(int)EnemyVariable.ID],
                    prefabName = data[(int)EnemyVariable.PrefabName],
                    charName = data[(int)EnemyVariable.EnemyName],
                    aiID = data[(int)EnemyVariable.AI_ID],
                    dropTableID = data[(int)EnemyVariable.DropTableID],
                    uniqueItemID = data[(int)EnemyVariable.UniqueItemID],
                    strength = int.Parse(data[(int)EnemyVariable.Strength]),
                    vitality = int.Parse(data[(int)EnemyVariable.Vitality]),
                    intellect = int.Parse(data[(int)EnemyVariable.Intellect]),
                    wisdom = int.Parse(data[(int)EnemyVariable.Wisdom]),
                    agility = int.Parse(data[(int)EnemyVariable.Agility]),
                    dexterity = int.Parse(data[(int)EnemyVariable.Dexterity]),
                    maxAction = int.Parse(data[(int)EnemyVariable.MaxAction]),
                    maxHealth = int.Parse(data[(int)EnemyVariable.MaxHealth]),
                    maxStamina = int.Parse(data[(int)EnemyVariable.MaxStamina]),
                    sight = float.Parse(data[(int)EnemyVariable.Sight]),
                    aiming = int.Parse(data[(int)EnemyVariable.Aiming]),
                    reaction = int.Parse(data[(int)EnemyVariable.Reaction]),
                    aimShot = int.Parse(data[(int)EnemyVariable.AimShot]),
                    RPM = int.Parse(data[(int)EnemyVariable.RPM]),
                    range = float.Parse(data[(int)EnemyVariable.Range]),
                    watchAngle = int.Parse(data[(int)EnemyVariable.WatchAngle]),
                    MOA = float.Parse(data[(int)EnemyVariable.MOA]),
                    stability = int.Parse(data[(int)EnemyVariable.Stability]),
                    rebound = int.Parse(data[(int)EnemyVariable.Rebound]),
                    propellant = int.Parse(data[(int)EnemyVariable.Propellant]),
                    damage = int.Parse(data[(int)EnemyVariable.Damage]),
                    penetrate = int.Parse(data[(int)EnemyVariable.Penetrate]),
                    armorBreak = int.Parse(data[(int)EnemyVariable.ArmorBreak]),
                    critical = int.Parse(data[(int)EnemyVariable.Critical]),
                    mainWeapon1 = ReadEnemyWeapon(data[(int)EnemyVariable.MainWeapon1_type],
                                                  data[(int)EnemyVariable.MainWeapon1_prefabName],
                                                  data[(int)EnemyVariable.MainWeapon1_meshType],
                                                  data[(int)EnemyVariable.MainWeapon1_pelletNum],
                                                  data[(int)EnemyVariable.MainWeapon1_magMax]),
                    mainWeapon2 = ReadEnemyWeapon(data[(int)EnemyVariable.MainWeapon2_type],
                                                  data[(int)EnemyVariable.MainWeapon2_prefabName],
                                                  data[(int)EnemyVariable.MainWeapon2_meshType],
                                                  data[(int)EnemyVariable.MainWeapon2_pelletNum],
                                                  data[(int)EnemyVariable.MainWeapon2_magMax]),
                    subWeapon = ReadEnemyWeapon(data[(int)EnemyVariable.SubWeapon_type],
                                                data[(int)EnemyVariable.SubWeapon_prefabName],
                                                data[(int)EnemyVariable.SubWeapon_meshType],
                                                data[(int)EnemyVariable.SubWeapon_pelletNum],
                                                data[(int)EnemyVariable.SubWeapon_magMax]),
                };
                enemyData.enemyInfos.Add(enemyInfo);
            }
            Debug.Log("Update Enemy Data");
        }

        EnemyWeapon ReadEnemyWeapon(string typeData, string prefabNameData, string meshType, string pelletNum, string magMaxData)
        {
            var enemyWeapon = new EnemyWeapon()
            {
                type = (WeaponType)int.Parse(typeData),
                prefabName = prefabNameData,
                meshType = int.Parse(meshType),
                pelletNum = int.Parse(pelletNum),
                magMax = int.Parse(magMaxData),
            };

            return enemyWeapon;
        }
    }
    #endregion

    #region AI Data
    [HideInInspector] public AIData aiData;
    private readonly string aiDB = "https://docs.google.com/spreadsheets/d/1K4JDpojMJeJPpvA-u_sOK591Y16PBG45T77HCHyn_9w/export?format=tsv&gid=590787557&range=A3:J";
    private enum AIVariable
    {
        ID,
        TypeName,
        ActionType,
        Score_move,
        Score_noneCover,
        Score_halfCover,
        Score_fullCover,
        Score_noneShoot,
        Score_halfShoot,
        Score_fullShoot,
    }

    public void UpdateAIData()
    {
        if (aiData == null) aiData = Resources.Load<AIData>("ScriptableObjects/AIData");
        if (aiData.aiInfos.Count > 0) aiData.aiInfos.Clear();

        StartCoroutine(ReadAIData());

        IEnumerator ReadAIData()
        {
            UnityWebRequest www = UnityWebRequest.Get(aiDB);
            yield return www.SendWebRequest();

            var text = www.downloadHandler.text;
            var datas = text.Split('\n');
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i].Split('\t');
                var enemyInfo = new AIDataInfo
                {
                    indexName = $"{data[(int)AIVariable.ID]}: {data[(int)AIVariable.TypeName]}",
                    ID = data[(int)AIVariable.ID],
                    typeName = data[(int)AIVariable.TypeName],
                    actionType = (UseActionType)int.Parse(data[(int)AIVariable.ActionType]),
                    score_move = int.Parse(data[(int)AIVariable.Score_move]),
                    score_noneCover = int.Parse(data[(int)AIVariable.Score_noneCover]),
                    score_halfCover = int.Parse(data[(int)AIVariable.Score_halfCover]),
                    score_fullCover = int.Parse(data[(int)AIVariable.Score_fullCover]),
                    score_noneShoot = int.Parse(data[(int)AIVariable.Score_noneShoot]),
                    score_halfShoot = int.Parse(data[(int)AIVariable.Score_halfShoot]),
                    score_fullShoot = int.Parse(data[(int)AIVariable.Score_fullShoot]),
                };
                aiData.aiInfos.Add(enemyInfo);
            }
            Debug.Log("Update AI Data");
        }
    }
    #endregion

    #region Stage Data
    [HideInInspector] public StageData stageData;
    private readonly string stageDB = "https://docs.google.com/spreadsheets/d/1K4JDpojMJeJPpvA-u_sOK591Y16PBG45T77HCHyn_9w/export?format=tsv&gid=1301337997&range=A3:K";
    private enum StageVariable
    {
        ID,
        StageName,
        Level,
        WaveNum,
        MapList,
        BossMap,
        ShortRangeEnemys,
        MiddleRangeEnemys,
        LongRangeEnemys,
        EliteEnemys,
        BossEnemy,
    }

    public void UpdateStageData()
    {
        if (stageData == null) stageData = Resources.Load<StageData>("ScriptableObjects/StageData");
        if (stageData.stageInfos.Count > 0) stageData.stageInfos.Clear();

        StartCoroutine(ReadStageData());

        IEnumerator ReadStageData()
        {
            UnityWebRequest www = UnityWebRequest.Get(stageDB);
            yield return www.SendWebRequest();

            var text = www.downloadHandler.text;
            var datas = text.Split('\n');
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i].Split('\t');
                var stageInfo = new StageDataInfo
                {
                    indexName = $"{data[(int)StageVariable.ID]}",
                    ID = data[(int)StageVariable.ID],
                    stageName = data[(int)StageVariable.StageName],
                    level = (StageLevel)int.Parse(data[(int)StageVariable.Level]),
                    waveNum = int.Parse(data[(int)StageVariable.WaveNum]),
                    mapList = ReadMapList(data[(int)StageVariable.MapList]),
                    bossMap = data[(int)StageVariable.BossMap],
                    shortRangeEnemys = ReadSpawnEnemyInfos(data[(int)StageVariable.ShortRangeEnemys]),
                    middleRangeEnemys = ReadSpawnEnemyInfos(data[(int)StageVariable.MiddleRangeEnemys]),
                    longRangeEnemys = ReadSpawnEnemyInfos(data[(int)StageVariable.LongRangeEnemys]),
                    eliteEnemys = ReadSpawnEnemyInfos(data[(int)StageVariable.EliteEnemys]),
                    bossEnemy = ReadSpawnEnemyInfo(data[(int)StageVariable.BossEnemy]),
                };
                stageData.stageInfos.Add(stageInfo);
            }
            Debug.Log("Update Stage Data");
        }

        List<string> ReadMapList(string mapData)
        {
            var mapList = new List<string>();
            var mapInfos = mapData.Split(' ');
            for (int i = 0; i < mapInfos.Length; i++)
            {
                var mapInfo = mapInfos[i];
                mapList.Add(mapInfo);
            }

            return mapList;
        }

        List<SpawnEnemyInfo> ReadSpawnEnemyInfos(string enemyDatas)
        {
            var enemyList = new List<SpawnEnemyInfo>();
            var enemyInfos = enemyDatas.Split(' ');
            if (enemyInfos[0] == "None") return null;

            for (int i = 0; i < enemyInfos.Length; i++)
            {
                enemyList.Add(ReadSpawnEnemyInfo(enemyInfos[i]));
            }

            return enemyList;
        }

        SpawnEnemyInfo ReadSpawnEnemyInfo(string enemyDatas)
        {
            var enemyData = enemyDatas.Split('(', ')');
            var enemyInfo = new SpawnEnemyInfo()
            {
                ID = enemyData[0],
                level = int.Parse(enemyData[1]),
            };

            return enemyInfo;
        }
    }
    #endregion

    #region Item Data
    [HideInInspector] public ItemData itemData;
    private readonly string itemDB = "https://docs.google.com/spreadsheets/d/1K4JDpojMJeJPpvA-u_sOK591Y16PBG45T77HCHyn_9w/export?format=tsv&gid=267991501&range=A3:L";
    private enum ItemVariable
    {
        ID,
        DataID,
        ItemName,
        Type,
        Level,
        Rarity,
        MaxNesting,
        Price,
        X_Size,
        Y_Size,
        AddOption,
        SetDropTable,
    }

    public void UpdateItemData()
    {
        if (itemData == null)
        {
            itemData = Resources.Load<ItemData>("ScriptableObjects/ItemData");
        }

        if (itemData.itemInfos.Count > 0)
        {
            itemData.itemInfos.Clear();
        }
        StartCoroutine(ReadItemData());

        IEnumerator ReadItemData()
        {
            UnityWebRequest www = UnityWebRequest.Get(itemDB);
            yield return www.SendWebRequest();

            var text = www.downloadHandler.text;
            var datas = text.Split('\n');
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i].Split('\t');
                var itemInfo = new ItemDataInfo
                {
                    indexName = $"{data[(int)ItemVariable.ID]}: {data[(int)ItemVariable.ItemName]}",
                    ID = data[(int)ItemVariable.ID],
                    dataID = data[(int)ItemVariable.DataID],
                    itemName = data[(int)ItemVariable.ItemName],
                    type = (ItemType)int.Parse(data[(int)ItemVariable.Type]),
                    level = int.Parse(data[(int)ItemVariable.Level]),
                    rarity = (ItemRarity)int.Parse(data[(int)ItemVariable.Rarity]),
                    maxNesting = int.Parse(data[(int)ItemVariable.MaxNesting]),
                    price = int.Parse(data[(int)ItemVariable.Price]),
                    size = new Vector2Int(int.Parse(data[(int)ItemVariable.X_Size]), int.Parse(data[(int)ItemVariable.Y_Size])),
                    addOption = System.Convert.ToBoolean(int.Parse(data[(int)ItemVariable.AddOption])),
                    setDropTable = System.Convert.ToBoolean(int.Parse(data[(int)ItemVariable.SetDropTable])),
                };
                itemData.itemInfos.Add(itemInfo);
            }
            Debug.Log("Update Item Data");
        }
    }
    #endregion

    #region Weapon Data
    [HideInInspector] public WeaponData weaponData;
    private readonly string weaponDB = "https://docs.google.com/spreadsheets/d/1K4JDpojMJeJPpvA-u_sOK591Y16PBG45T77HCHyn_9w/export?format=tsv&gid=719783222&range=A3:Y";
    private enum WeaponVariable
    {
        ID,
        PrefabName,
        WeaponName,
        Model,
        Caliber,
        Weight,
        IsMain,
        WeaponType,
        AimShot_point,
        AimShot_aim,
        AimShot_sight,
        RPM,
        Range,
        WatchAngle,
        MOA,
        Stability,
        Rebound,
        ActionCost,
        UseMagazine,
        UseMuzzle,
        UseScope,
        UseAttachment,
        UseUnderBarrel,
        EquipMagID,
        EquipPartsIDs,
    }

    public void UpdateWeaponData()
    {
        if (weaponData == null) weaponData = Resources.Load<WeaponData>("ScriptableObjects/WeaponData");
        if (weaponData.weaponInfos.Count > 0) weaponData.weaponInfos.Clear();

        StartCoroutine(ReadWeaponData());

        IEnumerator ReadWeaponData()
        {
            UnityWebRequest www = UnityWebRequest.Get(weaponDB);
            yield return www.SendWebRequest();

            var text = www.downloadHandler.text;
            var datas = text.Split('\n');
            int intMagMax;
            for (int i = 0; i < datas.Length; i++)
            {
                intMagMax = 0;
                var data = datas[i].Split('\t');
                var weaponInfo = new WeaponDataInfo
                {
                    indexName = $"{data[(int)WeaponVariable.ID]}: {data[(int)WeaponVariable.WeaponName]}",
                    ID = data[(int)WeaponVariable.ID],
                    prefabName = data[(int)WeaponVariable.PrefabName],
                    weaponName = data[(int)WeaponVariable.WeaponName],
                    model = int.Parse(data[(int)WeaponVariable.Model]),
                    caliber = float.Parse(data[(int)WeaponVariable.Caliber]),
                    weight = float.Parse(data[(int)WeaponVariable.Weight]),
                    isMain = System.Convert.ToBoolean(int.Parse(data[(int)WeaponVariable.IsMain])),
                    type = (WeaponType)int.Parse(data[(int)WeaponVariable.WeaponType]),
                    aimShot_point = int.Parse(data[(int)WeaponVariable.AimShot_point]),
                    aimShot_aim = int.Parse(data[(int)WeaponVariable.AimShot_aim]),
                    aimShot_sight = int.Parse(data[(int)WeaponVariable.AimShot_sight]),
                    RPM = int.Parse(data[(int)WeaponVariable.RPM]),
                    range = float.Parse(data[(int)WeaponVariable.Range]),
                    watchAngle = int.Parse(data[(int)WeaponVariable.WatchAngle]),
                    MOA = float.Parse(data[(int)WeaponVariable.MOA]),
                    stability = int.Parse(data[(int)WeaponVariable.Stability]),
                    rebound = int.Parse(data[(int)WeaponVariable.Rebound]),
                    actionCost = int.Parse(data[(int)WeaponVariable.ActionCost]),
                    useMagazine = ReadUseMagazineSize(data[(int)WeaponVariable.UseMagazine]),
                    useMuzzle = ReadUsePartsSize(data[(int)WeaponVariable.UseMuzzle]),
                    useSight = ReadUsePartsSize(data[(int)WeaponVariable.UseScope]),
                    useUnderRail = ReadUsePartsSize(data[(int)WeaponVariable.UseAttachment]),
                    useRail = ReadUsePartsSize(data[(int)WeaponVariable.UseUnderBarrel]),
                    equipMagID = data[(int)WeaponVariable.EquipMagID],
                    equipPartsIDs = ReadEquipPartsID(data[(int)WeaponVariable.EquipPartsIDs])
                };
                SetInternalMagazine(ref weaponInfo);
                weaponData.weaponInfos.Add(weaponInfo);
            }
            Debug.Log("Update Weapon Data");

            List<WeaponPartsSize> ReadUseMagazineSize(string sizeData)
            {
                var partsSizeList = new List<WeaponPartsSize>();
                var sizeInfos = sizeData.Split(',');
                for (int i = 0; i < sizeInfos.Length; i++)
                {
                    var sizeInfo = sizeInfos[i];
                    if (sizeInfo == "None") return null;

                    WeaponPartsSize partsSize;
                    switch (sizeInfo)
                    {
                        case "S":
                            partsSize = WeaponPartsSize.Small;
                            break;
                        case "M":
                            partsSize = WeaponPartsSize.Medium;
                            break;
                        case "L":
                            partsSize = WeaponPartsSize.Large;
                            break;
                        default:
                            intMagMax = int.Parse(sizeInfo);
                            return null;
                    }
                    partsSizeList.Add(partsSize);
                }

                return partsSizeList;
            }

            List<WeaponPartsSize> ReadUsePartsSize(string sizeData)
            {
                var partsSizeList = new List<WeaponPartsSize>();
                var sizeInfos = sizeData.Split(',');
                for (int i = 0; i < sizeInfos.Length; i++)
                {
                    var sizeInfo = sizeInfos[i];
                    var partsSize = (WeaponPartsSize)int.Parse(sizeInfo);
                    if (partsSize == WeaponPartsSize.None) break;

                    partsSizeList.Add(partsSize);
                }

                return partsSizeList;
            }

            List<string> ReadEquipPartsID(string partsDatas)
            {
                var partsIDs = new List<string>();
                var partsInfos = partsDatas.Split(',', '\r');
                if (partsInfos[0] == "None") return null;

                for (int i = 0; i < partsInfos.Length; i++)
                {
                    var partsID = partsInfos[i];
                    partsIDs.Add(partsID);
                }

                return partsIDs;
            }

            void SetInternalMagazine(ref WeaponDataInfo weaponInfo)
            {
                if (intMagMax == 0) return;

                var intMag = new MagazineDataInfo()
                {
                    indexName = "InternalMagazine",
                    ID = "None",
                    intMag = true,
                    loadedBulletID = "None",
                    prefabName = "None",
                    magName = "InternalMagazine",
                    compatModel = new List<int> { weaponInfo.model },
                    compatCaliber = weaponInfo.caliber,
                    weight = 0f,
                    magSize = intMagMax,
                };
                weaponInfo.equipMag = intMag;
                weaponInfo.isMag = true;
            }
        }
    }
    #endregion

    #region WeaponParts Data
    [HideInInspector] public WeaponPartsData partsData;
    private readonly string partsDB = "https://docs.google.com/spreadsheets/d/1K4JDpojMJeJPpvA-u_sOK591Y16PBG45T77HCHyn_9w/export?format=tsv&gid=1233203314&range=A2:P";
    private enum WeaponPartsVariable
    {
        ID,
        PrefabName,
        PartsName,
        CompatModel,
        Weight,
        PartsType,
        Size,
        RPM,
        Range,
        MOA,
        Stability,
        Rebound,
        WatchAngle,
        Ergonomy,
        HeadShot,
        ActionCost,
    }

    public void UpdateWeaponPartsData()
    {
        if (partsData == null) partsData = Resources.Load<WeaponPartsData>("ScriptableObjects/WeaponPartsData");
        if (partsData.partsInfos.Count > 0) partsData.partsInfos.Clear();

        StartCoroutine(ReadWeaponPartsData());

        IEnumerator ReadWeaponPartsData()
        {
            UnityWebRequest www = UnityWebRequest.Get(partsDB);
            yield return www.SendWebRequest();

            var text = www.downloadHandler.text;
            var datas = text.Split('\n');
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i].Split('\t');
                var partsInfo = new WeaponPartsDataInfo
                {
                    indexName = $"{data[(int)WeaponPartsVariable.ID]}: {data[(int)WeaponPartsVariable.PartsName]}",
                    ID = data[(int)WeaponPartsVariable.ID],
                    prefabName = data[(int)WeaponPartsVariable.PrefabName],
                    partsName = data[(int)WeaponPartsVariable.PartsName],
                    compatModel = ReadCompatModelInfo(data[(int)WeaponPartsVariable.CompatModel]),
                    weight = float.Parse(data[(int)WeaponPartsVariable.Weight]),
                    type = (WeaponPartsType)int.Parse(data[(int)WeaponPartsVariable.PartsType]),
                    size = (WeaponPartsSize)int.Parse(data[(int)WeaponPartsVariable.Size]),
                    RPM = int.Parse(data[(int)WeaponPartsVariable.RPM]),
                    range = float.Parse(data[(int)WeaponPartsVariable.Range]),
                    MOA = float.Parse(data[(int)WeaponPartsVariable.MOA]),
                    stability = int.Parse(data[(int)WeaponPartsVariable.Stability]),
                    rebound = int.Parse(data[(int)WeaponPartsVariable.Rebound]),
                    watchAngle = int.Parse(data[(int)WeaponPartsVariable.WatchAngle]),
                    ergonomy = int.Parse(data[(int)WeaponPartsVariable.Ergonomy]),
                    headShot = float.Parse(data[(int)WeaponPartsVariable.HeadShot]),
                    actionCost = int.Parse(data[(int)WeaponPartsVariable.ActionCost]),
                };
                partsData.partsInfos.Add(partsInfo);
            }
            Debug.Log("Update WeaponParts Data");
        }
    }
    #endregion

    #region Magazine Data
    [HideInInspector] public MagazineData magData;
    private readonly string magDB = "https://docs.google.com/spreadsheets/d/1K4JDpojMJeJPpvA-u_sOK591Y16PBG45T77HCHyn_9w/export?format=tsv&gid=660227428&range=A2:H";
    private enum MagazineVariable
    {
        ID,
        LoadedBulletID,
        PrefabName,
        MagazineName,
        CompatModel,
        CompatCaliber,
        Weight,
        MagazineSize,
    }

    public void UpdateMagazineData()
    {
        if (magData == null) magData = Resources.Load<MagazineData>("ScriptableObjects/MagazineData");
        if (magData.magInfos.Count > 0) magData.magInfos.Clear();

        StartCoroutine(ReadMagazineData());

        IEnumerator ReadMagazineData()
        {
            UnityWebRequest www = UnityWebRequest.Get(magDB);
            yield return www.SendWebRequest();

            var text = www.downloadHandler.text;
            var datas = text.Split('\n');
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i].Split('\t');
                var magInfo = new MagazineDataInfo()
                {
                    indexName = $"{data[(int)MagazineVariable.ID]}: {data[(int)MagazineVariable.MagazineName]}",
                    ID = data[(int)MagazineVariable.ID],
                    intMag = false,
                    loadedBulletID = data[(int)MagazineVariable.LoadedBulletID] == "None" ? null : data[(int)MagazineVariable.LoadedBulletID],
                    prefabName = data[(int)MagazineVariable.PrefabName],
                    magName = data[(int)MagazineVariable.MagazineName],
                    compatModel = ReadCompatModelInfo(data[(int)MagazineVariable.CompatModel]),
                    compatCaliber = float.Parse(data[(int)MagazineVariable.CompatCaliber]),
                    weight = float.Parse(data[(int)MagazineVariable.Weight]),
                    magSize = int.Parse(data[(int)MagazineVariable.MagazineSize]),
                };
                magData.magInfos.Add(magInfo);
            }
            Debug.Log("Update Magazine Data");
        }
    }
    #endregion

    #region Bullet Data
    [HideInInspector] public BulletData bulletData;
    private readonly string bulletDB = "https://docs.google.com/spreadsheets/d/1K4JDpojMJeJPpvA-u_sOK591Y16PBG45T77HCHyn_9w/export?format=tsv&gid=515744337&range=A2:M";
    private enum BulletVariable
    {
        ID,
        BulletName,
        MeshType,
        Level,
        Caliber,
        PelletNum,
        Weight,
        Propellant,
        Damage,
        Penetrate,
        ArmorBreak,
        Critical,
    }

    public void UpdateBulletData()
    {
        if (bulletData == null)
        {
            bulletData = Resources.Load<BulletData>("ScriptableObjects/BulletData");
        }

        if (bulletData.bulletInfos.Count > 0)
        {
            bulletData.bulletInfos.Clear();
        }
        StartCoroutine(ReadBulletData());

        IEnumerator ReadBulletData()
        {
            UnityWebRequest www = UnityWebRequest.Get(bulletDB);
            yield return www.SendWebRequest();

            var text = www.downloadHandler.text;
            var datas = text.Split('\n');
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i].Split('\t');
                var bulletInfo = new BulletDataInfo()
                {
                    indexName = $"{data[(int)BulletVariable.ID]}: {data[(int)BulletVariable.BulletName]}",
                    ID = data[(int)BulletVariable.ID],
                    bulletName = data[(int)BulletVariable.BulletName],
                    meshType = int.Parse(data[(int)BulletVariable.MeshType]),
                    level = int.Parse(data[(int)BulletVariable.Level]),
                    caliber = float.Parse(data[(int)BulletVariable.Caliber]),
                    pelletNum = int.Parse(data[(int)BulletVariable.PelletNum]),
                    weight = float.Parse(data[(int)BulletVariable.Weight]),
                    propellant = int.Parse(data[(int)BulletVariable.Propellant]),
                    damage = int.Parse(data[(int)BulletVariable.Damage]),
                    penetrate = int.Parse(data[(int)BulletVariable.Penetrate]),
                    armorBreak = int.Parse(data[(int)BulletVariable.ArmorBreak]),
                    critical = int.Parse(data[(int)BulletVariable.Critical]),
                };
                bulletData.bulletInfos.Add(bulletInfo);
            }
            Debug.Log("Update Bullet Data");
        }
    }
    #endregion

    #region Grenade Data
    [HideInInspector] public GrenadeData grenadeData;
    private readonly string grenadeDB = "https://docs.google.com/spreadsheets/d/1K4JDpojMJeJPpvA-u_sOK591Y16PBG45T77HCHyn_9w/export?format=tsv&gid=1078537712&range=A3:G";
    private enum GrenadeVariable
    {
        ID,
        GrenadeName,
        FX_Name,
        ThrowRange,
        BlastRange,
        Weight,
        Damage,
    }

    public void UpdateGrenadeData()
    {
        if (grenadeData == null)
        {
            grenadeData = Resources.Load<GrenadeData>("ScriptableObjects/GrenadeData");
        }

        if (grenadeData.grenadeInfos.Count > 0)
        {
            grenadeData.grenadeInfos.Clear();
        }
        StartCoroutine(ReadGrenadeData());

        IEnumerator ReadGrenadeData()
        {
            UnityWebRequest www = UnityWebRequest.Get(grenadeDB);
            yield return www.SendWebRequest();

            var text = www.downloadHandler.text;
            var datas = text.Split('\n');
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i].Split('\t');
                var bulletInfo = new GrenadeDataInfo()
                {
                    indexName = $"{data[(int)GrenadeVariable.ID]}: {data[(int)GrenadeVariable.GrenadeName]}",
                    ID = data[(int)GrenadeVariable.ID],
                    grenadeName = data[(int)GrenadeVariable.GrenadeName],
                    FX_name = data[(int)GrenadeVariable.FX_Name],
                    throwRange = float.Parse(data[(int)GrenadeVariable.ThrowRange]),
                    blastRange = float.Parse(data[(int)GrenadeVariable.BlastRange]),
                    weight = float.Parse(data[(int)GrenadeVariable.Weight]),
                    damage = int.Parse(data[(int)GrenadeVariable.Damage]),
                };
                grenadeData.grenadeInfos.Add(bulletInfo);
            }
            Debug.Log("Update Grenade Data");
        }
    }

    #endregion

    #region Armor Data
    [HideInInspector] public ArmorData armorData;
    private readonly string armorDB = "https://docs.google.com/spreadsheets/d/1K4JDpojMJeJPpvA-u_sOK591Y16PBG45T77HCHyn_9w/export?format=tsv&gid=1373614489&range=A2:D";
    private enum ArmorVariable
    {
        ID,
        ArmorName,
        MaxBulletproof,
        MaxDurability,
    }

    public void UpdateArmorData()
    {
        if (armorData == null) armorData = Resources.Load<ArmorData>("ScriptableObjects/ArmorData");
        if (armorData.armorInfos.Count > 0) armorData.armorInfos.Clear();

        StartCoroutine(ReadArmorData());

        IEnumerator ReadArmorData()
        {
            UnityWebRequest www = UnityWebRequest.Get(armorDB);
            yield return www.SendWebRequest();

            var text = www.downloadHandler.text;
            var datas = text.Split('\n');
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i].Split('\t');
                var armorInfo = new ArmorDataInfo
                {
                    indexName = $"{data[(int)ArmorVariable.ID]}: {data[(int)ArmorVariable.ArmorName]}",
                    ID = data[(int)ArmorVariable.ID],
                    armorName = data[(int)ArmorVariable.ArmorName],
                    maxBulletproof = float.Parse(data[(int)ArmorVariable.MaxBulletproof]),
                    maxDurability = int.Parse(data[(int)ArmorVariable.MaxDurability]),
                };
                armorData.armorInfos.Add(armorInfo);
            }
            Debug.Log("Update Armor Data");
        }
    }
    #endregion

    #region Rig Data
    [HideInInspector] public RigData rigData;
    private readonly string rigDB = "https://docs.google.com/spreadsheets/d/1K4JDpojMJeJPpvA-u_sOK591Y16PBG45T77HCHyn_9w/export?format=tsv&gid=3068856&range=A3:D";
    private enum RigVariable
    {
        ID,
        RigName,
        X_Size,
        Y_Size,
    }

    public void UpdateRigData()
    {
        if (rigData == null) rigData = Resources.Load<RigData>("ScriptableObjects/RigData");
        if (rigData.rigInfos.Count > 0) rigData.rigInfos.Clear();

        StartCoroutine(ReadRigData());

        IEnumerator ReadRigData()
        {
            UnityWebRequest www = UnityWebRequest.Get(rigDB);
            yield return www.SendWebRequest();

            var text = www.downloadHandler.text;
            var datas = text.Split('\n');
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i].Split('\t');
                var rigInfo = new RigDataInfo
                {
                    indexName = $"{data[(int)RigVariable.ID]}: {data[(int)RigVariable.RigName]}",
                    ID = data[(int)RigVariable.ID],
                    rigName = data[(int)RigVariable.RigName],
                    storageSize = new Vector2Int(int.Parse(data[(int)RigVariable.X_Size]), int.Parse(data[(int)RigVariable.Y_Size])),
                };
                rigData.rigInfos.Add(rigInfo);
            }
            Debug.Log("Update Rig Data");
        }
    }
    #endregion

    #region Backpack Data
    [HideInInspector] public BackpackData backpackData;
    private readonly string backpackDB = "https://docs.google.com/spreadsheets/d/1K4JDpojMJeJPpvA-u_sOK591Y16PBG45T77HCHyn_9w/export?format=tsv&gid=954121519&range=A3:D";
    private enum BackpackVariable
    {
        ID,
        BackpackName,
        X_Size,
        Y_Size,
    }

    public void UpdateBackpackData()
    {
        if (backpackData == null) backpackData = Resources.Load<BackpackData>("ScriptableObjects/BackpackData");
        if (backpackData.backpackInfos.Count > 0) backpackData.backpackInfos.Clear();

        StartCoroutine(ReadBackpackData());

        IEnumerator ReadBackpackData()
        {
            UnityWebRequest www = UnityWebRequest.Get(backpackDB);
            yield return www.SendWebRequest();

            var text = www.downloadHandler.text;
            var datas = text.Split('\n');
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i].Split('\t');
                var backpackInfo = new BackpackDataInfo
                {
                    indexName = $"{data[(int)BackpackVariable.ID]}: {data[(int)BackpackVariable.BackpackName]}",
                    ID = data[(int)BackpackVariable.ID],
                    backpackName = data[(int)BackpackVariable.BackpackName],
                    storageSize = new Vector2Int(int.Parse(data[(int)BackpackVariable.X_Size]), int.Parse(data[(int)BackpackVariable.Y_Size])),
                };
                backpackData.backpackInfos.Add(backpackInfo);
            }
            Debug.Log("Update Backpack Data");
        }
    }
    #endregion

    #region ItemOption Data
    [HideInInspector] public ItemOptionData itemOptionData;
    private readonly string itemOptionDB = "https://docs.google.com/spreadsheets/d/1K4JDpojMJeJPpvA-u_sOK591Y16PBG45T77HCHyn_9w/export?format=tsv&gid=1636963263&range=A3:H";
    private enum ItemOptionDataVariable
    {
        ID,
        Rank,
        MainType,
        SubType,
        OptionType,
        MinValue,
        MaxValue,
        ScriptText,
    }

    public void UpdateItemOptionData()
    {
        if (itemOptionData == null) itemOptionData = Resources.Load<ItemOptionData>("ScriptableObjects/ItemOptionData");
        if (itemOptionData.itemOptionInfos.Count > 0) itemOptionData.itemOptionInfos.Clear();

        StartCoroutine(ReadItemOptionData());

        IEnumerator ReadItemOptionData()
        {
            UnityWebRequest www = UnityWebRequest.Get(itemOptionDB);
            yield return www.SendWebRequest();

            var text = www.downloadHandler.text;
            var datas = text.Split('\n');
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i].Split('\t');
                var itemOptionInfo = new ItemOptionDataInfo
                {
                    indexName = $"{data[(int)ItemOptionDataVariable.ID]}: Rank{int.Parse(data[(int)ItemOptionDataVariable.Rank])}",
                    rank = int.Parse(data[(int)ItemOptionDataVariable.Rank]),
                    mainType = int.Parse(data[(int)ItemOptionDataVariable.MainType]),
                    subType = int.Parse(data[(int)ItemOptionDataVariable.SubType]),
                    optionType = (ItemOptionType)int.Parse(data[(int)ItemOptionDataVariable.OptionType]),
                    minValue = int.Parse(data[(int)ItemOptionDataVariable.MinValue]),
                    maxValue = int.Parse(data[(int)ItemOptionDataVariable.MaxValue]),
                    scriptText = data[(int)ItemOptionDataVariable.ScriptText],
                };
                itemOptionData.itemOptionInfos.Add(itemOptionInfo);
            }
            Debug.Log("Update ItemOption Data");
        }
    }
    #endregion

    #region OptionSheet Data
    [HideInInspector] public OptionSheetData optionSheetData;
    private readonly string optionSheetDB = "https://docs.google.com/spreadsheets/d/1K4JDpojMJeJPpvA-u_sOK591Y16PBG45T77HCHyn_9w/export?format=tsv&gid=1423114483&range=A4:R";
    private enum OptionSheetVariable
    {
        MinLevel,
        MaxLevel,
        rank1_option1,
        rank1_option2,
        rank1_option3,
        rank1_option4,
        rank2_option1,
        rank2_option2,
        rank2_option3,
        rank2_option4,
        rank3_option1,
        rank3_option2,
        rank3_option3,
        rank3_option4,
        rank4_option1,
        rank4_option2,
        rank4_option3,
        rank4_option4,
    }

    public void UpdateOptionSheetData()
    {
        if (optionSheetData == null) optionSheetData = Resources.Load<OptionSheetData>("ScriptableObjects/OptionSheetData");
        if (optionSheetData.optionSheetInfos.Count > 0) optionSheetData.optionSheetInfos.Clear();

        var rankMax = 4;
        StartCoroutine(ReadOptionSheetData());

        IEnumerator ReadOptionSheetData()
        {
            UnityWebRequest www = UnityWebRequest.Get(optionSheetDB);
            yield return www.SendWebRequest();

            var text = www.downloadHandler.text;
            var datas = text.Split('\n');
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i].Split('\t');
                var optionSheetInfo = new OptionSheetDataInfo
                {
                    indexName = $"{data[(int)OptionSheetVariable.MinLevel]} ~ {data[(int)OptionSheetVariable.MaxLevel]}",
                    levelInfo = ReadLevelInfo(data[(int)OptionSheetVariable.MinLevel], data[(int)OptionSheetVariable.MaxLevel]),
                };
                for (int j = 0; j < rankMax; j++)
                {
                    optionSheetInfo.rankOptions.Add(ReadRankOptionInfo(j, data[(j * rankMax) + (int)OptionSheetVariable.rank1_option1],
                                                                          data[(j * rankMax) + (int)OptionSheetVariable.rank1_option2],
                                                                          data[(j * rankMax) + (int)OptionSheetVariable.rank1_option3],
                                                                          data[(j * rankMax) + (int)OptionSheetVariable.rank1_option4]));
                }
                optionSheetData.optionSheetInfos.Add(optionSheetInfo);
            }
            Debug.Log("Update OptionSheet Data");
        }

        ItemLevelInfo ReadLevelInfo(string minLevel, string maxLevel)
        {
            var levelInfo = new ItemLevelInfo
            {
                minLevel = int.Parse(minLevel),
                maxLevel = int.Parse(maxLevel),
            };

            return levelInfo;
        }

        ItemRankOptionInfo ReadRankOptionInfo(int rank, string option1_rank, string option2_rank, string option3_rank, string option4_rank)
        {
            var rankOptionInfo = new ItemRankOptionInfo
            {
                indexName = $"Rank{rank + 1}",
                option1_rank = int.Parse(option1_rank),
                option2_rank = int.Parse(option2_rank),
                option3_rank = int.Parse(option3_rank),
                option4_rank = int.Parse(option4_rank),
            };

            return rankOptionInfo;
        }
    }
    #endregion

    #region DropTable Data
    [HideInInspector] public DropTableData dropTableData;
    private readonly string dropTableDB = "https://docs.google.com/spreadsheets/d/1K4JDpojMJeJPpvA-u_sOK591Y16PBG45T77HCHyn_9w/export?format=tsv&gid=1891763678&range=A4:AE";
    private enum DropTableVariable
    {
        ID,
        StageLevel,
        MinItemLevel,
        MaxItemLevel,
        Equipment_itemMinNum,
        Equipment_itemMaxNum,
        Equipment_dropPercentage_lowGrade,
        Equipment_dropPercentage_nomal,
        Equipment_dropPercentage_middleGrade,
        Equipment_dropPercentage_highGrade,
        Equipment_dropPercentage_advanced,
        Equipment_dropPercentage_set,
        Expendable_itemMinNum,
        Expendable_itemMaxNum,
        Expendable_dropPercentage_lowGrade,
        Expendable_dropPercentage_nomal,
        Expendable_dropPercentage_middleGrade,
        Expendable_dropPercentage_highGrade,
        Expendable_dropPercentage_advanced,
        Expendable_dropPercentage_set,
        Ingredient_itemMinNum,
        Ingredient_itemMaxNum,
        Ingredient_dropPercentage_lowGrade,
        Ingredient_dropPercentage_nomal,
        Ingredient_dropPercentage_middleGrade,
        Ingredient_dropPercentage_highGrade,
        Ingredient_dropPercentage_advanced,
        Ingredient_dropPercentage_set,
        UniqueTable_itemMinNum,
        UniqueTable_itemMaxNum,
        UniqueTable_dropPercentage,
    }

    public void UpdateDropTableData()
    {
        if (dropTableData == null) dropTableData = Resources.Load<DropTableData>("ScriptableObjects/DropTableData");
        if (dropTableData.dropTableInfo.Count > 0) dropTableData.dropTableInfo.Clear();

        StartCoroutine(ReadDropTableData());

        IEnumerator ReadDropTableData()
        {
            UnityWebRequest www = UnityWebRequest.Get(dropTableDB);
            yield return www.SendWebRequest();

            var text = www.downloadHandler.text;
            var datas = text.Split('\n');
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i].Split('\t');
                var dropTableInfo = new DropTableDataInfo()
                {
                    ID = data[(int)DropTableVariable.ID],
                    stageLevel = (StageLevel)int.Parse(data[(int)DropTableVariable.StageLevel]),
                    minItemLevel = int.Parse(data[(int)DropTableVariable.MinItemLevel]),
                    maxItemLevel = int.Parse(data[(int)DropTableVariable.MaxItemLevel]),
                    dropInfo_equipment = ReadDropTableInfo("Table_Equipment",
                                                           data[(int)DropTableVariable.Equipment_itemMinNum],
                                                           data[(int)DropTableVariable.Equipment_itemMaxNum],
                                                           data[(int)DropTableVariable.Equipment_dropPercentage_lowGrade],
                                                           data[(int)DropTableVariable.Equipment_dropPercentage_nomal],
                                                           data[(int)DropTableVariable.Equipment_dropPercentage_middleGrade],
                                                           data[(int)DropTableVariable.Equipment_dropPercentage_highGrade],
                                                           data[(int)DropTableVariable.Equipment_dropPercentage_advanced],
                                                           data[(int)DropTableVariable.Equipment_dropPercentage_set]),
                    dropInfo_expendable = ReadDropTableInfo("Table_Expendable",
                                                            data[(int)DropTableVariable.Expendable_itemMinNum],
                                                            data[(int)DropTableVariable.Expendable_itemMaxNum],
                                                            data[(int)DropTableVariable.Expendable_dropPercentage_lowGrade],
                                                            data[(int)DropTableVariable.Expendable_dropPercentage_nomal],
                                                            data[(int)DropTableVariable.Expendable_dropPercentage_middleGrade],
                                                            data[(int)DropTableVariable.Expendable_dropPercentage_highGrade],
                                                            data[(int)DropTableVariable.Expendable_dropPercentage_advanced],
                                                            data[(int)DropTableVariable.Expendable_dropPercentage_set]),
                    dropInfo_ingredient = ReadDropTableInfo("Table_Ingredient",
                                                            data[(int)DropTableVariable.Ingredient_itemMinNum],
                                                            data[(int)DropTableVariable.Ingredient_itemMaxNum],
                                                            data[(int)DropTableVariable.Ingredient_dropPercentage_lowGrade],
                                                            data[(int)DropTableVariable.Ingredient_dropPercentage_nomal],
                                                            data[(int)DropTableVariable.Ingredient_dropPercentage_middleGrade],
                                                            data[(int)DropTableVariable.Ingredient_dropPercentage_highGrade],
                                                            data[(int)DropTableVariable.Ingredient_dropPercentage_advanced],
                                                            data[(int)DropTableVariable.Ingredient_dropPercentage_set]),
                    uniqueTable = ReadUniqueTableInfo(data[(int)DropTableVariable.UniqueTable_itemMinNum],
                                                      data[(int)DropTableVariable.UniqueTable_itemMaxNum],
                                                      data[(int)DropTableVariable.UniqueTable_dropPercentage])
                };
                dropTableData.dropTableInfo.Add(dropTableInfo);
            }
            Debug.Log("Update DropTable Data");
        }

        DropTable ReadDropTableInfo(string tableName, string itemMinNum, string itemMaxNum, string dropPercentage_lowGrade, string dropPercentage_nomal, string dropPercentage_middleGrade, string dropPercentage_highGrade, string dropPercentage_advanced, string dropPercentage_set)
        {
            var dropTableInfo = new DropTable()
            {
                indexName = $"{tableName}",
                itemMinNum = int.Parse(itemMinNum),
                itemMaxNum = int.Parse(itemMaxNum),
                dropPercentage_lowGrade = int.Parse(dropPercentage_lowGrade),
                dropPercentage_nomal = int.Parse(dropPercentage_nomal),
                dropPercentage_middleGrade = int.Parse(dropPercentage_middleGrade),
                dropPercentage_highGrade = int.Parse(dropPercentage_highGrade),
                dropPercentage_advanced = int.Parse(dropPercentage_advanced),
                dropPercentage_set = int.Parse(dropPercentage_set),
            };

            return dropTableInfo;
        }

        UniqueTable ReadUniqueTableInfo(string itemMinNum, string itemMaxNum, string dropPercentage)
        {
            var uniqueTable = new UniqueTable()
            {
                itemMinNum = int.Parse(itemMinNum),
                itemMaxNum = int.Parse(itemMaxNum),
                dropPercentage = int.Parse(dropPercentage),
            };

            return uniqueTable;
        }
    }
    #endregion

    private List<int> ReadCompatModelInfo(string modelData)
    {
        var compatModels = new List<int>();
        var modelInfos = modelData.Split(',');
        for (int i = 0; i < modelInfos.Length; i++)
        {
            var modelInfo = modelInfos[i];
            var compatModel = int.Parse(modelInfo);
            compatModels.Add(compatModel);
        }

        return compatModels;
    }

    #region Custom Editor
    [CustomEditor(typeof(DataManager))]
    public class DataEditor : Editor
    {
        private DataManager dataMgr;

        private void OnEnable()
        {
            dataMgr = (DataManager)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Label('\n' + "---Read GoogleSheet Data---");
            if (GUILayout.Button("Update the Player Database"))
            {
                dataMgr.UpdatePlayerData();
                EditorUtility.SetDirty(dataMgr.playerData);
            }
            if (GUILayout.Button("Update the Enemy Database"))
            {
                dataMgr.UpdateEnemyData();
                EditorUtility.SetDirty(dataMgr.enemyData);
            }
            if (GUILayout.Button("Update the AI Database"))
            {
                dataMgr.UpdateAIData();
                EditorUtility.SetDirty(dataMgr.aiData);
            }
            if (GUILayout.Button("Update the Stage Database"))
            {
                dataMgr.UpdateStageData();
                EditorUtility.SetDirty(dataMgr.stageData);
            }
            if (GUILayout.Button("Update the Item Database"))
            {
                dataMgr.UpdateItemData();
                EditorUtility.SetDirty(dataMgr.itemData);
            }
            if (GUILayout.Button("Update the Weapon Database"))
            {
                dataMgr.UpdateWeaponData();
                EditorUtility.SetDirty(dataMgr.weaponData);
            }
            if (GUILayout.Button("Update the WeaponParts Database"))
            {
                dataMgr.UpdateWeaponPartsData();
                EditorUtility.SetDirty(dataMgr.partsData);
            }
            if (GUILayout.Button("Update the Magazine Database"))
            {
                dataMgr.UpdateMagazineData();
                EditorUtility.SetDirty(dataMgr.magData);
            }
            if (GUILayout.Button("Update the Bullet Database"))
            {
                dataMgr.UpdateBulletData();
                EditorUtility.SetDirty(dataMgr.bulletData);
            }
            if (GUILayout.Button("Update the Grenade Database"))
            {
                dataMgr.UpdateGrenadeData();
                EditorUtility.SetDirty(dataMgr.grenadeData);
            }
            if (GUILayout.Button("Update the Armor Database"))
            {
                dataMgr.UpdateArmorData();
                EditorUtility.SetDirty(dataMgr.armorData);
            }
            if (GUILayout.Button("Update the Rig Database"))
            {
                dataMgr.UpdateRigData();
                EditorUtility.SetDirty(dataMgr.rigData);
            }
            if (GUILayout.Button("Update the Backpack Database"))
            {
                dataMgr.UpdateBackpackData();
                EditorUtility.SetDirty(dataMgr.backpackData);
            }
            if (GUILayout.Button("Update the ItemOption Database"))
            {
                dataMgr.UpdateItemOptionData();
                EditorUtility.SetDirty(dataMgr.itemOptionData);
            }
            if (GUILayout.Button("Update the OptionSheet Database"))
            {
                dataMgr.UpdateOptionSheetData();
                EditorUtility.SetDirty(dataMgr.optionSheetData);
            }
            if (GUILayout.Button("Update the DropTable Database"))
            {
                dataMgr.UpdateDropTableData();
                EditorUtility.SetDirty(dataMgr.dropTableData);
            }
            GUILayout.Label(" ");
            if (GUILayout.Button("Update All Database"))
            {
                dataMgr.UpdatePlayerData();
                EditorUtility.SetDirty(dataMgr.playerData);
                dataMgr.UpdateEnemyData();
                EditorUtility.SetDirty(dataMgr.enemyData);
                dataMgr.UpdateAIData();
                EditorUtility.SetDirty(dataMgr.aiData);
                dataMgr.UpdateStageData();
                EditorUtility.SetDirty(dataMgr.stageData);
                dataMgr.UpdateItemData();
                EditorUtility.SetDirty(dataMgr.itemData);
                dataMgr.UpdateWeaponData();
                EditorUtility.SetDirty(dataMgr.weaponData);
                dataMgr.UpdateWeaponPartsData();
                EditorUtility.SetDirty(dataMgr.partsData);
                dataMgr.UpdateMagazineData();
                EditorUtility.SetDirty(dataMgr.magData);
                dataMgr.UpdateBulletData();
                EditorUtility.SetDirty(dataMgr.bulletData);
                dataMgr.UpdateGrenadeData();
                EditorUtility.SetDirty(dataMgr.grenadeData);
                dataMgr.UpdateArmorData();
                EditorUtility.SetDirty(dataMgr.armorData);
                dataMgr.UpdateItemOptionData();
                EditorUtility.SetDirty(dataMgr.itemOptionData);
                dataMgr.UpdateOptionSheetData();
                EditorUtility.SetDirty(dataMgr.optionSheetData);
                dataMgr.UpdateDropTableData();
                EditorUtility.SetDirty(dataMgr.dropTableData);
            }
        }
    }
    #endregion
}
