using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class ScreenFaderTests
{
    private const string ScreenFaderTypeName = "ScreenFader, EchoesInside.Narrative";

    [Test]
    public void DefaultFadeCurve_EasesAtBeginningAndEnd()
    {
        Type screenFaderType = RequireScreenFaderType();
        GameObject gameObject = new GameObject("ScreenFaderTest", typeof(CanvasGroup));

        try
        {
            Component screenFader = gameObject.AddComponent(screenFaderType);
            FieldInfo field = screenFaderType.GetField(
                "fadeCurve",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(field, Is.Not.Null, "Missing serialized field fadeCurve.");

            AnimationCurve fadeCurve = field.GetValue(screenFader) as AnimationCurve;
            Assert.That(fadeCurve, Is.Not.Null);
            Assert.That(fadeCurve.Evaluate(0.25f), Is.LessThan(0.25f));
            Assert.That(fadeCurve.Evaluate(0.75f), Is.GreaterThan(0.75f));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(gameObject);
        }
    }

    [UnityTest]
    public IEnumerator FadeIn_WithZeroDuration_RevealsScreenAndReleasesInput()
    {
        yield return ExerciseFade(
            methodName: "FadeIn",
            startingAlpha: 1f,
            expectedAlpha: 0f,
            expectedBlocksRaycasts: false);
    }

    [UnityTest]
    public IEnumerator FadeOut_WithZeroDuration_CoversScreenAndBlocksInput()
    {
        yield return ExerciseFade(
            methodName: "FadeOut",
            startingAlpha: 0f,
            expectedAlpha: 1f,
            expectedBlocksRaycasts: true);
    }

    [UnityTest]
    public IEnumerator FadeCallbacks_WithZeroDuration_RunAfterAlphaIsApplied()
    {
        Type screenFaderType = RequireScreenFaderType();
        GameObject gameObject = new GameObject("ScreenFaderCallbackTest", typeof(CanvasGroup));

        try
        {
            CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
            Component screenFader = gameObject.AddComponent(screenFaderType);
            SetPrivateField(screenFader, "fadeDuration", 0f);
            SetPrivateField(screenFader, "fadeInOnStart", false);

            bool fadeOutCompleted = false;
            Invoke(screenFader, "FadeOut", new Action(() =>
            {
                Assert.That(canvasGroup.alpha, Is.EqualTo(1f).Within(0.001f));
                fadeOutCompleted = true;
            }));

            bool fadeInCompleted = false;
            Invoke(screenFader, "FadeIn", new Action(() =>
            {
                Assert.That(canvasGroup.alpha, Is.EqualTo(0f).Within(0.001f));
                fadeInCompleted = true;
            }));

            yield return null;

            Assert.That(fadeOutCompleted, Is.True);
            Assert.That(fadeInCompleted, Is.True);
        }
        finally
        {
            UnityEngine.Object.Destroy(gameObject);
        }
    }

    [UnityTest]
    public IEnumerator FadeOutAndLoadScene_WithEmptyName_LogsErrorWithoutStartingFade()
    {
        Type screenFaderType = RequireScreenFaderType();
        GameObject gameObject = new GameObject("ScreenFaderTest", typeof(CanvasGroup));

        try
        {
            CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            Component screenFader = gameObject.AddComponent(screenFaderType);
            SetPrivateField(screenFader, "fadeInOnStart", false);

            LogAssert.Expect(LogType.Error, "ScreenFader cannot load a scene because the scene name is empty.");
            Invoke(screenFader, "FadeOutAndLoadScene", string.Empty);
            yield return null;

            Assert.That(canvasGroup.alpha, Is.EqualTo(0f).Within(0.001f));
            Assert.That(canvasGroup.blocksRaycasts, Is.False);
        }
        finally
        {
            UnityEngine.Object.Destroy(gameObject);
        }
    }

    [UnityTest]
    public IEnumerator FadeOutAndLoadScene_WithSceneMusic_StartsMusicFadeOut()
    {
        string scenePath = SceneUtility.GetScenePathByBuildIndex(0);
        Assert.That(scenePath, Is.Not.Empty, "Build Profile must contain at least one scene.");
        string sceneName = Path.GetFileNameWithoutExtension(scenePath);

        Type screenFaderType = RequireScreenFaderType();
        Type musicPlayerType = Type.GetType("SceneMusicPlayer, EchoesInside.Narrative");
        Assert.That(musicPlayerType, Is.Not.Null);

        GameObject musicObject = new GameObject("SceneMusicTest", typeof(AudioSource));
        GameObject fadeObject = new GameObject("ScreenFaderTest", typeof(CanvasGroup));

        try
        {
            AudioSource source = musicObject.GetComponent<AudioSource>();
            source.volume = 1f;
            Component musicPlayer = musicObject.AddComponent(musicPlayerType);
            SetPrivateField(musicPlayer, "playOnStart", false);

            Component screenFader = fadeObject.AddComponent(screenFaderType);
            SetPrivateField(screenFader, "fadeInOnStart", false);
            SetPrivateField(screenFader, "fadeDuration", 10f);

            Invoke(screenFader, "FadeOutAndLoadScene", sceneName);
            yield return null;

            Assert.That(source.volume, Is.LessThan(1f));
        }
        finally
        {
            UnityEngine.Object.Destroy(fadeObject);
            UnityEngine.Object.Destroy(musicObject);
        }
    }

    private static IEnumerator ExerciseFade(
        string methodName,
        float startingAlpha,
        float expectedAlpha,
        bool expectedBlocksRaycasts)
    {
        Type screenFaderType = RequireScreenFaderType();
        GameObject gameObject = new GameObject("ScreenFaderTest", typeof(CanvasGroup));

        try
        {
            CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
            canvasGroup.alpha = startingAlpha;
            Component screenFader = gameObject.AddComponent(screenFaderType);
            SetPrivateField(screenFader, "fadeDuration", 0f);
            SetPrivateField(screenFader, "fadeInOnStart", false);

            Invoke(screenFader, methodName);
            yield return null;

            Assert.That(canvasGroup.alpha, Is.EqualTo(expectedAlpha).Within(0.001f));
            Assert.That(canvasGroup.blocksRaycasts, Is.EqualTo(expectedBlocksRaycasts));
        }
        finally
        {
            UnityEngine.Object.Destroy(gameObject);
        }
    }

    private static Type RequireScreenFaderType()
    {
        Type type = Type.GetType(ScreenFaderTypeName);
        Assert.That(type, Is.Not.Null, "ScreenFader has not been implemented yet.");
        return type;
    }

    private static void Invoke(Component component, string methodName, params object[] arguments)
    {
        MethodInfo method = Array.Find(
            component.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public),
            candidate => candidate.Name == methodName
                && candidate.GetParameters().Length == arguments.Length);

        Assert.That(method, Is.Not.Null, $"Missing public method {methodName}.");
        method.Invoke(component, arguments);
    }

    private static void SetPrivateField(Component component, string fieldName, object value)
    {
        FieldInfo field = component.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Missing serialized field {fieldName}.");
        field.SetValue(component, value);
    }
}
