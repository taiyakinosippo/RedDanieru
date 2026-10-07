using UnityEngine;

public class StickerEffectManager : MonoBehaviour
{
    public static StickerEffectManager Instance { get; private set; }

    [SerializeField] private GameObject explosionEffect;

    public GameObject ExplosionEffect => explosionEffect;

    [SerializeField] private GameObject chargeEffect;

    public GameObject ChargeEffect => chargeEffect;

    private void Awake()
    {
        Instance = this;
    }
}
