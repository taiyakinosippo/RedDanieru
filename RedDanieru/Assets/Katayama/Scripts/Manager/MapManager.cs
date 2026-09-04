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

    [SerializeField] private GameObject floorPrefab;

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

    //==================================================
    // リスポーン
    //==================================================

    [Header("リスポーン")]
    [SerializeField] private GameObject respawnPointPrefab;
    [SerializeField] private GameObject respawnAreaPrefab;

    [Header("リスポーン初期位置")]
    [SerializeField]
    private Vector3 defaultRespawnPointPosition =
        new Vector3(0f, 0f, 0f);

    [SerializeField]
    private Vector3 defaultRespawnAreaPosition =
        new Vector3(0f, -1f, 0f);

    [Header("リスポーンエリア初期サイズ")]
    [SerializeField]
    private Vector3 defaultRespawnAreaScale =
        new Vector3(3f, 1f, 3f);

    private GameObject respawnPointObject;
    private GameObject respawnAreaObject;


    private TileType[,,] map;

    private GameObject[,,] wallObjects;
    private GameObject[,,] floorObjects;
    private GameObject[,,] placedObjects;

    private PlaceObjectType[,,] placedObjectTypes;


    //==================================================
    // Start
    //==================================================

    private void Start()
    {
        if (createOnStart)
        {
            CreateNewMap();
        }
    }


    //==================================================
    // 新規マップ作成
    //==================================================

    public void CreateNewMap()
    {
        GenerateMap();

        CreateMap();

        // 新規マップでは最初から配置
        CreateDefaultRespawnObjects();
    }


    //==================================================
    // 配列生成
    //==================================================

    private void GenerateMap()
    {
        map =
            new TileType[width, height, depth];

        wallObjects =
            new GameObject[width, height, depth];

        floorObjects =
            new GameObject[width, height, depth];

        placedObjects =
            new GameObject[width, height, depth];

        placedObjectTypes =
            new PlaceObjectType[width, height, depth];


        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    map[x, y, z] =
                        TileType.Wall;
                }
            }
        }
    }


    //==================================================
    // マップ生成
    //==================================================

    private void CreateMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    if (
                        map[x, y, z]
                        != TileType.Wall
                    )
                    {
                        continue;
                    }


                    GameObject wall =
                        Instantiate(
                            wallPrefab,
                            new Vector3(
                                x,
                                y,
                                z
                            ),
                            Quaternion.identity,
                            transform
                        );


                    wallObjects[
                        x,
                        y,
                        z
                    ] = wall;


                    WallBlock block =
                        wall.GetComponent<WallBlock>();


                    if (block != null)
                    {
                        block.GridPosition =
                            new Vector3Int(
                                x,
                                y,
                                z
                            );
                    }
                }
            }
        }
    }


    //==================================================
    // 初期リスポーン生成
    //==================================================

    private void CreateDefaultRespawnObjects()
    {
        // リスポーンポイント
        if (respawnPointPrefab != null)
        {
            respawnPointObject =
                Instantiate(
                    respawnPointPrefab,
                    defaultRespawnPointPosition,
                    Quaternion.identity,
                    transform
                );

            respawnPointObject.name =
                "RespawnPoint";
        }
        else
        {
            Debug.LogError(
                "Respawn Point Prefabが設定されていません。"
            );
        }


        // リスポーンエリア
        if (respawnAreaPrefab != null)
        {
            respawnAreaObject =
                Instantiate(
                    respawnAreaPrefab,
                    defaultRespawnAreaPosition,
                    Quaternion.identity,
                    transform
                );

            respawnAreaObject.name =
                "RespawnArea";


            respawnAreaObject
                .transform
                .localScale =
                defaultRespawnAreaScale;
        }
        else
        {
            Debug.LogError(
                "Respawn Area Prefabが設定されていません。"
            );
        }
    }


    //==================================================
    // 掘る
    //==================================================

    public void Dig(Vector3Int pos)
    {
        if (!IsInsideMap(pos))
            return;


        if (
            map[pos.x, pos.y, pos.z]
            != TileType.Wall
        )
        {
            return;
        }


        map[pos.x, pos.y, pos.z] =
            TileType.Floor;


        if (
            wallObjects[
                pos.x,
                pos.y,
                pos.z
            ] != null
        )
        {
            Destroy(
                wallObjects[
                    pos.x,
                    pos.y,
                    pos.z
                ]
            );


            wallObjects[
                pos.x,
                pos.y,
                pos.z
            ] = null;


            GameObject floor =
                Instantiate(
                    floorPrefab,
                    new Vector3(
                        pos.x,
                        pos.y +
                        floorYOffset,
                        pos.z
                    ),
                    Quaternion.identity,
                    transform
                );


            floorObjects[
                pos.x,
                pos.y,
                pos.z
            ] = floor;


            FloorBlock block =
                floor.GetComponent<FloorBlock>();


            if (block != null)
            {
                block.GridPosition =
                    pos;
            }
        }
    }


    //==================================================
    // セーブデータ作成
    //==================================================

    public DungeonMapData CreateSaveData()
    {
        DungeonMapData data =
            new DungeonMapData();


        data.width = width;
        data.height = height;
        data.depth = depth;


        //==================================================
        // 壁・床
        //==================================================

        data.tiles =
            new byte[
                width *
                height *
                depth
            ];


        int index = 0;


        for (int y = 0; y < height; y++)
        {
            for (int z = 0; z < depth; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    data.tiles[index++] =
                        (byte)map[
                            x,
                            y,
                            z
                        ];
                }
            }
        }


        //==================================================
        // 配置Object
        //==================================================

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    if (
                        placedObjects[
                            x,
                            y,
                            z
                        ] == null
                    )
                    {
                        continue;
                    }


                    data.objects.Add(
                        new ObjectData()
                        {
                            x = x,
                            y = y,
                            z = z,

                            type =
                                placedObjectTypes[
                                    x,
                                    y,
                                    z
                                ]
                        }
                    );
                }
            }
        }


        //==================================================
        // リスポーンポイント保存
        //==================================================

        if (respawnPointObject != null)
        {
            data.hasRespawnPoint = true;


            Vector3 position =
                respawnPointObject
                    .transform
                    .position;


            data.spawnPointX =
                position.x;

            data.spawnPointY =
                position.y;

            data.spawnPointZ =
                position.z;


            data.spawnPointRotY =
                respawnPointObject
                    .transform
                    .eulerAngles
                    .y;
        }
        else
        {
            data.hasRespawnPoint = false;
        }


        //==================================================
        // リスポーンエリア保存
        //==================================================

        if (respawnAreaObject != null)
        {
            data.hasRespawnArea = true;


            Vector3 position =
                respawnAreaObject
                    .transform
                    .position;


            data.respawnAreaX =
                position.x;

            data.respawnAreaY =
                position.y;

            data.respawnAreaZ =
                position.z;


            Vector3 scale =
                respawnAreaObject
                    .transform
                    .localScale;


            data.respawnAreaScaleX =
                scale.x;

            data.respawnAreaScaleY =
                scale.y;

            data.respawnAreaScaleZ =
                scale.z;
        }
        else
        {
            data.hasRespawnArea = false;
        }


        return data;
    }


    //==================================================
    // ロード
    //==================================================

    public void LoadDungeon(
        DungeonMapData data)
    {
        width = data.width;
        height = data.height;
        depth = data.depth;


        map =
            new TileType[
                width,
                height,
                depth
            ];


        placedObjectTypes =
            new PlaceObjectType[
                width,
                height,
                depth
            ];


        wallObjects =
            new GameObject[
                width,
                height,
                depth
            ];


        floorObjects =
            new GameObject[
                width,
                height,
                depth
            ];


        placedObjects =
            new GameObject[
                width,
                height,
                depth
            ];


        // 現在のマップを削除
        for (
            int i = transform.childCount - 1;
            i >= 0;
            i--
        )
        {
            Destroy(
                transform.GetChild(i).gameObject
            );
        }


        respawnPointObject = null;
        respawnAreaObject = null;


        int index = 0;


        //==================================================
        // 壁・床復元
        //==================================================

        for (int y = 0; y < height; y++)
        {
            for (int z = 0; z < depth; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    map[x, y, z] =
                        (TileType)data.tiles[index++];


                    // 壁
                    if (
                        map[x, y, z]
                        == TileType.Wall
                    )
                    {
                        GameObject wall =
                            Instantiate(
                                wallPrefab,
                                new Vector3(
                                    x,
                                    y,
                                    z
                                ),
                                Quaternion.identity,
                                transform
                            );


                        wallObjects[
                            x,
                            y,
                            z
                        ] = wall;


                        WallBlock block =
                            wall.GetComponent<WallBlock>();


                        if (block != null)
                        {
                            block.GridPosition =
                                new Vector3Int(
                                    x,
                                    y,
                                    z
                                );
                        }
                    }


                    // 床
                    else if (
                        map[x, y, z]
                        == TileType.Floor
                    )
                    {
                        GameObject floor =
                            Instantiate(
                                floorPrefab,
                                new Vector3(
                                    x,
                                    y +
                                    floorYOffset,
                                    z
                                ),
                                Quaternion.identity,
                                transform
                            );


                        floorObjects[
                            x,
                            y,
                            z
                        ] = floor;


                        FloorBlock block =
                            floor.GetComponent<FloorBlock>();


                        if (block != null)
                        {
                            block.GridPosition =
                                new Vector3Int(
                                    x,
                                    y,
                                    z
                                );
                        }
                    }
                }
            }
        }


        //==================================================
        // 床が完成してからNavMeshを作る
        //==================================================

        BuildNavigation();


        //==================================================
        // 保存したObjectを復元
        //==================================================

        if (data.objects != null)
        {
            foreach (
                ObjectData objData
                in data.objects
            )
            {
                Vector3Int pos =
                    new Vector3Int(
                        objData.x,
                        objData.y,
                        objData.z
                    );


                PlaceObject(
                    pos,
                    objData.type
                );
            }
        }


        //==================================================
        // 敵をNavMesh上に配置して動かす
        //==================================================

        EnableEnemyMovement();


        //==================================================
        // リスポーン復元
        //==================================================

        CreateRespawnObjects(data);


        Debug.Log(
            "ダンジョン復元完了"
        );
    }


    //==================================================
    // リスポーン復元
    //==================================================

    private void CreateRespawnObjects(
        DungeonMapData data)
    {
        //==================================================
        // リスポーンポイント
        //==================================================

        if (respawnPointPrefab != null)
        {
            Vector3 position;
            Quaternion rotation;


            if (data.hasRespawnPoint)
            {
                position =
                    new Vector3(
                        data.spawnPointX,
                        data.spawnPointY,
                        data.spawnPointZ
                    );


                rotation =
                    Quaternion.Euler(
                        0f,
                        data.spawnPointRotY,
                        0f
                    );
            }
            else
            {
                position =
                    defaultRespawnPointPosition;


                rotation =
                    Quaternion.identity;
            }


            respawnPointObject =
                Instantiate(
                    respawnPointPrefab,
                    position,
                    rotation,
                    transform
                );


            respawnPointObject.name =
                "RespawnPoint";
        }


        //==================================================
        // リスポーンエリア
        //==================================================

        if (respawnAreaPrefab != null)
        {
            Vector3 position;
            Vector3 scale;


            if (data.hasRespawnArea)
            {
                position =
                    new Vector3(
                        data.respawnAreaX,
                        data.respawnAreaY,
                        data.respawnAreaZ
                    );


                scale =
                    new Vector3(
                        data.respawnAreaScaleX,
                        data.respawnAreaScaleY,
                        data.respawnAreaScaleZ
                    );
            }
            else
            {
                position =
                    defaultRespawnAreaPosition;


                scale =
                    defaultRespawnAreaScale;
            }


            respawnAreaObject =
                Instantiate(
                    respawnAreaPrefab,
                    position,
                    Quaternion.identity,
                    transform
                );


            respawnAreaObject.name =
                "RespawnArea";


            respawnAreaObject
                .transform
                .localScale =
                scale;
        }
    }


    //==================================================
    // Object配置
    //==================================================

    public void PlaceObject(
        Vector3Int pos,
        PlaceObjectType type)
    {
        if (!IsInsideMap(pos))
            return;


        // 床以外には配置できない
        if (
            map[pos.x, pos.y, pos.z]
            != TileType.Floor
        )
        {
            Debug.Log(
                "床以外にはオブジェクトを配置できません。"
            );

            return;
        }


        // すでにオブジェクトがある
        if (
            placedObjects[
                pos.x,
                pos.y,
                pos.z
            ] != null
        )
        {
            return;
        }


        // Goalは1個だけ
        if (
            type == PlaceObjectType.Goal &&
            HasGoal()
        )
        {
            Debug.Log(
                "Goalは1つしか配置できません。"
            );

            return;
        }


        GameObject prefab = null;


        foreach (
            var data
            in objectPrefabs
        )
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


        Vector3 spawnPosition =
            new Vector3(
                pos.x,
                pos.y +
                floorYOffset +
                objectYOffset,
                pos.z
            );


        GameObject obj =
            Instantiate(
                prefab,
                spawnPosition,
                Quaternion.identity,
                transform
            );


        //==================================================
        // Rigidbodyを固定
        //==================================================

        Rigidbody[] rigidbodies =
            obj.GetComponentsInChildren<Rigidbody>();


        foreach (
            Rigidbody rb
            in rigidbodies
        )
        {
            rb.isKinematic = true;
            rb.useGravity = false;

            rb.linearVelocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;

            rb.constraints =
                RigidbodyConstraints.FreezeAll;
        }


        //==================================================
        // NavMeshAgentは一旦無効
        //==================================================

        NavMeshAgent[] agents =
            obj.GetComponentsInChildren<NavMeshAgent>();


        foreach (
            NavMeshAgent agent
            in agents
        )
        {
            agent.enabled = false;
        }


        //==================================================
        // 配置情報保存
        //==================================================

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
            placeObject.GridPosition =
                pos;
        }


        obj.transform.position =
            spawnPosition;

        obj.transform.rotation =
            Quaternion.identity;
    }


    //==================================================
    // Object削除
    //==================================================

    public void DeleteObject(
        Vector3Int pos)
    {
        if (!IsInsideMap(pos))
            return;


        if (
            placedObjects[
                pos.x,
                pos.y,
                pos.z
            ] == null
        )
        {
            return;
        }


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


    //==================================================
    // Goal確認
    //==================================================

    public bool HasGoal()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    if (
                        placedObjects[
                            x,
                            y,
                            z
                        ] != null &&
                        placedObjectTypes[
                            x,
                            y,
                            z
                        ]
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


    //==================================================
    // Object取得
    //==================================================

    public GameObject GetPlacedObject(
        Vector3Int pos)
    {
        if (!IsInsideMap(pos))
            return null;


        return placedObjects[
            pos.x,
            pos.y,
            pos.z
        ];
    }


    //==================================================
    // NavMesh生成
    //==================================================

    public void BuildNavigation()
    {
        if (navMeshSurface == null)
        {
            Debug.LogError(
                "NavMeshSurfaceが設定されていません。"
            );

            return;
        }


        navMeshSurface.BuildNavMesh();


        Debug.Log(
            "床のNavMeshを再生成しました。"
        );
    }


    //==================================================
    // 敵のNavMesh移動を有効化
    //==================================================

    public void EnableEnemyMovement()
    {
        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag(
                "Enemy"
            );


        foreach (
            GameObject enemy
            in enemies
        )
        {
            NavMeshAgent[] agents =
                enemy.GetComponentsInChildren<
                    NavMeshAgent
                >();


            foreach (
                NavMeshAgent agent
                in agents
            )
            {
                NavMeshHit hit;


                bool found =
                    NavMesh.SamplePosition(
                        enemy.transform.position,
                        out hit,
                        2f,
                        NavMesh.AllAreas
                    );


                if (found)
                {
                    // NavMesh上へ移動
                    enemy.transform.position =
                        hit.position;


                    // Agentを有効化
                    agent.enabled = true;


                    // Agent自身もNavMesh上へ移動
                    agent.Warp(
                        hit.position
                    );
                }
                else
                {
                    Debug.LogWarning(
                        "敵の足元にNavMeshがありません : "
                        + enemy.name
                    );
                }
            }
        }
    }


    //==================================================
    // マップ範囲確認
    //==================================================

    private bool IsInsideMap(
        Vector3Int pos)
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