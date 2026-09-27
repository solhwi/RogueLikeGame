using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace RogueLike.UI
{
    // Floating virtual joystick: this component lives on a full-screen touch
    // zone, and the joystick base appears wherever a press starts, then
    // steers by dragging relative to that point. It hides again on release
    // and direction snaps back to zero. PlayerInputHandler blends Direction
    // in alongside keyboard/gamepad input via the static Instance, so
    // gameplay code needs no inspector wiring to pick it up.
    //
    // The UI event system is only trusted for the press start; after that the
    // pressing device itself is polled every frame for position and release.
    // Relying on OnDrag/OnPointerUp alone let the player stop mid-hold once
    // the finger stopped moving.
    public class MoveController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField] private float handleRange = 60f;

        public static MoveController Instance { get; private set; }

        public Vector2 Direction { get; private set; }

        // Only the finger that spawned the joystick steers it; extra touches
        // landing on the zone mid-drag are ignored.
        private int activePointerId = NoPointer;
        private const int NoPointer = int.MinValue;

        // Device (and touch id, for touchscreens) that owns the current press,
        // polled in Update. Null when the event data didn't come from the
        // Input System UI module, in which case the UI events drive everything.
        private InputDevice activeDevice;
        private int activeTouchId;
        private Camera activeEventCamera;

        private void Awake()
        {
            Instance = this;
            SetJoystickVisible(false);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnDisable()
        {
            Release();
        }

        private void Update()
        {
            if (activePointerId == NoPointer || activeDevice == null)
            {
                return;
            }

            if (TryReadActivePointer(out Vector2 screenPosition))
            {
                Steer(screenPosition);
            }
            else
            {
                Release();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (activePointerId != NoPointer)
            {
                return;
            }
            activePointerId = eventData.pointerId;
            activeEventCamera = eventData.pressEventCamera;

            activeDevice = null;
            activeTouchId = 0;
            if (eventData is ExtendedPointerEventData extended)
            {
                activeDevice = extended.device;
                activeTouchId = extended.touchId;
            }

            if (background != null &&
                RectTransformUtility.ScreenPointToWorldPointInRectangle(background, eventData.position, activeEventCamera, out Vector3 worldPoint))
            {
                background.position = worldPoint;
            }

            SetJoystickVisible(true);
            Steer(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId)
            {
                return;
            }
            Steer(eventData.position);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // When the device is polled, Update decides when the press really ends.
            if (eventData.pointerId != activePointerId || activeDevice != null)
            {
                return;
            }
            Release();
        }

        private bool TryReadActivePointer(out Vector2 screenPosition)
        {
            screenPosition = default;

            if (activeDevice is Touchscreen touchscreen)
            {
                foreach (var touch in touchscreen.touches)
                {
                    if (touch.touchId.ReadValue() != activeTouchId)
                    {
                        continue;
                    }
                    if (!touch.press.isPressed)
                    {
                        return false;
                    }

                    screenPosition = touch.position.ReadValue();
                    return true;
                }
                return false;
            }

            if (activeDevice is Pointer pointer && activeDevice.added)
            {
                if (!pointer.press.isPressed)
                {
                    return false;
                }

                screenPosition = pointer.position.ReadValue();
                return true;
            }

            return false;
        }

        private void Steer(Vector2 screenPosition)
        {
            if (background == null)
            {
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(background, screenPosition, activeEventCamera, out Vector2 localPoint);

            Vector2 offset = Vector2.ClampMagnitude(localPoint, handleRange);
            if (handle != null)
            {
                handle.anchoredPosition = offset;
            }
            Direction = offset / handleRange;
        }

        private void Release()
        {
            activePointerId = NoPointer;
            activeDevice = null;
            activeEventCamera = null;
            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
            }
            Direction = Vector2.zero;
            SetJoystickVisible(false);
        }

        private void SetJoystickVisible(bool visible)
        {
            // Never deactivate our own GameObject, or we'd stop receiving input.
            if (background != null && background.gameObject != gameObject)
            {
                background.gameObject.SetActive(visible);
            }
        }
    }
}
