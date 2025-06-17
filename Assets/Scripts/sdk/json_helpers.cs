using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SceneDeserialization;

public class ListConverter<TBase, TConverter> : JsonConverter<List<TBase>>
    where TConverter : JsonConverter, new()
{
    private readonly TConverter _itemConverter = new TConverter();

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
        List<TBase> list = new List<TBase>(array.Count);
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
        switch (type)
        {
            case "spot":
                return new SpotLight();
            case "directional":
                return new DirectionalLight();
            case "point":
                return new PointLight();
            case "area":
                return new AreaLight();
            default:
                throw new ArgumentException($"The light type {type} is not supported");
        }
    }
}

public class SkyboxConverter : JsonCreationConverter<Skybox>
{
    protected override Skybox Create(Type objectType, JObject jObject)
    {
        var type = (string)jObject.Property("type");
        switch (type)
        {
            case "gradient":
                return new GradientSkybox();
            case "sun":
                return new SunSkybox();
            case "cubed":
                return new CubedSkybox();
            default:
                throw new ArgumentException($"The skybox type {type} is not supported");
        }
    }
}
