using UnityEngine;
using UnityEngine.UI;
using CustomExtensions;using System.Collections;using System.Collections.Generic;using System.Linq;
using UnityEngine.Events;public class CoopSelector : MonoBehaviour{
    [Header("Components")]
    public RectTransform parentRect;
    public RectTransform armRect;

    [Header("Properties")]
    public float growthSpeed = 400;
    public float declineSpeed = 600;
    [Range(0, 5)]
    public float lengthMulti = 2;
    [Range(0, 5)]
    public float heightMulti = 2;
    [Range(0.9f, 1)]
    public float acceptanceThreshold = 0.9f;
    public float holdDuration = 0.1f;

    public UnityEvent OnCoopSelect = new UnityEvent();
    public event System.Action OnCoopSelection;

    public float percentage
    {
        get
        {
            return armRect.offsetMin.x.Normalize(minLength1, maxLength1, 0, 0.5f) + armRect.offsetMax.x.Normalize(minLength2, maxLength2, 0, 0.5f);
        }
    }
    public float currentHeight
    {
        get { return Mathf.Lerp(maxHeight, minHeight, percentage); }
    }

    private float minLength1, maxLength1, minLength2, maxLength2;
    private float minHeight, maxHeight;
    private float holdTimer;
    private bool initialized;

    void Awake()
    {
        // Set component references
        if (!parentRect)
            parentRect = GetComponent<RectTransform>();

        if (!armRect)
            armRect = GetComponentInChildren<RectTransform>();

        SetDefinitions();
    }

    void Update()
    {
        if (percentage > acceptanceThreshold)
            holdTimer += Time.unscaledDeltaTime;
        else
            holdTimer = 0;

        if (holdTimer > holdDuration)
            PerformAction();

        // Update coop selector position
        if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject)
            parentRect.localPosition = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.transform.localPosition;
    }

    void OnDestroy()
    {
        if (enabled)
        {
            GameManager.InputManager.MovementP1 -= Player1Movement;
            GameManager.InputManager.MovementP2 -= Player2Movement;
        }
    }    private void Player1Movement(float forward, float right)
    {
        float newValue;
        if (right < 0)
        {
            newValue = Mathf.Clamp(armRect.offsetMin.x + (right * GameManager.InputManager.unscaledMovementDelta * growthSpeed), maxLength1, minLength1);
            armRect.offsetMin = new Vector2(newValue, currentHeight);
        }
        else
        {
            newValue = Mathf.Clamp(armRect.offsetMin.x + (GameManager.InputManager.unscaledMovementDelta * declineSpeed), maxLength1, minLength1);
            armRect.offsetMin = new Vector2(newValue, currentHeight);
        }
    }

    private void Player2Movement(float forward, float right)
    {
        float newValue;
        if (right > 0)
        {
            newValue = Mathf.Clamp(armRect.offsetMax.x + (right * GameManager.InputManager.unscaledMovementDelta * growthSpeed), minLength2, maxLength2);
            armRect.offsetMax = new Vector2(newValue, currentHeight * -1);
        }
        else
        {
            newValue = Mathf.Clamp(armRect.offsetMax.x - (GameManager.InputManager.unscaledMovementDelta * declineSpeed), minLength2, maxLength2);
            armRect.offsetMax = new Vector2(newValue, currentHeight * -1);
        }
    }    private void PerformAction()
    {
        if (OnCoopSelection != null)
            OnCoopSelection();

        if (OnCoopSelect != null)
            OnCoopSelect.Invoke();

        SetActive(false);
    }    public void SetActive(bool state)
    {
        SetDefinitions();

        // Reset arm state
        holdTimer = 0;
        armRect.offsetMin = new Vector2(minLength1, maxHeight);
        armRect.offsetMax = new Vector2(minLength2, maxHeight);

        // Subscribe / Unsubscribe from input events
        if (state)
        {
            GameManager.InputManager.MovementP1 += Player1Movement;
            GameManager.InputManager.MovementP2 += Player2Movement;
        }
        else
        {
            GameManager.InputManager.MovementP1 -= Player1Movement;
            GameManager.InputManager.MovementP2 -= Player2Movement;
        }

        // Disable / Enable game objects
        armRect.gameObject.SetActive(state);
        enabled = state;
    }    private void SetDefinitions()
    {
        if (initialized) return;

        // Set length definitions
        minLength1 = armRect.offsetMin.x;
        maxLength1 = minLength1 * lengthMulti;

        minLength2 = armRect.offsetMax.x;
        maxLength2 = minLength2 * lengthMulti;

        maxHeight = armRect.offsetMin.y;
        minHeight = maxHeight / heightMulti;

        initialized = true;
    }}