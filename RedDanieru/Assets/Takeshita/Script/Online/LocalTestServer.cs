using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// テストモード用の「サーバーの代わり」
/// PHPサーバーの代わりに、同じPC内のファイルでルームとステージを管理する
/// 同じPCで複数起動したゲーム同士はルーム一覧を共有できる
/// </summary>
public static class LocalTestServer
{
    // これより長くAlive更新が無いルームは終了したものとみなす
    private const double RoomTimeoutSeconds = 10.0;

    [Serializable]
    private class LocalRoomFile
    {
        public RoomData room;
        public long aliveUnixMs;
    }

    private static string RoomFolder =>
        Path.Combine(Application.persistentDataPath, "LocalTestRooms");

    private static string TestStageFolder =>
        Path.Combine(Application.streamingAssetsPath, "TestStages");

    private static long NowUnixMs =>
        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    //==================================================
    // ルーム
    //==================================================

    public static void SaveRoom(RoomData room)
    {
        WriteRoom(new LocalRoomFile { room = room, aliveUnixMs = NowUnixMs });
    }

    public static void DeleteRoom(string roomId)
    {
        if (string.IsNullOrEmpty(roomId))
            return;

        try
        {
            File.Delete(GetRoomPath(roomId));
        }
        catch (IOException e)
        {
            Debug.LogWarning("ローカルルームの削除に失敗 : " + e.Message);
        }
    }

    public static void KeepAlive(string roomId, int currentPlayers)
    {
        LocalRoomFile file = ReadRoom(GetRoomPath(roomId));

        if (file == null)
            return;

        file.room.current_players = currentPlayers;
        file.aliveUnixMs = NowUnixMs;

        WriteRoom(file);
    }

    public static RoomData FindRoom(string roomId)
    {
        if (string.IsNullOrEmpty(roomId))
            return null;

        LocalRoomFile file = ReadRoom(GetRoomPath(roomId));

        return file != null && !IsExpired(file) ? file.room : null;
    }

    public static RoomData[] GetRooms()
    {
        List<RoomData> rooms = new List<RoomData>();

        if (!Directory.Exists(RoomFolder))
            return rooms.ToArray();

        foreach (string path in Directory.GetFiles(RoomFolder, "*.json"))
        {
            LocalRoomFile file = ReadRoom(path);

            if (file == null)
                continue;

            if (IsExpired(file))
            {
                // 落ちたホストのルームを掃除
                DeleteRoom(file.room.room_id);
                continue;
            }

            rooms.Add(file.room);
        }

        return rooms.ToArray();
    }

    private static bool IsExpired(LocalRoomFile file)
    {
        return (NowUnixMs - file.aliveUnixMs) / 1000.0 > RoomTimeoutSeconds;
    }

    private static string GetRoomPath(string roomId)
    {
        return Path.Combine(RoomFolder, roomId + ".json");
    }

    private static LocalRoomFile ReadRoom(string path)
    {
        try
        {
            if (!File.Exists(path))
                return null;

            LocalRoomFile file =
                JsonUtility.FromJson<LocalRoomFile>(File.ReadAllText(path));

            return file != null && file.room != null ? file : null;
        }
        catch (Exception)
        {
            // 他のプロセスが書き込み中など。次の更新で読めればよい
            return null;
        }
    }

    private static void WriteRoom(LocalRoomFile file)
    {
        try
        {
            Directory.CreateDirectory(RoomFolder);
            File.WriteAllText(GetRoomPath(file.room.room_id), JsonUtility.ToJson(file));
        }
        catch (IOException e)
        {
            Debug.LogWarning("ローカルルームの保存に失敗 : " + e.Message);
        }
    }

    //==================================================
    // ステージ（ローカル保存したマップ）
    //==================================================

    /// <summary>
    /// マップエディタでローカル保存したマップと、同梱のテスト用ステージの一覧
    /// </summary>
    public static DungeonListItem[] GetStages()
    {
        List<DungeonListItem> stages = new List<DungeonListItem>();

        foreach (string path in GetStageFiles())
        {
            DungeonMapData data = ReadStage(path);

            if (data == null)
                continue;

            stages.Add(new DungeonListItem
            {
                dungeonId = GetStageId(data, path),
                dungeonName = string.IsNullOrEmpty(data.dungeonName)
                    ? Path.GetFileNameWithoutExtension(path)
                    : data.dungeonName,
                creatorName = "ローカル",
                tag = "NORMAL"
            });
        }

        return stages.ToArray();
    }

    /// <summary>
    /// ステージIDかステージ名で読み込む
    /// </summary>
    public static DungeonMapData LoadStage(string idOrName)
    {
        foreach (string path in GetStageFiles())
        {
            DungeonMapData data = ReadStage(path);

            if (data == null)
                continue;

            if (GetStageId(data, path) == idOrName ||
                data.dungeonName == idOrName ||
                Path.GetFileNameWithoutExtension(path) == idOrName)
            {
                return data;
            }
        }

        return null;
    }

    private static IEnumerable<string> GetStageFiles()
    {
        foreach (string folder in new[] { Application.persistentDataPath, TestStageFolder })
        {
            if (!Directory.Exists(folder))
                continue;

            foreach (string path in Directory.GetFiles(folder, "*.json"))
            {
                yield return path;
            }
        }
    }

    private static string GetStageId(DungeonMapData data, string path)
    {
        return string.IsNullOrEmpty(data.dungeonId)
            ? Path.GetFileNameWithoutExtension(path)
            : data.dungeonId;
    }

    private static DungeonMapData ReadStage(string path)
    {
        try
        {
            DungeonMapData data =
                JsonUtility.FromJson<DungeonMapData>(File.ReadAllText(path));

            // マップ以外のJSONは除外
            return data != null && data.tiles != null && data.tiles.Length > 0 ? data : null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
