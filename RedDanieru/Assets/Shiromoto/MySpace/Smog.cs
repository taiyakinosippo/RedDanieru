using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class DustEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem dustParticle;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask targetLayer;

    private ParticleSystem.EmissionModule emission;

    private void Awake()
    {
        if (dustParticle != null)
        {
            emission = dustParticle.emission;
        }
    }

    private void Start()
    {
        SetupEffect();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetupEffect();
    }

    private void SetupEffect()
    {
        mainCamera = Camera.main;

        if (dustParticle == null)
            return;

        emission = dustParticle.emission;

        // Testplayでは完全停止
        if (IsTestPlayScene())
        {
            emission.enabled = false;

            dustParticle.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }
        else
        {
            // 編集シーンではParticleSystemを再生可能な状態に戻す
            emission.enabled = false;

            if (!dustParticle.isPlaying)
            {
                dustParticle.Play();
            }
        }
    }

    private void Update()
    {
        // Testplayでは完全に処理しない
        if (IsTestPlayScene())
            return;

        if (dustParticle == null)
            return;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
                return;
        }

        // 左クリックを押している間
        if (Input.GetMouseButton(0))
        {
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
            {
                emission.enabled = false;
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
                Vector3 pos = hit.point;

                // 高さを固定
                pos.y = 2.5f;

                dustParticle.transform.position = pos;

                emission.enabled = true;
            }
            else
            {
                emission.enabled = false;
            }
        }
        else
        {
            emission.enabled = false;
        }
    }

    private bool IsTestPlayScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        return sceneName == "Testplay" ||
               sceneName == "Takeshita_Matching";
    }
}