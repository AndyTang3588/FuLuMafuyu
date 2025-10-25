using UnityEngine;
using System.Collections.Generic;

namespace ZeroFactory.AvatarPoseSystem.NDMF.Interface
{

    public interface IAvatarPoseSystemAlterBody
    {
        GameObject alterBodyAvatar { get; set; }
        bool createHandle { get; set; }
        List<GameObject> ignoreObjects { get; set; }

        Transform transform { get; }
    }


}
