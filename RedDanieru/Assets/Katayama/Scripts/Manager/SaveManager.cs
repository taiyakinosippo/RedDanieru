using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    //==================================================
    // 参照
    //==================================================

    [Header("参照")]
    [SerializeField] private MapManager mapManager;

    //==================================================
    // 保存情報
    //==================================================

    public static string LastDungeonId;

    //==================================================
    // クリア情報
    //==================================================

    // テストプレイをクリアしたか
    private bool testPlayCleared = false;

    // クリアした時点のマップ変更番号
    private int clearedMapRevision = -1;

    //==================================================
    // クリア通知
    //==================================================

    /// <summary>
    /// テストプレイでGoalに到達したことを通知する
    /// </summary>
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

        Debug.Log(
            "テストプレイクリア。保存可能になりました。"
        );
    }

    //==================================================
    // マップ変更通知
    //==================================================

    /// <summary>
    /// マップが変更されたときに呼び出す
    /// </summary>
    public void SetMapModified()
    {
        testPlayCleared = false;
        clearedMapRevision = -1;

        Debug.Log(
            "マップが変更されたため、クリア状態を解除しました。"
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

        // クリア後にマップが変更されていないか確認
        if (
            clearedMapRevision !=
            mapManager.MapRevision
        )
        {
            testPlayCleared = false;
            clearedMapRevision = -1;

            Debug.LogError(
                "クリア後にマップが変更されています。"
                + "\nもう一度テストプレイをクリアしてください。"
            );

            return false;
        }

        // Goal確認
        if (!mapManager.HasGoal())
        {
            Debug.LogError(
                "Goalを配置してください。"
            );

            return false;
        }

        // SpawnPoint確認
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
    // ダンジョン保存
    //==================================================

    /// <summary>
    /// ダンジョンを名前指定で保存する
    /// </summary>
    public void Save(string dungeonName)
    {
        //==================================================
        // MapManager確認
        //==================================================

        if (mapManager == null)
        {
            Debug.LogError(
                "MapManagerが設定されていません。"
            );

            return;
        }

        //==================================================
        // 保存条件確認
        //==================================================

        if (!CanSave())
        {
            return;
        }

        //==================================================
        // ダンジョン名確認
        //==================================================

        if (string.IsNullOrWhiteSpace(dungeonName))
        {
            Debug.LogError(
                "ダンジョン名が入力されていません。"
            );

            return;
        }

        //==================================================
        // ファイル名に使用できない文字を除去
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

        if (string.IsNullOrWhiteSpace(dungeonName))
        {
            Debug.LogError(
                "使用できる文字がないため保存できません。"
            );

            return;
        }

        //==================================================
        // 保存前にNavMesh生成
        //==================================================

        mapManager.BuildNavigation();

        //==================================================
        // 現在のマップ情報取得
        //==================================================

        DungeonMapData dungeonData =
            mapManager.CreateSaveData();

        //==================================================
        // ダンジョンID作成
        //==================================================

        string dungeonId =
            System.Guid.NewGuid().ToString();

        dungeonData.dungeonId =
            dungeonId;

        dungeonData.dungeonName =
            dungeonName;

        LastDungeonId =
            dungeonId;

        //==================================================
        // JSON変換
        //==================================================

        string json =
            JsonUtility.ToJson(
                dungeonData,
                true
            );

        //==================================================
        // 保存先
        //==================================================

        string path =
            Path.Combine(
                Application.persistentDataPath,
                dungeonId + ".json"
            );

        //==================================================
        // 保存
        //==================================================

        File.WriteAllText(
            path,
            json
        );

        Debug.Log(
            $"保存完了 : {path}"
        );

        // 保存完了後は現在の状態を基準にする
        testPlayCleared = false;
        clearedMapRevision = -1;
    }
}