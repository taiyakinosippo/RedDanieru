using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager_Takeshita : MonoBehaviour
{
    public static BGMManager_Takeshita Instance;

    [SerializeField]
    private AudioSource bgmSource;

    // タイトルで流す元のBGM
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

        defaultClip = bgmSource.clip;
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

    public void PlayBGM()
    {
        if (!bgmSource.isPlaying)
        {
            bgmSource.Play();
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    // BGMを差し替えて再生する
    public void ChangeBGM(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    // タイトルのBGMに戻して最初から再生する
    public void ResetBGM()
    {
        ChangeBGM(defaultClip);
    }
}
