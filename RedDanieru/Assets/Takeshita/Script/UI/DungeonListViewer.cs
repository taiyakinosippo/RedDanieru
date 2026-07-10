using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class DungeonListViewer : MonoBehaviour
{
    public Transform content;
    public GameObject dungeonItemPrefab;

    public void RefreshList()
    {
        StartCoroutine(
            RefreshListCoroutine()
        );
    }

    private IEnumerator RefreshListCoroutine()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        UnityWebRequest request =
            UnityWebRequest.Get(
                "http://localhost/RedDaniel/list_dungeon.php"
            );

        yield return request.SendWebRequest();

        if (request.result !=
            UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            yield break;
        }

        string json =
            request.downloadHandler.text;

        DungeonListResponse response =
            JsonUtility.FromJson<DungeonListResponse>(
                json
            );

        foreach (var dungeon in response.items)
        {
            GameObject item =
                Instantiate(
                    dungeonItemPrefab,
                    content
                );

            DungeonItemUI ui =
                item.GetComponent<DungeonItemUI>();

            ui.Setup(
                dungeon.dungeon_name,
                dungeon.creator_name,
                dungeon.created_at
            );
        }
    }
}