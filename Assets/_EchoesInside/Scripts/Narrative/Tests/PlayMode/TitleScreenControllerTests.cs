using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class TitleScreenControllerTests
{
    private const string ControllerTypeName =
        "TitleScreenController, EchoesInside.Narrative";

    [Test]
    public void StartGame_WithEmptySceneName_LogsErrorAndKeepsButtonEnabled()
    {
        Type controllerType = RequireControllerType();
        GameObject buttonObject = CreateButtonObject();
        GameObject fadeObject = new GameObject(
            "FadeOverlay",
            typeof(CanvasGroup),
            typeof(ScreenFader));
        GameObject controllerObject = new GameObject("TitleScreenControllerTest");
        controllerObject.SetActive(false);

        try
        {
            Button button = buttonObject.GetComponent<Button>();
            Component controller = controllerObject.AddComponent(controllerType);
            SetPrivateField(controller, "startButton", button);
            SetPrivateField(controller, "screenFader", fadeObject.GetComponent<ScreenFader>());
            SetPrivateField(controller, "introSceneName", string.Empty);
            controllerObject.SetActive(true);

            LogAssert.Expect(
                LogType.Error,
                "TitleScreenController cannot start because Intro Scene Name is empty.");
            button.onClick.Invoke();

            Assert.That(button.interactable, Is.True);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(controllerObject);
            UnityEngine.Object.DestroyImmediate(fadeObject);
            UnityEngine.Object.DestroyImmediate(buttonObject);
        }
    }

    [Test]
    public void StartGame_WithoutScreenFader_LogsErrorAndKeepsButtonEnabled()
    {
        string sceneName = GetBuildSceneName();
        Type controllerType = RequireControllerType();
        GameObject buttonObject = CreateButtonObject();
        GameObject controllerObject = new GameObject("TitleScreenControllerTest");
        controllerObject.SetActive(false);

        try
        {
            Button button = buttonObject.GetComponent<Button>();
            Component controller = controllerObject.AddComponent(controllerType);
            SetPrivateField(controller, "startButton", button);
            SetPrivateField(controller, "introSceneName", sceneName);
            controllerObject.SetActive(true);

            LogAssert.Expect(
                LogType.Error,
                "TitleScreenController cannot start because Screen Fader is not assigned.");
            button.onClick.Invoke();

            Assert.That(button.interactable, Is.True);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(controllerObject);
            UnityEngine.Object.DestroyImmediate(buttonObject);
        }
    }

    [UnityTest]
    public IEnumerator ValidButtonClick_DisablesButtonAndDoesNotRestartFade()
    {
        string sceneName = GetBuildSceneName();
        Type controllerType = RequireControllerType();
        GameObject buttonObject = CreateButtonObject();
        GameObject fadeObject = new GameObject(
            "FadeOverlay",
            typeof(CanvasGroup),
            typeof(ScreenFader));
        GameObject controllerObject = new GameObject("TitleScreenControllerTest");
        controllerObject.SetActive(false);

        try
        {
            Button button = buttonObject.GetComponent<Button>();
            ScreenFader screenFader = fadeObject.GetComponent<ScreenFader>();
            SetPrivateField(screenFader, "fadeInOnStart", false);
            SetPrivateField(screenFader, "fadeDuration", 10f);

            Component controller = controllerObject.AddComponent(controllerType);
            SetPrivateField(controller, "startButton", button);
            SetPrivateField(controller, "screenFader", screenFader);
            SetPrivateField(controller, "introSceneName", sceneName);
            controllerObject.SetActive(true);

            button.onClick.Invoke();
            object firstFade = GetPrivateField(screenFader, "activeFade");

            Assert.That(button.interactable, Is.False);
            Assert.That(fadeObject.GetComponent<CanvasGroup>().blocksRaycasts, Is.True);
            Assert.That(firstFade, Is.Not.Null);

            button.onClick.Invoke();
            object secondFade = GetPrivateField(screenFader, "activeFade");
            Assert.That(secondFade, Is.SameAs(firstFade));

            yield return null;
        }
        finally
        {
            UnityEngine.Object.Destroy(controllerObject);
            UnityEngine.Object.Destroy(fadeObject);
            UnityEngine.Object.Destroy(buttonObject);
        }
    }

    private static Type RequireControllerType()
    {
        Type type = Type.GetType(ControllerTypeName);
        Assert.That(type, Is.Not.Null, "TitleScreenController has not been implemented yet.");
        return type;
    }

    private static GameObject CreateButtonObject()
    {
        return new GameObject(
            "StartButton",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button));
    }

    private static string GetBuildSceneName()
    {
        string path = SceneUtility.GetScenePathByBuildIndex(0);
        Assert.That(path, Is.Not.Empty, "Build Profile must contain at least one scene.");
        return Path.GetFileNameWithoutExtension(path);
    }

    private static object GetPrivateField(object target, string fieldName)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Missing field {fieldName}.");
        return field.GetValue(target);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Missing field {fieldName}.");
        field.SetValue(target, value);
    }
}
