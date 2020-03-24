using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;

public class ARTapToPlaceObject : MonoBehaviour
{
    public GameObject ObjectPlace;
    public GameObject placementIndicator;
    private ARSessionOrigin arOrigin;
    private ARRaycastManager arMangaer;
    private ARPlaneManager m_ARPlaneManager;
    private PlaneDetectionController planeDetectionController;
    private Pose placementPose;
    private bool placementPoseIsValid = false;
    private bool objectPlaced = false;
    public GameObject spawnedObject { get; private set; }
    private Vector3 rotationSpeed = new Vector3(1, 1, 1);
    // Start is called before the first frame update 
    void Awake()
    {
        arMangaer = FindObjectOfType<ARRaycastManager>();
        planeDetectionController = FindObjectOfType<PlaneDetectionController>();
    }


    bool TryGetTouchPosition(out Vector2 touchPosition)
    {
#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            var mousePosition = Input.mousePosition;
            touchPosition = new Vector2(mousePosition.x, mousePosition.y);
            return true;
        }
#else
        if (Input.touchCount > 0)
        {
            touchPosition = Input.GetTouch(0).position;
            return true;
        }
#endif

        touchPosition = default;
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlacementPose();
        UpdatePlacemntIndicator();

        if (placementPoseIsValid && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {

            if (!objectPlaced)
            {
                PlaceObject();
                objectPlaced = true;
                planeDetectionController.TogglePlaneDetection();
            }
        }
    }

    public void reset()
    {
        objectPlaced = false;
        Destroy(spawnedObject);
        planeDetectionController.TogglePlaneDetection();
    }
    private void PlaceObject()
    {
        spawnedObject = Instantiate(ObjectPlace, placementPose.position, placementPose.rotation);
        spawnedObject.transform.Rotate(0, 59, 0 * Time.deltaTime);
    }

    private void UpdatePlacemntIndicator()
    {
        if (placementPoseIsValid)
        {
            if (!objectPlaced) {
                placementIndicator.SetActive(true);
                placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
            }
            else
            {
                placementIndicator.SetActive(false);
            }
        }
        else
        {
            placementIndicator.SetActive(false);
        }
    }

    private void UpdatePlacementPose()
    {
        var screenCentre = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        arMangaer.Raycast(screenCentre, hits, TrackableType.Planes);

        placementPoseIsValid = hits.Count > 0;
        if (placementPoseIsValid)
        {
            placementPose = hits[0].pose;

            var cameraForward = Camera.current.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();
}
