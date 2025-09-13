using UnityEngine;

namespace SuzuFactory.Alterith
{
    public struct ClothingSMRTuple
    {
        public SkinnedMeshRenderer Source;
        public SkinnedMeshRenderer DestinationOriginal;
        public SkinnedMeshRenderer DestinationConverted;
        public bool Excluded;
        public bool TransferBoneWeights;

        public ClothingSMRTuple(SkinnedMeshRenderer source, SkinnedMeshRenderer destinationOriginal, SkinnedMeshRenderer destinationConverted, bool excluded, bool transferBoneWeights)
        {
            Source = source;
            DestinationOriginal = destinationOriginal;
            DestinationConverted = destinationConverted;
            Excluded = excluded;
            TransferBoneWeights = transferBoneWeights;
        }
    }
}
