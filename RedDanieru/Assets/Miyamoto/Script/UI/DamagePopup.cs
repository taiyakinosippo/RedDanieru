using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    private TextMeshProUGUI _text;  //ダメージを表記させるテキスト
    [SerializeField] private Vector2 offset = Vector2.zero;

    public float _lifeTime = 4f;          //ダメージを表記する時間の長さ
    private RectTransform _rectTransform; //ダメージの表記をするテキストのRectTransform
    private Vector3 _transform;           //ダメージの表記をするテキストのTransform
    private Camera _mainCamera;          //メインカメラのTransform


    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Update()
    {
        _rectTransform.position = RectTransformUtility.WorldToScreenPoint(_mainCamera, _transform + (Vector3)offset);
    }
    

    public void DamageSetUp(int damage, Vector3 position, Camera main)
    {
        _transform = position;
        _mainCamera = main;
        _text.text = damage.ToString();

        //ダメージの強さによって色を変える
        if(damage < 10)
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

        //時間になったら表示を消すd
        Destroy(gameObject, _lifeTime);

    }
}
