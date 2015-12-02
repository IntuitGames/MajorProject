using UnityEngine;
using UnityEngine.UI;
using CustomExtensions;using System.Collections;using System.Collections.Generic;using System.Linq;
using UnityEngine.Events;
using UnityEngine.EventSystems;public class CoopSelector : MonoBehaviour, ISelectHandler{
    [Header("Components")]
    public RectTransform parentRect;
    public RectTransform armRect;
    public Button defaultSelection;

    [Header("Properties")]
    public float smoothTime = 0.2f;
    public float maxSpeed = 1000;
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
    private float minHeight, maxHeight, minOffsetSpeed, maxOffsetSpeed;
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

    void Start()
    {
        // Set coop selector position
        defaultSelection.Select();
        parentRect.localPosition = defaultSelection.transform.localPosition;
    }

    void OnEnable()
    {
        // Set coop selector position
        defaultSelection.Select();
        parentRect.localPosition = defaultSelection.transform.localPosition;
    }

    void Update()
    {
        if (percentage > acceptanceThreshold)
            holdTimer += Time.unscaledDeltaTime;
        else
            holdTimer = 0;

        if (holdTimer > holdDuration)
            PerformAction();
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
        float targetValue = right < 0 ? maxLength1 : minLength1;
        armRect.offsetMin = new Vector2(Mathf.SmoothDamp(armRect.offsetMin.x, targetValue, ref minOffsetSpeed, smoothTime, maxSpeed, GameManager.InputManager.unscaledMovementDelta), currentHeight);
    }

    private void Player2Movement(float forward, float right)
    {
        float targetValue = right > 0 ? maxLength2 : minLength2;
        armRect.offsetMax = new Vector2(Mathf.SmoothDamp(armRect.offsetMax.x, targetValue, ref maxOffsetSpeed, smoothTime, maxSpeed, GameManager.InputManager.unscaledMovementDelta), currentHeight * -1);
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
    }

    public void OnSelect(BaseEventData eventData)
    {
        // Set coop selector position
        parentRect.localPosition = eventData.selectedObject.transform.localPosition;
    }
}