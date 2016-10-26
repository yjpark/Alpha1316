using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

// https://gist.github.com/benblo/10732554

namespace yjpark.alpha1316.editor {
    public class EditorCoroutine {
        public static EditorCoroutine Start(IEnumerator _routine) {
            EditorCoroutine coroutine = new EditorCoroutine(_routine);
            coroutine.Start();
            return coroutine;
        }

        readonly IEnumerator routine;
        EditorCoroutine(IEnumerator _routine) {
            routine = _routine;
        }

        public void Start() {
            //Debug.Log("start");
            EditorApplication.update += Update;
        }
        public void Stop() {
            //Debug.Log("stop");
            EditorApplication.update -= Update;
        }

        public void Update() {
            /* NOTE: no need to try/catch MoveNext,
            * if an IEnumerator throws its next iteration returns false.
            * Also, Unity probably catches when calling EditorApplication.update.
            */

            //Debug.Log("update");
            if (!routine.MoveNext()) {
                Stop();
            }
        }
    }
}

