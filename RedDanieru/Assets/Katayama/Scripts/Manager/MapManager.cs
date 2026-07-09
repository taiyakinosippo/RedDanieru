using UnityEngine;

public class MapManager : MonoBehaviour
{
    // EditorSceneでのみ新規マップを生成する
    [Header("新規マップを生成する（EditorSceneのみON）")]
    [SerializeField] private bool createOnStart = true;

    // マップサイズ
    [Header("Map Size")]
    public int width = 32;
    public int height = 1;
    public int depth = 32;

    // 壁プレハブ
    [Header("Prefab")]
    public GameObject wallPrefab;

    // マップデータ
    private TileType[,,] map;

    // 壁オブジェクトを管理
    private GameObject[,,] wallObjects;

    void Start()
    {
        // EditorSceneのみ新規マップを生成
        if (createOnStart)
        {
            CreateNewMap();
        }
    }

    /// 新しいマップを作成する
    public void CreateNewMap()
    {
        GenerateMap();
        CreateMap();
    }

    /// マップデータを初期化する
    void GenerateMap()
    {
        map = new TileType[width, height, depth];
        wallObjects = new GameObject[width, height, depth];

        // 全て壁で初期化
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    map[x, y, z] = TileType.Wall;
                }
            }
        }
    }

    /// マップデータから壁を生成する
    void CreateMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    // 壁のみ生成
                    if (map[x, y, z] == TileType.Wall)
                    {
                        GameObject wall = Instantiate(
                            wallPrefab,
                            new Vector3(x, y, z),
                            Quaternion.identity,
                            transform);

                        // 生成した壁を保存
                        wallObjects[x, y, z] = wall;

                        // 壁にグリッド座標を設定
                        WallBlock block = wall.GetComponent<WallBlock>();
                        block.GridPosition = new Vector3Int(x, y, z);
                    }
                }
            }
        }
    }

    /// 指定した壁を掘る
    public void Dig(Vector3Int pos)
    {
        // 壁以外は掘らない
        if (map[pos.x, pos.y, pos.z] != TileType.Wall)
            return;

        // 床へ変更
        map[pos.x, pos.y, pos.z] = TileType.Floor;

        // 壁オブジェクトを削除
        if (wallObjects[pos.x, pos.y, pos.z] != null)
        {
            Destroy(wallObjects[pos.x, pos.y, pos.z]);
            wallObjects[pos.x, pos.y, pos.z] = null;
        }
    }

    /// 保存用データを作成する
    public DungeonMapData CreateSaveData()
    {
        DungeonMapData data = new DungeonMapData();

        // マップサイズを保存
        data.width = width;
        data.height = height;
        data.depth = depth;

        // タイル情報を1次元配列へ変換
        data.tiles = new byte[width * height * depth];

        int index = 0;

        for (int y = 0; y < height; y++)
        {
            for (int z = 0; z < depth; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    data.tiles[index++] = (byte)map[x, y, z];
                }
            }
        }

        return data;
    }

    /// 保存データからダンジョンを読み込む
    public void LoadDungeon(DungeonMapData data)
    {
        // マップサイズを取得
        width = data.width;
        height = data.height;
        depth = data.depth;

        // 配列を作成
        map = new TileType[width, height, depth];
        wallObjects = new GameObject[width, height, depth];

        // 現在の壁を削除
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        int index = 0;

        // 保存データからマップを復元
        for (int y = 0; y < height; y++)
        {
            for (int z = 0; z < depth; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    map[x, y, z] = (TileType)data.tiles[index++];

                    // 壁のみ生成
                    if (map[x, y, z] == TileType.Wall)
                    {
                        GameObject wall = Instantiate(
                            wallPrefab,
                            new Vector3(x, y, z),
                            Quaternion.identity,
                            transform);

                        wallObjects[x, y, z] = wall;

                        WallBlock block = wall.GetComponent<WallBlock>();
                        block.GridPosition = new Vector3Int(x, y, z);
                    }
                }
            }
        }
    }
}