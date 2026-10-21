using Fusion;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

///<summry>
///プレイヤーの攻撃を制御するためのスクリプト
///</summry>
namespace Player
{
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] List<PlayerAttackData> attackData = new();  // このlistの中から攻撃データを選択するようにする
        [SerializeField] LayerMask _wallLayer;                        // 攻撃する際に壁があるかどうかを判定するためのレイヤー
        public Transform _attackPoint;                               // 当たり判定の位置を決めるためのTransform

        private PlayerAnimation _playerAnimation;                     // プレイヤーのアニメーションを制御するためのスクリプト
        private PlayerStatus _playerStatus;                           // プレイヤーのステータスを管理するためのスクリプト
        private PlayerAttackData _currentAttackData;                  // 現在の攻撃データを格納する変数
        private PlayerInputPriority _actionPriority;                  // プレイヤーのアクションの優先度を管理するためのスクリプト
        private HashSet<EnemyBase> hitEnemies = new();                // 今回の攻撃で当たった敵を格納するためのHashSet

        private bool _isAttackHitActive = false;
        private Vector3 _attackPosition;                               // 攻撃の当たる位置と回転を計算するための格納変数
        private Quaternion _attackRotation;                            // 攻撃の当たる位置と回転を計算するための格納変数

        //コンボ攻撃用の変数たち
        private int  _comboIndex = 0;                                  // 現在のコンボ段数（0から始まる）
        private bool _isAttacking = false;                             // 攻撃アニメーションが再生されているかどうか
        private bool _isComboInputWindowOpen = false;　　　　　　　　　 // 次の攻撃を入力できるタイミングかどうか。
        private bool _hasQueuedComboInput = false;　　　　　　　　　　 // 受付時間中に次の攻撃入力があったかどうか。


        //-----------------------------------------------------
        //初期化
        //-----------------------------------------------------
        void Start()
        {
            _playerAnimation = GetComponent<PlayerAnimation>();
            _playerStatus = GetComponent<PlayerStatus>();
            _actionPriority = GetComponent<PlayerInputPriority>();
        }

        //-----------------------------------------------------
        //攻撃したときのここにUpdateで攻撃入力を受け取り続けるようにする
        //-----------------------------------------------------
        void Update()
        {
            if (_isAttackHitActive)
            {
                AttackHit();
            }
        }

        //------------------------------------------------------
        //プレイヤーの攻撃アニメーションを再生させるかどうか
        //------------------------------------------------------
        public void Attack(StarterAssetsInputs _input)
        {
            if (!_input.attack)
                return;
            //当たり判定をここで有効にする
            _isAttackHitActive = false;

            //ここでアクションン
            _input.attack = false;

            if (_isAttacking)
            {
                if (_isComboInputWindowOpen && _comboIndex < attackData.Count - 1)
                {
                    _hasQueuedComboInput = true;
                }
                return;
            }

            // 新しい攻撃を開始する
            BeginAttack();
        }

        private void BeginAttack()
        {
            _isAttacking = true;
            _isAttackHitActive = false;
            _isComboInputWindowOpen = false;

            // 現在のコンボ番号に対応する攻撃データを取得
            _currentAttackData = attackData[_comboIndex];

            // 攻撃済みの敵をリセット
            hitEnemies.Clear();

            Debug.Log($"{_comboIndex + 1}段目の攻撃を開始");

            // コンボ番号に対応した攻撃アニメーションを再生
            _playerAnimation.PlayerAttackAnimator(_comboIndex);
        }

        //------------------------------------------------------
        //プレイヤーの攻撃があたった場合の(ダメージ、属性)
        //------------------------------------------------------
        public void AttackHit()
        {
            if (_currentAttackData != null)
            {
                //攻撃の当たる位置を計算する
                _attackPosition =
                _attackPoint.position + 
                _attackPoint.rotation * 
                _currentAttackData.attackOffset;

                //攻撃の当たり判定の回転を計算する
                _attackRotation =
                _attackPoint.rotation *
                Quaternion.Euler(_currentAttackData.attackRotation);

                //攻撃があったかどうかを判定する
                Collider[] hitColliders =
                Physics.OverlapBox(
                _attackPosition,
                _currentAttackData.playerAttackRadius,
                _attackRotation,
                _currentAttackData.hitLayer,
                QueryTriggerInteraction.Ignore);

                //攻撃があたった場合の処理
                foreach (Collider collider in hitColliders)
                {
                    EnemyBase enemy = collider.GetComponent<EnemyBase>();

                    if (enemy == null) continue;

                    if (IsWallInFront(enemy)) continue;

                    // すでに今回の攻撃で攻撃済みなら無視
                    if (hitEnemies.Contains(enemy)) continue;

                    hitEnemies.Add(enemy);

                    // 今のところ仕様が決まっていないので、ダメージはプレイヤーの攻撃力と攻撃モーションによって決まるようにする
                    int damage = _currentAttackData.additionalDamage + _playerStatus.CurrentAttack;

                    // マルチでは全員の画面の敵にダメージが入るようにNetworkGameStateを通す
                    NetworkGameState.DamageEnemy(enemy, damage);

                }
            }
        }

        //--------------------------------------------------
        //Animation Eventで攻撃判定開始
        //--------------------------------------------------
        public void StartAttackHit()
        {
            _isAttackHitActive = true;
        }

        //--------------------------------------------------
        // コンボ入力受付を開始
        //--------------------------------------------------
        public void OpenComboInputWindow()
        {
            _isComboInputWindowOpen = true;
        }

        //--------------------------------------------------
        // コンボ入力受付を終了
        //--------------------------------------------------
        public void CloseComboInputWindow()
        {
            _isComboInputWindowOpen = false;
        }

        //------------------------------------------------------
        //プレイヤーの攻撃アニメーションが終わるときに呼び出される
        //------------------------------------------------------
        public void AttackEnd()
        {
            _isAttackHitActive = false;
            _currentAttackData = null;
            hitEnemies.Clear();

            _isComboInputWindowOpen = false;

            // 次のコンボ入力が予約されている場合
            if (_hasQueuedComboInput &&
                _comboIndex < attackData.Count - 1)
            {
                // 次の攻撃へ進む
                _comboIndex++;

                // 予約を解除
                _hasQueuedComboInput = false;

                Debug.Log($"{_comboIndex + 1}段目のコンボ攻撃へ移行");

                // 次の攻撃アニメーションを再生
                BeginAttack();

                return;
            }

            // コンボが終了した場合
            _hasQueuedComboInput = false;
            _comboIndex = 0;
            _isAttacking = false;

            _actionPriority.EndAction();
        }

        //------------------------------------------------------
        //プレイヤーの前に壁があるかどうかを判定する
        //------------------------------------------------------
        private bool IsWallInFront(EnemyBase enemy)
        {
            Vector3 PlayerPosition = transform.position;
            Vector3 targetPosition = enemy.transform.position;

            Vector3 direction = targetPosition - PlayerPosition;
            float distance = direction.magnitude;

            return Physics.Raycast(
                PlayerPosition,
                direction.normalized,
                distance,
                _wallLayer,
                QueryTriggerInteraction.Ignore
            );
        }
    }

    


}


