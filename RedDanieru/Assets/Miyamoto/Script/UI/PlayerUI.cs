using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public TextMeshProUGUI _dieText;

    public PlayerStatus _status;

    public PlayerCamera _playerCamera;

    private void Start()
    {
        _dieText.enabled = false;

        Debug.Log("Status = " + _status);
    }

    private void Update()
    {
        if (_status._isDead)
        {
            _dieText.enabled = true;
            _playerCamera.PlayerDiedCamera();
            Debug.Log("死亡UI表示");
        }
    }
}

