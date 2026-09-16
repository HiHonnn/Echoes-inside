using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class SceneTransitionLoaderTests
{
    private const string TypeName = "SceneTransitionLoader, EchoesInside.Narrative";

    [UnityTest]
    public IEnumerator LoadScene_WithEmptyName_LogsError()
    {
        Type type = RequireType();
        LogAssert.Expect(LogType.Error, "SceneTransitionLoader cannot load an empty scene name.");

        InvokeStatic(type, "LoadScene", string.Empty, null);
        yield return null;
    }

    [UnityTest]
    public IEnumerator LoadScene_WithScreenFader_StartsVisualFade()
    {
        string scenePath = SceneUtility.GetScenePathByBuildIndex(0);
        Assert.That(scenePath, Is.Not.Empty, "Build Profile must contain at least one scene.");
        string sceneName = Path.GetFileNameWithoutExtension(scenePath);

        Type type = RequireType();
        Type faderType = Type.GetType("ScreenFader, EchoesInside.Narrative");
        Assert.That(faderType, Is.Not.Null);
        GameObject gameObject = new GameObject("TransitionFaderTest", typeof(CanvasGroup));

        try
        {
            CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
            Component fader = gameObject.AddComponent(faderType);
            SetPrivateField(fader, "fadeInOnStart", false);
            SetPrivateField(fader, "fadeDuration", 10f);

            InvokeStatic(type, "LoadScene", sceneName, null);
            yield return null;

            Assert.That(canvasGroup.alpha, Is.GreaterThan(0f));
        }
        finally
        {
            UnityEngine.Object.Destroy(gameObject);
        }
    }

    private static Type RequireType()
    {
        Type type = Type.GetType(TypeName);
        Assert.That(type, Is.Not.Null, "SceneTransitionLoader has not been implemented yet.");
        return type;
    }

    private static void InvokeStatic(
        Type type,
        string methodName,
        params object[] arguments)
    {
        MethodInfo method = Array.Find(
            type.GetMethods(BindingFlags.Static | BindingFlags.Public),
            candidate => candidate.Name == methodName
                && candidate.GetParameters().Length == arguments.Length);

        Assert.That(method, Is.Not.Null, $"Missing public static method {methodName}.");
        method.Invoke(null, arguments);
    }

    private static void SetPrivateField(Component component, string fieldName, object value)
    {
        FieldInfo field = component.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Missing serialized field {fieldName}.");
        field.SetValue(component, value);
    }
}
