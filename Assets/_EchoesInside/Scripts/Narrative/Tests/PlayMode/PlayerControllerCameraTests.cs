using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class PlayerControllerCameraTests
{
    private const string PlayerControllerTypeName = "PlayerController, Assembly-CSharp";
    private const string GameManagerTypeName = "GameManager, Assembly-CSharp";
    private const string ConfinerTypeName = "Unity.Cinemachine.CinemachineConfiner2D, Unity.Cinemachine";

    [UnityTest]
    public IEnumerator RestoringPlayerPosition_UsesTheBoundaryContainingThePlayer()
    {
        Type playerControllerType = RequireType(PlayerControllerTypeName);
        Type gameManagerType = RequireType(GameManagerTypeName);
        Type confinerType = RequireType(ConfinerTypeName);
        ResetGameManagerSingleton(gameManagerType);

        GameObject gameManagerObject = new GameObject("GameManagerTest");
        GameObject cameraObject = new GameObject("CameraConfinerTest");
        GameObject kitchenBoundaryObject = new GameObject("KitchenBoundaryTest");
        GameObject livingRoomBoundaryObject = new GameObject("LivingRoomBoundaryTest");
        GameObject playerObject = new GameObject("PlayerTest");

        try
        {
            Component gameManager = gameManagerObject.AddComponent(gameManagerType);
            Vector3 restoredPosition = new Vector3(10f, 0f, 0f);
            SetPublicField(gameManager, "lastMainScenePosition", restoredPosition);
            SetPublicField(gameManager, "savedSceneName", SceneManager.GetActiveScene().name);
            SetPublicField(gameManager, "hasSavedPosition", true);

            PolygonCollider2D kitchenBoundary = CreateSquareBoundary(kitchenBoundaryObject, Vector2.zero);
            PolygonCollider2D livingRoomBoundary = CreateSquareBoundary(livingRoomBoundaryObject, new Vector2(10f, 0f));

            Component confiner = cameraObject.AddComponent(confinerType);
            SetPublicField(confiner, "BoundingShape2D", kitchenBoundary);

            playerObject.AddComponent<Rigidbody2D>().gravityScale = 0f;
            playerObject.AddComponent(playerControllerType);

            yield return null;

            Assert.That(playerObject.transform.position, Is.EqualTo(restoredPosition));
            Assert.That(GetPublicField<Collider2D>(confiner, "BoundingShape2D"),
                Is.SameAs(livingRoomBoundary),
                "Camera confiner kept the old room boundary after restoring the player position.");
        }
        finally
        {
            UnityEngine.Object.Destroy(playerObject);
            UnityEngine.Object.Destroy(livingRoomBoundaryObject);
            UnityEngine.Object.Destroy(kitchenBoundaryObject);
            UnityEngine.Object.Destroy(cameraObject);
            UnityEngine.Object.Destroy(gameManagerObject);
            ResetGameManagerSingleton(gameManagerType);
        }
    }

    private static PolygonCollider2D CreateSquareBoundary(GameObject gameObject, Vector2 position)
    {
        gameObject.transform.position = position;
        PolygonCollider2D boundary = gameObject.AddComponent<PolygonCollider2D>();
        boundary.points = new[]
        {
            new Vector2(-2f, -2f),
            new Vector2(-2f, 2f),
            new Vector2(2f, 2f),
            new Vector2(2f, -2f)
        };
        return boundary;
    }

    private static Type RequireType(string typeName)
    {
        Type type = Type.GetType(typeName);
        Assert.That(type, Is.Not.Null, $"Could not load {typeName}.");
        return type;
    }

    private static void SetPublicField(Component component, string fieldName, object value)
    {
        FieldInfo field = component.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
        Assert.That(field, Is.Not.Null, $"Missing public field {fieldName}.");
        field.SetValue(component, value);
    }

    private static T GetPublicField<T>(Component component, string fieldName)
    {
        FieldInfo field = component.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
        Assert.That(field, Is.Not.Null, $"Missing public field {fieldName}.");
        return (T)field.GetValue(component);
    }

    private static void ResetGameManagerSingleton(Type gameManagerType)
    {
        FieldInfo instanceField = gameManagerType.GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(instanceField, Is.Not.Null, "Missing GameManager singleton field.");
        instanceField.SetValue(null, null);
    }
}
