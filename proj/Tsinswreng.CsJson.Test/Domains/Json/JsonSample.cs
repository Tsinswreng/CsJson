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