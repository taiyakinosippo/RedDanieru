using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    // マップ管理
    [Header("参照")]
    [SerializeField] private MapManager mapManager;

    /// ダンジョンを名前指定で保存する
    /// <param name="dungeonName">保存するダンジョン名</param>
    public void Save(string dungeonName)
    {
        // MapManagerが設定されているか確認
        if (mapManager == null)
        {
            Debug.LogError("MapManagerが設定されていません。");
            return;
        }

        // ダンジョン名が入力されているか確認
        if (string.IsNullOrWhiteSpace(dungeonName))
        {
            Debug.LogError("ダンジョン名が入力されていません。");
            return;
        }

        // ファイル名に使用できない文字を除去
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            dungeonName = dungeonName.Replace(c.ToString(), "");
        }

        // 現在のマップ情報を取得
        DungeonMapData dungeonData = mapManager.CreateSaveData();

        // マップ情報をJSON形式へ変換
        string json = JsonUtility.ToJson(dungeonData, true);

        // 保存先のパスを作成
        string path = Path.Combine(
            Application.persistentDataPath,
            dungeonName + ".json"
        );

        // JSONファイルとして保存
        File.WriteAllText(path, json);

        Debug.Log($"保存完了 : {path}");
    }
}