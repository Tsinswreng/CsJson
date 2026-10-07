using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Tsinswreng.CsJson.Test.Domains.Json;

// 測試數據:一個普通 POCO + 對應的源生成 context(模擬 Ngan.Dict 的 AppJsonCtx 用法)。
public class JsonSample{
	public str Name{get; set;} = "";
	public int Age{get; set;}
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified)]
[JsonSerializable(typeof(JsonSample))]
public partial class JsonSampleCtx:JsonSerializerContext{
}

// 模擬 Ngan.Dict 的 AppJsonCtx:業務 context 註冊了 DictJson 的接口 TypeInfo,
// 以及 polymorphic 序列化需要的原始值類型(i64/f64/str/bool——枚舉 IDictionary<string,object?>
// 的值時按運行時類型查 TypeInfo)。
// 用於復現「DeserializeToDictList 產物(JsonNodeDict)→ Serialize(IDictionary)」的再序列化鏈。
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified)]
[JsonSerializable(typeof(IDictionary<str, obj?>))]
[JsonSerializable(typeof(IList<obj?>))]
[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(i64))]
[JsonSerializable(typeof(f64))]
[JsonSerializable(typeof(str))]
[JsonSerializable(typeof(bool))]
public partial class DictJsonCtx:JsonSerializerContext{
}