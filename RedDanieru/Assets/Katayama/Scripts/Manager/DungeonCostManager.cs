using UnityEngine;

/// <summary>
/// ダンジョンのコストを管理するクラス。
/// 配置物によるコスト変化の計算を担当する。
/// </summary>
public class DungeonCostManager : MonoBehaviour
{
    [Header("ダンジョンコスト")]
    [SerializeField] private int initialCost = 500;
    [SerializeField] private int currentCost = 500;

    public int InitialCost => initialCost;
    public int CurrentCost => currentCost;

    /// <summary>
    /// コストを初期値に戻す。
    /// 新規ダンジョン作成時などに使用する。
    /// </summary>
    public void ResetCost()
    {
        currentCost = Mathf.Max(0, initialCost);
    }

    /// <summary>
    /// コスト変化を適用する。
    /// 減少によって0未満になる場合は適用しない。
    /// 増加には上限を設けない。
    /// </summary>
    public bool TryChangeCost(int costChange)
    {
        if (costChange < 0 && currentCost + costChange < 0)
        {
            return false;
        }

        currentCost = Mathf.Max(0, currentCost + costChange);
        return true;
    }

    /// <summary>
    /// 保存データなどから現在コストを設定する。
    /// </summary>
    public void SetCost(int cost)
    {
        currentCost = Mathf.Max(0, cost);
    }
}
