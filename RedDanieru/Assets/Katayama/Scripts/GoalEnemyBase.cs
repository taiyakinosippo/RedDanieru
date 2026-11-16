
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

        // 通常の敵AIによる状態遷移を開始させない
        currentState = enemyState.Idle;

        // ネットワーク側で参照されるHPも初期化
        currentHp = goalCurrentHP;
    }

    public override void Start()
    {
        // EnemyBase.Startの初期化処理は実行しない
    }

    public override void Damage(int playerPow)
    {
        Debug.Log("GoalEnemyBaseのDamageが呼ばれました");

        if (isGoalDestroyed)
            return;

        int damage = Mathf.Max(1, playerPow);

        goalCurrentHP = Mathf.Max(0, goalCurrentHP - damage);
        currentHp = goalCurrentHP;

        Debug.Log($"ゴールHP：{goalCurrentHP}/{goalMaxHP}");

        if (goalCurrentHP <= 0)
            DestroyGoal();
    }

    private void DestroyGoal()
    {
        if (isGoalDestroyed)
            return;

        isGoalDestroyed = true;

        // 以降の通常攻撃を受け付けない
        currentState = enemyState.Dead;

        Debug.Log("ゴール破壊！");

        // 指定されたオブジェクトを破壊
        if (destroyObjectsOnBreak != null)
        {
            foreach (GameObject obj in destroyObjectsOnBreak)
            {
                if (obj == null || obj == gameObject)
                    continue;

                Destroy(obj);
            }
        }

        // ゴール破壊によるクリア処理
        GoalClear goalClear = FindObjectOfType<GoalClear>();

        if (goalClear == null)
        {
            Debug.LogError("GoalClearがシーン内に見つかりません。");
            return;
        }

        goalClear.ClearByGoalDestroyed();

        // ゴール本体は破壊しない
        Debug.Log("登録オブジェクトを破壊し、ゴールを残しました。");
    }
}
