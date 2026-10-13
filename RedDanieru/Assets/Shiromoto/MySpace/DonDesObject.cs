using UnityEngine;
using UnityEngine.SceneManagement;

public class DonDesObject : MonoBehaviour
{
    private static DonDesObject instance;
    [SerializeField] CreateStick createStick;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
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
        if (createStick == null) return;

        switch (scene.name)
        {
            case "TitleScene":
                createStick.ChangeMode(CreateStick.EffectPermission.OK);
                break;

            case "Takeshita_Matching":
            case "Katayama_ren":
                createStick.ChangeMode(CreateStick.EffectPermission.depends);
                break;

            /*case "":
                createStick.ChangeMode(CreateStick.EffectPermission.NG);
                break;*/

            default:
                break;
        }
    }
}
