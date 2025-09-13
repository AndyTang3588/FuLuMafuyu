#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace SuzuFactory.Alterith
{
    public static class AlterithUtil
    {
        public struct IndexedName
        {
            public string name;
            public int index;
        }

        private struct PreservedBlendShape
        {
            public string name;
            public float[] frameWeights;
            public float weight;
        }

        public static IndexedName[] GetIndexedPath(Transform parent, Transform child)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent), "Parent cannot be null");
            }

            if (child == null)
            {
                throw new ArgumentNullException(nameof(child), "Child cannot be null");
            }

            if (!child.IsChildOf(parent))
            {
                throw new ArgumentException("Child must be a descendant of the parent", nameof(child));
            }

            List<IndexedName> path = new List<IndexedName>();
            Transform current = child;

            while (current != null && current != parent)
            {
                int index = 0;
                bool found = false;

                for (int i = 0; i < current.parent.childCount; ++i)
                {
                    var sibling = current.parent.GetChild(i);

                    if (sibling == current)
                    {
                        path.Add(new IndexedName { name = current.name, index = index });
                        found = true;
                        break;
                    }
                    else if (sibling.name == current.name)
                    {
                        ++index;
                    }
                }

                if (!found)
                {
                    throw new ArgumentException("Child not found in parent's children");
                }

                current = current.parent;
            }

            path.Reverse();
            return path.ToArray();
        }

        public static Transform GetChildByIndexedPath(Transform parent, IndexedName[] path)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent), "Parent cannot be null");
            }

            if (path == null)
            {
                throw new ArgumentException("Path cannot be null or empty", nameof(path));
            }

            Transform current = parent;

            foreach (var indexedName in path)
            {
                int index = 0;
                bool found = false;

                for (int i = 0; i < current.childCount; ++i)
                {
                    var child = current.GetChild(i);

                    if (child.name == indexedName.name)
                    {
                        if (index == indexedName.index)
                        {
                            current = child;
                            found = true;
                            break;
                        }
                        else
                        {
                            ++index;
                        }
                    }
                }

                if (!found)
                {
                    throw new ArgumentException($"Child '{indexedName.name}' with index {indexedName.index} not found in parent '{current.name}'");
                }
            }

            return current;
        }

        public static Transform[] GetHierarchyBetween(Transform parent, Transform child)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent), "Parent cannot be null");
            }

            if (child == null)
            {
                throw new ArgumentNullException(nameof(child), "Child cannot be null");
            }

            if (!child.IsChildOf(parent))
            {
                throw new ArgumentException("Child must be a descendant of the parent", nameof(child));
            }

            List<Transform> hierarchy = new List<Transform>();
            Transform current = child;

            while (current != null && current != parent)
            {
                hierarchy.Add(current);
                current = current.parent;
            }

            hierarchy.Reverse();
            return hierarchy.ToArray();
        }

        public static Transform SyncHierarchy(Transform sourceParent, Transform sourceChild, Transform destinationParent, Dictionary<Transform, Transform> boneMap)
        {
            if (sourceParent == null)
            {
                throw new ArgumentNullException(nameof(sourceParent), "Source parent cannot be null");
            }

            if (sourceChild == null)
            {
                throw new ArgumentNullException(nameof(sourceChild), "Source child cannot be null");
            }

            if (!sourceChild.IsChildOf(sourceParent))
            {
                throw new ArgumentException("Source child must be a descendant of the source parent", nameof(sourceChild));
            }

            if (destinationParent == null)
            {
                throw new ArgumentNullException(nameof(destinationParent), "Destination parent cannot be null");
            }

            if (boneMap == null)
            {
                throw new ArgumentNullException(nameof(boneMap), "Bone map cannot be null");
            }

            var hierarchy = GetHierarchyBetween(sourceParent, sourceChild);

            var currentDestination = destinationParent;

            foreach (var transform in hierarchy)
            {
                if (boneMap.TryGetValue(transform, out var mappedBone))
                {
                    currentDestination = mappedBone;
                }
                else
                {
                    var newChild = new GameObject(transform.name).transform;
                    newChild.SetParent(currentDestination, false);
                    newChild.localPosition = transform.localPosition;
                    newChild.localRotation = transform.localRotation;
                    newChild.localScale = transform.localScale;
                    currentDestination = newChild;

                    boneMap[transform] = currentDestination;
                }
            }

            return currentDestination;
        }

        public static Dictionary<Component, List<Component>> BuildDependencyGraph(Component[] components)
        {
            var graph = new Dictionary<Component, List<Component>>();

            var typeComponentMap = new Dictionary<Type, List<Component>>();

            foreach (Component component in components)
            {
                Type type = component.GetType();

                if (!typeComponentMap.ContainsKey(type))
                {
                    typeComponentMap[type] = new List<Component>();
                }

                typeComponentMap[type].Add(component);
            }

            foreach (Component component in components)
            {
                Type type = component.GetType();

                foreach (RequireComponent attr in type.GetCustomAttributes(typeof(RequireComponent), true))
                {
                    foreach (var requiredType in new Type[] { attr.m_Type0, attr.m_Type1, attr.m_Type2 })
                    {
                        if (requiredType == null)
                        {
                            continue;
                        }

                        if (!typeComponentMap.ContainsKey(requiredType))
                        {
                            continue;
                        }

                        foreach (var requiredComponent in typeComponentMap[requiredType])
                        {
                            if (!graph.ContainsKey(requiredComponent))
                            {
                                graph[requiredComponent] = new List<Component>();
                            }

                            graph[requiredComponent].Add(component);
                        }
                    }
                }
            }

            return graph;
        }

        public static Component[] TopologicalSort(Component[] components, Dictionary<Component, List<Component>> graph)
        {
            var sorted = new List<Component>();
            var visited = new HashSet<Component>();

            foreach (var component in components)
            {
                Visit(component, visited, sorted, graph);
            }

            return sorted.ToArray();
        }

        private static void Visit(Component component, HashSet<Component> visited, List<Component> sorted, Dictionary<Component, List<Component>> graph)
        {
            if (visited.Contains(component))
            {
                return;
            }

            visited.Add(component);

            if (graph.ContainsKey(component))
            {
                foreach (var dependency in graph[component])
                {
                    Visit(dependency, visited, sorted, graph);
                }
            }

            sorted.Add(component);
        }

        public static Mesh BakeBlendShapes(SkinnedMeshRenderer smr, bool preserveZeroBlendShapes)
        {
            if (smr == null)
            {
                throw new ArgumentNullException(nameof(smr), "SkinnedMeshRenderer cannot be null");
            }

            var mesh = UnityEngine.Object.Instantiate(smr.sharedMesh);
            mesh.name = smr.sharedMesh.name;

            var preservedBlendShapes = new List<PreservedBlendShape>();

            if (preserveZeroBlendShapes)
            {
                for (int blendShapeIndex = 0; blendShapeIndex < smr.sharedMesh.blendShapeCount; ++blendShapeIndex)
                {
                    var weight = smr.GetBlendShapeWeight(blendShapeIndex);

                    if (weight == 0.0f)
                    {
                        string name = smr.sharedMesh.GetBlendShapeName(blendShapeIndex);
                        int frameCount = smr.sharedMesh.GetBlendShapeFrameCount(blendShapeIndex);
                        float[] frameWeights = new float[frameCount];

                        for (int frameIndex = 0; frameIndex < frameCount; ++frameIndex)
                        {
                            frameWeights[frameIndex] = smr.sharedMesh.GetBlendShapeFrameWeight(blendShapeIndex, frameIndex);
                        }

                        preservedBlendShapes.Add(new PreservedBlendShape
                        {
                            name = name,
                            frameWeights = frameWeights,
                            weight = weight
                        });
                    }
                }
            }

            mesh.ClearBlendShapes();

            var newVertices = (Vector3[])mesh.vertices.Clone();
            var newNormals = (Vector3[])mesh.normals.Clone();
            var newTangents = (Vector4[])mesh.tangents.Clone();

            for (int blendShapeIndex = 0; blendShapeIndex < smr.sharedMesh.blendShapeCount; ++blendShapeIndex)
            {
                var weight = smr.GetBlendShapeWeight(blendShapeIndex);

                if (weight == 0.0f)
                {
                    continue;
                }

                var frameCount = smr.sharedMesh.GetBlendShapeFrameCount(blendShapeIndex);

                if (frameCount == 0)
                {
                    continue;
                }

                var upperFrameIndex = frameCount;

                for (int frameIndex = 0; frameIndex < frameCount; ++frameIndex)
                {
                    if (smr.sharedMesh.GetBlendShapeFrameWeight(blendShapeIndex, frameIndex) >= weight)
                    {
                        upperFrameIndex = frameIndex;
                        break;
                    }
                }

                var blendedVertices = new Vector3[mesh.vertexCount];
                var blendedNormals = new Vector3[mesh.vertexCount];
                var blendedTangents = new Vector3[mesh.vertexCount];

                if (upperFrameIndex == 0)
                {
                    var vertices = new Vector3[mesh.vertexCount];
                    var normals = new Vector3[mesh.vertexCount];
                    var tangents = new Vector3[mesh.vertexCount];

                    var frameWeight = smr.sharedMesh.GetBlendShapeFrameWeight(blendShapeIndex, 0);
                    smr.sharedMesh.GetBlendShapeFrameVertices(blendShapeIndex, 0, vertices, normals, tangents);

                    for (int i = 0; i < mesh.vertexCount; ++i)
                    {
                        blendedVertices[i] = vertices[i] * (weight / frameWeight);
                        blendedNormals[i] = normals[i] * (weight / frameWeight);
                        blendedTangents[i] = tangents[i] * (weight / frameWeight);
                    }
                }
                else if (upperFrameIndex == frameCount)
                {
                    var vertices = new Vector3[mesh.vertexCount];
                    var normals = new Vector3[mesh.vertexCount];
                    var tangents = new Vector3[mesh.vertexCount];

                    var frameWeight = smr.sharedMesh.GetBlendShapeFrameWeight(blendShapeIndex, frameCount - 1);
                    smr.sharedMesh.GetBlendShapeFrameVertices(blendShapeIndex, frameCount - 1, vertices, normals, tangents);

                    for (int i = 0; i < mesh.vertexCount; ++i)
                    {
                        blendedVertices[i] = vertices[i] * (weight / frameWeight);
                        blendedNormals[i] = normals[i] * (weight / frameWeight);
                        blendedTangents[i] = tangents[i] * (weight / frameWeight);
                    }
                }
                else
                {
                    var lowerFrameWeight = smr.sharedMesh.GetBlendShapeFrameWeight(blendShapeIndex, upperFrameIndex - 1);
                    var upperFrameWeight = smr.sharedMesh.GetBlendShapeFrameWeight(blendShapeIndex, upperFrameIndex);

                    var lowerVertices = new Vector3[mesh.vertexCount];
                    var lowerNormals = new Vector3[mesh.vertexCount];
                    var lowerTangents = new Vector3[mesh.vertexCount];

                    var upperVertices = new Vector3[mesh.vertexCount];
                    var upperNormals = new Vector3[mesh.vertexCount];
                    var upperTangents = new Vector3[mesh.vertexCount];

                    smr.sharedMesh.GetBlendShapeFrameVertices(blendShapeIndex, upperFrameIndex - 1, lowerVertices, lowerNormals, lowerTangents);
                    smr.sharedMesh.GetBlendShapeFrameVertices(blendShapeIndex, upperFrameIndex, upperVertices, upperNormals, upperTangents);

                    for (int i = 0; i < mesh.vertexCount; ++i)
                    {
                        blendedVertices[i] = Vector3.Lerp(lowerVertices[i], upperVertices[i], (weight - lowerFrameWeight) / (upperFrameWeight - lowerFrameWeight));
                        blendedNormals[i] = Vector3.Lerp(lowerNormals[i], upperNormals[i], (weight - lowerFrameWeight) / (upperFrameWeight - lowerFrameWeight));
                        blendedTangents[i] = Vector3.Lerp(lowerTangents[i], upperTangents[i], (weight - lowerFrameWeight) / (upperFrameWeight - lowerFrameWeight));
                    }
                }

                for (int i = 0; i < mesh.vertexCount; ++i)
                {
                    if (i < blendedVertices.Length)
                    {
                        newVertices[i] += blendedVertices[i];
                    }

                    if (i < blendedNormals.Length)
                    {
                        newNormals[i] += blendedNormals[i];
                    }

                    if (i < newTangents.Length)
                    {
                        newTangents[i] += new Vector4(blendedTangents[i].x, blendedTangents[i].y, blendedTangents[i].z, 0.0f);
                    }
                }
            }

            mesh.vertices = newVertices;
            mesh.normals = newNormals.Select(n => n.normalized).ToArray();
            mesh.tangents = newTangents;

            if (preserveZeroBlendShapes)
            {
                foreach (var preservedShape in preservedBlendShapes)
                {
                    for (int frameIndex = 0; frameIndex < preservedShape.frameWeights.Length; ++frameIndex)
                    {
                        var deltaVertices = new Vector3[mesh.vertexCount];
                        var deltaNormals = new Vector3[mesh.vertexCount];
                        var deltaTangents = new Vector3[mesh.vertexCount];

                        int originalIndex = smr.sharedMesh.GetBlendShapeIndex(preservedShape.name);
                        smr.sharedMesh.GetBlendShapeFrameVertices(originalIndex, frameIndex, deltaVertices, deltaNormals, deltaTangents);

                        mesh.AddBlendShapeFrame(preservedShape.name, preservedShape.frameWeights[frameIndex], deltaVertices, deltaNormals, deltaTangents);
                    }
                }
            }

            return mesh;
        }

        public static void TransferBlendShapes(Mesh originalMesh, Mesh destinationMesh)
        {
            Matrix4x4[] originalBindposes = originalMesh.bindposes;

            Matrix4x4[] destinationBindposeInverses = destinationMesh.bindposes
                .Select(bindpose => bindpose.inverse)
                .ToArray();

            Vector3[] originalDeltaVertices = new Vector3[originalMesh.vertexCount];
            Vector3[] originalDeltaNormals = new Vector3[originalMesh.vertexCount];
            Vector3[] originalDeltaTangents = new Vector3[originalMesh.vertexCount];

            Vector3[] newDeltaVertices = new Vector3[destinationMesh.vertexCount];
            Vector3[] newDeltaNormals = new Vector3[destinationMesh.vertexCount];
            Vector3[] newDeltaTangents = new Vector3[destinationMesh.vertexCount];

            for (int blendShapeIndex = 0; blendShapeIndex < originalMesh.blendShapeCount; ++blendShapeIndex)
            {
                string blendShapeName = originalMesh.GetBlendShapeName(blendShapeIndex);
                int frameCount = originalMesh.GetBlendShapeFrameCount(blendShapeIndex);

                for (int frameIndex = 0; frameIndex < frameCount; ++frameIndex)
                {
                    float frameWeight = originalMesh.GetBlendShapeFrameWeight(blendShapeIndex, frameIndex);
                    originalMesh.GetBlendShapeFrameVertices(blendShapeIndex, frameIndex, originalDeltaVertices, originalDeltaNormals, originalDeltaTangents);

                    for (int vertexIndex = 0; vertexIndex < originalMesh.vertexCount; ++vertexIndex)
                    {
                        BoneWeight bw = originalMesh.boneWeights[vertexIndex];

                        Vector3 originalLocalDelta = ProcessBoneWeightVector(bw, originalDeltaVertices[vertexIndex], originalBindposes);
                        Vector3 originalLocalNormalDelta = ProcessBoneWeightVector(bw, originalDeltaNormals[vertexIndex], originalBindposes);
                        Vector3 originalLocalTangentDelta = ProcessBoneWeightVector(bw, originalDeltaTangents[vertexIndex], originalBindposes);

                        BoneWeight newBw = destinationMesh.boneWeights[vertexIndex];

                        Vector3 newDelta = Vector3.zero;
                        Vector3 newNormalDelta = Vector3.zero;
                        Vector3 newTangentDelta = Vector3.zero;

                        if (newBw.weight0 > 0 && newBw.boneIndex0 < destinationMesh.bindposes.Length)
                        {
                            newDelta += newBw.weight0 * destinationBindposeInverses[newBw.boneIndex0].MultiplyVector(originalLocalDelta);
                            newNormalDelta += newBw.weight0 * destinationMesh.bindposes[newBw.boneIndex0].MultiplyVector(originalLocalNormalDelta);
                            newTangentDelta += newBw.weight0 * destinationMesh.bindposes[newBw.boneIndex0].MultiplyVector(originalLocalTangentDelta);
                        }

                        if (newBw.weight1 > 0 && newBw.boneIndex1 < destinationMesh.bindposes.Length)
                        {
                            newDelta += newBw.weight1 * destinationBindposeInverses[newBw.boneIndex1].MultiplyVector(originalLocalDelta);
                            newNormalDelta += newBw.weight1 * destinationMesh.bindposes[newBw.boneIndex1].MultiplyVector(originalLocalNormalDelta);
                            newTangentDelta += newBw.weight1 * destinationMesh.bindposes[newBw.boneIndex1].MultiplyVector(originalLocalTangentDelta);
                        }

                        if (newBw.weight2 > 0 && newBw.boneIndex2 < destinationMesh.bindposes.Length)
                        {
                            newDelta += newBw.weight2 * destinationBindposeInverses[newBw.boneIndex2].MultiplyVector(originalLocalDelta);
                            newNormalDelta += newBw.weight2 * destinationMesh.bindposes[newBw.boneIndex2].MultiplyVector(originalLocalNormalDelta);
                            newTangentDelta += newBw.weight2 * destinationMesh.bindposes[newBw.boneIndex2].MultiplyVector(originalLocalTangentDelta);
                        }

                        if (newBw.weight3 > 0 && newBw.boneIndex3 < destinationMesh.bindposes.Length)
                        {
                            newDelta += newBw.weight3 * destinationBindposeInverses[newBw.boneIndex3].MultiplyVector(originalLocalDelta);
                            newNormalDelta += newBw.weight3 * destinationMesh.bindposes[newBw.boneIndex3].MultiplyVector(originalLocalNormalDelta);
                            newTangentDelta += newBw.weight3 * destinationMesh.bindposes[newBw.boneIndex3].MultiplyVector(originalLocalTangentDelta);
                        }

                        newDeltaVertices[vertexIndex] = newDelta;
                        newDeltaNormals[vertexIndex] = newNormalDelta;
                        newDeltaTangents[vertexIndex] = newTangentDelta;
                    }

                    destinationMesh.AddBlendShapeFrame(blendShapeName, frameWeight, newDeltaVertices, newDeltaNormals, newDeltaTangents);
                }
            }
        }

        public static void AddOriginalMeshAsBlendShape(Mesh originalMesh, Mesh destinationMesh, Matrix4x4[] originalPoses)
        {
            if (originalMesh.vertexCount != destinationMesh.vertexCount)
            {
                throw new ArgumentException("Original mesh and destination mesh must have the same number of vertices.");
            }

            Vector3[] originalVertices = originalMesh.vertices;
            Vector3[] destinationVertices = destinationMesh.vertices;
            Vector3[] deltaVertices = new Vector3[originalMesh.vertexCount];

            Vector3[] originalNormals = originalMesh.normals;
            Vector3[] destinationNormals = destinationMesh.normals;
            Vector3[] deltaNormals = new Vector3[originalMesh.vertexCount];

            Vector3[] originalTangents = originalMesh.tangents.Select(t => new Vector3(t.x, t.y, t.z)).ToArray();
            Vector3[] destinationTangents = destinationMesh.tangents.Select(t => new Vector3(t.x, t.y, t.z)).ToArray();
            Vector3[] deltaTangents = new Vector3[originalMesh.vertexCount];

            Matrix4x4[] originalBindposes = originalMesh.bindposes;

            for (int i = 0; i < originalMesh.vertexCount; ++i)
            {
                BoneWeight originalBw = originalMesh.boneWeights[i];
                Vector3 originalLocalVertex = Vector3.zero;
                Vector3 originalLocalNormal = Vector3.zero;
                Vector3 originalLocalTangent = Vector3.zero;

                if (originalBw.weight0 > 0 && originalBw.boneIndex0 < originalBindposes.Length && originalBw.boneIndex0 < originalPoses.Length)
                {
                    Matrix4x4 boneToWorld = originalPoses[originalBw.boneIndex0];
                    Matrix4x4 worldToBone = originalBindposes[originalBw.boneIndex0];
                    Matrix4x4 transform = boneToWorld * worldToBone;

                    originalLocalVertex += originalBw.weight0 * transform.MultiplyPoint3x4(originalVertices[i]);
                    originalLocalNormal += originalBw.weight0 * transform.MultiplyVector(originalNormals[i]);
                    originalLocalTangent += originalBw.weight0 * transform.MultiplyVector(originalTangents[i]);
                }
                if (originalBw.weight1 > 0 && originalBw.boneIndex1 < originalBindposes.Length && originalBw.boneIndex1 < originalPoses.Length)
                {
                    Matrix4x4 boneToWorld = originalPoses[originalBw.boneIndex1];
                    Matrix4x4 worldToBone = originalBindposes[originalBw.boneIndex1];
                    Matrix4x4 transform = boneToWorld * worldToBone;

                    originalLocalVertex += originalBw.weight1 * transform.MultiplyPoint3x4(originalVertices[i]);
                    originalLocalNormal += originalBw.weight1 * transform.MultiplyVector(originalNormals[i]);
                    originalLocalTangent += originalBw.weight1 * transform.MultiplyVector(originalTangents[i]);
                }
                if (originalBw.weight2 > 0 && originalBw.boneIndex2 < originalBindposes.Length && originalBw.boneIndex2 < originalPoses.Length)
                {
                    Matrix4x4 boneToWorld = originalPoses[originalBw.boneIndex2];
                    Matrix4x4 worldToBone = originalBindposes[originalBw.boneIndex2];
                    Matrix4x4 transform = boneToWorld * worldToBone;

                    originalLocalVertex += originalBw.weight2 * transform.MultiplyPoint3x4(originalVertices[i]);
                    originalLocalNormal += originalBw.weight2 * transform.MultiplyVector(originalNormals[i]);
                    originalLocalTangent += originalBw.weight2 * transform.MultiplyVector(originalTangents[i]);
                }
                if (originalBw.weight3 > 0 && originalBw.boneIndex3 < originalBindposes.Length && originalBw.boneIndex3 < originalPoses.Length)
                {
                    Matrix4x4 boneToWorld = originalPoses[originalBw.boneIndex3];
                    Matrix4x4 worldToBone = originalBindposes[originalBw.boneIndex3];
                    Matrix4x4 transform = boneToWorld * worldToBone;

                    originalLocalVertex += originalBw.weight3 * transform.MultiplyPoint3x4(originalVertices[i]);
                    originalLocalNormal += originalBw.weight3 * transform.MultiplyVector(originalNormals[i]);
                    originalLocalTangent += originalBw.weight3 * transform.MultiplyVector(originalTangents[i]);
                }

                deltaVertices[i] = originalLocalVertex - destinationVertices[i];
                deltaNormals[i] = originalLocalNormal - destinationNormals[i];
                deltaTangents[i] = originalLocalTangent - destinationTangents[i];
            }

            destinationMesh.AddBlendShapeFrame("Original", 100.0f, deltaVertices, deltaNormals, deltaTangents);
        }

        public static Vector3 ProcessBoneWeightVector(BoneWeight bw, Vector3 vector, Matrix4x4[] bindposes)
        {
            Vector3 result = Vector3.zero;

            if (bw.weight0 > 0 && bw.boneIndex0 < bindposes.Length)
            {
                result += bw.weight0 * bindposes[bw.boneIndex0].MultiplyVector(vector);
            }

            if (bw.weight1 > 0 && bw.boneIndex1 < bindposes.Length)
            {
                result += bw.weight1 * bindposes[bw.boneIndex1].MultiplyVector(vector);
            }

            if (bw.weight2 > 0 && bw.boneIndex2 < bindposes.Length)
            {
                result += bw.weight2 * bindposes[bw.boneIndex2].MultiplyVector(vector);
            }

            if (bw.weight3 > 0 && bw.boneIndex3 < bindposes.Length)
            {
                result += bw.weight3 * bindposes[bw.boneIndex3].MultiplyVector(vector);
            }

            return result;
        }

        public static string GetScriptRelativePath(Type scriptType, params string[] paths)
        {
            MonoScript script = null;

            foreach (var s in Resources.FindObjectsOfTypeAll<MonoScript>())
            {
                if (s.GetClass() == scriptType)
                {
                    script = s;
                    break;
                }
            }

            var scriptPath = AssetDatabase.GetAssetPath(script);
            var result = Path.GetDirectoryName(scriptPath);

            foreach (var path in paths)
            {
                if (path == ".")
                {
                }
                else if (path == "..")
                {
                    result = Path.GetDirectoryName(result);
                }
                else
                {
                    result = Path.Combine(result, path);
                }
            }

            return result.Replace("\\", "/");
        }
    }
}

#endif
