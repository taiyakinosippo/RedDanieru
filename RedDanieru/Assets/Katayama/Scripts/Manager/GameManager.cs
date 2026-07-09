using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private SaveManager saveManager;

    [Header("シーン名")]
    [SerializeField] private string editorSceneName = "EditorScene";
    [SerializeField] private string playSceneName = "PlayScene";

    /// プレイ開始
    public void StartPlay()
    {
        // ダンジョンを保存
        if (saveManager != null)
        {
            saveManager.Save();
        }

        // プレイシーンへ移動
        SceneManager.LoadScene(playSceneName);
    }

    /// 編集画面へ戻る
    public void BackToEditor()
    {
        SceneManager.LoadScene(editorSceneName);
    }

    /// シーン名を指定して移動
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    /// 現在のシーンを再読み込み
    public void ReloadScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}