using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Text;

public class SceneManager_Takeshita : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField userNameInput;

    private void Start()
    {
        BGMManager_Takeshita.Instance.PlayBGM();

        if (userNameInput != null)
        {
            userNameInput.text =
                UserData.UserName;

            userNameInput.onValueChanged.AddListener(
                (value) =>
                {
                    UserData.UserName = value;
                });
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene,LoadSceneMode mode)
    {
        GameObject obj =
            GameObject.Find("UserName");

        if (obj != null)
        {
            userNameInput =
                obj.GetComponent<TMP_InputField>();

            userNameInput.text =
                UserData.UserName;
        }
    }

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
        BGMManager_Takeshita.Instance.StopBGM();

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

        BGMManager_Takeshita.Instance.StopBGM();

        SceneManager.LoadScene("Takeshita_Matching");
    }
}