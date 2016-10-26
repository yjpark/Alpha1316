using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;

namespace yjpark.alpha1316.editor {
    [System.Serializable]
    public class Alpha1316Preference {
        public const string Preference_Path = "Assets/.alpha1316.json";

        private static Alpha1316Preference LoadFromConfig() {
            Alpha1316Preference result = null;
            string content = Util.LoadStringFromFile(Preference_Path, true);
            if (content != null) {
                try {
                    result = JsonUtility.FromJson<Alpha1316Preference>(content);
                } catch (Exception e) {
                    Log.Error("Failed to load configuration: {0} -> {1}", Preference_Path, e);
                }
            }
            if (result == null) {
                result = new Alpha1316Preference();
            }
            return result;
        }

        private static void SaveConfig(Alpha1316Preference instance) {
            string content = JsonUtility.ToJson(instance);
            Util.WriteStringToFile(Preference_Path, content);
        }

        private static Alpha1316Preference _Instance = null;
        public static Alpha1316Preference Instance {
            get {
                if (_Instance == null) {
                    _Instance = LoadFromConfig();
                }
                return _Instance;
            }
        }

        [PreferenceItem("Alpha 1316")]
        public static void OnPreferenceGUI() {
            GUILayout.BeginHorizontal();
            if (_Instance == null || GUILayout.Button("Reload")) {
                _Instance = LoadFromConfig();
            }
            if (GUILayout.Button("Save")) {
                SaveConfig(_Instance);
            }
            if (GUILayout.Button("Add")) {
                _Instance.AddFolderConfig("", "", null);
            }
            GUILayout.EndHorizontal();
            foreach (Alpha1316FolderConfig folder in _Instance.FolderConfigs) {
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
                folder.SrcFolder = EditorGUILayout.TextField("Source", folder.SrcFolder);
                folder.DestFolder = EditorGUILayout.TextField("Destination", folder.DestFolder);
                if (GUILayout.Button("Remove")) {
                    _Instance.RemoveFolderConfig(folder);
                }
            }
        }

        public Alpha1316FolderConfig[] FolderConfigs = new Alpha1316FolderConfig[0];

        private void AddFolderConfig(string srcFolder, string destFolder,
                                              string debugDestFolder=null) {
            Array.Resize(ref FolderConfigs, FolderConfigs.Length + 1);
            FolderConfigs[FolderConfigs.Length - 1] = new Alpha1316FolderConfig(srcFolder, destFolder, debugDestFolder);
        }

        private void  RemoveFolderConfig(Alpha1316FolderConfig toRemove) {
            List<Alpha1316FolderConfig> configs = new List<Alpha1316FolderConfig>();
            foreach (Alpha1316FolderConfig folder in _Instance.FolderConfigs) {
                if (folder != toRemove) {
                    configs.Add(folder);
                }
            }
            FolderConfigs = configs.ToArray();
        }
    }
}
