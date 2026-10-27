
using UnityEngine;

public class MapCameraController : MonoBehaviour
{
    [Header("ズーム設定")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 5f;

    [Header("カメラ移動設定")]
    [SerializeField] private float moveSpeed = 0.15f;

    [Header("移動するカメラ")]
    [SerializeField] private Camera targetCamera;

    [Header("マップの範囲（ワールド座標）")]
    [SerializeField] private float mapMinX = 0f;
    [SerializeField] private float mapMaxX = 31f;
    [SerializeField] private float mapMinZ = 0f;
    [SerializeField] private float mapMaxZ = 31f;

    // 起動時のズーム値
    private float initialCameraZoom;

    // 最大まで縮小したときのカメラ位置
    private Vector3 maxZoomCameraPosition;

    private Vector3 lastMousePosition;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
        {
            Debug.LogError(
                "MapCameraControllerにカメラが設定されていません。");
            enabled = false;
            return;
        }

        // 起動時の倍率を縮小限界として保存
        initialCameraZoom = targetCamera.orthographic
            ? targetCamera.orthographicSize
            : targetCamera.fieldOfView;

        // 起動時の位置を最大縮小時の基準位置として保存
        maxZoomCameraPosition = targetCamera.transform.position;
    }

    private void Update()
    {
        if (targetCamera == null)
            return;

        HandleZoom();
        HandleCameraMove();
    }

    private void HandleZoom()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) < 0.01f)
            return;

        if (targetCamera.orthographic)
        {
            targetCamera.orthographicSize = Mathf.Clamp(
                targetCamera.orthographicSize - scroll * zoomSpeed,
                Mathf.Min(minZoom, initialCameraZoom),
                initialCameraZoom);
        }
        else
        {
            targetCamera.fieldOfView = Mathf.Clamp(
                targetCamera.fieldOfView - scroll * zoomSpeed,
                Mathf.Min(minZoom, initialCameraZoom),
                initialCameraZoom);
        }

        // 最大縮小状態では基準位置に戻す
        if (IsAtMaximumZoomOut())
        {
            targetCamera.transform.position = maxZoomCameraPosition;
        }
        else
        {
            ClampCameraPosition();
        }
    }

    private void HandleCameraMove()
    {
        // 最大まで縮小しているときは移動できない
        if (IsAtMaximumZoomOut())
        {
            targetCamera.transform.position = maxZoomCameraPosition;
            return;
        }

        bool isMoving =
            Input.GetMouseButton(2) ||
            Input.GetMouseButton(1);

        if (!isMoving)
            return;

        if (Input.GetMouseButtonDown(2) ||
            Input.GetMouseButtonDown(1))
        {
            lastMousePosition = Input.mousePosition;
            return;
        }

        Vector3 currentMousePosition = Input.mousePosition;
        Vector3 mouseDelta = currentMousePosition - lastMousePosition;

        Transform cameraTransform = targetCamera.transform;

        Vector3 move =
            (-cameraTransform.right * mouseDelta.x
            - cameraTransform.up * mouseDelta.y)
            * moveSpeed;

        targetCamera.transform.position += move;

        ClampCameraPosition();

        lastMousePosition = currentMousePosition;
    }

    private bool IsAtMaximumZoomOut()
    {
        float currentZoom = targetCamera.orthographic
            ? targetCamera.orthographicSize
            : targetCamera.fieldOfView;

        return currentZoom >= initialCameraZoom - 0.001f;
    }

    private void ClampCameraPosition()
    {
        if (targetCamera == null)
            return;

        Vector3 position = targetCamera.transform.position;

        if (targetCamera.orthographic)
        {
            // 現在の倍率から画面に映る範囲を計算
            float halfHeight = targetCamera.orthographicSize;
            float halfWidth = halfHeight * targetCamera.aspect;

            float minX = mapMinX + halfWidth;
            float maxX = mapMaxX - halfWidth;
            float minZ = mapMinZ + halfHeight;
            float maxZ = mapMaxZ - halfHeight;

            if (minX > maxX)
                position.x = (mapMinX + mapMaxX) * 0.5f;
            else
                position.x = Mathf.Clamp(position.x, minX, maxX);

            if (minZ > maxZ)
                position.z = (mapMinZ + mapMaxZ) * 0.5f;
            else
                position.z = Mathf.Clamp(position.z, minZ, maxZ);
        }
        else
        {
            position.x = Mathf.Clamp(position.x, mapMinX, mapMaxX);
            position.z = Mathf.Clamp(position.z, mapMinZ, mapMaxZ);
        }

        targetCamera.transform.position = position;
    }
}
