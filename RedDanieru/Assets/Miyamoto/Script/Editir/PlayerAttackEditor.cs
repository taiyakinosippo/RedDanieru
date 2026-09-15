using Player;
using System.Drawing;
using UnityEditor;
using UnityEngine;
using static UnityEditor.FilePathAttribute;

/// <summary>
/// 攻撃の当たり判定などをeditor上で可視化して編集出来るようにするためのスクリプト
/// </summary>
[CustomEditor(typeof(PlayerDebug))]
public class PlayerAttackEditor : Editor
{
    private void OnSceneGUI()
    {
        PlayerDebug _playerDebug = (PlayerDebug)target;

        //編集対象の攻撃データを取得
        PlayerAttackData _attackData = _playerDebug.GetDebugAttackData();

        //調べたい攻撃データがなければ処理を抜ける
        if (_attackData == null) return;

        PlayerAttack _playerAttack = _playerDebug.GetComponent<PlayerAttack>();

        if (_playerAttack == null) return;
        Transform _attackPoint = _playerAttack._attackPoint;

        Quaternion _attackRotation = _attackPoint.rotation;

        Vector3 _size = _attackData.playerAttackRadius;

        //攻撃の当たり判定の中心位置を計算
        Vector3 _center = _attackPoint.position + _attackRotation * _attackData.attackOffset;

        // 攻撃判定の回転
        Quaternion _rotation = _attackRotation * Quaternion.Euler(_attackData.attackRotation);


        //-----------------------------------
        //攻撃の当たり判定を可視化する
        //-----------------------------------

        //前の描画行列を保存して、初期状態に戻すことで、プレイヤーの回転に合わせて当たり判定を描画する
        Matrix4x4 _oldMatrix = Handles.matrix;

        //攻撃判定の位置・回転・スケールを設定
        Handles.matrix = Matrix4x4.TRS(_center, _rotation, _size * 2.0f);
        // 実際にCubeを描画

        Handles.DrawWireCube( Vector3.zero, _size );

        //描画行列を元に戻す
        Handles.matrix = _oldMatrix;

        //-----------------------------------
        //当たり判定の移動・拡大縮小するためのコード
        //-----------------------------------

        //編集された際に動作を軽くするための関数（多分）
        EditorGUI.BeginChangeCheck();

        //当たり判定の位置を移動させる
        Vector3 _newCenter = Handles.PositionHandle(_center, _rotation);

        // 攻撃判定の回転
        Quaternion _newRotation =Handles.RotationHandle(_rotation,_center);

        //当たり判定を拡大縮小させる
        Vector3 _newSize = Handles.ScaleHandle(_size, _center, _rotation, HandleUtility.GetHandleSize(_center));
        if (EditorGUI.EndChangeCheck())
        {
            // Undoできるようにする
            Undo.RecordObject(
                _attackData,
                "Edit Attack Hitbox"
            );

            // ----------------------------
            // 移動した結果をattackOffsetに変換
            // ----------------------------

            _attackData.attackOffset = Quaternion.Inverse(_rotation) * (_newCenter - _attackPoint.position);

            //-----------------------------------
            // 回転をattackRotationに変換
            //-----------------------------------

            Quaternion localRotation =
                Quaternion.Inverse(_attackRotation) *
                _newRotation;

            _attackData.attackRotation =
                localRotation.eulerAngles;

            // ----------------------------
            // サイズを半分にする
            // ----------------------------

            Vector3 halfSize = _newSize * 0.5f;

            // マイナスにならないようにする
            halfSize.x = Mathf.Max(0.01f, Mathf.Abs(halfSize.x));
            halfSize.y = Mathf.Max(0.01f, Mathf.Abs(halfSize.y));
            halfSize.z = Mathf.Max(0.01f, Mathf.Abs(halfSize.z));

            _attackData.playerAttackRadius = halfSize;

            // ScriptableObjectを変更したことをUnityに知らせる
            EditorUtility.SetDirty(_attackData);

            // Sceneビューを更新
            SceneView.RepaintAll();
        }
    }
}


