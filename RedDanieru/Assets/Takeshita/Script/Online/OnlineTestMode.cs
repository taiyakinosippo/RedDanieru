using System;
using UnityEngine;

/// <summary>
/// テストプレイ用モード（PHPサーバーなしでマルチプレイ）の切り替え
///
/// 有効にする方法
///   エディタ : メニュー「RedDanieru > オンラインテストモード」
///   ビルド   : 起動引数に -localtest を付ける
///
/// 有効時は
///   ・ルーム一覧/検索は同じPC内のファイルで共有（LocalTestServer）
///   ・ステージはローカル保存したマップ（persistentDataPath と StreamingAssets/TestStages）から読む
/// 通信そのものは Photon Cloud を使うので、インターネット接続は必要
/// </summary>
public static class OnlineTestMode
{
    public const string EditorPrefsKey = "RedDanieru.OnlineTestMode";
    public const string CommandLineFlag = "-localtest";

    private static bool? cached;

    public static bool Enabled
    {
        get
        {
            if (cached == null)
            {
                cached = ReadSetting();
            }

            return cached.Value;
        }
    }

    // エディタのメニューから切り替えたときに呼ぶ
    public static void Refresh()
    {
        cached = null;
    }

    private static bool ReadSetting()
    {
        foreach (string arg in Environment.GetCommandLineArgs())
        {
            if (string.Equals(arg, CommandLineFlag, StringComparison.OrdinalIgnoreCase))
                return true;
        }

#if UNITY_EDITOR
        return UnityEditor.EditorPrefs.GetBool(EditorPrefsKey, false);
#else
        return false;
#endif
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        cached = null;
    }
}
