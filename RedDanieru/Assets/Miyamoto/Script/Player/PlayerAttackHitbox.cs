using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// 攻撃の当たり判定を管理する
    /// </summary>
    public class PlayerAttackHitbox : MonoBehaviour
    {
        [Header("壁判定")]
        [SerializeField] private LayerMask _wallLayer;

        private readonly HashSet<EnemyBase> _hitEnemies = new();

        public void BeginHit()
        {
            _hitEnemies.Clear();
        }

        public void AttackHit(
            Transform attackPoint,
            PlayerAttackData attackData,
            PlayerStatus playerStatus)
        {
            if (attackPoint == null ||
                attackData == null ||
                playerStatus == null)
            {
                return;
            }

            Vector3 attackPosition =
                attackPoint.position +
                attackPoint.rotation * attackData.attackOffset;

            Quaternion attackRotation =
                attackPoint.rotation *
                Quaternion.Euler(attackData.attackRotation);

            Collider[] hitColliders = Physics.OverlapBox(
                attackPosition,
                attackData.playerAttackRadius,
                attackRotation,
                attackData.hitLayer,
                QueryTriggerInteraction.Ignore);

            foreach (Collider collider in hitColliders)
            {
                // 敵本体または親から EnemyBase を取得する
                EnemyBase enemy = collider.GetComponentInParent<EnemyBase>();

                if (enemy == null)
                {
                    continue;
                }

                if (IsWallInFront(enemy))
                {
                    continue;
                }

                // Add が false なら、同じ攻撃ですでに命中している
                if (!_hitEnemies.Add(enemy))
                {
                    continue;
                }

                int damage =
                    attackData.additionalDamage +
                    playerStatus.CurrentAttack;

                NetworkGameState.DamageEnemy(enemy, damage);
            }
        }

        private bool IsWallInFront(EnemyBase enemy)
        {
            Vector3 playerPosition = transform.position;
            Vector3 targetPosition = enemy.transform.position;

            Vector3 direction = targetPosition - playerPosition;
            float distance = direction.magnitude;

            if (distance <= 0f)
            {
                return false;
            }

            return Physics.Raycast(
                playerPosition,
                direction.normalized,
                distance,
                _wallLayer,
                QueryTriggerInteraction.Ignore);
        }

        public void EndHit()
        {
            _hitEnemies.Clear();
        }
    }
}