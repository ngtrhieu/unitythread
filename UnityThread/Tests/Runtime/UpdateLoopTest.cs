using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityEngine.Tests {
  public class UpdateLoopTest {

    [SetUp]
    public void SetUp() {
      var runner = GameObject.Find("UnityThreadRunner");
      if (runner != null)
        Object.Destroy(runner);
    }

    [UnityTest]
    public IEnumerator ActionExecuteNextUpdate() {
      var counter = 0;
      UnityThread.ExecuteInUpdate(() => {
        ++counter;
        Debug.Log("Update loop");
      });

      // Runner is spawned
      var runner = GameObject.Find("UnityThreadRunner");
      Assert.NotNull(runner);

      // Counter not yet modified
      Assert.AreEqual(0, counter);

      yield return null;

      // Logged
      LogAssert.Expect(LogType.Log, "Update loop");

      // Counter increased
      Assert.AreEqual(1, counter);

      yield return null;

      // Counter not increased any further
      Assert.AreEqual(1, counter);
    }

    [UnityTest]
    public IEnumerator ActionExecuteNextLateUpdate() {

      // Wait for a new frame
      yield return new WaitForEndOfFrame();

      var counter = 0;
      UnityThread.ExecuteInUpdate(() => {
        ++counter;
        Debug.Log("Update loop");
      });
      UnityThread.ExecuteInLateUpdate(() => {
        counter = counter * 2;
        Debug.Log("LateUpdate loop");
      });

      // Runner is spawned
      var runner = GameObject.Find("UnityThreadRunner");
      Assert.NotNull(runner);

      yield return null;
      LogAssert.Expect(LogType.Log, "Update loop");
      Assert.AreEqual(1, counter);

      yield return new WaitForEndOfFrame();
      LogAssert.Expect(LogType.Log, "LateUpdate loop");
      Assert.AreEqual(2, counter);
    }

    [UnityTest]
    public IEnumerator ActionExecuteNextFixedUpdate() {

      // Wait for a new fixed frame
      yield return new WaitForFixedUpdate();

      Time.fixedDeltaTime = 0.33f;

      var counter = 0;
      UnityThread.ExecuteInFixedUpdate(() => {
        counter += 10;
        Debug.Log("FixedUpdate loop");
      });

      // Runner is spawned
      var runner = GameObject.Find("UnityThreadRunner");
      Assert.NotNull(runner);

      yield return null;
      Assert.AreEqual(0, counter);

      yield return new WaitForEndOfFrame();
      Assert.AreEqual(0, counter);

      yield return new WaitForFixedUpdate();
      LogAssert.Expect(LogType.Log, "FixedUpdate loop");
      Assert.AreEqual(10, counter);
    }
  }
}
