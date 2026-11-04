
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

    private Category currentCategory = Category.Object;
    private bool isShowingAllPanels;

    private RectTransform objectRect;
    private RectTransform enemyRect;
    private RectTransform trapRect;

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
            // 現在のカテゴリだけ表示
            UpdateCategory();
        }

        UpdateCategoryText();
    }

    // 全一覧を縦に並べる
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

    // カテゴリ選択ボタン
    public void SelectObject()
    {
        ChangeCategory(Category.Object);
    }

    public void SelectEnemy()
    {
        ChangeCategory(Category.Enemy);
    }

    public void SelectTrap()
    {
        ChangeCategory(Category.Trap);
    }

    // 配置ボタンから呼び出すカテゴリ切り替え
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

    // カテゴリ変更
    private void ChangeCategory(Category category)
    {
        currentCategory = category;

        // カテゴリを切り替える
        isShowingAllPanels = false;

        // パネル位置は変更しない
        UpdateCategory();
        UpdateCategoryText();
    }

    // 全パネルの表示変更
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
