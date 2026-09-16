using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class StaticCutscenePlayerTests
{
    private const string PlayerTypeName = "StaticCutscenePlayer, EchoesInside.Narrative";
    private const string LineTypeName = "CutsceneLine, EchoesInside.Narrative";

    [Test]
    public void Play_DisplaysFirstLineAtItsAnchor()
    {
        CutsceneTestObjects objects = CreateCutsceneWithOneLine();

        try
        {
            Invoke(objects.Player, "Play");

            Assert.That(objects.Panel.gameObject.activeSelf, Is.True);
            Assert.That(objects.SpeakerText.text, Is.EqualTo("Bà"));
            Assert.That(objects.ContentText.text, Is.EqualTo("Bữa cơm đã nguội rồi."));
            Assert.That(objects.Panel.position, Is.EqualTo(objects.Anchor.position));
            Assert.That(GetPublicProperty<bool>(objects.Player, "IsPlaying"), Is.True);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(objects.Root);
        }
    }

    [Test]
    public void Advance_AfterFinalLine_HidesPanelAndRaisesFinishedEvent()
    {
        CutsceneTestObjects objects = CreateCutsceneWithOneLine();
        bool finished = false;

        try
        {
            UnityEvent finishedEvent = new UnityEvent();
            finishedEvent.AddListener(() => finished = true);
            SetPrivateField(objects.Player, "onCutsceneFinished", finishedEvent);

            Invoke(objects.Player, "Play");
            Invoke(objects.Player, "Advance");

            Assert.That(objects.Panel.gameObject.activeSelf, Is.False);
            Assert.That(GetPublicProperty<bool>(objects.Player, "IsPlaying"), Is.False);
            Assert.That(finished, Is.True);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(objects.Root);
        }
    }

    [Test]
    public void Play_WithTransparentLineColor_KeepsSpeakerTextDefaultColor()
    {
        CutsceneTestObjects objects = CreateCutsceneWithOneLine();
        Color defaultColor = new Color(1f, 0.8f, 0.3f, 1f);

        try
        {
            objects.SpeakerText.color = defaultColor;

            FieldInfo linesField = objects.Player.GetType().GetField(
                "lines",
                BindingFlags.Instance | BindingFlags.NonPublic);
            IList lines = (IList)linesField.GetValue(objects.Player);
            SetPrivateField(lines[0], "speakerColor", Color.clear);

            Invoke(objects.Player, "Play");

            Assert.That(objects.SpeakerText.color, Is.EqualTo(defaultColor));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(objects.Root);
        }
    }

    [Test]
    public void Play_WithLineBackgroundAndZeroFade_ChangesSprite()
    {
        CutsceneTestObjects objects = CreateCutsceneWithOneLine();
        GameObject backgroundObject = new GameObject(
            "Background",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        backgroundObject.transform.SetParent(objects.Root.transform, false);
        Texture2D oldTexture = new Texture2D(2, 2);
        Texture2D newTexture = new Texture2D(2, 2);
        Sprite oldSprite = CreateSprite(oldTexture);
        Sprite newSprite = CreateSprite(newTexture);

        try
        {
            Image backgroundImage = backgroundObject.GetComponent<Image>();
            backgroundImage.sprite = oldSprite;
            SetPrivateField(objects.Player, "backgroundImage", backgroundImage);
            SetPrivateField(objects.Player, "backgroundFadeDuration", 0f);

            IList lines = GetLines(objects.Player);
            SetPrivateField(lines[0], "backgroundSprite", newSprite);

            Invoke(objects.Player, "Play");

            Assert.That(backgroundImage.sprite, Is.SameAs(newSprite));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(objects.Root);
            UnityEngine.Object.DestroyImmediate(oldSprite);
            UnityEngine.Object.DestroyImmediate(newSprite);
            UnityEngine.Object.DestroyImmediate(oldTexture);
            UnityEngine.Object.DestroyImmediate(newTexture);
        }
    }

    [UnityTest]
    public IEnumerator Advance_DuringBackgroundFade_DoesNotSkipLine()
    {
        CutsceneTestObjects objects = CreateCutsceneWithOneLine();
        GameObject backgroundObject = new GameObject(
            "Background",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        backgroundObject.transform.SetParent(objects.Root.transform, false);
        Texture2D texture = new Texture2D(2, 2);
        Sprite newSprite = CreateSprite(texture);

        try
        {
            Image backgroundImage = backgroundObject.GetComponent<Image>();
            SetPrivateField(objects.Player, "backgroundImage", backgroundImage);
            SetPrivateField(objects.Player, "backgroundFadeDuration", 10f);

            IList lines = GetLines(objects.Player);
            SetPrivateField(lines[0], "backgroundSprite", newSprite);

            Invoke(objects.Player, "Play");
            Invoke(objects.Player, "Advance");

            Assert.That(
                GetPublicProperty<int>(objects.Player, "CurrentLineIndex"),
                Is.EqualTo(0));

            yield return null;
        }
        finally
        {
            UnityEngine.Object.Destroy(objects.Root);
            UnityEngine.Object.Destroy(newSprite);
            UnityEngine.Object.Destroy(texture);
        }
    }

    [UnityTest]
    public IEnumerator BackgroundFade_DarkensImageWithoutChangingAlpha()
    {
        CutsceneTestObjects objects = CreateCutsceneWithOneLine();
        GameObject backgroundObject = new GameObject(
            "Background",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        backgroundObject.transform.SetParent(objects.Root.transform, false);
        Texture2D oldTexture = new Texture2D(2, 2);
        Texture2D newTexture = new Texture2D(2, 2);
        Sprite oldSprite = CreateSprite(oldTexture);
        Sprite newSprite = CreateSprite(newTexture);
        Color originalColor = new Color(0.8f, 0.7f, 0.6f, 0.75f);

        try
        {
            Image backgroundImage = backgroundObject.GetComponent<Image>();
            backgroundImage.sprite = oldSprite;
            backgroundImage.color = originalColor;
            SetPrivateField(objects.Player, "backgroundImage", backgroundImage);
            SetPrivateField(objects.Player, "backgroundFadeDuration", 1f);
            SetPrivateField(objects.Player, "defaultBackgroundSprite", oldSprite);
            SetPrivateField(objects.Player, "defaultBackgroundColor", originalColor);

            IList lines = GetLines(objects.Player);
            SetPrivateField(lines[0], "backgroundSprite", newSprite);

            Invoke(objects.Player, "Play");
            yield return new WaitForSecondsRealtime(0.05f);

            Color transitionColor = backgroundImage.color;
            Assert.That(transitionColor.a, Is.EqualTo(originalColor.a).Within(0.001f));
            Assert.That(transitionColor.r, Is.LessThan(originalColor.r));
            Assert.That(transitionColor.g, Is.LessThan(originalColor.g));
            Assert.That(transitionColor.b, Is.LessThan(originalColor.b));
        }
        finally
        {
            UnityEngine.Object.Destroy(objects.Root);
            UnityEngine.Object.Destroy(oldSprite);
            UnityEngine.Object.Destroy(newSprite);
            UnityEngine.Object.Destroy(oldTexture);
            UnityEngine.Object.Destroy(newTexture);
        }
    }

    [Test]
    public void Play_AfterBackgroundChanged_RestoresOriginalSprite()
    {
        CutsceneTestObjects objects = CreateCutsceneWithOneLine();
        GameObject backgroundObject = new GameObject(
            "Background",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        backgroundObject.transform.SetParent(objects.Root.transform, false);
        Texture2D oldTexture = new Texture2D(2, 2);
        Texture2D newTexture = new Texture2D(2, 2);
        Sprite oldSprite = CreateSprite(oldTexture);
        Sprite newSprite = CreateSprite(newTexture);

        try
        {
            Image backgroundImage = backgroundObject.GetComponent<Image>();
            backgroundImage.sprite = oldSprite;
            SetPrivateField(objects.Player, "backgroundImage", backgroundImage);
            SetPrivateField(objects.Player, "backgroundFadeDuration", 0f);
            SetPrivateField(objects.Player, "defaultBackgroundSprite", oldSprite);
            SetPrivateField(objects.Player, "defaultBackgroundColor", backgroundImage.color);

            IList lines = GetLines(objects.Player);
            SetPrivateField(lines[0], "backgroundSprite", newSprite);
            Invoke(objects.Player, "Play");
            Assert.That(backgroundImage.sprite, Is.SameAs(newSprite));

            SetPrivateField(lines[0], "backgroundSprite", null);
            Invoke(objects.Player, "Play");
            Assert.That(backgroundImage.sprite, Is.SameAs(oldSprite));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(objects.Root);
            UnityEngine.Object.DestroyImmediate(oldSprite);
            UnityEngine.Object.DestroyImmediate(newSprite);
            UnityEngine.Object.DestroyImmediate(oldTexture);
            UnityEngine.Object.DestroyImmediate(newTexture);
        }
    }

    private static CutsceneTestObjects CreateCutsceneWithOneLine()
    {
        Type playerType = RequireType(PlayerTypeName, "StaticCutscenePlayer has not been implemented yet.");
        Type lineType = RequireType(LineTypeName, "CutsceneLine has not been implemented yet.");

        GameObject root = new GameObject("CutsceneTestRoot");
        RectTransform panel = CreateRectTransform("DialoguePanel", root.transform);
        TMP_Text speakerText = CreateText("SpeakerText", panel);
        TMP_Text contentText = CreateText("ContentText", panel);
        RectTransform anchor = CreateRectTransform("GrandmaAnchor", root.transform);
        anchor.position = new Vector3(123f, 234f, 0f);

        Component player = root.AddComponent(playerType);
        SetPrivateField(player, "dialoguePanel", panel);
        SetPrivateField(player, "speakerText", speakerText);
        SetPrivateField(player, "contentText", contentText);
        SetPrivateField(player, "playOnStart", false);

        object line = Activator.CreateInstance(lineType);
        SetPrivateField(line, "speakerName", "Bà");
        SetPrivateField(line, "content", "Bữa cơm đã nguội rồi.");
        SetPrivateField(line, "anchor", anchor);

        IList lines = (IList)Activator.CreateInstance(typeof(System.Collections.Generic.List<>).MakeGenericType(lineType));
        lines.Add(line);
        SetPrivateField(player, "lines", lines);

        return new CutsceneTestObjects(root, player, panel, speakerText, contentText, anchor);
    }

    private static RectTransform CreateRectTransform(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return gameObject.GetComponent<RectTransform>();
    }

    private static TMP_Text CreateText(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        gameObject.transform.SetParent(parent, false);
        return gameObject.GetComponent<TMP_Text>();
    }

    private static Type RequireType(string typeName, string message)
    {
        Type type = Type.GetType(typeName);
        Assert.That(type, Is.Not.Null, message);
        return type;
    }

    private static void Invoke(Component component, string methodName)
    {
        MethodInfo method = component.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
        Assert.That(method, Is.Not.Null, $"Missing public method {methodName}.");
        method.Invoke(component, null);
    }

    private static T GetPublicProperty<T>(Component component, string propertyName)
    {
        PropertyInfo property = component.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        Assert.That(property, Is.Not.Null, $"Missing public property {propertyName}.");
        return (T)property.GetValue(component);
    }

    private static IList GetLines(Component player)
    {
        FieldInfo field = player.GetType().GetField(
            "lines",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, "Missing serialized field lines.");
        return (IList)field.GetValue(player);
    }

    private static Sprite CreateSprite(Texture2D texture)
    {
        return Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f));
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Missing serialized field {fieldName}.");
        field.SetValue(target, value);
    }

    private sealed class CutsceneTestObjects
    {
        public readonly GameObject Root;
        public readonly Component Player;
        public readonly RectTransform Panel;
        public readonly TMP_Text SpeakerText;
        public readonly TMP_Text ContentText;
        public readonly RectTransform Anchor;

        public CutsceneTestObjects(
            GameObject root,
            Component player,
            RectTransform panel,
            TMP_Text speakerText,
            TMP_Text contentText,
            RectTransform anchor)
        {
            Root = root;
            Player = player;
            Panel = panel;
            SpeakerText = speakerText;
            ContentText = contentText;
            Anchor = anchor;
        }
    }
}
