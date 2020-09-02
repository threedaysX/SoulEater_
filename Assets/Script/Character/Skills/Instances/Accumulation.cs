using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Accumulation : DisposableSkill
{
    public const string accumulationForceField = "AccumulationForceField";
    public float slowSelfValue = 20f;

    protected override void AddAffectEvent()
    {
        immediatelyAffect.AddListener(ApplyForceField);
        immediatelyAffect.AddListener(SlowSelf);
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
        sourceCaster.buffController.AddBuff(accumulationForceField, FieldEffect, null, currentSkill.duration, IsAttacked);
    }

    

    private void SlowSelf()
    {
        Debuff.Instance.SlowMoveSpeed(sourceCaster, slowSelfValue, currentSkill.duration);
    }

    private void GenerateEnergeBall()
    {
        Vector3 ballPos = GetBallPos();
    }

    private Vector3 GetBallPos()
    {
        float x = Random.Range(-5, 5);
        float y = Random.Range(4, 8);
        return sourceCaster.skillController.skillCenterPoint.position + new Vector3(x, y, 0);
    }
}
