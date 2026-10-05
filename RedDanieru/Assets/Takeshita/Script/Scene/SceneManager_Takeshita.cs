using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Text;

public class SceneManager_Takeshita : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField userNameInput;

    private string GenerateRandomUserName()
    {
        const string chars =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        StringBuilder sb =
            new StringBuilder();

        for (int i = 0; i < 4; i++)
        {
            sb.Append(
                chars[Random.Range(0, chars.Length)]
            );
        }

        int number =
            Random.Range(1000, 10000);

        return $"{sb}_{number}";
    }

    public void DungeonCreateButton()
    {
        SceneManager.LoadScene("Katayama_ren");
    }

    public void DungeonDownloadButton()
    {
        string userName = userNameInput.text.Trim();

        if (string.IsNullOrEmpty(userName))
        {
            userName = GenerateRandomUserName();
        }

        UserData.UserName = userName;

        Debug.Log(
            $"UserName:{UserData.UserName}"
        );

        SceneManager.LoadScene("Takeshita_Matching");
    }
}