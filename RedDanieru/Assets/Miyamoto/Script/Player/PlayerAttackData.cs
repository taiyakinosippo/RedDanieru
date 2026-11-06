using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAttackData", menuName = "AttackData")]
public class PlayerAttackData : ScriptableObject
{
    // 攻撃の名前
    public string attackName;

    // 攻撃の基本ダメージ
    public int additionalDamage;

    // 攻撃の範囲
    public Vector3 playerAttackRadius;

    // 攻撃判定の位置
    public Vector3 attackOffset;

    // 攻撃判定の回転
    public Vector3 attackRotation;

    // 攻撃のスタミナ消費量
    public float staminaCost;

    // 攻撃のクールダウン時間
    public float cooldownTime;

    // 攻撃がヒットするレイヤー
    public LayerMask hitLayer;

    // 攻撃の種類（近接攻撃、遠距離攻撃など）
    public AttackType attackType;

    [Header("コンボ入力受付時間")]

    [Tooltip("攻撃開始から入力受付を開始するまでの時間")]
    [Min(0f)]
    public float comboInputStartDelay = 0.3f;

    [Tooltip("コンボ入力を受け付ける時間")]
    [Min(0f)]
    public float comboInputDuration = 0.3f;
}
