using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class TitleScreenSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/TitleScreen.unity";
    private const string BackgroundPath =
        "Assets/_EchoesInside/Art/Backgrounds/TitleScreen/TitleScreenBackground.png";
    private const string FontPath =
        "Assets/_EchoesInside/Fonts/VT323-Regular SDF.asset";

    [MenuItem("Echoes Inside/Build Title Screen")]
    public static void BuildTitleScreen()
    {
        ConfigureBackgroundImporter();

        Sprite backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

        if (backgroundSprite == null)
        {
            throw new InvalidOperationException(
                $"Title-screen sprite was not found at {BackgroundPath}.");
        }

        if (font == null)
        {
            throw new InvalidOperationException(
                $"TMP font was not found at {FontPath}.");
        }

        Scene scene = EditorSceneManager.NewScene(
            NewSceneSetup.EmptyScene,
            NewSceneMode.Single);

        Canvas canvas = CreateCanvas();
        CreateBackground(canvas.transform, backgroundSprite);
        CreateTitle(canvas.transform, font);
        Button startButton = CreateStartButton(canvas.transform, font);
        ScreenFader screenFader = CreateFadeOverlay(canvas.transform);
        CreateController(startButton, screenFader);
        CreateEventSystem();

        EditorSceneManager.SaveScene(scene, ScenePath);
        PutTitleScreenFirstInBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        Debug.Log($"Title screen created at {ScenePath}.");
    }

    private static void ConfigureBackgroundImporter()
    {
        AssetDatabase.ImportAsset(BackgroundPath, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(BackgroundPath) as TextureImporter;

        if (importer == null)
        {
            throw new InvalidOperationException(
                $"Texture importer was not found for {BackgroundPath}.");
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;
        importer.SaveAndReimport();
    }

    private static Canvas CreateCanvas()
    {
        GameObject gameObject = new GameObject(
            "Canvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        Canvas canvas = gameObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    private static void CreateBackground(Transform parent, Sprite sprite)
    {
        GameObject gameObject = new GameObject(
            "Background",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(AspectRatioFitter));
        gameObject.transform.SetParent(parent, false);

        SetFullScreen(gameObject.GetComponent<RectTransform>());

        Image image = gameObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.raycastTarget = false;

        AspectRatioFitter fitter = gameObject.GetComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fitter.aspectRatio = sprite.rect.width / sprite.rect.height;
    }

    private static void CreateTitle(Transform parent, TMP_FontAsset font)
    {
        GameObject gameObject = new GameObject(
            "TitleText",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI),
            typeof(Shadow));
        gameObject.transform.SetParent(parent, false);

        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.82f);
        rect.anchorMax = new Vector2(0.5f, 0.82f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(1200f, 150f);

        TextMeshProUGUI text = gameObject.GetComponent<TextMeshProUGUI>();
        text.text = "ECHOES INSIDE";
        text.font = font;
        text.fontSize = 92f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = HtmlColor("#F3E7C8");
        text.raycastTarget = false;

        Shadow shadow = gameObject.GetComponent<Shadow>();
        shadow.effectColor = new Color(0.03f, 0.04f, 0.07f, 0.75f);
        shadow.effectDistance = new Vector2(3f, -3f);
        shadow.useGraphicAlpha = true;
    }

    private static Button CreateStartButton(Transform parent, TMP_FontAsset font)
    {
        GameObject gameObject = new GameObject(
            "StartButton",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button),
            typeof(Outline));
        gameObject.transform.SetParent(parent, false);

        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.25f);
        rect.anchorMax = new Vector2(0.5f, 0.25f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(300f, 76f);

        Image image = gameObject.GetComponent<Image>();
        image.color = HtmlColor("#17151AD9");

        Outline outline = gameObject.GetComponent<Outline>();
        outline.effectColor = HtmlColor("#A87A3C");
        outline.effectDistance = new Vector2(2f, -2f);
        outline.useGraphicAlpha = true;

        Button button = gameObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        button.navigation = new Navigation { mode = Navigation.Mode.Automatic };
        button.colors = new ColorBlock
        {
            normalColor = HtmlColor("#17151AD9"),
            highlightedColor = HtmlColor("#29242BF2"),
            pressedColor = HtmlColor("#0D0C0FFF"),
            selectedColor = HtmlColor("#29242BF2"),
            disabledColor = HtmlColor("#17151A80"),
            colorMultiplier = 1f,
            fadeDuration = 0.08f
        };

        GameObject labelObject = new GameObject(
            "Label",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(gameObject.transform, false);
        SetFullScreen(labelObject.GetComponent<RectTransform>());

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.text = "BẮT ĐẦU";
        label.font = font;
        label.fontSize = 42f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = HtmlColor("#F3E7C8");
        label.raycastTarget = false;
        return button;
    }

    private static ScreenFader CreateFadeOverlay(Transform parent)
    {
        GameObject gameObject = new GameObject(
            "FadeOverlay",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(CanvasGroup),
            typeof(ScreenFader));
        gameObject.transform.SetParent(parent, false);
        gameObject.transform.SetAsLastSibling();
        SetFullScreen(gameObject.GetComponent<RectTransform>());

        Image image = gameObject.GetComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = true;

        CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;

        ScreenFader screenFader = gameObject.GetComponent<ScreenFader>();
        SerializedObject serializedFader = new SerializedObject(screenFader);
        serializedFader.FindProperty("fadeDuration").floatValue = 1.2f;
        serializedFader.FindProperty("fadeInOnStart").boolValue = true;
        serializedFader.ApplyModifiedPropertiesWithoutUndo();
        return screenFader;
    }

    private static void CreateController(Button button, ScreenFader screenFader)
    {
        GameObject gameObject = new GameObject("TitleScreenController");
        TitleScreenController controller = gameObject.AddComponent<TitleScreenController>();

        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("startButton").objectReferenceValue = button;
        serializedController.FindProperty("screenFader").objectReferenceValue = screenFader;
        serializedController.FindProperty("introSceneName").stringValue = "Intro_DinnerScene";
        serializedController.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateEventSystem()
    {
        Type inputModuleType = Type.GetType(
            "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");

        if (inputModuleType == null)
        {
            throw new InvalidOperationException(
                "InputSystemUIInputModule was not found. Verify that the Input System package is installed.");
        }

        new GameObject("EventSystem", typeof(EventSystem), inputModuleType);
    }

    private static void PutTitleScreenFirstInBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!string.Equals(scene.path, ScenePath, StringComparison.OrdinalIgnoreCase))
            {
                scenes.Add(scene);
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void SetFullScreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Color HtmlColor(string value)
    {
        if (!ColorUtility.TryParseHtmlString(value, out Color color))
        {
            throw new ArgumentException($"Invalid HTML color: {value}", nameof(value));
        }

        return color;
    }
}
