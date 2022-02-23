using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARManager : MonoBehaviour
{
    public GameObject gameplayCanvas;
    public GameObject calibrationCanvas;
    public Slider fieldScale;
    public Slider fieldAltitude;
    public GameObject gameplayComponents;
    public GameObject aRRoot;
    public ARSession aRSession;
    public Button fieldSetBtn;

    public GameManager GameManager;

    public Globals Globals;

    public ARRaycastManager aRRaycastManager;
    Pose placementPose;
    bool placementPoseValid = false;
    bool fieldSet = false;
    Vector3 altitudeVec;
    Vector3 originalScale;

    // This asks for Camera permission right from the MenuScene.

    void Start()
    {
        Globals.usingAR = true;
        //aRRaycastManager = FindObjectOfType<ARRaycastManager>();
        originalScale = aRRoot.transform.localScale;
    }

    void Update()
    {
        if (!fieldSet)
        {
            ARUpdateField();
        }
    }

    public void ARUpdateField()
    {
        aRRaycastManager.enabled = false;
        OnscreenDebugOutput.Log($"Current camera: {Camera.main}");
        Vector3 screenCenter = Camera.main.ViewportToScreenPoint(new Vector3(.5f, .5f));
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        aRRaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinInfinity);

        placementPoseValid = hits.Count > 0;
        if (placementPoseValid)
        {
            placementPose = hits[0].pose;
            OnscreenDebugOutput.Log($"Placement available: {placementPose.position}, {placementPose.rotation}");
            //gameplayComponents.SetActive(true);
            fieldSetBtn.enabled = true;
            gameplayComponents.transform.SetPositionAndRotation(placementPose.position + altitudeVec, placementPose.rotation);
        }
        else
        {
            //OnscreenDebugOutput.Log($"Placement invalid");
            //gameplayComponents.SetActive(false);
            //fieldSetBtn.enabled = false;
        }
    }

    public void ARScaleField()
    {
        aRRoot.transform.localScale = originalScale * fieldScale.value;
        //OnscreenDebugOutput.Log($"Scale value changed: {fieldScale.value}");
    }

    public void ARAltitudeField()
    {
        // In case player wants the field to
        // *floats* in the air
        altitudeVec = new Vector3(0, 1, 0) * fieldAltitude.value;
        //OnscreenDebugOutput.Log($"Altitude value changed: {fieldAltitude.value}");
    }

    public void ARReset()
    {
        aRSession.Reset();
        fieldSet = false;
        fieldScale.value = 1;
        fieldAltitude.value = 1;
    }

    public void ARExit()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MenuScene");
    }

    public void ARSetField()
    {
        fieldSet = true;
        gameplayComponents.SetActive(true);
        gameplayCanvas.SetActive(true);
        aRRaycastManager.enabled = true;
        calibrationCanvas.SetActive(false);
        Globals.gamePaused = false;
        GameManager.Start();
    }
}
