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
[System.Serializable]
public class TimerPlus: IDisposable
{
    // Static reference to all timers (Timer's not in here do not function)
    protected static List<TimerPlus> AllTimers = new List<TimerPlus>();

    // Instance fields
    public float Length;                            // How long will the timer take
    public float Value { get; private set; }        // Current time of this timer
    public bool IsPlaying = false;                  // Is the timer currently on and playing?
    public bool AutoReset = false;                  // Will the timer reset automatically once finished
    public bool IsPersistent = false;               // Will the timer automatically dereference itself after completion
    // Note: If IsPersistent = true: manual disposal is required by calling Dispose()
    public bool DisableOnLoad = true;               // Is this timer automatically Disposed() when a new scene loads
    public bool TimeScaled = true;                  // Is this timer affected by time scale
    public bool IsRandomized { get; private set; }  // Does this timer have a randomized length?
    public float MinLength;
    public float MaxLength;
    
    // Quick-access properties
    private float GetNewLength
    {
        get
        {
            Length = IsRandomized ? UnityEngine.Random.Range(MinLength, MaxLength) : Length;
            return Length;
        }
    }
    public float Percentage { get { return Value / Length; } }
    public float TimeRemaining { get { return Value; } }
    public float TimeElapsed { get { return Length - Value; } }

    // Events
    public event Action Elapsed = delegate { };

    #region Constructors

    // Defines how this timer will be used
    public enum Presets { Standard, OneTimeUse, Repeater, BackgroundStandard, BackgroundOneTimeUse, BackgroundRepeater };

    /// <summary>
    /// Creates a non-randomized simple one-time use timer.
    /// </summary>
    public TimerPlus(float length, Action action = null)
    {
        AllTimers.Add(this);
        Length = length;
        IsRandomized = false;
        Value = Length;

        TimerPlusSetup(Presets.OneTimeUse);

        if (!action.IsNullOrEmpty())
            Elapsed += action;
    }

    /// <summary>
    /// Creates a non-randomized simple specified preset timer.
    /// </summary>
    public TimerPlus(float length, Presets mode, Action action = null)
    {
        AllTimers.Add(this);
        Length = length;
        IsRandomized = false;
        Value = Length;

        TimerPlusSetup(mode);

        if (!action.IsNullOrEmpty())
            Elapsed += action;
    }

    /// <summary>
    /// Creates a non-randomized timer with all customized settings.
    /// </summary>
    public TimerPlus(float length, bool startPlaying, bool autoReset, bool persist, bool disableOnLoad, bool timeScaled, Action action = null)
    {
        AllTimers.Add(this);
        Length = length;
        IsRandomized = false;
        Value = Length;

        IsPlaying = startPlaying;
        AutoReset = autoReset;
        IsPersistent = persist;
        DisableOnLoad = disableOnLoad;
        TimeScaled = timeScaled;

        if (!action.IsNullOrEmpty())
            Elapsed += action;
    }

    /// <summary>
    /// Creates a randomized simple one-time use timer.
    /// </summary>
    public TimerPlus(float minLength, float maxLength, Action action = null)
    {
        AllTimers.Add(this);
        MinLength = minLength;
        MaxLength = maxLength;
        IsRandomized = true;
        Value = GetNewLength;

        TimerPlusSetup(Presets.OneTimeUse);

        if (!action.IsNullOrEmpty())
            Elapsed += action;
    }

    /// <summary>
    /// Creates a randomized simple specified preset timer.
    /// </summary>
    public TimerPlus(float minLength, float maxLength, Presets mode, Action action = null)
    {
        AllTimers.Add(this);
        MinLength = minLength;
        MaxLength = maxLength;
        IsRandomized = true;
        Value = GetNewLength;

        TimerPlusSetup(mode);

        if (!action.IsNullOrEmpty())
            Elapsed += action;
    }

    /// <summary>
    /// Creates a randomized timer with all customized settings.
    /// </summary>
    public TimerPlus(float minLength, float maxLength, bool startPlaying, bool autoReset, bool persist, bool disableOnLoad, bool timeScaled, Action action = null)
    {
        AllTimers.Add(this);
        MinLength = minLength;
        MaxLength = maxLength;
        IsRandomized = true;
        Value = GetNewLength;

        IsPlaying = startPlaying;
        AutoReset = autoReset;
        IsPersistent = persist;
        DisableOnLoad = disableOnLoad;
        TimeScaled = timeScaled;

        if (!action.IsNullOrEmpty())
            Elapsed += action;
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
                DisableOnLoad = true;
                TimeScaled = true;
                break;

            case Presets.OneTimeUse:
                IsPlaying = true;
                AutoReset = false;
                IsPersistent = false;
                DisableOnLoad = true;
                TimeScaled = true;
                break;

            case Presets.Repeater:
                IsPlaying = true;
                AutoReset = true;
                IsPersistent = false;
                DisableOnLoad = true;
                TimeScaled = true;
                break;

            case Presets.BackgroundStandard:
                IsPlaying = false;
                AutoReset = false;
                IsPersistent = true;
                DisableOnLoad = false;
                TimeScaled = false;
                break;

            case Presets.BackgroundOneTimeUse:
                IsPlaying = true;
                AutoReset = false;
                IsPersistent = false;
                DisableOnLoad = false;
                TimeScaled = false;
                break;

            case Presets.BackgroundRepeater:
                IsPlaying = true;
                AutoReset = true;
                IsPersistent = false;
                DisableOnLoad = false;
                TimeScaled = false;
                break;

            default:
                break;
        }
    }

    #endregion

    #region Static Constructors

    /// <summary>
    /// Creates a non-randomized simple one-time use timer.
    /// </summary>
    public static TimerPlus Create(float length, Action action = null)
    {
        return new TimerPlus(length, action);
    }

    /// <summary>
    /// Creates a non-randomized simple specified preset timer.
    /// </summary>
    public static TimerPlus Create(float length, Presets mode, Action action = null)
    {
        return new TimerPlus(length, mode, action);
    }

    /// <summary>
    /// Creates a non-randomized timer with all customized settings.
    /// </summary>
    public static TimerPlus Create(float length, bool startPlaying, bool autoReset, bool persist, bool disableOnLoad, bool timeScaled, Action action = null)
    {
        return new TimerPlus(length, startPlaying, autoReset, persist, disableOnLoad, timeScaled, action);
    }

    /// <summary>
    /// Creates a randomized simple one-time use timer.
    /// </summary>
    public static TimerPlus CreateRandom(float minLength, float maxLength, Action action = null)
    {
        return new TimerPlus(minLength, maxLength, action);
    }

    /// <summary>
    /// Creates a randomized simple specified preset timer.
    /// </summary>
    public static TimerPlus CreateRandom(float minLength, float maxLength, Presets mode, Action action = null)
    {
        return new TimerPlus(minLength, maxLength, mode, action);
    }

    /// <summary>
    /// Creates a randomized timer with all customized settings.
    /// </summary>
    public static TimerPlus CreateRandom(float minLength, float maxLength, bool startPlaying, bool autoReset, bool persist, bool disableOnLoad, bool timeScaled, Action action = null)
    {
        return new TimerPlus(minLength, maxLength, startPlaying, autoReset, persist, disableOnLoad, timeScaled, action);
    }

    #endregion

    #region Static Methods

    /// <summary>
    /// Updates all timers the value of all timers.
    /// </summary>
    public static void UpdateAll()
    {
        AllTimers.ForEach(x => { if (x.TimeScaled) x.Update(Time.deltaTime); else x.Update(Time.unscaledDeltaTime); });
        AllTimers.RemoveAll(x => x.IsDisposed);
    }

    /// <summary>
    /// Dispose all timers that should be disabled on load.
    /// </summary>
    public static void DisposeAllOnLoad()
    {
        AllTimers.ForEach(x => { if (x.DisableOnLoad) x.Dispose(); });
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
        Value = GetNewLength;
    }

    /// <summary>
    /// Resets the timer's value and starts the timer up.
    /// </summary>
    public void Restart()
    {
        Value = GetNewLength;
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

    /// <summary>
    /// Sets to timer to zero.
    /// </summary>
    public void End()
    {
        Value = 0;
    }

    /// <summary>
    /// Converts this timer into a randomized timer. (Reset recommended after this)
    /// </summary>
    /// <param name="minLength"></param>
    /// <param name="maxLength"></param>
    public void Randomize(float minLength, float maxLength)
    {
        MinLength = minLength;
        MaxLength = maxLength;
        IsRandomized = true;
    }

    /// <summary>
    /// Converts this timer to a non-randomized one. (Reset recommended after this)
    /// </summary>
    /// <param name="length"></param>
    public void UnRandomize(float length)
    {
        Length = length;
        IsRandomized = false;
    }

    /// <summary>
    /// Changes the length of the timer and also adjusts the value if it is currently above the new length.
    /// </summary>
    /// <param name="newLength">New length of this timer.</param>
    public void ModifyLength(float newLength)
    {
        Length = newLength;
        Value = Mathf.Min(Value, Length);
    }

    /// <summary>
    /// Changes the length of the timer and also adjusts the value if it is currently above the new length with the option to scale it.
    /// </summary>
    /// <param name="newLength">New length of this timer.</param>
    /// <param name="scaleValue">Scale the current value to the new length?</param>
    public void ModifyLength(float newLength, bool scaleValue)
    {
        float NewValue;

        if (scaleValue)
            NewValue = newLength * Percentage;
        else
            NewValue = Mathf.Min(Value, Length);

        Length = newLength;
        Value = NewValue;
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
            Value = GetNewLength - LeftOver;
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

    public bool IsDisposed { get; private set; }
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
            if (Handle != null)
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
[System.Serializable]
public class TimerPlus<T> : TimerPlus
{
    // The object that gets passed into the generic action
    public T ActionParam;

    public event Action<T> ElapsedGeneric = delegate { };

    #region Constructors

    /// <summary>
    /// Creates a non-randomized timer simple one-time use timer.
    /// </summary>
    public TimerPlus(float length, T parameter, Action<T> action = null) : base(length, null)
    {
        ActionParam = parameter;

        if (!action.IsNullOrEmpty())
            ElapsedGeneric += action;
    }

    /// <summary>
    /// Creates a non-randomized timer specified preset timer.
    /// </summary>
    public TimerPlus(float length, Presets mode, T parameter, Action<T> action) : base(length, mode, null)
    {
        ActionParam = parameter;

        if (!action.IsNullOrEmpty())
            ElapsedGeneric += action;
    }

    /// <summary>
    /// Creates a non-randomized timer with all customized settings.
    /// </summary>
    public TimerPlus(float length, bool startPlaying, bool autoReset, bool persist, bool disableOnLoad, bool timeScaled, T parameter, Action<T> action)
        : base(length, startPlaying, autoReset, persist, disableOnLoad, timeScaled, null)
    {
        ActionParam = parameter;

        if (!action.IsNullOrEmpty())
            ElapsedGeneric += action;
    }

    /// <summary>
    /// Creates a randomized timer simple one-time use timer.
    /// </summary>
    public TimerPlus(float minLength, float maxLength, T parameter, Action<T> action = null)
        : base(minLength, maxLength, null)
    {
        ActionParam = parameter;

        if (!action.IsNullOrEmpty())
            ElapsedGeneric += action;
    }

    /// <summary>
    /// Creates a randomized timer specified preset timer.
    /// </summary>
    public TimerPlus(float minLength, float maxLength, Presets mode, T parameter, Action<T> action)
        : base(minLength, maxLength, mode, null)
    {
        ActionParam = parameter;

        if (!action.IsNullOrEmpty())
            ElapsedGeneric += action;
    }

    /// <summary>
    /// Creates a randomized timer with all customized settings.
    /// </summary>
    public TimerPlus(float minLength, float maxLength, bool startPlaying, bool autoReset, bool persist, bool disableOnLoad, bool timeScaled, T parameter, Action<T> action)
        : base(minLength, maxLength, startPlaying, autoReset, persist, disableOnLoad, timeScaled, null)
    {
        ActionParam = parameter;

        if (!action.IsNullOrEmpty())
            ElapsedGeneric += action;
    }

    #endregion

    #region Static Constructors

    /// <summary>
    /// Creates a non-randomized timer simple one-time use timer.
    /// </summary>
    public static TimerPlus<T> Create(float length, T parameter, Action<T> action = null)
    {
        return new TimerPlus<T>(length, parameter, action);
    }

    /// <summary>
    /// Creates a non-randomized timer specified preset timer.
    /// </summary>
    public static TimerPlus<T> Create(float length, Presets mode, T parameter, Action<T> action = null)
    {
        return new TimerPlus<T>(length, mode, parameter, action);
    }

    /// <summary>
    /// Creates a non-randomized timer with all customized settings.
    /// </summary>
    public static TimerPlus<T> Create(float length, bool startPlaying, bool autoReset, bool persist, bool disableOnLoad, bool timeScaled, T parameter, Action<T> action = null)
    {
        return new TimerPlus<T>(length, startPlaying, autoReset, persist, disableOnLoad, timeScaled, parameter, action);
    }

    /// <summary>
    /// Creates a randomized timer simple one-time use timer.
    /// </summary>
    public static TimerPlus<T> CreateRandom(float minLength, float maxLength, T parameter, Action<T> action = null)
    {
        return new TimerPlus<T>(minLength, maxLength, parameter, action);
    }

    /// <summary>
    /// Creates a randomized timer specified preset timer.
    /// </summary>
    public static TimerPlus<T> CreateRandom(float minLength, float maxLength, Presets mode, T parameter, Action<T> action = null)
    {
        return new TimerPlus<T>(minLength, maxLength, mode, parameter, action);
    }

    /// <summary>
    /// Creates a randomized timer with all customized settings.
    /// </summary>
    public static TimerPlus<T> CreateRandom(float minLength, float maxLength, bool startPlaying, bool autoReset, bool persist, bool disableOnLoad, bool timeScaled, T parameter, Action<T> action = null)
    {
        return new TimerPlus<T>(minLength, maxLength, startPlaying, autoReset, persist, disableOnLoad, timeScaled, parameter, action);
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

    new public bool IsDisposed { get; private set;}
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
