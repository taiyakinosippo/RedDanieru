using System.Collections.Generic;
using UnityEngine;

public class UndoManager : MonoBehaviour
{
    [SerializeField] private MapManager mapManager;

    // 戻る履歴
    private Stack<DungeonMapData> undoStack = new Stack<DungeonMapData>();

    // 進む履歴
    private Stack<DungeonMapData> redoStack = new Stack<DungeonMapData>();

    /// 編集前の状態を保存
    public void SaveState()
    {
        undoStack.Push(mapManager.CreateSaveData());

        // 新しい編集をしたらRedo履歴は消す
        redoStack.Clear();
    }

    /// 一つ前に戻す
    public void Undo()
    {
        if (undoStack.Count == 0)
            return;

        // 現在の状態をRedoへ保存
        redoStack.Push(mapManager.CreateSaveData());

        // 一つ前へ戻す
        mapManager.LoadDungeon(undoStack.Pop());
    }

    /// 一つ進める
    public void Redo()
    {
        if (redoStack.Count == 0)
            return;

        // 現在の状態をUndoへ保存
        undoStack.Push(mapManager.CreateSaveData());

        // 次の状態へ進む
        mapManager.LoadDungeon(redoStack.Pop());
    }

    /// 履歴を削除
    public void ClearHistory()
    {
        undoStack.Clear();
        redoStack.Clear();
    }
}