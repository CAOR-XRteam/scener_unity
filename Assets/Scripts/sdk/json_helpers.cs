using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SceneDeserialization;
using UnityEngine;

public class ListConverter<TBase, TConverter> : JsonConverter<List<TBase>>
    where TConverter : JsonConverter, new()
{
    private readonly TConverter _itemConverter = new();

    public override void WriteJson(JsonWriter writer, List<TBase> value, JsonSerializer serializer)
    {
        // Complete with serialization logic?
        throw new NotImplementedException();
    }

    public override List<TBase> ReadJson(
        JsonReader reader,
        Type objectType,
        List<TBase> existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        JArray array = JArray.Load(reader);
        List<TBase> list = new(array.Count);
        foreach (JToken token in array)
        {
            list.Add(
                (TBase)
                    _itemConverter.ReadJson(token.CreateReader(), typeof(TBase), null, serializer)
            );
        }
        return list;
    }
}

public abstract class JsonCreationConverter<T> : JsonConverter
{
    protected abstract T Create(Type objectType, JObject jObject);

    public override bool CanConvert(Type objectType)
    {
        return typeof(T).IsAssignableFrom(objectType);
    }

    public override object ReadJson(
        JsonReader reader,
        Type objectType,
        object existingValue,
        JsonSerializer serializer
    )
    {
        JObject jObject = JObject.Load(reader);
        T target = Create(objectType, jObject);
        serializer.Populate(jObject.CreateReader(), target);
        return target;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        // Complete with serialization logic?
        throw new NotImplementedException();
    }
}

public class LightConverter : JsonCreationConverter<BaseLight>
{
    protected override BaseLight Create(Type objectType, JObject jObject)
    {
        var type = (string)jObject.Property("type");
        return type switch
        {
            "spot" => new SpotLight(),
            "directional" => new DirectionalLight(),
            "point" => new PointLight(),
            "area" => new AreaLight(),
            _ => throw new ArgumentException($"The light type {type} is not supported"),
        };
    }

    public static void MapBaseLightProperties<T>(T lightData, Light light)
        where T : BaseLight
    {
        lightData.id = light.gameObject.GetInstanceID().ToString();
        lightData.position = light.transform.position.ToVector3();
        lightData.rotation = light.transform.eulerAngles.ToVector3();
        lightData.scale = light.transform.localScale.ToVector3();
        lightData.color = light.color.ToColorRGBA();
        lightData.intensity = light.intensity;
        lightData.indirect_multiplier = light.bounceIntensity;
    }

    public static void MapLightModeAndShadows(dynamic lightData, Light light)
    {
        lightData.mode = light.lightmapBakeType switch
        {
            LightmapBakeType.Baked => LightMode.Baked,
            LightmapBakeType.Mixed => LightMode.Mixed,
            _ => LightMode.Realtime,
        };
        lightData.shadow_type = light.shadows switch
        {
            LightShadows.None => ShadowType.NoShadows,
            LightShadows.Hard => ShadowType.HardShadows,
            _ => ShadowType.SoftShadows,
        };
    }
}

public class SkyboxConverter : JsonCreationConverter<SceneDeserialization.Skybox>
{
    protected override SceneDeserialization.Skybox Create(Type objectType, JObject jObject)
    {
        var type = (string)jObject.Property("type");
        return type switch
        {
            "gradient" => new GradientSkybox(),
            "sun" => new SunSkybox(),
            "cubed" => new CubedSkybox(),
            _ => throw new ArgumentException($"The skybox type {type} is not supported"),
        };
    }
}

public class ObjectConverter
{
    public static bool IsPrimitive(GameObject obj)
    {
        MeshFilter mf = obj.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
            return false;

        string meshName = mf.sharedMesh.name;
        return meshName.StartsWith("Cube")
            || meshName.StartsWith("Sphere")
            || meshName.StartsWith("Capsule")
            || meshName.StartsWith("Cylinder")
            || meshName.StartsWith("Plane")
            || meshName.StartsWith("Quad");
    }

    public static ShapeType? GetPrimitiveShape(GameObject obj)
    {
        MeshFilter mf = obj.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
            return null;

        string meshName = mf.sharedMesh.name;
        return meshName.StartsWith("Cube") ? ShapeType.Cube
            : meshName.StartsWith("Sphere") ? ShapeType.Sphere
            : meshName.StartsWith("Capsule") ? ShapeType.Capsule
            : meshName.StartsWith("Cylinder") ? ShapeType.Cylinder
            : meshName.StartsWith("Plane") ? ShapeType.Plane
            : meshName.StartsWith("Quad") ? ShapeType.Quad
            : null;
    }
}
