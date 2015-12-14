using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Monitors for and stores collision data.
/// </summary>
public class CollisionMonitor : MonoBehaviour
{
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

    public HashSet<Collider> collisionObjects = new HashSet<Collider>();

    public event System.Action<int> OnCollisionCountChange = delegate { };
    public event System.Action<GameObject> OnNewCollisionObject = delegate { };
    public event System.Action<GameObject> OnRemovedCollisionObject = delegate { };

    void Update()
    {
        float refCount = collisionObjects.Count;

        // Remove nulls, disabled colliders and inactive game objects.
        if (collisionObjects.Any(x => x == null || !x || !x.gameObject.activeInHierarchy || !x.enabled))
        {
            collisionObjects = new HashSet<Collider>(collisionObjects.Where(x => x != null && x && x.gameObject.activeInHierarchy && x.enabled));
        }

        // Check for change
        if (refCount != collisionObjects.Count)
            UpdateData();
    }

    void OnLevelWasLoaded(int level)
    {
        float refCount = collisionObjects.Count;

        // Clear list
        collisionObjects.Clear();

        // Check for change
        if (refCount != collisionObjects.Count)
            UpdateData();
    }

    void OnCollisionEnter(Collision col)
    {
        if (!collisionObjects.Contains(col.collider))
        {
            collisionObjects.Add(col.collider);
            UpdateData();
            OnNewCollisionObject(col.gameObject);
        }
    }

    void OnCollisionExit(Collision col)
    {
        if (collisionObjects.Contains(col.collider))
        {
            collisionObjects.Remove(col.collider);
            UpdateData();
            OnRemovedCollisionObject(col.gameObject);
        }
    }

    private void UpdateData()
    {
        collisionCount = collisionObjects.Count;
        isColliding = collisionCount > 0;
        OnCollisionCountChange(collisionCount);
    }

    // Call this if manually disabling collider or game object
    public void RemoveCollidingObject(Collider removeCollider)
    {
        if (collisionObjects.Contains(removeCollider))
        {
            collisionObjects.Remove(removeCollider);
            UpdateData();
            OnRemovedCollisionObject(removeCollider.gameObject);
        }
    }
}
