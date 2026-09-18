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

        // ローカル保存はいつでも可能
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

        // ローカル保存は常に可能
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

        // クリア後にマップが変更された場合
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
    // ローカル保存
    //==================================================

    public void Save(string dungeonName)
    {
        if (mapManager == null)
        {
            Debug.LogError(
                "MapManagerが設定されていません。"
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(dungeonName))
        {
            Debug.LogError(
                "ダンジョン名が入力されていません。"
            );

            return;
        }

        //==================================================
        // ファイル名に使用できない文字を削除
        //==================================================

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

        dungeonName =
            dungeonName.Trim();

        if (string.IsNullOrWhiteSpace(dungeonName))
        {
            Debug.LogError(
                "使用できる文字がないため保存できません。"
            );

            return;
        }

        //==================================================
        // 既に同じ名前が存在するか
        //==================================================

        string path =
            Path.Combine(
                Application.persistentDataPath,
                dungeonName + ".json"
            );

        if (File.Exists(path))
        {
            Debug.LogWarning(
                "同じ名前のダンジョンが既に存在します : "
                + dungeonName
            );

            return;
        }

        //==================================================
        // マップデータ作成
        //==================================================

        DungeonMapData dungeonData =
            mapManager.CreateSaveData();

        //==================================================
        // ダンジョンID生成
        //==================================================

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

        //==================================================
        // JSON化
        //==================================================

        string json =
            JsonUtility.ToJson(
                dungeonData,
                true
            );

        //==================================================
        // ローカル保存
        //==================================================

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
    }

    //==================================================
    // 保存済みダンジョンを削除
    //==================================================

    public void Delete(string dungeonName)
    {
        if (string.IsNullOrWhiteSpace(dungeonName))
        {
            return;
        }

        string path =
            Path.Combine(
                Application.persistentDataPath,
                dungeonName + ".json"
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
}
