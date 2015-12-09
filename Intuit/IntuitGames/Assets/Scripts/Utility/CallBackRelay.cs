using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// Receives callback from encapsulation-breaking animator and UI elements to raise custom raises events.
/// </summary>
public class CallBackRelay : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public UnityIntEvent OnFootStep = new UnityIntEvent();

    public UnityEventDataEvent OnSelect = new UnityEventDataEvent();

    public UnityEventDataEvent OnDeselect = new UnityEventDataEvent();

    void FootStep(int footStepIndex)
    {
        OnFootStep.Invoke(footStepIndex);
    }

    void ISelectHandler.OnSelect(BaseEventData eventData)
    {
        OnSelect.Invoke(eventData);
    }

    void IDeselectHandler.OnDeselect(BaseEventData eventData)
    {
        OnDeselect.Invoke(eventData);
    }

    [System.Serializable]
    public class UnityIntEvent : UnityEvent<int> {}

    [System.Serializable]
    public class UnityEventDataEvent : UnityEvent<BaseEventData> {}
}
