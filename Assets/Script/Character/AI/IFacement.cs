using UnityEngine;

public interface IFacement
{
    void FaceTarget(MonoBehaviour self, Transform target, bool force = false);
}
