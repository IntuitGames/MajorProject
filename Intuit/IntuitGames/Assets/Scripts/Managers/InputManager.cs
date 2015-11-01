using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.EventSystems;

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

    // Input Booleans
    [System.NonSerialized] public InputData jumpData = new InputData(jumpStr);
    [System.NonSerialized] public InputData dashData = new InputData(dashStr);
    [System.NonSerialized] public InputData heavyData = new InputData(heavyStr);
    [System.NonSerialized] public InputData pauseData = new InputData(pauseStr);
    [System.NonSerialized] public InputData unpauseData = new InputData(unpauseStr);
    [System.NonSerialized] public InputData sprintData = new InputData(sprintStr);

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
    }

    // Events
    public event Action<float> PreUpdate = delegate { };
    public event Action<float> PostUpdate = delegate { };
    public event Action<float, float> MovementP1 = delegate { };
    public event Action<float, float> MovementP2 = delegate { };
    public event Action<bool> JumpP1 = delegate { };
    public event Action<bool> JumpP2 = delegate { };
    public event Action<bool> JumpToggleP1 = delegate { };
    public event Action<bool> JumpToggleP2 = delegate { };
    public event Action<bool> DashP1 = delegate { };
    public event Action<bool> DashP2 = delegate { };
    public event Action<bool> DashToggleP1 = delegate { };
    public event Action<bool> DashToggleP2 = delegate { };
    public event Action<bool> HeavyP1 = delegate { };
    public event Action<bool> HeavyP2 = delegate { };
    public event Action<bool> HeavyToggleP1 = delegate { };
    public event Action<bool> HeavyToggleP2 = delegate { };
    public event Action<bool> Pause = delegate { };
    public event Action<bool> Unpause = delegate { };
    public event Action<bool> PauseToggle = delegate { };
    public event Action<bool> UnpauseToggle = delegate { };
    public event Action<bool> SprintP1 = delegate { };
    public event Action<bool> SprintP2 = delegate { };
    public event Action<bool> SprintToggleP1 = delegate { };
    public event Action<bool> SprintToggleP2 = delegate { };

    // Settings
    public enum UpdateTypes { Update, FixedUpdate, LateUpdate };
    public UpdateTypes preAndPostUpdates = UpdateTypes.FixedUpdate;
    public UpdateTypes movementUpdates = UpdateTypes.FixedUpdate;
    public UpdateTypes jumpUpdates = UpdateTypes.FixedUpdate;
    public UpdateTypes dashUpdates = UpdateTypes.FixedUpdate;
    public UpdateTypes heavyUpdates = UpdateTypes.Update;
    public UpdateTypes pauseUpdates = UpdateTypes.Update;
    public UpdateTypes sprintUpdates = UpdateTypes.Update;

    // Quick-access Properties
    public float jumpDelta { get { return GetDelta(jumpUpdates); } }
    public float dashDelta { get { return GetDelta(dashUpdates); } }
    public float heavyDelta { get { return GetDelta(heavyUpdates); } }
    public float pauseDelta { get { return GetDelta(pauseUpdates); } }
    public float sprintDelta { get { return GetDelta(sprintUpdates); } }

    #endregion

    #region MESSAGES

    public override void ManagerAwake()
    {
        if (!eventSystem)
            eventSystem = GameObject.FindObjectOfType<EventSystem>();

        eventSystem.enabled = true;
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

    private void CheckForInput()
    {
        jumpData.Update();
        dashData.Update();
        heavyData.Update();
        pauseData.Update();
        unpauseData.Update();
        sprintData.Update();
    }

    private void HandleInput(UpdateTypes type, float delta)
    {
        if (type == preAndPostUpdates) PreUpdate(delta);

        // Only check inputs for certain game modes
        switch (ModeManager.currentGameMode)
        {
            case ModeManager.GameMode.PauseMenu:
                if (type == pauseUpdates) HandlePauseMenuEvents();
                break;

            case ModeManager.GameMode.InGame:
                HandleInGameEvents(type);
                break;

            default:
                break;
        }

        if (type == preAndPostUpdates) PostUpdate(delta);
    }

    #region IN-GAME

    // Is called every frame if game is in a in-game state
    private void HandleInGameEvents(UpdateTypes type)
    {
        // Raise axis input events
        if (type == movementUpdates) HandleMovementEvents();

        // Raise action button event
        if (type == jumpUpdates) HandleJumpEvents();
        if (type == dashUpdates) HandleDashEvents();
        if (type == heavyUpdates) HandleHeavyEvents();
        if (type == sprintUpdates) HandleSprintEvents();

        // Pause and unpause check
        if (type == pauseUpdates) HandlePauseEvents();
    }

    private void HandleMovementEvents()
    {
        MovementP1(Input.GetAxis(forwardStr + player1Str), Input.GetAxis(rightStr + player1Str));
        MovementP2(Input.GetAxis(forwardStr + player2Str), Input.GetAxis(rightStr + player2Str));
    }

    private void HandleJumpEvents()
    {
        if (jumpData.p1Down)
        {
            JumpToggleP1(true);
            jumpData.p1Down = false;
        }
        if (jumpData.p2Down)
        {
            JumpToggleP2(true);
            jumpData.p2Down = false;
        }

        JumpP1(jumpData.p1Hold);
        JumpP2(jumpData.p2Hold);

        if (jumpData.p1Up)
        {
            JumpToggleP1(false);
            jumpData.p1Up = false;
        }
        if (jumpData.p2Up)
        {
            JumpToggleP2(false);
            jumpData.p2Up = false;
        }
    }

    private void HandleDashEvents()
    {
        if (dashData.p1Down)
        {
            DashToggleP1(true);
            dashData.p1Down = false;
        }
        if (dashData.p2Down)
        {
            DashToggleP2(true);
            dashData.p2Down = false;
        }

        DashP1(dashData.p1Hold);
        DashP2(dashData.p2Hold);

        if (dashData.p1Up)
        {
            DashToggleP1(false);
            dashData.p1Up = false;
        }
        if (dashData.p2Up)
        {
            DashToggleP2(false);
            dashData.p2Up = false;
        }
    }

    private void HandleHeavyEvents()
    {
        if (heavyData.p1Down)
        {
            HeavyToggleP1(true);
            heavyData.p1Down = false;
        }
        if (heavyData.p2Down)
        {
            HeavyToggleP2(true);
            heavyData.p2Down = false;
        }

        HeavyP1(heavyData.p1Hold);
        HeavyP2(heavyData.p2Hold);

        if (heavyData.p1Up)
        {
            HeavyToggleP1(false);
            heavyData.p1Up = false;
        }
        if (heavyData.p2Up)
        {
            HeavyToggleP2(false);
            heavyData.p2Up = false;
        }
    }

    private void HandlePauseEvents()
    {
        if (pauseData.p1Down || pauseData.p2Down)
        {
            PauseToggle(true);
            pauseData.p1Down = false;
            pauseData.p2Down = false;
        }

        Pause(pauseData.p1Hold || pauseData.p2Hold);

        if (pauseData.p1Up || pauseData.p2Up)
        {
            PauseToggle(false);
            pauseData.p1Up = false;
            pauseData.p2Up = false;
        }
    }

    private void HandleSprintEvents()
    {
        if (sprintData.p1Down)
        {
            SprintToggleP1(true);
            sprintData.p1Down = false;
        }
        if (sprintData.p2Down)
        {
            SprintToggleP2(true);
            sprintData.p2Down = false;
        }

        SprintP1(sprintData.p1Hold);
        SprintP2(sprintData.p2Hold);

        if (sprintData.p1Up)
        {
            SprintToggleP1(false);
            sprintData.p1Up = false;
        }
        if (sprintData.p2Up)
        {
            SprintToggleP2(false);
            sprintData.p2Up = false;
        }
    }

    #endregion

    #region PAUSE MENU

    // Is called every frame if the game is in a paused state
    private void HandlePauseMenuEvents()
    {
        if (unpauseData.p1Down || unpauseData.p2Down)
        {
            UnpauseToggle(true);
            unpauseData.p1Down = false;
            unpauseData.p2Down = false;
        }

        Unpause(unpauseData.p1Hold || unpauseData.p2Hold);

        if (unpauseData.p1Up || unpauseData.p2Up)
        {
            UnpauseToggle(false);
            unpauseData.p1Up = false;
            unpauseData.p2Up = false;
        }
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

        if (characterObj.isPlayerOne)
        {
            // Subscribe to player 1 events
            MovementP1 += characterObj.Movement;
            JumpP1 += characterObj.Jump;
            JumpToggleP1 += characterObj.JumpToggle;
            DashToggleP1 += characterObj.Dash;
            HeavyP1 += characterObj.Heavy;
            SprintP1 += characterObj.Sprint;
        }
        else
        {
            // Subscribe to player 2 events
            MovementP2 += characterObj.Movement;
            JumpP2 += characterObj.Jump;
            JumpToggleP2 += characterObj.JumpToggle;
            DashToggleP2 += characterObj.Dash;
            HeavyP2 += characterObj.Heavy;
            SprintP2 += characterObj.Sprint;
        }
    }

    // Should be called when characters get destroyed
    public void UnsubscribeCharacterEvents(Character characterObj)
    {
        // General events
        PreUpdate -= characterObj.PreInputUpdate;
        PostUpdate -= characterObj.PostInputUpdate;
        PauseToggle -= characterObj.Pause;
        UnpauseToggle -= characterObj.Unpause;

        if (characterObj.isPlayerOne)
        {
            // Subscribe to player 1 events
            MovementP1 -= characterObj.Movement;
            JumpP1 -= characterObj.Jump;
            JumpToggleP1 -= characterObj.JumpToggle;
            DashToggleP1 -= characterObj.Dash;
            HeavyP1 -= characterObj.Heavy;
            SprintP1 -= characterObj.Sprint;
        }
        else
        {
            // Subscribe to player 2 events
            MovementP2 -= characterObj.Movement;
            JumpP2 -= characterObj.Jump;
            JumpToggleP2 -= characterObj.JumpToggle;
            DashToggleP2 -= characterObj.Dash;
            HeavyP2 -= characterObj.Heavy;
            SprintP2 -= characterObj.Sprint;
        }
    }

    private float GetDelta(UpdateTypes type)
    {
        return type == UpdateTypes.FixedUpdate ? Time.fixedDeltaTime : Time.deltaTime;
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
}
