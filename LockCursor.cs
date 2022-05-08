using UnityEngine;
using UnityEngine.InputSystem;

namespace MarcosPereira.UnityUtilities {
    public class LockCursor : MonoBehaviour {
        public void Start() {
            // Lock and hide cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        public void Update() {
            if (Keyboard.current.escapeKey.wasPressedThisFrame) {
                if (Cursor.lockState == CursorLockMode.Locked) {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                } else if (Cursor.lockState == CursorLockMode.None) {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }
#endif
    }
}
