using TMPro;
using UnityEngine;

public class SaveUI : MonoBehaviour
{
    // 保存パネル
    [SerializeField] private GameObject savePanel;

    // ダンジョン名入力欄
    [SerializeField] private TMP_InputField dungeonNameInput;

    // 保存管理
    [SerializeField] private SaveManager saveManager;

    /// 保存パネルを開く
    public void OpenSavePanel()
    {
        // 前回入力した文字を消去
        dungeonNameInput.text = "";

        // 保存パネルを表示
        savePanel.SetActive(true);
    }

    /// ダンジョンを保存する
    public void SaveDungeon()
    {
        // 入力されたダンジョン名を取得
        string dungeonName = dungeonNameInput.text.Trim();

        // ダンジョン名が入力されているか確認
        if (string.IsNullOrEmpty(dungeonName))
        {
            Debug.Log("ダンジョン名を入力してください。");
            return;
        }

        // ダンジョンを保存
        saveManager.Save(dungeonName);

        // 保存パネルを閉じる
        savePanel.SetActive(false);
    }

    /// 保存をキャンセルする
    public void Cancel()
    {
        // 保存パネルを閉じる
        savePanel.SetActive(false);
    }
}