
// Copied from: https://forum.unity.com/threads/editor-coroutines.589504/
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

public static class EditorCoroutines
{

    public class Coroutine
    {
        public IEnumerator enumerator;
        public System.Action<bool> OnUpdate;
        public List<IEnumerator> history = new List<IEnumerator>();
    }

    static readonly List<Coroutine> coroutines = new List<Coroutine>();

    [InitializeOnLoadMethod]
    private static void OnLoad() {
        EditorCoroutines.StopAll();
    }

    public static Coroutine Execute(IEnumerator enumerator, System.Action<bool> OnUpdate = null)
    {
        if (coroutines.Count == 0)
        {
            EditorApplication.update += Update;
        }
        var coroutine = new Coroutine { enumerator = enumerator, OnUpdate = OnUpdate };
        coroutines.Add(coroutine);
        return coroutine;
    }

    static void Update()
    {
        for (int i = 0; i < coroutines.Count; i++)
        {
            var coroutine = coroutines[i];
            bool done = !coroutine.enumerator.MoveNext();
            if (done)
            {
                if (coroutine.history.Count == 0)
                {
                    coroutines.RemoveAt(i);
                    i--;
                }
                else
                {
                    done = false;
                    coroutine.enumerator = coroutine.history[coroutine.history.Count - 1];
                    coroutine.history.RemoveAt(coroutine.history.Count - 1);
                }
            }
            else
            {
                if (coroutine.enumerator.Current is IEnumerator)
                {
                    coroutine.history.Add(coroutine.enumerator);
                    coroutine.enumerator = (IEnumerator)coroutine.enumerator.Current;
                }
            }
            if (coroutine.OnUpdate != null) coroutine.OnUpdate(done);
        }
        if (coroutines.Count == 0) EditorApplication.update -= Update;
    }

    internal static void StopAll()
    {
        coroutines.Clear();
        EditorApplication.update -= Update;
    }

}