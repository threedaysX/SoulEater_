using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Accumulation : DisposableSkill
{
    public const string accumulationForceField = "AccumulationForceField";
    public int maxEnergeBallAmount = 20;
    public float slowSelfValue = 20f;
    public float fieldDuration = 2f;
    public GameObject energeBall;
    private List<GameObject> activeBalls = new List<GameObject>();

    protected override void AddAffectEvent()
    {
        immediatelyAffect.AddListener(ApplyForceField);
        immediatelyAffect.AddListener(SlowSelf);
    }

    private void Start()
    {
        // Render Objects with pools in parent.
        RenderEnergeBallOnStart();
    }

    private void ApplyForceField()
    {
        var cdc = sourceCaster.cumulativeDataController;
        void FieldEffect() { GenerateEnergeBall(); };
        bool IsAttacked()
        {
            if (cdc.GetData(accumulationForceField, CumulativeDataType.Take) >= 1)
            {
                cdc.ModifyData(accumulationForceField, CumulativeDataType.Take, 0);
                return true;
            }
            return false;
        };
        cdc.AddData(accumulationForceField, CumulativeDataType.Take, 0);
        sourceCaster.buffController.AddBuff(accumulationForceField, FieldEffect, null, fieldDuration, IsAttacked);
    }

    private void SlowSelf()
    {
        Debuff.Instance.SlowMoveSpeed(sourceCaster, slowSelfValue, fieldDuration);
    }

    private void GenerateEnergeBall()
    {
        // Re-position energeBall.
        EnergeBall ballObj = ObjectPools.Instance.GetObjectInPools<EnergeBall>(energeBall.name, GetBallPos(), true);
        ballObj.lifeTime = 20f;
        ballObj.ResetEnergeBallLifeTime();
        activeBalls.Add(ballObj.gameObject);
        // Start to aim target or front side.
    }

    private void ResetAllEnergeBall()
    {
        ObjectPools.Instance.Reload(energeBall.name, activeBalls);
    }

    private void RenderEnergeBallOnStart()
    {
        ObjectPools.Instance.RenderObjectPoolsInParent(energeBall, maxEnergeBallAmount, this.transform);
    }

    private Vector3 GetBallPos()
    {
        SpriteRenderer rdr = sourceCaster.GetComponent<SpriteRenderer>();
        float xHalfBodySize = rdr.bounds.size.x / 2;
        float yHalfBodySize = rdr.bounds.size.y / 2;
        Vector3 center = sourceCaster.skillController.skillCenterPoint.position;
        float xRange = center.x + xHalfBodySize;
        float yRange = center.y + yHalfBodySize;
        float x = Random.Range(-xRange - 3, xRange + 3);
        float y = Random.Range(yRange + 2, yRange + 4);
        return new Vector3(x, y, 0);
    }
}
