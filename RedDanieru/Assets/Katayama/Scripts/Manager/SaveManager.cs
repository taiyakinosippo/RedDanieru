using System.Collections.Generic;
using System.IO;
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

    private Image saveButtonImage;


    //==================================================
    // Start
    //==================================================

    private void Start()
    {
        if (saveButton != null)
        {
            saveButtonImage =
                saveButton.GetComponent<Image>();
        }

        UpdateSaveButton();
    }


    //==================================================
    // テストプレイクリア
    //==================================================

    public void SetTestPlayCleared()
    {
        if (mapManager == null)
        {
            Debug.LogError(
                "MapManagerが設定されていません。"
            );

            return;
        }

        testPlayCleared = true;

        clearedMapRevision =
            mapManager.MapRevision;

        UpdateSaveButton();

        Debug.Log(
            "テストプレイクリア。"
        );
    }


    //==================================================
    // マップ変更
    //==================================================

    public void SetMapModified()
    {
        testPlayCleared = false;
        clearedMapRevision = -1;

        UpdateSaveButton();

        Debug.Log(
            "マップが変更されました。"
        );
    }


    //==================================================
    // Saveボタン更新
    //==================================================

    private void UpdateSaveButton()
    {
        if (saveButton == null)
        {
            return;
        }

        // ローカル保存はいつでも可能
        saveButton.interactable = true;

        if (saveButtonImage == null)
        {
            saveButtonImage =
                saveButton.GetComponent<Image>();
        }

        if (saveButtonImage != null)
        {
            saveButtonImage.color =
                enabledColor;
        }
    }


    //==================================================
    // テストプレイをクリア済みか
    //==================================================

    public bool IsTestPlayCleared()
    {
        if (!testPlayCleared)
        {
            return false;
        }

        if (mapManager == null)
        {
            return false;
        }

        if (
            clearedMapRevision !=
            mapManager.MapRevision
        )
        {
            testPlayCleared = false;
            clearedMapRevision = -1;

            Debug.Log(
                "クリア後にマップが変更されています。"
            );

            return false;
        }

        return true;
    }


    //==================================================
    // 新規保存
    //==================================================

    public bool Save(
        string dungeonName
    )
    {
        if (mapManager == null)
        {
            Debug.LogError(
                "MapManagerが設定されていません。"
            );

            return false;
        }

        dungeonName =
            CleanDungeonName(
                dungeonName
            );

        if (string.IsNullOrWhiteSpace(
            dungeonName
        ))
        {
            Debug.LogError(
                "ダンジョン名が入力されていません。"
            );

            return false;
        }

        string path =
            GetDungeonPath(
                dungeonName
            );

        // 同じ名前が存在する場合は新規保存しない
        if (File.Exists(path))
        {
            Debug.LogWarning(
                "同じ名前のダンジョンが既に存在します : "
                + dungeonName
            );

            return false;
        }

        // 現在のマップデータを作成
        DungeonMapData dungeonData =
            mapManager.CreateSaveData();

        // 新しいIDを作成
        string dungeonId =
            System.Guid.NewGuid().ToString();

        dungeonData.dungeonId =
            dungeonId;

        dungeonData.dungeonName =
            dungeonName;

        LastDungeonId =
            dungeonId;

        LastDungeonName =
            dungeonName;

        // JSON化
        string json =
            JsonUtility.ToJson(
                dungeonData,
                true
            );

        // 保存
        File.WriteAllText(
            path,
            json
        );

        Debug.Log(
            "ローカル保存完了 : "
            + dungeonName
        );

        Debug.Log(
            "保存先 : "
            + path
        );

        return true;
    }


    //==================================================
    // 上書き保存
    //==================================================

    public bool Overwrite(
        string dungeonName
    )
    {
        if (mapManager == null)
        {
            Debug.LogError(
                "MapManagerが設定されていません。"
            );

            return false;
        }

        dungeonName =
            CleanDungeonName(
                dungeonName
            );

        if (string.IsNullOrWhiteSpace(
            dungeonName
        ))
        {
            Debug.LogError(
                "ダンジョン名がありません。"
            );

            return false;
        }

        string path =
            GetDungeonPath(
                dungeonName
            );

        if (!File.Exists(path))
        {
            Debug.LogError(
                "上書き対象が存在しません : "
                + dungeonName
            );

            return false;
        }

        // 既存データを読み込む
        string oldJson =
            File.ReadAllText(
                path
            );

        DungeonMapData oldData =
            JsonUtility.FromJson<DungeonMapData>(
                oldJson
            );

        if (oldData == null)
        {
            Debug.LogError(
                "既存のダンジョンデータを読み込めませんでした。"
            );

            return false;
        }

        // 現在のマップデータを取得
        DungeonMapData newData =
            mapManager.CreateSaveData();

        // 既存のIDを維持
        newData.dungeonId =
            oldData.dungeonId;

        // 古いデータでIDが無い場合は新しく作成
        if (string.IsNullOrEmpty(newData.dungeonId))
        {
            newData.dungeonId =
                System.Guid.NewGuid().ToString();
        }

        newData.dungeonName =
            dungeonName;

        LastDungeonId =
            newData.dungeonId;

        LastDungeonName =
            dungeonName;

        // JSON化
        string json =
            JsonUtility.ToJson(
                newData,
                true
            );

        // 上書き
        File.WriteAllText(
            path,
            json
        );

        Debug.Log(
            "ローカル上書き保存完了 : "
            + dungeonName
        );

        Debug.Log(
            "保存先 : "
            + path
        );

        return true;
    }


    //==================================================
    // 保存済みダンジョン名を取得
    //==================================================

    public string[] GetSavedDungeonNames()
    {
        string[] files =
            Directory.GetFiles(
                Application.persistentDataPath,
                "*.json"
            );

        List<string> dungeonNames =
            new List<string>();

        foreach (string file in files)
        {
            try
            {
                string json =
                    File.ReadAllText(
                        file
                    );

                DungeonMapData data =
                    JsonUtility.FromJson<DungeonMapData>(
                        json
                    );

                if (
                    data != null &&
                    !string.IsNullOrWhiteSpace(
                        data.dungeonName
                    )
                )
                {
                    dungeonNames.Add(
                        data.dungeonName
                    );
                }
            }
            catch
            {
                Debug.LogWarning(
                    "ダンジョンデータを読み込めませんでした : "
                    + file
                );
            }
        }

        return dungeonNames.ToArray();
    }


    //==================================================
    // ダンジョン削除
    //==================================================

    public void Delete(
        string dungeonName
    )
    {
        if (string.IsNullOrWhiteSpace(
            dungeonName
        ))
        {
            return;
        }

        string path =
            GetDungeonPath(
                dungeonName
            );

        if (!File.Exists(path))
        {
            Debug.LogWarning(
                "削除するダンジョンがありません : "
                + dungeonName
            );

            return;
        }

        File.Delete(path);

        Debug.Log(
            "ローカルダンジョンを削除しました : "
            + dungeonName
        );
    }


    //==================================================
    // ダンジョン名を整理
    //==================================================

    private string CleanDungeonName(
        string dungeonName
    )
    {
        if (string.IsNullOrWhiteSpace(
            dungeonName
        ))
        {
            return "";
        }

        foreach (
            char c
            in Path.GetInvalidFileNameChars()
        )
        {
            dungeonName =
                dungeonName.Replace(
                    c.ToString(),
                    ""
                );
        }

        return dungeonName.Trim();
    }


    //==================================================
    // 保存パス
    //==================================================

    private string GetDungeonPath(
        string dungeonName
    )
    {
        return Path.Combine(
            Application.persistentDataPath,
            dungeonName + ".json"
        );
    }
}