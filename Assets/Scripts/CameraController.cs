using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float panSensitivity;
    [SerializeField] float zoomSensitivity;
    [SerializeField] float screenSizeDivisor;

    Camera gameCamera;

    private void Start()
    {
        gameCamera = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        var panned = false;

        var oldPosition = gameObject.transform.position;

        float xNew = oldPosition.x;
        float yNew = oldPosition.y;

        if (Input.GetMouseButton(1))
        {
            var xMouse = Input.GetAxis("Mouse X");
            var yMouse = Input.GetAxis("Mouse Y");
            panned = xMouse != 0 || yMouse != 0;
            if (panned)
            {
                var screenSizeFactor = Mathf.Pow(gameCamera.orthographicSize, 2) / screenSizeDivisor;
                xNew = oldPosition.x - xMouse * panSensitivity * screenSizeFactor;
                yNew = oldPosition.y - yMouse * panSensitivity * screenSizeFactor;
            }
        }

        var scroll = Input.mouseScrollDelta.y;
        var zoomed = scroll != 0;
        if (zoomed)
        {
            gameCamera.orthographicSize -= scroll * zoomSensitivity;
        }

        if (panned)
        {
            gameObject.transform.position = new(xNew, yNew, oldPosition.z);
        }
    }
}
