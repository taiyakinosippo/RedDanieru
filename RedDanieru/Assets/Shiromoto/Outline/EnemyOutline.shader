// 敵のアウトライン（控えめ）
// ・ある程度近づくと、暗い中でも敵の輪郭がうっすら見える
// ・_ShowDistance より近いと見え、_HideDistance に向かって薄くなり、それより遠いと見えない
// ・線の太さは画面上のピクセル数で決める。ライトと霧の影響は受けない
// ・裏面を法線方向に少し膨らませて1色で塗る
Shader "Shiromoto/EnemyOutline"
{
    Properties
    {
        _OutlineColor("線の色（A = 濃さ）", Color) = (0.55, 0.62, 0.75, 0.6)
        _OutlineWidth("線の太さ (px)", Range(0, 10)) = 1.5
        _ShowDistance("この距離より近いと見える (m)", Float) = 10
        _HideDistance("この距離より遠いと見えない (m)", Float) = 14
    }

    SubShader
    {
        // 元の不透明マテリアルが描かれた後に、半透明で重ねる
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent-10" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Front
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                float _OutlineWidth;
                float _ShowDistance;
                float _HideDistance;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half fade : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                float4 positionCS = positions.positionCS;

                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float2 normalCS = mul((float3x3)GetWorldToHClipMatrix(), normalWS).xy;

                // 画面のピクセル単位で向きを決める（縦横比で線の太さがずれないように）
                float2 dirPixel = normalCS * _ScreenParams.xy;
                float len = length(dirPixel);
                dirPixel = len > 1e-5 ? dirPixel / len : float2(0, 0);

                // ピクセル → クリップ座標（画面の端から端が 2）。w を掛けて距離に関係なく同じ太さにする
                positionCS.xy += dirPixel * _OutlineWidth * 2.0 / _ScreenParams.xy * positionCS.w;

                // 近いと 1、遠いと 0
                float distance = length(GetCameraPositionWS() - positions.positionWS);
                output.fade = 1.0 - saturate((distance - _ShowDistance) / max(_HideDistance - _ShowDistance, 0.001));

                output.positionCS = positionCS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half alpha = _OutlineColor.a * input.fade;
                clip(alpha - 0.001);  // 見えない所は奥行きも書かない
                return half4(_OutlineColor.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
