using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAttackData", menuName = "AttackData")]
public class PlayerAttackData : ScriptableObject
{
    public string attackName;

    public int additionalDamage;

    public Vector3 playerAttackRadius;

    public float staminaCost;

    public LayerMask hitLayer;

    public AttackType attackType;
}
