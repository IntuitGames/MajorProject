using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Handles all input related elements.
/// </summary>
public class InputManager : Manager
{
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

    // Events
    public event Action<float> forwardP1 = delegate { };
    public event Action<float> forwardP2 = delegate { };
    public event Action<float> rightP1 = delegate { };
    public event Action<float> rightP2 = delegate { };
    public event Action<float, float> movementP1 = delegate { };
    public event Action<float, float> movementP2 = delegate { };
    public event Action jumpP1 = delegate { };
    public event Action jumpP2 = delegate { };
    public event Action dashP1 = delegate { };
    public event Action dashP2 = delegate { };
    public event Action heavyP1 = delegate { };
    public event Action heavyP2 = delegate { };
    public event Action pause = delegate { };
    public event Action unpause = delegate { };

    void Update()
    {
        // Only check inputs for certain game modes
        switch (GameManager.modeManager.currentGameMode)
        {
            case ModeManager.GameMode.InGame:
                HandleInGameEvents();
                break;

            case ModeManager.GameMode.PauseMenu:
                HandlePauseMenuEvents();
                break;

            default:
                break;
        }
    }

    private void HandleInGameEvents()
    {
        // Raise axis input events
        forwardP1(Input.GetAxis(forwardStr + player1Str));
        forwardP2(Input.GetAxis(forwardStr + player2Str));
        rightP1(Input.GetAxis(rightStr + player1Str));
        rightP2(Input.GetAxis(rightStr + player2Str));
        movementP1(Input.GetAxis(forwardStr + player1Str), Input.GetAxis(rightStr + player1Str));
        movementP2(Input.GetAxis(forwardStr + player2Str), Input.GetAxis(rightStr + player2Str));

        // Raise action button event
        if (Input.GetButtonDown(jumpStr + player1Str))
            jumpP1();
        if (Input.GetButtonDown(jumpStr + player2Str))
            jumpP2();
        if (Input.GetButtonDown(dashStr + player1Str))
            dashP1();
        if (Input.GetButtonDown(dashStr + player2Str))
            dashP2();
        if (Input.GetButtonDown(heavyStr + player1Str))
            heavyP1();
        if (Input.GetButtonDown(heavyStr + player2Str))
            heavyP2();

        // Pause and unpause check
        if (Input.GetButtonDown(pauseStr + player1Str))
            pause();
        if (Input.GetButtonDown(pauseStr + player2Str))
            pause();
    }

    private void HandlePauseMenuEvents()
    {
        if (Input.GetButtonDown(unpauseStr + player1Str))
            unpause();
        if (Input.GetButtonDown(unpauseStr + player2Str))
            unpause();
    }
}
