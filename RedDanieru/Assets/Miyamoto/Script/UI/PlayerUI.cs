using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Unity.VisualScripting;

public class PlayerUI : MonoBehaviour
{

    public PlayerStatus _status;

    public PlayerCamera _playerCamera;

    public Slider _hpBar;

    private CursorController _cursorController;

    private void Start()
    {
        Debug.Log("PlayerUI Object = " +gameObject.name);
        Debug.Log("Root=" + transform.root.name);
        _cursorController = GetComponent<CursorController>();
    }

    public void initializePlayerState(int maxHp)
    {
        if (_hpBar != null)
        {
            _hpBar.maxValue = maxHp;
            _hpBar.value = maxHp;
        }
    }

    public void ChangeHp(int CurrentHp)
    {
       _hpBar.value = CurrentHp;
    }

    public void GameOver()
    {

       NetworkObject obj = GetComponentInParent<NetworkObject>();

        Debug.Log("死亡UI表示");

       if (!GameModeManager.IsMultiplayer)
       {
                FindObjectOfType<GameOverManager>()
                    ?.GameOver();
            // カーソルを表示する
            _cursorController.ShowCursor();
        }
        
    }
}

