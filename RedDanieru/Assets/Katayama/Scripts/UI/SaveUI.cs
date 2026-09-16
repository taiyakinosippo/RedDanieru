using TMPro;
using UnityEngine;
using System.Collections;

public class SaveUI : MonoBehaviour
{
    // 保存パネル
    [SerializeField] private GameObject savePanel;

    // ダンジョン名入力欄
    [SerializeField] private TMP_InputField dungeonNameInput;

    // 製作者名入力欄
    [SerializeField] private TMP_InputField creatorNameInput;

    // 保存管理
    [SerializeField] private SaveManager saveManager;

    // ダンジョン投稿
    [SerializeField] private DungeonUploader uploader;

    // 注意文
    [SerializeField] private GameObject cautionObj;

    // セーブ＆ロード管理
    [SerializeField] private SaveLoadUI saveLoadUI;

    public void Start()
    {
        if (cautionObj != null)
        {
            cautionObj.SetActive(false);
        }

        if (savePanel != null)
        {
            savePanel.SetActive(false);
        }
    }

    //==================================================
    // 保存パネルを開く
    //==================================================

    public void OpenSavePanel()
    {
        // 前回入力した文字を消去
        if (dungeonNameInput != null)
        {
            dungeonNameInput.text = "";
        }

        // 保存パネルを表示
        if (savePanel != null)
        {
            savePanel.SetActive(true);
        }
    }

    //==================================================
    // ダンジョンを保存する
    //==================================================

    public void SaveDungeon()
    {
        // 入力されたダンジョン名を取得
        string dungeonName =
            dungeonNameInput.text.Trim();

        string creatorName =
            creatorNameInput.text.Trim();

        // ダンジョン名が入力されているか確認
        if (string.IsNullOrEmpty(dungeonName))
        {
            Debug.Log(
                "ダンジョン名を入力してください。"
            );

            return;
        }

        // 製作者名が入力されているか確認
        if (string.IsNullOrEmpty(creatorName))
        {
            Debug.Log(
                "製作者名を入力してください。"
            );

            return;
        }

        // ダンジョンを保存
        saveManager.Save(
            dungeonName
        );

        // ダンジョンを投稿
        uploader.UploadDungeon(
            dungeonName,
            creatorName
        );

        // 保存パネルを閉じる
        savePanel.SetActive(false);

        // Save / Loadを両方押せる状態に戻す
        if (saveLoadUI != null)
        {
            saveLoadUI.ReturnFromSaveLoad();
        }
    }

    //==================================================
    // 戻る
    //==================================================

    public void Return()
    {
        // 保存パネルを閉じる
        if (savePanel != null)
        {
            savePanel.SetActive(false);
        }

        // Save / Loadを両方押せる状態に戻す
        if (saveLoadUI != null)
        {
            saveLoadUI.ReturnFromSaveLoad();
        }

        Debug.Log(
            "Save画面から戻りました。"
        );
    }

    //==================================================
    // 注意文
    //==================================================

    private IEnumerator CautionText()
    {
        if (cautionObj != null)
        {
            cautionObj.SetActive(true);
        }

        yield return new WaitForSeconds(3f);

        if (cautionObj != null)
        {
            cautionObj.SetActive(false);
        }
    }

    //==================================================
    // 保存をキャンセル
    //==================================================

    public void Cancel()
    {
        Return();
    }
}
