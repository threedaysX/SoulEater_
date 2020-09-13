using System.Collections;
using UnityEngine;

public class AiHaviourBase : ScriptableObject
{
    protected AI Ai { get; private set; }

    public void GetCurrentAIHavior(AI currentAi)
    {
        if (Ai == null)
            Ai = currentAi;
    }

    protected T AI<T>() where T : Component
    {
        return Ai.GetComponent<T>();
    }

    protected void RunCoroutine(IEnumerator method, MonoBehaviour mono = default)
    {
        if (mono == default)
            mono = Ai;
        mono.StartCoroutine(method);
    }

    protected void StopCoroutine(IEnumerator method, MonoBehaviour mono = default)
    {
        if (mono == default)
            mono = Ai;
        mono.StopCoroutine(method);
    }
}
