using UnityEngine;

namespace ZeroFactory.AvatarPoseSystem.NDMF.Interface
{
    public interface IAvatarPoseSystemPropPlacer
    {
        Transform targetTransform { get; set; }
        string propName { get; set; }
        GameObject[] toggleObjects { get; set; }
        Vector3 positionOffset { get; set; }
        Vector3 rotationOffset { get; set; }
        float handleSize { get; set; }
        Transform transform { get; }
        Transform GetTargetTransform();
        Transform GetAffectedTransform();
        string GetPropName();
    }
}
