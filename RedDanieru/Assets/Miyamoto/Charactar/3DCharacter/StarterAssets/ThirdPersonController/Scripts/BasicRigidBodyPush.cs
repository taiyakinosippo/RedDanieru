using UnityEngine;

/// <summary>
/// 押せるオブジェクト判定するスクリプト
/// </summary>
public class BasicRigidBodyPush : MonoBehaviour
{
	public LayerMask pushLayers;
	public bool canPush;
	[Range(0.5f, 5f)] public float strength = 1.1f;

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (canPush) PushRigidBodies(hit);
	}

	private void PushRigidBodies(ControllerColliderHit hit)
	{
        // https://docs.unity3d.com/ScriptReference/CharacterController.OnControllerColliderHit.html

        // rigitbodyがアタッチされていない場合は処理しないまた、isKinematicがtrueの場合も処理しない
        Rigidbody body = hit.collider.attachedRigidbody;
		if (body == null || body.isKinematic) return;

		// レイヤーでの判定(違うレイヤーの場合は処理しない)
		var bodyLayerMask = 1 << body.gameObject.layer;
		if ((bodyLayerMask & pushLayers.value) == 0) return;

        // 下方向での衝突は無視する（横方向に押す場合のみ処理する）
        if (hit.moveDirection.y < -0.3f) return;

		// 押す方向を計算する（水平方向のみ）
		Vector3 pushDir = new Vector3(hit.moveDirection.x, 0.0f, hit.moveDirection.z);

        // 押した方向に力を加える
        body.AddForce(pushDir * strength, ForceMode.Impulse);
	}
}