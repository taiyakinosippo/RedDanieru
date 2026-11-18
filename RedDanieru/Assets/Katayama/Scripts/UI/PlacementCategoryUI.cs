
using UnityEngine;
using TMPro;

public class PlacementCategoryUI : MonoBehaviour
{
    public enum Category
    {
        Object,
        Enemy,
        Trap
    }

    [Header("カテゴリ別パネル")]
    [SerializeField] private GameObject objectPanel;
    [SerializeField] private GameObject enemyPanel;
    [SerializeField] private GameObject trapPanel;

    [Header("カテゴリ表示")]
    [SerializeField] private TMP_Text categoryText;

    [Header("全一覧表示設定")]
    [SerializeField] private float panelSpacing = 8f;

    [Header("カテゴリ設定")]
    [SerializeField] private Category currentCategory = Category.Object;

    private bool isShowingAllPanels = false;

    private RectTransform objectRect;
    private RectTransform enemyRect;
    private RectTransform trapRect;

    private Vector2 objectOriginalPosition;
    private Vector2 enemyOriginalPosition;
    private Vector2 trapOriginalPosition;

    // テストプレイ後も選択カテゴリを保持する
    private static Category savedCategory = Category.Object;
    private static bool hasSavedCategory = false;

    public Category CurrentCategory => currentCategory;

    private void Awake()
    {
        CachePanelPositions();
    }

    private void Start()
    {
        if (hasSavedCategory)
        {
            currentCategory = savedCategory;
        }
        else
        {
            currentCategory = Category.Object;
            SaveCurrentCategory();
        }

        isShowingAllPanels = false;

        UpdateCategory();
    }

    // パネルの初期位置を保存
    private void CachePanelPositions()
    {
        if (objectPanel != null)
        {
            objectRect = objectPanel.GetComponent<RectTransform>();

            if (objectRect != null)
                objectOriginalPosition = objectRect.anchoredPosition;
        }

        if (enemyPanel != null)
        {
            enemyRect = enemyPanel.GetComponent<RectTransform>();

            if (enemyRect != null)
                enemyOriginalPosition = enemyRect.anchoredPosition;
        }

        if (trapPanel != null)
        {
            trapRect = trapPanel.GetComponent<RectTransform>();

            if (trapRect != null)
                trapOriginalPosition = trapRect.anchoredPosition;
        }
    }

    // カテゴリを切り替える
    public void SwitchCategory(Category category)
    {
        currentCategory = category;

        isShowingAllPanels = false;

        SaveCurrentCategory();

        UpdateCategory();
    }

    // オブジェクトボタン
    public void OnObjectPanelButton()
    {
        SwitchCategory(Category.Object);
    }

    // エネミーボタン
    public void OnEnemyPanelButton()
    {
        SwitchCategory(Category.Enemy);
    }

    // トラップボタン
    public void OnTrapPanelButton()
    {
        SwitchCategory(Category.Trap);
    }

    // パネルクリック
    public void OnPanelClick(Category category)
    {
        if (!isShowingAllPanels)
            return;

        currentCategory = category;

        isShowingAllPanels = false;

        SaveCurrentCategory();

        UpdateCategory();
    }

    // 全一覧表示に切り替える
    // 配置一覧の表示・非表示を切り替える
    public void ShowAllPanels()
    {
        if (isShowingAllPanels)
        {
            // 全一覧を閉じて、最後に選択していたカテゴリに戻す
            isShowingAllPanels = false;
        }
        else
        {
            // 全一覧を表示する
            isShowingAllPanels = true;
        }

        SaveCurrentCategory();
        UpdateCategory();
    }

    // 全パネルの表示を更新
    private void UpdateCategory()
    {
        if (isShowingAllPanels)
        {
            SetAllPanelsActive(true);
            ArrangeAllPanels();
        }
        else
        {
            SetAllPanelsActive(false);
            RestorePanelPositions();

            switch (currentCategory)
            {
                case Category.Object:
                    if (objectPanel != null)
                        objectPanel.SetActive(true);
                    break;

                case Category.Enemy:
                    if (enemyPanel != null)
                        enemyPanel.SetActive(true);
                    break;

                case Category.Trap:
                    if (trapPanel != null)
                        trapPanel.SetActive(true);
                    break;
            }
        }

        UpdateCategoryText();
    }

    // 全パネルの表示状態を変更
    private void SetAllPanelsActive(bool active)
    {
        if (objectPanel != null)
            objectPanel.SetActive(active);

        if (enemyPanel != null)
            enemyPanel.SetActive(active);

        if (trapPanel != null)
            trapPanel.SetActive(active);
    }

    // 全一覧を縦に配置
    private void ArrangeAllPanels()
    {
        RestorePanelPositions();

        float currentY = 0f;

        ArrangePanel(objectRect, ref currentY);
        ArrangePanel(enemyRect, ref currentY);
        ArrangePanel(trapRect, ref currentY);
    }

    private void ArrangePanel(RectTransform rect, ref float currentY)
    {
        if (rect == null)
            return;

        float height = rect.rect.height;

        Vector2 position = rect.anchoredPosition;
        position.y = -currentY;

        rect.anchoredPosition = position;

        currentY += height + panelSpacing;
    }

    // パネル位置を元に戻す
    private void RestorePanelPositions()
    {
        if (objectRect != null)
            objectRect.anchoredPosition = objectOriginalPosition;

        if (enemyRect != null)
            enemyRect.anchoredPosition = enemyOriginalPosition;

        if (trapRect != null)
            trapRect.anchoredPosition = trapOriginalPosition;
    }

    // カテゴリ名を更新
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

    // 掘削時に全一覧を終了
    public void CloseAllPanelsForDig()
    {
        isShowingAllPanels = false;

        SaveCurrentCategory();

        UpdateCategory();
    }

    // 配置成功時に全一覧を終了
    public void CloseAllPanelsForPlacement()
    {
        isShowingAllPanels = false;

        SaveCurrentCategory();

        UpdateCategory();
    }

    // 現在のカテゴリを保存
    public void SaveCurrentCategory()
    {
        savedCategory = currentCategory;
        hasSavedCategory = true;
    }
}
