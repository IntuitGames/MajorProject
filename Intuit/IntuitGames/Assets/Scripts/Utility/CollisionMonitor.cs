using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;/// <summary>
/// Monitors for and stores collision data.
/// </summary>public class CollisionMonitor : MonoBehaviour{
    [SerializeField, ReadOnly]
    private bool _isColliding;
    [SerializeField, ReadOnly]
    private int _collisionCount;

    public bool isColliding
    {
        get
        {
            return _isColliding;
        }
        private set
        {
            _isColliding = value;
        }
    }
    public int collisionCount
    {
        get
        {
            return _collisionCount;
        }
        private set
        {
            _collisionCount = value;
        }
    }

    public List<GameObject> collisionObjects = new List<GameObject>(10);
    private List<Collision> collisionInfos = new List<Collision>(10);

    public event System.Action<int> OnCollisionCountChange = delegate { };
    public event System.Action<GameObject> OnNewCollisionObject = delegate { };
    public event System.Action<GameObject> OnRemovedCollisionObject = delegate { };

    void Update()
    {
        float refCount = collisionObjects.Count;

        // Remove nulls
        collisionObjects.RemoveAll(x => !x);
        collisionInfos.RemoveAll(x => !x.gameObject);

        // Check for change
        if (refCount != collisionObjects.Count)
            UpdateData();
    }

    void OnLevelWasLoaded(int level)
    {
        float refCount = collisionObjects.Count;

        // Clear list
        collisionObjects.Clear();
        collisionInfos.Clear();

        // Check for change
        if (refCount != collisionObjects.Count)
            UpdateData();
    }

    void OnCollisionEnter(Collision col)
    {
        if (!collisionObjects.Contains(col.gameObject))
        {
            collisionObjects.Add(col.gameObject);
            collisionInfos.Add(col);
            UpdateData();
            OnNewCollisionObject(col.gameObject);
        }
    }

    void OnCollisionExit(Collision col)
    {
        if (collisionObjects.Contains(col.gameObject))
        {
            collisionObjects.Remove(col.gameObject);
            collisionInfos.Remove(col);
            UpdateData();
            OnRemovedCollisionObject(col.gameObject);
        }
    }    private void UpdateData()    {
        collisionCount = collisionObjects.Count;
        isColliding = collisionCount > 0;
        OnCollisionCountChange(collisionCount);
    }        // Retrieves the collision info for a specific object    public bool GetCollisionInfo(GameObject collisionObject, ref Collision collisionInfo)
    {
        if (!collisionObjects.Contains(collisionObject))
            return false;

        collisionInfo = collisionInfos.First(x => x.gameObject == collisionObject);
        return true;
    }}