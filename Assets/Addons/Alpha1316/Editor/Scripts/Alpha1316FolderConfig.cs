using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace yjpark.alpha1316.editor {
    [System.Serializable]
    public class Alpha1316FolderConfig {
        public const string AssetsPrefix = "Assets/";

        public string SrcFolder;
        public string DestFolder;
        public string DebugDestFolder;

        public Alpha1316FolderConfig(string srcFolder, string destFolder, string debugDestFolder) {
            SrcFolder = GetFolder(srcFolder);
            DestFolder = GetFolder(destFolder);
            DebugDestFolder = debugDestFolder == null ? null : GetFolder(debugDestFolder);
        }

        private string GetFolder(string folder) {
            if (!folder.StartsWith(AssetsPrefix)) {
                folder = AssetsPrefix + folder;
            }
            if (folder.EndsWith("/")) {
                folder = folder.Substring(0, folder.Length - 1);
            }
            return folder;
        }

        public bool IsSrcPath(string assetPath) {
            return assetPath.StartsWith(SrcFolder + "/");
        }

        public bool IsDestPath(string assetPath) {
            if (assetPath.StartsWith(DestFolder + "/")) {
                return true;
            }
            if (DebugDestFolder != null) {
                return assetPath.StartsWith(DebugDestFolder + "/");
            }
            return false;
        }

        private string CheckAndGetDestPath(string assetPath, string destFolder) {
            if (!assetPath.StartsWith(AssetsPrefix)) {
                Log.Error("Invalid assetPath: {0}, -> {1}", assetPath, destFolder);
                return null;
            }
            string destPath = assetPath.Replace(SrcFolder, destFolder);
            string dir = Path.GetDirectoryName(destPath);
            if (!Directory.Exists(dir)) {
                Log.Info("Creating Folder: {0}", dir);
                Directory.CreateDirectory(dir);
            }
            return destPath;
        }

        public string GetDestPath(string assetPath) {
            return CheckAndGetDestPath(assetPath, DestFolder);
        }

        public string GetDebugDestPath(string assetPath) {
            if (!string.IsNullOrEmpty(DebugDestFolder)) {
                return CheckAndGetDestPath(assetPath, DebugDestFolder);
            }
            return null;
        }
    }
}
