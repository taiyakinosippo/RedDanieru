
using UnityEngine;
using TMPro;

public class DungeonCostUI : MonoBehaviour
{
    [Header("コスト管理")]
    [SerializeField]
    private DungeonCostManager dungeonCostManager;

    [Header("表示")]
    [SerializeField]
    private TMP_Text costText;

    private void Update()
    {
        if (dungeonCostManager == null ||
            costText == null)
        {
            return;
        }

        int current = dungeonCostManager.CurrentCost;
        int initial = dungeonCostManager.InitialCost;

        costText.text = current + "/" + initial;
    }
}
