using UnityEngine;

public class StickerEffectManager : MonoBehaviour
{
    public static StickerEffectManager Instance { get; private set; }

    [SerializeField] private GameObject explosionEffect;
    public GameObject ExplosionEffect => explosionEffect;

    [SerializeField] private GameObject chargeEffect;
    public GameObject ChargeEffect => chargeEffect;

    [SerializeField] private GameObject cactusPrefab;
    public GameObject CactusPrefab => cactusPrefab;

    //突進で壁やプレイヤーにぶつかってスタンしたときの頭の上の星
    [SerializeField] private GameObject stunStarEffect;     //スタン中ずっと回る星
    public GameObject StunStarEffect => stunStarEffect;

    [SerializeField] private GameObject stunStarHitEffect;  //ぶつかった瞬間にはじける星
    public GameObject StunStarHitEffect => stunStarHitEffect;

    [SerializeField] private float stunStarHeight = 0.3f;   //頭のてっぺんから星までの高さ
    public float StunStarHeight => stunStarHeight;

    [SerializeField] private float stunStarScale = 2.0f;    //スタン中に回る星の大きさ（1で元のプレハブのまま。回る輪も一緒に大きくなる）
    public float StunStarScale => stunStarScale;

    //竜化ステッカーの炎ブレス（Hun0FX の FX_Fire_06_Fan）
    [SerializeField] private GameObject fireBreathEffect;
    public GameObject FireBreathEffect => fireBreathEffect;

    private void Awake()
    {
        Instance = this;
    }
}
