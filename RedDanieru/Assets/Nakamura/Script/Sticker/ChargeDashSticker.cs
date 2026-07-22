using UnityEngine;

public class ChargeDashSticker : StickerBase
{
    private float dashSpeed = 10.0f; //突進速度
    private float chargeTime = 2.0f; //溜め時間
    private float chargeTimer = 0.0f; //溜め時間のタイマー

    private void Start()
    {
        chargeTimer = chargeTime;
    }

    public override void OnEnemyApply()
    {

    }

    public override void OnEnemyRemove()
    {
    }

    public override void OnTrapApply()
    {
    }

    public override void OnTrapRemove()
    {
    }

    //突進
    private void ChargeDash()
    {
        if (chargeTimer <= 0.0f)
        {

        }
        else
            chargeTimer -= Time.deltaTime;
    }
}
