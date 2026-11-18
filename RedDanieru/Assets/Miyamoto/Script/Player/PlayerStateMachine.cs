using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    // 現在のステート
    private IPlayerState _currentState;

    //すべてのステートを保持する変数
    private Dictionary<State, IPlayerState> _states = new Dictionary<State, IPlayerState>();


}
