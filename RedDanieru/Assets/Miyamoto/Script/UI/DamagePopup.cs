using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [SerializeField]private TextMeshProUGUI _text;  //ダメージを表記させるテキスト
    public float _lifeTime = 1f;　　　　　　　　　　//ダメージを表記する時間の長さ
    
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

        //時間になったら表示を消す
        Destroy(gameObject, _lifeTime);
    }
}
