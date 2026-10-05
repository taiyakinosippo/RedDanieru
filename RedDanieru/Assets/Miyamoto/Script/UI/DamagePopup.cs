using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    private TextMeshProUGUI _text;

    [SerializeField]
    private Vector2 offset = Vector2.zero;

    public float _lifeTime = 4f;

    private RectTransform _rectTransform;
    private Vector3 _transform;
    private Camera _mainCamera;

    // DamageSetUpが呼ばれたか
    private bool _initialized = false;

    // 残り時間
    private float _timer;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        // DamageSetUp前は何もしない
        if (!_initialized)
        {
            return;
        }

        // ダメージ表示の位置を敵の位置に合わせる
        if (_mainCamera != null)
        {
            _rectTransform.position =
                RectTransformUtility.WorldToScreenPoint(
                    _mainCamera,
                    _transform + (Vector3)offset
                );
        }

        // Time.timeScaleが0でも時間を進める
        _timer -= Time.unscaledDeltaTime;

        if (_timer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    public void DamageSetUp(
        int damage,
        Vector3 position,
        Camera main
    )
    {
        _transform = position;
        _mainCamera = main;

        if (_text != null)
        {
            _text.text = damage.ToString();

            // ダメージの強さによって色を変える
            if (damage < 10)
            {
                _text.color = Color.white;
            }
            else if (damage < 20)
            {
                _text.color = Color.yellow;
            }
            else
            {
                _text.color = Color.red;
            }
        }

        // 表示時間を設定
        _timer = _lifeTime;

        // 初期化完了
        _initialized = true;
    }
}