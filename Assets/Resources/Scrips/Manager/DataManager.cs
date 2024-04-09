using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

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
    }

    #region Character Data
    [HideInInspector] public CharacterData charData;
    private readonly string charDB = "https://docs.google.com/spreadsheets/d/1K4JDpojMJeJPpvA-u_sOK591Y16PBG45T77HCHyn_9w/export?format=tsv&gid=676425891&range=A3:P";
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
        MaxHealth,
        MaxStamina,
        Sight,
        Mobility,
        Aiming,
        Reaction,
        MainWeaponID,
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
                    maxHealth = int.Parse(data[(int)CharacterVariable.MaxHealth]),
                    maxStamina = int.Parse(data[(int)CharacterVariable.MaxStamina]),
                    sight = float.Parse(data[(int)CharacterVariable.Sight]),
                    mobility = int.Parse(data[(int)CharacterVariable.Mobility]),
                    aiming = int.Parse(data[(int)CharacterVariable.Aiming]),
                    reaction = int.Parse(data[(int)CharacterVariable.Reaction]),
                    mainWeaponID = data[(int)CharacterVariable.MainWeaponID].Replace("\r", ""),
                };
                charData.charInfos.Add(playerInfo);
            }
            Debug.Log("Update Character Data");
        }
    }
    #endregion

    #region Weapon Data
    [HideInInspector] public WeaponData weaponData;
    private readonly string weaponDB = "https://docs.google.com/spreadsheets/d/1K4JDpojMJeJPpvA-u_sOK591Y16PBG45T77HCHyn_9w/export?format=tsv&gid=719783222&range=A2:M";
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
        Range,
        MOA,
        Stability,
        Rebound,
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
                    range = float.Parse(data[(int)WeaponVariable.Range]),
                    MOA = int.Parse(data[(int)WeaponVariable.MOA]),
                    stability = int.Parse(data[(int)WeaponVariable.Stability]),
                    rebound = int.Parse(data[(int)WeaponVariable.Rebound]),
                    magMax = int.Parse(data[(int)WeaponVariable.MagMax]),
                };
                weaponData.weaponInfos.Add(weaponInfo);
            }
            Debug.Log("Update Weapon Data");
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
            if (GUILayout.Button("Update All Database"))
            {
                dataMgr.SetComponents();
                dataMgr.UpdateCharacterData();
                EditorUtility.SetDirty(dataMgr.charData);
                dataMgr.UpdateWeaponData();
                EditorUtility.SetDirty(dataMgr.weaponData);
            }
        }
    }
    #endregion
}
