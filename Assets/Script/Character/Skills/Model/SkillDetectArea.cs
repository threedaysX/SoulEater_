using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SkillDetectArea : MonoBehaviour
{
    public Collider2D detectArea;

    private readonly List<Character> targets = new List<Character>();
    private Character sourceCaster;
    private bool includesSelf;

    private ISkillDetectControl _control;

    public List<Character> Detect(Character sourceCaster, ISkillDetectControl control, bool includesSelf, Vector3 centerPos = default)
    {
        // Set up area stats.
        #region Init
        // Set area center pos.
        if (centerPos == default)
        {
            this.transform.position = sourceCaster.transform.position;
        }
        else
        {
            this.transform.position = centerPos;
        }
        this.gameObject.layer = sourceCaster.gameObject.layer;
        this.sourceCaster = sourceCaster;
        this.includesSelf = includesSelf;
        #endregion
        
        // Set up type.
        _control = control;
        _control.SetupArea(detectArea);

        // Return.
        return GetDetected();
    }

    private List<Character> GetDetected()
    {
        List<Collider2D> results = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D() { layerMask = this.gameObject.layer };

        Physics2D.OverlapCollider(detectArea, filter, results);
        foreach (var col in results)
        {
            Character target = col.GetComponent<Character>();
            if (!col.CompareTag(sourceCaster.tag))
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

    public interface ISkillDetectControl
    {
        void SetupArea(Collider2D detectArea);
    }

    public class CircleDetect : ISkillDetectControl
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

    public class BoxDetect : ISkillDetectControl
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
