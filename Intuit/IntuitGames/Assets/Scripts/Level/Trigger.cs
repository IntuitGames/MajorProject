using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CustomExtensions;

/// <summary>
/// Base class for trigger effects.
/// </summary>
public abstract class Trigger : MonoBehaviour
{
    // NESTED TYPES
    public enum TriggerType { TriggerVolume = 0, CollisionVolume = 1, ProximityBased = 2 };
    public enum VolumeType { Box = 0, Sphere = 1, Capsule = 2, Mesh = 3 };

    // COMPONENTS
    [HideInInspector]
    public Collider colliderComp;
    [HideInInspector]
    public TimerPlus proximityTimer;

    // INSPECTOR
    public TriggerType triggerType;
    public VolumeType volumeType;
    public float proximityDistance = 1;
    [Range(0.1f, 120)]
    public float proximityCheckFrequency = 1;
    public LayerMask triggerLayer = 1 << 9 | 1 << 10;
    public bool isMultiTrigger;
    [ReadOnly]
    public bool hasBeenTriggered;
    public UnityEvent onTriggeredEvent = new UnityEvent();                  // (SLOW) Let designers use this

    public event System.Action<GameObject> onTriggered = delegate { };      // (FAST) Subscribe to this through code
    protected virtual bool canBeTriggered { get { return true; } }             // Override this if you have custom trigger conditions to specify in child class

    void Start()
    {
        // Just in-case a timer leaks
        if (proximityTimer != null)
        {
            proximityTimer.Dispose();
            proximityTimer = null;
        }

        // Create a new timer if necessary
        if (triggerType == TriggerType.ProximityBased)
        {
            proximityTimer = TimerPlus.Create(proximityCheckFrequency, TimerPlus.Presets.Repeater, CheckProximity);
            proximityTimer.Start();
        }
    }

    // REMOVABLE - Tagged so that it can be removed before the final build if looking for quick optimizations. I would like to use preprocessor directives but apparently Unity doesn't like that.
    void Update()
    {
        // Update trigger type
        if (colliderComp) colliderComp.isTrigger = triggerType == TriggerType.TriggerVolume;

        // Update proximity timer settings
        if (triggerType == TriggerType.ProximityBased && proximityTimer == null)
        {
            proximityTimer = TimerPlus.Create(proximityCheckFrequency, TimerPlus.Presets.Repeater, CheckProximity);
            proximityTimer.Start();
        }
        else if (triggerType != TriggerType.ProximityBased && proximityTimer != null)
        {
            proximityTimer.Dispose();
            proximityTimer = null;
        }

        if (proximityTimer != null)
        {
            proximityTimer.ModifyLength(proximityCheckFrequency);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (triggerLayer == (triggerLayer | (1 << collision.collider.gameObject.layer)))
            CallTrigger(collision.collider.gameObject);
    }

	protected virtual void OnTriggerEnter(Collider other)
    {
        if (triggerLayer == (triggerLayer | (1 << other.gameObject.layer)))
            CallTrigger(other.gameObject);
    }

    private void CheckProximity()
    {
        List<Collider> colliders = Physics.OverlapSphere(transform.position, proximityDistance, triggerLayer).ToList().FindAll(x => x.gameObject != this.gameObject);

        if (colliders != null && colliders.Any() && colliders.Count > 0)
            CallTrigger(colliders.First().gameObject);
    }

    // Call trigger events if able
    public void CallTrigger(GameObject triggerObject)
    {
        if (!gameObject.activeInHierarchy || !enabled) return;

        if (!hasBeenTriggered && canBeTriggered || isMultiTrigger && canBeTriggered)
        {
            hasBeenTriggered = true;

            // Dispose proximity timer
            if (proximityTimer != null && !isMultiTrigger)
                proximityTimer.Dispose();

            // Invoke abstract method 1st
            OnTrigger(triggerObject);

            // Raise Action<> 2nd
            if (onTriggered != null)
                onTriggered(triggerObject);

            // Raise Unity Event 3rd
            if (onTriggeredEvent != null)
                onTriggeredEvent.Invoke();
        }
    }

    protected abstract void OnTrigger(GameObject triggerObject);
}
