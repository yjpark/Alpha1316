using System;
using System.Collections;

using UnityEngine;

namespace yjpark.alpha1316.editor {
    public static class Log {
        public static void Info(UnityEngine.Object context, string format, params object[] values) {
            string msg = format;
            if (values != null && values.Length > 0) msg = string.Format(format, values);
            if (context != null) {
                Debug.Log(string.Format("[{0}] {1}", context.GetType().Name, msg), context);
            } else {
                Debug.Log(msg);
            }
        }

        public static void Info(string format, params object[] values) {
            Info(null, format, values);
        }

        public static void Error(UnityEngine.Object context, string format, params object[] values) {
            string msg = format;
            if (values != null && values.Length > 0) msg = string.Format(format, values);
            if (context != null) {
                Debug.LogError(string.Format("[{0}] [{1}] {2}", Time.frameCount, context.GetType().Name, msg), context);
            } else {
                Debug.LogError(msg);
            }
        }

        public static void Error(string format, params object[] values) {
            Error(null, format, values);
        }
    }
}

