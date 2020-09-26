using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneController : Singleton<SceneController>
{
    public UnityEvent onSceneChangedCall;

    private void Start()
    {
        onSceneChangedCall.AddListener(
            delegate
            {
                AudioControl.Fmod.Instance.ReleaseAll();
                CameraControl.OnDisableCall();
            });
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;     
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        onSceneChangedCall.Invoke();
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void DelayLoadScene(string sceneName, float duration)
    {
        StartCoroutine(DelayLoadSceneCoroutine(sceneName, duration));
    }

    private IEnumerator DelayLoadSceneCoroutine(string sceneName, float duration)
    {
        yield return new WaitForSeconds(duration);
        SceneManager.LoadScene(sceneName);
    }
}

public interface ISceneEventBase
{
    void OnSceneChanged();
    void OnSceneLoad(string sceneName);
}
