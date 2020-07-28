using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityEngine.Tests {
  public class MultithreadTest {

    [SetUp]
    public void SetUp() {
      var runner = GameObject.Find("UnityThreadRunner");
      if (runner != null)
        Object.Destroy(runner);
    }

    [UnityTest]
    public IEnumerator MustFirstInstantiateUnityThreadOnMainThread() {
      // Must initialize UnityThread on the main thread
      UnityThread.CreateUnityThreadInstance();

      var runner = GameObject.Find("UnityThreadRunner");
      Assert.NotNull(runner);

      yield return new WaitForEndOfFrame();
      var count = 0;
      for (var i = 0; i < 10; ++i)
        Task.Run(() => UnityThread.ExecuteInUpdate(() => ++count));
      Assert.AreEqual(0, count);

      // Wait at least 2 frames, since Task.Run will not guarantee it will run within this frame
      yield return new WaitForEndOfFrame();
      yield return new WaitForEndOfFrame();
      Assert.AreEqual(10, count);
    }

    [UnityTest]
    public IEnumerator UnityThreadWillNotRunWhenCallOnSideThread() {
      try {
        Task.Run(() => UnityThread.CreateUnityThreadInstance()).Wait();
        Assert.Fail("Expect exception to be thrown as UnityThread trying to initialize on a side thread");
      } catch (AggregateException e) {
        Assert.True(
          e.GetBaseException().Message.ToLower().Contains("can only be called from the main thread")
        );
      }
      yield return null;
    }
  }
}