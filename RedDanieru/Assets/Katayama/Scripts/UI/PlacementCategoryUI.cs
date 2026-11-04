
using UnityEngine;
using TMPro;

public class PlacementCategoryUI : MonoBehaviour
{
    public enum Category
    {
        Enemy,
        Object,
        Trap
    }

    [Header("カテゴリ別パネル")]
    [SerializeField] private GameObject enemyPanel;
    [SerializeField] private GameObject objectPanel;
    [SerializeField] private GameObject trapPanel;

    [Header("カテゴリ表示")]
    [SerializeField] private TMP_Text categoryText;

    [Header("一覧の配置設定")]
    [SerializeField] private float panelSpacing = 8f;

    // 現在のカテゴリ
    private Category currentCategory = Category.Object;

    // 全一覧表示中か
    private bool isShowingAllPanels;

    // 各パネルのRectTransform
    private RectTransform objectRect;
    private RectTransform enemyRect;
    private RectTransform trapRect;

    // 初期位置
    private Vector2 objectOriginalPosition;
    private Vector2 enemyOriginalPosition;
    private Vector2 trapOriginalPosition;

    private void Awake()
    {
        if (objectPanel != null)
            objectRect = objectPanel.GetComponent<RectTransform>();

        if (enemyPanel != null)
            enemyRect = enemyPanel.GetComponent<RectTransform>();

        if (trapPanel != null)
            trapRect = trapPanel.GetComponent<RectTransform>();

        SaveOriginalPosition();

        currentCategory = Category.Object;
        isShowingAllPanels = false;

        UpdateCategory();
    }

    // 初期位置を保存
    private void SaveOriginalPosition()
    {
        if (objectRect != null)
            objectOriginalPosition = objectRect.anchoredPosition;

        if (enemyRect != null)
            enemyOriginalPosition = enemyRect.anchoredPosition;

        if (trapRect != null)
            trapOriginalPosition = trapRect.anchoredPosition;
    }

    // 全一覧の表示切り替え
    public void SwitchCategory()
    {
        isShowingAllPanels = !isShowingAllPanels;

        if (isShowingAllPanels)
        {
            SetAllPanelsActive(true);
            ArrangeAllPanels();
        }
        else
        {
            UpdateCategory();
        }

        UpdateCategoryText();
    }

    // 3つの一覧を縦に並べる
    private void ArrangeAllPanels()
    {
        if (objectRect == null ||
            enemyRect == null ||
            trapRect == null)
        {
            Debug.LogError("一覧パネルの設定を確認してください。");
            return;
        }

        objectRect.anchoredPosition = objectOriginalPosition;

        float objectHeight = objectRect.rect.height;
        float enemyHeight = enemyRect.rect.height;

        enemyRect.anchoredPosition = new Vector2(
            objectOriginalPosition.x,
            objectOriginalPosition.y - objectHeight - panelSpacing
        );

        trapRect.anchoredPosition = new Vector2(
            objectOriginalPosition.x,
            objectOriginalPosition.y
                - objectHeight
                - panelSpacing
                - enemyHeight
                - panelSpacing
        );
    }

    // オブジェクトカテゴリ
    public void SelectObject()
    {
        ChangeCategory(Category.Object);
    }

    // エネミーカテゴリ
    public void SelectEnemy()
    {
        ChangeCategory(Category.Enemy);
    }

    // トラップカテゴリ
    public void SelectTrap()
    {
        ChangeCategory(Category.Trap);
    }

    // 配置ボタンから呼ぶカテゴリ切り替え
    public void ShowObjectCategory()
    {
        ChangeCategory(Category.Object);
    }

    public void ShowEnemyCategory()
    {
        ChangeCategory(Category.Enemy);
    }

    public void ShowTrapCategory()
    {
        ChangeCategory(Category.Trap);
    }

    // オブジェクト一覧の枠クリック
    public void OnObjectPanelButton()
    {
        OnPanelClick(Category.Object);
    }

    // エネミー一覧の枠クリック
    public void OnEnemyPanelButton()
    {
        OnPanelClick(Category.Enemy);
    }

    // トラップ一覧の枠クリック
    public void OnTrapPanelButton()
    {
        OnPanelClick(Category.Trap);
    }

    // 配置一覧表示中のみ枠クリックを受け付ける
    private void OnPanelClick(Category category)
    {
        // 全一覧表示中でなければ何もしない
        if (!isShowingAllPanels)
            return;

        // クリックしたカテゴリだけ表示
        ChangeCategory(category);
    }

    // カテゴリ変更
    private void ChangeCategory(Category category)
    {
        currentCategory = category;

        // 全一覧表示を解除
        isShowingAllPanels = false;

        // 選択カテゴリだけ表示
        UpdateCategory();

        UpdateCategoryText();
    }

    // 掘削成功時に呼び出す
    public void CloseAllPanelsForDig()
    {
        if (!isShowingAllPanels)
            return;

        isShowingAllPanels = false;

        UpdateCategory();
        UpdateCategoryText();

        Debug.Log("掘削成功：配置一覧を閉じました。");
    }

    // 配置成功時に呼び出す
    public void CloseAllPanelsForPlacement()
    {
        if (!isShowingAllPanels)
            return;

        isShowingAllPanels = false;

        UpdateCategory();
        UpdateCategoryText();

        Debug.Log("配置完了：配置一覧を閉じました。");
    }

    // 全パネル表示
    private void SetAllPanelsActive(bool active)
    {
        if (objectPanel != null)
            objectPanel.SetActive(active);

        if (enemyPanel != null)
            enemyPanel.SetActive(active);

        if (trapPanel != null)
            trapPanel.SetActive(active);
    }

    // 現在のカテゴリだけ表示
    private void UpdateCategory()
    {
        if (objectPanel != null)
            objectPanel.SetActive(
                currentCategory == Category.Object);

        if (enemyPanel != null)
            enemyPanel.SetActive(
                currentCategory == Category.Enemy);

        if (trapPanel != null)
            trapPanel.SetActive(
                currentCategory == Category.Trap);

        UpdateCategoryText();
    }

    // カテゴリ名更新
    private void UpdateCategoryText()
    {
        if (categoryText == null)
            return;

        if (isShowingAllPanels)
        {
            categoryText.text = "配置一覧";
            return;
        }

        switch (currentCategory)
        {
            case Category.Object:
                categoryText.text = "オブジェクト";
                break;

            case Category.Enemy:
                categoryText.text = "エネミー";
                break;

            case Category.Trap:
                categoryText.text = "トラップ";
                break;
        }
    }
}
