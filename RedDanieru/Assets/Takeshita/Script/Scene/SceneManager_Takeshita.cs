using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager_Takeshita: MonoBehaviour
{
    public void GameStartButton()
    {
        SceneManager.LoadScene("Test_Load");
    }

    public void DungeonCreateButton()
    {
        SceneManager.LoadScene("Katayama_ren");
    }

    public void DungeonDownloadButton()
    {
        SceneManager.LoadScene("Takeshita_Matching");
    }

}
