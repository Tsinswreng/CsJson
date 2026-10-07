//Impl:IJsonSerializerExtn 的實現。聲明見 IJsonSerializerExtn.cs
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Tsinswreng.CsJson;

public static partial class IJsonSerializerExtn{
	public static partial obj? DeserializeToDictList(this IJsonSerializer z, str Json, JsonSerializerOptions? options){
		if(str.IsNullOrEmpty(Json)){
			return null;
		}
		var node = z.Deserialize<JsonNode>(Json, options);
		return JsonNodeDict.Adapt(node);
	}
}