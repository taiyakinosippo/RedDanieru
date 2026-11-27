using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using System.Collections;

public class GetDamageManager : MonoBehaviour
{
    private Coroutine flameCoroutine;
    private Coroutine shakeCoroutine;

    [SerializeField] private Camera myCamera;
    [SerializeField] private Renderer damageRenderer;
    private UniversalAdditionalCameraData cameraData;

    private List<RectTransform> shakeTargets = new();
    private List<Vector3> defaultPositions = new();


    private float effectPower = 0;


    void Start()
    {
        if (myCamera == null) myCamera = Camera.main;
        cameraData = myCamera.GetComponent<UniversalAdditionalCameraData>();

        shakeTargets.Add(GameObject.Find("PlayerUI").GetComponent<RectTransform>());
        shakeTargets.Add(GameObject.Find("StickerUI").GetComponent<RectTransform>());
        foreach (RectTransform pos in shakeTargets) defaultPositions.Add(pos.localPosition);
    }

    //ダメージ受けた時呼び出し
    public void GetDamage()
    {
        effectPower += 1.0f;
        if (effectPower >= 2.0f) effectPower = 2.0f;

        if (flameCoroutine == null)
            StartCoroutine(ChangeRenderer());
        if (shakeCoroutine == null)
            StartCoroutine(ShakeUI());
        StartCoroutine(PlayEffect());
    }

    //カメラの色を変えるゾ
    private IEnumerator ChangeRenderer()
    {
        while (effectPower > 0)
        {
            effectPower -= Time.deltaTime;

            Color color = damageRenderer.material.color;
            color.a = Mathf.Clamp01(effectPower);

            damageRenderer.material.color = color;

            yield return null;
        }
    }

    //出してるUIを揺らすゾ
    private IEnumerator ShakeUI()
    {
        float timer = 0.1f;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            Vector3 shake = (Vector3)Random.insideUnitCircle * 10f;

            for (int i = 0; i < shakeTargets.Count; i++)
            {
                shakeTargets[i].localPosition = defaultPositions[i] + shake;
            }

            yield return null;
        }

        for (int i = 0; i < shakeTargets.Count; i++)
        {
            shakeTargets[i].localPosition = defaultPositions[i];
        }
    }

    //体からダメージエフェクトを出すゾ
    private IEnumerator PlayEffect()
    {
        yield return new WaitForSeconds(0.0f);
    }
}
