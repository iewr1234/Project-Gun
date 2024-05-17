using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEditor;
using TMPro;

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

    #region Weapon Data
    [HideInInspector] public WeaponData weaponData;
    private readonly string weaponDB = "https://docs.google.com/spreadsheets/d/1K4JDpojMJeJPpvA-u_sOK591Y16PBG45T77HCHyn_9w/export?format=tsv&gid=719783222&range=A2:P";
    private enum WeaponVariable
    {
        ID,
        PrefabName,
        WeaponName,
        WeaponType,
        Damage,
        Penetrate,
        ArmorBreak,
        Critical,
        RPM,
        Range,
        WatchAngle,
        MOA,
        Stability,
        Rebound,
        ActionCost,
        MagMax,
    }

    public void UpdateWeaponData()
    {
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
                    prefabName = data[(int)CharacterVariable.PrefabName],
                    weaponName = data[(int)WeaponVariable.WeaponName],
                    type = (WeaponType)int.Parse(data[(int)WeaponVariable.WeaponType]),
                    damage = int.Parse(data[(int)WeaponVariable.Damage]),
                    penetrate = int.Parse(data[(int)WeaponVariable.Penetrate]),
                    armorBreak = int.Parse(data[(int)WeaponVariable.ArmorBreak]),
                    critical = int.Parse(data[(int)WeaponVariable.Critical]),
                    rpm = int.Parse(data[(int)WeaponVariable.RPM]),
                    range = float.Parse(data[(int)WeaponVariable.Range]),
                    watchAngle = int.Parse(data[(int)WeaponVariable.WatchAngle]),
                    MOA = float.Parse(data[(int)WeaponVariable.MOA]),
                    stability = int.Parse(data[(int)WeaponVariable.Stability]),
                    rebound = int.Parse(data[(int)WeaponVariable.Rebound]),
                    actionCost = int.Parse(data[(int)WeaponVariable.ActionCost]),
                    magMax = int.Parse(data[(int)WeaponVariable.MagMax]),
                };
                weaponData.weaponInfos.Add(weaponInfo);
            }
            Debug.Log("Update Weapon Data");
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
            if (GUILayout.Button("Update the Character Database"))
            {
                dataMgr.SetComponents();
                dataMgr.UpdateCharacterData();
                EditorUtility.SetDirty(dataMgr.charData);
            }
            if (GUILayout.Button("Update the Weapon Database"))
            {
                dataMgr.SetComponents();
                dataMgr.UpdateWeaponData();
                EditorUtility.SetDirty(dataMgr.weaponData);
            }
            if (GUILayout.Button("Update the Armor Database"))
            {
                dataMgr.SetComponents();
                dataMgr.UpdateArmorData();
                EditorUtility.SetDirty(dataMgr.armorData);
            }
            GUILayout.Label(" ");
            if (GUILayout.Button("Update All Database"))
            {
                dataMgr.SetComponents();
                dataMgr.UpdateCharacterData();
                EditorUtility.SetDirty(dataMgr.charData);
                dataMgr.UpdateWeaponData();
                EditorUtility.SetDirty(dataMgr.weaponData);
                dataMgr.UpdateArmorData();
                EditorUtility.SetDirty(dataMgr.armorData);
            }
        }
    }
    #endregion
}
