using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ChapterMusicStatePlayerTests
{
    private const string SelectorTypeName = "ChapterMusicStatePlayer, Assembly-CSharp";
    private const string MusicTypeName = "SceneMusicPlayer, EchoesInside.Narrative";
    private const string GameManagerTypeName = "GameManager, Assembly-CSharp";

    [UnityTest]
    public IEnumerator Grandma_FramePickedUp_SelectsHealedClip()
    {
        yield return VerifyStateSelectsHealedClip(
            "Grandma",
            "chapter1State",
            "framePickedUp");
    }

    [UnityTest]
    public IEnumerator Father_MazeCompleted_SelectsHealedClip()
    {
        yield return VerifyStateSelectsHealedClip(
            "Father",
            "chapter2State",
            "mazeCompleted");
    }

    [UnityTest]
    public IEnumerator Mother_MealCompleted_SelectsHealedClip()
    {
        yield return VerifyStateSelectsHealedClip(
            "Mother",
            "chapter3State",
            "mealCompleted");
    }

    [UnityTest]
    public IEnumerator GoBackToBrightRoom_RequestsHealedMusic()
    {
        Type selectorType = RequireType(SelectorTypeName);
        Type musicType = RequireType(MusicTypeName);
        Type gameManagerType = RequireType(GameManagerTypeName);
        Type handlerType = RequireType("PuzzleCompleteHandler, Assembly-CSharp");
        Type chapterType = selectorType.Assembly.GetType("HealingMusicChapter");
        Assert.That(chapterType, Is.Not.Null);
        ResetGameManagerSingleton(gameManagerType);

        AudioClip beforeClip = AudioClip.Create(
            "GrandmaBefore",
            4410,
            1,
            44100,
            false);
        AudioClip healedClip = AudioClip.Create(
            "GrandmaHealed",
            4410,
            1,
            44100,
            false);
        GameObject managerObject = new GameObject("GameManagerTest");
        GameObject musicObject = new GameObject("ChapterMusicTest");
        GameObject handlerObject = new GameObject("PuzzleCompleteHandlerTest");
        musicObject.SetActive(false);

        try
        {
            managerObject.AddComponent(gameManagerType);

            AudioSource source = musicObject.AddComponent<AudioSource>();
            Component musicPlayer = musicObject.AddComponent(musicType);
            SetField(musicPlayer, "playOnStart", false);
            SetField(musicPlayer, "fadeInDuration", 0f);

            Component selector = musicObject.AddComponent(selectorType);
            SetField(selector, "chapter", Enum.Parse(chapterType, "Grandma"));
            SetField(selector, "beforeHealingClip", beforeClip);
            SetField(selector, "healedClip", healedClip);
            SetField(selector, "transitionDuration", 0f);

            Component handler = handlerObject.AddComponent(handlerType);
            SetField(handler, "chapterMusic", selector);

            musicObject.SetActive(true);
            yield return null;

            musicType.GetMethod("Play", BindingFlags.Instance | BindingFlags.Public)
                .Invoke(musicPlayer, null);
            handlerType.GetMethod(
                    "GoBackToBrightRoom",
                    BindingFlags.Instance | BindingFlags.Public)
                .Invoke(handler, null);
            yield return null;

            Assert.That(source.clip, Is.SameAs(healedClip));
        }
        finally
        {
            UnityEngine.Object.Destroy(handlerObject);
            UnityEngine.Object.Destroy(musicObject);
            UnityEngine.Object.Destroy(managerObject);
            UnityEngine.Object.Destroy(beforeClip);
            UnityEngine.Object.Destroy(healedClip);
            ResetGameManagerSingleton(gameManagerType);
        }
    }

    private static IEnumerator VerifyStateSelectsHealedClip(
        string chapterName,
        string stateFieldName,
        string completionFieldName)
    {
        Type selectorType = RequireType(SelectorTypeName);
        Type musicType = RequireType(MusicTypeName);
        Type gameManagerType = RequireType(GameManagerTypeName);
        ResetGameManagerSingleton(gameManagerType);

        AudioClip beforeClip = AudioClip.Create("Before", 4410, 1, 44100, false);
        AudioClip healedClip = AudioClip.Create("Healed", 4410, 1, 44100, false);
        GameObject managerObject = new GameObject("GameManagerTest");
        GameObject musicObject = new GameObject("ChapterMusicTest");
        musicObject.SetActive(false);

        try
        {
            Component manager = managerObject.AddComponent(gameManagerType);
            object state = GetField(manager, stateFieldName);
            SetField(state, completionFieldName, true);

            musicObject.AddComponent<AudioSource>();
            Component musicPlayer = musicObject.AddComponent(musicType);
            SetField(musicPlayer, "playOnStart", false);
            Component selector = musicObject.AddComponent(selectorType);
            Type chapterType = selectorType.Assembly.GetType("HealingMusicChapter");
            Assert.That(chapterType, Is.Not.Null);
            SetField(selector, "chapter", Enum.Parse(chapterType, chapterName));
            SetField(selector, "beforeHealingClip", beforeClip);
            SetField(selector, "healedClip", healedClip);

            musicObject.SetActive(true);
            yield return null;

            Assert.That(GetField(musicPlayer, "musicClip"), Is.SameAs(healedClip));
        }
        finally
        {
            UnityEngine.Object.Destroy(musicObject);
            UnityEngine.Object.Destroy(managerObject);
            UnityEngine.Object.Destroy(beforeClip);
            UnityEngine.Object.Destroy(healedClip);
            ResetGameManagerSingleton(gameManagerType);
        }
    }

    private static Type RequireType(string typeName)
    {
        Type type = Type.GetType(typeName);
        Assert.That(type, Is.Not.Null, $"Missing type {typeName}.");
        return type;
    }

    private static object GetField(object target, string fieldName)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Missing field {fieldName}.");
        return field.GetValue(target);
    }

    private static void SetField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Missing field {fieldName}.");
        field.SetValue(target, value);
    }

    private static void ResetGameManagerSingleton(Type gameManagerType)
    {
        FieldInfo instanceField = gameManagerType.GetField(
            "_instance",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(instanceField, Is.Not.Null);
        instanceField.SetValue(null, null);
    }
}
