using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class DustEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem dustParticle;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask targetLayer;

    private ParticleSystem.EmissionModule emission;

    void Start()
    {
        mainCamera = Camera.main;
        emission = dustParticle.emission;

        // テストプレイ中は砂埃を再生しない
        if (IsTestPlayScene())
        {
            emission.enabled = false;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mainCamera = Camera.main;

        // テストプレイ中は砂埃を再生しない
        if (IsTestPlayScene())
        {
            if (dustParticle != null)
            {
                dustParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            if (dustParticle != null)
            {
                emission = dustParticle.emission;
                emission.enabled = false;
            }
        }
    }

    void Update()
    {
        // テストプレイ中は処理しない
        if (IsTestPlayScene())
        {
            if (dustParticle != null)
            {
                emission.enabled = false;
            }

            return;
        }

        // 左クリックを押している間
        if (Input.GetMouseButton(0))
        {
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;

                if (mainCamera == null)
                    return;
            }

            Ray ray =
                mainCamera.ScreenPointToRay(
                    Input.mousePosition
                );

            if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                100f,
                targetLayer))
            {
                // 高さを固定
                Vector3 pos = hit.point;
                pos.y = 2.5f;

                // マウスが当たった場所へ砂埃を移動
                dustParticle.transform.position = pos;

                // 砂埃を発生させる
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

    private bool IsTestPlayScene()
    {
        return SceneManager.GetActiveScene().name == "Testplay";
    }
}