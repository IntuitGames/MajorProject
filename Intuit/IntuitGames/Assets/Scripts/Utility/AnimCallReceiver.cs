using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// Receives callback from encapsulation-breaking animator and raises events.
/// </summary>
public class AnimCallReceiver : MonoBehaviour
{
    public UnityIntEvent OnFootStep = new UnityIntEvent();

    void FootStep(int footStepIndex)
    {
        OnFootStep.Invoke(footStepIndex);
    }

    [System.Serializable]
    public class UnityIntEvent : UnityEvent<int> {}
}
