#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace SuzuFactory.Alterith
{
    public enum MaskArea
    {
        WhiteAreas,
        BlackAreas,
    }

    public enum OptionalBool
    {
        Inherit,
        Enable,
        Disable,
    }

    public class AlterithMask : MonoBehaviour, IEditorOnly
    {
        public bool specifyTargetMaterials = false;
        public Material[] targetMaterials;

        public bool specifyMaskImage = false;
        public Texture2D maskImage;
        public MaskArea maskArea = MaskArea.WhiteAreas;

        public OptionalBool applyFitting = OptionalBool.Inherit;

        public bool overrideMinimumMargin = false;
        public float fittingMinimumMargin = 0.0f;
        public bool overrideMarginScale = false;
        public float fittingMarginScale = 1.0f;

        public OptionalBool affectedByWeightTransfer = OptionalBool.Inherit;
        public OptionalBool affectsWeightTransfer = OptionalBool.Inherit;
    }
}

#endif
