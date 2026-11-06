using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class PlayerInputPriority : MonoBehaviour
    {
        //行動管理

        // 現在実行中のアクション
        public ActionType currentActionType {  get; private set; } = ActionType.None;

        // 新しく押されたアクション
        private ActionType nextActionType = ActionType.None;

        // 待機中のアクション
        private Queue<ActionType> actionQueue = new Queue<ActionType>();

        // 攻撃中に回避入力があったかどうか
        public bool IsCancelAttackByEvade { get; private set; } = false;

        private PlayerAttack _playerAttack;
        private void Start()
        {
            _playerAttack = GetComponent<PlayerAttack>();
        }


        //-----------------------------------------------------------
        //入力を取得する関数
        //-----------------------------------------------------------
        public void CheckInput(StarterAssetsInputs _input, bool Grounded)
        {
            // 空中で押した攻撃は無効
            if (!Grounded)
            {
                _input.attack = false;
                _input.sticker = false;
                _input.evade = false;
            }

            // 攻撃中は移動できないようにする
            if (currentActionType == ActionType.Attack)
            {
                // 移動入力を無効にする
                _input.move = Vector2.zero;

                // 1段目の攻撃中は回避でキャンセルできる
                if (_playerAttack.IsFirstAttack && _input.evade && Grounded)
                {
                    _playerAttack.CancelAttack();

                    currentActionType = ActionType.Evade;
                    IsCancelAttackByEvade = true;

                    actionQueue.Clear();
                    _input.attack = false;

                    return;
                }

                // 攻撃中のその他の入力は受け付けない
                _input.jump = false;
                _input.sticker = false;
                actionQueue.Clear();

                return;
            }

            // 歩行中・回避中は攻撃入力を受け付けない
            if (currentActionType == ActionType.Move ||
                currentActionType == ActionType.Evade)
            {
                _input.attack = false;
            }

            // 攻撃中以外は中断フラグを解除する
            IsCancelAttackByEvade = false;

            if (_input.attack && Grounded)
            {
                AddAction(ActionType.Attack);
            }

            if (_input.jump)
            {
                AddAction(ActionType.Jump);
            }

            if (_input.move != Vector2.zero)
            {
                AddAction(ActionType.Move);
            }

            if (_input.evade && Grounded)
            {
                AddAction(ActionType.Evade);
            }

            if (_input.sticker && Grounded)
            {
                AddAction(ActionType.Sticker);
            }

        }


        //-----------------------------------------------------------
        //アクションの優先度を取得する関数
        //-----------------------------------------------------------
        private int GetPriority(ActionType action)
        {
            switch (action)
            {
                case ActionType.Jump:
                    return 100;

                case ActionType.Evade:
                    return 90;

                case ActionType.Sticker:
                    return 80;

                case ActionType.Attack:
                    return 70;

                //case ActionType.CameraChange:
                //    return 30;

                case ActionType.Move:
                    return 10;

                case ActionType.None:
                    return 0;
            }

            return 0;
        }


        //--------------------------------------------------------------
        //優先度の高いアクションを優先する
        //--------------------------------------------------------------

        private ActionType AddAction(ActionType action)
        {
            // 今押されたアクション
            nextActionType = action;

            // 現在の行動より優先度が高いなら
            if (GetPriority(nextActionType) > GetPriority(currentActionType))
            {
                // 待機中を全部捨てる
                actionQueue.Clear();

                // 現在の行動を切り替える
                currentActionType = nextActionType;
            }

            else
            {
                // 優先度が低いので待機
                actionQueue.Enqueue(nextActionType);
            }

            return currentActionType;
        }

        //--------------------------------------------------------------
        //アクションをもとに戻す
        //--------------------------------------------------------------
        public void EndAction()
        {
            currentActionType = ActionType.None;
        }
    }
}
