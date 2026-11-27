
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class WindTrap : MonoBehaviour
{
    [Header("送風範囲")]
    [SerializeField] private float windLength = 8f;
    [SerializeField] private float windWidth = 3f;
    [SerializeField] private float windPower = 10f;

    [Header("動作時間")]
    [SerializeField] private float warningTime = 1f;
    [SerializeField] private float blowingTime = 5f;
    [SerializeField] private float cooldownTime = 5f;

    [Header("色の設定")]
    [SerializeField] private Renderer warningRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color blowingColor = Color.red;

    [Header("風エフェクト")]
    [SerializeField] private GameObject windEffectPrefab;
    [SerializeField] private float windEffectDistance = 1.5f;

    // ステッカー関連（未実装のため一時的に無効化）
    // private StickerState stickerState;

    private Coroutine windCoroutine;
    private GameObject windEffect;

    private void Awake()
    {
        // stickerState = GetComponent<StickerState>();

        CreateWindEffect();

        SetColor(normalColor);
        SetWindEffect(false);
    }

    private void OnEnable()
    {
        if (windCoroutine == null)
        {
            windCoroutine = StartCoroutine(WindRoutine());
        }
    }

    private void OnDisable()
    {
        if (windCoroutine != null)
        {
            StopCoroutine(windCoroutine);
            windCoroutine = null;
        }

        SetColor(normalColor);
        SetWindEffect(false);
    }

    // 風エフェクトを生成
    private void CreateWindEffect()
    {
        if (windEffectPrefab == null)
        {
            return;
        }

        Vector3 direction = transform.forward;

        Vector3 position =
            transform.position + direction * windEffectDistance;

        windEffect = Instantiate(
            windEffectPrefab,
            position,
            Quaternion.LookRotation(direction),
            transform
        );

        windEffect.SetActive(false);
    }

    // 風エフェクトの表示切り替え
    private void SetWindEffect(bool active)
    {
        if (windEffect != null)
        {
            windEffect.SetActive(active);
        }
    }

    // 動作サイクル
    private IEnumerator WindRoutine()
    {
        while (true)
        {
            // ステッカーによる動作制限は未実装のため無効化
            // if (stickerState == null ||
            //     stickerState.currentSticker == Sticker.None)
            // {
            //     yield return null;
            //     continue;
            // }

            // 警告状態
            SetWindEffect(false);
            SetColor(warningColor);

            yield return new WaitForSeconds(warningTime);

            // 送風状態
            SetColor(blowingColor);
            SetWindEffect(true);

            // 送風対象を取得
            CharacterController[] controllers =
                FindObjectsOfType<CharacterController>();

            NavMeshAgent[] agents =
                FindObjectsOfType<NavMeshAgent>();

            float elapsed = 0f;

            while (elapsed < blowingTime)
            {
                float deltaTime = Time.deltaTime;

                PushTargets(deltaTime, controllers, agents);

                elapsed += deltaTime;

                yield return null;
            }

            // クールタイム
            SetWindEffect(false);
            SetColor(normalColor);

            yield return new WaitForSeconds(cooldownTime);
        }
    }

    // 色の変更
    private void SetColor(Color color)
    {
        if (warningRenderer != null)
        {
            warningRenderer.material.color = color;
        }
    }

    // 風の範囲内にいるか確認
    private bool IsInWindRange(Transform target)
    {
        Vector3 localPosition =
            transform.InverseTransformPoint(target.position);

        // 正面方向、左右の幅を確認
        return localPosition.z >= 0f &&
               localPosition.z <= windLength &&
               Mathf.Abs(localPosition.x) <= windWidth / 2f;
    }

    // 風で対象を押し出す
    private void PushTargets(
        float deltaTime,
        CharacterController[] controllers,
        NavMeshAgent[] agents)
    {
        // 送風方向
        Vector3 pushDirection = transform.forward;
        pushDirection.y = 0f;
        pushDirection.Normalize();

        // 1フレーム分の移動距離
        Vector3 pushMovement =
            pushDirection * windPower * deltaTime;

        // CharacterControllerを押す
        foreach (CharacterController controller in controllers)
        {
            if (controller == null || !controller.enabled)
            {
                continue;
            }

            if (!IsInWindRange(controller.transform))
            {
                continue;
            }

            controller.Move(pushMovement);
        }

        // NavMeshAgentを押す
        foreach (NavMeshAgent agent in agents)
        {
            if (agent == null ||
                !agent.enabled ||
                !agent.isOnNavMesh)
            {
                continue;
            }

            if (!IsInWindRange(agent.transform))
            {
                continue;
            }

            agent.Move(pushMovement);
        }
    }

    // 風エフェクトを破棄
    private void OnDestroy()
    {
        if (windEffect != null)
        {
            Destroy(windEffect);
        }
    }

    // 送風範囲をSceneビューに表示
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix = transform.localToWorldMatrix;

        // 長方形の送風範囲
        Gizmos.DrawWireCube(
            new Vector3(0f, 1f, windLength / 2f),
            new Vector3(windWidth, 2f, windLength)
        );

        Gizmos.matrix = oldMatrix;

        // 送風方向
        Gizmos.color = Color.red;

        Gizmos.DrawRay(
            transform.position,
            transform.forward * windLength
        );
    }
}
