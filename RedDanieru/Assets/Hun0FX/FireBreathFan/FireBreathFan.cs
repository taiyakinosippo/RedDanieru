using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 炎ブレス（FX_Fire_06）を扇形に広げる。
/// 円すい（Cone）に広がる炎の層をまとめて、横に広く・縦に平たくする。
/// 炎の芯（Fire_Mesh）と口元の輪（Shockwave_Ring）はそのまま。
///
/// 再生中に Inspector で数値を変えるとすぐ反映される。
/// 再生を止めると数値は戻るので、気に入った数値は止めてからプレハブに入れ直す
/// （テストシーンの「数値をコンソールに出す」ボタンで確認できる）。
///
/// プレハブには同じ数値を書き込んであるので、再生していなくても扇形で見える。
/// ただし「届く距離」だけは再生中にしか反映されない。
/// </summary>
[DisallowMultipleComponent]
public class FireBreathFan : MonoBehaviour
{
    [Tooltip("扇の片側の角度（度）。30なら左右合わせて60度に広がる")]
    [Range(0f, 80f)] public float spreadAngle = 30f;

    [Tooltip("縦の広がり。1で丸い円すい、小さいほど平たい扇")]
    [Range(0.05f, 1f)] public float flatness = 0.3f;

    [Tooltip("1秒あたりに出す炎の数（扇の層ごと）。広げると薄く見えるので元の15より多め")]
    [Range(5f, 100f)] public float emissionRate = 30f;

    [Tooltip("炎の届く距離の倍率（炎の速さを変える）。1で元のまま")]
    [Range(0.3f, 3f)] public float reach = 1f;

    class Layer
    {
        public ParticleSystem ps;
        public float angleOffset;   //いちばん細い層との角度の差（火の粉は少し広めなど、元の差を残す）
        public Vector3 shapeScale;
        public ParticleSystem.MinMaxCurve startSpeed;
    }

    List<Layer> layers;
    float appliedAngle = -1f, appliedFlatness = -1f, appliedRate = -1f, appliedReach = -1f;

    void Awake()
    {
        Cache();
    }

    void OnEnable()
    {
        Apply();
    }

    void Update()
    {
        //Inspector で数値が変わったら反映する
        if (spreadAngle != appliedAngle || flatness != appliedFlatness ||
            emissionRate != appliedRate || reach != appliedReach)
        {
            Apply();
        }
    }

    //円すいに広がる層を集める（角度0の芯や、形を使っていない層は除く）
    void Cache()
    {
        if (layers != null)
            return;

        layers = new List<Layer>();
        float minAngle = float.MaxValue;

        foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>(true))
        {
            var shape = ps.shape;
            if (!shape.enabled || shape.shapeType != ParticleSystemShapeType.Cone || shape.angle <= 0f)
                continue;

            layers.Add(new Layer
            {
                ps = ps,
                angleOffset = shape.angle,
                shapeScale = shape.scale,
                startSpeed = ps.main.startSpeed,
            });

            minAngle = Mathf.Min(minAngle, shape.angle);
        }

        foreach (Layer layer in layers)
        {
            layer.angleOffset -= minAngle;
        }
    }

    public void Apply()
    {
        Cache();

        foreach (Layer layer in layers)
        {
            var shape = layer.ps.shape;
            shape.angle = Mathf.Min(spreadAngle + layer.angleOffset, 89f);

            //形は Y 軸まわりに回して +X 向きにしてあるので、Y を縮めると縦だけ平たくなる
            shape.scale = new Vector3(layer.shapeScale.x, flatness, layer.shapeScale.z);

            var emission = layer.ps.emission;
            emission.rateOverTime = emissionRate;

            var main = layer.ps.main;
            main.startSpeed = Scale(layer.startSpeed, reach);
        }

        appliedAngle = spreadAngle;
        appliedFlatness = flatness;
        appliedRate = emissionRate;
        appliedReach = reach;
    }

    static ParticleSystem.MinMaxCurve Scale(ParticleSystem.MinMaxCurve m, float k)
    {
        switch (m.mode)
        {
            case ParticleSystemCurveMode.Constant:
                return new ParticleSystem.MinMaxCurve(m.constant * k);
            case ParticleSystemCurveMode.TwoConstants:
                return new ParticleSystem.MinMaxCurve(m.constantMin * k, m.constantMax * k);
            case ParticleSystemCurveMode.Curve:
                return new ParticleSystem.MinMaxCurve(m.curveMultiplier * k, m.curve);
            case ParticleSystemCurveMode.TwoCurves:
                return new ParticleSystem.MinMaxCurve(m.curveMultiplier * k, m.curveMin, m.curveMax);
        }
        return m;
    }
}
