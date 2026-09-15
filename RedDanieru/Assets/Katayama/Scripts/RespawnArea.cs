using UnityEngine;

public class RespawnArea : MonoBehaviour
{
    [Header("リスポーン地点")]
    [SerializeField] private Transform spawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (spawnPoint == null)
        {
            Debug.LogError("スポーンポイントが設定されていません。");
            return;
        }

        CharacterController controller =
            other.GetComponentInParent<CharacterController>();

        if (controller == null)
        {
            Debug.LogError("CharacterControllerが見つかりません。");
            return;
        }

        // CharacterControllerを無効化
        controller.enabled = false;

        // スポーン地点へ移動
        controller.transform.SetPositionAndRotation(
            spawnPoint.position,
            spawnPoint.rotation
        );

        // CharacterControllerを再有効化
        controller.enabled = true;
    }
}