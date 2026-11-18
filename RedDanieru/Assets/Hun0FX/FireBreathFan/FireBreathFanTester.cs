using Hun0FX.Demo;
using UnityEngine;

/// <summary>
/// テストシーン用：扇形ブレスの数値を画面のスライダーで変えて見比べる。
/// 画面に出ている FireBreathFan すべてに、スライダーの数値を入れる。
/// 気に入ったら「数値をコンソールに出す」を押し、その数値をプレハブの FireBreathFan に入れる。
/// </summary>
public class FireBreathFanTester : MonoBehaviour
{
    [Tooltip("最初のスライダーの数値を読むプレハブ（FX_Fire_06_Fan）")]
    public FireBreathFan source;

    float spreadAngle, flatness, emissionRate, reach;

    void Start()
    {
        ResetToSource();
    }

    void ResetToSource()
    {
        if (source == null)
            return;

        spreadAngle = source.spreadAngle;
        flatness = source.flatness;
        emissionRate = source.emissionRate;
        reach = source.reach;
    }

    void Update()
    {
        //見本が切り替わるたびに作り直されるので、毎フレーム数値を入れておく
        foreach (FireBreathFan fan in FindObjectsByType<FireBreathFan>(FindObjectsSortMode.None))
        {
            fan.spreadAngle = spreadAngle;
            fan.flatness = flatness;
            fan.emissionRate = emissionRate;
            fan.reach = reach;
        }
    }

    void OnGUI()
    {
        DemoGUI.Begin();

        float w = 340f, h = 210f;
        GUILayout.BeginArea(new Rect((DemoGUI.W - w) * 0.5f, DemoGUI.H - h - 16f, w, h));
        GUILayout.BeginVertical(DemoGUI.Box);
        GUILayout.Label("FireBreathFan（Fire_06_Fan のときだけ効く）", DemoGUI.Small);

        spreadAngle = DemoGUI.Slider("角度", spreadAngle, 0f, 80f);
        flatness = DemoGUI.Slider("平たさ", flatness, 0.05f, 1f);
        emissionRate = DemoGUI.Slider("量", emissionRate, 5f, 100f);
        reach = DemoGUI.Slider("距離", reach, 0.3f, 3f);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("数値をコンソールに出す"))
        {
            Debug.Log($"FireBreathFan  spreadAngle: {spreadAngle:0.##}  flatness: {flatness:0.##}  " +
                      $"emissionRate: {emissionRate:0.#}  reach: {reach:0.##}");
        }
        if (GUILayout.Button("プレハブの数値に戻す"))
        {
            ResetToSource();
        }
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}
