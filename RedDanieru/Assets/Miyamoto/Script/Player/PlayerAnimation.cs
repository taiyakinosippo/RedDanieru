using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Windows;

/// <summary>
/// アニメーションを管理するためのスクリプト
/// </summary>
namespace Player
{
    public class PlayerAnimation : MonoBehaviour
    {
        // アニメーションのIDを格納する変数
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDAttack;
        private int _animIDStickerPaste;
        private int _animIDStickerPeelOff;

        private Animator _animator;                      // アニメーションを制御するためのAnimatorコンポーネント

        private bool _hasAnimator;                       // Animatorコンポーネントが存在するかどうかを判定するための変数
        private PlayerInputPriority _actionPriority;
        private StickerCheck        _stickerCheck;
        private NetworkMecanimAnimator _networkAnimator;

        //----------------------------------------------------------
        //初期化
        //----------------------------------------------------------
        private void Start()
        {
            // アニメーションの有無を判定し、Animatorコンポーネントを取得
            _hasAnimator = TryGetComponent(out _animator);

            //アニメーションを使用する
            _animator = GetComponent<Animator>();

            _actionPriority = GetComponent<PlayerInputPriority>();

            _stickerCheck = GetComponent<StickerCheck>();

            _networkAnimator = GetComponent<NetworkMecanimAnimator>();
            //アニメーションをIDに変換
            AssignAnimationIDs();
        }

        //----------------------------------------------------------
        //Animatorコンポーネントを取得しているかの判定
        //----------------------------------------------------------
        public void AnimatorComPonent()
        {
            _hasAnimator = TryGetComponent(out _animator);
        }

        //-----------------------------------------------------------
        //アニメーションのIDを取得する関数
        //-----------------------------------------------------------
        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDAttack = Animator.StringToHash("Attack");
            _animIDStickerPaste = Animator.StringToHash("StickerPaste");
            _animIDStickerPeelOff = Animator.StringToHash("StickerPeelOff");

        }

        //----------------------------------------------------------
        //地面についているかそうではないのかを判定するtrueになった瞬間に着地のアニメーションを再生させる
        //----------------------------------------------------------
        public void GroundedCheckAnimator(bool Grounded)
        {
            // アニメーションのパラメータを更新するfalseならアニメーションのパラメータを更新しない
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        //----------------------------------------------------------
        //プレイヤーの動き（止めっている状態、歩き、走る）アニメーション再生
        //----------------------------------------------------------
        public void PlayerMoveAnimatior(float _animationBlend, float inputMagnitude)
        {
            // アニメーションのブレンド値を更新する
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        //----------------------------------------------------------
        //プレイヤーのジャンプや落下のアニメーション側の判定を初期化する
        //----------------------------------------------------------
        public void PlayerJumpAnimatorFalse()
        {
            if (_hasAnimator)
            {
                // ここでアニメーションをジャンプと落下のフラグをfalseにする
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }
        }

        //----------------------------------------------------------
        //プレイヤーのジャンプのアニメーション再生させる
        //----------------------------------------------------------
        public void PlayerJumpAnimtor()
        {
            // アニメーションのジャンプのフラグをtrueにする
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, true);
            }
        }

        //----------------------------------------------------------
        //プレイヤーの落下のアニメーションを再生させる
        //----------------------------------------------------------
        public void PlayerfallAnimatorFall()
        {
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDFreeFall, true);
            }
        }

        //----------------------------------------------------------
        //プレイヤーのアッタクアニメーションを再生させる
        //----------------------------------------------------------
        public void PlayerAttackAnimator()
        {
            if (_hasAnimator)
            {
                _animator.SetTrigger(_animIDAttack);
            }
        }

        //----------------------------------------------------------
        //プレイヤーのアッタクアニメーションが終了すると勝手に呼ばれる
        //----------------------------------------------------------
        public void OnAttackAnimationEnd()
        {
            _animator.ResetTrigger(_animIDAttack);
            Debug.Log("Attack animation end");
            _actionPriority.EndAction();
        }

        //----------------------------------------------------------
        //プレイヤーのステッカーを貼るときのアニメーション
        //----------------------------------------------------------
        public void PlayerStickerPasteAnimator()
        {
            if (_hasAnimator)
            {
                _animator.SetTrigger(_animIDStickerPaste);
            }
        }

        //----------------------------------------------------------
        //プレイヤーのステッカーを貼るアニメーションが終了したときに呼ばれる
        //----------------------------------------------------------
        public void OnStickerPasteAnimationEnd()
        {
            _animator.ResetTrigger(_animIDStickerPaste);
            Debug.Log("Sticker Paste animation end");
            _actionPriority.EndAction();
        }

        //----------------------------------------------------------
        //プレイヤーのステッカーをはがすときのアニメーション
        //----------------------------------------------------------
        public void StickerPeelOffAnimation()
        {
            if (_hasAnimator)
            {
                Debug.Log("はがすアニメーション再生");
                _animator.SetTrigger(_animIDStickerPeelOff);
            }
        }

        //----------------------------------------------------------
        //プレイヤーのステッカーをはがすアニメーションが終了したときに呼ばれる
        //----------------------------------------------------------
        public void OnStickerPeelOffAnimationEnd()
        {
            _animator.ResetTrigger(_animIDStickerPeelOff);
            Debug.Log("Sticker Peel off animation end");
            _stickerCheck.isStickerDected = false;
            _actionPriority.EndAction();
        }


        public void OnFootstep(AnimationEvent animationEvent)
        {
            // 足音を鳴らしたいならここに書く
        }

        public void OnLand(AnimationEvent animationEvent)
        {
            // 着地音
        }


    }
}
