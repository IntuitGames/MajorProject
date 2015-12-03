using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using CustomExtensions;/// <summary>
/// Handles how the camera movement and interactions.
/// </summary>public class CameraManager : Manager{
    [Header("Components")]
    public Camera mainCamera;
    public SmoothCameraFollow followCamera;
    public SoundSource backgroundMusic;
    public AudioSource audioSourceComp;

    [Header("Dynamic Zoom"), ReadOnly]
    public bool isDynamic;
    public bool dynamicZoom = true;
    [Range(0, 50)]
    public float minCamZoomDistance = 3;
    [Range(0, 50)]
    public float maxCamZoomDistance = 25;
    [Range(0, 50)]
    public float minCamProximity = 3;
    [Range(0, 50)]
    public float maxCamProximity = 50;

    [Header("Dynamic FOV")]
    public bool dynamicFOV = true;
    [Range(0, 120)]
    public float minCamFOV = 60;
    [Range(0, 120)]
    public float maxCamFOV = 110;

    [Header("Camera Shake")]
    public bool shakeCamera = true;
    public float shakeTime = 1;
    public float shakeStrength = 0.5f;
    public float shakeFrequency = 0.1f;

    [Header("On Single Death")]
    public bool refocusToAlive = true;
    [Range(0, 50)]
    public float singleCamZoom = 4;
    private bool singleAlive;

    public override void ManagerAwake()
    {
        FindComponentReferences();
    }

    public override void ManagerOnLevelLoad()
    {
        FindComponentReferences();

        // Reset single death effects
        followCamera.target = followCamera.initialTarget;
        singleAlive = false;
    }    private void FindComponentReferences()
    {
        if (!mainCamera) mainCamera = Camera.main;
        if (!followCamera) followCamera = mainCamera.GetComponent<SmoothCameraFollow>();
        if (!backgroundMusic) backgroundMusic = mainCamera.GetComponent<SoundSource>();
        if (!audioSourceComp) audioSourceComp = mainCamera.GetComponent<AudioSource>();
    }    void Start()
    {
        // Subscribe to tether events
        GameManager.TetherManager.OnDisconnected += UnhingeCamera;
        GameManager.TetherManager.OnReconnected += StabalizeCamera;
        GameManager.PlayerManager.OnSingleDead += OnSingleDead;
    }

    void Update()
    {
        // Update unhinged camera zoom
        if (singleAlive && refocusToAlive)
            followCamera.distance = singleCamZoom;
        else if (isDynamic && dynamicZoom)
            followCamera.distance = GameManager.PlayerManager.distanceBetweenCharacters
                .Normalize(minCamProximity, maxCamProximity, minCamZoomDistance, maxCamZoomDistance);

        if (isDynamic && dynamicFOV)
            followCamera.targetFOV = GameManager.PlayerManager.distanceBetweenCharacters
                .Normalize(minCamProximity, maxCamProximity, minCamFOV, maxCamFOV);
    }

    public void UnhingeCamera(TetherJoint brokenJoint)
    {
        if (dynamicZoom || dynamicFOV)
            isDynamic = true;

        if (shakeCamera)
            StartCoroutine(mainCamera.Shake(shakeStrength, shakeTime, shakeFrequency));
    }    public void StabalizeCamera(TetherJoint reconnectedJoint)
    {
        isDynamic = false;

        // Reset camera zoom
        followCamera.distance = followCamera.initialDistance;

        // Reset camera FOV
        followCamera.targetFOV = followCamera.initialFOV;
    }

    private void OnSingleDead(Character aliveCharacter)
    {
        singleAlive = true;

        if (refocusToAlive)
            followCamera.target = aliveCharacter.transform;
    }}