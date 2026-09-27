using UnityEngine;
using UnityEngine.InputSystem;
using RogueLike.UI;

namespace RogueLike.Player
{
    /// <summary>
    /// Polls the new Input System directly (keyboard + gamepad) and blends in
    /// the on-screen MoveController when one exists in the scene, so it works
    /// standalone with no wiring — desktop testing needs no joystick present,
    /// and mobile needs no keyboard.
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public bool PausePressed { get; private set; }

        private void Update()
        {
            Vector2 move = Vector2.zero;
            bool pause = false;

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                {
                    move.x -= 1f;
                }
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                {
                    move.x += 1f;
                }
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                {
                    move.y -= 1f;
                }
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                {
                    move.y += 1f;
                }

                pause |= keyboard.escapeKey.wasPressedThisFrame;
            }

            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                move += gamepad.leftStick.ReadValue();
                pause |= gamepad.startButton.wasPressedThisFrame;
            }

            if (MoveController.Instance != null)
            {
                move += MoveController.Instance.Direction;
            }

            MoveInput = Vector2.ClampMagnitude(move, 1f);
            PausePressed = pause;
        }
    }
}
