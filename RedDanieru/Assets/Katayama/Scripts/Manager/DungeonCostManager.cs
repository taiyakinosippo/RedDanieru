
using UnityEngine;
using System;

public class DungeonCostManager : MonoBehaviour
{
    [Header("ダンジョンコスト上限")]
    [SerializeField] private int maxCost = 200;

    [Header("配置物ごとのコスト")]
    [SerializeField] private int blockCost = 1;
    [SerializeField] private int wallCost = 1;
    [SerializeField] private int chestCost = 10;
    [SerializeField] private int enemyCost = 20;
    [SerializeField] private int trapCost = 15;
    [SerializeField] private int doorCost = 5;
    [SerializeField] private int spawnPointCost = 0;
    [SerializeField] private int goalCost = 50;
    [SerializeField] private int tentativeEnemyCost = 20;
    [SerializeField] private int chargeEnemyCost = 25;
    [SerializeField] private int tortoiseCost = 30;
    [SerializeField] private int leglessArcherCost = 20;

    // 現在のコスト
    public int CurrentCost { get; private set; }

    // コスト上限
    public int MaxCost => maxCost;

    // コスト変更通知
    public event Action<int, int> OnCostChanged;

    //==================================================
    // 配置物のコスト取得
    //==================================================

    public int GetCost(PlaceObjectType type)
    {
        switch (type)
        {
            case PlaceObjectType.Block:
                return blockCost;

            case PlaceObjectType.Wall:
                return wallCost;

            case PlaceObjectType.Chest:
                return chestCost;

            case PlaceObjectType.Enemy:
                return enemyCost;

            case PlaceObjectType.Trap:
                return trapCost;

            case PlaceObjectType.Door:
                return doorCost;

            case PlaceObjectType.SpawnPoint:
                return spawnPointCost;

            case PlaceObjectType.Goal:
                return goalCost;

            case PlaceObjectType.TentativeEnemy:
                return tentativeEnemyCost;

            case PlaceObjectType.ChargeEnemy:
                return chargeEnemyCost;

            case PlaceObjectType.Tortoise:
                return tortoiseCost;

            case PlaceObjectType.LeglessArcher:
                return leglessArcherCost;

            default:
                return 0;
        }
    }

    //==================================================
    // 現在のコストを設定
    //==================================================

    public void SetCurrentCost(int cost)
    {
        cost = Mathf.Max(0, cost);

        if (CurrentCost == cost)
        {
            return;
        }

        CurrentCost = cost;

        OnCostChanged?.Invoke(
            CurrentCost,
            maxCost
        );
    }

    //==================================================
    // コスト上限チェック
    //==================================================

    public bool CanAddCost(int cost)
    {
        cost = Mathf.Max(0, cost);

        return (long)CurrentCost + cost <= maxCost;
    }
}
