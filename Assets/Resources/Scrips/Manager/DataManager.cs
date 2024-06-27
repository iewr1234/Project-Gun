using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using TMPro;
using System;
using UnityEngine.Tilemaps;

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

    [Header("[Marker]")]
    public bool isMarker;
    public CharacterOwner markerType;
    public int markerIndex;

    [Header("[Object]")]
    public bool isObject;
    public ObjectData[] objectDatas;
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
        var find = FindObjectsOfType<DataManager>();
        if (find.Length == 1)
        {
            DontDestroyOnLoad(gameObject);
            SetComponents();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetComponents()
    {
        if (charData == null)
            charData = Resources.Load<CharacterData>("ScriptableObjects/CharacterData");

        if (weaponData == null)
            weaponData = Resources.Load<WeaponData>("ScriptableObjects/WeaponData");

        if (armorData == null)
            armorData = Resources.Load<ArmorData>("ScriptableObjects/ArmorData");

        if (itemData == null)
            itemData = Resources.Load<ItemData>("ScriptableObjects/ItemData");
    }

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

            // Marker Data
            nodeData.isMarker = node.Marker.activeSelf;
            if (nodeData.isMarker)
            {
                nodeData.markerType = node.MarkerOutline.color == DataUtility.color_Player ? CharacterOwner.Player : CharacterOwner.Enemy;
                var indexText = node.MarkerText.text;
                var match = Regex.Match(indexText, @"\d+");
                if (match.Success)
                {
                    if (int.TryParse(match.Value, out int index))
                    {
                        nodeData.markerIndex = index;
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

    #region Character Data
    [HideInInspector] public CharacterData charData;
    private readonly string charDB = "https://docs.google.com/spreadsheets/d/1K4JDpojMJeJPpvA-u_sOK591Y16PBG45T77HCHyn_9w/export?format=tsv&gid=676425891&range=A3:S";
    private enum CharacterVariable
    {
        ID,
        PrefabName,
        CharacterName,
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
        MainWeapon1_ID,
        MainWeapon2_ID,
        SubWeapon_ID,
        ArmorID,
    }

    public void UpdateCharacterData()
    {
        if (charData == null)
        {
            charData = Resources.Load<CharacterData>("ScriptableObjects/CharacterData");
        }

        if (charData.charInfos.Count > 0)
        {
            charData.charInfos.Clear();
        }
        StartCoroutine(ReadCharacterData());

        IEnumerator ReadCharacterData()
        {
            UnityWebRequest www = UnityWebRequest.Get(charDB);
            yield return www.SendWebRequest();

            var text = www.downloadHandler.text;
            var datas = text.Split('\n');
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i].Split('\t');
                var playerInfo = new CharacterDataInfo
                {
                    indexName = $"{data[(int)CharacterVariable.ID]}: {data[(int)CharacterVariable.CharacterName]}",
                    ID = data[(int)CharacterVariable.ID],
                    prefabName = data[(int)CharacterVariable.PrefabName],
                    charName = data[(int)CharacterVariable.CharacterName],
                    strength = int.Parse(data[(int)CharacterVariable.Strength]),
                    vitality = int.Parse(data[(int)CharacterVariable.Vitality]),
                    intellect = int.Parse(data[(int)CharacterVariable.Intellect]),
                    wisdom = int.Parse(data[(int)CharacterVariable.Wisdom]),
                    agility = int.Parse(data[(int)CharacterVariable.Agility]),
                    dexterity = int.Parse(data[(int)CharacterVariable.Dexterity]),
                    maxAction = int.Parse(data[(int)CharacterVariable.MaxAction]),
                    maxHealth = int.Parse(data[(int)CharacterVariable.MaxHealth]),
                    maxStamina = int.Parse(data[(int)CharacterVariable.MaxStamina]),
                    sight = float.Parse(data[(int)CharacterVariable.Sight]),
                    aiming = int.Parse(data[(int)CharacterVariable.Aiming]),
                    reaction = int.Parse(data[(int)CharacterVariable.Reaction]),
                    mainWeapon1_ID = data[(int)CharacterVariable.MainWeapon1_ID],
                    mainWeapon2_ID = data[(int)CharacterVariable.MainWeapon2_ID],
                    subWeapon_ID = data[(int)CharacterVariable.SubWeapon_ID],
                    armorID = data[(int)CharacterVariable.ArmorID].Replace("\r", ""),
                };
                charData.charInfos.Add(playerInfo);
            }
            Debug.Log("Update Character Data");
        }
    }
    #endregion

    #region Item Data
    [HideInInspector] public ItemData itemData;
    private readonly string itemDB = "https://docs.google.com/spreadsheets/d/1K4JDpojMJeJPpvA-u_sOK591Y16PBG45T77HCHyn_9w/export?format=tsv&gid=267991501&range=A3:I";
    private enum ItemVariable
    {
        ID,
        DataID,
        ItemName,
        Type,
        Rarity,
        MaxNesting,
        Price,
        X_Size,
        Y_Size,
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
                    rarity = (ItemRarity)int.Parse(data[(int)ItemVariable.Rarity]),
                    maxNesting = int.Parse(data[(int)ItemVariable.MaxNesting]),
                    price = int.Parse(data[(int)ItemVariable.Price]),
                    size = new Vector2Int(int.Parse(data[(int)ItemVariable.X_Size]), int.Parse(data[(int)ItemVariable.Y_Size])),
                };
                itemData.itemInfos.Add(itemInfo);
            }
            Debug.Log("Update Item Data");
        }
    }
    #endregion

    #region Weapon Data
    [HideInInspector] public WeaponData weaponData;
    private readonly string weaponDB = "https://docs.google.com/spreadsheets/d/1K4JDpojMJeJPpvA-u_sOK591Y16PBG45T77HCHyn_9w/export?format=tsv&gid=719783222&range=A2:S";
    private enum WeaponVariable
    {
        ID,
        PrefabName,
        WeaponName,
        Model,
        Caliber,
        Weight,
        WeaponType,
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
    }

    public void UpdateWeaponData()
    {
        if (weaponData == null)
        {
            weaponData = Resources.Load<WeaponData>("ScriptableObjects/WeaponData");
        }

        if (weaponData.weaponInfos.Count > 0)
        {
            weaponData.weaponInfos.Clear();
        }
        StartCoroutine(ReadWeaponData());

        IEnumerator ReadWeaponData()
        {
            UnityWebRequest www = UnityWebRequest.Get(weaponDB);
            yield return www.SendWebRequest();

            var text = www.downloadHandler.text;
            var datas = text.Split('\n');
            for (int i = 0; i < datas.Length; i++)
            {
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
                    type = (WeaponType)int.Parse(data[(int)WeaponVariable.WeaponType]),
                    RPM = int.Parse(data[(int)WeaponVariable.RPM]),
                    range = float.Parse(data[(int)WeaponVariable.Range]),
                    watchAngle = int.Parse(data[(int)WeaponVariable.WatchAngle]),
                    MOA = float.Parse(data[(int)WeaponVariable.MOA]),
                    stability = int.Parse(data[(int)WeaponVariable.Stability]),
                    rebound = int.Parse(data[(int)WeaponVariable.Rebound]),
                    actionCost = int.Parse(data[(int)WeaponVariable.ActionCost]),
                    useMagazine = ReadUsePartsSize(data[(int)WeaponVariable.UseMagazine]),
                    useMuzzle = ReadUsePartsSize(data[(int)WeaponVariable.UseMuzzle]),
                    useSight = ReadUsePartsSize(data[(int)WeaponVariable.UseScope]),
                    useUnderRail = ReadUsePartsSize(data[(int)WeaponVariable.UseAttachment]),
                    useRail = ReadUsePartsSize(data[(int)WeaponVariable.UseUnderBarrel]),
                };
                weaponData.weaponInfos.Add(weaponInfo);
            }
            Debug.Log("Update Weapon Data");

            List<WeaponPartsSize> ReadUsePartsSize(string sizeData)
            {
                var partSizeList = new List<WeaponPartsSize>();
                var sizeInfos = sizeData.Split(',');
                for (int i = 0; i < sizeInfos.Length; i++)
                {
                    var sizeInfo = sizeInfos[i];
                    var partsSize = (WeaponPartsSize)int.Parse(sizeInfo);
                    if (partsSize == WeaponPartsSize.None) break;

                    partSizeList.Add(partsSize);
                }

                return partSizeList;
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
        if (partsData == null)
        {
            partsData = Resources.Load<WeaponPartsData>("ScriptableObjects/WeaponPartsData");
        }

        if (partsData.partsInfos.Count > 0)
        {
            partsData.partsInfos.Clear();
        }
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
    private readonly string magDB = "https://docs.google.com/spreadsheets/d/1K4JDpojMJeJPpvA-u_sOK591Y16PBG45T77HCHyn_9w/export?format=tsv&gid=660227428&range=A2:G";
    private enum MagazineVariable
    {
        ID,
        PrefabName,
        MagazineName,
        CompatModel,
        CompatCaliber,
        Weight,
        MagazineSize,
    }

    public void UpdateMagazineData()
    {
        if (magData == null)
        {
            magData = Resources.Load<MagazineData>("ScriptableObjects/MagazineData");
        }

        if (magData.magInfos.Count > 0)
        {
            magData.magInfos.Clear();
        }
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
    private readonly string bulletDB = "https://docs.google.com/spreadsheets/d/1K4JDpojMJeJPpvA-u_sOK591Y16PBG45T77HCHyn_9w/export?format=tsv&gid=515744337&range=A2:J";
    private enum BulletVariable
    {
        ID,
        BulletName,
        Level,
        Caliber,
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
                    level = int.Parse(data[(int)BulletVariable.Level]),
                    caliber = float.Parse(data[(int)BulletVariable.Caliber]),
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
        if (armorData == null)
        {
            armorData = Resources.Load<ArmorData>("ScriptableObjects/ArmorData");
        }

        if (armorData.armorInfos.Count > 0)
        {
            armorData.armorInfos.Clear();
        }
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

    //#region Custom Editor
    //[CustomEditor(typeof(DataManager))]
    //public class DataEditor : Editor
    //{
    //    private DataManager dataMgr;

    //    private void OnEnable()
    //    {
    //        dataMgr = (DataManager)target;
    //    }

    //    public override void OnInspectorGUI()
    //    {
    //        base.OnInspectorGUI();
    //        GUILayout.Label('\n' + "---Read GoogleSheet Data---");
    //        if (GUILayout.Button("Update the Character Database"))
    //        {
    //            dataMgr.UpdateCharacterData();
    //            EditorUtility.SetDirty(dataMgr.charData);
    //        }
    //        if (GUILayout.Button("Update the Item Database"))
    //        {
    //            dataMgr.UpdateItemData();
    //            EditorUtility.SetDirty(dataMgr.itemData);
    //        }
    //        if (GUILayout.Button("Update the Weapon Database"))
    //        {
    //            dataMgr.UpdateWeaponData();
    //            EditorUtility.SetDirty(dataMgr.weaponData);
    //        }
    //        if (GUILayout.Button("Update the WeaponParts Database"))
    //        {
    //            dataMgr.UpdateWeaponPartsData();
    //            EditorUtility.SetDirty(dataMgr.partsData);
    //        }
    //        if (GUILayout.Button("Update the Magazine Database"))
    //        {
    //            dataMgr.UpdateMagazineData();
    //            EditorUtility.SetDirty(dataMgr.magData);
    //        }
    //        if (GUILayout.Button("Update the Bullet Database"))
    //        {
    //            dataMgr.UpdateBulletData();
    //            EditorUtility.SetDirty(dataMgr.bulletData);
    //        }
    //        if (GUILayout.Button("Update the Armor Database"))
    //        {
    //            dataMgr.UpdateArmorData();
    //            EditorUtility.SetDirty(dataMgr.armorData);
    //        }
    //        GUILayout.Label(" ");
    //        if (GUILayout.Button("Update All Database"))
    //        {
    //            dataMgr.UpdateCharacterData();
    //            EditorUtility.SetDirty(dataMgr.charData);
    //            dataMgr.UpdateItemData();
    //            EditorUtility.SetDirty(dataMgr.itemData);
    //            dataMgr.UpdateWeaponData();
    //            EditorUtility.SetDirty(dataMgr.weaponData);
    //            dataMgr.UpdateWeaponPartsData();
    //            EditorUtility.SetDirty(dataMgr.partsData);
    //            dataMgr.UpdateMagazineData();
    //            EditorUtility.SetDirty(dataMgr.magData);
    //            dataMgr.UpdateBulletData();
    //            EditorUtility.SetDirty(dataMgr.bulletData);
    //            dataMgr.UpdateArmorData();
    //            EditorUtility.SetDirty(dataMgr.armorData);
    //        }
    //    }
    //}
    //#endregion
}
