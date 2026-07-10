using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadUI : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private LoadManager loadManager;

    [SerializeField] private Transform content;

    [SerializeField] private GameObject buttonPrefab;

    private void OnEnable()
    {
        CreateButtonList();
    }

    /// <summary>
    /// 保存されているダンジョン一覧を表示する
    /// </summary>
    public void CreateButtonList()
    {
        // 以前作成したボタンを削除
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // 保存されているJSONファイルを取得
        string[] files = Directory.GetFiles(
            Application.persistentDataPath,
            "*.json"
        );

        foreach (string file in files)
        {
            // 拡張子を除いたダンジョン名
            string dungeonName = Path.GetFileNameWithoutExtension(file);

            // ボタン生成
            GameObject button = Instantiate(buttonPrefab, content);

            // ボタンの文字を変更
            button.GetComponentInChildren<TMP_Text>().text = dungeonName;

            // ボタンを押したときの処理
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                loadManager.Load(dungeonName);
            });
        }
    }
}