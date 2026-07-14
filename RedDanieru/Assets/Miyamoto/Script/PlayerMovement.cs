using StarterAssets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class PlayerMovement : MonoBehaviour
    {
        [Header("プレイヤーの設定")]
        [Tooltip("プレイヤーの歩く速度")]
        public float MoveSpeed = 2.0f;

        [Tooltip("プレイヤーの走る速度")]
        public float SprintSpeed = 5.335f;

        [Tooltip("キャラクターが移動方向を向く速度")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("加速減速の速さ")]
        public float SpeedChangeRate = 10.0f;

        [Space(10)]
        [Tooltip("ジャンプの高さ")]
        public float JumpHeight = 1.2f;

        [Tooltip("キャラクターが使用する重力値。エンジンのデフォルトは -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("ジャンプできるようになるまでに必要な時間。0fに設定すると即座にジャンプできる")]
        public float JumpTimeout = 0.50f;

        [Tooltip("落下アニメーションに入るまでの時間")]
        public float FallTimeout = 0.15f;

        [Tooltip("プレイヤーが地面にいるかどうか")]
        public bool Grounded = true;

        [Tooltip("プレイヤーが地面にいるかどうかの判定場所を少し下に下げる")]
        public float GroundedOffset = -0.14f;

        [Tooltip("プレイヤーが地面にいるかどうかの判定半径")]
        public float GroundedRadius = 0.28f;

        [Tooltip("どのレイヤーを地面として使用するか")]
        public LayerMask GroundLayers;

        // プレイヤーの基礎設定を格納する変数
        private float _speed;              // プレイヤーの現在の速度
        private float _animationBlend;                   // アニメーションへのブレンド値
        private float _targetRotation = 0.0f;            // プレイヤーの目標回転角度
        private float _rotationVelocity;                 // プレイヤーの左右速度
        private float _verticalVelocity;                 // プレイヤーの上下速度
        private float _terminalVelocity = 53.0f;     // 落下速度の上限値

        // 次のジャンプが出来る時間
        private float _jumpTimeoutDelta;
        // 落下アニメーションに入るまでの時間
        private float _fallTimeoutDelta;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private Animator _animator;           // アニメーションを制御するためのAnimatorコンポーネント
        private CharacterController _controller;         // プレイヤーの移動を制御するためのCharacterControllerコンポーネント
        private StarterAssetsInputs _input;              // プレイヤーの入力を制御するためのStarterAssetsInputsコンポーネント
 
        private void Update()
        {


        }
    }
}
