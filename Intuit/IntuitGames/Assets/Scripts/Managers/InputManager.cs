using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Handles all input related elements.
/// </summary>
public class InputManager : Manager
{
    #region VARIABLES

    // Input Axes Strings
    private const string player1Str = "P1";
    private const string player2Str = "P2";
    private const string forwardStr = "Vertical_";
    private const string rightStr = "Horizontal_";
    private const string jumpStr = "Jump_";
    private const string dashStr = "Dash_";
    private const string heavyStr = "Heavy_";
    private const string pauseStr = "Submit_";
    private const string unpauseStr = "Submit_";
    private const string sprintStr = "Sprint_";

    // Input Booleans
    [System.NonSerialized] public bool p1JumpDown, p2JumpDown, p1JumpHold, p2JumpHold, p1JumpUp, p2JumpUp;
    [System.NonSerialized] public bool p1DashDown, p2DashDown, p1DashHold, p2DashHold, p1DashUp, p2DashUp;
    [System.NonSerialized] public bool p1HeavyDown, p2HeavyDown, p1HeavyHold, p2HeavyHold, p1HeavyUp, p2HeavyUp;
    [System.NonSerialized] public bool p1PauseDown, p2PauseDown, p1PauseHold, p2PauseHold, p1PauseUp, p2PauseUp;
    [System.NonSerialized] public bool p1UnpauseDown, p2UnpauseDown, p1UnpauseHold, p2UnpauseHold, p1UnpauseUp, p2UnpauseUp;
    [System.NonSerialized] public bool p1SprintDown, p2SprintDown, p1SprintHold, p2SprintHold, p1SprintUp, p2SprintUp;

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
        if (Input.GetButtonDown(jumpStr + player1Str)) p1JumpDown = true;
        if (Input.GetButtonDown(jumpStr + player2Str)) p2JumpDown = true;
        p1JumpHold = Input.GetButton(jumpStr + player1Str);
        p2JumpHold = Input.GetButton(jumpStr + player2Str);
        if (Input.GetButtonUp(jumpStr + player1Str)) p1JumpUp = true;
        if (Input.GetButtonUp(jumpStr + player2Str)) p2JumpUp = true;

        if (Input.GetButtonDown(dashStr + player1Str)) p1DashDown = true;
        if (Input.GetButtonDown(dashStr + player2Str)) p2DashDown = true;
        p1DashHold = Input.GetButton(dashStr + player1Str);
        p2DashHold = Input.GetButton(dashStr + player2Str);
        if (Input.GetButtonUp(dashStr + player1Str)) p1DashUp = true;
        if (Input.GetButtonUp(dashStr + player2Str)) p2DashUp = true;

        if (Input.GetButtonDown(heavyStr + player1Str)) p1HeavyDown = true;
        if (Input.GetButtonDown(heavyStr + player2Str)) p2HeavyDown = true;
        p1HeavyHold = Input.GetButton(heavyStr + player1Str);
        p2HeavyHold = Input.GetButton(heavyStr + player2Str);
        if (Input.GetButtonUp(heavyStr + player1Str)) p1HeavyUp = true;
        if (Input.GetButtonUp(heavyStr + player2Str)) p2HeavyUp = true;

        if (Input.GetButtonDown(pauseStr + player1Str)) p1PauseDown = true;
        if (Input.GetButtonDown(pauseStr + player2Str)) p2PauseDown = true;
        p1PauseHold = Input.GetButton(pauseStr + player1Str);
        p2PauseHold = Input.GetButton(pauseStr + player2Str);
        if (Input.GetButtonUp(pauseStr + player1Str)) p1PauseUp = true;
        if (Input.GetButtonUp(pauseStr + player2Str)) p2PauseUp = true;

        if (Input.GetButtonDown(unpauseStr + player1Str)) p1UnpauseDown = true;
        if (Input.GetButtonDown(unpauseStr + player2Str)) p2UnpauseDown = true;
        p1UnpauseHold = Input.GetButton(unpauseStr + player1Str);
        p2UnpauseHold = Input.GetButton(unpauseStr + player2Str);
        if (Input.GetButtonUp(unpauseStr + player1Str)) p1UnpauseUp = true;
        if (Input.GetButtonUp(unpauseStr + player2Str)) p2UnpauseUp = true;

        if (Input.GetButtonDown(sprintStr + player1Str)) p1SprintDown = true;
        if (Input.GetButtonDown(sprintStr + player2Str)) p2SprintDown = true;
        p1SprintHold = Input.GetButton(sprintStr + player1Str);
        p2SprintHold = Input.GetButton(sprintStr + player2Str);
        if (Input.GetButtonUp(sprintStr + player1Str)) p1SprintUp = true;
        if (Input.GetButtonUp(sprintStr + player2Str)) p2SprintUp = true;
    }

    private void HandleInput(UpdateTypes type, float delta)
    {
        if (type == preAndPostUpdates) PreUpdate(delta);

        // Only check inputs for certain game modes
        switch (ModeManager.currentGameMode)
        {
            case ModeManager.GameMode.InGame:
                HandleInGameEvents(type);
                break;

            case ModeManager.GameMode.PauseMenu:
                if (type == pauseUpdates) HandlePauseMenuEvents();
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
        if (p1JumpDown)
        {
            JumpToggleP1(true);
            p1JumpDown = false;
        }
        if (p2JumpDown)
        {
            JumpToggleP2(true);
            p2JumpDown = false;
        }

        JumpP1(p1JumpHold);
        JumpP2(p2JumpHold);

        if (p1JumpUp)
        {
            JumpToggleP1(false);
            p1JumpUp = false;
        }
        if (p2JumpUp)
        {
            JumpToggleP2(false);
            p2JumpUp = false;
        }
    }

    private void HandleDashEvents()
    {
        if (p1DashDown)
        {
            DashToggleP1(true);
            p1DashDown = false;
        }
        if (p2DashDown)
        {
            DashToggleP2(true);
            p2DashDown = false;
        }

        DashP1(p1DashHold);
        DashP2(p2DashHold);

        if (p1DashUp)
        {
            DashToggleP1(false);
            p1DashUp = false;
        }
        if (p2DashUp)
        {
            DashToggleP2(false);
            p2DashUp = false;
        }
    }

    private void HandleHeavyEvents()
    {
        if (p1HeavyDown)
        {
            HeavyToggleP1(true);
            p1HeavyDown = false;
        }
        if (p2HeavyDown)
        {
            HeavyToggleP2(true);
            p2HeavyDown = false;
        }

        HeavyP1(p1HeavyHold);
        HeavyP2(p2HeavyHold);

        if (p1HeavyUp)
        {
            HeavyToggleP1(false);
            p1HeavyUp = false;
        }
        if (p2HeavyUp)
        {
            HeavyToggleP2(false);
            p2HeavyUp = false;
        }
    }

    private void HandlePauseEvents()
    {
        if (p1PauseDown || p2PauseDown)
        {
            PauseToggle(true);
            p1PauseDown = false;
            p2PauseDown = false;
        }

        Pause(p1PauseHold || p2PauseHold);

        if(p1PauseUp || p2PauseUp)
        {
            PauseToggle(false);
            p1PauseUp = false;
            p2PauseUp = false;
        }
    }

    private void HandleSprintEvents()
    {
        if (p1SprintDown)
        {
            SprintToggleP1(true);
            p1SprintDown = false;
        }
        if (p2SprintDown)
        {
            SprintToggleP2(true);
            p2SprintDown = false;
        }

        SprintP1(p1SprintHold);
        SprintP2(p2SprintHold);

        if (p1SprintUp)
        {
            SprintToggleP1(false);
            p1SprintUp = false;
        }
        if (p2SprintUp)
        {
            SprintToggleP2(false);
            p2SprintUp = false;
        }
    }

    #endregion

    #region PAUSE MENU

    // Is called every frame if the game is in a paused state
    private void HandlePauseMenuEvents()
    {
        if (p1UnpauseDown || p2UnpauseDown)
        {
            UnpauseToggle(true);
            p1UnpauseDown = false;
            p2UnpauseDown = false;
        }

        Unpause(p1UnpauseHold || p2UnpauseHold);

        if (p1UnpauseUp || p2UnpauseUp)
        {
            UnpauseToggle(false);
            p1UnpauseUp = false;
            p2UnpauseUp = false;
        }
    }

    #endregion

    #region HELPERS

    // Subscribes to all character specific events
    public void SetupCharacterInput(Character characterObj)
    {
        // Ensure these methods are only subscribed to once
        PreUpdate += characterObj.PreInputUpdate;
        PostUpdate += characterObj.PostInputUpdate;
        PauseToggle += characterObj.Pause;
        UnpauseToggle += characterObj.Pause;

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

    private float GetDelta(UpdateTypes type)
    {
        return type == UpdateTypes.FixedUpdate ? Time.fixedDeltaTime : Time.deltaTime;
    }

    // Is a player currently requesting a jump?
    public bool IsRequestingJump(bool player1)
    {
        if (player1)
            return p1JumpHold;
        else
            return p2JumpHold;
    }

    #endregion
}
