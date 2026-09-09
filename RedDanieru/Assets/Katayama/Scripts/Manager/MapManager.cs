using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class MapManager : MonoBehaviour
{
    //==================================================
    // 新規マップ
    //==================================================

    [Header("新規マップを生成する（EditorSceneのみON）")]
    [SerializeField] private bool createOnStart = true;

    //==================================================
    // Map Size
    //==================================================

    [Header("Map Size")]
    public int width = 32;
    public int height = 1;
    public int depth = 32;

    //==================================================
    // 壁サイズ設定
    //==================================================

    [Header("壁サイズ設定")]

    [SerializeField]
    [Min(0.01f)]
    private float wallSize = 1f;

    [SerializeField]
    [Min(0.01f)]
    private float wallSpacing = 1f;

    //==================================================
    // 床サイズ設定
    //==================================================

    [Header("床サイズ設定")]

    [SerializeField]
    [Min(0.01f)]
    private float floorSize = 1f;

    [SerializeField]
    [Min(0.01f)]
    private float floorSpacing = 1f;

    [SerializeField]
    private float floorYOffset = -1f;

    //==================================================
    // 敵配置間隔設定
    //==================================================

    [Header("敵配置設定")]

    [SerializeField]
    [Min(0.01f)]
    private float enemySpacing = 1f;

    //==================================================
    // 設置オブジェクトサイズ設定
    //==================================================

    [Header("設置オブジェクトサイズ設定")]

    [SerializeField]
    [Min(0.01f)]
    private float objectSize = 1f;

    [SerializeField]
    [Min(0.01f)]
    private float objectSpacing = 1f;

    [SerializeField]
    private float objectYOffset = 0f;

    //==================================================
    // Prefab
    //==================================================

    [Header("Prefab")]

    public GameObject wallPrefab;

    [SerializeField]
    private GameObject floorPrefab;

    [SerializeField]
    private PlaceObjectPrefab[] objectPrefabs;

    [System.Serializable]
    public class PlaceObjectPrefab
    {
        public PlaceObjectType type;
        public GameObject prefab;
    }

    //==================================================
    // NavMesh
    //==================================================

    [Header("NavMesh")]

    [SerializeField]
    private NavMeshSurface navMeshSurface;

    //==================================================
    // カメラ
    //==================================================

    [Header("カメラ設定")]

    [SerializeField]
    private Camera mapCamera;

    [Tooltip("32×32マップ時のカメラ位置")]
    [SerializeField]
    private Vector3 baseCameraPosition =
        new Vector3(
            15.5f,
            35f,
            18.5f
        );

    [Tooltip("カメラ角度")]
    [SerializeField]
    private Vector3 cameraRotation =
        new Vector3(
            90f,
            0f,
            0f
        );

    [Tooltip("32×32マップ時のOrthographic Size")]
    [SerializeField]
    private float baseOrthographicSize = 20f;

    [Tooltip("基準となるマップサイズ")]
    [SerializeField]
    private float baseMapSize = 31f;

    //==================================================
    // リスポーン
    //==================================================

    [Header("リスポーン")]

    [SerializeField]
    private GameObject respawnPointPrefab;

    [SerializeField]
    private GameObject respawnAreaPrefab;

    [Header("リスポーン初期位置")]

    [SerializeField]
    private Vector3 defaultRespawnPointPosition =
        new Vector3(
            0f,
            0f,
            0f
        );

    [SerializeField]
    private Vector3 defaultRespawnAreaPosition =
        new Vector3(
            0f,
            -1f,
            0f
        );

    [Header("リスポーンエリア初期サイズ")]

    [SerializeField]
    private Vector3 defaultRespawnAreaScale =
        new Vector3(
            3f,
            1f,
            3f
        );

    private GameObject respawnPointObject;
    private GameObject respawnAreaObject;

    //==================================================
    // マップデータ
    //==================================================

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
    // 壁ワールド座標
    //==================================================

    private Vector3 GetWallWorldPosition(Vector3Int pos)
    {
        return new Vector3(
            pos.x * wallSpacing,
            pos.y * wallSpacing,
            pos.z * wallSpacing
        );
    }

    //==================================================
    // 床ワールド座標
    //==================================================

    private Vector3 GetFloorWorldPosition(Vector3Int pos)
    {
        return new Vector3(
            pos.x * floorSpacing,
            pos.y * floorSpacing + floorYOffset,
            pos.z * floorSpacing
        );
    }

    //==================================================
    // 敵ワールド座標
    //==================================================

    private Vector3 GetEnemyWorldPosition(Vector3Int pos)
    {
        return new Vector3(
            pos.x * enemySpacing,
            pos.y * enemySpacing + floorYOffset,
            pos.z * enemySpacing
        );
    }

    //==================================================
    // オブジェクトワールド座標
    //==================================================

    private Vector3 GetObjectWorldPosition(Vector3Int pos)
    {
        return new Vector3(
            pos.x * objectSpacing,
            pos.y * objectSpacing
                + floorYOffset
                + objectYOffset,
            pos.z * objectSpacing
        );
    }

    //==================================================
    // 壁サイズ
    //==================================================

    private void SetWallScale(GameObject obj)
    {
        if (obj == null)
            return;

        obj.transform.localScale *= wallSize;
    }

    //==================================================
    // 床サイズ
    //==================================================

    private void SetFloorScale(GameObject obj)
    {
        if (obj == null)
            return;

        obj.transform.localScale *= floorSize;
    }

    //==================================================
    // オブジェクトサイズ
    //==================================================

    private void SetObjectScale(GameObject obj)
    {
        if (obj == null)
            return;

        obj.transform.localScale *= objectSize;
    }

    //==================================================
    // 新規マップ作成
    //==================================================

    public void CreateNewMap()
    {
        GenerateMap();

        CreateMap();

        CreateDefaultRespawnObjects();

        AdjustCamera();
    }

    //==================================================
    // マップ配列生成
    //==================================================

    private void GenerateMap()
    {
        map =
            new TileType[
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

        placedObjectTypes =
            new PlaceObjectType[
                width,
                height,
                depth
            ];

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
                    if (map[x, y, z] != TileType.Wall)
                        continue;

                    Vector3Int pos =
                        new Vector3Int(x, y, z);

                    GameObject wall =
                        Instantiate(
                            wallPrefab,
                            GetWallWorldPosition(pos),
                            Quaternion.identity,
                            transform
                        );

                    SetWallScale(wall);

                    wallObjects[x, y, z] = wall;

                    WallBlock block =
                        wall.GetComponent<WallBlock>();

                    if (block != null)
                    {
                        block.GridPosition = pos;
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

            respawnAreaObject.transform.localScale =
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
    // 一律3×3で掘削
    //==================================================

    public void Dig(Vector3Int pos)
    {
        if (!IsInsideMap(pos))
            return;

        //==================================================
        // 中心マスを基準に3×3を掘る
        //==================================================

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector3Int digPos =
                    new Vector3Int(
                        pos.x + x,
                        pos.y,
                        pos.z + z
                    );

                DigSingleTile(digPos);
            }
        }
    }

    //==================================================
    // 1マスだけ掘る
    //==================================================

    private void DigSingleTile(Vector3Int pos)
    {
        // マップ外なら何もしない
        if (!IsInsideMap(pos))
            return;

        // 壁以外は掘らない
        if (map[pos.x, pos.y, pos.z] != TileType.Wall)
            return;

        //==================================================
        // 壁 → 床
        //==================================================

        map[pos.x, pos.y, pos.z] =
            TileType.Floor;

        //==================================================
        // 壁オブジェクト削除
        //==================================================

        if (wallObjects[pos.x, pos.y, pos.z] != null)
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
        }

        //==================================================
        // 床生成
        //==================================================

        GameObject floor =
            Instantiate(
                floorPrefab,
                GetFloorWorldPosition(pos),
                Quaternion.identity,
                transform
            );

        SetFloorScale(floor);

        floorObjects[
            pos.x,
            pos.y,
            pos.z
        ] = floor;

        //==================================================
        // GridPosition設定
        //==================================================

        FloorBlock block =
            floor.GetComponent<FloorBlock>();

        if (block != null)
        {
            block.GridPosition = pos;
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
        // リスポーンポイント
        //==================================================

        if (respawnPointObject != null)
        {
            data.hasRespawnPoint = true;

            Vector3 position =
                respawnPointObject.transform.position;

            data.spawnPointX = position.x;
            data.spawnPointY = position.y;
            data.spawnPointZ = position.z;

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
        // リスポーンエリア
        //==================================================

        if (respawnAreaObject != null)
        {
            data.hasRespawnArea = true;

            Vector3 position =
                respawnAreaObject.transform.position;

            data.respawnAreaX = position.x;
            data.respawnAreaY = position.y;
            data.respawnAreaZ = position.z;

            Vector3 scale =
                respawnAreaObject.transform.localScale;

            data.respawnAreaScaleX = scale.x;
            data.respawnAreaScaleY = scale.y;
            data.respawnAreaScaleZ = scale.z;
        }
        else
        {
            data.hasRespawnArea = false;
        }

        return data;
    }

    //==================================================
    // ダンジョンロード
    //==================================================

    public void LoadDungeon(
        DungeonMapData data,
        bool enableEnemyMovement = true)
    {
        if (data == null)
        {
            Debug.LogError(
                "DungeonMapDataがnullです。"
            );

            return;
        }

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

        //==================================================
        // 敵停止
        //==================================================

        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag(
                "Enemy"
            );

        foreach (GameObject enemy in enemies)
        {
            NavMeshAgent[] agents =
                enemy.GetComponentsInChildren<
                    NavMeshAgent
                >();

            foreach (NavMeshAgent agent in agents)
            {
                if (!agent.enabled)
                    continue;

                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
                agent.enabled = false;
            }
        }

        //==================================================
        // 現在のマップ削除
        //==================================================

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

        //==================================================
        // 壁・床復元
        //==================================================

        int index = 0;

        for (int y = 0; y < height; y++)
        {
            for (int z = 0; z < depth; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    map[x, y, z] =
                        (TileType)data.tiles[index++];

                    Vector3Int pos =
                        new Vector3Int(
                            x,
                            y,
                            z
                        );

                    //==================================================
                    // 壁
                    //==================================================

                    if (
                        map[x, y, z]
                        == TileType.Wall
                    )
                    {
                        GameObject wall =
                            Instantiate(
                                wallPrefab,
                                GetWallWorldPosition(pos),
                                Quaternion.identity,
                                transform
                            );

                        SetWallScale(wall);

                        wallObjects[
                            x,
                            y,
                            z
                        ] = wall;

                        WallBlock block =
                            wall.GetComponent<WallBlock>();

                        if (block != null)
                        {
                            block.GridPosition = pos;
                        }
                    }

                    //==================================================
                    // 床
                    //==================================================

                    else if (
                        map[x, y, z]
                        == TileType.Floor
                    )
                    {
                        GameObject floor =
                            Instantiate(
                                floorPrefab,
                                GetFloorWorldPosition(pos),
                                Quaternion.identity,
                                transform
                            );

                        SetFloorScale(floor);

                        floorObjects[
                            x,
                            y,
                            z
                        ] = floor;

                        FloorBlock block =
                            floor.GetComponent<FloorBlock>();

                        if (block != null)
                        {
                            block.GridPosition = pos;
                        }
                    }
                }
            }
        }

        //==================================================
        // NavMesh生成
        //==================================================

        BuildNavigation();

        //==================================================
        // Object復元
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
        // 敵移動開始
        //==================================================

        if (enableEnemyMovement)
        {
            EnableEnemyMovement();
        }

        //==================================================
        // リスポーン復元
        //==================================================

        CreateRespawnObjects(data);

        //==================================================
        // カメラ調整
        //==================================================

        AdjustCamera();

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

            respawnAreaObject.transform.localScale =
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

        //==================================================
        // 床以外には配置不可
        //==================================================

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

        //==================================================
        // すでにObjectがある
        //==================================================

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

        //==================================================
        // Goalは1つ
        //==================================================

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

        //==================================================
        // Prefab検索
        //==================================================

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

        //==================================================
        // 配置位置
        //==================================================

        Vector3 spawnPosition;

        if (type == PlaceObjectType.Enemy)
        {
            spawnPosition =
                GetEnemyWorldPosition(pos);
        }
        else
        {
            spawnPosition =
                GetObjectWorldPosition(pos);
        }

        //==================================================
        // 生成
        //==================================================

        GameObject obj =
            Instantiate(
                prefab,
                spawnPosition,
                Quaternion.identity,
                transform
            );

        //==================================================
        // サイズ
        //==================================================

        if (type != PlaceObjectType.Enemy)
        {
            SetObjectScale(obj);
        }

        // 敵はPrefab側のScaleをそのまま使用します。

        //==================================================
        // Rigidbody固定
        //==================================================

        Rigidbody[] rigidbodies =
            obj.GetComponentsInChildren<
                Rigidbody
            >();

        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints =
                RigidbodyConstraints.FreezeAll;
        }

        //==================================================
        // NavMeshAgent無効
        //==================================================

        NavMeshAgent[] agents =
            obj.GetComponentsInChildren<
                NavMeshAgent
            >();

        foreach (NavMeshAgent agent in agents)
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
            placeObject.GridPosition = pos;
        }
    }

    //==================================================
    // Object削除
    //==================================================

    public void DeleteObject(Vector3Int pos)
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
    // 敵移動有効化
    //==================================================

    public void EnableEnemyMovement()
    {
        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag(
                "Enemy"
            );

        foreach (GameObject enemy in enemies)
        {
            NavMeshAgent[] agents =
                enemy.GetComponentsInChildren<
                    NavMeshAgent
                >();

            foreach (NavMeshAgent agent in agents)
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
                    enemy.transform.position =
                        hit.position;

                    agent.enabled = true;

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
    // カメラ自動調整
    //==================================================

    public void AdjustCamera()
    {
        if (mapCamera == null)
        {
            Debug.LogWarning(
                "Map Cameraが設定されていません。"
            );

            return;
        }

        float mapWidth =
            (width - 1) *
            wallSpacing;

        float mapDepth =
            (depth - 1) *
            wallSpacing;

        float mapSize =
            Mathf.Max(
                mapWidth,
                mapDepth
            );

        float centerX =
            mapWidth / 2f;

        float centerZ =
            mapDepth / 2f;

        float scale =
            mapSize /
            baseMapSize;

        if (scale <= 0f)
        {
            scale = 1f;
        }

        float baseCenterX =
            baseMapSize / 2f;

        float baseCenterZ =
            baseMapSize / 2f;

        float offsetX =
            baseCameraPosition.x -
            baseCenterX;

        float offsetZ =
            baseCameraPosition.z -
            baseCenterZ;

        float cameraX =
            centerX +
            offsetX * scale;

        float cameraY =
            baseCameraPosition.y *
            scale;

        float cameraZ =
            centerZ +
            offsetZ * scale;

        mapCamera.transform.position =
            new Vector3(
                cameraX,
                cameraY,
                cameraZ
            );

        mapCamera.transform.rotation =
            Quaternion.Euler(
                cameraRotation
            );

        if (mapCamera.orthographic)
        {
            mapCamera.orthographicSize =
                baseOrthographicSize *
                scale;
        }

        Debug.Log(
            "カメラ位置 : " +
            mapCamera.transform.position
        );

        Debug.Log(
            "Orthographic Size : " +
            mapCamera.orthographicSize
        );
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