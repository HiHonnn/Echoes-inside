using System;
using System.Reflection;
using NUnit.Framework;

public class DreamAudioRoutingTests
{
    [TestCase("OldRadioMusic")]
    [TestCase("FatherRoomMusic")]
    [TestCase("MazeMusic")]
    [TestCase("KitchenMusic")]
    public void DreamAudioProduct_StoresSceneMusicPlayerReference(string typeName)
    {
        Type productType = Type.GetType($"{typeName}, Assembly-CSharp");
        Assert.That(productType, Is.Not.Null);

        FieldInfo field = productType.GetField(
            "_musicPlayer",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(field, Is.Not.Null);
        Assert.That(field.FieldType.FullName, Is.EqualTo("SceneMusicPlayer"));
    }
}
