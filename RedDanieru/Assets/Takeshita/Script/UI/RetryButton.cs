using UnityEngine;
using System.Collections;

public class RetryButton : MonoBehaviour
{
    private DungeonImporter dungeonImporter;

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

        GoalClear goalClear =
            FindObjectOfType<GoalClear>();

        if (dungeonImporter == null)
        {
            Debug.LogError("DungeonImporterが見つからない");
        }

        if (goalClear != null)
        {
            goalClear.ResetClearState();
        }

        ResetCameras();

        Debug.Log("⑦ Player検索開始");

        GameObject[] players =
            GameObject.FindGameObjectsWithTag("Player");

        Debug.Log("⑧ Player数 = " + players.Length);

        foreach (GameObject player in players)
        {
            Debug.Log("⑨ Destroyする = " + player.name);

            Destroy(player);
        }

        Debug.Log("⑩ Player削除予約完了");

        yield return null;

        Debug.Log("ImportDungeon開始");

        dungeonImporter.ImportDungeon(
            RoomInfo.SelectedDungeon
        );

        Debug.Log("StartSolo直前");

        if (FusionLauncher.Instance == null)
        {
            Debug.LogError("FusionLauncher.Instance が null");
            yield break;
        }

        Debug.Log("StartSolo呼び出し");

        FusionLauncher.Instance.StartSolo();

        Debug.Log("StartSolo呼び出し後");
    }

    private void ResetCameras()
    {
        Player.PlayerCamera[] cameras =
            FindObjectsOfType<Player.PlayerCamera>(true);

        foreach (var camera in cameras)
        {
            if (camera._deadCamera != null)
            {
                camera._deadCamera.SetActive(false);
            }

            if (camera.ThirdPersonPerspective != null)
            {
                camera.ThirdPersonPerspective.SetActive(true);
            }

            if (camera.FirstPersonPerspective != null)
            {
                camera.FirstPersonPerspective.SetActive(false);
            }
        }
    }
}