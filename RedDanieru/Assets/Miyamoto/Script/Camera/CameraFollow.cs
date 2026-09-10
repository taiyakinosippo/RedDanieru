using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class CameraFollow : MonoBehaviour
{

    private Vector3 diff; //カメラとプレイヤーの距離
    public GameObject target; //追従するターゲットオブジェクト
    public float followSpeed; //追従するスピード
    public float _cameraHeight; //カメラの高さ
    private Vector3 playerPosition;
    public float rayDistance = 10.0f;

    void Start()
    {
        playerPosition = target.transform.position + Vector3.up * _cameraHeight;
        diff = transform.position - playerPosition;
    }


    public void FixedUpdate()
    {
        playerPosition = target.transform.position + Vector3.up * _cameraHeight;
        Vector3 targetPosition =　playerPosition + diff;
        transform.position = playerPosition + diff;

        // カメラの正面方向にRayを飛ばす
        Debug.DrawRay(
            transform.position,
            transform.forward * rayDistance,
            Color.red
        );

    }
}
//if (Physics.SphereCast(transform.position, _sphereSize, tpos - transform.position, out RaycastHit hit, (tpos - transform.position).magnitude, _wallLayer))
//{
//    transform.position = hit.point + hit.normal * _sphereSize;
//}
//else
//{
//    transform.position = tpos;
//}