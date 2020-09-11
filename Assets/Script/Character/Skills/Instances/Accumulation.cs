using UnityEngine;

public class Accumulation : DisposableSkill
{
    public const string accumulationForceField = "AccumulationForceField";
    public int maxEnergeBallAmount = 20;
    public float slowSelfValue = 20f;
    public float fieldDuration = 2f;
    public GameObject energeBall;
    private int activeBallsCount;

    protected override void AddAffectEvent()
    {
        immediatelyAffect.AddListener(ApplyForceField);
        immediatelyAffect.AddListener(SlowSelf);
    }

    protected override void Start()
    {
        base.Start();
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
        if (activeBallsCount == maxEnergeBallAmount)
        {
            return;
        }
        // Re-position energeBall.
        EnergeBall ball = ObjectPools.Instance.GetObjectInPools<EnergeBall>(energeBall.name, GetBallPos(), default, true);
        // Reload [EnergeBall] for sourceCaster used.
        ObjectPools.Instance.Reload(energeBall.name, ball, true, sourceCaster.characterName);
        ball.lifeTime = 20f;
        ball.ResetEnergeBallLifeTime(() => ReloadEnergeBall());
        activeBallsCount++;
    }

    private void ReloadEnergeBall()
    {
        // Unload [EnergeBall] that bind with sourceCaster.
        var ball = ObjectPools.Instance.Unload<EnergeBall>(energeBall.name, sourceCaster.characterName);
        // Reload(reset) this ball to public obj pools.
        ObjectPools.Instance.Reload(energeBall.name, ball.gameObject);
        activeBallsCount--;
    }

    private void RenderEnergeBallOnStart()
    {
        ObjectPools.Instance.RenderObjectPoolsInParent(energeBall, maxEnergeBallAmount, default);
    }

    private Vector3 GetBallPos()
    {
        SpriteRenderer rdr = sourceCaster.GetComponent<SpriteRenderer>();
        float xHalfBodySize = rdr.bounds.size.x / 2;
        float yHalfBodySize = rdr.bounds.size.y / 2;
        Vector3 center = sourceCaster.skillController.skillCenterPoint.position;
        float xRange = center.x + xHalfBodySize;
        float yRange = center.y + yHalfBodySize;
        float x = Random.Range(xRange - 3, xRange + 3);
        float y = Random.Range(yRange + 2, yRange + 4);
        return new Vector3(x, y, 0);
    }
}
