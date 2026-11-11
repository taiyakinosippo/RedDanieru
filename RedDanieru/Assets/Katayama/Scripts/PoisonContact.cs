
using Player;
using System.Collections.Generic;
using UnityEngine;

public class PoisonContact : MonoBehaviour
{
    [Header("毒ガス設定")]
    [SerializeField] private float poisonRange = 5f;
    [SerializeField] private int gasDamage = 50;
    [SerializeField] private float gasDamageInterval = 1f;

    [Header("毒ガスエフェクト")]
    [SerializeField] private GameObject poisonGasEffectPrefab;

    [Header("毒の蓄積設定")]
    [SerializeField] private float poisonAccumulationTime = 5f;
    [SerializeField] private float accumulationDecreaseSpeed = 0.5f;

    [Header("毒状態設定")]
    [SerializeField] private int poisonStatusDamage = 10;
    [SerializeField] private float poisonStatusDuration = 60f;

    [Header("対象設定")]
    [SerializeField] private bool affectEnemy = true;
    [SerializeField] private bool affectPlayer = true;

    private StickerState sourceStickerState;
    private GameObject poisonGasEffectInstance;

    private class TargetData
    {
        public float accumulation;
    }

    // 全毒ガスで共有する対象情報
    private static Dictionary<GameObject, TargetData> sharedTargets =
        new Dictionary<GameObject, TargetData>();

    // 全毒ガスで共有するダメージタイマー
    private static Dictionary<GameObject, float> gasDamageTimers =
        new Dictionary<GameObject, float>();

    private static readonly List<PoisonContact> activeGasContacts =
        new List<PoisonContact>();

    private void Awake()
    {
        sourceStickerState = GetComponent<StickerState>();
    }

    private void OnEnable()
    {
        if (!activeGasContacts.Contains(this))
            activeGasContacts.Add(this);
    }

    private void OnDisable()
    {
        activeGasContacts.Remove(this);

        if (poisonGasEffectInstance != null)
        {
            Destroy(poisonGasEffectInstance);
            poisonGasEffectInstance = null;
        }
    }

    private void Update()
    {
        bool hasPoison =
            sourceStickerState != null &&
            sourceStickerState.currentSticker == Sticker.Poison;

        UpdatePoisonEffect(hasPoison);

        if (!hasPoison)
            return;

        Collider[] nearbyObjects =
            Physics.OverlapSphere(
                transform.position,
                poisonRange
            );

        foreach (Collider col in nearbyObjects)
        {
            GameObject target = col.transform.root.gameObject;

            bool isEnemy = target.CompareTag("Enemy");
            bool isPlayer = target.CompareTag("Player");

            if (isEnemy && !affectEnemy) continue;
            if (isPlayer && !affectPlayer) continue;
            if (!isEnemy && !isPlayer) continue;

            if (!sharedTargets.ContainsKey(target))
            {
                sharedTargets.Add(target, new TargetData());
            }
        }
    }

    // 毒ガスの効果を全体でまとめて管理
    private void LateUpdate()
    {
        if (activeGasContacts.Count == 0)
            return;

        HashSet<GameObject> insideTargets =
            new HashSet<GameObject>();

        // 有効な毒ガスすべての範囲を確認
        foreach (PoisonContact gas in activeGasContacts)
        {
            if (gas == null ||
                gas.sourceStickerState == null ||
                gas.sourceStickerState.currentSticker != Sticker.Poison)
                continue;

            Collider[] nearbyObjects =
                Physics.OverlapSphere(
                    gas.transform.position,
                    gas.poisonRange
                );

            foreach (Collider col in nearbyObjects)
            {
                GameObject target = col.transform.root.gameObject;

                bool isEnemy = target.CompareTag("Enemy");
                bool isPlayer = target.CompareTag("Player");

                if (isEnemy && !gas.affectEnemy) continue;
                if (isPlayer && !gas.affectPlayer) continue;
                if (!isEnemy && !isPlayer) continue;

                insideTargets.Add(target);
            }
        }

        // 毒ガスに入っている対象を処理
        foreach (GameObject target in insideTargets)
        {
            if (!sharedTargets.ContainsKey(target))
                sharedTargets.Add(target, new TargetData());

            TargetData data = sharedTargets[target];

            // 蓄積は毒ガスの数に関係なく一度だけ増やす
            data.accumulation += Time.deltaTime;

            // 毒ガスによる継続ダメージ
            ApplySharedGasDamage(target);

            // 5秒蓄積したら毒状態
            if (data.accumulation >= GetAccumulationTime(target))
            {
                PoisonStatus status =
                    target.GetComponent<PoisonStatus>();

                if (status == null)
                    status = target.AddComponent<PoisonStatus>();

                if (!status.IsPoisoned)
                {
                    status.ApplyPoison(
                        GetPoisonDamage(target),
                        GetPoisonDuration(target)
                    );
                }

                data.accumulation = 0f;
            }
        }

        // 範囲外の対象は蓄積を減らす
        List<GameObject> removeTargets =
            new List<GameObject>();

        foreach (var pair in sharedTargets)
        {
            GameObject target = pair.Key;

            if (target == null)
            {
                removeTargets.Add(target);
                continue;
            }

            if (!insideTargets.Contains(target))
            {
                float decreaseSpeed = GetDecreaseSpeed(target);

                pair.Value.accumulation -=
                    decreaseSpeed * Time.deltaTime;

                pair.Value.accumulation =
                    Mathf.Max(0f, pair.Value.accumulation);
            }
        }

        foreach (GameObject target in removeTargets)
        {
            sharedTargets.Remove(target);
            gasDamageTimers.Remove(target);
        }
    }

    private void ApplySharedGasDamage(GameObject target)
    {
        if (gasDamageTimers.TryGetValue(target, out float nextDamageTime))
        {
            if (Time.time < nextDamageTime)
                return;
        }

        // 対象に接触している毒ガスからダメージ設定を取得
        int damage = GetGasDamage(target);
        float interval = GetGasDamageInterval(target);

        gasDamageTimers[target] = Time.time + interval;

        DamageTarget(target, damage);
    }

    private int GetGasDamage(GameObject target)
    {
        foreach (PoisonContact gas in activeGasContacts)
        {
            if (gas != null &&
                gas.sourceStickerState != null &&
                gas.sourceStickerState.currentSticker == Sticker.Poison)
            {
                if (IsInsideGas(gas, target))
                    return gas.gasDamage;
            }
        }

        return gasDamage;
    }

    private float GetGasDamageInterval(GameObject target)
    {
        foreach (PoisonContact gas in activeGasContacts)
        {
            if (gas != null &&
                gas.sourceStickerState != null &&
                gas.sourceStickerState.currentSticker == Sticker.Poison)
            {
                if (IsInsideGas(gas, target))
                    return gas.gasDamageInterval;
            }
        }

        return gasDamageInterval;
    }

    private float GetAccumulationTime(GameObject target)
    {
        foreach (PoisonContact gas in activeGasContacts)
        {
            if (gas != null &&
                gas.sourceStickerState != null &&
                gas.sourceStickerState.currentSticker == Sticker.Poison)
            {
                if (IsInsideGas(gas, target))
                    return gas.poisonAccumulationTime;
            }
        }

        return poisonAccumulationTime;
    }

    private float GetDecreaseSpeed(GameObject target)
    {
        foreach (PoisonContact gas in activeGasContacts)
        {
            if (gas != null &&
                gas.sourceStickerState != null &&
                gas.sourceStickerState.currentSticker == Sticker.Poison)
            {
                if (IsInsideGas(gas, target))
                    return gas.accumulationDecreaseSpeed;
            }
        }

        return accumulationDecreaseSpeed;
    }

    private int GetPoisonDamage(GameObject target)
    {
        foreach (PoisonContact gas in activeGasContacts)
        {
            if (gas != null &&
                gas.sourceStickerState != null &&
                gas.sourceStickerState.currentSticker == Sticker.Poison)
            {
                if (IsInsideGas(gas, target))
                    return gas.poisonStatusDamage;
            }
        }

        return poisonStatusDamage;
    }

    private float GetPoisonDuration(GameObject target)
    {
        foreach (PoisonContact gas in activeGasContacts)
        {
            if (gas != null &&
                gas.sourceStickerState != null &&
                gas.sourceStickerState.currentSticker == Sticker.Poison)
            {
                if (IsInsideGas(gas, target))
                    return gas.poisonStatusDuration;
            }
        }

        return poisonStatusDuration;
    }

    private bool IsInsideGas(PoisonContact gas, GameObject target)
    {
        return Vector3.Distance(
            gas.transform.position,
            target.transform.position
        ) <= gas.poisonRange;
    }

    private void UpdatePoisonEffect(bool hasPoison)
    {
        if (hasPoison)
        {
            if (poisonGasEffectInstance == null &&
                poisonGasEffectPrefab != null)
            {
                poisonGasEffectInstance = Instantiate(
                    poisonGasEffectPrefab,
                    transform,
                    false
                );

                poisonGasEffectInstance.transform.localPosition =
                    Vector3.zero;

                // Prefabの回転設定（X軸-90度）を維持
            }
        }
        else
        {
            if (poisonGasEffectInstance != null)
            {
                Destroy(poisonGasEffectInstance);
                poisonGasEffectInstance = null;
            }
        }
    }

    private void DamageTarget(GameObject target, int damage)
    {
        if (target == null)
            return;

        EnemyBase enemy = target.GetComponent<EnemyBase>();
        PlayerStatus player = target.GetComponent<PlayerStatus>();

        if (enemy != null)
        {
            enemy.AddEnemyCurrentHP(-damage);
        }
        else if (player != null)
        {
            player.Damage(damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, poisonRange);
    }

    private void OnDestroy()
    {
        activeGasContacts.Remove(this);

        if (poisonGasEffectInstance != null)
            Destroy(poisonGasEffectInstance);
    }
}

// 毒状態を管理するコンポーネント
public class PoisonStatus : MonoBehaviour
{
    private int damage;
    private float duration;
    private float timer;
    private float damageTimer;

    public bool IsPoisoned { get; private set; }

    public void ApplyPoison(int poisonDamage, float poisonDuration)
    {
        damage = poisonDamage;
        duration = poisonDuration;

        timer = 0f;
        damageTimer = 0f;

        IsPoisoned = true;
    }

    private void Update()
    {
        if (!IsPoisoned)
            return;

        timer += Time.deltaTime;
        damageTimer -= Time.deltaTime;

        if (damageTimer <= 0f)
        {
            EnemyBase enemy = GetComponent<EnemyBase>();
            PlayerStatus player = GetComponent<PlayerStatus>();

            if (enemy != null)
                enemy.AddEnemyCurrentHP(-damage);
            else if (player != null)
                player.Damage(damage);

            damageTimer = 1f;
        }

        if (timer >= duration)
            IsPoisoned = false;
    }
}
