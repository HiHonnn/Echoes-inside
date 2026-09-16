using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class DoorInteractionTests
{
    private const string DoorTypeName = "DoorInteraction, Assembly-CSharp";
    private const string ScreenFaderTypeName = "ScreenFader, EchoesInside.Narrative";

    [UnityTest]
    public IEnumerator TryEnterDoor_WhenFaderExists_StartsFadeBeforeLoadingScene()
    {
        Type doorType = RequireType(DoorTypeName);
        Type screenFaderType = RequireType(ScreenFaderTypeName);
        GameObject faderObject = new GameObject("DoorFaderTest", typeof(CanvasGroup));
        GameObject doorObject = new GameObject("DoorTest");

        try
        {
            faderObject.SetActive(false);
            CanvasGroup canvasGroup = faderObject.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            Component screenFader = faderObject.AddComponent(screenFaderType);
            SetPrivateField(screenFader, "fadeDuration", 60f);
            SetPrivateField(screenFader, "fadeInOnStart", false);
            UnityEngine.Object.DontDestroyOnLoad(faderObject);
            faderObject.SetActive(true);

            Component door = doorObject.AddComponent(doorType);
            SetPrivateField(door, "targetScene", "SampleScene");
            SetPrivateField(door, "isSelfDoor", false);

            yield return null;
            string startingScene = SceneManager.GetActiveScene().name;

            InvokePrivate(door, "TryEnterDoor");

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(startingScene),
                "Door loaded the target scene before the fade completed.");
            Assert.That(canvasGroup.blocksRaycasts, Is.True,
                "Door did not start the available ScreenFader.");
        }
        finally
        {
            UnityEngine.Object.Destroy(faderObject);
            UnityEngine.Object.Destroy(doorObject);
        }
    }

    private static Type RequireType(string typeName)
    {
        Type type = Type.GetType(typeName);
        Assert.That(type, Is.Not.Null, $"Could not load {typeName}.");
        return type;
    }

    private static void SetPrivateField(Component component, string fieldName, object value)
    {
        FieldInfo field = component.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(field, Is.Not.Null, $"Missing serialized field {fieldName}.");
        field.SetValue(component, value);
    }

    private static void InvokePrivate(Component component, string methodName)
    {
        MethodInfo method = component.GetType().GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(method, Is.Not.Null, $"Missing private method {methodName}.");
        method.Invoke(component, null);
    }
}
