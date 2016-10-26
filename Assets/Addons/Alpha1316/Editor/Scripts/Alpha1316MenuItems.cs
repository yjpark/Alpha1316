using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEditor;

namespace yjpark.alpha1316.editor {
    public static class Alpha1316MenuItems {
        [MenuItem("Assets/Alpha 1316 - Generate Selected")]
        public static void ConvertSelectedAlpha1316Texture() {
            string error = null;
            if (Selection.activeObject == null) {
                error = "Please select the texture first";
            } else if (Selection.activeObject is Texture2D) {
                Texture2D srcTex = Selection.activeObject as Texture2D;
                string srcPath = AssetDatabase.GetAssetPath(srcTex);

                Alpha1316FolderConfig folderConfig = null;
                foreach (Alpha1316FolderConfig _folderConfig in Alpha1316TextureImporter.A1316_FOLDER_CONFIGS) {
                    if (_folderConfig.IsSrcPath(srcPath)) {
                        folderConfig = _folderConfig;
                        break;
                    }
                }

                string destPath = null;
                string debugDestPath = null;
                if (folderConfig == null) {
                    destPath = srcPath.Replace(".png", Alpha1316TextureImporter.A1316_DEST_SUFFIX);
                    debugDestPath = null;
                    if (Alpha1316TextureImporter.DEBUG_CHECK_ALPHA_AFTER_PACK) {
                        debugDestPath = srcPath.Replace(".png", Alpha1316TextureImporter.A1316_DEBUG_DEST_SUFFIX);
                    }
                } else {
                    destPath = folderConfig.GetDestPath(srcPath);
                    debugDestPath = folderConfig.GetDebugDestPath(srcPath);
                }
                error = PackAlpha1316(srcTex, destPath, debugDestPath);
            } else {
                error = string.Format("Selected object is not a texture:\n{0}\n{1}",
                                    Selection.activeObject.GetType(), Selection.activeObject);
            }
            if (error != null) {
                EditorUtility.DisplayDialog("Alpha 1316", error, "OK");
            }
        }

        [MenuItem("Assets/Alpha 1316 - Generate Selected", true)]
        public static bool ConvertSelectedAlpha1316TextureValidation() {
            if (Selection.activeObject is Texture2D) {
                return Alpha1316Packer.IsValidAlpha1316Source(Selection.activeObject as Texture2D);
            }
            return false;
        }

        [MenuItem("Assets/Alpha 1316 - Regenerate All")]
        public static void ConvertAllAlpha1316Texture() {
            EditorCoroutine.Start(ConvertAllAlpha1316TextureAsync());
        }

        private static IEnumerator ConvertAllAlpha1316TextureAsync() {
            foreach (Alpha1316FolderConfig folderConfig in Alpha1316TextureImporter.A1316_FOLDER_CONFIGS) {
                IEnumerator convertFolder = ConvertAlph1316FolderAsync(folderConfig);
                while (convertFolder.MoveNext()) yield return convertFolder.Current;
            }
        }

        private static IEnumerator ConvertAlph1316FolderAsync(Alpha1316FolderConfig folderConfig) {
            if (!Directory.Exists(folderConfig.SrcFolder)) {
                Log.Error("Src Folder Not Exist {0}", folderConfig.SrcFolder);
                yield break;
            }

            string[] dirs = new string[1];
            dirs[0] = folderConfig.SrcFolder;
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", dirs);
            foreach (var guid in guids) {
                string srcPath = AssetDatabase.GUIDToAssetPath(guid);
                Texture2D srcTex = (Texture2D)AssetDatabase.LoadAssetAtPath(srcPath, typeof(Texture2D));

                string destPath = folderConfig.GetDestPath(srcPath);
                string debugDestPath = folderConfig.GetDebugDestPath(srcPath);
                string error = PackAlpha1316(srcTex, destPath, debugDestPath);
                if (error != null) {
                    Log.Error(srcTex, "Failed to Convert Alpha 13_16 Texture: {0} -> {1}", srcPath, error);
                }
                yield return new WaitForEndOfFrame();
            }
        }

        private static string PackAlpha1316(Texture2D srcTex, string destPath, string debugDestPath) {
            string error = null;

            if (srcTex.width != srcTex.height || srcTex.width % Alpha1316Packer.SRC_SIZE_STEP != 0) {
                return "The width and height of the texture should be same, and can be divided by 13!";
            }

            error = Alpha1316Packer.PackAlpha1316(srcTex, destPath, false);

            if (debugDestPath != null) {
                Alpha1316Packer.PackAlpha1316(srcTex, debugDestPath, true);
            }
            return error;
        }
    }
}
