using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;

public class SceneIntroDialogueTests
{
    private const string DialogueTypeName = "SceneIntroDialogue, EchoesInside.Narrative";

    [Test]
    public void PlayerControllerField_AcceptsOnlyMonoBehaviourComponents()
    {
        Type dialogueType = RequireDialogueType();
        FieldInfo field = dialogueType.GetField(
            "playerController",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(field, Is.Not.Null);
        Assert.That(field.FieldType, Is.EqualTo(typeof(MonoBehaviour)));
    }

    [Test]
    public void StartDialogue_ShowsFirstLineAndDisablesPlayerControl()
    {
        DialogueTestObjects objects = CreateDialogue(playOncePerSession: false);

        try
        {
            Invoke(objects.Dialogue, "StartDialogue");

            Assert.That(objects.Panel.activeSelf, Is.True);
            Assert.That(objects.SpeakerText.text, Is.EqualTo("Mình"));
            Assert.That(objects.ContentText.text, Is.EqualTo("Mình... đang ở đâu?"));
            Assert.That(objects.PlayerControl.enabled, Is.False);
            Assert.That(GetPublicProperty<bool>(objects.Dialogue, "IsPlaying"), Is.True);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(objects.Root);
        }
    }

    [Test]
    public void Advance_AfterFinalLine_HidesPanelAndRestoresPlayerControl()
    {
        DialogueTestObjects objects = CreateDialogue(playOncePerSession: false);

        try
        {
            Invoke(objects.Dialogue, "StartDialogue");
            Invoke(objects.Dialogue, "Advance");

            Assert.That(objects.Panel.activeSelf, Is.False);
            Assert.That(objects.PlayerControl.enabled, Is.True);
            Assert.That(GetPublicProperty<bool>(objects.Dialogue, "IsPlaying"), Is.False);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(objects.Root);
        }
    }

    [Test]
    public void StartDialogue_WhenAlreadyPlayedThisSession_DoesNotShowAgain()
    {
        Type dialogueType = RequireDialogueType();
        InvokeStatic(dialogueType, "ResetSessionState");

        DialogueTestObjects first = CreateDialogue(playOncePerSession: true, resetSession: false);
        Invoke(first.Dialogue, "StartDialogue");
        Invoke(first.Dialogue, "Advance");
        UnityEngine.Object.DestroyImmediate(first.Root);

        DialogueTestObjects second = CreateDialogue(playOncePerSession: true, resetSession: false);

        try
        {
            Invoke(second.Dialogue, "StartDialogue");

            Assert.That(second.Panel.activeSelf, Is.False);
            Assert.That(second.PlayerControl.enabled, Is.True);
            Assert.That(GetPublicProperty<bool>(second.Dialogue, "IsPlaying"), Is.False);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(second.Root);
        }
    }

    private static DialogueTestObjects CreateDialogue(
        bool playOncePerSession,
        bool resetSession = true)
    {
        Type dialogueType = RequireDialogueType();
        if (resetSession)
        {
            InvokeStatic(dialogueType, "ResetSessionState");
        }

        GameObject root = new GameObject("SceneIntroDialogueTest");
        root.SetActive(false);

        GameObject panel = new GameObject("DialoguePanel", typeof(RectTransform));
        panel.transform.SetParent(root.transform, false);
        TMP_Text speakerText = CreateText("SpeakerText", panel.transform);
        TMP_Text contentText = CreateText("ContentText", panel.transform);
        StaticCutscenePlayer playerControl = root.AddComponent<StaticCutscenePlayer>();
        Component dialogue = root.AddComponent(dialogueType);

        SetPrivateField(dialogue, "dialoguePanel", panel);
        SetPrivateField(dialogue, "speakerText", speakerText);
        SetPrivateField(dialogue, "contentText", contentText);
        SetPrivateField(dialogue, "playerController", playerControl);
        SetPrivateField(dialogue, "speakerName", "Mình");
        SetPrivateField(dialogue, "lines", new List<string> { "Mình... đang ở đâu?" });
        SetPrivateField(dialogue, "initialDelay", 0f);
        SetPrivateField(dialogue, "playOnStart", false);
        SetPrivateField(dialogue, "playOncePerSession", playOncePerSession);

        root.SetActive(true);
        return new DialogueTestObjects(root, dialogue, panel, speakerText, contentText, playerControl);
    }

    private static TMP_Text CreateText(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        gameObject.transform.SetParent(parent, false);
        return gameObject.GetComponent<TMP_Text>();
    }

    private static Type RequireDialogueType()
    {
        Type type = Type.GetType(DialogueTypeName);
        Assert.That(type, Is.Not.Null, "SceneIntroDialogue has not been implemented yet.");
        return type;
    }

    private static void Invoke(Component component, string methodName)
    {
        MethodInfo method = component.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
        Assert.That(method, Is.Not.Null, $"Missing public method {methodName}.");
        method.Invoke(component, null);
    }

    private static void InvokeStatic(Type type, string methodName)
    {
        MethodInfo method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null, $"Missing static method {methodName}.");
        method.Invoke(null, null);
    }

    private static T GetPublicProperty<T>(Component component, string propertyName)
    {
        PropertyInfo property = component.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        Assert.That(property, Is.Not.Null, $"Missing public property {propertyName}.");
        return (T)property.GetValue(component);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Missing serialized field {fieldName}.");
        field.SetValue(target, value);
    }

    private sealed class DialogueTestObjects
    {
        public readonly GameObject Root;
        public readonly Component Dialogue;
        public readonly GameObject Panel;
        public readonly TMP_Text SpeakerText;
        public readonly TMP_Text ContentText;
        public readonly MonoBehaviour PlayerControl;

        public DialogueTestObjects(
            GameObject root,
            Component dialogue,
            GameObject panel,
            TMP_Text speakerText,
            TMP_Text contentText,
            MonoBehaviour playerControl)
        {
            Root = root;
            Dialogue = dialogue;
            Panel = panel;
            SpeakerText = speakerText;
            ContentText = contentText;
            PlayerControl = playerControl;
        }
    }
}
