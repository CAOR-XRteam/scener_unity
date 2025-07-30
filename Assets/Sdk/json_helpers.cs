using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scener.Sdk;
using UnityEngine;

public class ListConverter<TBase, TConverter> : JsonConverter<List<TBase>>
    where TConverter : JsonConverter, new()
{
    private readonly TConverter _itemConverter = new();

    public override bool CanWrite
    {
        get { return false; }
    }

    public override void WriteJson(JsonWriter writer, List<TBase> value, JsonSerializer serializer)
    {
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

    public override bool CanWrite => false;

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
        throw new NotImplementedException();
    }
}

public class SceneComponentConverter : JsonCreationConverter<SceneComponent>
{
    protected override SceneComponent Create(Type objectType, JObject jObject)
    {
        string componentTypeValue = jObject["component_type"]?.ToString();

        switch (componentTypeValue.ToLower())
        {
            case "primitive":
                return new PrimitiveObject();
            case "dynamic":
                return new DynamicObject();
            case "light":
                string lightType = jObject["type"]?.ToString();
                return lightType switch
                {
                    "spot" => new SpotLight(),
                    "directional" => new DirectionalLight(),
                    "point" => new PointLight(),
                    "area" => new AreaLight(),
                    _ => throw new ArgumentException($"Unknown light type: {lightType}"),
                };
            default:
                throw new ArgumentException($"Unknown component type: {componentTypeValue}");
        }
    }
}

public class LightConverter
{
    public static void MapBaseLightProperties<T>(T lightData, Light light)
        where T : BaseLight
    {
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

public class SkyboxConverter : JsonCreationConverter<Scener.Sdk.Skybox>
{
    protected override Scener.Sdk.Skybox Create(Type objectType, JObject jObject)
    {
        string type = (string)jObject.Property("type");
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
    public static bool IsPrimitive(GameObject obj, out ShapeType? shape)
    {
        MeshFilter mf = obj.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            shape = null;
            return false;
        }

        string meshName = mf.sharedMesh.name;
        if (meshName.StartsWith("Cube"))
        {
            shape = ShapeType.Cube;
            return true;
        }
        if (meshName.StartsWith("Sphere"))
        {
            shape = ShapeType.Sphere;
            return true;
        }
        if (meshName.StartsWith("Capsule"))
        {
            shape = ShapeType.Capsule;
            return true;
        }
        if (meshName.StartsWith("Cylinder"))
        {
            shape = ShapeType.Cylinder;
            return true;
        }
        if (meshName.StartsWith("Plane"))
        {
            shape = ShapeType.Plane;
            return true;
        }
        if (meshName.StartsWith("Quad"))
        {
            shape = ShapeType.Quad;
            return true;
        }
        shape = null;
        return false;
    }
}

public static class EnumExtensions
{
    public static string ToEnumString<T>(this T enumValue)
        where T : struct, IConvertible
    {
        var memberInfo = typeof(T).GetMember(enumValue.ToString()).FirstOrDefault();
        if (memberInfo == null)
        {
            return enumValue.ToString();
        }

        var attribute = memberInfo
            .GetCustomAttributes(typeof(EnumMemberAttribute), false)
            .OfType<EnumMemberAttribute>()
            .FirstOrDefault();

        return attribute?.Value ?? enumValue.ToString();
    }
}

public class ComponentUpdateConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(ComponentUpdate);

    public override bool CanWrite => false;

    public override object ReadJson(
        JsonReader reader,
        Type objectType,
        object existingValue,
        JsonSerializer serializer
    )
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        JObject item = JObject.Load(reader);
        var componentTypeStr = item["component_type"]?.Value<string>();

        if (!Enum.TryParse(componentTypeStr, true, out SceneObjectType componentType))
        {
            throw new JsonSerializationException($"Unsupported component_type: {componentTypeStr}");
        }

        return componentType switch
        {
            SceneObjectType.Primitive => item.ToObject<PrimitiveObjectUpdate>(serializer),
            SceneObjectType.Light => item.ToObject<BaseLightUpdate>(serializer),
            _ => throw new JsonSerializationException(
                $"Unsupported component_type: {componentTypeStr}"
            ),
        };
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
        throw new NotImplementedException();
}

public class LightUpdateConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(BaseLightUpdate);

    public override bool CanWrite => false;

    public override object ReadJson(
        JsonReader reader,
        Type objectType,
        object existingValue,
        JsonSerializer serializer
    )
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        JObject item = JObject.Load(reader);
        var lightTypeStr = item["type"]?.Value<string>();

        if (!Enum.TryParse(lightTypeStr, true, out Scener.Sdk.LightType lightType))
        {
            throw new JsonSerializationException($"Unsupported light type: {lightTypeStr}");
        }

        return lightType switch
        {
            Scener.Sdk.LightType.Spot => item.ToObject<SpotLightUpdate>(serializer),
            Scener.Sdk.LightType.Directional => item.ToObject<DirectionalLightUpdate>(serializer),
            Scener.Sdk.LightType.Point => item.ToObject<PointLightUpdate>(serializer),
            Scener.Sdk.LightType.Area => item.ToObject<AreaLightUpdate>(serializer),
            _ => throw new JsonSerializationException($"Unsupported light type: {lightTypeStr}"),
        };
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
        throw new NotImplementedException();
}

public class AdditionInfoConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(SceneObject);
    }

    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override object ReadJson(
        JsonReader reader,
        Type objectType,
        object existingValue,
        JsonSerializer serializer
    )
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        JObject wrapperObject = JObject.Load(reader);

        JToken sceneObjectToken = wrapperObject["scene_object"];

        if (sceneObjectToken == null)
        {
            return null;
        }

        return sceneObjectToken.ToObject<SceneObject>(serializer);
    }
}
