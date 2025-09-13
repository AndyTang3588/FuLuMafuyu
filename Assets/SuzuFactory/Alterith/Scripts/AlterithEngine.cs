#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

#if MODULAR_AVATAR
using nadena.dev.modular_avatar.core;
#endif

using UnityEngine;

namespace SuzuFactory.Alterith
{
    public class AlterithEngine : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct ALTERITH_Vector2f
        {
            public float x;
            public float y;

            public ALTERITH_Vector2f(Vector2 v)
            {
                x = v.x;
                y = v.y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ALTERITH_Vector3f
        {
            public float x;
            public float y;
            public float z;

            public ALTERITH_Vector3f(Vector3 v)
            {
                x = v.x;
                y = v.y;
                z = v.z;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ALTERITH_Vector4f
        {
            public float x;
            public float y;
            public float z;
            public float w;

            public ALTERITH_Vector4f(Vector4 v)
            {
                x = v.x;
                y = v.y;
                z = v.z;
                w = v.w;
            }

            public ALTERITH_Vector4f(Color c)
            {
                x = c.r;
                y = c.g;
                z = c.b;
                w = c.a;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ALTERITH_Matrix4f
        {
            public float m00, m10, m20, m30;
            public float m01, m11, m21, m31;
            public float m02, m12, m22, m32;
            public float m03, m13, m23, m33;

            public ALTERITH_Matrix4f(Matrix4x4 m)
            {
                m00 = m.m00; m01 = m.m01; m02 = m.m02; m03 = m.m03;
                m10 = m.m10; m11 = m.m11; m12 = m.m12; m13 = m.m13;
                m20 = m.m20; m21 = m.m21; m22 = m.m22; m23 = m.m23;
                m30 = m.m30; m31 = m.m31; m32 = m.m32; m33 = m.m33;
            }

            public Matrix4x4 Matrix
            {
                get
                {
                    return new Matrix4x4(
                        new Vector4(m00, m10, m20, m30),
                        new Vector4(m01, m11, m21, m31),
                        new Vector4(m02, m12, m22, m32),
                        new Vector4(m03, m13, m23, m33)
                    );
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ALTERITH_BoneWeight_t
        {
            public uint bone_index;
            public float weight;

            public ALTERITH_BoneWeight_t(int index, float w)
            {
                bone_index = (uint)index;
                weight = w;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ALTERITH_Vertex_t
        {
            public ALTERITH_Vector3f position;
            public ALTERITH_Vector3f normal;
            public ALTERITH_Vector4f tangent;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public ALTERITH_BoneWeight_t[] bone_weights;
            public float apply_fitting;
            public float fitting_minimum_margin;
            public float fitting_margin_scale;
            public float transfer_bone_weights_affected_by;
            public float transfer_bone_weights_affects;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ALTERITH_SubMesh_t
        {
            public uint num_indices;
            public IntPtr indices;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ALTERITH_Mesh_t
        {
            public uint num_vertices;
            public IntPtr vertices;
            public uint num_submeshes;
            public IntPtr submeshes;
            public uint num_bindposes;
            public IntPtr bindposes;
        }

        private enum ALTERITH_BoneType_t : uint
        {
            Other,
            LeftShoulder,
            RightShoulder,
            LeftUpperArm,
            RightUpperArm,
            LeftLowerArm,
            RightLowerArm,
            LeftHand,
            RightHand,
            LeftUpperLeg,
            RightUpperLeg,
            LeftLowerLeg,
            RightLowerLeg,
            LeftFoot,
            RightFoot,
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ALTERITH_Bone_t
        {
            public ALTERITH_BoneType_t type;
            public ALTERITH_Matrix4f pose;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ALTERITH_Avatar_t
        {
            public uint num_bones;
            public IntPtr bones;
            public uint left_hand_bone_index;
            public uint right_hand_bone_index;
            public uint left_upper_leg_bone_index;
            public uint right_upper_leg_bone_index;
            public uint left_foot_bone_index;
            public uint right_foot_bone_index;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ALTERITH_Engine_Init_o_t
        {
            public IntPtr log_callback;
            public IntPtr log_callback_user_data;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ALTERITH_ClothingSettings_t
        {
            public bool excluded;
            public bool transfer_bone_weights;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ALTERITH_Engine_Start_o_t
        {
            public ALTERITH_Avatar_t source_avatar;
            public ALTERITH_Avatar_t destination_avatar;
            public uint num_source_avatar_body_meshes;
            public IntPtr source_avatar_body_meshes;
            public uint num_destination_avatar_body_meshes;
            public IntPtr destination_avatar_body_meshes;
            public uint num_clothing_meshes;
            public IntPtr source_clothing_meshes;
            public IntPtr destination_clothing_meshes;
            public IntPtr clothing_settings;
            public uint fitting_num_iterations;
            public float fitting_range;
            public uint fitting_num_nearest_neighbors;
            public uint transfer_bone_weights_num_iterations;
            public float transfer_bone_weights_distance;
            public float transfer_bone_weights_width;
            public uint transfer_bone_weights_num_samples;
            public uint transfer_bone_weights_num_nearest_neighbors;
            public float smooth_ratio;
            public uint smooth_num_samples;
            public uint smooth_num_nearest_neighbors;
            public bool separate_left_right;
            public float bone_position_threshold;
            public bool ignore_hand_shape;
            public bool ignore_foot_shape;
            public float reference_arm_distance;
            public float reference_leg_angle;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ALTERITH_Engine_GetProgress_r_t
        {
            public double progress_ratio;
            public bool completed;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ALTERITH_Engine_GetResult_o_t
        {
            public uint num_clothing_meshes;
            public IntPtr clothing_meshes;
        }

        private struct MergedAvatarData
        {
            public ALTERITH_Avatar_t AvatarData;
            public Dictionary<Transform, int> BoneIndexMap;
            public Transform[] MergedBones;

            public MergedAvatarData(ALTERITH_Avatar_t avatarData, Dictionary<Transform, int> boneIndexMap, Transform[] mergedBones)
            {
                AvatarData = avatarData;
                BoneIndexMap = boneIndexMap;
                MergedBones = mergedBones;
            }
        }

        public struct ResultData
        {
            public Mesh Mesh;
            public Transform[] Bones;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void LogCallbackDelegate(IntPtr userData, IntPtr message);

        [DllImport("AlterithEngine", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr ALTERITH_Engine_Init(ref ALTERITH_Engine_Init_o_t options);

        [DllImport("AlterithEngine", CallingConvention = CallingConvention.StdCall)]
        private static extern void ALTERITH_Engine_Free(IntPtr engine);

        [DllImport("AlterithEngine", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr ALTERITH_Engine_GetLastError(IntPtr engine);

        [DllImport("AlterithEngine", CallingConvention = CallingConvention.StdCall)]
        private static extern bool ALTERITH_Engine_Start(IntPtr engine, ref ALTERITH_Engine_Start_o_t options);

        [DllImport("AlterithEngine", CallingConvention = CallingConvention.StdCall)]
        private static extern bool ALTERITH_Engine_GetProgress(IntPtr engine, ref ALTERITH_Engine_GetProgress_r_t result);

        [DllImport("AlterithEngine", CallingConvention = CallingConvention.StdCall)]
        private static extern bool ALTERITH_Engine_GetResult(IntPtr engine, ref ALTERITH_Engine_GetResult_o_t options);

        private Transform sourceAvatar;
        private Transform destinationAvatar;
        private SkinnedMeshRenderer[] sourceAvatarBodies;
        private SkinnedMeshRenderer[] destinationAvatarBodies;
        private ClothingSMRTuple[] clothingSMRTuples;
        private Settings settings;

        private IntPtr engineHandle;
        private bool disposed = false;
        private LogCallbackDelegate logCallback;
        private GCHandle logCallbackHandle;
        private GCHandle userDataHandle;
        private List<GCHandle> allocatedHandles = new List<GCHandle>();

        private bool startCalled = false;
        private bool getResultCalled = false;

        private Transform[] mergedBones;
        private SkinnedMeshRenderer[] destinationClothingMeshRenderers;

#if MODULAR_AVATAR
        private Dictionary<ModularAvatarMergeArmature, Dictionary<Transform, Transform>> boneMapCache = new Dictionary<ModularAvatarMergeArmature, Dictionary<Transform, Transform>>();
#endif

        public AlterithEngine(
            Transform sourceAvatar,
            Transform destinationAvatar,
            SkinnedMeshRenderer[] sourceAvatarBodies,
            SkinnedMeshRenderer[] destinationAvatarBodies,
            ClothingSMRTuple[] clothingSMRTuples,
            Settings settings)
        {
            this.sourceAvatar = sourceAvatar;
            this.destinationAvatar = destinationAvatar;
            this.sourceAvatarBodies = sourceAvatarBodies;
            this.destinationAvatarBodies = destinationAvatarBodies;
            this.clothingSMRTuples = clothingSMRTuples;
            this.settings = settings;

            logCallback = new LogCallbackDelegate(OnLogMessage);
            logCallbackHandle = GCHandle.Alloc(logCallback);

            userDataHandle = GCHandle.Alloc(this);

            var options = new ALTERITH_Engine_Init_o_t
            {
                log_callback = Marshal.GetFunctionPointerForDelegate(logCallback),
                log_callback_user_data = GCHandle.ToIntPtr(userDataHandle)
            };

            engineHandle = ALTERITH_Engine_Init(ref options);

            if (engineHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to initialize Alterith Engine");
            }

            Start();
        }

        ~AlterithEngine()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string GetLastError()
        {
            CheckDisposed();
            IntPtr errorPtr = ALTERITH_Engine_GetLastError(engineHandle);

            if (errorPtr != IntPtr.Zero)
            {
                return Marshal.PtrToStringBSTR(errorPtr);
            }

            return null;
        }

        public struct Settings
        {
            public uint FittingNumIterations;
            public float FittingRange;
            public uint FittingNumNearestNeighbors;
            public float MinimumMargin;
            public float MarginScale;
            public uint TransferBoneWeightsNumIterations;
            public float TransferBoneWeightsDistance;
            public float TransferBoneWeightsWidth;
            public uint TransferBoneWeightsNumSamples;
            public uint TransferBoneWeightsNumNearestNeighbors;
            public float SmoothRatio;
            public uint SmoothNumSamples;
            public uint SmoothNumNearestNeighbors;
            public bool SeparateLeftRight;
            public float BonePositionThreshold;
            public bool IgnoreHandShape;
            public bool IgnoreFootShape;
            public float ReferenceArmDistance;
            public float ReferenceLegAngle;
            public bool PreserveZeroBlendShapes;
            public bool AddOriginalMeshAsBlendShape;
        }

        private void Start()
        {
            CheckDisposed();

            if (startCalled)
            {
                throw new InvalidOperationException("Start method has already been called. Create a new AlterithEngine instance for another operation.");
            }

            var sourceRenderers = sourceAvatarBodies.Concat(clothingSMRTuples.Select(pair => pair.Source)).ToArray();
            var destinationRenderers = destinationAvatarBodies.Concat(clothingSMRTuples.Select(pair => pair.DestinationConverted)).ToArray();

            var sourceMergedData = PrepareAvatarWithMergedBones(sourceAvatar, sourceRenderers);
            var destinationMergedData = PrepareAvatarWithMergedBones(destinationAvatar, destinationRenderers);

            var sourceBodyMeshData = sourceAvatarBodies.Select(body => PrepareMeshWithRemappedBoneIndices(body, sourceMergedData)).ToArray();
            var destinationBodyMeshData = destinationAvatarBodies.Select(body => PrepareMeshWithRemappedBoneIndices(body, destinationMergedData)).ToArray();

            var sourceClothingMeshesData = new ALTERITH_Mesh_t[clothingSMRTuples.Length];
            var destinationClothingMeshesData = new ALTERITH_Mesh_t[clothingSMRTuples.Length];

            for (int i = 0; i < clothingSMRTuples.Length; i++)
            {
                sourceClothingMeshesData[i] = PrepareMeshWithRemappedBoneIndices(clothingSMRTuples[i].Source, sourceMergedData);
                destinationClothingMeshesData[i] = PrepareMeshWithRemappedBoneIndices(clothingSMRTuples[i].DestinationConverted, destinationMergedData);
            }

            var sourceClothingMeshesPtr = AllocateAndMarshalArray(sourceClothingMeshesData);
            var destinationClothingMeshesPtr = AllocateAndMarshalArray(destinationClothingMeshesData);
            var sourceBodyMeshes = AllocateAndMarshalArray(sourceBodyMeshData);
            var destinationBodyMeshes = AllocateAndMarshalArray(destinationBodyMeshData);

            var clothingSettings = clothingSMRTuples.Select(tuple => new ALTERITH_ClothingSettings_t
            {
                excluded = tuple.Excluded,
                transfer_bone_weights = tuple.TransferBoneWeights,
            }).ToArray();

            var clothingSettingsPtr = AllocateAndMarshalArray(clothingSettings);

            var options = new ALTERITH_Engine_Start_o_t
            {
                source_avatar = sourceMergedData.AvatarData,
                destination_avatar = destinationMergedData.AvatarData,
                num_source_avatar_body_meshes = (uint)sourceBodyMeshData.Length,
                source_avatar_body_meshes = sourceBodyMeshes,
                num_destination_avatar_body_meshes = (uint)destinationBodyMeshData.Length,
                destination_avatar_body_meshes = destinationBodyMeshes,
                num_clothing_meshes = (uint)clothingSMRTuples.Length,
                source_clothing_meshes = sourceClothingMeshesPtr,
                destination_clothing_meshes = destinationClothingMeshesPtr,
                clothing_settings = clothingSettingsPtr,
                fitting_num_iterations = settings.FittingNumIterations,
                fitting_range = settings.FittingRange,
                fitting_num_nearest_neighbors = settings.FittingNumNearestNeighbors,
                transfer_bone_weights_num_iterations = (uint)settings.TransferBoneWeightsNumIterations,
                transfer_bone_weights_distance = settings.TransferBoneWeightsDistance,
                transfer_bone_weights_width = settings.TransferBoneWeightsWidth,
                transfer_bone_weights_num_samples = settings.TransferBoneWeightsNumSamples,
                transfer_bone_weights_num_nearest_neighbors = settings.TransferBoneWeightsNumNearestNeighbors,
                smooth_ratio = settings.SmoothRatio,
                smooth_num_samples = settings.SmoothNumSamples,
                smooth_num_nearest_neighbors = settings.SmoothNumNearestNeighbors,
                separate_left_right = settings.SeparateLeftRight,
                bone_position_threshold = settings.BonePositionThreshold,
                ignore_hand_shape = settings.IgnoreHandShape,
                ignore_foot_shape = settings.IgnoreFootShape,
                reference_arm_distance = settings.ReferenceArmDistance,
                reference_leg_angle = settings.ReferenceLegAngle * Mathf.Deg2Rad,
            };

            bool success = ALTERITH_Engine_Start(engineHandle, ref options);

            if (!success)
            {
                throw new InvalidOperationException("Failed to start Alterith Engine: " + GetLastError());
            }

            this.destinationClothingMeshRenderers = clothingSMRTuples.Select(pair => pair.DestinationConverted).ToArray();
            mergedBones = destinationMergedData.MergedBones;
            startCalled = true;
        }

        public ResultData[] GetResult()
        {
            CheckDisposed();

            if (!startCalled)
            {
                throw new InvalidOperationException("Start method must be called before GetResult.");
            }

            if (getResultCalled)
            {
                throw new InvalidOperationException("GetResult method has already been called. Create a new AlterithEngine instance for another operation.");
            }

            if (destinationClothingMeshRenderers.Length == 0)
            {
                return new ResultData[0];
            }

            var resultOptions = new ALTERITH_Engine_GetResult_o_t
            {
                num_clothing_meshes = (uint)destinationClothingMeshRenderers.Length,
                clothing_meshes = IntPtr.Zero
            };

            var meshDataArray = new ALTERITH_Mesh_t[destinationClothingMeshRenderers.Length];

            for (int i = 0; i < destinationClothingMeshRenderers.Length; i++)
            {
                Mesh originalMesh = destinationClothingMeshRenderers[i].sharedMesh;

                ALTERITH_SubMesh_t[] submeshes = new ALTERITH_SubMesh_t[originalMesh.subMeshCount];

                for (int j = 0; j < originalMesh.subMeshCount; j++)
                {
                    int[] indices = originalMesh.GetTriangles(j);

                    submeshes[j] = new ALTERITH_SubMesh_t
                    {
                        num_indices = (uint)indices.Length,
                        indices = AllocateAndMarshalIntegers(indices)
                    };
                }

                IntPtr verticesPtr = AllocateUnmanagedMemory(Marshal.SizeOf<ALTERITH_Vertex_t>() * originalMesh.vertexCount);
                IntPtr submeshesPtr = AllocateAndMarshalArray(submeshes);
                IntPtr bindposesPtr = AllocateUnmanagedMemory(Marshal.SizeOf<ALTERITH_Matrix4f>() * mergedBones.Length);

                meshDataArray[i] = new ALTERITH_Mesh_t
                {
                    num_vertices = (uint)originalMesh.vertexCount,
                    vertices = verticesPtr,
                    num_submeshes = (uint)originalMesh.subMeshCount,
                    submeshes = submeshesPtr,
                    num_bindposes = (uint)mergedBones.Length,
                    bindposes = bindposesPtr
                };
            }

            resultOptions.clothing_meshes = AllocateAndMarshalArray(meshDataArray);

            bool success = ALTERITH_Engine_GetResult(engineHandle, ref resultOptions);

            if (!success)
            {
                throw new InvalidOperationException("Failed to get result from Alterith Engine: " + GetLastError());
            }

            ResultData[] resultData = new ResultData[destinationClothingMeshRenderers.Length];

            for (int i = 0; i < destinationClothingMeshRenderers.Length; i++)
            {
                IntPtr meshPtr = IntPtr.Add(resultOptions.clothing_meshes, i * Marshal.SizeOf<ALTERITH_Mesh_t>());
                ALTERITH_Mesh_t meshData = Marshal.PtrToStructure<ALTERITH_Mesh_t>(meshPtr);

                var smr = destinationClothingMeshRenderers[i];
                var bakedMesh = AlterithUtil.BakeBlendShapes(smr, settings.PreserveZeroBlendShapes);

                var originalPoses = smr.bones.Select(bone =>
                {
                    if (bone == null)
                    {
                        return Matrix4x4.identity;
                    }

                    var m = bone.localToWorldMatrix;
                    return destinationAvatar.worldToLocalMatrix * m;
                }).ToArray();

                resultData[i] = CreateUnityMeshFromNativeMesh(meshData, bakedMesh, originalPoses);
            }

            getResultCalled = true;

            return resultData;
        }

        public ProgressInfo GetProgress()
        {
            CheckDisposed();

            var result = new ALTERITH_Engine_GetProgress_r_t();
            bool success = ALTERITH_Engine_GetProgress(engineHandle, ref result);

            if (!success)
            {
                throw new InvalidOperationException("Failed to get progress: " + GetLastError());
            }

            return new ProgressInfo(result.progress_ratio, result.completed);
        }

        public struct ProgressInfo
        {
            public double Progress { get; }
            public bool Completed { get; }

            public ProgressInfo(double progress, bool completed)
            {
                Progress = progress;
                Completed = completed;
            }
        }

        private IntPtr AllocateUnmanagedMemory(int size)
        {
            if (size <= 0)
            {
                return IntPtr.Zero;
            }

            IntPtr ptr = Marshal.AllocHGlobal(size);
            allocatedHandles.Add(GCHandle.Alloc(ptr, GCHandleType.Pinned));
            return ptr;
        }

        private IntPtr AllocateAndMarshalArray<T>(T[] array) where T : struct
        {
            if (array == null || array.Length == 0)
            {
                return IntPtr.Zero;
            }

            IntPtr ptr = AllocateUnmanagedMemory(Marshal.SizeOf<T>() * array.Length);

            for (int i = 0; i < array.Length; ++i)
            {
                IntPtr elementPtr = IntPtr.Add(ptr, i * Marshal.SizeOf<T>());
                Marshal.StructureToPtr(array[i], elementPtr, false);
            }

            return ptr;
        }

        private IntPtr AllocateAndMarshalIntegers(int[] array)
        {
            if (array == null || array.Length == 0)
            {
                return IntPtr.Zero;
            }

            IntPtr ptr = AllocateUnmanagedMemory(array.Length * sizeof(uint));

            for (int i = 0; i < array.Length; ++i)
            {
                Marshal.WriteInt32(IntPtr.Add(ptr, i * sizeof(uint)), array[i]);
            }

            return ptr;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (engineHandle != IntPtr.Zero)
                {
                    ALTERITH_Engine_Free(engineHandle);
                    engineHandle = IntPtr.Zero;
                }

                if (logCallbackHandle.IsAllocated)
                {
                    logCallbackHandle.Free();
                }

                if (userDataHandle.IsAllocated)
                {
                    userDataHandle.Free();
                }

                foreach (var handle in allocatedHandles)
                {
                    if (handle.IsAllocated)
                    {
                        handle.Free();
                    }
                }

                if (readableTextureCache != null)
                {
                    foreach (var texture in readableTextureCache.Values)
                    {
                        if (texture != null)
                        {
                            UnityEngine.Object.DestroyImmediate(texture);
                        }
                    }

                    readableTextureCache.Clear();
                }

                allocatedHandles.Clear();
                disposed = true;
            }
        }

        private void CheckDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(AlterithEngine));
            }
        }

        private void OnLogMessage(IntPtr userData, IntPtr message)
        {
            if (message != IntPtr.Zero)
            {
                string msg = Marshal.PtrToStringBSTR(message);
                Debug.Log($"[Alterith] {msg}");
            }
        }

        private ResultData CreateUnityMeshFromNativeMesh(ALTERITH_Mesh_t nativeMesh, Mesh originalMesh, Matrix4x4[] originalPoses)
        {
            Mesh destinationMesh = UnityEngine.Object.Instantiate(originalMesh);
            destinationMesh.name = originalMesh.name;

            Matrix4x4[] bindposes = new Matrix4x4[(int)nativeMesh.num_bindposes];

            for (int i = 0; i < nativeMesh.num_bindposes; ++i)
            {
                IntPtr bindposePtr = IntPtr.Add(nativeMesh.bindposes, i * Marshal.SizeOf<ALTERITH_Matrix4f>());
                ALTERITH_Matrix4f nativeBindpose = Marshal.PtrToStructure<ALTERITH_Matrix4f>(bindposePtr);
                bindposes[i] = nativeBindpose.Matrix;
            }

            destinationMesh.bindposes = bindposes;

            var vertices = new Vector3[(int)nativeMesh.num_vertices];
            var normals = new Vector3[(int)nativeMesh.num_vertices];
            var tangents = new Vector4[(int)nativeMesh.num_vertices];
            var boneWeights = new BoneWeight[(int)nativeMesh.num_vertices];

            for (int i = 0; i < nativeMesh.num_vertices; ++i)
            {
                IntPtr vertexPtr = IntPtr.Add(nativeMesh.vertices, i * Marshal.SizeOf<ALTERITH_Vertex_t>());
                ALTERITH_Vertex_t vertex = Marshal.PtrToStructure<ALTERITH_Vertex_t>(vertexPtr);

                vertices[i] = new Vector3(vertex.position.x, vertex.position.y, vertex.position.z);
                normals[i] = new Vector3(vertex.normal.x, vertex.normal.y, vertex.normal.z);
                tangents[i] = new Vector4(vertex.tangent.x, vertex.tangent.y, vertex.tangent.z, vertex.tangent.w);

                BoneWeight bw = new BoneWeight
                {
                    boneIndex0 = (int)vertex.bone_weights[0].bone_index,
                    weight0 = vertex.bone_weights[0].weight,
                    boneIndex1 = (int)vertex.bone_weights[1].bone_index,
                    weight1 = vertex.bone_weights[1].weight,
                    boneIndex2 = (int)vertex.bone_weights[2].bone_index,
                    weight2 = vertex.bone_weights[2].weight,
                    boneIndex3 = (int)vertex.bone_weights[3].bone_index,
                    weight3 = vertex.bone_weights[3].weight
                };

                boneWeights[i] = bw;
            }

            destinationMesh.vertices = vertices;

            if (originalMesh.normals != null && originalMesh.normals.Length > 0)
            {
                destinationMesh.normals = normals;
            }
            else
            {
                destinationMesh.RecalculateNormals();
            }

            if (originalMesh.tangents != null && originalMesh.tangents.Length > 0)
            {
                destinationMesh.tangents = tangents;
            }
            else
            {
                destinationMesh.RecalculateTangents();
            }

            destinationMesh.boneWeights = boneWeights;

            destinationMesh.subMeshCount = (int)nativeMesh.num_submeshes;

            for (int i = 0; i < nativeMesh.num_submeshes; ++i)
            {
                IntPtr submeshPtr = IntPtr.Add(nativeMesh.submeshes, i * Marshal.SizeOf<ALTERITH_SubMesh_t>());
                ALTERITH_SubMesh_t submesh = Marshal.PtrToStructure<ALTERITH_SubMesh_t>(submeshPtr);
                int[] indices = new int[(int)submesh.num_indices];

                for (int j = 0; j < submesh.num_indices; ++j)
                {
                    IntPtr indexPtr = IntPtr.Add(submesh.indices, j * sizeof(uint));
                    indices[j] = Marshal.ReadInt32(indexPtr);
                }

                destinationMesh.SetIndices(indices, MeshTopology.Triangles, i);
            }

            destinationMesh.ClearBlendShapes();

            AlterithUtil.TransferBlendShapes(originalMesh, destinationMesh);

            if (settings.AddOriginalMeshAsBlendShape)
            {
                AlterithUtil.AddOriginalMeshAsBlendShape(originalMesh, destinationMesh, originalPoses);
            }

            ResultData result = new ResultData();
            result.Mesh = destinationMesh;
            result.Bones = mergedBones;

            return result;
        }

        private MergedAvatarData PrepareAvatarWithMergedBones(Transform root, SkinnedMeshRenderer[] renderers)
        {
            var animator = root.GetComponent<Animator>();

            if (animator == null)
            {
                throw new InvalidOperationException("The root Transform must have an Animator component.");
            }

            if (animator.avatar == null)
            {
                throw new InvalidOperationException("The Animator component must have a valid Avatar assigned.");
            }

            if (!animator.avatar.isHuman)
            {
                throw new InvalidOperationException("The Avatar must be a humanoid avatar.");
            }

            var leftShoulderBone = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
            var rightShoulderBone = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
            var leftUpperArmBone = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            var rightUpperArmBone = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            var leftLowerArmBone = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            var rightLowerArmBone = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            var leftHandBone = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            var rightHandBone = animator.GetBoneTransform(HumanBodyBones.RightHand);
            var leftUpperLegBone = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            var rightUpperLegBone = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            var leftLowerLegBone = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            var rightLowerLegBone = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            var leftFootBone = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            var rightFootBone = animator.GetBoneTransform(HumanBodyBones.RightFoot);

            var boneIndexMap = new Dictionary<Transform, int>();

            for (var bone = HumanBodyBones.Hips; bone < HumanBodyBones.LastBone; ++bone)
            {
                var boneTransform = animator.GetBoneTransform(bone);

                if (boneTransform == null)
                {
                    continue;
                }

                boneIndexMap.TryAdd(boneTransform, boneIndexMap.Count);
            }

            foreach (var renderer in renderers)
            {
                var bones = renderer.bones;

                for (int i = 0; i < bones.Length; ++i)
                {
                    if (bones[i] == null)
                    {
                        continue;
                    }

                    boneIndexMap.TryAdd(bones[i], boneIndexMap.Count);
                }
            }

            var mergedBones = new Transform[boneIndexMap.Count];

            foreach (var kvp in boneIndexMap)
            {
                mergedBones[kvp.Value] = kvp.Key;
            }

            var boneData = new ALTERITH_Bone_t[mergedBones.Length];

            for (int i = 0; i < mergedBones.Length; ++i)
            {
                Transform boneTransform = mergedBones[i];

                Matrix4x4 localToWorldMatrix = boneTransform.localToWorldMatrix;
                var worldToRootMatrix = root.worldToLocalMatrix;
                var m = worldToRootMatrix * localToWorldMatrix;

                ALTERITH_BoneType_t boneType = ALTERITH_BoneType_t.Other;

                if (IsChildOf(boneTransform, leftHandBone))
                {
                    boneType = ALTERITH_BoneType_t.LeftHand;
                }
                else if (IsChildOf(boneTransform, rightHandBone))
                {
                    boneType = ALTERITH_BoneType_t.RightHand;
                }
                else if (IsChildOf(boneTransform, leftLowerArmBone))
                {
                    boneType = ALTERITH_BoneType_t.LeftLowerArm;
                }
                else if (IsChildOf(boneTransform, rightLowerArmBone))
                {
                    boneType = ALTERITH_BoneType_t.RightLowerArm;
                }
                else if (IsChildOf(boneTransform, leftUpperArmBone))
                {
                    boneType = ALTERITH_BoneType_t.LeftUpperArm;
                }
                else if (IsChildOf(boneTransform, rightUpperArmBone))
                {
                    boneType = ALTERITH_BoneType_t.RightUpperArm;
                }
                else if (IsChildOf(boneTransform, leftShoulderBone))
                {
                    boneType = ALTERITH_BoneType_t.LeftShoulder;
                }
                else if (IsChildOf(boneTransform, rightShoulderBone))
                {
                    boneType = ALTERITH_BoneType_t.RightShoulder;
                }
                else if (IsChildOf(boneTransform, leftFootBone))
                {
                    boneType = ALTERITH_BoneType_t.LeftFoot;
                }
                else if (IsChildOf(boneTransform, rightFootBone))
                {
                    boneType = ALTERITH_BoneType_t.RightFoot;
                }
                else if (IsChildOf(boneTransform, leftLowerLegBone))
                {
                    boneType = ALTERITH_BoneType_t.LeftLowerLeg;
                }
                else if (IsChildOf(boneTransform, rightLowerLegBone))
                {
                    boneType = ALTERITH_BoneType_t.RightLowerLeg;
                }
                else if (IsChildOf(boneTransform, leftUpperLegBone))
                {
                    boneType = ALTERITH_BoneType_t.LeftUpperLeg;
                }
                else if (IsChildOf(boneTransform, rightUpperLegBone))
                {
                    boneType = ALTERITH_BoneType_t.RightUpperLeg;
                }

                boneData[i] = new ALTERITH_Bone_t
                {
                    type = boneType,
                    pose = new ALTERITH_Matrix4f(m),
                };
            }

            IntPtr bonesPtr = AllocateAndMarshalArray(boneData);

            var avatarData = new ALTERITH_Avatar_t
            {
                num_bones = (uint)boneData.Length,
                bones = bonesPtr,
                left_hand_bone_index = (uint)(leftHandBone != null ? boneIndexMap[leftHandBone] : 0),
                right_hand_bone_index = (uint)(rightHandBone != null ? boneIndexMap[rightHandBone] : 0),
                left_upper_leg_bone_index = (uint)(leftUpperLegBone != null ? boneIndexMap[leftUpperLegBone] : 0),
                right_upper_leg_bone_index = (uint)(rightUpperLegBone != null ? boneIndexMap[rightUpperLegBone] : 0),
                left_foot_bone_index = (uint)(leftFootBone != null ? boneIndexMap[leftFootBone] : 0),
                right_foot_bone_index = (uint)(rightFootBone != null ? boneIndexMap[rightFootBone] : 0),
            };

            return new MergedAvatarData(avatarData, boneIndexMap, mergedBones);
        }

        private bool IsChildOf(Transform clothingBone, Transform avatarBone)
        {
            if (clothingBone == null)
            {
                return false;
            }

            if (avatarBone == null)
            {
                return false;
            }

            if (clothingBone.IsChildOf(avatarBone))
            {
                return true;
            }

            var avatarBoneForClothingBone = GetAvatarBoneFromClothingBone(clothingBone);

            if (avatarBoneForClothingBone != null && avatarBoneForClothingBone.IsChildOf(avatarBone))
            {
                return true;
            }

            return false;
        }

        private Transform GetAvatarBoneFromClothingBone(Transform clothingBone)
        {
#if MODULAR_AVATAR
            var mergeArmature = clothingBone.GetComponentInParent<ModularAvatarMergeArmature>();

            if (mergeArmature == null)
            {
                return null;
            }

            Dictionary<Transform, Transform> boneMap;

            if (!boneMapCache.TryGetValue(mergeArmature, out boneMap))
            {
                var getBonesForLockMethod = typeof(ModularAvatarMergeArmature).GetMethod("GetBonesForLock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (getBonesForLockMethod == null)
                {
                    throw new InvalidOperationException("Could not find GetBonesForLock method in ModularAvatarMergeArmature.");
                }

                var bonesForLock = (List<(Transform, Transform)>)getBonesForLockMethod.Invoke(mergeArmature, null);

                if (bonesForLock == null)
                {
                    return null;
                }

                boneMap = bonesForLock.ToDictionary(pair => pair.Item2, pair => pair.Item1);

                boneMapCache[mergeArmature] = boneMap;
            }

            if (boneMap.TryGetValue(clothingBone, out Transform avatarBone))
            {
                return avatarBone;
            }
#endif

            return null;
        }

        private static IEnumerable<AlterithMask> GetMasks(Transform t, Material material)
        {
            if (t.parent != null)
            {
                foreach (var mask in GetMasks(t.parent, material))
                {
                    yield return mask;
                }
            }

            foreach (var mask in t.GetComponents<AlterithMask>())
            {
                if (mask == null)
                {
                    continue;
                }

                if (mask.specifyTargetMaterials)
                {
                    if (mask.targetMaterials == null)
                    {
                        continue;
                    }

                    if (!mask.targetMaterials.Contains(material))
                    {
                        continue;
                    }
                }

                yield return mask;
            }
        }

        private static Dictionary<Texture2D, Texture2D> readableTextureCache = new Dictionary<Texture2D, Texture2D>();

        private static Texture2D GetReadableTexture(Texture2D source)
        {
            if (readableTextureCache.TryGetValue(source, out Texture2D cachedTexture))
            {
                return cachedTexture;
            }

            RenderTexture rt = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear
            );

            Graphics.Blit(source, rt);

            RenderTexture previous = RenderTexture.active;

            RenderTexture.active = rt;

            Texture2D readableTexture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);

            readableTexture.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            readableTexture.Apply();

            RenderTexture.active = previous;

            RenderTexture.ReleaseTemporary(rt);

            readableTextureCache[source] = readableTexture;

            return readableTexture;
        }

        private static float GetTargetVertexRatio(AlterithMask mask, Mesh mesh, int vertexIndex)
        {
            if (mask.specifyMaskImage && mask.maskImage != null && mesh.uv != null && vertexIndex < mesh.uv.Length)
            {
                var uv = mesh.uv[vertexIndex];

                Texture2D readableTexture = GetReadableTexture(mask.maskImage);

                var c = readableTexture.GetPixelBilinear(uv.x, uv.y);

                switch (mask.maskArea)
                {
                    case MaskArea.WhiteAreas:
                        return c.grayscale;
                    case MaskArea.BlackAreas:
                        return 1.0f - c.grayscale;
                    default:
                        break;
                }
            }

            return 1.0f;
        }

        private static ALTERITH_Vertex_t ApplyMaskToVertex(ALTERITH_Vertex_t v, AlterithMask mask, float ratio)
        {
            if (mask.overrideMinimumMargin)
            {
                v.fitting_minimum_margin = Mathf.Lerp(v.fitting_minimum_margin, mask.fittingMinimumMargin, ratio);
            }

            if (mask.overrideMarginScale)
            {
                v.fitting_margin_scale = Mathf.Lerp(v.fitting_margin_scale, mask.fittingMarginScale, ratio);
            }

            switch (mask.affectedByWeightTransfer)
            {
                case OptionalBool.Inherit:
                    break;
                case OptionalBool.Enable:
                    v.transfer_bone_weights_affected_by = Mathf.Lerp(v.transfer_bone_weights_affected_by, 1.0f, ratio);
                    break;
                case OptionalBool.Disable:
                    v.transfer_bone_weights_affected_by = Mathf.Lerp(v.transfer_bone_weights_affected_by, 0.0f, ratio);
                    break;
            }

            switch (mask.affectsWeightTransfer)
            {
                case OptionalBool.Inherit:
                    break;
                case OptionalBool.Enable:
                    v.transfer_bone_weights_affects = Mathf.Lerp(v.transfer_bone_weights_affects, 1.0f, ratio);
                    break;
                case OptionalBool.Disable:
                    v.transfer_bone_weights_affects = Mathf.Lerp(v.transfer_bone_weights_affects, 0.0f, ratio);
                    break;
            }

            switch (mask.applyFitting)
            {
                case OptionalBool.Inherit:
                    break;
                case OptionalBool.Enable:
                    v.apply_fitting = Mathf.Lerp(v.apply_fitting, 1.0f, ratio);
                    break;
                case OptionalBool.Disable:
                    v.apply_fitting = Mathf.Lerp(v.apply_fitting, 0.0f, ratio);
                    break;
            }

            return v;
        }

        private ALTERITH_Mesh_t PrepareMeshWithRemappedBoneIndices(SkinnedMeshRenderer smr, MergedAvatarData mergedAvatarData)
        {
            var mesh = AlterithUtil.BakeBlendShapes(smr, settings.PreserveZeroBlendShapes);
            ALTERITH_Vertex_t[] vertices = new ALTERITH_Vertex_t[mesh.vertexCount];

            var meshVertices = mesh.vertices;
            var meshNormals = mesh.normals;
            var meshTangents = mesh.tangents;
            var meshBoneWeights = mesh.boneWeights;

            for (int i = 0; i < mesh.vertexCount; ++i)
            {
                var vertex = new ALTERITH_Vertex_t();

                vertex.position = new ALTERITH_Vector3f(meshVertices[i]);

                if (meshNormals != null && meshNormals.Length > 0)
                {
                    vertex.normal = new ALTERITH_Vector3f(meshNormals[i]);
                }

                if (meshTangents != null && meshTangents.Length > 0)
                {
                    vertex.tangent = new ALTERITH_Vector4f(meshTangents[i]);
                }

                vertex.bone_weights = new ALTERITH_BoneWeight_t[4];

                if (meshBoneWeights != null && meshBoneWeights.Length > 0)
                {
                    var bw = meshBoneWeights[i];
                    var bone0 = smr.bones[bw.boneIndex0];
                    vertex.bone_weights[0] = new ALTERITH_BoneWeight_t(bone0 == null ? 0 : mergedAvatarData.BoneIndexMap[bone0], bw.weight0);
                    var bone1 = smr.bones[bw.boneIndex1];
                    vertex.bone_weights[1] = new ALTERITH_BoneWeight_t(bone1 == null ? 0 : mergedAvatarData.BoneIndexMap[bone1], bw.weight1);
                    var bone2 = smr.bones[bw.boneIndex2];
                    vertex.bone_weights[2] = new ALTERITH_BoneWeight_t(bone2 == null ? 0 : mergedAvatarData.BoneIndexMap[bone2], bw.weight2);
                    var bone3 = smr.bones[bw.boneIndex3];
                    vertex.bone_weights[3] = new ALTERITH_BoneWeight_t(bone3 == null ? 0 : mergedAvatarData.BoneIndexMap[bone3], bw.weight3);
                }

                vertex.fitting_minimum_margin = settings.MinimumMargin;
                vertex.fitting_margin_scale = settings.MarginScale;
                vertex.transfer_bone_weights_affected_by = 1.0f;
                vertex.transfer_bone_weights_affects = 1.0f;
                vertex.apply_fitting = 1.0f;

                vertices[i] = vertex;
            }

            ALTERITH_SubMesh_t[] submeshes = new ALTERITH_SubMesh_t[mesh.subMeshCount];

            for (int i = 0; i < mesh.subMeshCount; ++i)
            {
                int[] indices = mesh.GetTriangles(i);

                submeshes[i] = new ALTERITH_SubMesh_t
                {
                    num_indices = (uint)indices.Length,
                    indices = AllocateAndMarshalIntegers(indices)
                };

                Material material = null;

                if (smr.sharedMaterials != null && i < smr.sharedMaterials.Length)
                {
                    material = smr.sharedMaterials[i];
                }

                var masks = GetMasks(smr.transform, material).ToArray();

                foreach (var mask in masks)
                {
                    foreach (var vertexIndex in indices)
                    {
                        var ratio = GetTargetVertexRatio(mask, mesh, vertexIndex);

                        if (ratio == 0.0f)
                        {
                            continue;
                        }

                        vertices[vertexIndex] = ApplyMaskToVertex(vertices[vertexIndex], mask, ratio);
                    }
                }
            }

            IntPtr verticesPtr = AllocateAndMarshalArray(vertices);
            IntPtr submeshesPtr = AllocateAndMarshalArray(submeshes);

            var bindPoseMap = new Dictionary<Transform, Matrix4x4>();

            for (int i = 0; i < smr.bones.Length; ++i)
            {
                if (smr.bones[i] == null)
                {
                    continue;
                }

                if (i >= mesh.bindposes.Length)
                {
                    continue;
                }

                bindPoseMap.TryAdd(smr.bones[i], mesh.bindposes[i]);
            }

            Matrix4x4[] bindposes = mergedAvatarData.MergedBones.Select(bone =>
            {
                if (bindPoseMap.TryGetValue(bone, out Matrix4x4 bindpose))
                {
                    return bindpose;
                }
                else
                {
                    return Matrix4x4.identity;
                }
            }).ToArray();

            ALTERITH_Matrix4f[] nativeBindposes = bindposes.Select(bp => new ALTERITH_Matrix4f(bp)).ToArray();
            IntPtr bindposesPtr = AllocateAndMarshalArray(nativeBindposes);

            return new ALTERITH_Mesh_t
            {
                num_vertices = (uint)mesh.vertexCount,
                vertices = verticesPtr,
                num_submeshes = (uint)mesh.subMeshCount,
                submeshes = submeshesPtr,
                num_bindposes = (uint)bindposes.Length,
                bindposes = bindposesPtr
            };
        }
    }
}

#endif
