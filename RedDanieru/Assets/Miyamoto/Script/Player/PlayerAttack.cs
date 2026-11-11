using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// 攻撃開始と、各攻撃機能の連携を管理する
    /// </summary>
    [RequireComponent(typeof(PlayerComboController))]
    [RequireComponent(typeof(PlayerAttackHitbox))]
    public class PlayerAttack : MonoBehaviour
    {
        [Header("攻撃データ")]
        [SerializeField] private List<PlayerAttackData> attackData = new();

        [Header("攻撃位置")]
        [SerializeField] public Transform _attackPoint;

        private PlayerAnimation _playerAnimation;
        private PlayerStatus _playerStatus;
        private PlayerInputPriority _actionPriority;

        private PlayerComboController _comboController;
        private PlayerAttackHitbox _attackHitbox;

        private PlayerAttackData _currentAttackData;

        private bool _isAttackHitActive;

        // コンボ入力受付時間の計測
        private float _comboElapsedTime;

        // コンボ入力受付時間を計測中か
        private bool _comboTimerActive;

        public bool IsFirstAttack => _comboController != null && _comboController.IsFirstAttack;

        private bool _isAttackAnimationEnded;
        private bool _isComboInputDeadlineEnded;

        private void Awake()
        {

            _playerAnimation = GetComponent<PlayerAnimation>();
            _playerStatus = GetComponent<PlayerStatus>();
            _actionPriority = GetComponent<PlayerInputPriority>();
            _comboController = GetComponent<PlayerComboController>();
            _attackHitbox = GetComponent<PlayerAttackHitbox>();

        }

        private void FixedUpdate()
        {
            if (_isAttackHitActive)
            {
                AttackHit();
            }

            UpdateComboInputTimer();
        }

        /// <summary>
        /// 攻撃入力を処理する
        /// </summary>
        public void Attack(StarterAssetsInputs input)
        {
            if (input == null || !input.attack)
            {
                return;
            }

            // 入力を消費し、押しっぱなしで再入力されないようにする
            input.attack = false;

            if (_comboController == null)
            {
                return;
            }

            // 3段目終了後などのアクション制限中は受け付けない
            if (_comboController.IsActionLocked)
            {

                return;
            }

            // 攻撃中なら、コンボ入力として受付を試みる
            if (_comboController.IsAttacking)
            {
                _comboController.TryQueueComboInput(attackData.Count);
                return;
            }

            if (!_comboController.StartCombo())
            {
                return;
            }

            BeginAttack();
        }

        /// <summary>
        /// 現在のコンボ段数の攻撃を開始する
        /// </summary>
        private void BeginAttack()
        {
            int comboIndex = _comboController.ComboIndex;

            if (comboIndex < 0 || comboIndex >= attackData.Count)
            {
                Debug.LogError("PlayerAttack：コンボ段数に対応する攻撃データがありません。", this);
                CancelAttack();
                return;
            }

            _comboController.BeginCurrentAttack();

            _isAttackHitActive = false;
            _currentAttackData = attackData[comboIndex];

            _attackHitbox.BeginHit();

            _comboElapsedTime = 0f;
            _comboTimerActive = true;

            _isAttackAnimationEnded = false;
            _isComboInputDeadlineEnded = false;

            Debug.Log($"{comboIndex + 1}段目の攻撃を開始");

            if (_playerAnimation != null)
            {
                _playerAnimation.PlayerAttackAnimator(comboIndex);
            }
            else
            {
                Debug.LogError("PlayerAttack：PlayerAnimation が見つかりません。", this);
                CancelAttack();
            }
        }

        private void UpdateComboInputTimer()
        {
            if (!_comboTimerActive || _currentAttackData == null)
                return;

            _comboElapsedTime += Time.deltaTime;

            float startDelay = _currentAttackData.comboInputStartDelay;
            float duration = _currentAttackData.comboInputDuration;

            // 入力受付開始
            if (!_comboController.IsComboInputWindowOpen && _comboElapsedTime >= startDelay)
            {
                _comboController.OpenComboInputWindow();
                Debug.Log("コンボ入力受付開始");
            }

            // 入力受付終了
            if (_comboElapsedTime >= startDelay + duration)
            {
                _comboController.CloseComboInputWindow();

                _comboTimerActive = false;

                _isComboInputDeadlineEnded = true;

                _isAttackAnimationEnded = true;

                Debug.Log("コンボ入力受付終了");

                TryCompleteAttack();
            }
        }

        private void TryCompleteAttack()
        {
            if (!_isAttackAnimationEnded)
                return;

            if (!_isComboInputDeadlineEnded)
                return;

            if (_comboController.TryAdvanceCombo(attackData.Count))
            {
                BeginAttack();
                return;
            }

            float duration = _currentAttackData.comboInputDuration;

            _currentAttackData = null;

            _comboController.FinishCombo(duration);

            _actionPriority?.EndAction();
        }

        /// <summary>
        /// 攻撃判定を実行する
        /// </summary>
        public void AttackHit()
        {
            if (!_isAttackHitActive ||
                _currentAttackData == null ||
                _attackHitbox == null)
            {
                return;
            }

            _attackHitbox.AttackHit(
                _attackPoint,
                _currentAttackData,
                _playerStatus);
        }
        
        /// <summary>
        /// アニメーションイベントから呼び出す攻撃判定の開始
        /// </summary>
        public void StartAttackHit()
        {
            _isAttackHitActive = true;
        }


        /// <summary>
        /// アニメーションイベントから呼び出すコンボ入力受付の開始
        /// </summary>
        public void OpenComboInputWindow()
        {
            _comboController?.OpenComboInputWindow();
        }


        /// <summary>
        /// アニメーションイベントから呼び出すコンボ入力受付の終了
        /// </summary>
        public void CloseComboInputWindow()
        {
            _comboController?.CloseComboInputWindow();
        }


        /// <summary>
        /// 攻撃アニメーション終了時に呼び出す
        /// </summary>
        public void EndAttackHit()
        {
            _isAttackHitActive = false;
            _attackHitbox?.EndHit();
        }

        //public void AttackAnimationEnd()
        //{
        //    Debug.Log("攻撃アニメーション終了");

        //    _isAttackAnimationEnded = true;

        //    TryCompleteAttack();
        //}


        /// <summary>
        /// 回避などによって攻撃を中断する
        /// </summary>

        public void CancelAttack()
        {
            _isAttackHitActive = false;
            _currentAttackData = null;

            _attackHitbox?.EndHit();

            // コンボの進行と予約をリセットする
            _comboController?.CancelCombo();

            Debug.Log("攻撃を中断しました");
        }

    }
}