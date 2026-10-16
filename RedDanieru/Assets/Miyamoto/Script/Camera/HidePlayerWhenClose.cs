using UnityEngine;

/// <summary>
/// プレイヤーがカメラに近づいたときに、プレイヤーのレンダラーを非表示にするスクリプト
/// </summary>
public class HidePlayerWhenClose : MonoBehaviour
{
    [SerializeField] private GameObject _playerRenderers;       // 子をオブジェクトも含めたプレイヤーのレンダラーを非表示にするため配列
    [SerializeField] private float _hideDistance = 2.0f;        // プレイヤーを非表示にする距離
    [SerializeField] private float _showDistance = 3.0f;        // プレイヤーを表示にする距離
    [SerializeField] private Vector3 _playerOffset = new Vector3(0, 1.0f, 0); // プレイヤーの中心位置を調整するためのオフセット
    private bool _hidden = false;
    private Renderer[] _targets;


    private void Start()
    {
        _targets = _playerRenderers.GetComponentsInChildren<Renderer>(true);
    }

    //----------------------------------------------------------------
    // プレイヤーとカメラの距離を測定して、非表示にするかどうかを判定する
    //----------------------------------------------------------------

    public void isWithinHideDistance(GameObject camera)
    {
        float dist = Vector3.Distance(camera.transform.position, transform.position + _playerOffset);

        if (!_hidden && dist < _hideDistance) SetHidden(true);
        else if (_hidden && dist > _showDistance) SetHidden(false);
    }

    //----------------------------------------------------------------
    // プレイヤーのレンダラーを非表示にする(影は残す)
    //----------------------------------------------------------------
    private void SetHidden(bool hide)
    {
        _hidden = hide;
        foreach (var r in _targets)
        {
            // 影だけ残す(体が消えても影は出る)
            r.shadowCastingMode = hide
                ? UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly
                : UnityEngine.Rendering.ShadowCastingMode.On;
        }
    }
}
