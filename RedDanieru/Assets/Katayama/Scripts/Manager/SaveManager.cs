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

    public static string LastDungeonId;

    private bool testPlayCleared = false;
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
            "テストプレイクリア。保存可能になりました。"
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
            "マップが変更されたため、クリア状態を解除しました。"
        );
    }

    //==================================================
    // Saveボタン更新
    //==================================================

    private void UpdateSaveButton()
    {
        if (saveButton == null)
        {
            Debug.LogWarning(
                "Save Buttonが設定されていません。"
            );

            return;
        }

        // ボタンを押せるか
        saveButton.interactable =
            testPlayCleared;

        // ボタンのImageを取得
        if (saveButtonImage == null)
        {
            saveButtonImage =
                saveButton.GetComponent<Image>();
        }

        if (saveButtonImage != null)
        {
            if (testPlayCleared)
            {
                // クリア後
                saveButtonImage.color =
                    enabledColor;
            }
            else
            {
                // クリア前
                saveButtonImage.color =
                    disabledColor;
            }
        }

        Debug.Log(
            "Saveボタン更新 : " +
            (testPlayCleared ? "白" : "灰色")
        );
    }

    //==================================================
    // 保存可能か確認
    //==================================================

    private bool CanSave()
    {
        if (!testPlayCleared)
        {
            Debug.LogError(
                "テストプレイをクリアしていないため保存できません。"
            );

            return false;
        }

        if (mapManager == null)
        {
            Debug.LogError(
                "MapManagerが設定されていません。"
            );

            return false;
        }

        if (clearedMapRevision !=
            mapManager.MapRevision)
        {
            testPlayCleared = false;
            clearedMapRevision = -1;

            UpdateSaveButton();

            Debug.LogError(
                "クリア後にマップが変更されています。\n" +
                "もう一度テストプレイをクリアしてください。"
            );

            return false;
        }

        if (!mapManager.HasGoal())
        {
            Debug.LogError(
                "Goalを配置してください。"
            );

            return false;
        }

        if (!mapManager.HasRespawnPoint())
        {
            Debug.LogError(
                "SpawnPointがありません。"
            );

            return false;
        }

        return true;
    }

    //==================================================
    // 保存
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

        if (!CanSave())
            return;

        if (string.IsNullOrWhiteSpace(dungeonName))
        {
            Debug.LogError(
                "ダンジョン名が入力されていません。"
            );

            return;
        }

        foreach (char c in Path.GetInvalidFileNameChars())
        {
            dungeonName =
                dungeonName.Replace(
                    c.ToString(),
                    ""
                );
        }

        if (string.IsNullOrWhiteSpace(dungeonName))
        {
            Debug.LogError(
                "使用できる文字がないため保存できません。"
            );

            return;
        }

        mapManager.BuildNavigation();

        DungeonMapData dungeonData =
            mapManager.CreateSaveData();

        string dungeonId =
            System.Guid.NewGuid().ToString();

        dungeonData.dungeonId =
            dungeonId;

        dungeonData.dungeonName =
            dungeonName;

        LastDungeonId =
            dungeonId;

        string json =
            JsonUtility.ToJson(
                dungeonData,
                true
            );

        string path =
            Path.Combine(
                Application.persistentDataPath,
                dungeonId + ".json"
            );

        File.WriteAllText(
            path,
            json
        );

        Debug.Log(
            $"保存完了 : {path}"
        );

        // 保存後は再び保存不可
        testPlayCleared = false;
        clearedMapRevision = -1;

        UpdateSaveButton();
    }
}
