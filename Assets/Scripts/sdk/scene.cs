using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace SceneDeserialization
{
    public class ColorRGBA
    {
        public float r;
        public float g;
        public float b;
        public float? a;

        public Color ToUnityColor()
        {
            return new Color(r, g, b, a ?? 1.0f);
        }
    }

    public static class ColorRGBAExtensions
    {
        public static ColorRGBA ToColorRGBA(this Color color)
        {
            return new ColorRGBA
            {
                r = color.r,
                g = color.g,
                b = color.b,
                a = color.a,
            };
        }
    }

    public class Vector3
    {
        public float x;
        public float y;
        public float z;

        public UnityEngine.Vector3 ToUnityVector3()
        {
            return new UnityEngine.Vector3(x, y, z);
        }
    }

    public static class Vector3Extensions
    {
        public static Vector3 ToVector3(this UnityEngine.Vector3 vector)
        {
            return new Vector3
            {
                x = vector.x,
                y = vector.y,
                z = vector.z,
            };
        }
    }

    public class Vector4
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public UnityEngine.Vector4 ToUnityVector4()
        {
            return new UnityEngine.Vector4(x, y, z, w);
        }
    }

    public static class Vector4Extensions
    {
        public static Vector4 ToVector4(this UnityEngine.Vector4 vector)
        {
            return new Vector4
            {
                x = vector.x,
                y = vector.y,
                z = vector.z,
                w = vector.w,
            };
        }
    }

    public abstract class SceneComponent
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public abstract SceneObjectType componentType { get; }
    }

    public enum SceneObjectType
    {
        [EnumMember(Value = "dynamic")]
        Dynamic,

        [EnumMember(Value = "primitive")]
        Primitive,

        [EnumMember(Value = "light")]
        Light,
    }

    public enum ShapeType
    {
        [EnumMember(Value = "cube")]
        Cube,

        [EnumMember(Value = "sphere")]
        Sphere,

        [EnumMember(Value = "cylinder")]
        Cylinder,

        [EnumMember(Value = "capsule")]
        Capsule,

        [EnumMember(Value = "plane")]
        Plane,

        [EnumMember(Value = "quad")]
        Quad,
    }

    public class PrimitiveObject : SceneComponent
    {
        public override SceneObjectType componentType => SceneObjectType.Primitive;

        [JsonConverter(typeof(StringEnumConverter))]
        public ShapeType shape;
        public ColorRGBA color;
    }

    public class DynamicObject : SceneComponent
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public override SceneObjectType componentType => SceneObjectType.Dynamic;

        public string id;
    }

    public class SceneObject
    {
        public string id;

        public Vector3 position;

        public Vector3 rotation;

        public Vector3 scale;

        [JsonProperty(ItemConverterType = typeof(SceneComponentConverter))]
        public List<SceneComponent> components = new();

        public List<SceneObject> children = new();
    }

    public enum SkyboxType
    {
        [EnumMember(Value = "gradient")]
        Gradient,

        [EnumMember(Value = "sun")]
        Sun,

        [EnumMember(Value = "cubed")]
        Cubed,
    }

    public abstract class Skybox
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public SkyboxType type { get; set; }
    }

    public class GradientSkybox : Skybox
    {
        public ColorRGBA color1;
        public ColorRGBA color2;
        public Vector4 up_vector;
        public float intensity;
        public float exponent;
    }

    public class SunSkybox : Skybox
    {
        public ColorRGBA top_color;

        public float top_exponent;

        public ColorRGBA horizon_color;

        public ColorRGBA bottom_color;

        public float bottom_exponent;
        public float sky_intensity;

        public ColorRGBA sun_color;

        public float sun_intensity;

        public float sun_alpha;

        public float sun_beta;

        public Vector4 sun_vector;
    }

    public class CubedSkybox : Skybox
    {
        public ColorRGBA tint_color;

        public float exposure;

        public float rotation;

        public string cube_map;
    }

    public enum LightType
    {
        [EnumMember(Value = "spot")]
        Spot,

        [EnumMember(Value = "directional")]
        Directional,

        [EnumMember(Value = "point")]
        Point,

        [EnumMember(Value = "area")]
        Area,
    }

    public enum LightMode
    {
        [EnumMember(Value = "baked")]
        Baked,

        [EnumMember(Value = "mixed")]
        Mixed,

        [EnumMember(Value = "realtime")]
        Realtime,
    }

    public enum ShadowType
    {
        [EnumMember(Value = "no_shadows")]
        NoShadows,

        [EnumMember(Value = "hard_shadows")]
        HardShadows,

        [EnumMember(Value = "soft_shadows")]
        SoftShadows,
    }

    public enum LightShape
    {
        [EnumMember(Value = "rectangle")]
        Rectangle,

        [EnumMember(Value = "disk")]
        Disk,
    }

    public abstract class BaseLight : SceneComponent
    {
        public override SceneObjectType componentType => SceneObjectType.Light;

        [JsonConverter(typeof(StringEnumConverter))]
        public LightType type { get; set; }
        public ColorRGBA color;
        public float intensity;
        public float indirect_multiplier;
    }

    public class SpotLight : BaseLight
    {
        public float range;

        public float spot_angle;

        [JsonConverter(typeof(StringEnumConverter))]
        public LightMode mode;

        [JsonConverter(typeof(StringEnumConverter))]
        public ShadowType shadow_type;
    }

    public class DirectionalLight : BaseLight
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public LightMode mode;

        [JsonConverter(typeof(StringEnumConverter))]
        public ShadowType shadow_type;
    }

    public class PointLight : BaseLight
    {
        public float range;

        [JsonConverter(typeof(StringEnumConverter))]
        public LightMode mode;

        [JsonConverter(typeof(StringEnumConverter))]
        public ShadowType shadow_type;
    }

    public class AreaLight : BaseLight
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public LightShape shape;

        public float range;

        public float? width;

        public float? height;

        public float? radius;
    }

    public class Scene
    {
        public string name;

        [JsonConverter(typeof(SkyboxConverter))]
        public Skybox skybox;
        public List<SceneObject> graph;
    }
}
