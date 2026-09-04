using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UndoManager : MonoBehaviour
{
    [SerializeField]
    private MapManager mapManager;

    // 戻る履歴
    private Stack<DungeonMapData> undoStack =
        new Stack<DungeonMapData>();

    // 進む履歴
    private Stack<DungeonMapData> redoStack =
        new Stack<DungeonMapData>();

    // 編集中か
    private bool isEditing = false;


    //==================================================
    // 編集開始
    //==================================================

    public void BeginEdit()
    {
        if (isEditing)
            return;

        isEditing = true;

        DungeonMapData data =
            mapManager.CreateSaveData();

        Debug.Log(
            "保存: objects数 = "
            + data.objects.Count
        );

        undoStack.Push(data);

        redoStack.Clear();
    }


    //==================================================
    // 編集終了
    //==================================================

    public void EndEdit()
    {
        isEditing = false;
    }


    //==================================================
    // 一つ前に戻す
    //==================================================

    public void Undo()
    {
        if (undoStack.Count == 0)
        {
            Debug.Log("Undo履歴なし");
            return;
        }

        DungeonMapData current =
            mapManager.CreateSaveData();

        Debug.Log(
            "現在objects数 = "
            + current.objects.Count
        );

        DungeonMapData undoData =
            undoStack.Pop();

        Debug.Log(
            "戻すobjects数 = "
            + undoData.objects.Count
        );

        redoStack.Push(current);


        // 現在の敵を完全に停止
        StopEnemies();


        // 敵を動かさずにマップを復元
        mapManager.LoadDungeon(
            undoData,
            false
        );
    }


    //==================================================
    // 一つ進める
    //==================================================

    public void Redo()
    {
        if (redoStack.Count == 0)
            return;

        DungeonMapData current =
            mapManager.CreateSaveData();

        undoStack.Push(current);

        DungeonMapData redoData =
            redoStack.Pop();


        // 現在の敵を完全に停止
        StopEnemies();


        // 敵を動かさずにマップを復元
        mapManager.LoadDungeon(
            redoData,
            false
        );
    }


    //==================================================
    // 敵を停止
    //==================================================

    private void StopEnemies()
    {
        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag(
                "Enemy"
            );

        foreach (
            GameObject enemy
            in enemies
        )
        {
            NavMeshAgent[] agents =
                enemy.GetComponentsInChildren<
                    NavMeshAgent
                >();

            foreach (
                NavMeshAgent agent
                in agents
            )
            {
                if (!agent.enabled)
                    continue;

                agent.isStopped = true;

                agent.ResetPath();

                agent.velocity =
                    Vector3.zero;

                agent.enabled = false;
            }
        }
    }


    //==================================================
    // 履歴削除
    //==================================================

    public void ClearHistory()
    {
        undoStack.Clear();

        redoStack.Clear();

        isEditing = false;
    }
}