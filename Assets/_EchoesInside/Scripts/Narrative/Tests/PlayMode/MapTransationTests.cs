using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MapTransationTests
{
    private const string TransitionTypeName = "MapTransation, Assembly-CSharp";
    private const string PlayerControllerTypeName = "PlayerController, Assembly-CSharp";
    private const string ScreenFaderTypeName = "ScreenFader, EchoesInside.Narrative";

    [Test]
    public void TriggerEnter_PlayerArrivesBelowTargetAndStopsMoving()
    {
        Type transitionType = Type.GetType(TransitionTypeName);
        Assert.That(transitionType, Is.Not.Null, "MapTransation could not be loaded.");

        GameObject transitionObject = new GameObject("MapTransitionTest");
        GameObject playerObject = new GameObject("PlayerTest");

        try
        {
            transitionObject.SetActive(false);
            Component transition = transitionObject.AddComponent(transitionType);
            SetPrivateField(transition, "teleportPosition", new Vector2(10f, 20f));
            SetPrivateField(transition, "arrivalOffset", new Vector2(0f, -0.75f));
            transitionObject.SetActive(true);

            playerObject.tag = "Player";
            playerObject.transform.position = new Vector3(1f, 2f, 3f);
            Rigidbody2D rigidbody = playerObject.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0f;
            rigidbody.linearVelocity = new Vector2(2f, 4f);
            BoxCollider2D playerCollider = playerObject.AddComponent<BoxCollider2D>();

            InvokeTriggerEnter(transition, playerCollider);

            Assert.That(playerObject.transform.position.x, Is.EqualTo(10f).Within(0.001f));
            Assert.That(playerObject.transform.position.y, Is.EqualTo(19.25f).Within(0.001f));
            Assert.That(playerObject.transform.position.z, Is.EqualTo(3f).Within(0.001f));
            Assert.That(rigidbody.linearVelocity, Is.EqualTo(Vector2.zero));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(transitionObject);
            UnityEngine.Object.DestroyImmediate(playerObject);
        }
    }

    [UnityTest]
    public IEnumerator TriggerEnter_WithFader_TeleportsBetweenFadePhases()
    {
        Type transitionType = RequireType(TransitionTypeName);
        Type playerControllerType = RequireType(PlayerControllerTypeName);
        Type screenFaderType = RequireType(ScreenFaderTypeName);
        GameObject faderObject = new GameObject("RoomTransitionFaderTest", typeof(CanvasGroup));
        GameObject transitionObject = new GameObject("MapTransitionFadeTest");
        GameObject playerObject = new GameObject("PlayerFadeTest");

        try
        {
            faderObject.SetActive(false);
            CanvasGroup canvasGroup = faderObject.GetComponent<CanvasGroup>();
            Component screenFader = faderObject.AddComponent(screenFaderType);
            SetPrivateField(screenFader, "fadeDuration", 0.05f);
            SetPrivateField(screenFader, "fadeInOnStart", false);
            faderObject.SetActive(true);

            transitionObject.SetActive(false);
            Component transition = transitionObject.AddComponent(transitionType);
            SetPrivateField(transition, "teleportPosition", new Vector2(10f, 20f));
            SetPrivateField(transition, "arrivalOffset", new Vector2(0f, -0.75f));
            transitionObject.SetActive(true);

            playerObject.tag = "Player";
            Rigidbody2D rigidbody = playerObject.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0f;
            BoxCollider2D playerCollider = playerObject.AddComponent<BoxCollider2D>();
            Behaviour playerController = (Behaviour)playerObject.AddComponent(playerControllerType);

            yield return null;
            Vector3 startingPosition = new Vector3(1f, 2f, 3f);
            playerObject.transform.position = startingPosition;
            SetPrivateField(playerController, "_moveInput", Vector2.up);
            rigidbody.linearVelocity = Vector2.up * 2f;

            InvokeTriggerEnter(transition, playerCollider);

            Assert.That(playerObject.transform.position, Is.EqualTo(startingPosition),
                "Player teleported before fade-out completed.");
            Assert.That(playerController.enabled, Is.False,
                "Player control remained enabled during the room transition.");
            Assert.That(GetPrivateField<Vector2>(playerController, "_moveInput"), Is.EqualTo(Vector2.zero),
                "Player retained walking input while control was locked.");
            Assert.That(rigidbody.linearVelocity, Is.EqualTo(Vector2.zero));

            float teleportTimeout = 1f;
            while (playerObject.transform.position == startingPosition && teleportTimeout > 0f)
            {
                teleportTimeout -= Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.That(playerObject.transform.position.x, Is.EqualTo(10f).Within(0.001f));
            Assert.That(playerObject.transform.position.y, Is.EqualTo(19.25f).Within(0.001f));
            Assert.That(playerController.enabled, Is.False,
                "Player control was restored before fade-in completed.");

            float fadeInTimeout = 1f;
            while (!playerController.enabled && fadeInTimeout > 0f)
            {
                fadeInTimeout -= Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.That(canvasGroup.alpha, Is.EqualTo(0f).Within(0.001f));
            Assert.That(playerController.enabled, Is.True);
        }
        finally
        {
            UnityEngine.Object.Destroy(playerObject);
            UnityEngine.Object.Destroy(transitionObject);
            UnityEngine.Object.Destroy(faderObject);
        }
    }

    private static Type RequireType(string typeName)
    {
        Type type = Type.GetType(typeName);
        Assert.That(type, Is.Not.Null, $"Could not load {typeName}.");
        return type;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Missing serialized field {fieldName}.");
        field.SetValue(target, value);
    }

    private static T GetPrivateField<T>(object target, string fieldName)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Missing private field {fieldName}.");
        return (T)field.GetValue(target);
    }

    private static void InvokeTriggerEnter(Component transition, Collider2D playerCollider)
    {
        MethodInfo method = transition.GetType().GetMethod(
            "OnTriggerEnter2D",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(method, Is.Not.Null, "Missing OnTriggerEnter2D.");
        method.Invoke(transition, new object[] { playerCollider });
    }
}
