using UnityEngine;

public class StickerEffectManager : MonoBehaviour
{
    public static StickerEffectManager Instance { get; private set; }

    [SerializeField] private GameObject explosionEffect;

    public GameObject ExplosionEffect => explosionEffect;

    private void Awake()
    {
        Instance = this;
    }
}
