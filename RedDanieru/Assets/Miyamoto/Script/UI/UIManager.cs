using Player;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class UIManager : MonoBehaviour
{
    [Header("ダメージ関係のUI")]
    [SerializeField] private GameObject _damageUI;
    [SerializeField] private Camera _mainCamera;
    private GameObject _canvas;
    private PlayerUI _playerUI;
    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        // プレイヤーごとに1つずつあるので、2つ目以降をDestroyしてはいけない
        // （マルチで他の人のキャラが先に出てくると、自分のHPバーごと消えてしまっていた）
        if (Instance == null)
        {
            Instance = this;
        }

        _playerUI = GetComponent<PlayerUI>();
        _canvas = GetComponent<Canvas>().gameObject;
    }

    // マルチで自分が操作するキャラのUIを使うようにする（NetworkAuthorityControllerから呼ぶ）
    public static void SetLocal(UIManager uiManager)
    {
        Instance = uiManager;
    }

    private void OnDisable()
    {
        // 他の人のキャラのUIは無効化されるので、そちらを使わないようにする
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void ShowDamage(int damage, Vector3 position)
    {
        // ダメージUIを生成
        GameObject ui = Instantiate(_damageUI, _canvas.transform);

        // UIを表示
        ui.SetActive(true);

        // 敵の周辺に少しランダムな位置を作る
        Vector2 circlePos = Random.insideUnitCircle * 0.5f;

        Vector3 damagePosition = position + Vector3.up * Random.Range(1f, 2f) + new Vector3(circlePos.x, 0f, circlePos.y);

        // ワールド座標 → スクリーン座標
        Vector2 screenPosition =RectTransformUtility.WorldToScreenPoint(_mainCamera,damagePosition);

        // UIの位置に設定
        ui.GetComponent<RectTransform>().position = screenPosition;

        // ダメージ値を設定
        ui.GetComponent<DamagePopup>().DamageSetUp(damage, position, _mainCamera);
    }
}
