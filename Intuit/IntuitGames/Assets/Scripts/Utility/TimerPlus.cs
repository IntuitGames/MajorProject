using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using CustomExtensions;

/// <summary>
/// Time related utility class.
/// </summary>
public class TimerPlus: IDisposable
{
    // Static reference to all timers (Timer's not in here do not function)
    protected static List<TimerPlus> AllTimers = new List<TimerPlus>();

    // Instance fields
    public readonly float Length;                   // How long will the timer take
    public float Value { get; private set; }        // Current time of this timer
    public bool IsPlaying = false;                  // Is the timer currently on and playing?
    public bool AutoReset = false;                  // Will the timer reset automatically once finished
    public bool IsPersistent = false;               // Will the timer automatically dereference itself after completion
    // Note: If IsPersistent = true: manual disposal is required by calling Dispose()
    
    // Quick-access properties
    public float Percentage { get { return Value / Length; } }
    public float TimeRemaining { get { return Value; } }
    public float TimeElapsed { get { return Length - Value; } }

    // Events
    public event Action Elapsed = delegate { };

    #region Constructors

    // Defines how this timer will be used
    public enum Presets { Standard, OneTimeUse, Repeater };

    /// <summary>
    /// Creates a simple one-time use timer.
    /// </summary>
    public TimerPlus(float length, params Action[] actions)
    {
        AllTimers.Add(this);
        Length = length;
        Value = Length;

        TimerPlusSetup(Presets.OneTimeUse);
        
        if (!actions.IsNullOrEmpty())
            actions.ToList().ForEach(x => Elapsed += x);
    }

    /// <summary>
    /// Creates a simple specified preset timer.
    /// </summary>
    public TimerPlus(float length, Presets mode, params Action[] actions)
    {
        AllTimers.Add(this);
        Length = length;
        Value = Length;

        TimerPlusSetup(mode);

        if (!actions.IsNullOrEmpty())
            actions.ToList().ForEach(x => Elapsed += x);
    }

    /// <summary>
    /// Creates a timer with all customized settings.
    /// </summary>
    public TimerPlus(float length, bool startPlaying, bool autoReset, bool persist, params Action[] actions)
    {
        AllTimers.Add(this);
        Length = length;
        Value = Length;

        IsPlaying = startPlaying;
        AutoReset = autoReset;
        IsPersistent = persist;

        if (!actions.IsNullOrEmpty())
            actions.ToList().ForEach(x => Elapsed += x);
    }

    // Sets up timer defaults to presets
    protected void TimerPlusSetup(Presets Mode)
    {
        switch(Mode)
        {
            case Presets.Standard:
                IsPlaying = false;
                AutoReset = false;
                IsPersistent = true;
                break;

            case Presets.OneTimeUse:
                IsPlaying = true;
                AutoReset = false;
                IsPersistent = false;
                break;

            case Presets.Repeater:
                IsPlaying = true;
                AutoReset = true;
                IsPersistent = false;
                break;

            default:
                break;
        }
    }

    #endregion

    #region Static Constructors

    public static TimerPlus Create(float length, params Action[] actions)
    {
        return new TimerPlus(length, actions);
    }

    public static TimerPlus Create(float length, Presets mode, params Action[] actions)
    {
        return new TimerPlus(length, mode, actions);
    }

    public static TimerPlus Create(float length, bool startPlaying, bool autoReset, bool persist, params Action[] actions)
    {
        return new TimerPlus(length, startPlaying, autoReset, persist, actions);
    }

    #endregion

    #region Static Methods

    /// <summary>
    /// Updates all timers the value of all timers.
    /// </summary>
    public static void UpdateAll()
    {
        AllTimers.ForEach(x => x.Update(Time.deltaTime));
    }

    /// <summary>
    /// Disposes all currently active timers.
    /// </summary>
    public static void DisposeAll()
    {
        AllTimers.ForEach(x => x.Dispose());
    }

    /// <summary>
    /// Removes a timer from the list permanently. Causing it to stop running.
    /// </summary>
    /// <param name="Instance"></param>
    public static void Remove(TimerPlus TimerInstance)
    {
        if(AllTimers.Contains(TimerInstance))
        {
            AllTimers.Remove(TimerInstance);
        }
    }

    #endregion

    #region Public Instance Methods

    /// <summary>
    /// Stops the timer and sets the timer value back.
    /// </summary>
    public void Reset()
    {
        IsPlaying = false;
        Value = Length;
    }

    /// <summary>
    /// Resets the timer's value and starts the timer up.
    /// </summary>
    public void Restart()
    {
        Value = Length;
        IsPlaying = true;
    }

    /// <summary>
    /// Toggles the playing state of this timer.
    /// </summary>
    public void Toggle()
    {
        IsPlaying = !IsPlaying;
    }

    /// <summary>
    /// Sets is playing to true.
    /// </summary>
    public void Start()
    {
        IsPlaying = true;
    }

    /// <summary>
    /// Sets is playing to false.
    /// </summary>
    public void Stop()
    {
        IsPlaying = false;
    }

    #endregion

    #region Non-public

    // Updates a timer's value.
    private void Update(float Delta)
    {
        if (IsPlaying)
            Value -= Delta;
        else
            return;

        if (Value <= 0)
        {
            OnTimerElapsed(Mathf.Abs(Value));
        }
    }

    // Called privately. Handles event invocation and reseting.
    protected virtual void OnTimerElapsed(float LeftOver)
    {
        Elapsed.Invoke();

        if(AutoReset)
        {
            Value = Length - LeftOver;
        }
        else
        {
            IsPlaying = false;
            Value = 0;

            if(!IsPersistent)
            {
                this.Dispose();
            }
        }
    }

    #endregion

    #region Disposal

    private bool IsDisposed = false;
    private SafeHandle Handle = new SafeFileHandle(IntPtr.Zero, true);

    /// <summary>
    /// Call to free resources if this object won't be needed again.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);

        // Prevent costly deconstructor
        //GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool Disposing)
    {
        if (IsDisposed)
            return;

        // Free managed objects
        if(Disposing)
        {
            Handle.Dispose();
            Elapsed = null;
        }
        
        // Free unmanaged objects
        Stop();
        Remove(this);

        IsDisposed = true;
    }

    ~TimerPlus()
    {
        Dispose(false);
    }

    #endregion
}

/// <summary>
/// Generic time related utility class.
/// </summary>
public class TimerPlus<T> : TimerPlus
{
    // The object that gets passed into the generic action
    public T ActionParam;

    public event Action<T> ElapsedGeneric = delegate { };

    #region Constructors

    /// <summary>
    /// Creates a simple one-time use timer.
    /// </summary>
    public TimerPlus(float length, T parameter, params Action<T>[] actions) : base(length, null)
    {
        ActionParam = parameter;

        if(!actions.IsNullOrEmpty())
            actions.ToList().ForEach(x => ElapsedGeneric += x);
    }

    /// <summary>
    /// Creates a simple specified preset timer.
    /// </summary>
    public TimerPlus(float length, Presets mode, T parameter, params Action<T>[] actions) : base(length, mode, null)
    {
        ActionParam = parameter;

        if (!actions.IsNullOrEmpty())
            actions.ToList().ForEach(x => ElapsedGeneric += x);
    }

    /// <summary>
    /// Creates a timer with all customized settings.
    /// </summary>
    public TimerPlus(float length, bool startPlaying, bool autoReset, bool persist, T parameter, params Action<T>[] actions)
        : base(length, startPlaying, autoReset, persist, null)
    {
        ActionParam = parameter;

        if (!actions.IsNullOrEmpty())
            actions.ToList().ForEach(x => ElapsedGeneric += x);
    }

    #endregion

    #region Static Constructors

    public static TimerPlus<T> Create(float length, T parameter, params Action<T>[] actions)
    {
        return new TimerPlus<T>(length, parameter, actions);
    }

    public static TimerPlus<T> Create(float length, Presets mode, T parameter, params Action<T>[] actions)
    {
        return new TimerPlus<T>(length, mode, parameter, actions);
    }

    public static TimerPlus<T> Create(float length, bool startPlaying, bool autoReset, bool persist, T parameter, params Action<T>[] actions)
    {
        return new TimerPlus<T>(length, startPlaying, autoReset, persist, parameter, actions);
    }

    #endregion

    #region Non-public

    protected override void OnTimerElapsed(float LeftOver)
    {
        // Overridden to allow for the generic action invocation
        ElapsedGeneric.Invoke(ActionParam);

        base.OnTimerElapsed(LeftOver);
    }

    #endregion

    #region Disposal

    private bool IsDisposed = false;
    private SafeHandle Handle = new SafeFileHandle(IntPtr.Zero, true);

    protected override void Dispose(bool Disposing)
    {
        if (IsDisposed)
            return;

        if(Disposing)
        {
            Handle.Dispose();
            ElapsedGeneric = null;
        }

        base.Dispose(Disposing);
    }

    #endregion
}
