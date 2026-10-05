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

    private void Start()
    {
        if (cautionObj != null)
        {
            cautionObj.SetActive(false);
        }

        if (savePanel != null)
        {
            savePanel.SetActive(false);
        }

        // Inspector設定確認
        if (dungeonNameInput == null)
        {
            Debug.LogError("SaveUI：dungeonNameInput が設定されていません。");
        }

        if (creatorNameInput == null)
        {
            Debug.LogError("SaveUI：creatorNameInput が設定されていません。");
        }

        if (saveManager == null)
        {
            Debug.LogError("SaveUI：saveManager が設定されていません。");
        }

        if (uploader == null)
        {
            Debug.LogError("SaveUI：uploader が設定されていません。");
        }
    }

    //==================================================
    // 保存パネルを開く
    //==================================================

    public void OpenSavePanel()
    {
        if (dungeonNameInput != null)
        {
            dungeonNameInput.text = "";
        }

        if (creatorNameInput != null)
        {
            creatorNameInput.text = "";
        }

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
        // 必要な参照を確認
        if (dungeonNameInput == null)
        {
            Debug.LogError(
                "SaveUI：dungeonNameInput が設定されていません。"
            );
            return;
        }

        if (creatorNameInput == null)
        {
            Debug.LogError(
                "SaveUI：creatorNameInput が設定されていません。"
            );
            return;
        }

        if (saveManager == null)
        {
            Debug.LogError(
                "SaveUI：saveManager が設定されていません。"
            );
            return;
        }

        if (uploader == null)
        {
            Debug.LogError(
                "SaveUI：uploader が設定されていません。"
            );
            return;
        }

        // 入力されたダンジョン名を取得
        string dungeonName =
            dungeonNameInput.text.Trim();

        // 製作者名を取得
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
        bool saved = saveManager.Save(
            dungeonName
        );

        // 同じ名前が既にある場合は上書き保存
        if (!saved)
        {
            saved = saveManager.Overwrite(
                dungeonName
            );
        }

        // 保存に失敗した場合は投稿しない
        if (!saved)
        {
            Debug.LogError(
                "保存に失敗したため投稿しません : " +
                dungeonName
            );

            StartCoroutine(CautionText());

            return;
        }

        // ダンジョンを投稿
        uploader.UploadDungeon(
            dungeonName,
            creatorName
        );

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
    }

    //==================================================
    // 戻る
    //==================================================

    public void Return()
    {
        if (savePanel != null)
        {
            savePanel.SetActive(false);
        }

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