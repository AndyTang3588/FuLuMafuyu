/*
 * 材质复制工具 - Material Copy Tool
 * 功能：将材质A的各种设置复制到材质B上，支持选择性同步各种属性
 * 作者：诺喵工具箱
 * 用途：快速同步材质属性，提高工作效率
 */

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace NyameauToolbox.Editor
{
    public partial class MaterialCopyWindow : EditorWindow
    {
        // 源材质和目标材质
        private Material sourceMaterial;
        private Material targetMaterial;
        
        // 滚动位置
        private Vector2 scrollPosition;
        
        // UI样式
        private GUIStyle headerStyle;
        private GUIStyle sectionStyle;
        private GUIStyle buttonStyle;
        
        // UI状态变量
        
        // 复制选项 - 基本设置
        private bool copyBasicSettings = false;
        private bool copyLightingSettings = false;
        private bool copyUVSettings = false;
        private bool copyVRChatSettings = false;
        
        // 复制选项 - 颜色设置
        private bool copyColorSettings = false;
        private bool copyMainColorAlpha = false;
        private bool copyShadowSettings = false;
        private bool copyRimShadeSettings = false;
        
        // 复制选项 - 发光和法线
        private bool copyEmissionSettings = false;
        private bool copyNormalReflectionSettings = false;
        private bool copyNormalMapSettings = false;
        private bool copyBacklightSettings = false;
        private bool copyReflectionSettings = false;
        
        // 复制选项 - 特效设置
        private bool copyMatCapSettings = false;
        private bool copyRimLightSettings = false;
        private bool copyGlitterSettings = false;
        
        // 复制选项 - 扩展设置
        private bool copyOutlineSettings = false;
        private bool copyParallaxSettings = false;
        private bool copyDistanceFadeSettings = false;
        private bool copyAudioLinkSettings = false;
        private bool copyDissolveSettings = false;
        private bool copyIDMaskSettings = false;
        private bool copyUVTileDiscardSettings = false;
        private bool copyStencilSettings = false;
        
        // 复制选项 - 渲染设置
        private bool copyRenderSettings = false;
        private bool copyLightmapSettings = false;
        private bool copyTessellationSettings = false;
        private bool copyOptimizationSettings = false;
        
        // 材质压缩选项
        private bool enableTextureCompression = false;
        private int compressionSize = 1024;
        private readonly int[] compressionSizes = { 128, 256, 512, 1024, 1280, 2048, 4096 };
        private readonly string[] compressionSizeLabels = { "128x128", "256x256", "512x512", "1024x1024", "1280x720", "2048x2048", "4096x4096" };
        
        // 全面复制选项
        private bool useComprehensiveCopy = false;
        
        [MenuItem("诺喵工具箱/材质复制器", false, 13)]
        public static void ShowWindow()
        {
            var window = GetWindow<MaterialCopyWindow>("材质复制工具");
            window.minSize = new Vector2(400, 600);
            window.Show();
        }
        
        private void OnEnable()
        {
            InitializeStyles();
        }
        
        private void InitializeStyles()
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.2f, 0.6f, 1f) }
            };
            
            sectionStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                normal = { textColor = new Color(0.3f, 0.3f, 0.3f) }
            };
            
            buttonStyle = new GUIStyle("Button")
            {
                fontSize = 12,
                fixedHeight = 30
            };
        }
        
        private void OnGUI()
        {
            DrawHeader();
            DrawMaterialSelection();
            DrawCopyOptions();
            DrawActionButtons();
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("🎨 材质复制工具", headerStyle);
            GUILayout.Label("将材质A的设置复制到材质B上", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.Space(10);
        }
        
        private void DrawMaterialSelection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("材质选择", sectionStyle);
            EditorGUILayout.Space(5);
            
            sourceMaterial = (Material)EditorGUILayout.ObjectField("源材质 (复制自)", sourceMaterial, typeof(Material), false);
            targetMaterial = (Material)EditorGUILayout.ObjectField("目标材质 (复制到)", targetMaterial, typeof(Material), false);
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
        }
        
        private void DrawCopyOptions()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // 显示选择性复制选项
                // 基本设置组
                DrawSectionGroup("基本设置", new System.Action[]
                {
                    () => copyBasicSettings = EditorGUILayout.Toggle("基本设置", copyBasicSettings),
                    () => copyLightingSettings = EditorGUILayout.Toggle("照明设置", copyLightingSettings),
                    () => copyUVSettings = EditorGUILayout.Toggle("UV 设置", copyUVSettings),
                    () => copyVRChatSettings = EditorGUILayout.Toggle("VRChat", copyVRChatSettings)
                });
            
            // 颜色设置组
            DrawSectionGroup("颜色设置", new System.Action[]
            {
                () => copyColorSettings = EditorGUILayout.Toggle("颜色设置", copyColorSettings),
                () => copyMainColorAlpha = EditorGUILayout.Toggle("主色/Alpha 设置", copyMainColorAlpha),
                () => copyShadowSettings = EditorGUILayout.Toggle("阴影设置", copyShadowSettings),
                () => copyRimShadeSettings = EditorGUILayout.Toggle("RimShade", copyRimShadeSettings)
            });
            
            // 发光和法线设置组
            DrawSectionGroup("发光和法线设置", new System.Action[]
            {
                () => copyEmissionSettings = EditorGUILayout.Toggle("发光设置", copyEmissionSettings),
                () => copyNormalReflectionSettings = EditorGUILayout.Toggle("法线贴图", copyNormalReflectionSettings),
                () => copyNormalMapSettings = EditorGUILayout.Toggle("法线贴图设置", copyNormalMapSettings),
                () => copyBacklightSettings = EditorGUILayout.Toggle("背光灯设置", copyBacklightSettings),
                () => copyReflectionSettings = EditorGUILayout.Toggle("反射设置", copyReflectionSettings)
            });
            
            // 特效设置组
            DrawSectionGroup("特效设置", new System.Action[]
            {
                () => copyMatCapSettings = EditorGUILayout.Toggle("MatCap 设置", copyMatCapSettings),
                () => copyRimLightSettings = EditorGUILayout.Toggle("Rim Light 设置", copyRimLightSettings),
                () => copyGlitterSettings = EditorGUILayout.Toggle("Glitter设置", copyGlitterSettings)
            });
            
            // 扩展设置组
            DrawSectionGroup("扩展设置", new System.Action[]
            {
                () => copyOutlineSettings = EditorGUILayout.Toggle("轮廓设置", copyOutlineSettings),
                () => copyParallaxSettings = EditorGUILayout.Toggle("视差", copyParallaxSettings),
                () => copyDistanceFadeSettings = EditorGUILayout.Toggle("距离淡化", copyDistanceFadeSettings),
                () => copyAudioLinkSettings = EditorGUILayout.Toggle("AudioLink", copyAudioLinkSettings),
                () => copyDissolveSettings = EditorGUILayout.Toggle("Dissolve", copyDissolveSettings),
                () => copyIDMaskSettings = EditorGUILayout.Toggle("ID Mask", copyIDMaskSettings),
                () => copyUVTileDiscardSettings = EditorGUILayout.Toggle("UV TileDiscard", copyUVTileDiscardSettings),
                () => copyStencilSettings = EditorGUILayout.Toggle("Stencil 设置", copyStencilSettings)
            });
            
            // 渲染设置组
            DrawSectionGroup("渲染设置", new System.Action[]
            {
                () => copyRenderSettings = EditorGUILayout.Toggle("渲染设置", copyRenderSettings),
                () => copyLightmapSettings = EditorGUILayout.Toggle("光照烘培设置", copyLightmapSettings),
                () => copyTessellationSettings = EditorGUILayout.Toggle("镶嵌（极高负载)", copyTessellationSettings),
                () => copyOptimizationSettings = EditorGUILayout.Toggle("优化", copyOptimizationSettings)
            });

            // 材质压缩设置组
            DrawTextureCompressionSection();
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawSectionGroup(string title, System.Action[] toggleActions)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(title, sectionStyle);
            EditorGUILayout.Space(5);
            
            EditorGUI.indentLevel++;
            foreach (var action in toggleActions)
            {
                action.Invoke();
            }
            EditorGUI.indentLevel--;
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
        
        private void DrawActionButtons()
        {
            EditorGUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            
            // 全选按钮
            if (GUILayout.Button("全选", buttonStyle))
            {
                SetAllOptions(true);
            }
            
            // 全不选按钮
            if (GUILayout.Button("全不选", buttonStyle))
            {
                SetAllOptions(false);
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
            
            // 复制按钮
            GUI.enabled = sourceMaterial != null && targetMaterial != null && HasAnyOptionSelected();
            
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            string buttonText = "🚀 开始复制材质属性";
            if (GUILayout.Button(buttonText, GUILayout.Height(40)))
            {
                PerformCopy();
            }
            GUI.backgroundColor = Color.white;
            GUI.enabled = true;
            
            if (sourceMaterial == null || targetMaterial == null)
            {
                EditorGUILayout.HelpBox("请选择源材质和目标材质", MessageType.Warning);
            }
            else if (!HasAnyOptionSelected())
            {
                EditorGUILayout.HelpBox("请至少选择一个复制选项", MessageType.Warning);
            }
        }
        
        private void SetAllOptions(bool value)
        {
                copyBasicSettings = value;
                copyLightingSettings = value;
                copyUVSettings = value;
                copyVRChatSettings = value;
                copyColorSettings = value;
                copyMainColorAlpha = value;
                copyShadowSettings = value;
                copyRimShadeSettings = value;
                copyEmissionSettings = value;
                copyNormalReflectionSettings = value;
                copyNormalMapSettings = value;
                copyBacklightSettings = value;
                copyReflectionSettings = value;
                copyMatCapSettings = value;
                copyRimLightSettings = value;
                copyGlitterSettings = value;
                copyOutlineSettings = value;
                copyParallaxSettings = value;
                copyDistanceFadeSettings = value;
                copyAudioLinkSettings = value;
                copyDissolveSettings = value;
                copyIDMaskSettings = value;
                copyUVTileDiscardSettings = value;
                copyStencilSettings = value;
                copyRenderSettings = value;
                copyLightmapSettings = value;
                copyTessellationSettings = value;
                copyOptimizationSettings = value;
                enableTextureCompression = value;
        }
        
        private bool HasAnyOptionSelected()
        {
            return copyBasicSettings || copyLightingSettings || copyUVSettings || copyVRChatSettings ||
                   copyColorSettings || copyMainColorAlpha || copyShadowSettings || copyRimShadeSettings ||
                   copyEmissionSettings || copyNormalReflectionSettings || copyNormalMapSettings || 
                   copyBacklightSettings || copyReflectionSettings || copyMatCapSettings || 
                   copyRimLightSettings || copyGlitterSettings || copyOutlineSettings || 
                   copyParallaxSettings || copyDistanceFadeSettings || copyAudioLinkSettings || 
                   copyDissolveSettings || copyIDMaskSettings || copyUVTileDiscardSettings || 
                   copyStencilSettings || copyRenderSettings || copyLightmapSettings || 
                   copyTessellationSettings || copyOptimizationSettings || enableTextureCompression;
        }
        
        private void PerformCopy()
        {
            if (sourceMaterial == null || targetMaterial == null)
            {
                EditorUtility.DisplayDialog("错误", "请选择源材质和目标材质", "确定");
                return;
            }
            
            // 记录撤销操作
            Undo.RecordObject(targetMaterial, "复制材质属性");
            
            int copiedCount = 0;
            bool shaderChanged = false;
            
            try
            {
                // 检查着色器是否相同，如果不同则先复制着色器
                if (sourceMaterial.shader != targetMaterial.shader)
                {
                    targetMaterial.shader = sourceMaterial.shader;
                    shaderChanged = true;
                    Debug.Log($"[材质复制工具] 着色器已从 '{sourceMaterial.shader.name}' 复制到目标材质");
                }
                
                Debug.Log("[材质复制工具] 开始选择性复制材质属性");
                
                // 基本设置
                if (copyBasicSettings)
                {
                    copiedCount += CopyBasicSettings();
                }
                
                // 照明设置
                if (copyLightingSettings)
                {
                    copiedCount += CopyLightingSettings();
                }
                
                // UV设置
                if (copyUVSettings)
                {
                    copiedCount += CopyUVSettings();
                }
                
                // VRChat设置
                if (copyVRChatSettings)
                {
                    copiedCount += CopyVRChatSettings();
                }
                
                // 颜色设置
                if (copyColorSettings)
                {
                    copiedCount += CopyColorSettings();
                }
                
                // 主色/Alpha设置
                if (copyMainColorAlpha)
                {
                    copiedCount += CopyMainColorAlphaSettings();
                }
                
                // 阴影设置
                if (copyShadowSettings)
                {
                    copiedCount += CopyShadowSettings();
                }
                
                // RimShade设置
                if (copyRimShadeSettings)
                {
                    copiedCount += CopyRimShadeSettings();
                }
                
                // 发光设置
                if (copyEmissionSettings)
                {
                    copiedCount += CopyEmissionSettings();
                }
                
                // 法线贴图&反射设置
                if (copyNormalReflectionSettings)
                {
                    copiedCount += CopyNormalReflectionSettings();
                }
                
                // 法线贴图设置
                if (copyNormalMapSettings)
                {
                    copiedCount += CopyNormalMapSettings();
                }
                
                // 背光灯设置
                if (copyBacklightSettings)
                {
                    copiedCount += CopyBacklightSettings();
                }
                
                // 反射设置
                if (copyReflectionSettings)
                {
                    copiedCount += CopyReflectionSettings();
                }
                
                // MatCap设置
                if (copyMatCapSettings)
                {
                    copiedCount += CopyMatCapSettings();
                }
                
                // Rim Light设置
                if (copyRimLightSettings)
                {
                    copiedCount += CopyRimLightSettings();
                }
                
                // Glitter设置
                if (copyGlitterSettings)
                {
                    copiedCount += CopyGlitterSettings();
                }
                
                // 轮廓设置
                if (copyOutlineSettings)
                {
                    copiedCount += CopyOutlineSettings();
                }
                
                // 视差设置
                if (copyParallaxSettings)
                {
                    copiedCount += CopyParallaxSettings();
                }
                
                // 距离淡化设置
                if (copyDistanceFadeSettings)
                {
                    copiedCount += CopyDistanceFadeSettings();
                }
                
                // AudioLink设置
                if (copyAudioLinkSettings)
                {
                    copiedCount += CopyAudioLinkSettings();
                }
                
                // Dissolve设置
                if (copyDissolveSettings)
                {
                    copiedCount += CopyDissolveSettings();
                }
                
                // ID Mask设置
                if (copyIDMaskSettings)
                {
                    copiedCount += CopyIDMaskSettings();
                }
                
                // UV TileDiscard设置
                if (copyUVTileDiscardSettings)
                {
                    copiedCount += CopyUVTileDiscardSettings();
                }
                
                // Stencil设置
                if (copyStencilSettings)
                {
                    copiedCount += CopyStencilSettings();
                }
                
                // 渲染设置
                if (copyRenderSettings)
                {
                    copiedCount += CopyRenderSettings();
                }
                
                // 光照烘培设置
                if (copyLightmapSettings)
                {
                    copiedCount += CopyLightmapSettings();
                }
                
                // 镶嵌设置
                if (copyTessellationSettings)
                {
                    copiedCount += CopyTessellationSettings();
                }
                
                // 优化设置
                if (copyOptimizationSettings)
                {
                    copiedCount += CopyOptimizationSettings();
                }
                
                // 材质压缩
                if (enableTextureCompression)
                {
                    copiedCount += CompressTextures();
                }
                
                // 仅在有选项被勾选时，执行补充复制确保完整性
                if (HasAnyOptionSelected())
                {
                    Debug.Log("[材质复制工具] 执行补充属性复制，确保选中项目完整复制");
                    copiedCount += CopySelectedUnknownProperties();
                }
                
                // 标记材质为脏数据，确保保存
                EditorUtility.SetDirty(targetMaterial);
                
                // 显示完成消息
                string message = $"成功复制了 {copiedCount} 个属性到目标材质\n\n" +
                    $"源材质: {sourceMaterial.name}\n" +
                    $"目标材质: {targetMaterial.name}";
                
                if (shaderChanged)
                {
                    message += "\n\n✓ 着色器已同步复制";
                }
                
                EditorUtility.DisplayDialog("复制完成", message, "确定");
                    
                string logMessage = $"[材质复制工具] 成功复制 {copiedCount} 个属性从 '{sourceMaterial.name}' 到 '{targetMaterial.name}'";
                if (shaderChanged)
                {
                    logMessage += " (包含着色器)";
                }
                Debug.Log(logMessage);
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("复制失败", $"复制过程中发生错误:\n{e.Message}", "确定");
                Debug.LogError($"[材质复制工具] 复制失败: {e.Message}");
            }
        }
        
        // 绘制材质压缩设置区域
        private void DrawTextureCompressionSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("🗜️ 材质压缩设置", sectionStyle);
            EditorGUILayout.Space(5);
            
            enableTextureCompression = EditorGUILayout.Toggle("启用纹理压缩", enableTextureCompression);
            
            if (enableTextureCompression)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("压缩尺寸:", GUILayout.Width(80));
                
                int selectedIndex = System.Array.IndexOf(compressionSizes, compressionSize);
                if (selectedIndex == -1) selectedIndex = 3; // 默认1024
                
                selectedIndex = EditorGUILayout.Popup(selectedIndex, compressionSizeLabels);
                compressionSize = compressionSizes[selectedIndex];
                
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.HelpBox(
                    "智能压缩功能说明:\n" +
                    "• 自动检测材质中的贴图、纹理、蒙版和法线图\n" +
                    "• 只压缩最大边大于选择尺寸的图片\n" +
                    "• 保持原始宽高比进行压缩\n" +
                    "• 支持1280x720等非正方形纹理\n" +
                    "• 小于选择尺寸的图片保持不变",
                    MessageType.Info);
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
        
        // 压缩纹理方法
        private int CompressTextures()
        {
            if (targetMaterial == null)
            {
                Debug.LogWarning("[材质压缩] 目标材质为空");
                return 0;
            }
            
            Debug.Log($"[材质压缩] 开始压缩材质 '{targetMaterial.name}' 中的纹理，目标尺寸: {compressionSize}x{compressionSize}");
            
            int compressedCount = 0;
            var shader = targetMaterial.shader;
            
            // 获取所有纹理属性
            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    string propertyName = ShaderUtil.GetPropertyName(shader, i);
                    Texture texture = targetMaterial.GetTexture(propertyName);
                    
                    if (texture != null && texture is Texture2D)
                    {
                        Texture2D texture2D = texture as Texture2D;
                        
                        // 检查是否需要压缩（最大边大于目标尺寸）
                        int maxDimension = Mathf.Max(texture2D.width, texture2D.height);
                        if (maxDimension > compressionSize)
                        {
                            Debug.Log($"[材质压缩] 发现需要压缩的纹理: {propertyName} - {texture2D.name} ({texture2D.width}x{texture2D.height})");
                            
                            // 压缩纹理
                            Texture2D compressedTexture = CompressTexture2D(texture2D, propertyName);
                            if (compressedTexture != null)
                            {
                                targetMaterial.SetTexture(propertyName, compressedTexture);
                                compressedCount++;
                                Debug.Log($"[材质压缩] 成功压缩纹理: {propertyName} -> 新尺寸");
                            }
                        }
                        else
                        {
                            Debug.Log($"[材质压缩] 纹理 {propertyName} - {texture2D.name} ({texture2D.width}x{texture2D.height}) 小于目标尺寸，跳过压缩");
                        }
                    }
                }
            }
            
            if (compressedCount > 0)
            {
                EditorUtility.SetDirty(targetMaterial);
                AssetDatabase.SaveAssets();
                Debug.Log($"[材质压缩] 压缩完成，共处理 {compressedCount} 个纹理");
            }
            else
            {
                Debug.Log("[材质压缩] 没有找到需要压缩的纹理");
            }
            
            return compressedCount;
        }
        
        // 压缩单个Texture2D
        private Texture2D CompressTexture2D(Texture2D originalTexture, string propertyName)
        {
            try
            {
                // 获取原始纹理的路径
                string assetPath = AssetDatabase.GetAssetPath(originalTexture);
                if (string.IsNullOrEmpty(assetPath))
                {
                    Debug.LogWarning($"[材质压缩] 无法获取纹理路径: {originalTexture.name}");
                    return null;
                }
                
                // 读取纹理导入设置
                TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (textureImporter == null)
                {
                    Debug.LogWarning($"[材质压缩] 无法获取纹理导入器: {originalTexture.name}");
                    return null;
                }
                
                // 备份原始设置
                bool wasReadable = textureImporter.isReadable;
                TextureImporterCompression originalCompression = textureImporter.textureCompression;
                int originalMaxSize = textureImporter.maxTextureSize;
                
                // 设置为可读取
                textureImporter.isReadable = true;
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                
                // 应用设置
                AssetDatabase.ImportAsset(assetPath);
                
                // 计算保持宽高比的新尺寸
                int newWidth, newHeight;
                CalculateNewSize(originalTexture.width, originalTexture.height, compressionSize, out newWidth, out newHeight);
                
                Debug.Log($"[材质压缩] 原始尺寸: {originalTexture.width}x{originalTexture.height}, 压缩后尺寸: {newWidth}x{newHeight}");
                
                // 创建新的压缩纹理
                Texture2D compressedTexture = new Texture2D(newWidth, newHeight, originalTexture.format, false);
                
                // 获取原始像素数据
                Color[] originalPixels = originalTexture.GetPixels();
                
                // 重新采样到目标尺寸
                Color[] resizedPixels = new Color[newWidth * newHeight];
                
                float xRatio = (float)originalTexture.width / newWidth;
                float yRatio = (float)originalTexture.height / newHeight;
                
                for (int y = 0; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        int originalX = Mathf.FloorToInt(x * xRatio);
                        int originalY = Mathf.FloorToInt(y * yRatio);
                        
                        originalX = Mathf.Clamp(originalX, 0, originalTexture.width - 1);
                        originalY = Mathf.Clamp(originalY, 0, originalTexture.height - 1);
                        
                        int originalIndex = originalY * originalTexture.width + originalX;
                        int newIndex = y * newWidth + x;
                        
                        resizedPixels[newIndex] = originalPixels[originalIndex];
                    }
                }
                
                // 设置像素数据
                compressedTexture.SetPixels(resizedPixels);
                compressedTexture.Apply();
                
                // 保存压缩后的纹理
                string directory = System.IO.Path.GetDirectoryName(assetPath);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                string extension = System.IO.Path.GetExtension(assetPath);
                string newPath = System.IO.Path.Combine(directory, $"{fileName}_compressed_{newWidth}x{newHeight}{extension}");
                
                byte[] pngData = compressedTexture.EncodeToPNG();
                System.IO.File.WriteAllBytes(newPath, pngData);
                
                // 恢复原始设置
                textureImporter.isReadable = wasReadable;
                textureImporter.textureCompression = originalCompression;
                textureImporter.maxTextureSize = originalMaxSize;
                AssetDatabase.ImportAsset(assetPath);
                
                // 刷新资源数据库
                AssetDatabase.Refresh();
                
                // 加载新创建的纹理
                Texture2D newTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(newPath);
                
                // 清理临时纹理
                DestroyImmediate(compressedTexture);
                
                return newTexture;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[材质压缩] 压缩纹理失败 {originalTexture.name}: {e.Message}");
                return null;
            }
        }
        
        // 计算保持宽高比的新尺寸
        private void CalculateNewSize(int originalWidth, int originalHeight, int maxSize, out int newWidth, out int newHeight)
        {
            // 特殊处理1280x720的情况
            if (maxSize == 1280 && originalWidth == 1280 && originalHeight == 720)
            {
                newWidth = 1280;
                newHeight = 720;
                return;
            }
            
            // 找到最大的边
            int maxDimension = Mathf.Max(originalWidth, originalHeight);
            
            // 如果最大边小于等于目标尺寸，保持原尺寸
            if (maxDimension <= maxSize)
            {
                newWidth = originalWidth;
                newHeight = originalHeight;
                return;
            }
            
            // 计算缩放比例
            float scale = (float)maxSize / maxDimension;
            
            // 计算新尺寸，保持宽高比
            newWidth = Mathf.RoundToInt(originalWidth * scale);
            newHeight = Mathf.RoundToInt(originalHeight * scale);
            
            // 确保新尺寸不为0
            newWidth = Mathf.Max(1, newWidth);
            newHeight = Mathf.Max(1, newHeight);
        }
    }
}