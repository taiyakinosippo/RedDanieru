using UnityEngine;

public class ResetManager : MonoBehaviour
{
    // マップ管理
    [SerializeField] private MapManager mapManager;

    // Undo管理
    [SerializeField] private UndoManager undoManager;

    // リセット確認パネル（任意）
    [SerializeField] private GameObject resetPanel;

    /// <summary>
    /// ダンジョンを初期状態に戻す
    /// </summary>
    public void ResetDungeon()
    {
        // 確認パネルを閉じる
        if (resetPanel != null)
        {
            resetPanel.SetActive(false);
        }

        // Undo履歴を削除
        if (undoManager != null)
        {
            undoManager.ClearHistory();
        }

        // 現在のダンジョンを削除
        foreach (Transform child in mapManager.transform)
        {
            Destroy(child.gameObject);
        }

        // 新しいダンジョンを生成
        mapManager.CreateNewMap();

        Debug.Log("ダンジョンをリセットしました。");
    }

    /// <summary>
    /// リセット確認パネルを開く
    /// </summary>
    public void OpenResetPanel()
    {
        if (resetPanel != null)
        {
            resetPanel.SetActive(true);
        }
    }

    /// <summary>
    /// リセットをキャンセル
    /// </summary>
    public void CancelReset()
    {
        if (resetPanel != null)
        {
            resetPanel.SetActive(false);
        }
    }
}