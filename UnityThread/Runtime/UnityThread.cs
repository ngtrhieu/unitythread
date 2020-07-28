using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine {

  ///<summary>
  ///Helper class to execute lambdas in Unity's main thread.
  ///</summary>
  public class UnityThread : MonoBehaviour {

    private static UnityThread current;

    private static List<System.Action> updateLoopQueue = new List<Action>();
    private static List<System.Action> lateUpdateLoopQueue = new List<Action>();
    private static List<System.Action> fixedUpdateLoopQueue = new List<Action>();

    private static List<System.Action> tempQueue = new List<System.Action>();

    public static UnityThread CreateUnityThreadInstance(bool visible = true) {
      if (current != null) return current;

      if (Application.isPlaying) {
        var obj = new GameObject("UnityThreadRunner");
        obj.hideFlags = visible ? HideFlags.HideAndDontSave : HideFlags.None;
        current = obj.AddComponent<UnityThread>();
      }
      return current;
    }

    private void Awake() {
      if (current == null || current == this)
        current = this;
      else {
        Destroy(gameObject);
        return;
      }

      DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy() {
      if (current == this) current = null;
    }

    private void Update() {
      if (updateLoopQueue.Count == 0)
        return;

      tempQueue.Clear();
      lock (updateLoopQueue) {
        tempQueue.AddRange(updateLoopQueue);
        updateLoopQueue.Clear();
      }

      foreach (var action in tempQueue)
        action.Invoke();
    }

    private void LateUpdate() {
      if (lateUpdateLoopQueue.Count == 0)
        return;

      tempQueue.Clear();
      lock (lateUpdateLoopQueue) {
        tempQueue.AddRange(lateUpdateLoopQueue);
        lateUpdateLoopQueue.Clear();
      }

      foreach (var action in tempQueue)
        action.Invoke();
    }

    private void FixedUpdate() {
      if (fixedUpdateLoopQueue.Count == 0)
        return;

      tempQueue.Clear();
      lock (fixedUpdateLoopQueue) {
        tempQueue.AddRange(fixedUpdateLoopQueue);
        fixedUpdateLoopQueue.Clear();
      }

      foreach (var action in tempQueue)
        action.Invoke();
    }

    public static void ExecuteCoroutine(IEnumerator action) {
      if (action == null)
        return;

      CreateUnityThreadInstance();

      ExecuteInUpdate(() => current.StartCoroutine(action));
    }

    public static void ExecuteInUpdate(System.Action action) {
      if (action == null)
        return;

      CreateUnityThreadInstance();

      lock (updateLoopQueue) {
        updateLoopQueue.Add(action);
      }
    }

    public static void ExecuteInLateUpdate(System.Action action) {
      if (action == null)
        return;

      CreateUnityThreadInstance();

      lock (lateUpdateLoopQueue) {
        lateUpdateLoopQueue.Add(action);
      }
    }

    public static void ExecuteInFixedUpdate(System.Action action) {
      if (action == null)
        return;

      CreateUnityThreadInstance();

      lock (fixedUpdateLoopQueue) {
        fixedUpdateLoopQueue.Add(action);
      }
    }

  }
}