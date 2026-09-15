using Fusion;
using System.Collections.Generic;
using UnityEngine;

///<summry>
///プレイヤーの攻撃を制御するためのスクリプト
///</summry>
namespace Player
{
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] List<PlayerAttackData> attackData = new ();

        private PlayerAttackData currentAttackData;
       
        private PlayerAnimation _playerAnimation;
        
        void Start()
        {
            _playerAnimation = GetComponent<PlayerAnimation>();
        }
        //------------------------------------------------------
        //プレイヤーの攻撃アニメーションを再生させるかどうか
        //------------------------------------------------------
        public void Attack(StarterAssetsInputs _input)
        {
            if (!_input.attack)
                return;

            _input.attack = false;
            //アッタクモーション
            _playerAnimation.PlayerAttackAnimator();
            
        }
        //------------------------------------------------------
        //プレイヤーの攻撃があたった場合の(ダメージ、属性)
        //------------------------------------------------------
        public void AttackHit()
        {
         

        }


        //------------------------------------------------------
        //プレイヤーの攻撃アニメーションが終わるときに呼び出される
        //------------------------------------------------------
        public void ColliderRemove()
        {
           
        }
    }
}


