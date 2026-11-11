
using Player;
using System.Collections.Generic;
using UnityEngine;

public class PoisonContact : MonoBehaviour
{
    private enum GasState
    {
        Waiting,
        Warning,
        Spraying,
        Cooldown
    }

    [Header("起動範囲設定")]
    [SerializeField] private float activationRange = 5f;

    [Header("毒ガス範囲設定")]
    [SerializeField] private float poisonRange = 5f;

    [Header("発射時間設定")]
    [SerializeField] private float warningDuration = 1f;
    [SerializeField] private float sprayingDuration = 10f;
    [SerializeField] private float cooldownDuration = 5f;

    [Header("毒ガスダメージ設定")]
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

    private StickerState sourceStickerState;

    private GameObject poisonGasEffectInstance;

    private GasState currentState = GasState.Waiting;

    private float stateTimer;

    // キャラクターに直接貼られた場合のダメージタイマー
    private float characterDamageTimer;

    // 対象ごとの毒蓄積情報
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

    // 有効な毒ガス一覧
    private static readonly List<PoisonContact> activeGasContacts =
        new List<PoisonContact>();

    private void Awake()
    {
        sourceStickerState = GetComponent<StickerState>();
    }

    private void OnEnable()
    {
        if (!activeGasContacts.Contains(this))
        {
            activeGasContacts.Add(this);
        }
    }

    private void OnDisable()
    {
        activeGasContacts.Remove(this);

        currentState = GasState.Waiting;
        stateTimer = 0f;
        characterDamageTimer = 0f;

        UpdatePoisonEffect(false);
    }

    private void Update()
    {
        bool hasPoison =
            sourceStickerState != null &&
            sourceStickerState.currentSticker == Sticker.Poison;

        if (!hasPoison)
        {
            currentState = GasState.Waiting;
            stateTimer = 0f;
            characterDamageTimer = 0f;

            UpdatePoisonEffect(false);
            return;
        }

        // PlayerまたはEnemyに直接貼られているか確認
        GameObject character = GetCharacterRoot();

        if (character != null)
        {
            // キャラクターに貼られている場合は毒ガスを使用しない
            currentState = GasState.Waiting;
            stateTimer = 0f;

            UpdatePoisonEffect(false);

            // 貼られている間だけ継続ダメージ
            UpdateCharacterDamage(character);

            return;
        }

        // オブジェクト・トラップの場合は毒ガスを起動
        switch (currentState)
        {
            case GasState.Waiting:
                UpdateWaiting();
                break;

            case GasState.Warning:
                UpdateWarning();
                break;

            case GasState.Spraying:
                UpdateSpraying();
                break;

            case GasState.Cooldown:
                UpdateCooldown();
                break;
        }

        UpdatePoisonEffect(currentState == GasState.Spraying);
    }

    // PlayerまたはEnemyのルートオブジェクトを取得
    private GameObject GetCharacterRoot()
    {
        Transform current = transform;

        while (current != null)
        {
            if (current.GetComponent<PlayerStatus>() != null ||
                current.GetComponent<EnemyBase>() != null)
            {
                return current.gameObject;
            }

            current = current.parent;
        }

        return null;
    }

    // キャラクターに直接貼られた場合の継続ダメージ
    private void UpdateCharacterDamage(GameObject target)
    {
        characterDamageTimer -= Time.deltaTime;

        if (characterDamageTimer > 0f)
            return;

        // 毒状態を付与せず直接ダメージだけを与える
        DamageTarget(target, gasDamage);

        characterDamageTimer = Mathf.Max(
            gasDamageInterval,
            0.01f
        );
    }

    // 待機中：Playerが起動範囲に入ったか確認
    private void UpdateWaiting()
    {
        if (IsPlayerInActivationRange())
        {
            currentState = GasState.Warning;
            stateTimer = warningDuration;
        }
    }

    // 発動予兆：1秒間
    private void UpdateWarning()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            // 予兆開始後はPlayerが離れても発射する
            currentState = GasState.Spraying;
            stateTimer = sprayingDuration;
        }
    }

    // 毒ガス噴射：10秒間
    private void UpdateSpraying()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            currentState = GasState.Cooldown;
            stateTimer = cooldownDuration;

            UpdatePoisonEffect(false);
        }
    }

    // クールタイム：5秒間
    private void UpdateCooldown()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            currentState = GasState.Waiting;
            stateTimer = 0f;
        }
    }

    // Playerが起動範囲内にいるか確認
    private bool IsPlayerInActivationRange()
    {
        Collider[] nearbyObjects =
            Physics.OverlapSphere(
                transform.position,
                activationRange
            );

        foreach (Collider col in nearbyObjects)
        {
            Transform current = col.transform;

            while (current != null)
            {
                if (current.CompareTag("Player"))
                    return true;

                current = current.parent;
            }
        }

        return false;
    }

    // 毒ガスの効果を全体でまとめて管理
    private void LateUpdate()
    {
        if (activeGasContacts.Count == 0)
            return;

        Dictionary<GameObject, PoisonContact> insideTargets =
            new Dictionary<GameObject, PoisonContact>();

        // 噴射中の毒ガスだけ範囲判定する
        foreach (PoisonContact gas in activeGasContacts)
        {
            if (gas == null ||
                !gas.isActiveAndEnabled ||
                gas.sourceStickerState == null ||
                gas.sourceStickerState.currentSticker != Sticker.Poison ||
                gas.currentState != GasState.Spraying ||
                gas.GetCharacterRoot() != null)
            {
                continue;
            }

            Collider[] nearbyObjects =
                Physics.OverlapSphere(
                    gas.transform.position,
                    gas.poisonRange
                );

            foreach (Collider col in nearbyObjects)
            {
                GameObject target = FindDamageTarget(col.transform);

                if (target == null)
                    continue;

                // 同じ対象は一度だけ登録する
                if (!insideTargets.ContainsKey(target))
                {
                    insideTargets.Add(target, gas);
                }
            }
        }

        // 毒ガスに入っている対象を処理
        foreach (var pair in insideTargets)
        {
            GameObject target = pair.Key;
            PoisonContact gas = pair.Value;

            if (target == null || gas == null)
                continue;

            if (!sharedTargets.ContainsKey(target))
            {
                sharedTargets.Add(target, new TargetData());
            }

            TargetData data = sharedTargets[target];

            // 毒ガスによる継続ダメージ
            ApplySharedGasDamage(target, gas);

            PoisonStatus status =
                target.GetComponent<PoisonStatus>();

            // すでに毒状態なら蓄積を増やさない
            if (status != null && status.IsPoisoned)
            {
                data.accumulation = 0f;
                continue;
            }

            // 毒状態でなければ蓄積する
            data.accumulation += Time.deltaTime;

            // 5秒蓄積したら毒状態
            if (data.accumulation >= gas.poisonAccumulationTime)
            {
                if (status == null)
                {
                    status = target.AddComponent<PoisonStatus>();
                }

                status.ApplyPoison(
                    gas.poisonStatusDamage,
                    gas.poisonStatusDuration
                );

                data.accumulation = 0f;
            }
        }

        // 毒ガスの範囲外にいる対象の蓄積を減らす
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

            if (!insideTargets.ContainsKey(target))
            {
                float decreaseSpeed = GetDecreaseSpeed();

                pair.Value.accumulation -=
                    decreaseSpeed * Time.deltaTime;

                pair.Value.accumulation =
                    Mathf.Max(0f, pair.Value.accumulation);
            }
        }

        // 削除対象を整理
        foreach (GameObject target in removeTargets)
        {
            sharedTargets.Remove(target);
            gasDamageTimers.Remove(target);
        }
    }

    // ColliderからPlayerまたはEnemyのルートを取得
    private GameObject FindDamageTarget(Transform current)
    {
        while (current != null)
        {
            if (current.GetComponent<PlayerStatus>() != null)
                return current.gameObject;

            if (current.GetComponent<EnemyBase>() != null)
                return current.gameObject;

            current = current.parent;
        }

        return null;
    }

    // 複数の毒ガスでダメージが重複しないようにする
    private void ApplySharedGasDamage(
        GameObject target,
        PoisonContact gas)
    {
        if (gasDamageTimers.TryGetValue(
            target,
            out float nextDamageTime))
        {
            if (Time.time < nextDamageTime)
                return;
        }

        float interval = Mathf.Max(
            gas.gasDamageInterval,
            0.01f
        );

        gasDamageTimers[target] = Time.time + interval;

        DamageTarget(target, gas.gasDamage);
    }

    // 毒ガスの蓄積減少速度を取得
    private float GetDecreaseSpeed()
    {
        foreach (PoisonContact gas in activeGasContacts)
        {
            if (gas != null &&
                gas.isActiveAndEnabled &&
                gas.sourceStickerState != null &&
                gas.sourceStickerState.currentSticker == Sticker.Poison &&
                gas.GetCharacterRoot() == null)
            {
                return gas.accumulationDecreaseSpeed;
            }
        }

        return accumulationDecreaseSpeed;
    }

    // ダメージ処理
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

    // 毒ガスエフェクトの表示・非表示
    private void UpdatePoisonEffect(bool shouldShow)
    {
        if (shouldShow)
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

                // PrefabのX軸-90度の回転設定を維持する
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            activationRange
        );

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(
            transform.position,
            poisonRange
        );
    }

    private void OnDestroy()
    {
        activeGasContacts.Remove(this);

        if (poisonGasEffectInstance != null)
        {
            Destroy(poisonGasEffectInstance);
        }
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
        // すでに毒状態なら再付与しない
        if (IsPoisoned)
            return;

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
            {
                enemy.AddEnemyCurrentHP(-damage);
            }
            else if (player != null)
            {
                player.Damage(damage);
            }

            damageTimer = 1f;
        }

        if (timer >= duration)
        {
            IsPoisoned = false;
        }
    }
}
