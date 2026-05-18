using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace Xxhq.Htmltougui.Editor.Tests 
{
	
	class EditorExampleTest 
	{

		[Test]
		public void EditorSampleTestSimplePasses()
        {
           var go = UguiElementFactory.CreateViaMenu(
                    true ? "GameObject/UI/Legacy/Text" : "GameObject/UI/Text - TextMeshPro",
                    null, "Text");
            // Use the Assert class to test conditions.
        }

		// A UnityTest behaves like a coroutine in PlayMode
		// and allows you to yield null to skip a frame in EditMode
		[UnityTest]
		public IEnumerator EditorSampleTestWithEnumeratorPasses() 
		{
			// Use the Assert class to test conditions.
			// yield to skip a frame
			yield return null;
		}
	}
}