/// <summary>
/// PHPサーバーのURL（変更するときはここだけ直す）
/// </summary>
public static class ServerApi
{
    public const string BaseUrl = "http://10.219.32.66/RedDaniel/";

    public static string Url(string file)
    {
        return BaseUrl + file;
    }
}
