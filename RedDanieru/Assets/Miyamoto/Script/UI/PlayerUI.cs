using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Unity.VisualScripting;

public class PlayerUI : MonoBehaviour
{
    public TextMeshProUGUI _dieText;

    public PlayerStatus _status;

    public PlayerCamera _playerCamera;

    public Slider _hpBar;

    private void Start()
    {
        _dieText.enabled = false;
        Debug.Log("PlayerUI Object = " +gameObject.name);
        Debug.Log("Root=" + transform.root.name);
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

       if (obj != null && obj.HasInputAuthority)
       {
                _dieText.enabled = true;
       }

       Debug.Log("死亡UI表示");

       if (!GameModeManager.IsMultiplayer)
       {
                FindObjectOfType<GameOverManager>()
                    ?.GameOver();
       }
        
    }
}

