using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.EventSystems;
using CustomExtensions;

/// <summary>
/// Handles all input related elements.
/// </summary>
public class InputManager : Manager
{
    #region VARIABLES

    // References
    public EventSystem eventSystem;

    // Input Axes Strings
    private const string player1Str = "P1";
    private const string player2Str = "P2";
    private const string forwardStr = "Vertical_";
    private const string rightStr = "Horizontal_";
    private const string jumpStr = "Jump_";
    private const string dashStr = "Dash_";
    private const string heavyStr = "Heavy_";
    private const string pauseStr = "Cancel_";
    private const string unpauseStr = "Cancel_";
    private const string sprintStr = "Sprint_";
    private const string waveStr = "Wave_";

    // Input Booleans
    [System.NonSerialized] public InputData jumpData = new InputData(jumpStr);
    [System.NonSerialized] public InputData dashData = new InputData(dashStr);
    [System.NonSerialized] public InputData heavyData = new InputData(heavyStr);
    [System.NonSerialized] public InputData pauseData = new InputData(pauseStr);
    [System.NonSerialized] public InputData unpauseData = new InputData(unpauseStr);
    [System.NonSerialized] public InputData sprintData = new InputData(sprintStr);
    [System.NonSerialized] public InputData waveData = new InputData(waveStr);

    // Events
    public event Action<float> PreUpdate = delegate { };
    public event Action<float> PostUpdate = delegate { };
    // 1st float - forward | 2nd float - right
    public event Action<float, float> MovementP1 = delegate { };
    public event Action<float, float> MovementP2 = delegate { };
    // 1st bool - isPlayerOne? | 2nd bool - is pressed?
    public event Action<bool, bool> Jump = delegate { };
    public event Action<bool, bool> JumpToggle = delegate { };
    public event Action<bool, bool> Dash = delegate { };
    public event Action<bool, bool> DashToggle = delegate { };
    public event Action<bool, bool> Heavy = delegate { };
    public event Action<bool, bool> HeavyToggle = delegate { };
    public event Action<bool, bool> Sprint = delegate { };
    public event Action<bool, bool> SprintToggle = delegate { };
    public event Action<bool, bool> Pause = delegate { };
    public event Action<bool, bool> Unpause = delegate { };
    public event Action<bool, bool> PauseToggle = delegate { };
    public event Action<bool, bool> UnpauseToggle = delegate { };
    public event Action<bool, bool> Wave = delegate { };
    public event Action<bool, bool> WaveToggle = delegate { };


    // Settings
    public enum UpdateTypes { Update, FixedUpdate, LateUpdate };
    public UpdateTypes preAndPostUpdates = UpdateTypes.FixedUpdate;
    public UpdateTypes movementUpdates = UpdateTypes.FixedUpdate;
    public UpdateTypes jumpUpdates = UpdateTypes.FixedUpdate;
    public UpdateTypes dashUpdates = UpdateTypes.FixedUpdate;
    public UpdateTypes heavyUpdates = UpdateTypes.Update;
    public UpdateTypes pauseUpdates = UpdateTypes.Update;
    public UpdateTypes sprintUpdates = UpdateTypes.Update;
    public UpdateTypes waveUpdates = UpdateTypes.Update;

    // Quick-access Properties
    public float movementDelta { get { return GetDelta(movementUpdates); } }
    public float unscaledMovementDelta { get { return GetDelta(movementUpdates, false); } }
    public float jumpDelta { get { return GetDelta(jumpUpdates); } }
    public float dashDelta { get { return GetDelta(dashUpdates); } }
    public float heavyDelta { get { return GetDelta(heavyUpdates); } }
    public float pauseDelta { get { return GetDelta(pauseUpdates); } }
    public float sprintDelta { get { return GetDelta(sprintUpdates); } }
    public float waveDelta { get { return GetDelta(waveUpdates); } }

    #endregion

    #region MESSAGES

    public override void ManagerAwake()
    {
        if (!eventSystem)
            eventSystem = GameObject.FindObjectOfType<EventSystem>();

        eventSystem.enabled = true;

        // While in game over mode switch movement updates to Update
        UpdateTypes cachedMovementUpdate = movementUpdates;
        ModeManager.OnGameModeChanged += (n, o) =>
            {
                if (n.EqualToAny(global::ModeManager.GameMode.GameOver, global::ModeManager.GameMode.MainMenu))
                    movementUpdates = UpdateTypes.Update;
                else if (o == global::ModeManager.GameMode.GameOver)
                    movementUpdates = cachedMovementUpdate;
            };
    }

    void Update()
    {
        CheckForInput();

        HandleInput(UpdateTypes.Update, Time.deltaTime);
    }

    void FixedUpdate()
    {
        HandleInput(UpdateTypes.FixedUpdate, Time.fixedDeltaTime);
    }

    void LateUpdate()
    {
        HandleInput(UpdateTypes.LateUpdate, Time.deltaTime);
    }

    #endregion

    #region HANDLERS

    private void CheckForInput()
    {
        // Updates all input data
        jumpData.Update();
        dashData.Update();
        heavyData.Update();
        pauseData.Update();
        unpauseData.Update();
        sprintData.Update();
        waveData.Update();
    }

    private void HandleInput(UpdateTypes type, float delta)
    {
        if (type == preAndPostUpdates) PreUpdate(delta);

        // Only check inputs for certain game modes
        switch (ModeManager.currentGameMode)
        {
            case ModeManager.GameMode.PauseMenu:
                if (type == pauseUpdates) unpauseData.RaiseEvent(Unpause, UnpauseToggle);
                break;

            case ModeManager.GameMode.InGame:
                // Raise axis input events
                if (type == movementUpdates)
                {
                    MovementP1(Input.GetAxis(forwardStr + player1Str), Input.GetAxis(rightStr + player1Str));
                    MovementP2(Input.GetAxis(forwardStr + player2Str), Input.GetAxis(rightStr + player2Str));
                }

                // Raise action button event
                if (type == jumpUpdates) jumpData.RaiseEvent(Jump, JumpToggle);
                if (type == dashUpdates) dashData.RaiseEvent(Dash, DashToggle);
                if (type == heavyUpdates) heavyData.RaiseEvent(Heavy, HeavyToggle);
                if (type == sprintUpdates) sprintData.RaiseEvent(Sprint, SprintToggle);
                if (type == waveUpdates) waveData.RaiseEvent(Wave, WaveToggle);

                // Pause check
                if (type == pauseUpdates) pauseData.RaiseEvent(Pause, PauseToggle);
                break;

            case ModeManager.GameMode.GameOver:
                // Raise axis input events
                if (type == movementUpdates)
                {
                    MovementP1(Input.GetAxisRaw(forwardStr + player1Str), Input.GetAxisRaw(rightStr + player1Str));
                    MovementP2(Input.GetAxisRaw(forwardStr + player2Str), Input.GetAxisRaw(rightStr + player2Str));
                }
                break;

            case global::ModeManager.GameMode.MainMenu:
                // Raise axis input events
                if (type == movementUpdates)
                {
                    MovementP1(Input.GetAxis(forwardStr + player1Str), Input.GetAxis(rightStr + player1Str));
                    MovementP2(Input.GetAxis(forwardStr + player2Str), Input.GetAxis(rightStr + player2Str));
                }
                break;

            default:
                break;
        }

        if (type == preAndPostUpdates) PostUpdate(delta);
    }

    #endregion

    #region HELPERS

    // Subscribes to all character specific events
    public void SubscribeCharacterEvents(Character characterObj)
    {
        // General events
        PreUpdate += characterObj.PreInputUpdate;
        PostUpdate += characterObj.PostInputUpdate;
        PauseToggle += characterObj.Pause;
        UnpauseToggle += characterObj.Unpause;

        Jump += characterObj.Jump;
        JumpToggle += characterObj.JumpToggle;
        DashToggle += characterObj.Dash;
        Heavy += characterObj.Heavy;
        Sprint += characterObj.Sprint;
        WaveToggle += characterObj.Wave;

        // Player specific events
        if (characterObj.isPlayerOne)
            MovementP1 += characterObj.Movement;
        else
            MovementP2 += characterObj.Movement;
    }

    // Should be called when characters get destroyed
    public void UnsubscribeCharacterEvents(Character characterObj)
    {
        // General events
        PreUpdate -= characterObj.PreInputUpdate;
        PostUpdate -= characterObj.PostInputUpdate;
        PauseToggle -= characterObj.Pause;
        UnpauseToggle -= characterObj.Unpause;

        Jump -= characterObj.Jump;
        JumpToggle -= characterObj.JumpToggle;
        DashToggle -= characterObj.Dash;
        Heavy -= characterObj.Heavy;
        Sprint -= characterObj.Sprint;
        WaveToggle -= characterObj.Wave;

        // Player specific events
        if (characterObj.isPlayerOne)
            MovementP1 -= characterObj.Movement;
        else
            MovementP2 -= characterObj.Movement;
    }

    private float GetDelta(UpdateTypes type, bool scaled = true)
    {
        return scaled ? (type == UpdateTypes.FixedUpdate ? Time.fixedDeltaTime : Time.deltaTime) :
            (type == UpdateTypes.FixedUpdate ? Time.fixedDeltaTime : Time.unscaledDeltaTime);
    }

    // Is a player currently requesting a jump?
    public bool IsRequestingJump(bool player1)
    {
        if (player1)
            return jumpData.p1Hold;
        else
            return jumpData.p2Hold;
    }

    #endregion

    #region NESTED

    [System.Serializable]
    public struct InputData
    {
        public readonly string actionStr;

        public bool p1Down, p2Down;
        public bool p1Hold, p2Hold;
        public bool p1Up, p2Up;

        public InputData(string inputStr)
        {
            actionStr = inputStr;
            p1Down = false; p2Down = false;
            p1Hold = false; p2Hold = false;
            p1Up = false; p2Up = false;
        }

        public void Update()
        {
            if (Input.GetButtonDown(actionStr + player1Str)) p1Down = true;
            if (Input.GetButtonDown(actionStr + player2Str)) p2Down = true;
            p1Hold = Input.GetButton(actionStr + player1Str);
            p2Hold = Input.GetButton(actionStr + player2Str);
            if (Input.GetButtonUp(actionStr + player1Str)) p1Up = true;
            if (Input.GetButtonUp(actionStr + player2Str)) p2Up = true;
        }

        // Raise specified events based on input data
        public void RaiseEvent(Action<bool, bool> action, Action<bool, bool> toggleAction)
        {
            if (p1Down)
            {
                toggleAction(true, true);
                p1Down = false;
            }
            if (p2Down)
            {
                toggleAction(false, true);
                p2Down = false;
            }

            action(true, p1Hold);
            action(false, p2Hold);

            if (p1Up)
            {
                toggleAction(true, false);
                p1Up = false;
            }
            if (p2Up)
            {
                toggleAction(false, false);
                p2Up = false;
            }
        }
    }

    #endregion
}
