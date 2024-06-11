using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    /*TODO: work out how to limit the cameras pannign to the bacground image
     * something about image culling blah blh
     */



    [SerializeField]
    private float cameraSpeed = 10;
    //[SerializeField]
    //private float cameraZoomSpeed = 2;

    [SerializeField]
    private Transform background;

    //private float xMax;
    //private float yMin;
    //private float yMax;
    //private float xMin;
    private float minZoom = 2f;
    private float maxZoom = 15f;

    private Vector3 Origin;
    private Vector3 Difference;
    private Camera mainCamera;
    private LevelManager lm;


    private void Start()
    {
        lm = LevelManager.Instance;
        mainCamera = Camera.main;   //apparently Camer.main can be expensive if called enough

        Origin = MousePos();
        Difference = MousePos() - transform.position;
        transform.position = Origin - Difference;
        //background.position = transform.position / 2;
    }

    void Update()
    {
        GetInput();
    }

    void GetInput()
    {
        if (!lm.paused)
        {            
            MouseControl();
        }
    }

    private void MouseControl()
    {
        if (Input.mouseScrollDelta.magnitude > 0)
            CameraZoom(Input.GetAxis("Mouse ScrollWheel") * 3);

        //Allow drag screen with RMB
        if (Input.GetMouseButtonDown(0))
        {
            Origin = MousePos();
        }
        if (Input.touchCount == 2)
        {
            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame, reversed for this use case
            float deltaMagnitudeDiff = touchDeltaMag - prevTouchDeltaMag;

            CameraZoom(deltaMagnitudeDiff * 0.01f);
        }else if (Input.GetMouseButton(0))
        {
            Difference = MousePos() - transform.position;
            transform.position = Origin - Difference;
            background.position = transform.position / 2;
        }
    }

    void CameraZoom(float increment)
    {
        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize - increment, minZoom, maxZoom);
        //float scaleZoom = background.localScale.x;
        //scaleZoom = Mathf.Clamp(scaleZoom + increment, 2, 5);
        float scaleZoom = 0.19f * mainCamera.orthographicSize + 0.11f;
       // float scaleZoom = 0.07f * mainCamera.orthographicSize + 1.8f;
        background.localScale = new Vector3(scaleZoom, scaleZoom, 1);
    }

    private Vector3 MousePos()
    {
        return mainCamera.ScreenToWorldPoint(Input.mousePosition);
    }

}
