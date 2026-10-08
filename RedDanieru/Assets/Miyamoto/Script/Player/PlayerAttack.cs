using System.Collections.Generic;
using UnityEngine;

///<summry>
///プレイヤーの攻撃を制御するためのスクリプト
///</summry>
namespace Player
{
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] List<PlayerAttackData> attackData = new ();
        [SerializeField] LayerMask _wallLayer;
        public  Transform _attackPoint;

        private PlayerAnimation _playerAnimation;
        private PlayerStatus _playerStatus;
        private PlayerAttackData _currentAttackData;
        private PlayerInputPriority _actionPriority;
        // 今回の攻撃ですでに攻撃した敵
        private HashSet<EnemyBase> hitEnemies = new();

        private bool _isAttackHitActive = false;

        // 攻撃の当たる位置と回転を計算するための変数
        private Vector3 _attackPosition;
        private Quaternion _attackRotation;

        void Start()
        {
            _playerAnimation = GetComponent<PlayerAnimation>();
            _playerStatus = GetComponent<PlayerStatus>();
            _actionPriority = GetComponent<PlayerInputPriority>();
        }

        //-----------------------------------------------------
        //攻撃したときの
        //ここにUpdateで攻撃入力を受け取り続けるようにする
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

            //ここでアクションン
            _input.attack = false;

            //当たり判定をここで有効にする
            _isAttackHitActive = false;

            // 攻撃データを決定
            _currentAttackData = attackData[0];

            // 今回の攻撃で当たった敵をリセット
            hitEnemies.Clear();

            //アッタクモーション
            _playerAnimation.PlayerAttackAnimator();
            
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
    　　　　　　_attackPoint.rotation * _currentAttackData.attackOffset;

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
                    if (hitEnemies.Contains(enemy))continue;

                    hitEnemies.Add(enemy);

                    // 今のところ仕様が決まっていないので、ダメージはプレイヤーの攻撃力と攻撃モーションによって決まるようにする
                    int damage =_currentAttackData.additionalDamage + _playerStatus.CurrentAttack;

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


        //------------------------------------------------------
        //プレイヤーの攻撃アニメーションが終わるときに呼び出される
        //------------------------------------------------------
        public void AttackEnd()
        {
            _isAttackHitActive = false;
            _currentAttackData = null;

            hitEnemies.Clear();

            // プレイヤーのAttackアクション終了
            _actionPriority.EndAction();
        }

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


