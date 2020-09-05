using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Accumulation : DisposableSkill
{
    public const string accumulationForceField = "AccumulationForceField";
    public int maxEnergeBallAmount = 20;
    public float slowSelfValue = 20f;
    public GameObject energeBall;

    protected override void AddAffectEvent()
    {
        immediatelyAffect.AddListener(ApplyForceField);
        immediatelyAffect.AddListener(SlowSelf);
    }

    private void Start()
    {
        // Render Objects with pools in parent.
        ResetEnergeBallOnStart();
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
        // Re-position energeBall.
        GameObject ballObj = ObjectPools.Instance.GetObjectInPools(energeBall.name, GetBallPos());
        ResetEnergeBallLifeTime(ballObj);
        // Start to aim target or front side.
    }

    private void ResetEnergeBallLifeTime(GameObject energeBall)
    {
        energeBall.SetActive(true);
        Counter.Instance.StopAllCountDown(energeBall.GetComponent<MonoBehaviour>());
        Counter.Instance.StartCountDown(energeBall.GetComponent<MonoBehaviour>(), 20f, false, null, delegate { energeBall.SetActive(false); });
    }

    private void ResetEnergeBallOnStart()
    {
        ObjectPools.Instance.RenderObjectPoolsInParent(energeBall, maxEnergeBallAmount, this.transform);
    }

    private Vector3 GetBallPos()
    {
        float x = Random.Range(-5, 5);
        float y = Random.Range(4, 8);
        return sourceCaster.skillController.skillCenterPoint.position + new Vector3(x, y, 0);
    }
}
