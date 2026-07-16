using UnityEngine;
using TMPro;
using UnityEngine.UI;

//ゲームモード
public static class GameModeManager
{
    public static bool IsMultiplayer = false;
}

//選択したダンジョン名保存
public static class RoomInfo
{
    public static string SelectedDungeon;

}

public static class RoomIdGenerator
{
    private const string Characters =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
        "abcdefghijklmnopqrstuvwxyz" +
        "0123456789";

    public static string GenerateRoomId()
    {
        char[] id = new char[10];

        for(int i = 0; i < id.Length; i++)
        {
            id[i] = Characters[
                Random.Range(0, Characters.Length)
                ];
        }

        return new string(id);
    }
}

public class DungeonUIManager : MonoBehaviour
{
    public DungeonUploader uploader;
    public DungeonImporter importer;

    public TMP_InputField dungeonNameInput;
    public TMP_InputField creatorNameInput;

    public GameObject SelectCanvas;

    public GameObject ScrolView;

    public GameObject RoomInfoObj;

    public GameObject RoomCreateObj;
    public GameObject RoomJoinObj;

    public GameObject Laycast;
    public GameObject CautionObj;

    [SerializeField]private Text dungeonNameText;
    [SerializeField] private Text RoomIdText;
    [SerializeField] private Text RoomKeyText;
    [SerializeField] private Text CautionText;

    [SerializeField]
    private InputField passwordInputField;

    [SerializeField]
    private Dropdown playerCountDropdown;

    public static int MaxPlayers = 2;

    public static bool IsPrivateRoom;
    public static string Password;

    public void Start()
    {
        ScrolView.SetActive(false);
        RoomInfoObj.SetActive(false);
        Laycast.SetActive(false);
        CautionObj.SetActive(false);

        passwordInputField.onValueChanged.AddListener(OnPasswordChanged);

        OnPlayerCountChanged(
              playerCountDropdown.value
          );

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
        SelectCanvas.SetActive(false);
        ScrolView.SetActive(true);
    }

    public void MultiMode()
    {
        GameModeManager.IsMultiplayer = true;
        Debug.Log("Multi");
        SelectCanvas.SetActive(false);
        ScrolView.SetActive(true);
    }

    public void ScrollBackButton()
    {
        SelectCanvas.SetActive(true);
        ScrolView.SetActive(false);
    }

    public void  RoomInfoBackButton()
    {
        ScrolView.SetActive(true);
        RoomInfoObj.SetActive(false);
    }

    public void RoomCreateButton()
    {
        RoomCreateObj.SetActive(true);
        RoomJoinObj.SetActive(false);
    }

    public void RoomJoinButton()
    {
        RoomCreateObj.SetActive(false);
        RoomJoinObj.SetActive(true);
    }

    public void MapSelectButton()
    {
        dungeonNameText.text = "マップ：" + RoomInfo.SelectedDungeon;
        
        string roomId = RoomIdGenerator.GenerateRoomId();
        RoomIdText.text = "RoomID："+roomId;

        RoomInfoObj.SetActive(true);
        ScrolView.SetActive(false);

        RoomCreateObj.SetActive(false);
        RoomJoinObj.SetActive(false);
    }

    public void CreateButton()
    {
        Laycast.SetActive(true);
        CautionObj.SetActive(true);

        string roomId = RoomIdGenerator.GenerateRoomId();

        string roomType;

        if (IsPrivateRoom)
        {
            roomType =
                "非公開ルーム\n" +
                "パスワード：" + Password;
        }
        else
        {
            roomType = "公開ルーム";
        }

        CautionText.text =
            "マップ：" + RoomInfo.SelectedDungeon +
            "\nRoomID：" + roomId +
            "\n" + roomType +
            "\n最大人数：" + MaxPlayers + "人";
    }

    private void OnPasswordChanged(string value)
    {
        // 数字以外を除去
        string numbersOnly = "";

        foreach (char c in value)
        {
            if (char.IsDigit(c))
            {
                numbersOnly += c;
            }
        }

        // 5文字まで
        if (numbersOnly.Length > 5)
        {
            numbersOnly = numbersOnly.Substring(0, 5);
        }

        // InputFieldへ反映
        if (passwordInputField.text != numbersOnly)
        {
            passwordInputField.text = numbersOnly;
        }

        Password = numbersOnly;

        // 1文字でも入力されたらPrivate
        IsPrivateRoom = !string.IsNullOrEmpty(numbersOnly);

        if (IsPrivateRoom)
        {
            RoomKeyText.text = "Private";
        }
        else
        {
            RoomKeyText.text = "Public";
        }

        Debug.Log(
            IsPrivateRoom
            ? "Private Room"
            : "Public Room"
        );
    }

    private void OnPlayerCountChanged(int index)
    {
        MaxPlayers = index + 1;

        Debug.Log(
            $"最大人数 : {MaxPlayers}人"
        );
    }
}