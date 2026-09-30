using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class UIManager : MonoBehaviour
{
    [Header("ダメージ関係のUI")]
    [SerializeField] private DamagePopup _damagePopupPrefab;
    [SerializeField] private Transform damagePopupParent;

    public void ShowDamage(int damage, Vector3 position)
    {
        DamagePopup popup = 
            Instantiate(_damagePopupPrefab,position,Quaternion.identity,damagePopupParent);

        popup.DamageSetUp(damage);
    }
}
