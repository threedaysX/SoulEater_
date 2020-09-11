using UnityEngine;

// 複製用: 
// [CreateAssetMenu(menuName = "Character/AI/Action/繼承的新ClassName")]
public abstract class AiAction : AiHaviourBase
{
    public bool on = true;
    public bool switchOnActionOnStart = true;

    /// <summary>
    /// 此動作的權重，基本以每2點為一個階段。
    /// [0~1] 不太想
    /// [2~3] 較少
    /// [4~5] 普通
    /// [6~7] 常採取的動作
    /// [8~9] 擅長而且喜歡選擇的動作
    /// [10] 超想這麼做
    /// </summary>
#if UNITY_EDITOR
    [ReadOnly]
#endif
    public float currentActionWeight;

    /// <summary>
    /// 這個行動已經被降低過了N點權重
    /// </summary>
    public float DiffCount
    {
        get
        {
            return _diffCount;
        }
        set
        {
            _diffCount = value;
            if (_diffCount < 0)
                _diffCount = 0;
        }
    }

    public float originalActionWeight;

    [Header("行為後延遲")]
    public float offsetActionDelay; // 校正行為延遲(可調整)
#if UNITY_EDITOR
    [ReadOnly]
#endif
    public float additionActionDelay;   // 行為本身延遲(至行為結束的時長)

    [Header("權重減值")]
    public float minusWeightAmountWhenNotSuccess = 2;
    public float minusWeightAmountAfterAction = 1;
#if UNITY_EDITOR
    [ReadOnly]
#endif
    [SerializeField] private float _diffCount;

    [Header("行為")]
    public AiActionType actionType;
    public Judgement[] judjements;

    [Header("接續行為")]
    public LinkedAction linkedAction;

    public bool CheckActionThatCanDo()
    {
        // 若權重已經小於0，則重置權重。
        if (currentActionWeight < 0)
        {
            ResetWeight();
        }
        // 沒有任何條件，直接觸發
        if (judjements.Length == 0)
        {
            return true;
        }
        foreach (Judgement judje in judjements)
        {
            if (judje == null)
                continue;
            // Reset
            judje.GetCurrentAIHavior(ai);
            // 開始檢查該動作的各個觸發條件
            judje.StartCheckActCondition();
            if (judje.CheckTrueConditionCount())
            {
                // 將判斷後的權重設在動作權重上
                if (judje.actionWeightAfterJudge != 0)
                    currentActionWeight = judje.actionWeightAfterJudge - DiffCount;
                return true;
            }
        }
        return false;
    }

    public void AddDiffCount(float diff)
    {
        DiffCount += diff;
    }

    public void ResetActionOn()
    {
        if (switchOnActionOnStart)
        {
            on = true;
        }
    }
    /// <summary>
    /// 重置權重與權重減值
    /// </summary>
    /// <param name="reduceDiffCount">可決定權重減值要恢復多少，而不是歸0</param>
    public void ResetWeight(float reduceDiffCount = -999)
    {
        currentActionWeight = originalActionWeight;
        if (reduceDiffCount == -999)
            DiffCount = 0;
        else
            DiffCount -= reduceDiffCount;
    }
    public void ApplyActionDelay(float value)
    {
        additionActionDelay = value;
    }
    public void SetAiActionSwitchOn(bool on)
    {
        this.on = on;
    }

    /// <summary>
    /// 做什麼動作
    /// </summary>
    public abstract bool StartActHaviour();
}

[System.Serializable]
public struct LinkedAction
{
    public float delay;             // 執行連接動作前的延遲
    public bool ignoreJudgement;    // 無視連接動作的條件判斷
    public bool influenceWeight;    // 是否影響連接動作的權重
    public AiAction action;
}
