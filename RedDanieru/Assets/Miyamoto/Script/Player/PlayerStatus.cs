using UnityEngine;
using System.Collections.Generic;

namespace Player
{
    /// <summary>
    /// プレイヤーの基本的なステータスを実装する場所
    /// </summary>
    public class PlayerStatus : MonoBehaviour
    {
        [Header("基本ステータス")]
        [Tooltip("プレイヤーのHP")]
        [SerializeField]private int PlayerHP  = 100;

        [Tooltip("プレイヤーの攻撃力")]
        [SerializeField] private int PlayerAttack = 20;

        [Tooltip("プレイヤーの防御力")]
        [SerializeField] private int PlayerDefense  = 5;

        [Tooltip("プレイヤーの歩くスピード")]
        [SerializeField] private float PlayerMoveSpeed = 2f;

        [Tooltip("プレイヤーの走るスピード")]
        [SerializeField] private float PlayerRunSpeed = 5f;

        [Tooltip("ステッカーの所持数")]
        [SerializeField] private List<int> PlayerSticker;

        [Tooltip("プレイヤーの現在の体力")]
        public int CurrentHP { get; private set; }

        [Tooltip("プレイヤーの現在の攻撃力")]
        public int CurrentAttack { get; private set; }

        [Tooltip("プレイヤーの現在の防御力")]
        public int CurrentDefense { get; private set; }
        
        private float damageTimer = 0.0f; //ダメージを受けた後の無敵時間のタイマー
        private float damageInvincibleTime = 3.0f; //ダメージを受けた後の無敵時間
        public bool isInvincible => damageTimer > 0.0f; //無敵状態かどうかを判定するプロパティ

        public int _playerHP => PlayerHP;
        public int _playerAttack => PlayerAttack;

        public float _playerMoveSpeed => PlayerMoveSpeed;

        public float _playerRunSpeed => PlayerRunSpeed;

        private void Awake()
        {
            CurrentHP = PlayerHP;
            CurrentAttack = PlayerAttack;
            CurrentDefense = PlayerDefense;
        }

        private void Update()
        {
            //ダメージを受けた後の無敵時間のタイマーを更新
            if (damageTimer > 0.0f)
            {
                damageTimer -= Time.deltaTime;
            }
        }

        public void Damage(int damage)
        {
            damageTimer = damageInvincibleTime; //ダメージを受けた後の無敵時間をリセット

            damage -= PlayerDefense;

            if (damage < 1)
                damage = 0;

            CurrentHP -= damage;

            if (CurrentHP <= 0)
            {
                CurrentHP = 0;
                Die();
            }
        }

        public void Heal(int value)
        {
            CurrentHP += value;

            if (CurrentHP > PlayerHP)
                CurrentHP = PlayerHP;
        }

        private void Die()
        {
            Debug.Log("死亡");
        }
    }

}


