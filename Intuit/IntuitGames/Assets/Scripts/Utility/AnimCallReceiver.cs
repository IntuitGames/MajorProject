﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
/// Receives callback from encapsulation-breaking animator and raises events.
/// </summary>
    public UnityIntEvent OnFootStep = new UnityIntEvent();
    {
        OnFootStep.Invoke(footStepIndex);
    }