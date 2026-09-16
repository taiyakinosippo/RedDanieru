using UnityEngine;

public class DustEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem dustParticle;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask targetLayer;

    private ParticleSystem.EmissionModule emission;

    void Start()
    {
        emission = dustParticle.emission;
    }

    void Update()
    {
        // 左クリックを押している間
        if (Input.GetMouseButton(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, targetLayer))
            {
                // マウスが当たった場所へ砂埃を移動
                dustParticle.transform.position = hit.point;

                // 新しい砂埃を発生させる
                emission.enabled = true;
            }
            else
            {
                // 対象Layer以外なら砂埃を出さない
                emission.enabled = false;
            }
        }
        else
        {
            // 左クリックを離したら砂埃を出さない
            emission.enabled = false;
        }
    }
}