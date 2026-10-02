using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    private TextMeshProUGUI _text;  //ダメージを表記させるテキスト
    public float _lifeTime = 4f;          //ダメージを表記する時間の長さ

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    public void DamageSetUp(int damage)
    {

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
