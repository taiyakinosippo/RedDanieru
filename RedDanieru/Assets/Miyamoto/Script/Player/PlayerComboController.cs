using UnityEngine;

namespace Player
{
    /// <summary>
    /// コンボ段数とコンボ入力、攻撃終了後の待機を管理する
    /// </summary>
    public class PlayerComboController : MonoBehaviour
    {
        [Header("コンボ設定")]
        [SerializeField] private float thirdAttackActionStop = 0.5f;

        public int ComboIndex { get; private set; } = 0;
        public bool IsAttacking { get; private set; }
        public bool IsComboInputWindowOpen { get; private set; }
        public bool HasQueuedComboInput { get; private set; }
        public bool IsActionLocked { get; private set; }

        public bool IsFirstAttack => ComboIndex == 0;

        private float _actionStopTimer;

        private void Update()
        {
            if (!IsActionLocked)
            {
                return;
            }

            _actionStopTimer += Time.deltaTime;

            if (_actionStopTimer >= thirdAttackActionStop)
            {
                IsActionLocked = false;
                _actionStopTimer = 0f;
            }
        }

        /// <summary>
        /// 新しいコンボを開始する
        /// </summary>
        public bool StartCombo()
        {
            if (IsActionLocked || IsAttacking)
            {
                return false;
            }

            ComboIndex = 0;
            IsAttacking = true;
            IsComboInputWindowOpen = false;
            HasQueuedComboInput = false;

            return true;
        }

        /// <summary>
        /// 攻撃開始時にコンボ入力受付を閉じる
        /// </summary>
        public void BeginCurrentAttack()
        {
            IsAttacking = true;
            IsComboInputWindowOpen = false;
        }

        /// <summary>
        /// コンボ入力を受け付ける
        /// </summary>
        public void TryQueueComboInput(int attackCount)
        {
            if (!IsAttacking || !IsComboInputWindowOpen)
            {
                return;
            }

            if (ComboIndex >= attackCount - 1)
            {
                return;
            }

            HasQueuedComboInput = true;
        }

        /// <summary>
        /// アニメーションイベントからコンボ入力受付を開く
        /// </summary>
        public void OpenComboInputWindow()
        {
            if (IsAttacking)
            {
                IsComboInputWindowOpen = true;
            }
        }

        /// <summary>
        /// コンボ入力受付を閉じる
        /// </summary>
        public void CloseComboInputWindow()
        {
            IsComboInputWindowOpen = false;
        }

        /// <summary>
        /// 現在の攻撃終了後、次の攻撃へ進めるか判定する
        /// </summary>
        public bool TryAdvanceCombo(int attackCount)
        {
            IsComboInputWindowOpen = false;

            if (HasQueuedComboInput && ComboIndex < attackCount - 1)
            {
                ComboIndex++;
                HasQueuedComboInput = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// コンボを終了する
        /// </summary>
        public void FinishCombo()
        {
            bool wasThirdAttack = ComboIndex == 2;

            IsAttacking = false;
            IsComboInputWindowOpen = false;
            HasQueuedComboInput = false;
            ComboIndex = 0;

            if (wasThirdAttack && thirdAttackActionStop > 0f)
            {
                IsActionLocked = true;
                _actionStopTimer = 0f;
            }
        }

        /// <summary>
        /// 攻撃キャンセル時にコンボ状態を初期化する
        /// </summary>
        public void CancelCombo()
        {
            IsAttacking = false;
            IsComboInputWindowOpen = false;
            HasQueuedComboInput = false;
            ComboIndex = 0;
            IsActionLocked = false;
            _actionStopTimer = 0f;
        }

    }
}