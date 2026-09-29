using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class PlayerUI : MonoBehaviour
{
    public TextMeshProUGUI _dieText;

    public PlayerStatus _status;

    public PlayerCamera _playerCamera;

    private bool _isProcessed;

    private bool _gameOverShown = false;

    private void Start()
    {
        _dieText.enabled = false;

        Debug.Log("PlayerUI Object = " +gameObject.name);
        Debug.Log("Root=" + transform.root.name);
    }

   private void Update()
{
    if (_isProcessed)
        return;

    if (_status._isDead)
    {
        _isProcessed = true;

            NetworkObject obj= GetComponentInParent<NetworkObject>();

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
}

