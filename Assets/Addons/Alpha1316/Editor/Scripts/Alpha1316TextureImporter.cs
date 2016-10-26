using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEditor;

namespace yjpark.alpha1316.editor {
    public class Alpha1316TextureImporter : AssetPostprocessor {
        public static bool DEBUG_CHECK_ALPHA_AFTER_PACK = false;

        // Keep these as static in case user code wants different values
        public static int ANDROID_MAX_TEXTURE_SIZE = 1024;
        public static int IOS_MAX_TEXTURE_SIZE = 1024;

        public static string A1316_DEST_SUFFIX = "_a1316.png";
        public static string A1316_DEBUG_DEST_SUFFIX = "_a1316_debug.png";

        public static Alpha1316FolderConfig[] A1316_FOLDER_CONFIGS {
            get {
                return Alpha1316Preference.Instance.FolderConfigs;
            }
        }

        public static bool IsAlpha1316SrcPath(string assetPath) {
            if (A1316_FOLDER_CONFIGS == null) return false;

            foreach (Alpha1316FolderConfig folderConfig in A1316_FOLDER_CONFIGS) {
                if (folderConfig.IsSrcPath(assetPath)) {
                    return true;
                }
            }
            return false;
        }

        public static bool IsAlpha1316DestPath(string assetPath) {
            if (assetPath.EndsWith(A1316_DEST_SUFFIX)) {
                return true;
            }
            if (assetPath.EndsWith(A1316_DEBUG_DEST_SUFFIX)) {
                return true;
            }
            if (A1316_FOLDER_CONFIGS == null) return false;
            foreach (Alpha1316FolderConfig folderConfig in A1316_FOLDER_CONFIGS) {
                if (folderConfig.IsDestPath(assetPath)) {
                    return true;
                }
            }
            return false;
        }

        private void UpdateSrcTextureImporter(TextureImporter importer) {
            importer.textureType  = TextureImporterType.Advanced;
            importer.textureFormat = TextureImporterFormat.RGBA32;
            importer.isReadable = true;

            Object asset = AssetDatabase.LoadAssetAtPath(importer.assetPath, typeof(Texture2D));
            if (asset) {
                EditorUtility.SetDirty(asset);
            }
        }

        private void UpdateDestTextureImporter(TextureImporter importer) {
            importer.textureType  = TextureImporterType.Advanced;
            importer.textureFormat = TextureImporterFormat.RGB16;
            importer.isReadable = false;

            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.anisoLevel = 0;
            importer.npotScale = TextureImporterNPOTScale.None;

            importer.SetPlatformTextureSettings(BuildTarget.Android.ToString(), ANDROID_MAX_TEXTURE_SIZE, TextureImporterFormat.ETC_RGB4);
            importer.SetPlatformTextureSettings("iPhone", IOS_MAX_TEXTURE_SIZE, TextureImporterFormat.PVRTC_RGB4);

            Object asset = AssetDatabase.LoadAssetAtPath(importer.assetPath, typeof(Texture2D));
            if (asset) {
                EditorUtility.SetDirty(asset);
            }
        }

        public void OnPreprocessTexture() {
            if (IsAlpha1316SrcPath(assetPath)) {
                UpdateSrcTextureImporter(assetImporter as TextureImporter);
            } else if (IsAlpha1316DestPath(assetPath)) {
                UpdateDestTextureImporter(assetImporter as TextureImporter);
            }
        }
    }
}
