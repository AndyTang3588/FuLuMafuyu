using UnityEngine;
using System.Collections.Generic;

namespace ZeroFactory.AvatarPoseSystem.NDMF.Interface
{

    public interface IAvatarPoseSystemExtraBone
    {
        Transform rootTransform { get; set; }
        Transform[] ignoreTransforms { get; set; }
        float handleSize { get; set; }
        bool isDualHandle { get; set; }

        Transform GetAffectedRootTransform();
        List<Transform> GetAffectedTransforms();
    }


}
