using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalClear : MonoBehaviour
{
    [Header("クリアUI")]
    [SerializeField] private GameObject clearPanel;

    [Header("タイトルシーン")]
    [SerializeField] private string titleSceneName = "Title";

    private bool isCleared = false;

    private void Start()
    {
        if (clearPanel != null)
        {
            clearPanel.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCleared)
            return;

        if (!other.CompareTag("Player"))
            return;

        isCleared = true;

        // クリアUIを表示
        if (clearPanel != null)
        {
            clearPanel.SetActive(true);
        }

        Debug.Log("GAME CLEAR!");
    }

    /// <summary>
    /// タイトルへ戻るボタン
    /// </summary>
    public void ReturnToTitle()
    {
        SceneManager.LoadScene(titleSceneName);
    }
}