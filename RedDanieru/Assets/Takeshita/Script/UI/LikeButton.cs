using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class LikeButton : MonoBehaviour
{
    [SerializeField]
    private Button likeButton;

    private string dungeonId;

    private bool likedThisRun;

    private void OnEnable()
    {
        dungeonId = RoomInfo.SelectedDungeon;
    }

    private void Start()
    {
        likeButton.onClick.AddListener(
            OnLikeButtonClick
        );
    }

    private void OnLikeButtonClick()
    {
        if (string.IsNullOrEmpty(dungeonId))
        {
            Debug.LogError(
                "DungeonIDが空です"
            );
            return;
        }

        if (likedThisRun)
        {
            Debug.Log(
                "今回のプレイでは既にいいね済み"
            );
            return;
        }

        StartCoroutine(
            AddLikeCoroutine()
        );
    }

    private IEnumerator AddLikeCoroutine()
    {
        WWWForm form = new WWWForm();

        form.AddField(
            "dungeon_id",
            dungeonId
        );

        UnityWebRequest request =
            UnityWebRequest.Post(
                ServerApi.Url("add_like_count.php"),
                form
            );

        yield return request.SendWebRequest();

        if (request.result ==
            UnityWebRequest.Result.Success)
        {
            PlayerPrefs.SetInt(
                "Like_" + dungeonId,
                1
            );

            PlayerPrefs.Save();

            likeButton.interactable = false;
        }
        else
        {
            Debug.LogError(
                request.error
            );
        }
    }
}