using UnityEngine;

public interface IFacement
{
    void FaceTarget(Character self, Transform target, bool force = false);
}
