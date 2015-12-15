using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using CustomExtensions;/// <summary>
/// Handles how the camera movement and interactions.
/// </summary>public class CameraManager : Manager{
    [Header("Components")]
    public Camera mainCamera;
    public SmoothCameraFollow followCamera;
    public SoundSource backgroundMusic;
    public AudioSource audioSourceComp;
    public UnityStandardAssets.ImageEffects.BloomOptimized bloomComp;
    public UnityStandardAssets.ImageEffects.VignetteAndChromaticAberration vignetteComp;
    [EnumFlags]
    public ModeManager.GameMode unsnapCameraModes;

    [Header("Zoom"), ReadOnly]
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

    [Header("FOV")]
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

    [Header("Vignette")]
    public bool dynamicVignette = true;
    public float vignetteSmoothSpeed = 2;
    public float normalVignette = 0.15f;
    public float disconnectedVignette = 4;

    [Header("Swoop")]
    public bool swoopCamera = true;
    public float swoopThreshold = 8;
    public float swoopCap = 25;
    public float maxSwoopValue = 0.2f;

    private float targetVignette;
    private float vignetteSpeed;

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
        if (!bloomComp) bloomComp = mainCamera.GetComponent<UnityStandardAssets.ImageEffects.BloomOptimized>();
        if (!vignetteComp) vignetteComp = mainCamera.GetComponent<UnityStandardAssets.ImageEffects.VignetteAndChromaticAberration>();

        if (!unsnapCameraModes.IsFlagSet(GameManager.ModeManager.currentGameMode))
            followCamera.enabled = true;
        else
            followCamera.enabled = false;
    }    void Start()
    {
        targetVignette = normalVignette;

        // Subscribe to tether events
        GameManager.TetherManager.OnDisconnected += UnhingeCamera;
        GameManager.TetherManager.OnReconnected += StabalizeCamera;
        GameManager.PlayerManager.OnSingleDead += OnSingleDead;
        GameManager.ModeManager.OnGameModeChanged += OnGameModeChange;
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

        if (dynamicVignette)
            vignetteComp.intensity = Mathf.SmoothDamp(vignetteComp.intensity, targetVignette, ref vignetteSpeed, vignetteSmoothSpeed, 1000, Time.unscaledDeltaTime);
        else
            vignetteComp.intensity = normalVignette;

        float zDifference = Mathf.Abs((PlayerManager.character1Pos - PlayerManager.character2Pos).z);
        if (swoopCamera && zDifference > swoopThreshold && !singleAlive)
            followCamera.overridenOffsetDirection.y = zDifference.Normalize(swoopThreshold, swoopCap, followCamera.initialOverrideOffset.y, maxSwoopValue);
        else
            followCamera.overridenOffsetDirection.y = followCamera.initialOverrideOffset.y;
    }

    private void OnGameModeChange(ModeManager.GameMode newMode, ModeManager.GameMode oldMode)
    {
        if (followCamera == null) return;

        if (!unsnapCameraModes.IsFlagSet(newMode))
            followCamera.enabled = true;
        else
            followCamera.enabled = false;
    }

    public void UnhingeCamera(TetherJoint brokenJoint)
    {
        if (dynamicZoom || dynamicFOV)
            isDynamic = true;

        if (shakeCamera)
            StartCoroutine(mainCamera.Shake(shakeStrength, shakeTime, shakeFrequency));

        targetVignette = disconnectedVignette;
    }    public void StabalizeCamera(TetherJoint reconnectedJoint)
    {
        isDynamic = false;

        // Reset camera zoom
        followCamera.distance = followCamera.initialDistance;

        // Reset camera FOV
        followCamera.targetFOV = followCamera.initialFOV;

        // Reset vignette
        TimerPlus.Create(0.25f, () => targetVignette = normalVignette);
    }

    private void OnSingleDead(Character aliveCharacter)
    {
        singleAlive = true;

        if (refocusToAlive)
            followCamera.target = aliveCharacter.transform;
    }}