
using UnityEngine;
using UnityEngine.EventSystems;

public class PlacementPanelClick : MonoBehaviour, IPointerClickHandler
{
    public enum PanelCategory
    {
        Object,
        Enemy,
        Trap
    }

    [Header("カテゴリ設定")]
    [SerializeField] private PlacementCategoryUI categoryUI;

    [SerializeField] private PanelCategory panelCategory;

    // 枠がクリックされたとき
    public void OnPointerClick(PointerEventData eventData)
    {
        if (categoryUI == null)
            return;

        switch (panelCategory)
        {
            case PanelCategory.Object:
                categoryUI.OnObjectPanelButton();
                break;

            case PanelCategory.Enemy:
                categoryUI.OnEnemyPanelButton();
                break;

            case PanelCategory.Trap:
                categoryUI.OnTrapPanelButton();
                break;
        }
    }
}
