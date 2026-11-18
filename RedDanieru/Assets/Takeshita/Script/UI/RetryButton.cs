using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public enum RetryType
{
    Clear,
    GameOver
}


public class RetryButton : MonoBehaviour
{
    private DungeonImporter dungeonImporter;

    [SerializeField]
    private RetryType retryType;

    [SerializeField]
    private GameObject multiplayerText;

    [SerializeField]
    private GameObject stageLoadingObj;

    [SerializeField]
    private TextMeshProUGUI countdownText;

    [SerializeField]
    private TextMeshProUGUI retryVoteText;

    private bool hasVoted;

    private void Awake()
    {
        dungeonImporter =
            FindObjectOfType<DungeonImporter>();
    }

    private void Update()
    {
        if (!GameModeManager.IsMultiplayer)
            return;

        if (NetworkGameState.Instance == null)
            return;

        bool allVoted =
            NetworkGameState.Instance.RetryVoteCount >=
            FusionLauncher.Instance.PlayerCount;

        retryVoteText.text =
            $"{NetworkGameState.Instance.RetryVoteCount}/" +
            $"{FusionLauncher.Instance.PlayerCount}";

        if (!allVoted)
        {
            countdownText.text = "もう一回遊ぶ";

            retryVoteText.text =
                $"{NetworkGameState.Instance.RetryVoteCount}/" +
                $"{FusionLauncher.Instance.PlayerCount}";
        }
        else
        {
            countdownText.text =
                $"ゲーム開始まで {NetworkGameState.Instance.RetryCountdown}";

            retryVoteText.text =
                $"{NetworkGameState.Instance.RetryVoteCount}/" +
                $"{FusionLauncher.Instance.PlayerCount}";
        }
    }

    private void OnEnable()
    {
        if (multiplayerText != null)
        {
            multiplayerText.SetActive(
                GameModeManager.IsMultiplayer
            );
        }
    }

    public void Retry()
    {
        if (GameModeManager.IsMultiplayer)
        {
            MultiplayerRetry();
        }
        else
        {
            StartCoroutine(RetryCoroutine());
        }
    }

    private IEnumerator RetryCoroutine()
    {
        Time.timeScale = 1f;

        ResetCameras();

        Player.PlayerMovement[] players =
            FindObjectsOfType<Player.PlayerMovement>(true);

        foreach (var player in players)
        {
            Destroy(player.gameObject);
        }

        yield return null;

        stageLoadingObj.SetActive(true);

        yield return null;

        dungeonImporter.ImportDungeon(
            RoomInfo.SelectedDungeon
        );

        yield return null;

         FusionLauncher.Instance.StartSolo();

        // 最後にUIを閉じる
        switch (retryType)
        {
            case RetryType.Clear:
                {
                    GoalClear goalClear =
                        FindObjectOfType<GoalClear>();

                    if (goalClear != null)
                    {
                        goalClear.ResetClearState();
                    }
                    break;
                }

            case RetryType.GameOver:
                {
                    GameOverManager gameOverManager =
                        FindObjectOfType<GameOverManager>();

                    if (gameOverManager != null)
                    {
                        gameOverManager.ResetGameOverState();
                    }
                    break;
                }
        }
    }

    public void MultiplayerRetry()
    {
        if (hasVoted)
            return;

        hasVoted = true;

        NetworkGameState.Instance.RPC_AddRetryVote();

        UpdateRetryText();
    }

    public void UpdateRetryText()
    {
        if (retryVoteText == null ||
            NetworkGameState.Instance == null)
        {
            return;
        }

        retryVoteText.text =
            $"{NetworkGameState.Instance.RetryVoteCount}/" +
            $"{FusionLauncher.Instance.PlayerCount}";
    }

 

    private void ResetCameras()
    {
        Camera[] allCameras =
            FindObjectsOfType<Camera>(true);

        foreach (Camera cam in allCameras)
        {
            cam.gameObject.SetActive(true);
        }
    }

    public void ResetRetryState()
    {
        hasVoted = false;
    }
}