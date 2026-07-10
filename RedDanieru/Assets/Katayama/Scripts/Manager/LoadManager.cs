using System.IO;
using UnityEngine;

public class LoadManager : MonoBehaviour
{
    // マップ管理
    [Header("参照")]
    [SerializeField] private MapManager mapManager;

    // 保存するファイル名
    [Header("保存ファイル名")]
    [SerializeField] private string fileName = "Dungeon.json";

    void Start()
    {
        // シーン開始時に保存データを読み込む
        Load();
    }

    /// ダンジョンを読み込む
    public void Load()
    {
        // 保存ファイルのパスを取得
        string path = Path.Combine(Application.persistentDataPath, fileName);

        // 保存ファイルが存在しない場合
        if (!File.Exists(path))
        {
            Debug.LogError("保存データが見つかりません。");
            return;
        }

        // JSONファイルを文字列として読み込む
        string json = File.ReadAllText(path);

        // JSONをDungeonDataへ変換
        DungeonMapData data = JsonUtility.FromJson<DungeonMapData>(json);

        // MapManagerへ渡してマップを生成
        mapManager.LoadDungeon(data);

        Debug.Log("ダンジョンを読み込みました。");
    }

    /// 保存データが存在するか確認
    /// <returns>存在する場合はtrue</returns>
    public bool Exists()
    {
        // 保存ファイルのパスを取得
        string path = Path.Combine(Application.persistentDataPath, fileName);

        // ファイルの存在を返す
        return File.Exists(path);
    }
}