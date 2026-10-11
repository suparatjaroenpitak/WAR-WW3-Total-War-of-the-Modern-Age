using System;
using UnityEngine;

namespace WarWW3
{
    public sealed class RtsCamera : MonoBehaviour
    {
        public Func<bool> CanControl;
        Vector3 focus = new Vector3(46, 0, 44);
        float yaw = 0, distance = 64;
        void LateUpdate()
        {
            if (CanControl != null && CanControl() && Application.isFocused)
            {
                float horizontal = (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0);
                float vertical = (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0);
                Vector3 mouse = Input.mousePosition;
                bool inside = mouse.x >= 0 && mouse.y >= 0 && mouse.x <= Screen.width && mouse.y <= Screen.height;
                if (inside && !Input.GetMouseButton(0) && !GameUI.PointerOverUI())
                {
                    if (mouse.x < 12) horizontal -= 1; if (mouse.x > Screen.width - 12) horizontal += 1;
                    if (mouse.y < 12) vertical -= 1; if (mouse.y > Screen.height - 12) vertical += 1;
                }
                var rotation = Quaternion.Euler(0, yaw, 0);
                focus += rotation * Vector3.ClampMagnitude(new Vector3(horizontal, 0, vertical), 1) * (Input.GetKey(KeyCode.LeftShift) ? 55 : 30) * Time.unscaledDeltaTime;
                float spin = (Input.GetKey(KeyCode.E) ? 1 : 0) - (Input.GetKey(KeyCode.Q) ? 1 : 0);
                if (Input.GetMouseButton(2)) spin += Input.GetAxis("Mouse X") * 2;
                yaw += spin * 70 * Time.unscaledDeltaTime;
                if (!GameUI.PointerOverUI()) distance = Mathf.Clamp(distance - Input.mouseScrollDelta.y * 5, 20, 105);
                focus.x = Mathf.Clamp(focus.x, 0, 128); focus.z = Mathf.Clamp(focus.z, 0, 96);
            }
            transform.rotation = Quaternion.Euler(57, yaw, 0);
            transform.position = focus - transform.forward * distance;
        }
        public void Focus(Vector3 position) { focus = position; }
    }
}
