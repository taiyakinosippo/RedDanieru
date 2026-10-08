using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager_Takeshita : MonoBehaviour
{
    public static BGMManager_Takeshita Instance;

    [SerializeField]
    private AudioSource normalBgmSource;

    [SerializeField]
    private AudioSource battleBgmSource;

    // タイトルで流す元の通常BGM
    private AudioClip defaultClip;

    // 直前にいたシーン名
    private string currentSceneName;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        if (normalBgmSource != null)
        {
            defaultClip = normalBgmSource.clip;
        }

        currentSceneName = SceneManager.GetActiveScene().name;
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
        // 破棄予定の重複オブジェクトは処理しない
        if (Instance != this)
        {
            return;
        }

        string previousSceneName = currentSceneName;
        currentSceneName = scene.name;

        // ダンジョン制作シーン(テストプレイ含む)からタイトルに戻ったらBGMをリセット
        if (scene.name == "TitleScene" &&
            (previousSceneName == "Katayama_ren" || previousSceneName == "Testplay"))
        {
            ResetBGM();
        }
    }

    // 通常BGMを差し替えて最初から再生する
    public void ChangeBGM(AudioClip clip)
    {
        if (normalBgmSource == null || clip == null)
        {
            return;
        }

        StopBattleBGM();

        normalBgmSource.Stop();
        normalBgmSource.clip = clip;
        normalBgmSource.Play();
    }

    // タイトルの通常BGMに戻して最初から再生する
    public void ResetBGM()
    {
        ChangeBGM(defaultClip);
    }

    public void PlayNormalBGM()
    {
        StopBattleBGM();

        if (normalBgmSource != null)
        {
            normalBgmSource.Stop();
            normalBgmSource.Play();
        }

        Debug.Log("PlayNormalBGM再生中");
    }

    public void PlayBattleBGM()
    {
        StopNormalBGM();
        if (normalBgmSource != null)
        {
            normalBgmSource.Stop();
        }

        if (battleBgmSource != null)
        {
            battleBgmSource.Play();
        }
        Debug.Log("PlayButtleBGM再生中");
    }

    public void StopNormalBGM()
    {
        if (normalBgmSource != null)
        {
            normalBgmSource.Stop();
        }
        Debug.Log("PlayNormalBGM停止中");
    }

    public void StopBattleBGM()
    {
        if (battleBgmSource != null)
        {
            battleBgmSource.Stop();
        }
        Debug.Log("PlayButtleBGM停止中");
    }

    public void PlayTitleBGM()
    {
        if (normalBgmSource != null)
        {
            normalBgmSource.Stop();
        }

        if (battleBgmSource != null)
        {
            battleBgmSource.Stop();
        }

        if (normalBgmSource != null)
        {
            normalBgmSource.clip = defaultClip;
            normalBgmSource.Play();
        }
    }

    public void StopAllBGM()
    {
        StopNormalBGM();
        StopBattleBGM();
    }
}