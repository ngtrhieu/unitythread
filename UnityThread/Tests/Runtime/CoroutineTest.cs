using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityEngine.Tests {
  public class CoroutineTest {

    private int counter = 0;

    private IEnumerator ACoroutine() {
      while (counter < 5) {
        yield return new WaitForSeconds(0.3f);
        counter++;
      }
    }

    [UnityTest]
    public IEnumerator CoroutineExecutedNextUpdate() {
      UnityThread.ExecuteCoroutine(ACoroutine());

      // Runner is spawned
      var runner = GameObject.Find("UnityThreadRunner");
      Assert.NotNull(runner);

      // Counter not yet modified
      Assert.AreEqual(0, counter);

      yield return null;

      // 0.3 sec has not passed, counter should not been increased
      Assert.AreEqual(0, counter);

      yield return new WaitForSeconds(0.3f);
      Assert.AreEqual(1, counter);
      yield return new WaitForSeconds(0.4f);
      Assert.AreEqual(2, counter);
      yield return new WaitForSeconds(0.6f);
      Assert.AreEqual(4, counter);
      yield return new WaitForSeconds(1f);
      Assert.AreEqual(5, counter);
    }
  }
}
