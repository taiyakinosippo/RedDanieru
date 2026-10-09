using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class DustEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem dustParticle;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask targetLayer;

    [Header("マウス移動速度の判定（画面の高さ/秒）")]
    // この速さを超えたら砂埃を出し始める
    [SerializeField] private float startSpeed = 0.8f;
    // この速さを下回ったら砂埃を止める（startSpeedより小さくしてチラつきを防ぐ）
    [SerializeField] private float stopSpeed = 0.4f;
    // 速度のなめらかさ（大きいほど反応が早い）
    [SerializeField] private float speedSmoothing = 10f;

    private ParticleSystem.EmissionModule emission;

    private Vector3 lastMousePosition;
    private float smoothedSpeed;
    private bool isMovingFast;

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

        // クリックした瞬間は速度をリセット（その場でクリックしただけでは出さない）
        if (Input.GetMouseButtonDown(0))
        {
            ResetMouseSpeed();
        }

        // 左クリックを押している間
        if (Input.GetMouseButton(0))
        {
            UpdateMouseSpeed();

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

                // マウスを大きく動かしている時だけ出す（止まっている・細かい編集中は出さない）
                emission.enabled = isMovingFast;
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

    private void ResetMouseSpeed()
    {
        lastMousePosition = Input.mousePosition;
        smoothedSpeed = 0f;
        isMovingFast = false;
    }

    private void UpdateMouseSpeed()
    {
        Vector3 mousePosition = Input.mousePosition;
        float deltaTime = Time.unscaledDeltaTime;

        if (deltaTime > 0f && Screen.height > 0)
        {
            // 解像度に左右されないよう画面の高さで割る
            float speed =
                (mousePosition - lastMousePosition).magnitude
                / Screen.height
                / deltaTime;

            smoothedSpeed = Mathf.Lerp(
                smoothedSpeed,
                speed,
                1f - Mathf.Exp(-speedSmoothing * deltaTime)
            );
        }

        lastMousePosition = mousePosition;

        if (isMovingFast)
        {
            if (smoothedSpeed < stopSpeed)
                isMovingFast = false;
        }
        else if (smoothedSpeed > startSpeed)
        {
            isMovingFast = true;
        }
    }

    private bool IsTestPlayScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        return sceneName == "Testplay" ||
               sceneName == "Takeshita_Matching";
    }
}