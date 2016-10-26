using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace yjpark.alpha1316.editor {
    public static class Util {
        public static string LoadStringFromFile(string path, bool isDebug = false) {
            if (File.Exists(path)) {
                StreamReader reader = new StreamReader(path);
                string str = reader.ReadToEnd();
                reader.Close();
                return str;
            } else {
                if (!isDebug) {
                    Log.Error("File Not Exist: {0}", path);
                }
                return null;
            }
        }

        public static bool WriteStringToFile(string path, string content) {
            try {
                StreamWriter writer = new StreamWriter(path);
                writer.Write(content);
                writer.Close();
                return true;
            } catch (System.Exception e) {
                Log.Error("Failed to save string to file: {0} -> {1}", path, e);
            }
            return false;
        }
    }
}
