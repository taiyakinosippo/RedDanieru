
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

    [Header("起動範囲")]
    [SerializeField] private float activationRange = 5f;

    [Header("毒ガス範囲")]
    [SerializeField] private float poisonRange = 5f;

    [Header("毒ガスの動作時間")]
    [SerializeField] private float warningDuration = 1f;
    [SerializeField] private float sprayingDuration = 10f;
    [SerializeField] private float cooldownDuration = 5f;

    [Header("毒ガスダメージ")]
    [SerializeField] private int gasDamage = 50;
    [SerializeField] private float gasDamageInterval = 1f;

    [Header("毒ガスエフェクト")]
    [SerializeField] private GameObject poisonGasEffectPrefab;

    [Header("毒状態設定")]
    [SerializeField] private float poisonAccumulationTime = 5f;
    [SerializeField] private float accumulationDecreaseSpeed = 0.5f;
    [SerializeField] private int poisonStatusDamage = 10;
    [SerializeField] private float poisonStatusDuration = 60f;

    [Header("ステッカー状態")]
    [SerializeField] private StickerState sourceStickerState;

    private GasState currentState = GasState.Waiting;

    private float stateTimer;
    private float characterDamageTimer;

    private GameObject poisonGasEffect;

    // 毒ガスを発生させているオブジェクト
    private static readonly List<PoisonContact> activeGasContacts =
        new List<PoisonContact>();

    // 対象ごとの毒蓄積時間
    private class TargetData
    {
        public float accumulation;
    }

    // 複数の毒ガスで蓄積時間を共有
    private static readonly Dictionary<GameObject, TargetData> sharedTargets =
        new Dictionary<GameObject, TargetData>();

    // 複数の毒ガスでダメージ間隔を共有
    private static readonly Dictionary<GameObject, float> gasDamageTimers =
        new Dictionary<GameObject, float>();

    private static int lastProcessedFrame = -1;

    private void Awake()
    {
        if (sourceStickerState == null)
        {
            sourceStickerState = GetComponent<StickerState>();
        }
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
        UpdatePoisonEffect(false);
    }

    private void Update()
    {
        if (sourceStickerState == null)
        {
            sourceStickerState = GetComponent<StickerState>();
        }

        // 毒ステッカー以外なら毒ガスを停止
        if (sourceStickerState == null ||
            sourceStickerState.currentSticker != Sticker.Poison)
        {
            ResetGas();
            return;
        }

        // PlayerまたはEnemyに直接貼られているか確認
        GameObject character = GetCharacterRoot();

        if (character != null)
        {
            // キャラクターに直接貼られた場合は毒状態にしない
            activeGasContacts.Remove(this);
            UpdatePoisonEffect(false);

            characterDamageTimer += Time.deltaTime;

            if (characterDamageTimer >= gasDamageInterval)
            {
                characterDamageTimer = 0f;
                DamageTarget(character, gasDamage);
            }

            return;
        }

        characterDamageTimer = 0f;

        UpdateGasState();
    }

    //==================================================
    // 毒ガス状態管理
    //==================================================

    private void UpdateGasState()
    {
        switch (currentState)
        {
            case GasState.Waiting:

                if (IsPlayerInActivationRange())
                {
                    currentState = GasState.Warning;
                    stateTimer = 0f;
                }

                break;

            case GasState.Warning:

                stateTimer += Time.deltaTime;

                if (stateTimer >= warningDuration)
                {
                    currentState = GasState.Spraying;
                    stateTimer = 0f;

                    if (!activeGasContacts.Contains(this))
                    {
                        activeGasContacts.Add(this);
                    }

                    UpdatePoisonEffect(true);
                }

                break;

            case GasState.Spraying:

                stateTimer += Time.deltaTime;

                UpdatePoisonEffect(true);

                if (stateTimer >= sprayingDuration)
                {
                    currentState = GasState.Cooldown;
                    stateTimer = 0f;

                    activeGasContacts.Remove(this);
                    UpdatePoisonEffect(false);
                }

                break;

            case GasState.Cooldown:

                stateTimer += Time.deltaTime;

                if (stateTimer >= cooldownDuration)
                {
                    currentState = GasState.Waiting;
                    stateTimer = 0f;
                }

                break;
        }
    }

    //==================================================
    // 起動範囲内にPlayerがいるか確認
    //==================================================

    private bool IsPlayerInActivationRange()
    {
        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            activationRange
        );

        foreach (Collider col in colliders)
        {
            if (col == null)
                continue;

            PlayerStatus player =
                col.GetComponentInParent<PlayerStatus>();

            if (player != null)
                return true;
        }

        return false;
    }

    //==================================================
    // キャラクターに直接貼られているか確認
    //==================================================

    private GameObject GetCharacterRoot()
    {
        Transform current = transform;

        while (current != null)
        {
            // ゴールはキャラクターとして扱わない
            if (current.GetComponent<GoalEnemyBase>() != null)
                return null;

            if (current.GetComponent<PlayerStatus>() != null ||
                current.GetComponent<EnemyBase>() != null)
            {
                return current.gameObject;
            }

            current = current.parent;
        }

        return null;
    }

    //==================================================
    // 毒ガスの共有処理
    //==================================================

    private void LateUpdate()
    {
        // 複数のPoisonContactから同じフレームに
        // 重複して処理されることを防ぐ
        if (lastProcessedFrame == Time.frameCount)
            return;

        lastProcessedFrame = Time.frameCount;

        ProcessSharedGasDamage();
    }

    private static void ProcessSharedGasDamage()
    {
        HashSet<GameObject> targetsInGas =
            new HashSet<GameObject>();

        // 現在噴射中の毒ガス範囲を調べる
        for (int i = activeGasContacts.Count - 1; i >= 0; i--)
        {
            PoisonContact contact = activeGasContacts[i];

            if (contact == null || !contact.isActiveAndEnabled)
            {
                activeGasContacts.RemoveAt(i);
                continue;
            }

            Collider[] colliders = Physics.OverlapSphere(
                contact.transform.position,
                contact.poisonRange
            );

            foreach (Collider col in colliders)
            {
                if (col == null)
                    continue;

                GameObject target = FindDamageTarget(col.transform);

                if (target == null)
                    continue;

                // ゴールは毒ガスの対象から除外
                if (target.GetComponent<GoalEnemyBase>() != null)
                    continue;

                targetsInGas.Add(target);
            }
        }

        List<GameObject> removeTargets =
            new List<GameObject>();

        // 毒ガス内の対象を処理
        foreach (GameObject target in targetsInGas)
        {
            if (target == null)
                continue;

            if (!sharedTargets.ContainsKey(target))
            {
                sharedTargets[target] = new TargetData();
            }

            TargetData data = sharedTargets[target];

            // 毒蓄積時間を加算
            data.accumulation += Time.deltaTime;

            // 毒ガスのダメージ間隔を共有
            if (!gasDamageTimers.ContainsKey(target))
            {
                gasDamageTimers[target] = 0f;
            }

            gasDamageTimers[target] += Time.deltaTime;

            float interval = GetTargetGasDamageInterval(target);

            if (gasDamageTimers[target] >= interval)
            {
                gasDamageTimers[target] = 0f;

                int damage = GetTargetGasDamage(target);

                DamageTarget(target, damage);
            }

            // 一定時間毒ガスを受けたら毒状態にする
            if (data.accumulation >= GetTargetAccumulationTime(target))
            {
                ApplyPoisonStatus(target);
            }
        }

        // 毒ガスの範囲外に出た対象の蓄積時間を減らす
        foreach (KeyValuePair<GameObject, TargetData> pair in sharedTargets)
        {
            GameObject target = pair.Key;

            if (target == null)
            {
                removeTargets.Add(target);
                continue;
            }

            if (targetsInGas.Contains(target))
                continue;

            float decreaseSpeed = GetDecreaseSpeed();

            pair.Value.accumulation -=
                decreaseSpeed * Time.deltaTime;

            pair.Value.accumulation =
                Mathf.Max(0f, pair.Value.accumulation);

            if (pair.Value.accumulation <= 0f)
            {
                removeTargets.Add(target);
            }
        }

        // 不要になった対象データを削除
        foreach (GameObject target in removeTargets)
        {
            sharedTargets.Remove(target);
            gasDamageTimers.Remove(target);
        }
    }

    //==================================================
    // 対象の検索
    //==================================================

    private static GameObject FindDamageTarget(Transform current)
    {
        while (current != null)
        {
            // GoalEnemyBaseを最優先で確認
            if (current.GetComponent<GoalEnemyBase>() != null)
            {
                return current.gameObject;
            }

            if (current.GetComponent<PlayerStatus>() != null ||
                current.GetComponent<EnemyBase>() != null)
            {
                return current.gameObject;
            }

            current = current.parent;
        }

        return null;
    }

    //==================================================
    // 毒ガス設定の取得
    //==================================================

    private static int GetTargetGasDamage(GameObject target)
    {
        foreach (PoisonContact contact in activeGasContacts)
        {
            if (contact != null)
                return contact.gasDamage;
        }

        return 50;
    }

    private static float GetTargetGasDamageInterval(GameObject target)
    {
        foreach (PoisonContact contact in activeGasContacts)
        {
            if (contact != null)
                return Mathf.Max(0.01f, contact.gasDamageInterval);
        }

        return 1f;
    }

    private static float GetTargetAccumulationTime(GameObject target)
    {
        foreach (PoisonContact contact in activeGasContacts)
        {
            if (contact != null)
                return contact.poisonAccumulationTime;
        }

        return 5f;
    }

    private static float GetDecreaseSpeed()
    {
        foreach (PoisonContact contact in activeGasContacts)
        {
            if (contact != null)
                return contact.accumulationDecreaseSpeed;
        }

        return 0.5f;
    }

    //==================================================
    // 毒状態の付与
    //==================================================

    private static void ApplyPoisonStatus(GameObject target)
    {
        if (target == null)
            return;

        // ゴールには毒状態を付与しない
        if (target.GetComponent<GoalEnemyBase>() != null)
            return;

        PoisonStatus poisonStatus =
            target.GetComponent<PoisonStatus>();

        if (poisonStatus == null)
        {
            poisonStatus = target.AddComponent<PoisonStatus>();
        }

        foreach (PoisonContact contact in activeGasContacts)
        {
            if (contact == null)
                continue;

            poisonStatus.ApplyPoison(
                contact.poisonStatusDamage,
                contact.poisonStatusDuration
            );

            break;
        }
    }

    //==================================================
    // ダメージ処理
    //==================================================

    private static void DamageTarget(GameObject target, int damage)
    {
        if (target == null)
            return;

        // ゴールは毒ガス・毒状態のダメージを受けない
        GoalEnemyBase goal =
            target.GetComponentInParent<GoalEnemyBase>();

        if (goal != null)
            return;

        EnemyBase enemy =
            target.GetComponent<EnemyBase>();

        PlayerStatus player =
            target.GetComponent<PlayerStatus>();

        if (enemy != null)
        {
            // 通常Enemyは直接HPを減らす
            enemy.AddEnemySlipDamage(-damage);
        }
        else if (player != null)
        {
            // Playerは通常のダメージ処理
            player.Damage(damage);
        }
    }

    //==================================================
    // 毒ガスエフェクト
    //==================================================

    private void UpdatePoisonEffect(bool active)
    {
        if (active)
        {
            if (poisonGasEffect == null &&
                poisonGasEffectPrefab != null)
            {
                poisonGasEffect = Instantiate(
                    poisonGasEffectPrefab,
                    transform
                );

                poisonGasEffect.transform.localPosition =
                    Vector3.zero;

                // Prefabの回転をX=-90度に設定
                poisonGasEffect.transform.localRotation =
                    Quaternion.Euler(-90f, 0f, 0f);
            }
        }
        else
        {
            if (poisonGasEffect != null)
            {
                Destroy(poisonGasEffect);
                poisonGasEffect = null;
            }
        }
    }

    //==================================================
    // 毒ガス停止
    //==================================================

    private void ResetGas()
    {
        activeGasContacts.Remove(this);

        currentState = GasState.Waiting;
        stateTimer = 0f;
        characterDamageTimer = 0f;

        UpdatePoisonEffect(false);
    }

    //==================================================
    // Gizmo表示
    //==================================================

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

        UpdatePoisonEffect(false);
    }
}

//======================================================
// 毒状態管理
//======================================================

public class PoisonStatus : MonoBehaviour
{
    private int damage;
    private float duration;

    private float durationTimer;
    private float damageTimer;

    public bool IsPoisoned { get; private set; }

    //==================================================
    // 毒状態付与
    //==================================================

    public void ApplyPoison(int poisonDamage, float poisonDuration)
    {
        // すでに毒状態なら重複させない
        if (IsPoisoned)
            return;

        // ゴールには毒状態を付与しない
        if (GetComponentInParent<GoalEnemyBase>() != null)
            return;

        damage = poisonDamage;
        duration = poisonDuration;

        durationTimer = 0f;
        damageTimer = 0f;

        IsPoisoned = true;
    }

    //==================================================
    // 毒状態更新
    //==================================================

    private void Update()
    {
        if (!IsPoisoned)
            return;

        // ゴールは毒ダメージを受けない
        if (GetComponentInParent<GoalEnemyBase>() != null)
        {
            EndPoison();
            return;
        }

        durationTimer += Time.deltaTime;
        damageTimer += Time.deltaTime;

        // 毒状態の継続時間終了
        if (durationTimer >= duration)
        {
            EndPoison();
            return;
        }

        // ダメージ間隔
        if (damageTimer >= 1f)
        {
            damageTimer = 0f;

            // ゴール判定を再確認
            if (GetComponentInParent<GoalEnemyBase>() != null)
            {
                EndPoison();
                return;
            }

            EnemyBase enemy =
                GetComponent<EnemyBase>();

            PlayerStatus player =
                GetComponent<PlayerStatus>();

            if (enemy != null)
            {
                // 通常Enemyは直接HPを減らす
                enemy.AddEnemySlipDamage(-damage);
            }
            else if (player != null)
            {
                // Playerは通常のダメージ処理
                player.Damage(damage);
            }
        }
    }

    //==================================================
    // 毒状態終了
    //==================================================

    public void EndPoison()
    {
        IsPoisoned = false;

        durationTimer = 0f;
        damageTimer = 0f;

        Destroy(this);
    }
}
