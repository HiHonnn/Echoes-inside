using System;
using UnityEngine;

[Serializable]
public sealed class CutsceneLine
{
    [SerializeField] private string speakerName;
    [SerializeField, TextArea(2, 5)] private string content;
    [SerializeField] private RectTransform anchor;
    [SerializeField] private Color speakerColor = Color.white;
    [SerializeField] private AudioClip soundEffect;
    [SerializeField] private Sprite backgroundSprite;

    public string SpeakerName => speakerName;
    public string Content => content;
    public RectTransform Anchor => anchor;
    public Color SpeakerColor => speakerColor;
    public AudioClip SoundEffect => soundEffect;
    public Sprite BackgroundSprite => backgroundSprite;
}
