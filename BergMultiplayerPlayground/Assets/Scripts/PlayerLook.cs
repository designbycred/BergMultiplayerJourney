using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] private Transform cameraPivot; // drag CameraPivot
    [SerializeField] private float sensitivity = 0.1f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    private Vector2 look;
    private float pitch;

    void Awake()
    {
        if (!cameraPivot) cameraPivot = transform.Find("CameraPivot");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        look = ctx.ReadValue<Vector2>();
    }

    void Update()
    {
        // yaw (left/right) rotates the player root
        float yaw = look.x * sensitivity;
        transform.Rotate(0f, yaw, 0f);

        // pitch (up/down) rotates the camera pivot
        pitch -= look.y * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        cameraPivot.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }
}
