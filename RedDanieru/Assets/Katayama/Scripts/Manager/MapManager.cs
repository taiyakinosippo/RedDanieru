using UnityEngine;
//using static MapManager;
//using static TMPro.Examples.TMP_ExampleScript_01;


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

    // 床を生成する高さオフセット
    [Header("床生成設定")]
    [SerializeField] private float floorYOffset = -1f;

    // オブジェクトを生成する高さオフセット
    [Header("オブジェクト配置設定")]
    [SerializeField] private float objectYOffset = 0f;

    [SerializeField]
    private PlaceObjectPrefab[] objectPrefabs;

    // マップデータ
    private TileType[,,] map;

    // 壁オブジェクトを管理
    private GameObject[,,] wallObjects;

    [SerializeField] private GameObject floorPrefab;

    // 床オブジェクト
    private GameObject[,,] floorObjects;

    // 配置したオブジェクト
    private GameObject[,,] placedObjects;

    private PlaceObjectType[,,] placedObjectTypes;

    [System.Serializable]
    public class PlaceObjectPrefab
    {
        public PlaceObjectType type;
        public GameObject prefab;
    }

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
        floorObjects = new GameObject[width, height, depth];
        placedObjects = new GameObject[width, height, depth];
        placedObjectTypes = new PlaceObjectType[width, height, depth];

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

            // 床を生成
            GameObject floor = Instantiate(
                floorPrefab,
                new Vector3(
                    pos.x,
                    pos.y + floorYOffset,
                    pos.z),
                Quaternion.identity,
                transform);

            // 床を保存
            floorObjects[pos.x, pos.y, pos.z] = floor;

            // 床に座標を設定
            FloorBlock block = floor.GetComponent<FloorBlock>();
            block.GridPosition = pos;
        }
    }

    /// 保存用データを作成する
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
                    // 壁・床情報を保存
                    data.tiles[index++] = (byte)map[x, y, z];
                }
            }
        }


        // 配置オブジェクト情報を保存
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    // オブジェクトが存在する場合
                    if (placedObjects[x, y, z] != null)
                    {
                        data.objects.Add
                        (
                            new ObjectData()
                            {
                                x = x,
                                y = y,
                                z = z,
                                type = placedObjectTypes[x, y, z]
                            }
                        );
                    }
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
        placedObjectTypes = new PlaceObjectType[width, height, depth];
        wallObjects = new GameObject[width, height, depth];
        floorObjects = new GameObject[width, height, depth];
        placedObjects = new GameObject[width, height, depth];
        

        // 現在のオブジェクトを削除
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


                    // 壁生成
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


                    // 床生成
                    else if (map[x, y, z] == TileType.Floor)
                    {
                        GameObject floor = Instantiate(
                            floorPrefab,
                            new Vector3(
                                x,
                                y + floorYOffset,
                                z),
                            Quaternion.identity,
                            transform);


                        floorObjects[x, y, z] = floor;


                        FloorBlock block = floor.GetComponent<FloorBlock>();
                        block.GridPosition = new Vector3Int(x, y, z);
                    }
                }
            }
        }


        // 配置オブジェクト復元
        foreach (ObjectData objData in data.objects)
        {
            Vector3Int pos = new Vector3Int(
                objData.x,
                objData.y,
                objData.z
            );

            PlaceObject(pos, objData.type);
        }


        Debug.Log("ダンジョン復元完了");
    }

    public void PlaceObject(Vector3Int pos, PlaceObjectType type)
    {
        // 床以外には配置しない
        if (map[pos.x, pos.y, pos.z] != TileType.Floor)
            return;

        // 既に配置済み
        if (placedObjects[pos.x, pos.y, pos.z] != null)
            return;

        GameObject prefab = null;

        foreach (var data in objectPrefabs)
        {
            if (data.type == type)
            {
                prefab = data.prefab;
                break;
            }
        }

        if (prefab == null)
        {
            Debug.LogError(type + " のPrefabが設定されていません");
            return;
        }

        GameObject obj = Instantiate(
            prefab,
            new Vector3(
                pos.x,
                pos.y + floorYOffset + objectYOffset,
                pos.z),
            Quaternion.identity,
            transform);

        placedObjects[pos.x, pos.y, pos.z] = obj;
        placedObjectTypes[pos.x, pos.y, pos.z] = type;
    }
}