using UnityEngine;
using TMPro;

public static class GameModeManager
{
    public static bool IsMultiplayer = false;
}

public class DungeonUIManager : MonoBehaviour
{
    public DungeonUploader uploader;
    public DungeonImporter importer;

    public TMP_InputField dungeonNameInput;
    public TMP_InputField creatorNameInput;

    public GameObject SoloButton;
    public GameObject MultiButton;

    public GameObject ScrolView;

    public void Start()
    {
        ScrolView.SetActive(false);
    }

    public void UploadDungeon()
    {
        uploader.UploadDungeon(
            dungeonNameInput.text,
            creatorNameInput.text
        );
    }

    public void DownloadDungeon()
    {
        importer.ImportDungeon(
            dungeonNameInput.text
        );
    }

    public void SoloMode()
    {
        GameModeManager.IsMultiplayer = false;
        Debug.Log("Solo");
        SoloButton.SetActive(false);
        MultiButton.SetActive(false);
        ScrolView.SetActive(true);
    }

    public void MultiMode()
    {
        GameModeManager.IsMultiplayer = true;
        Debug.Log("Multi");
        SoloButton.SetActive(false);
        MultiButton.SetActive(false);
        ScrolView.SetActive(true);
    }
}