using System.Collections.Generic;
using Newtonsoft.Json;

namespace Scener.Sdk
{
    public class SceneUpdate
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("skybox")]
        public Skybox Skybox { get; set; }

        [JsonProperty("objects_to_add")]
        [JsonConverter(typeof(ListConverter<SceneObject, AdditionInfoConverter>))]
        public List<SceneObject> ObjectsToAdd { get; set; } = new List<SceneObject>();

        [JsonProperty("objects_to_update")]
        public List<SceneObjectUpdate> ObjectsToUpdate { get; set; } =
            new List<SceneObjectUpdate>();

        [JsonProperty("objects_to_delete")]
        public List<string> ObjectsToDelete { get; set; } = new List<string>();

        [JsonProperty("objects_to_regenerate")]
        public List<RegenerationInfo> ObjectsToRegenerate { get; set; } =
            new List<RegenerationInfo>();
    }

    public class SceneObjectUpdate
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("parent_id")]
        public string ParentId { get; set; }

        [JsonProperty("position")]
        public Vector3 Position { get; set; }

        [JsonProperty("rotation")]
        public Vector3 Rotation { get; set; }

        [JsonProperty("scale")]
        public Vector3 Scale { get; set; }

        [JsonProperty("components_to_update")]
        public List<ComponentUpdate> ComponentsToUpdate { get; set; } = new List<ComponentUpdate>();
    }

    public class RegenerationInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("new_id")]
        public string NewId { get; set; }

        [JsonProperty("prompt")]
        public string Prompt { get; set; }
    }

    [JsonConverter(typeof(ComponentUpdateConverter))]
    public abstract class ComponentUpdate
    {
        public SceneObjectType component_type { get; set; }
    }

    public class PrimitiveObjectUpdate : ComponentUpdate
    {
        public ShapeType? shape { get; set; }
        public ColorRGBA color { get; set; }
    }

    [JsonConverter(typeof(LightUpdateConverter))]
    public abstract class BaseLightUpdate : ComponentUpdate
    {
        public LightType type { get; set; }
        public ColorRGBA color { get; set; }
        public float? intensity { get; set; }
        public float? indirect_multiplier { get; set; }
    }

    public class SpotLightUpdate : BaseLightUpdate
    {
        public float? range { get; set; }
        public float? spot_angle { get; set; }
        public LightMode? mode { get; set; }
        public ShadowType? shadow_type { get; set; }
    }

    public class DirectionalLightUpdate : BaseLightUpdate
    {
        public LightMode? mode { get; set; }
        public ShadowType? shadow_type { get; set; }
    }

    public class PointLightUpdate : BaseLightUpdate
    {
        public float? range { get; set; }
        public LightMode? mode { get; set; }
        public ShadowType? shadow_type { get; set; }
    }

    public class AreaLightUpdate : BaseLightUpdate
    {
        public LightShape? shape { get; set; }
        public float? range { get; set; }
        public float? width { get; set; }
        public float? height { get; set; }
        public float? radius { get; set; }
    }
}
