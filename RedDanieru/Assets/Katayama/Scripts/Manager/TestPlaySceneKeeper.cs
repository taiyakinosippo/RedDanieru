using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 編集シーン(Katayama_ren)を読み込んだまま、テストプレイシーンを追加で読み込む。
// 編集シーンのルートオブジェクトは非表示にするだけなので、戻るときに読み込み直しが要らない。
// シーンには置かず、必要になったときに DontDestroyOnLoad で自動生成する。
public class TestPlaySceneKeeper : MonoBehaviour
{
    private static TestPlaySceneKeeper instance;

    // 残している編集シーン
    private Scene keptScene;

    // 非表示にしたルートオブジェクト(元から非表示のものは含めない)
    private readonly List<GameObject> hiddenRoots = new List<GameObject>();

    // 読み込み中のテストプレイシーン名
    private string loadingSceneName;

    // 遷移中か
    private bool isBusy;

    private static bool HasKeptScene
    {
        get
        {
            return instance != null &&
                   instance.keptScene.IsValid() &&
                   instance.keptScene.isLoaded;
        }
    }

    //==================================================
    // テストプレイ開始
    // 処理を引き受けたら true。false のときは呼び出し側で通常の LoadScene をする。
    //==================================================

    public static bool StartTestPlay(Scene editScene, string testPlaySceneName)
    {
        if (!editScene.IsValid() || !editScene.isLoaded)
        {
            return false;
        }

        if (!Application.CanStreamedLevelBeLoaded(testPlaySceneName))
        {
            Debug.LogError(
                "テストプレイシーンを読み込めません : " + testPlaySceneName
            );

            return false;
        }

        TestPlaySceneKeeper keeper = GetOrCreate();

        if (keeper.isBusy)
        {
            // 連打で二重に読み込まない
            return true;
        }

        keeper.StartCoroutine(
            keeper.EnterTestPlay(editScene, testPlaySceneName)
        );

        return true;
    }

    //==================================================
    // 編集シーンへ戻る
    // 処理を引き受けたら true。false のときは呼び出し側で通常の LoadScene をする。
    //==================================================

    public static bool ReturnToEdit(Scene testPlayScene)
    {
        if (!HasKeptScene)
        {
            return false;
        }

        if (instance.isBusy)
        {
            return true;
        }

        instance.StartCoroutine(
            instance.ExitTestPlay(testPlayScene)
        );

        return true;
    }

    //==================================================
    // 生成
    //==================================================

    private static TestPlaySceneKeeper GetOrCreate()
    {
        if (instance != null)
        {
            return instance;
        }

        GameObject obj = new GameObject("[TestPlaySceneKeeper]");
        DontDestroyOnLoad(obj);

        instance = obj.AddComponent<TestPlaySceneKeeper>();

        return instance;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    //==================================================
    // テストプレイシーンへ
    //==================================================

    private IEnumerator EnterTestPlay(Scene editScene, string testPlaySceneName)
    {
        isBusy = true;

        keptScene = editScene;
        loadingSceneName = testPlaySceneName;

        AsyncOperation op = SceneManager.LoadSceneAsync(
            testPlaySceneName,
            LoadSceneMode.Additive
        );

        // 読み込み完了の直前まで編集画面を表示しておく
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            yield return null;
        }

        // テストプレイシーンの Awake より前に編集シーンを隠す
        // (FindObjectOfType で編集シーン側の MapManager などを拾わないように)
        HideKeptScene();

        op.allowSceneActivation = true;

        while (!op.isDone)
        {
            yield return null;
        }

        loadingSceneName = null;
        isBusy = false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Additive ||
            string.IsNullOrEmpty(loadingSceneName) ||
            scene.name != loadingSceneName)
        {
            return;
        }

        // Start より前にアクティブシーンを切り替える
        // (シーン名で動きを変えているスクリプトやライティング設定のため)
        SceneManager.SetActiveScene(scene);
    }

    //==================================================
    // 編集シーンへ
    //==================================================

    private IEnumerator ExitTestPlay(Scene testPlayScene)
    {
        isBusy = true;

        SceneManager.SetActiveScene(keptScene);

        if (testPlayScene.IsValid() && testPlayScene.isLoaded)
        {
            AsyncOperation op =
                SceneManager.UnloadSceneAsync(testPlayScene);

            while (op != null && !op.isDone)
            {
                yield return null;
            }
        }

        ShowKeptScene();

        keptScene = default;
        isBusy = false;
    }

    //==================================================
    // 表示切り替え
    //==================================================

    private void HideKeptScene()
    {
        hiddenRoots.Clear();

        foreach (GameObject root in keptScene.GetRootGameObjects())
        {
            if (root.activeSelf)
            {
                root.SetActive(false);
                hiddenRoots.Add(root);
            }
        }
    }

    private void ShowKeptScene()
    {
        foreach (GameObject root in hiddenRoots)
        {
            if (root != null)
            {
                root.SetActive(true);
            }
        }

        hiddenRoots.Clear();
    }

    //==================================================
    // タイトルへ戻るなどで編集シーンが破棄されたとき
    //==================================================

    private void OnSceneUnloaded(Scene scene)
    {
        if (scene != keptScene)
        {
            return;
        }

        StopAllCoroutines();

        hiddenRoots.Clear();
        keptScene = default;
        loadingSceneName = null;
        isBusy = false;
    }
}
