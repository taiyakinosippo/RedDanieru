using Player;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class UIManager : MonoBehaviour
{
    [Header("ダメージ関係のUI")]
    [SerializeField] private GameObject _damageUI;
    [SerializeField] private GameObject _canvas;
    [SerializeField] private Camera _mainCamera;
    private PlayerUI _playerUI;

    private void Awake()
    {
        _playerUI = GetComponent<PlayerUI>();
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
        ui.GetComponent<DamagePopup>().DamageSetUp(damage);
    }
}
