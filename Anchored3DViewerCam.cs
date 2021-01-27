/*** A simple script for a runtime mouse-controlled 3D-Viewer camera akin to Blender's default and similar 3D software.
Just attach this script to your camera and it should work.
Sorry for the seemingly arbitrary numerical values for sensivity and smoothness; these were the values that gave me the best results.
Cheers!***/

using UnityEngine;
using UnityEngine.EventSystems;

public class Anchored3DViewerCam : MonoBehaviour
{
    [SerializeField]private Texture2D cursorIdleTexture;
    [SerializeField]private Texture2D cursorMoveTexture;
    [SerializeField]private Texture2D cursorRotateTexture;
    
    [SerializeField]private float rotationSensitivity = 50f;
    [SerializeField]private float moveSensitivity = 5f;
    [SerializeField]private float zoomSensitivity = 1000f;
    
    [Tooltip("Distance from rotation pivot point")]
    [SerializeField]private float distance = 10f;

    private const CursorMode cursorMode = CursorMode.Auto;

    private Transform anchor;
    private Quaternion targetRotCam;
    private Vector3 camRotFocus, camLookFocus, targetAnchor, dampVelAnchor, dampVelCamPos;
    private Vector2 mouseDelta;
    private float xAngle, yAngle;
    private bool rotating, moving;

    private const float anchorRotSmooth = 16f;

    private void Awake()
    {
        Cursor.SetCursor(cursorIdleTexture, Vector2.zero, cursorMode);

        anchor = new GameObject("CameraControlAnchor").transform;
        anchor.position = transform.position + (transform.forward * distance);
        anchor.rotation = Quaternion.identity;
        transform.SetParent(anchor);
    }

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        xAngle = angles.y;
        yAngle = angles.x;
        targetAnchor = anchor.position;
        targetRotCam = transform.rotation;
    }

    private void Update()
    {
        mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * distance;
        CheckMovement();
        CheckRotation();
        CheckZoom();
    }

    private void LateUpdate()
    {
        anchor.position = Vector3.SmoothDamp(anchor.position, targetAnchor, ref dampVelAnchor, 0.05f, float.MaxValue, Time.deltaTime);
        anchor.rotation = Quaternion.Slerp(transform.rotation, targetRotCam, Time.deltaTime * anchorRotSmooth);
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, new Vector3(0f, 0f, -distance), ref dampVelCamPos, 0.05f, float.MaxValue, Time.deltaTime);
    }

    private void CheckMovement()
    {
        if (Input.GetMouseButton(2))
        {
            if (Input.GetMouseButtonDown(2) && MouseNotOnMenu)
            {
                Cursor.SetCursor(cursorMoveTexture, Vector2.one * 0.5f, cursorMode);
                moving = true;
            }

            if (!moving) return;

            float movementX = mouseDelta.x * moveSensitivity * Time.deltaTime;
            float movementY = mouseDelta.y * moveSensitivity * Time.deltaTime;

            targetAnchor += transform.TransformDirection(-movementX, -movementY, 0f);
        }
        else if (Input.GetMouseButtonUp(2) && MouseNotOnMenu)
        {
            Cursor.SetCursor(cursorIdleTexture, Vector2.zero, cursorMode);
            moving = false;
        }
    }

    private void CheckRotation()
    {
        if (Input.GetMouseButton(1))
        {
            if (Input.GetMouseButtonDown(1) && MouseNotOnMenu)
            {
                Cursor.SetCursor(cursorRotateTexture, Vector2.one * 0.5f, cursorMode);
                rotating = true;
            }

            if (!rotating) return;

            xAngle += mouseDelta.x * rotationSensitivity * Time.deltaTime;
            yAngle -= mouseDelta.y * rotationSensitivity * Time.deltaTime;

            targetRotCam = Quaternion.Euler(yAngle, xAngle, 0f);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            Cursor.SetCursor(cursorIdleTexture, Vector2.zero, cursorMode);
            rotating = false;
        }
    }

    private void CheckZoom()
    {
        Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);

        if (!screenRect.Contains(Input.mousePosition) && MouseNotOnMenu) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (IsBetweenRange(scroll, -0.01f, 0.01f) || !MouseNotOnMenu) return;

        distance -= (scroll * zoomSensitivity * Time.deltaTime);

        if (!(distance < 0.5f)) return;
        
        targetAnchor += transform.TransformDirection(Vector3.forward * ((distance - 0.5f) * -0.1f));
        distance = 0.5f;
    }

    private static bool MouseNotOnMenu => !EventSystem.current.IsPointerOverGameObject();

    private static bool IsBetweenRange(float value, float min, float max)
    {
        return (value > min && value < max);
    }
}