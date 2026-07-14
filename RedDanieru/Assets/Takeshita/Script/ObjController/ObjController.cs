using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjController : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public int playerRole;

    public override void FixedUpdateNetwork()
    {

        if (!HasStateAuthority)
            return;

        if (PlayerSpawner.MyRole != playerRole)
            return;

        Vector3 move = Vector3.zero;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.wKey.isPressed)
            move += Vector3.forward;

        if (Keyboard.current.sKey.isPressed)
            move += Vector3.back;

        if (Keyboard.current.aKey.isPressed)
            move += Vector3.left;

        if (Keyboard.current.dKey.isPressed)
            move += Vector3.right;

        transform.position += move.normalized * moveSpeed * Runner.DeltaTime;
    }

    public override void Spawned()
{
        Debug.Log($"MyRole : {PlayerSpawner.MyRole}");
        //Debug.Log($"{name}");
        //Debug.Log($"InputAuthority : {Object.InputAuthority}");
        //Debug.Log($"HasInputAuthority : {HasInputAuthority}");
        //Debug.Log($"HasStateAuthority : {HasStateAuthority}");
    }
}