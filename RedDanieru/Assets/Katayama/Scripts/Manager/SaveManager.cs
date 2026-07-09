using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    // マップ管理
    [Header("参照")]
    [SerializeField] private MapManager mapManager;

    // 保存するファイル名
    [Header("保存ファイル名")]
    [SerializeField] private string fileName = "Dungeon.json";

    /// ダンジョンを保存する
    public void Save()
    {
        // MapManagerが設定されているか確認
        if (mapManager == null)
        {
            Debug.LogError("MapManagerが設定されていません。");
            return;
        }

        // 現在のマップ情報を取得
        DungeonMapData dungeonData = mapManager.CreateSaveData();

        // マップ情報をJSON形式へ変換
        string json = JsonUtility.ToJson(dungeonData, true);

        // 保存先のパスを取得
        string path = Path.Combine(Application.persistentDataPath, fileName);

        // JSONファイルとして保存
        File.WriteAllText(path, json);

        Debug.Log($"保存完了 : {path}");
    }

    /// 保存ファイルのパスを取得する
    /// <returns>保存先のパス</returns>
    public string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    /// 保存データが存在するか確認する
    /// <returns>存在する場合はtrue</returns>
    public bool Exists()
    {
        return File.Exists(GetSavePath());
    }

    /// 保存データを削除する
    public void DeleteSave()
    {
        // 保存データが存在する場合のみ削除
        if (Exists())
        {
            File.Delete(GetSavePath());
            Debug.Log("保存データを削除しました。");
        }
    }
}