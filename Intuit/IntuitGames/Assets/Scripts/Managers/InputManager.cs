using UnityEngine;
using System.Collections;
using System;

public class InputManager : MonoBehaviour
{
    public enum Player : uint { P1 = 1, P2 = 2 }; // Temp: This should be moved into the PlayerManager when created (This could also just be two const strings)

    // Input Axes Strings
    private const string ForwardStr = "Vertical_";
    private const string RightStr = "Horizontal_";
    private const string JumpStr = "Jump_";
    private const string DashStr = "Dash_";
    private const string HeavyStr = "Heavy_";
    private const string PauseStr = "Submit_";
    private const string UnpauseStr = "Submit_";

    // Events
    public event Action<float> ForwardP1 = delegate { };
    public event Action<float> ForwardP2 = delegate { };
    public event Action<float> RightP1 = delegate { };
    public event Action<float> RightP2 = delegate { };
    public event Action JumpP1 = delegate { };
    public event Action JumpP2 = delegate { };
    public event Action DashP1 = delegate { };
    public event Action DashP2 = delegate { };
    public event Action HeavyP1 = delegate { };
    public event Action HeavyP2 = delegate { };
    public event Action<Player> Pause = delegate { };
    public event Action<Player> Unpause = delegate { };

    void Awake()
    {
        // Testing
        ForwardP1 += (val) => { if (val > 0) Debug.Log("Player 1 moving forward"); else if (val < 0) Debug.Log("Player 1 moving backward"); };
        RightP1 += (val) => { if (val > 0) Debug.Log("Player 1 moving right"); else if (val < 0) Debug.Log("Player 1 moving left"); };
        JumpP1 += () => Debug.Log("Player 1 Jump");
        DashP1 += () => Debug.Log("Player 1 Dash");
        HeavyP1 += () => Debug.Log("Player 1 Heavy");
        ForwardP2 += (val) => { if (val > 0) Debug.Log("Player 2 moving forward"); else if (val < 0) Debug.Log("Player 2 moving backward"); };
        RightP2 += (val) => { if (val > 0) Debug.Log("Player 2 moving right"); else if (val < 0) Debug.Log("Player 2 moving left"); };
        JumpP2 += () => Debug.Log("Player 2 Jump");
        DashP2 += () => Debug.Log("Player 2 Dash");
        HeavyP2 += () => Debug.Log("Player 2 Heavy");
        Pause += (player) => Debug.Log("Pause requested by player " + (uint)player);
        Unpause += (player) => Debug.Log("Unpause requested by player " + (uint)player);
    }

    void Update()
    {
        // TO DO: Switch on game mode enum (in GameModeManager) to determine which input events to raise

        // Raise axis input events
        ForwardP1(Input.GetAxis(ForwardStr + Player.P1));
        ForwardP2(Input.GetAxis(ForwardStr + Player.P2));
        RightP1(Input.GetAxis(RightStr + Player.P1));
        RightP2(Input.GetAxis(RightStr + Player.P2));

        // Raise action button event
        if (Input.GetButtonDown(JumpStr + Player.P1))
            JumpP1();
        if (Input.GetButtonDown(JumpStr + Player.P2))
            JumpP2();
        if (Input.GetButtonDown(DashStr + Player.P1))
            DashP1();
        if (Input.GetButtonDown(DashStr + Player.P2))
            DashP2();
        if (Input.GetButtonDown(HeavyStr + Player.P1))
            HeavyP1();
        if (Input.GetButtonDown(HeavyStr + Player.P2))
            HeavyP2();

        // Pause and unpause check
        if (Input.GetButtonDown(PauseStr + Player.P1))
            Pause(Player.P1);
        if (Input.GetButtonDown(PauseStr + Player.P2))
            Pause(Player.P2);

        // To be uncommented once modes are implemented
        //if (Input.GetButtonDown(UnpauseStr + Player.P1))
        //    Unpause(Player.P1);
        //if (Input.GetButtonDown(UnpauseStr + Player.P2))
        //    Unpause(Player.P2);
    }
}
