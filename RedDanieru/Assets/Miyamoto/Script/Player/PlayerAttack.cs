using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

///<summry>
///プレイヤーの攻撃を制御するためのスクリプト
///</summry>
namespace StarterAssets
{
    public class PlayerAttack : MonoBehaviour
    {
        private PlayerAnimation _playerAnimation;

        void Start()
        {
            _playerAnimation = GetComponent<PlayerAnimation>();
        }
        //------------------------------------------------------
        //プレイヤーの攻撃関係の処理を書く関数(ダメージ、属性)
        //------------------------------------------------------
        public void Attack(StarterAssetsInputs _input)
        {

            if (!_input.attack)
                return;

            _input.attack = false;
            ///ここにダメージや属性を書く

            //アッタクモーション
            _playerAnimation.PlayerAttackAnimator();
            
        }
    }
}


