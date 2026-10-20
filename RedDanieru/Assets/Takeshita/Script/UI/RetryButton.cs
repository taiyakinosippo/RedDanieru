using UnityEngine;
using System.Collections;

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

    private void Awake()
    {
        dungeonImporter =
            FindObjectOfType<DungeonImporter>();
    }

    public void Retry()
    {
        StartCoroutine(RetryCoroutine());
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

        dungeonImporter.ImportDungeon(
            RoomInfo.SelectedDungeon
        );

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

    private void ResetCameras()
    {
        Camera[] allCameras =
            FindObjectsOfType<Camera>(true);

        foreach (Camera cam in allCameras)
        {
            cam.gameObject.SetActive(true);
        }
    }
}