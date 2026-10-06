using UnityEngine;
using System.Collections.Generic;
using Fusion;

namespace Player
{
    /// <summary>
    /// プレイヤーの基本的なステータスを実装する場所
    /// </summary>
    public class PlayerStatus : MonoBehaviour
    {
        [Header("基本ステータス")]

        [Header("プレイヤーネーム")]
        [SerializeField] private string _playerName = "";

        [Tooltip("プレイヤーのHP")]
        [SerializeField] private int PlayerHP = 100;

        [Tooltip("プレイヤーの攻撃力")]
        [SerializeField] private int PlayerAttack = 20;

        [Tooltip("プレイヤーの防御力")]
        [SerializeField] private int PlayerDefense = 5;

        [Tooltip("プレイヤーの動くスピード")]
        [SerializeField] private float PlayerMoveSpeed = 2f;

        [Tooltip("プレイヤーの回避のスピード")]
        [SerializeField] private float PlayerEvadeSpeed = 5f;

        [Tooltip("ステッカーの所持数")]
        [SerializeField] private List<int> PlayerSticker;

        [Tooltip("プレイヤーの現在の体力")]
        public int CurrentHP { get; private set; }

        [Tooltip("プレイヤーの現在の攻撃力")]
        public int CurrentAttack { get; private set; }

        [Tooltip("プレイヤーの現在の防御力")]
        public int CurrentDefense { get; private set; }

        [Networked]
        public bool IsDeadNetwork { get; set; }

        [SerializeField] private PlayerUI _playerUI;

        // ダメージを受けた後の無敵時間のタイマー
        private float damageTimer = 0.0f;

        // ダメージを受けた後の無敵時間
        private float damageInvincibleTime = 3.0f;

        // 無敵状態かどうか
        public bool isInvincible => damageTimer > 0.0f;

        public int _playerHP => PlayerHP;
        public int _playerAttack => PlayerAttack;

        public float _playerMoveSpeed => PlayerMoveSpeed;
        public float _playerEvadeSpeed => PlayerEvadeSpeed;

        // プレイヤーが死亡しているか
        public bool _isDead { get; private set; }

        //==================================================
        // 初期化
        //==================================================

        private void Awake()
        {
            CurrentHP = PlayerHP;
            CurrentAttack = PlayerAttack;
            CurrentDefense = PlayerDefense;

            _isDead = false;
            IsDeadNetwork = false;

            if (_playerUI != null)
            {
                _playerUI.initializePlayerState(CurrentHP);
            }
        }

        //==================================================
        // Update
        //==================================================

        private void Update()
        {
            // ダメージを受けた後の無敵時間のタイマーを更新
            if (damageTimer > 0.0f)
            {
                damageTimer -= Time.deltaTime;
            }
        }

        //==================================================
        // ダメージ
        //==================================================

        public void Damage(int damage)
        {
            // すでに死亡している場合は処理しない
            if (_isDead)
            {
                return;
            }

            Debug.Log(
                _playerName + " dame-ziを受けました"
            );

            // 無敵時間を設定
            damageTimer = damageInvincibleTime;

            // 防御力を引く
            damage -= PlayerDefense;

            if (damage < 1)
            {
                damage = 0;
            }

            CurrentHP -= damage;

            if (_playerUI != null)
            {
                _playerUI.ChangeHp(CurrentHP);
            }

            // HPが0以下になったら死亡
            if (CurrentHP <= 0)
            {
                CurrentHP = 0;

                Die();
            }
        }

        //==================================================
        // 回復
        //==================================================

        public void Heal(int value)
        {
            if (_isDead)
            {
                return;
            }

            CurrentHP += value;

            if (CurrentHP > PlayerHP)
            {
                CurrentHP = PlayerHP;
            }

            if (_playerUI != null)
            {
                _playerUI.ChangeHp(CurrentHP);
            }
        }

        //==================================================
        // 死亡
        //==================================================

        private void Die()
        {
            if (_isDead)
            {
                return;
            }

            _isDead = true;

            IsDeadNetwork = true;

            Debug.Log("プレイヤー死亡");

            TestPlayManager testPlayManager =
                FindObjectOfType<TestPlayManager>();

            if (testPlayManager != null &&
                testPlayManager.IsPlaying)
            {
                Debug.Log(
                    "テストプレイ中にプレイヤーが死亡しました"
                );

                Debug.Log(
                    "編集画面へ戻ります"
                );

                testPlayManager.ReturnToEdit();

                return;
            }

            if (_playerUI != null)
            {
                _playerUI.GameOver();
            }
        }
    }
}