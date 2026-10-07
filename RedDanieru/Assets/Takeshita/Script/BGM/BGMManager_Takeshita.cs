using UnityEngine;

public class BGMManager_Takeshita : MonoBehaviour
{
    public static BGMManager_Takeshita Instance;

    [SerializeField]
    private AudioSource normalBgmSource;

    [SerializeField]
    private AudioSource battleBgmSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void PlayNormalBGM()
    {
        StopBattleBGM();

        if (normalBgmSource != null &&
            !normalBgmSource.isPlaying)
        {
            normalBgmSource.Play();
        }
    }

    public void PlayBattleBGM()
    {
        if (normalBgmSource != null)
        {
            normalBgmSource.Stop();
        }

        if (battleBgmSource != null)
        {
            battleBgmSource.Play();
        }
    }

    public void StopNormalBGM()
    {
        if (normalBgmSource != null)
        {
            normalBgmSource.Stop();
        }
    }

    public void StopBattleBGM()
    {
        if (battleBgmSource != null)
        {
            battleBgmSource.Stop();
        }
    }
}