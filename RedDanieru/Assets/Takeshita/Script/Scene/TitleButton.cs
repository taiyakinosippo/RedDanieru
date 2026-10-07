using UnityEngine;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleButton: MonoBehaviour
{
    [SerializeField] SceneManager_Takeshita sceneManager_Takeshita;

    // ダンジョンクリエイト画面で流すBGM（未設定ならBGMを止める）
    [SerializeField] AudioClip createSceneBGM;

    public void DungeonCreateButton()
    {
        
        SceneManager.LoadScene("Katayama_ren");
    }

    public void DungeonPlayButton()
    {
        SceneManager_Takeshita manager =
            FindObjectOfType<SceneManager_Takeshita>();

        if (manager != null)
        {
            manager.DungeonDownloadButton();
        }

        SceneManager.LoadScene("Takeshita_Matching");
    }
}
