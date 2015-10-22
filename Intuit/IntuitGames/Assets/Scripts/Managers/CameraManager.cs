﻿using UnityEngine;
using CustomExtensions;
/// Handles how the camera movement and interactions.
/// </summary>
    [Header("Components")]
    public Camera mainCamera;
    public SmoothCameraFollow followCamera;
    public BackgroundMusic backgroundMusic;

    [Header("Tether Disconnection"), ReadOnly]
    public bool isUnhinged;
    public bool canUnhinge = true;
    [Range(0, 50)]
    public float minCamZoomDistance = 5;
    [Range(0, 50)]
    public float maxCamZoomDistance = 20;
    [Range(0, 50)]
    public float minCamProximity = 3;
    [Range(0, 50)]
    public float maxCamProximity = 20;
    {
        if (!mainCamera) mainCamera = Camera.main;
        if (!followCamera) followCamera = mainCamera.GetComponent<SmoothCameraFollow>();
        if (!backgroundMusic) backgroundMusic = mainCamera.GetComponent<BackgroundMusic>();
    }
    {
        // Subscribe to tether events
        GameManager.TetherManager.OnDisconnected += UnhingeCamera;
        GameManager.TetherManager.OnReconnected += StabalizeCamera;
    }

    void Update()
    {
        // Update unhinged camera zoom (Still need camera shake)
        if (isUnhinged)
            followCamera.distance = GameManager.PlayerManager.distanceBetweenCharacters
                .Normalize(minCamProximity, maxCamProximity, minCamZoomDistance, maxCamZoomDistance);
    }

    public void UnhingeCamera(TetherJoint brokenJoint)
    {
        if (canUnhinge)
            isUnhinged = true;
    }
    {
        isUnhinged = false;

        // Reset camera zoom
        followCamera.distance = followCamera.initialDistance;
    }