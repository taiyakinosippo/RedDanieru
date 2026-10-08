using UnityEditor;

/// <summary>
/// メニュー「RedDanieru > オンラインテストモード（サーバーなし）」で切り替える
/// Multiplayer Play Mode の仮想プレイヤーにも同じ設定が使われる
/// </summary>
public static class OnlineTestModeMenu
{
    private const string MenuPath = "RedDanieru/オンラインテストモード（サーバーなし）";

    [MenuItem(MenuPath)]
    private static void Toggle()
    {
        bool enabled = !EditorPrefs.GetBool(OnlineTestMode.EditorPrefsKey, false);

        EditorPrefs.SetBool(OnlineTestMode.EditorPrefsKey, enabled);
        OnlineTestMode.Refresh();

        EditorUtility.DisplayDialog(
            "オンラインテストモード",
            enabled
                ? "有効にしました。\nルームとステージはPHPサーバーを使わず、このPC内で管理します。\nステージはマップエディタでローカル保存したものと StreamingAssets/TestStages のものが選べます。"
                : "無効にしました。PHPサーバーを使います。",
            "OK");
    }

    [MenuItem(MenuPath, true)]
    private static bool ToggleValidate()
    {
        Menu.SetChecked(MenuPath, EditorPrefs.GetBool(OnlineTestMode.EditorPrefsKey, false));
        return true;
    }
}
