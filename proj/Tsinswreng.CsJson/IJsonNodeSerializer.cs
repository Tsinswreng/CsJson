using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

namespace Tsinswreng.CsJson;

public interface IJsonNodeSerializer{
	public JsonNode? SerializeToNode(
		obj? Value, Type InputType, JsonSerializerOptions? options = null
	);
	//public object? Deserialize(JsonNode? node, Type returnType, JsonSerializerOptions? options = null){}
}
