using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private MapManager mapManager;

    public void Save(string dungeonName)
    {
        if (mapManager == null)
        {
            Debug.LogError(
                "MapManagerが設定されていません。"
            );

            return;
        }

        /*
         * ダンジョン名を先に確認
         */
        if (string.IsNullOrWhiteSpace(dungeonName))
        {
            Debug.LogError(
                "ダンジョン名が入力されていません。"
            );

            return;
        }

        /*
         * 使用できない文字を削除
         */
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            dungeonName =
                dungeonName.Replace(
                    c.ToString(),
                    ""
                );
        }

        /*
         * Goalがあるか確認
         */
        if (!mapManager.HasGoal())
        {
            Debug.LogError(
                "Goalを配置してください。"
            );

            return;
        }

        /*
         * マップデータを作成
         */
        DungeonMapData dungeonData =
            mapManager.CreateSaveData();

        string json =
            JsonUtility.ToJson(
                dungeonData,
                true
            );

        string path =
            Path.Combine(
                Application.persistentDataPath,
                dungeonName + ".json"
            );

        /*
         * JSONを保存
         */
        File.WriteAllText(
            path,
            json
        );

        /*
         * 保存が完了してからNavMeshを再生成
         *
         * NavMeshは床だけを対象にする。
         */
        mapManager.BuildNavigation();

        Debug.Log(
            $"保存完了 : {path}"
        );
    }
}