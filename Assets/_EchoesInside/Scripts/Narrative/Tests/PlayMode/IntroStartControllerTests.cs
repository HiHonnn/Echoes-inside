using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class IntroStartControllerTests
{
    private const string ControllerTypeName = "IntroStartController, EchoesInside.Narrative";

    [Test]
    public void BeginTransition_WithEmptySceneName_LogsErrorWithoutStartingFade()
    {
        Type controllerType = RequireControllerType();
        GameObject gameObject = new GameObject("IntroStartControllerTest", typeof(CanvasGroup));

        try
        {
            CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            ScreenFader screenFader = gameObject.AddComponent<ScreenFader>();
            SetPrivateField(screenFader, "fadeInOnStart", false);

            Component controller = gameObject.AddComponent(controllerType);
            SetPrivateField(controller, "screenFader", screenFader);
            SetPrivateField(controller, "mainMapSceneName", string.Empty);

            LogAssert.Expect(LogType.Error, "IntroStartController cannot transition because Main Map Scene Name is empty.");
            Invoke(controller, "BeginTransition");

            Assert.That(canvasGroup.alpha, Is.EqualTo(0f).Within(0.001f));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(gameObject);
        }
    }

    [Test]
    public void CutsceneFinishedEvent_InvokesTransitionValidation()
    {
        Type controllerType = RequireControllerType();
        GameObject gameObject = new GameObject("IntroStartControllerTest");
        gameObject.SetActive(false);

        try
        {
            StaticCutscenePlayer cutscenePlayer = gameObject.AddComponent<StaticCutscenePlayer>();
            Component controller = gameObject.AddComponent(controllerType);
            SetPrivateField(controller, "cutscenePlayer", cutscenePlayer);
            SetPrivateField(controller, "mainMapSceneName", string.Empty);

            gameObject.SetActive(true);

            LogAssert.Expect(LogType.Error, "IntroStartController cannot transition because Main Map Scene Name is empty.");
            cutscenePlayer.OnCutsceneFinished.Invoke();
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(gameObject);
        }
    }

    private static Type RequireControllerType()
    {
        Type type = Type.GetType(ControllerTypeName);
        Assert.That(type, Is.Not.Null, "IntroStartController has not been implemented yet.");
        return type;
    }

    private static void Invoke(Component component, string methodName)
    {
        MethodInfo method = component.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
        Assert.That(method, Is.Not.Null, $"Missing public method {methodName}.");
        method.Invoke(component, null);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Missing serialized field {fieldName}.");
        field.SetValue(target, value);
    }
}
