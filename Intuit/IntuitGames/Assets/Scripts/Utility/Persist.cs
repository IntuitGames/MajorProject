using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;/// <summary>
/// Attached game objects will persist through scenes.
/// </summary>[DisallowMultipleComponent]public class Persist : MonoBehaviour{
    [HideInInspector, SerializeField]
    private bool persist = true;
    [SerializeField]
    private bool unique = true;
    [SerializeField]
    private string uniqueID = "Unique ID";

    private static Dictionary<string, GameObject> persistList = new Dictionary<string, GameObject>();
    private static Transform persistentTransform;

    void Start() { }    void OnDestroy()
    {
        // Remove this from this list if the object gets destroyed through code
        RemoveFromPersistList();
    }

    void OnLevelWasLoaded(int level)
    {
        // If this component is disabled when a new scene is loaded. Destroy the carrying object.
        if (!enabled)
            Destroy(this.gameObject);
    }    void Awake()
    {
        // Disable editing in play mode
        hideFlags = HideFlags.NotEditable;

        // Destroy this component if disabled
        if (!enabled)
        {
            Destroy(this);
            return;
        }

        // Attempt to add this game object to the persistent list
        persist = AddToPersistList();

        // Destroy this game object if it was unable to be added to the list
        if (!persist)
            Destroy(this.gameObject);
    }    private bool AddToPersistList()
    {
        // Check for unique ID's
        if (unique && persistList.ContainsKey(uniqueID))
            return false;

        // Add to list and make persistent
        persistList.Add(uniqueID, this.gameObject);
        this.transform.SetParent(GetPersistentTrasform());
        DontDestroyOnLoad(this.gameObject);
        return true;
    }

    private void RemoveFromPersistList()
    {
        if (persistList.ContainsValue(this.gameObject))
            persistList.Remove(persistList.First(x => x.Value == this.gameObject).Key);
    }

    private Transform GetPersistentTrasform()
    {
        if (persistentTransform) return persistentTransform;

        // Create the new root for persistent game objects
        persistentTransform = new GameObject().transform;
        persistentTransform.name = "Persistent Objects";
        DontDestroyOnLoad(persistentTransform);
        return persistentTransform;
    }}