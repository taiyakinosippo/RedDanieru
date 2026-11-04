
using UnityEngine;

public class GoalEnemyBase : EnemyBase
{
    [Header("ゴール設定")]
    [SerializeField] private int goalMaxHP = 900;

    [Header("ゴール破壊時に破壊するオブジェクト")]
    [SerializeField] private GameObject[] destroyObjectsOnBreak;

    private int goalCurrentHP;
    private bool isGoalDestroyed;

    public int GoalCurrentHP => goalCurrentHP;
    public int GoalMaxHP => goalMaxHP;
    public bool IsGoalDestroyed => isGoalDestroyed;

    public override void Awake()
    {
        base.Awake();

        goalMaxHP = Mathf.Max(1, goalMaxHP);
        goalCurrentHP = goalMaxHP;

        currentState = enemyState.Idle;
    }

    public override void Start()
    {
        // EnemyBase.Startの処理は実行しない
    }

    public override void Damage(int playerPow)
    {
        if (isGoalDestroyed)
        {
            return;
        }

        int damage = Mathf.Max(1, playerPow);

        goalCurrentHP =
            Mathf.Max(0, goalCurrentHP - damage);

        Debug.Log(
            $"ゴールHP：{goalCurrentHP}/{goalMaxHP}"
        );

        if (goalCurrentHP <= 0)
        {
            DestroyGoal();
        }
    }

    //==================================================
    // ゴール破壊
    //==================================================

    private void DestroyGoal()
    {
        if (isGoalDestroyed)
        {
            return;
        }

        isGoalDestroyed = true;

        Debug.Log("ゴール破壊！");

        // 登録されたオブジェクトを破壊
        if (destroyObjectsOnBreak != null)
        {
            foreach (GameObject obj in destroyObjectsOnBreak)
            {
                // 未設定のオブジェクトと自分自身は除外
                if (obj == null || obj == gameObject)
                {
                    continue;
                }

                Destroy(obj);
            }
        }

        // ゴール本体は破棄しない
        // クリア処理だけ実行する
        GoalClear goalClear =
            FindObjectOfType<GoalClear>();

        if (goalClear == null)
        {
            Debug.LogError(
                "GoalClearがシーン内に見つかりません。"
            );
            return;
        }

        goalClear.ClearByGoalDestroyed();

        Debug.Log(
            "登録オブジェクトを破壊し、ゴールを残したままクリア処理を実行しました"
        );
    }
}
