using UnityEngine;

public class CursorController : MonoBehaviour
{
    // カーソルを表示する
    public void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // カーソルを非表示にする
    public void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
