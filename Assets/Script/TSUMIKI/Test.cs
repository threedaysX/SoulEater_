using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : Singleton<Test>
{
    public Character sourceCaster;

    public void AddBuffs(string affixName)
    {
        switch (affixName)
        {
            case "001":
                AddBuff001_TriggerPer100Health();
                break;
            case "002":
                AddBuff002_TriggerAttack4Times();
                break;
            case "005":
                AddBuff005_TriggerIsEvading();
                break;
            case "006":
                AddBuff006_TriggerKeyQ();
                break;

        }
    }
    //////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////
    // 【自身每扣100血，會再額外受到10點傷害】
    public const string TestFor100Hp = "TestFor100Hp";
    public void AddBuff001_TriggerPer100Health()
    {
        void Affect()
        {
            sourceCaster.TakeDamage(new DamageData(ElementType.None, 10));
            Debug.LogWarning("自身每扣100血，會再額外受到10點傷害，持續5秒");
        }

        // 【效果移除時，自身恢復100hp】
        void EndAffect() { sourceCaster.CurrentHealth += 100f; }
        sourceCaster.cumulativeDataController.AddData(TestFor100Hp, cdType, 0);

        // 效果持續5秒，持續期間 【自身每扣100血，會再額外受到10點傷害】
        sourceCaster.buffController.AddBuff(TestFor100Hp, Affect, EndAffect, 5f, delegate { return TriggerPer100Health(); });
    }

    //// 儲存傷害用
    public CumulativeDataType cdType = CumulativeDataType.Take;
    private bool TriggerPer100Health()
    {
        var cdc = sourceCaster.cumulativeDataController;
        int cumulativeDamageTake = cdc.GetData(TestFor100Hp, cdType);
        if (cumulativeDamageTake >= 100)
        {
            cdc.ModifyData(TestFor100Hp, cdType, 0);
            return true;
        }
        return false;
    }
    //////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////
    // 【每當翻滾時..........】
    public const string TestForEvading = "TestForEvading";
    public void AddBuff005_TriggerIsEvading()
    {
        void Affect() { Debug.LogWarning("每當跳躍時.........."); }
        void EndAffect() { }
        sourceCaster.buffController.AddBuff(TestForEvading, Affect, EndAffect, -1, delegate { return TriggerIsEvading(); });
    }
    private bool TriggerIsEvading()
    {
        var dsc = sourceCaster.operationController;
        if (dsc.isEvading)
            return true;
        return false;
    }
    //////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////
    // 【當成功攻擊4次時..........】
    public const string TestForAttack4Times = "TestForAttack4Times";
    public CumulativeDataType cdHitType = CumulativeDataType.HitTimes;
    public void AddBuff002_TriggerAttack4Times()
    {
        void Affect() { Debug.LogWarning("當成功攻擊4次時.........."); }
        void EndAffect() { }
        sourceCaster.cumulativeDataController.AddData(TestForAttack4Times, cdHitType, 0);
        sourceCaster.buffController.AddBuff(TestForAttack4Times, Affect, EndAffect, -1, delegate { return TriggerAttack4Times(); });
    }
    private bool TriggerAttack4Times()
    {
        var cdc = sourceCaster.cumulativeDataController;
        if (cdc.GetData(TestForAttack4Times, cdHitType) >= 4)
        {
            cdc.ModifyData(TestForAttack4Times, cdHitType, 0);
            return true;
        }
        return false;
    }
    //////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////
    // 【當按下按鍵Q時..........】
    public const string TestForKeyQ = "TestForKeyQ";
    public void AddBuff006_TriggerKeyQ()
    {
        void Affect() { Debug.LogWarning("當按下按鍵Q時.........."); }
        void EndAffect() { }
        sourceCaster.buffController.AddBuff(TestForKeyQ, Affect, EndAffect, -1, delegate { return TriggerKeyQ(); });
    }
    private bool TriggerKeyQ()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            return true;
        return false;
    }


}
