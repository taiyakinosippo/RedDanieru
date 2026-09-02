using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class MapManager : MonoBehaviour
{
    [Header("新規マップを生成する（EditorSceneのみON）")]
    [SerializeField] private bool createOnStart = true;

    [Header("Map Size")]
    public int width = 32;
    public int height = 1;
    public int depth = 32;

    [Header("Prefab")]
    public GameObject wallPrefab;

    [Header("床生成設定")]
    [SerializeField] private float floorYOffset = -1f;

    [Header("NavMesh")]
    [SerializeField] private NavMeshSurface navMeshSurface;

    [Header("オブジェクト配置設定")]
    [SerializeField] private float objectYOffset = 0f;

    [SerializeField]
    private PlaceObjectPrefab[] objectPrefabs;

    [System.Serializable]
    public class PlaceObjectPrefab
    {
        public PlaceObjectType type;
        public GameObject prefab;
    }

    private TileType[,,] map;
    private GameObject[,,] wallObjects;
    private GameObject[,,] floorObjects;
    private GameObject[,,] placedObjects;
    private PlaceObjectType[,,] placedObjectTypes;

    [SerializeField] private GameObject floorPrefab;

    void Start()
    {
        if (createOnStart)
        {
            CreateNewMap();
        }
    }

    public void CreateNewMap()
    {
        GenerateMap();
        CreateMap();
    }

    void GenerateMap()
    {
        map = new TileType[width, height, depth];
        wallObjects = new GameObject[width, height, depth];
        floorObjects = new GameObject[width, height, depth];
        placedObjects = new GameObject[width, height, depth];
        placedObjectTypes = new PlaceObjectType[width, height, depth];

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

    void CreateMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    if (map[x, y, z] == TileType.Wall)
                    {
                        GameObject wall = Instantiate(
                            wallPrefab,
                            new Vector3(x, y, z),
                            Quaternion.identity,
                            transform
                        );

                        wallObjects[x, y, z] = wall;

                        WallBlock block =
                            wall.GetComponent<WallBlock>();

                        if (block != null)
                        {
                            block.GridPosition =
                                new Vector3Int(x, y, z);
                        }
                    }
                }
            }
        }
    }

    public void Dig(Vector3Int pos)
    {
        if (!IsInsideMap(pos))
            return;

        if (map[pos.x, pos.y, pos.z] != TileType.Wall)
            return;

        map[pos.x, pos.y, pos.z] = TileType.Floor;

        if (wallObjects[pos.x, pos.y, pos.z] != null)
        {
            Destroy(wallObjects[pos.x, pos.y, pos.z]);

            wallObjects[pos.x, pos.y, pos.z] = null;

            GameObject floor = Instantiate(
                floorPrefab,
                new Vector3(
                    pos.x,
                    pos.y + floorYOffset,
                    pos.z
                ),
                Quaternion.identity,
                transform
            );

            floorObjects[pos.x, pos.y, pos.z] = floor;

            FloorBlock block =
                floor.GetComponent<FloorBlock>();

            if (block != null)
            {
                block.GridPosition = pos;
            }
        }
    }

    public DungeonMapData CreateSaveData()
    {
        DungeonMapData data = new DungeonMapData();

        data.width = width;
        data.height = height;
        data.depth = depth;

        data.tiles = new byte[width * height * depth];

        int index = 0;

        for (int y = 0; y < height; y++)
        {
            for (int z = 0; z < depth; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    data.tiles[index++] =
                        (byte)map[x, y, z];
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    if (placedObjects[x, y, z] != null)
                    {
                        data.objects.Add(
                            new ObjectData()
                            {
                                x = x,
                                y = y,
                                z = z,
                                type =
                                    placedObjectTypes[x, y, z]
                            }
                        );
                    }
                }
            }
        }

        return data;
    }

    public void LoadDungeon(DungeonMapData data)
    {
        width = data.width;
        height = data.height;
        depth = data.depth;

        map =
            new TileType[width, height, depth];

        placedObjectTypes =
            new PlaceObjectType[width, height, depth];

        wallObjects =
            new GameObject[width, height, depth];

        floorObjects =
            new GameObject[width, height, depth];

        placedObjects =
            new GameObject[width, height, depth];

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        int index = 0;

        for (int y = 0; y < height; y++)
        {
            for (int z = 0; z < depth; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    map[x, y, z] =
                        (TileType)data.tiles[index++];

                    if (map[x, y, z] == TileType.Wall)
                    {
                        GameObject wall = Instantiate(
                            wallPrefab,
                            new Vector3(x, y, z),
                            Quaternion.identity,
                            transform
                        );

                        wallObjects[x, y, z] = wall;

                        WallBlock block =
                            wall.GetComponent<WallBlock>();

                        if (block != null)
                        {
                            block.GridPosition =
                                new Vector3Int(x, y, z);
                        }
                    }
                    else if (map[x, y, z] == TileType.Floor)
                    {
                        GameObject floor = Instantiate(
                            floorPrefab,
                            new Vector3(
                                x,
                                y + floorYOffset,
                                z
                            ),
                            Quaternion.identity,
                            transform
                        );

                        floorObjects[x, y, z] = floor;

                        FloorBlock block =
                            floor.GetComponent<FloorBlock>();

                        if (block != null)
                        {
                            block.GridPosition =
                                new Vector3Int(x, y, z);
                        }
                    }
                }
            }
        }

        foreach (ObjectData objData in data.objects)
        {
            Vector3Int pos = new Vector3Int(
                objData.x,
                objData.y,
                objData.z
            );

            PlaceObject(
                pos,
                objData.type
            );
        }

        BuildNavigation();

        Debug.Log("ダンジョン復元完了");
    }

    public void PlaceObject(
        Vector3Int pos,
        PlaceObjectType type)
    {
        if (!IsInsideMap(pos))
            return;

        if (map[pos.x, pos.y, pos.z] != TileType.Floor)
        {
            Debug.Log("床以外にはオブジェクトを配置できません。");
            return;
        }

        if (placedObjects[pos.x, pos.y, pos.z] != null)
            return;

        if (type == PlaceObjectType.Goal &&
            HasGoal())
        {
            Debug.Log(
                "Goalは1つしか配置できません。"
            );

            return;
        }

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
            Debug.LogError(
                type +
                " のPrefabが設定されていません。"
            );

            return;
        }

        Vector3 spawnPosition = new Vector3(
            pos.x,
            pos.y +
            floorYOffset +
            objectYOffset,
            pos.z
        );

        GameObject obj = Instantiate(
            prefab,
            spawnPosition,
            Quaternion.identity,
            transform
        );

        /*
         * Rigidbodyを完全に固定する
         */
        Rigidbody[] rigidbodies =
            obj.GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
            rb.useGravity = false;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.constraints =
                RigidbodyConstraints.FreezeAll;
        }

        /*
         * NavMeshAgentを無効化する
         *
         * 編集中に敵が勝手に動かないようにする。
         */
        NavMeshAgent[] agents =
            obj.GetComponentsInChildren<NavMeshAgent>();

        foreach (NavMeshAgent agent in agents)
        {
            agent.enabled = false;
        }

        /*
         * 配置情報を保存
         */
        placedObjects[
            pos.x,
            pos.y,
            pos.z
        ] = obj;

        placedObjectTypes[
            pos.x,
            pos.y,
            pos.z
        ] = type;

        PlaceObject placeObject =
            obj.GetComponent<PlaceObject>();

        if (placeObject != null)
        {
            placeObject.GridPosition = pos;
        }

        /*
         * 最後にもう一度座標を固定
         */
        obj.transform.position = spawnPosition;
        obj.transform.rotation = Quaternion.identity;
    }

    public void DeleteObject(Vector3Int pos)
    {
        if (!IsInsideMap(pos))
            return;

        if (placedObjects[pos.x, pos.y, pos.z] == null)
            return;

        Destroy(
            placedObjects[
                pos.x,
                pos.y,
                pos.z
            ]
        );

        placedObjects[
            pos.x,
            pos.y,
            pos.z
        ] = null;

        placedObjectTypes[
            pos.x,
            pos.y,
            pos.z
        ] = default;
    }

    public bool HasGoal()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    if (
                        placedObjects[x, y, z] != null &&
                        placedObjectTypes[x, y, z]
                            == PlaceObjectType.Goal
                    )
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public GameObject GetPlacedObject(Vector3Int pos)
    {
        if (!IsInsideMap(pos))
            return null;

        return placedObjects[
            pos.x,
            pos.y,
            pos.z
        ];
    }

    /*
     * 床だけを対象にNavMeshを作成する
     */
    public void BuildNavigation()
    {
        if (navMeshSurface == null)
        {
            Debug.LogError(
                "NavMeshSurfaceが設定されていません。"
            );

            return;
        }

        /*
         * 敵を無効化しない。
         *
         * SetActive(false) → true をすると
         * RigidbodyやAIが再開して、
         * オブジェクトが押し出される原因になる。
         */
        navMeshSurface.BuildNavMesh();

        Debug.Log("床のNavMeshを再生成しました。");
    }

    public void EnableEnemyMovement()
    {
        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            NavMeshAgent[] agents =
                enemy.GetComponentsInChildren<NavMeshAgent>();

            foreach (NavMeshAgent agent in agents)
            {
                if (!agent.enabled)
                {
                    agent.enabled = true;
                }
            }
        }
    }

    private bool IsInsideMap(Vector3Int pos)
    {
        return
            pos.x >= 0 &&
            pos.x < width &&
            pos.y >= 0 &&
            pos.y < height &&
            pos.z >= 0 &&
            pos.z < depth;
    }
}