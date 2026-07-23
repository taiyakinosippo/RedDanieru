using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
#endif

namespace Player
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool cameraChange;
		public bool sticker;
		public bool debuger;

        [Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;
        public bool attack;

#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void OnAttack(InputValue value)
		{
            Debug.Log("Attack");
            AttackInput(value.isPressed);
		}

		public void OnSticker(InputValue value)
		{
			StickerInput(value.isPressed);
		}

		public void OnCameraChange(InputValue value)
        {
            Debug.Log("CameraChange");
            CameraChangeInput(value.isPressed); 
        }
        public void OnDebug(InputValue value)
        {
            Debug.Log("Debug");
            DebugInput(value.isPressed);
        }
#endif


        public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		public void AttackInput(bool newAttackState)
        {
            attack = newAttackState;
        }

		public void StickerInput(bool newStickerState)
		{
			sticker = newStickerState;
		}
        public void CameraChangeInput(bool newCameraChangeState)
		{
            cameraChange = newCameraChangeState;
        }

		public void DebugInput(bool newDebugState)
		{
			debuger = newDebugState;
		}

        private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}