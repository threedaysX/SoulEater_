using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ColliderDetectControl : MonoBehaviour
{
    public Collider2D detectArea;
    private bool includesSelf;

    private IColDetectControl _control;

    public List<T> Detect<T>(Vector3 detectCenterPos, string tag, int sourceLayer, IColDetectControl control, bool includesSelf) where T: MonoBehaviour
    {
        // Set up area stats.
        #region Init
        // Set area center pos.
        this.transform.position = detectCenterPos;
        this.gameObject.layer = sourceLayer;
        this.includesSelf = includesSelf;
        #endregion
        
        // Set up type.
        _control = control;
        _control.SetupArea(detectArea);

        // Return.
        return GetDetected<T>(tag);
    }

    private List<T> GetDetected<T>(string tag) where T : MonoBehaviour
    {
        List<Collider2D> cols = new List<Collider2D>();
        List<T> targets = new List<T>();
        ContactFilter2D filter = new ContactFilter2D() { layerMask = this.gameObject.layer };

        Physics2D.OverlapCollider(detectArea, filter, cols);
        foreach (var col in cols)
        {
            T target = col.GetComponent<T>();
            if (!col.CompareTag(tag))
            {
                if (target != null)
                {
                    targets.Add(target);
                }
            }
            else
            {
                if (includesSelf)
                {
                    if (target != null)
                    {
                        targets.Add(target);
                    }
                }
            }
        }
        return targets;
    }

    public interface IColDetectControl
    {
        void SetupArea(Collider2D detectArea);
    }

    public class CircleDetect : IColDetectControl
    {
        private float radius;

        public CircleDetect(float radius)
        {
            this.radius = radius;
        }

        public void SetupArea(Collider2D detectArea)
        {           
            detectArea.enabled = true;
            detectArea.GetComponent<CircleCollider2D>().radius = radius;
        }
    }

    public class BoxDetect : IColDetectControl
    {
        private Vector2 size;

        public BoxDetect(Vector2 size)
        {
            this.size = size;
        }

        public void SetupArea(Collider2D detectArea)
        {
            detectArea.enabled = true;
            detectArea.GetComponent<BoxCollider2D>().size = size;
        }
    }
}
