using System.IO;
using UnityEngine;

public class LoadManager : MonoBehaviour
{
    // マップ管理
    [Header("参照")]
    [SerializeField] private MapManager mapManager;

    /// <summary>
    /// 指定したダンジョンを読み込む
    /// </summary>
    /// <param name="dungeonName">読み込むダンジョン名</param>
    public void Load(string dungeonName)
    {
        // 保存ファイルのパスを取得
        string path = Path.Combine(
            Application.persistentDataPath,
            dungeonName + ".json"
        );

        // 保存ファイルが存在しない場合
        if (!File.Exists(path))
        {
            Debug.LogError($"保存データが見つかりません : {dungeonName}");
            return;
        }

        // JSONファイルを文字列として読み込む
        string json = File.ReadAllText(path);

        // JSONをDungeonMapDataへ変換
        DungeonMapData data =
            JsonUtility.FromJson<DungeonMapData>(json);

        // MapManagerへ渡してマップを生成
        mapManager.LoadDungeon(data);

        Debug.Log($"ダンジョンを読み込みました : {dungeonName}");
    }

    /// <summary>
    /// 指定したダンジョンが存在するか確認
    /// </summary>
    public bool Exists(string dungeonName)
    {
        string path = Path.Combine(
            Application.persistentDataPath,
            dungeonName + ".json"
        );

        return File.Exists(path);
    }
}