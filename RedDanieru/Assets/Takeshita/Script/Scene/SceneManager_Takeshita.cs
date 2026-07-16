using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager_Takeshita: MonoBehaviour
{
    public void GameStartButton()
    {
        SceneManager.LoadScene("Test_Load");
    }
}
