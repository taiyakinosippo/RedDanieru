
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SaveManager : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private MapManager mapManager;

    [Header("Saveボタン")]
    [SerializeField] private Button saveButton;

    [Header("Saveボタンの色")]
    [SerializeField] private Color disabledColor = Color.gray;
    [SerializeField] private Color enabledColor = Color.white;

    // 最後に保存したダンジョンID
    public static string LastDungeonId;

    // 最後に保存したダンジョン名
    public static string LastDungeonName;

    // テストプレイをクリアしたか
    private bool testPlayCleared = false;

    // クリア時のマップRevision
    private int clearedMapRevision = -1;

    // セーブ後にマップが変更されたか
    private bool hasUnsavedChanges = false;

    private Image saveButtonImage;

    private void Start()
    {
        if (saveButton != null)
            saveButtonImage = saveButton.GetComponent<Image>();

        hasUnsavedChanges = false;
        UpdateSaveButton();
    }

    //==================================================
    // 未保存変更があるか
    //==================================================

    public bool HasUnsavedChanges()
    {
        return hasUnsavedChanges;
    }

    // ダンジョン読み込み完了時に未保存状態を解除する
    public void SetMapLoadedClean()
    {
        hasUnsavedChanges = false;
        testPlayCleared = false;
        clearedMapRevision = -1;
        UpdateSaveButton();
    }

    //==================================================
    // テストプレイクリア
    //==================================================

    public void SetTestPlayCleared()
    {
        if (mapManager == null)
        {
            Debug.LogError("MapManagerが設定されていません。");
            return;
        }

        testPlayCleared = true;
        clearedMapRevision = mapManager.MapRevision;
        UpdateSaveButton();

        Debug.Log("テストプレイクリア。");
    }

    //==================================================
    // マップ変更
    //==================================================

    public void SetMapModified()
    {
        testPlayCleared = false;
        clearedMapRevision = -1;
        hasUnsavedChanges = true;
        UpdateSaveButton();

        Debug.Log("マップが変更されました。");
    }

    //==================================================
    // Saveボタン更新
    //==================================================

    private void UpdateSaveButton()
    {
        if (saveButton == null)
            return;

        saveButton.interactable = true;

        if (saveButtonImage == null)
            saveButtonImage = saveButton.GetComponent<Image>();

        if (saveButtonImage != null)
            saveButtonImage.color = enabledColor;
    }

    //==================================================
    // テストプレイをクリア済みか
    //==================================================

    public bool IsTestPlayCleared()
    {
        if (!testPlayCleared || mapManager == null)
            return false;

        if (clearedMapRevision != mapManager.MapRevision)
        {
            testPlayCleared = false;
            clearedMapRevision = -1;

            Debug.Log("クリア後にマップが変更されています。");
            return false;
        }

        return true;
    }

    //==================================================
    // 新規保存
    //==================================================

    public bool Save(string dungeonName)
    {
        if (mapManager == null)
        {
            Debug.LogError("MapManagerが設定されていません。");
            return false;
        }

        dungeonName = CleanDungeonName(dungeonName);

        if (string.IsNullOrWhiteSpace(dungeonName))
        {
            Debug.LogError("ダンジョン名が入力されていません。");
            return false;
        }

        string path = GetDungeonPath(dungeonName);

        if (File.Exists(path))
        {
            Debug.LogWarning(
                "同じ名前のダンジョンが既に存在します : " + dungeonName
            );
            return false;
        }

        DungeonMapData dungeonData = mapManager.CreateSaveData();

        if (dungeonData == null)
        {
            Debug.LogError("保存データを作成できませんでした。");
            return false;
        }

        string dungeonId = Guid.NewGuid().ToString();

        dungeonData.dungeonId = dungeonId;
        dungeonData.dungeonName = dungeonName;

        LastDungeonId = dungeonId;
        LastDungeonName = dungeonName;

        string json = JsonUtility.ToJson(dungeonData, true);
        File.WriteAllText(path, json);

        hasUnsavedChanges = false;

        Debug.Log("ローカル保存完了 : " + dungeonName);
        Debug.Log("保存先 : " + path);

        return true;
    }

    //==================================================
    // 上書き保存
    //==================================================

    public bool Overwrite(string dungeonName)
    {
        if (mapManager == null)
        {
            Debug.LogError("MapManagerが設定されていません。");
            return false;
        }

        dungeonName = CleanDungeonName(dungeonName);

        if (string.IsNullOrWhiteSpace(dungeonName))
        {
            Debug.LogError("ダンジョン名がありません。");
            return false;
        }

        string path = GetDungeonPath(dungeonName);

        if (!File.Exists(path))
        {
            Debug.LogError("上書き対象が存在しません : " + dungeonName);
            return false;
        }

        DungeonMapData oldData;

        try
        {
            string oldJson = File.ReadAllText(path);
            oldData = JsonUtility.FromJson<DungeonMapData>(oldJson);
        }
        catch (Exception e)
        {
            Debug.LogError("既存データの読み込みに失敗しました: " + e.Message);
            return false;
        }

        if (oldData == null)
        {
            Debug.LogError("既存のダンジョンデータを読み込めませんでした。");
            return false;
        }

        DungeonMapData newData = mapManager.CreateSaveData();

        if (newData == null)
        {
            Debug.LogError("保存データを作成できませんでした。");
            return false;
        }

        // 既存のIDを維持する
        newData.dungeonId = oldData.dungeonId;

        if (string.IsNullOrEmpty(newData.dungeonId))
            newData.dungeonId = Guid.NewGuid().ToString();

        newData.dungeonName = dungeonName;

        LastDungeonId = newData.dungeonId;
        LastDungeonName = dungeonName;

        try
        {
            string json = JsonUtility.ToJson(newData, true);
            File.WriteAllText(path, json);
        }
        catch (Exception e)
        {
            Debug.LogError("上書き保存に失敗しました: " + e.Message);
            return false;
        }

        hasUnsavedChanges = false;

        Debug.Log("ローカル上書き保存完了 : " + dungeonName);
        Debug.Log("保存先 : " + path);

        return true;
    }

    //==================================================
    // 保存済みダンジョン名を登録日時の古い順に取得
    //==================================================

    public string[] GetSavedDungeonNames()
    {
        string[] files = Directory.GetFiles(
            Application.persistentDataPath,
            "*.json"
        );

        List<(string dungeonName, DateTime registeredAt)> dungeonList =
            new List<(string, DateTime)>();

        foreach (string file in files)
        {
            try
            {
                string json = File.ReadAllText(file);

                DungeonMapData data =
                    JsonUtility.FromJson<DungeonMapData>(json);

                if (data == null ||
                    string.IsNullOrWhiteSpace(data.dungeonName))
                {
                    continue;
                }

                // ファイルの作成日時を登録日時として使用する
                DateTime registeredAt = File.GetCreationTime(file);

                dungeonList.Add((
                    data.dungeonName,
                    registeredAt
                ));
            }
            catch (Exception e)
            {
                Debug.LogWarning(
                    "ダンジョンデータを読み込めませんでした : "
                    + file + "\n" + e.Message
                );
            }
        }

        // 古い登録日時を先に表示する
        return dungeonList
            .OrderBy(dungeon => dungeon.registeredAt)
            .ThenBy(dungeon => dungeon.dungeonName)
            .Select(dungeon => dungeon.dungeonName)
            .ToArray();
    }

    //==================================================
    // ダンジョン削除
    //==================================================

    public void Delete(string dungeonName)
    {
        if (string.IsNullOrWhiteSpace(dungeonName))
            return;

        string path = GetDungeonPath(dungeonName);

        if (!File.Exists(path))
        {
            Debug.LogWarning("削除するダンジョンがありません : " + dungeonName);
            return;
        }

        File.Delete(path);

        Debug.Log("ローカルダンジョンを削除しました : " + dungeonName);
    }

    //==================================================
    // ダンジョン名を整理
    //==================================================

    private string CleanDungeonName(string dungeonName)
    {
        if (string.IsNullOrWhiteSpace(dungeonName))
            return "";

        foreach (char c in Path.GetInvalidFileNameChars())
            dungeonName = dungeonName.Replace(c.ToString(), "");

        return dungeonName.Trim();
    }

    //==================================================
    // 保存パス
    //==================================================

    private string GetDungeonPath(string dungeonName)
    {
        return Path.Combine(
            Application.persistentDataPath,
            dungeonName + ".json"
        );
    }
}
