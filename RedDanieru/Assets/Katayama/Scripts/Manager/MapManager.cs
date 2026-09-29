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

    [Header("壁サイズ設定")]
    [SerializeField][Min(0.01f)] private float wallSize = 1f;
    [SerializeField][Min(0.01f)] private float wallSpacing = 1f;

    // 通常の壁の高さ
    [SerializeField] private float normalWallYOffset = 0f;

    // 配置した壁の高さ
    [SerializeField] private float placedWallYOffset = 0f;

    // 斜め壁の高さ
    [SerializeField] private float diagonalWallYOffset = 0f;

    [Header("床サイズ設定")]
    [SerializeField][Min(0.01f)] private float floorSize = 1f;
    [SerializeField][Min(0.01f)] private float floorSpacing = 1f;
    [SerializeField] private float floorYOffset = -1f;

    [Header("敵配置設定")]
    [SerializeField][Min(0.01f)] private float enemySpacing = 1f;

    [Header("設置オブジェクトサイズ設定")]
    [SerializeField][Min(0.01f)] private float objectSize = 1f;
    [SerializeField][Min(0.01f)] private float objectSpacing = 1f;
    [SerializeField] private float objectYOffset = 0f;

    [Header("Prefab")]
    public GameObject wallPrefab;

    [SerializeField] private GameObject diagonalWallPrefab;

    [SerializeField] private GameObject floorPrefab;

    [System.Serializable]
    public class PlaceObjectPrefab
    {
        public PlaceObjectType type;
        public GameObject prefab;
    }

    [SerializeField] private PlaceObjectPrefab[] objectPrefabs;

    [Header("NavMesh")]
    [SerializeField] private NavMeshSurface navMeshSurface;

    [Header("カメラ設定")]
    [SerializeField] private Camera mapCamera;

    [Tooltip("32×32マップ時のカメラ位置")]
    [SerializeField]
    private Vector3 baseCameraPosition =
        new Vector3(15.5f, 35f, 18.5f);

    [Tooltip("カメラ角度")]
    [SerializeField]
    private Vector3 cameraRotation =
        new Vector3(90f, 0f, 0f);

    [Tooltip("32×32マップ時のOrthographic Size")]
    [SerializeField]
    private float baseOrthographicSize = 20f;

    [Tooltip("基準となるマップサイズ")]
    [SerializeField]
    private float baseMapSize = 31f;

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

    // 斜め壁と床が同じマスに存在しているか
    private bool[,,] diagonalWallFloor;

    private int mapRevision = 0;

    public int MapRevision
    {
        get { return mapRevision; }
    }

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
    // 座標
    //==================================================

    // 通常の壁用
    private Vector3 GetNormalWallWorldPosition(
        Vector3Int pos)
    {
        return new Vector3(
            pos.x * wallSpacing,
            pos.y * wallSpacing + normalWallYOffset,
            pos.z * wallSpacing
        );
    }

    // 配置した壁用
    private Vector3 GetPlacedWallWorldPosition(
        Vector3Int pos)
    {
        return new Vector3(
            pos.x * wallSpacing,
            pos.y * wallSpacing + placedWallYOffset,
            pos.z * wallSpacing
        );
    }

    // 斜め壁用
    private Vector3 GetDiagonalWallWorldPosition(
        Vector3Int pos)
    {
        return new Vector3(
            pos.x * wallSpacing,
            pos.y * wallSpacing + diagonalWallYOffset,
            pos.z * wallSpacing
        );
    }

    private Vector3 GetFloorWorldPosition(
        Vector3Int pos)
    {
        return new Vector3(
            pos.x * floorSpacing,
            pos.y * floorSpacing + floorYOffset,
            pos.z * floorSpacing
        );
    }

    private Vector3 GetEnemyWorldPosition(
        Vector3Int pos)
    {
        return new Vector3(
            pos.x * enemySpacing,
            pos.y * enemySpacing + floorYOffset,
            pos.z * enemySpacing
        );
    }

    private Vector3 GetObjectWorldPosition(
        Vector3Int pos)
    {
        return new Vector3(
            pos.x * objectSpacing,
            pos.y * objectSpacing + floorYOffset + objectYOffset,
            pos.z * objectSpacing
        );
    }

    //==================================================
    // サイズ
    //==================================================

    private void SetWallScale(GameObject obj)
    {
        if (obj != null)
        {
            obj.transform.localScale *= wallSize;
        }
    }

    private void SetFloorScale(GameObject obj)
    {
        if (obj != null)
        {
            obj.transform.localScale *= floorSize;
        }
    }

    private void SetObjectScale(GameObject obj)
    {
        if (obj != null)
        {
            obj.transform.localScale *= objectSize;
        }
    }

    //==================================================
    // 変更通知
    //==================================================

    private void MarkMapChanged()
    {
        mapRevision++;

        SaveManager saveManager =
            FindObjectOfType<SaveManager>();

        if (saveManager != null)
        {
            saveManager.SetMapModified();
        }
    }

    //==================================================
    // 新規マップ
    //==================================================

    public void CreateNewMap()
    {
        GenerateMap();
        CreateMap();
        CreateDefaultRespawnObjects();

        AdjustCamera();

        mapRevision = 0;

        SaveManager saveManager =
            FindObjectOfType<SaveManager>();

        if (saveManager != null)
        {
            saveManager.SetMapModified();
        }
    }

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

        diagonalWallFloor =
            new bool[
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

                    placedObjectTypes[
                        x,
                        y,
                        z
                    ] = default;

                    diagonalWallFloor[
                        x,
                        y,
                        z
                    ] = false;
                }
            }
        }
    }

    private void CreateMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    if (map[x, y, z] != TileType.Wall)
                    {
                        continue;
                    }

                    CreateWall(
                        new Vector3Int(x, y, z)
                    );
                }
            }
        }

        UpdateDiagonalWalls();
    }

    //==================================================
    // 壁
    //==================================================

    private GameObject CreateWall(
        Vector3Int pos)
    {
        if (!IsInsideMap(pos))
        {
            return null;
        }

        if (wallPrefab == null)
        {
            Debug.LogError(
                "Wall Prefabが設定されていません。"
            );

            return null;
        }

        // 通常の壁はnormalWallYOffsetを使用
        GameObject wall =
            Instantiate(
                wallPrefab,
                GetNormalWallWorldPosition(pos),
                Quaternion.identity,
                transform
            );

        SetWallScale(wall);

        wallObjects[
            pos.x,
            pos.y,
            pos.z
        ] = wall;

        WallBlock block =
            wall.GetComponent<WallBlock>();

        if (block != null)
        {
            block.GridPosition = pos;
        }

        return wall;
    }

    //==================================================
    // 斜め壁判定
    //==================================================

    private void UpdateDiagonalWalls()
    {
        if (diagonalWallPrefab == null)
        {
            return;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    UpdateDiagonalWall(
                        new Vector3Int(x, y, z)
                    );
                }
            }
        }
    }

    private void UpdateDiagonalWall(
        Vector3Int pos)
    {
        if (!IsInsideMap(pos))
        {
            return;
        }

        // このマスが通常の壁として存在しているか
        bool mapWall =
            map[pos.x, pos.y, pos.z] ==
            TileType.Wall;

        // このマスが配置したWallか
        bool placedWall =
            map[pos.x, pos.y, pos.z] ==
            TileType.Floor &&
            placedObjects[
                pos.x,
                pos.y,
                pos.z
            ] != null &&
            placedObjectTypes[
                pos.x,
                pos.y,
                pos.z
            ] == PlaceObjectType.Wall;

        if (!mapWall && !placedWall)
        {
            return;
        }

        GameObject currentWall;

        if (mapWall)
        {
            currentWall =
                wallObjects[
                    pos.x,
                    pos.y,
                    pos.z
                ];
        }
        else
        {
            currentWall =
                placedObjects[
                    pos.x,
                    pos.y,
                    pos.z
                ];
        }

        if (currentWall == null)
        {
            return;
        }

        bool north =
            IsWall(
                new Vector3Int(
                    pos.x,
                    pos.y,
                    pos.z + 1
                )
            );

        bool south =
            IsWall(
                new Vector3Int(
                    pos.x,
                    pos.y,
                    pos.z - 1
                )
            );

        bool east =
            IsWall(
                new Vector3Int(
                    pos.x + 1,
                    pos.y,
                    pos.z
                )
            );

        bool west =
            IsWall(
                new Vector3Int(
                    pos.x - 1,
                    pos.y,
                    pos.z
                )
            );

        Quaternion rotation;

        // 北＋東
        if (north &&
            east &&
            !south &&
            !west)
        {
            rotation =
                Quaternion.Euler(
                    0f,
                    180f,
                    0f
                );
        }
        // 東＋南
        else if (east &&
                 south &&
                 !north &&
                 !west)
        {
            rotation =
                Quaternion.Euler(
                    0f,
                    270f,
                    0f
                );
        }
        // 南＋西
        else if (south &&
                 west &&
                 !north &&
                 !east)
        {
            rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    0f
                );
        }
        // 西＋北
        else if (west &&
                 north &&
                 !east &&
                 !south)
        {
            rotation =
                Quaternion.Euler(
                    0f,
                    90f,
                    0f
                );
        }
        else
        {
            RestoreNormalWall(pos);
            return;
        }

        ReplaceWithDiagonalWall(
            pos,
            rotation,
            mapWall
        );
    }

    //==================================================
    // 壁判定
    //==================================================

    private bool IsWall(
        Vector3Int pos)
    {
        if (!IsInsideMap(pos))
        {
            return false;
        }

        if (map[
                pos.x,
                pos.y,
                pos.z
            ] == TileType.Wall)
        {
            return true;
        }

        if (map[
                pos.x,
                pos.y,
                pos.z
            ] == TileType.Floor)
        {
            if (placedObjects[
                    pos.x,
                    pos.y,
                    pos.z
                ] != null &&
                placedObjectTypes[
                    pos.x,
                    pos.y,
                    pos.z
                ] == PlaceObjectType.Wall)
            {
                return true;
            }
        }

        return false;
    }

    //==================================================
    // 斜め壁判定
    //==================================================

    private bool IsDiagonalWall(
        GameObject obj)
    {
        if (obj == null ||
            diagonalWallPrefab == null)
        {
            return false;
        }

        string objectName =
            obj.name.Replace(
                "(Clone)",
                ""
            ).Trim();

        string diagonalName =
            diagonalWallPrefab.name.Trim();

        return objectName ==
               diagonalName;
    }

    //==================================================
    // 斜め壁へ変更
    //==================================================

    private void ReplaceWithDiagonalWall(
        Vector3Int pos,
        Quaternion rotation,
        bool isMapWall)
    {
        GameObject currentWall;

        if (isMapWall)
        {
            currentWall =
                wallObjects[
                    pos.x,
                    pos.y,
                    pos.z
                ];
        }
        else
        {
            currentWall =
                placedObjects[
                    pos.x,
                    pos.y,
                    pos.z
                ];
        }

        if (currentWall == null)
        {
            return;
        }

        // すでに斜め壁なら回転と高さを更新
        if (IsDiagonalWall(currentWall))
        {
            currentWall.transform.rotation =
                rotation;

            currentWall.transform.position =
                GetDiagonalWallWorldPosition(pos);

            if (isMapWall)
            {
                CreateDiagonalWallFloor(pos);
            }

            return;
        }

        Transform parent =
            currentWall.transform.parent;

        Destroy(currentWall);

        // 斜め壁はdiagonalWallYOffsetを使用
        GameObject diagonalWall =
            Instantiate(
                diagonalWallPrefab,
                GetDiagonalWallWorldPosition(pos),
                rotation,
                parent
            );

        SetWallScale(diagonalWall);

        if (isMapWall)
        {
            WallBlock block =
                diagonalWall.GetComponent<WallBlock>();

            if (block != null)
            {
                block.GridPosition = pos;
            }

            wallObjects[
                pos.x,
                pos.y,
                pos.z
            ] = diagonalWall;

            CreateDiagonalWallFloor(pos);
        }
        else
        {
            PlaceObject placeObject =
                diagonalWall.GetComponent<PlaceObject>();

            if (placeObject != null)
            {
                placeObject.GridPosition = pos;
            }

            WallBlock block =
                diagonalWall.GetComponent<WallBlock>();

            if (block != null)
            {
                block.GridPosition = pos;
            }

            placedObjects[
                pos.x,
                pos.y,
                pos.z
            ] = diagonalWall;

            placedObjectTypes[
                pos.x,
                pos.y,
                pos.z
            ] = PlaceObjectType.Wall;
        }
    }

    //==================================================
    // 斜め壁の下に床を作る
    //==================================================

    private void CreateDiagonalWallFloor(
        Vector3Int pos)
    {
        if (!IsInsideMap(pos))
        {
            return;
        }

        if (floorObjects[
                pos.x,
                pos.y,
                pos.z
            ] == null)
        {
            CreateFloor(pos);
        }

        diagonalWallFloor[
            pos.x,
            pos.y,
            pos.z
        ] = true;
    }

    //==================================================
    // 通常壁へ戻す
    //==================================================

    private void RestoreNormalWall(
        Vector3Int pos)
    {
        if (!IsInsideMap(pos))
        {
            return;
        }

        // 通常マップ壁
        if (map[
                pos.x,
                pos.y,
                pos.z
            ] == TileType.Wall)
        {
            GameObject currentWall =
                wallObjects[
                    pos.x,
                    pos.y,
                    pos.z
                ];

            if (currentWall == null)
            {
                return;
            }

            if (!IsDiagonalWall(currentWall))
            {
                return;
            }

            Transform parent =
                currentWall.transform.parent;

            Destroy(currentWall);

            // 通常壁はnormalWallYOffsetを使用
            GameObject normalWall =
                Instantiate(
                    wallPrefab,
                    GetNormalWallWorldPosition(pos),
                    Quaternion.identity,
                    parent
                );

            SetWallScale(normalWall);

            WallBlock block =
                normalWall.GetComponent<WallBlock>();

            if (block != null)
            {
                block.GridPosition = pos;
            }

            wallObjects[
                pos.x,
                pos.y,
                pos.z
            ] = normalWall;

            RemoveDiagonalWallFloor(pos);

            return;
        }

        // 配置したWall
        if (map[
                pos.x,
                pos.y,
                pos.z
            ] == TileType.Floor &&
            placedObjects[
                pos.x,
                pos.y,
                pos.z
            ] != null &&
            placedObjectTypes[
                pos.x,
                pos.y,
                pos.z
            ] == PlaceObjectType.Wall)
        {
            GameObject currentWall =
                placedObjects[
                    pos.x,
                    pos.y,
                    pos.z
                ];

            if (!IsDiagonalWall(currentWall))
            {
                return;
            }

            Transform parent =
                currentWall.transform.parent;

            Destroy(currentWall);

            // 配置壁はplacedWallYOffsetを使用
            GameObject normalWall =
                Instantiate(
                    wallPrefab,
                    GetPlacedWallWorldPosition(pos),
                    Quaternion.identity,
                    parent
                );

            SetWallScale(normalWall);

            WallBlock block =
                normalWall.GetComponent<WallBlock>();

            if (block != null)
            {
                block.GridPosition = pos;
            }

            placedObjects[
                pos.x,
                pos.y,
                pos.z
            ] = normalWall;

            placedObjectTypes[
                pos.x,
                pos.y,
                pos.z
            ] = PlaceObjectType.Wall;
        }
    }

    //==================================================
    // 斜め壁用の床を削除
    //==================================================

    private void RemoveDiagonalWallFloor(
        Vector3Int pos)
    {
        if (!IsInsideMap(pos))
        {
            return;
        }

        if (!diagonalWallFloor[
                pos.x,
                pos.y,
                pos.z])
        {
            return;
        }

        if (floorObjects[
                pos.x,
                pos.y,
                pos.z
            ] != null)
        {
            Destroy(
                floorObjects[
                    pos.x,
                    pos.y,
                    pos.z
                ]
            );

            floorObjects[
                pos.x,
                pos.y,
                pos.z
            ] = null;
        }

        diagonalWallFloor[
            pos.x,
            pos.y,
            pos.z
        ] = false;
    }

    //==================================================
    // リスポーン
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
    // 掘削
    //==================================================

    public void Dig(
        Vector3Int pos)
    {
        if (!IsInsideMap(pos))
        {
            return;
        }

        bool changed = false;

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

                if (IsDiggable(digPos))
                {
                    DigSingleTile(digPos);
                    changed = true;
                }
            }
        }

        if (changed)
        {
            UpdateDiagonalWalls();

            BuildNavigation();

            MarkMapChanged();
        }
    }

    private void DigSingleTile(
        Vector3Int pos)
    {
        if (!IsInsideMap(pos))
        {
            return;
        }

        bool isNormalWall =
            map[
                pos.x,
                pos.y,
                pos.z
            ] == TileType.Wall;

        bool isPlacedWall =
            map[
                pos.x,
                pos.y,
                pos.z
            ] == TileType.Floor &&
            placedObjects[
                pos.x,
                pos.y,
                pos.z
            ] != null &&
            placedObjectTypes[
                pos.x,
                pos.y,
                pos.z
            ] == PlaceObjectType.Wall;

        if (!isNormalWall &&
            !isPlacedWall)
        {
            return;
        }

        // 配置したWallを掘削
        if (isPlacedWall)
        {
            GameObject wall =
                placedObjects[
                    pos.x,
                    pos.y,
                    pos.z
                ];

            Destroy(wall);

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

            return;
        }

        // 通常壁を掘削
        map[
            pos.x,
            pos.y,
            pos.z
        ] = TileType.Floor;

        if (wallObjects[
                pos.x,
                pos.y,
                pos.z
            ] != null)
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

        diagonalWallFloor[
            pos.x,
            pos.y,
            pos.z
        ] = false;

        CreateFloor(pos);
    }

    //==================================================
    // 床
    //==================================================

    private GameObject CreateFloor(
        Vector3Int pos)
    {
        if (!IsInsideMap(pos))
        {
            return null;
        }

        if (floorPrefab == null)
        {
            Debug.LogError(
                "Floor Prefabが設定されていません。"
            );

            return null;
        }

        if (floorObjects[
                pos.x,
                pos.y,
                pos.z
            ] != null)
        {
            return floorObjects[
                pos.x,
                pos.y,
                pos.z
            ];
        }

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

        FloorBlock block =
            floor.GetComponent<FloorBlock>();

        if (block != null)
        {
            block.GridPosition = pos;
        }

        return floor;
    }

    //==================================================
    // 保存データ作成
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
                        (byte)map[
                            x,
                            y,
                            z
                        ];
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    if (placedObjects[
                            x,
                            y,
                            z
                        ] == null)
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
                    .eulerAngles.y;
        }
        else
        {
            data.hasRespawnPoint = false;
        }

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
    // ダンジョン読み込み
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

        diagonalWallFloor =
            new bool[
                width,
                height,
                depth
            ];

        // 敵を停止
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
                {
                    continue;
                }

                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
                agent.enabled = false;
            }
        }

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

        // タイルを復元
        int index = 0;

        for (int y = 0; y < height; y++)
        {
            for (int z = 0; z < depth; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    map[
                        x,
                        y,
                        z
                    ] =
                        (TileType)data.tiles[index++];

                    Vector3Int pos =
                        new Vector3Int(
                            x,
                            y,
                            z
                        );

                    if (
                        map[
                            x,
                            y,
                            z
                        ] == TileType.Wall
                    )
                    {
                        CreateWall(pos);
                    }
                    else if (
                        map[
                            x,
                            y,
                            z
                        ] == TileType.Floor
                    )
                    {
                        CreateFloor(pos);
                    }
                }
            }
        }

        // 斜め壁を再判定
        UpdateDiagonalWalls();

        // 配置オブジェクトを復元
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

        // 配置したWallを含めて再判定
        UpdateDiagonalWalls();

        BuildNavigation();

        if (enableEnemyMovement)
        {
            EnableEnemyMovement();
        }

        CreateRespawnObjects(data);

        AdjustCamera();

        mapRevision++;

        SaveManager saveManager =
            FindObjectOfType<SaveManager>();

        if (saveManager != null)
        {
            saveManager.SetMapModified();
        }

        Debug.Log(
            "ダンジョン復元完了"
        );
    }

    //==================================================
    // 配置オブジェクト
    //==================================================

    public void PlaceObject(
        Vector3Int pos,
        PlaceObjectType type)
    {
        if (!IsInsideMap(pos))
        {
            return;
        }

        if (type == PlaceObjectType.Wall)
        {
            PlaceDiggableWall(pos);
            return;
        }

        if (
            map[
                pos.x,
                pos.y,
                pos.z
            ] != TileType.Floor
        )
        {
            Debug.Log(
                "床以外にはオブジェクトを配置できません。"
            );

            return;
        }

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

        GameObject prefab =
            GetObjectPrefab(type);

        if (prefab == null)
        {
            Debug.LogError(
                type +
                " のPrefabが設定されていません。"
            );

            return;
        }

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

        GameObject obj =
            Instantiate(
                prefab,
                spawnPosition,
                Quaternion.identity,
                transform
            );

        if (type != PlaceObjectType.Enemy)
        {
            SetObjectScale(obj);

            AlignObjectToFloor(
                obj,
                pos
            );
        }

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

        NavMeshAgent[] agents =
            obj.GetComponentsInChildren<
                NavMeshAgent
            >();

        foreach (NavMeshAgent agent in agents)
        {
            agent.enabled = false;
        }

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

        MarkMapChanged();
    }

    //==================================================
    // オブジェクトの底面を床に合わせる
    //==================================================

    private bool TryGetBounds(
        GameObject obj,
        out Bounds bounds)
    {
        bounds = new Bounds();

        if (obj == null)
        {
            return false;
        }

        Renderer[] renderers =
            obj.GetComponentsInChildren<
                Renderer
            >(true);

        if (renderers.Length > 0)
        {
            bounds = renderers[0].bounds;

            for (
                int i = 1;
                i < renderers.Length;
                i++
            )
            {
                bounds.Encapsulate(
                    renderers[i].bounds
                );
            }

            return true;
        }

        Collider[] colliders =
            obj.GetComponentsInChildren<
                Collider
            >(true);

        if (colliders.Length > 0)
        {
            bounds = colliders[0].bounds;

            for (
                int i = 1;
                i < colliders.Length;
                i++
            )
            {
                bounds.Encapsulate(
                    colliders[i].bounds
                );
            }

            return true;
        }

        return false;
    }

    private void AlignObjectToFloor(
        GameObject obj,
        Vector3Int pos)
    {
        if (obj == null)
        {
            return;
        }

        if (!IsInsideMap(pos))
        {
            return;
        }

        GameObject floor =
            floorObjects[
                pos.x,
                pos.y,
                pos.z
            ];

        if (floor == null)
        {
            return;
        }

        if (
            !TryGetBounds(
                floor,
                out Bounds floorBounds
            )
        )
        {
            return;
        }

        if (
            !TryGetBounds(
                obj,
                out Bounds objectBounds
            )
        )
        {
            return;
        }

        float difference =
            floorBounds.max.y -
            objectBounds.min.y;

        obj.transform.position +=
            Vector3.up * difference;
    }

    //==================================================
    // 壁オブジェクト配置
    //==================================================

    private void PlaceDiggableWall(
        Vector3Int pos)
    {
        if (!IsInsideMap(pos))
        {
            return;
        }

        // 配置Wallは床マスに置く
        if (
            map[
                pos.x,
                pos.y,
                pos.z
            ] != TileType.Floor
        )
        {
            return;
        }

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

        if (wallPrefab == null)
        {
            Debug.LogError(
                "Wall Prefabが設定されていません。"
            );

            return;
        }

        // 配置した壁はplacedWallYOffsetを使用
        GameObject wall =
            Instantiate(
                wallPrefab,
                GetPlacedWallWorldPosition(pos),
                Quaternion.identity,
                transform
            );

        SetWallScale(wall);

        WallBlock wallBlock =
            wall.GetComponent<WallBlock>();

        if (wallBlock != null)
        {
            wallBlock.GridPosition = pos;
        }

        placedObjects[
            pos.x,
            pos.y,
            pos.z
        ] = wall;

        placedObjectTypes[
            pos.x,
            pos.y,
            pos.z
        ] = PlaceObjectType.Wall;

        UpdateDiagonalWalls();

        BuildNavigation();

        MarkMapChanged();
    }

    //==================================================
    // Prefab取得
    //==================================================

    private GameObject GetObjectPrefab(
        PlaceObjectType type)
    {
        if (objectPrefabs == null)
        {
            return null;
        }

        foreach (
            PlaceObjectPrefab data
            in objectPrefabs
        )
        {
            if (data.type == type)
            {
                return data.prefab;
            }
        }

        return null;
    }

    //==================================================
    // オブジェクト削除
    //==================================================

    public void DeleteObject(
        Vector3Int pos)
    {
        if (!IsInsideMap(pos))
        {
            return;
        }

        GameObject obj =
            placedObjects[
                pos.x,
                pos.y,
                pos.z
            ];

        if (obj == null)
        {
            return;
        }

        Destroy(obj);

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

        UpdateDiagonalWalls();

        BuildNavigation();

        MarkMapChanged();
    }

    //==================================================
    // Goal
    //==================================================

    public bool HasGoal()
    {
        if (placedObjects == null)
        {
            return false;
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
                        ] != null &&
                        placedObjectTypes[
                            x,
                            y,
                            z
                        ] == PlaceObjectType.Goal
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
    // リスポーン
    //==================================================

    public bool HasRespawnPoint()
    {
        return respawnPointObject != null;
    }

    public GameObject GetRespawnPointObject()
    {
        if (respawnPointObject == null)
        {
            return null;
        }

        return respawnPointObject;
    }

    private void CreateRespawnObjects(
        DungeonMapData data)
    {
        if (
            data.hasRespawnPoint &&
            respawnPointPrefab != null
        )
        {
            Vector3 position =
                new Vector3(
                    data.spawnPointX,
                    data.spawnPointY,
                    data.spawnPointZ
                );

            respawnPointObject =
                Instantiate(
                    respawnPointPrefab,
                    position,
                    Quaternion.identity,
                    transform
                );

            respawnPointObject.name =
                "RespawnPoint";

            Vector3 rotation =
                respawnPointObject
                    .transform
                    .eulerAngles;

            rotation.y =
                data.spawnPointRotY;

            respawnPointObject
                .transform
                .eulerAngles =
                rotation;
        }

        if (
            data.hasRespawnArea &&
            respawnAreaPrefab != null
        )
        {
            Vector3 position =
                new Vector3(
                    data.respawnAreaX,
                    data.respawnAreaY,
                    data.respawnAreaZ
                );

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
                new Vector3(
                    data.respawnAreaScaleX,
                    data.respawnAreaScaleY,
                    data.respawnAreaScaleZ
                );
        }
    }

    //==================================================
    // 配置オブジェクト取得
    //==================================================

    public GameObject GetPlacedObject(
        Vector3Int pos)
    {
        if (!IsInsideMap(pos))
        {
            return null;
        }

        return placedObjects[
            pos.x,
            pos.y,
            pos.z
        ];
    }

    //==================================================
    // 掘削可能判定
    //==================================================

    public bool IsDiggable(
        Vector3Int pos)
    {
        if (!IsInsideMap(pos))
        {
            return false;
        }

        if (
            map[
                pos.x,
                pos.y,
                pos.z
            ] == TileType.Wall
        )
        {
            return true;
        }

        if (
            map[
                pos.x,
                pos.y,
                pos.z
            ] == TileType.Floor &&
            placedObjects[
                pos.x,
                pos.y,
                pos.z
            ] != null &&
            placedObjectTypes[
                pos.x,
                pos.y,
                pos.z
            ] == PlaceObjectType.Wall
        )
        {
            return true;
        }

        return false;
    }

    public GameObject GetDiggableObject(
        Vector3Int pos)
    {
        if (!IsInsideMap(pos))
        {
            return null;
        }

        if (
            map[
                pos.x,
                pos.y,
                pos.z
            ] == TileType.Wall
        )
        {
            return wallObjects[
                pos.x,
                pos.y,
                pos.z
            ];
        }

        if (
            map[
                pos.x,
                pos.y,
                pos.z
            ] == TileType.Floor &&
            placedObjects[
                pos.x,
                pos.y,
                pos.z
            ] != null &&
            placedObjectTypes[
                pos.x,
                pos.y,
                pos.z
            ] == PlaceObjectType.Wall
        )
        {
            return placedObjects[
                pos.x,
                pos.y,
                pos.z
            ];
        }

        return null;
    }

    //==================================================
    // NavMesh
    //==================================================

    public void BuildNavigation()
    {
        if (navMeshSurface == null)
        {
            Debug.LogWarning(
                "NavMeshSurfaceが設定されていません。"
            );

            return;
        }

        navMeshSurface.BuildNavMesh();
    }

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
                if (!agent.enabled)
                {
                    agent.enabled = true;
                }

                if (agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                }
            }
        }
    }

    //==================================================
    // カメラ
    //==================================================

    public void AdjustCamera()
    {
        if (mapCamera == null)
        {
            return;
        }

        float currentMapSize =
            Mathf.Max(
                width,
                depth
            );

        float scale =
            currentMapSize /
            baseMapSize;

        mapCamera.transform.position =
            baseCameraPosition *
            scale;

        mapCamera.transform.eulerAngles =
            cameraRotation;

        mapCamera.orthographicSize =
            baseOrthographicSize *
            scale;
    }

    //==================================================
    // マップ内判定
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