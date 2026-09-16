using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SceneMusicPlayerTests
{
    private const string TypeName = "SceneMusicPlayer, EchoesInside.Narrative";

    [UnityTest]
    public IEnumerator Start_WithZeroFade_ConfiguresClipLoopAndTargetVolume()
    {
        Type type = RequireType();
        GameObject gameObject = new GameObject("SceneMusicPlayerTest", typeof(AudioSource));
        AudioClip clip = AudioClip.Create("TestMusic", 4410, 1, 44100, false);

        try
        {
            AudioSource source = gameObject.GetComponent<AudioSource>();
            Component player = gameObject.AddComponent(type);
            SetPrivateField(player, "musicClip", clip);
            SetPrivateField(player, "targetVolume", 0.3f);
            SetPrivateField(player, "fadeInDuration", 0f);
            SetPrivateField(player, "loop", true);
            SetPrivateField(player, "playOnStart", true);

            yield return null;

            Assert.That(source.clip, Is.SameAs(clip));
            Assert.That(source.loop, Is.True);
            Assert.That(source.playOnAwake, Is.False);
            Assert.That(source.volume, Is.EqualTo(0.3f).Within(0.001f));
        }
        finally
        {
            UnityEngine.Object.Destroy(gameObject);
            UnityEngine.Object.Destroy(clip);
        }
    }

    [UnityTest]
    public IEnumerator FadeOut_WithZeroDuration_MutesSourceImmediately()
    {
        Type type = RequireType();
        GameObject gameObject = new GameObject("SceneMusicFadeOutTest", typeof(AudioSource));

        try
        {
            AudioSource source = gameObject.GetComponent<AudioSource>();
            source.volume = 0.6f;
            Component player = gameObject.AddComponent(type);
            SetPrivateField(player, "playOnStart", false);

            Invoke(player, "FadeOut", 0f);
            yield return null;

            Assert.That(source.volume, Is.EqualTo(0f).Within(0.001f));
        }
        finally
        {
            UnityEngine.Object.Destroy(gameObject);
        }
    }

    [UnityTest]
    public IEnumerator Play_WithoutAnyClip_LogsWarningAndLeavesSourceSilent()
    {
        Type type = RequireType();
        GameObject gameObject = new GameObject("SceneMusicMissingClipTest", typeof(AudioSource));

        try
        {
            AudioSource source = gameObject.GetComponent<AudioSource>();
            Component player = gameObject.AddComponent(type);
            SetPrivateField(player, "playOnStart", false);

            LogAssert.Expect(
                LogType.Warning,
                "SceneMusicPlayer has no music clip assigned.");
            Invoke(player, "Play");
            yield return null;

            Assert.That(source.clip, Is.Null);
            Assert.That(source.isPlaying, Is.False);
        }
        finally
        {
            UnityEngine.Object.Destroy(gameObject);
        }
    }

    [UnityTest]
    public IEnumerator SetClip_BeforeStart_UsesSelectedClip()
    {
        Type type = RequireType();
        GameObject gameObject = new GameObject("SceneMusicSelectionTest", typeof(AudioSource));
        AudioClip selectedClip = AudioClip.Create("SelectedMusic", 4410, 1, 44100, false);

        try
        {
            AudioSource source = gameObject.GetComponent<AudioSource>();
            Component player = gameObject.AddComponent(type);
            SetPrivateField(player, "playOnStart", false);

            Invoke(player, "SetClip", selectedClip);
            Invoke(player, "Play");
            yield return null;

            Assert.That(source.clip, Is.SameAs(selectedClip));
        }
        finally
        {
            UnityEngine.Object.Destroy(gameObject);
            UnityEngine.Object.Destroy(selectedClip);
        }
    }

    [UnityTest]
    public IEnumerator TransitionTo_WithZeroDuration_ReplacesClipImmediately()
    {
        Type type = RequireType();
        GameObject gameObject = new GameObject("SceneMusicTransitionTest", typeof(AudioSource));
        AudioClip firstClip = AudioClip.Create("FirstMusic", 4410, 1, 44100, false);
        AudioClip nextClip = AudioClip.Create("NextMusic", 4410, 1, 44100, false);

        try
        {
            AudioSource source = gameObject.GetComponent<AudioSource>();
            Component player = gameObject.AddComponent(type);
            SetPrivateField(player, "playOnStart", false);
            SetPrivateField(player, "fadeInDuration", 0f);
            SetPrivateField(player, "targetVolume", 0.25f);

            Invoke(player, "SetClip", firstClip);
            Invoke(player, "Play");
            Invoke(player, "TransitionTo", nextClip, 0f);
            yield return null;

            Assert.That(source.clip, Is.SameAs(nextClip));
            Assert.That(source.volume, Is.EqualTo(0.25f).Within(0.001f));
        }
        finally
        {
            UnityEngine.Object.Destroy(gameObject);
            UnityEngine.Object.Destroy(firstClip);
            UnityEngine.Object.Destroy(nextClip);
        }
    }

    [UnityTest]
    public IEnumerator Play_WhenSameClipIsAlreadyPlaying_DoesNotResetVolume()
    {
        Type type = RequireType();
        GameObject gameObject = new GameObject("SceneMusicDuplicatePlayTest", typeof(AudioSource));
        AudioClip clip = AudioClip.Create("LoopingMusic", 44100, 1, 44100, false);

        try
        {
            AudioSource source = gameObject.GetComponent<AudioSource>();
            Component player = gameObject.AddComponent(type);
            SetPrivateField(player, "playOnStart", false);
            SetPrivateField(player, "fadeInDuration", 0f);
            SetPrivateField(player, "targetVolume", 0.3f);

            Invoke(player, "SetClip", clip);
            Invoke(player, "Play");
            source.volume = 0.18f;
            Invoke(player, "Play");
            yield return null;

            Assert.That(source.volume, Is.EqualTo(0.18f).Within(0.001f));
        }
        finally
        {
            UnityEngine.Object.Destroy(gameObject);
            UnityEngine.Object.Destroy(clip);
        }
    }

    private static Type RequireType()
    {
        Type type = Type.GetType(TypeName);
        Assert.That(type, Is.Not.Null, "SceneMusicPlayer has not been implemented yet.");
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
        FieldInfo field = component.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Missing serialized field {fieldName}.");
        field.SetValue(component, value);
    }
}
