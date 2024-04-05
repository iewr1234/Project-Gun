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
        if (playerData == null)
        {
            playerData = Resources.Load<PlayerData>("ScriptableObjects/PlayerData");
        }
    }

    #region Player Data
    [HideInInspector] public PlayerData playerData;
    private readonly string playerDB = "https://docs.google.com/spreadsheets/d/1K4JDpojMJeJPpvA-u_sOK591Y16PBG45T77HCHyn_9w/export?format=tsv&gid=676425891&range=A2:F";
    private enum PlayerVariable
    {
        ID,
        PrefabName,
        CharacterName,
        Mobility,
        MaxHealth,
        MainWeaponID,
    }

    public void UpdatePlayerData()
    {
        if (playerData.playerInfos.Count > 0)
        {
            playerData.playerInfos.Clear();
        }
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
                    indexName = $"{data[(int)PlayerVariable.ID]}: {data[(int)PlayerVariable.CharacterName]}",
                    ID = data[(int)PlayerVariable.ID],
                    prefabName = data[(int)PlayerVariable.PrefabName],
                    charName = data[(int)PlayerVariable.CharacterName],
                    mobility = int.Parse(data[(int)PlayerVariable.Mobility]),
                    maxHealth = int.Parse(data[(int)PlayerVariable.MaxHealth]),
                    mainWeaponID = data[(int)PlayerVariable.MainWeaponID],
                };
                playerData.playerInfos.Add(playerInfo);
            }
            Debug.Log("Update Player Data");
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
            if (GUILayout.Button("Update the Player Database"))
            {
                dataMgr.SetComponents();
                dataMgr.UpdatePlayerData();
                EditorUtility.SetDirty(dataMgr.playerData);
            }
            if (GUILayout.Button("Update All Database"))
            {
                dataMgr.SetComponents();
                dataMgr.UpdatePlayerData();
                EditorUtility.SetDirty(dataMgr.playerData);
            }
        }
    }
    #endregion
}