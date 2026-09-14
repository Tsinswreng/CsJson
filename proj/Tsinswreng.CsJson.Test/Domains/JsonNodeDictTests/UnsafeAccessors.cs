// 測試專用的 UnsafeAccessor:繞過可見性讀 JsonNodeDict/JsonNodeList 的 internal/private 字段做斷言。
// 不給測試項目開 InternalsVisibleTo,用 UnsafeAccessor 直接檢驗內部狀態(寫只進 Adapted、標量不入真相、Dirty 置位)。
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using Tsinswreng.CsJson;

namespace Tsinswreng.CsJson.Test.Domains.JsonNodeDictTests;

internal static class JsonNodeDictAcc{
	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "Src")]
	internal static extern ref JsonObject Src(JsonNodeDict d);

	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "Adapted")]
	internal static extern ref Dictionary<str, obj?> Adapted(JsonNodeDict d);

	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "Dirty")]
	internal static extern ref bool Dirty(JsonNodeDict d);

	[UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "Adapt")]
	internal static extern obj? Adapt(JsonNodeDict? d, JsonNode? node);
}

internal static class JsonNodeListAcc{
	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "Src")]
	internal static extern ref JsonArray Src(JsonNodeList l);

	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "Adapted")]
	internal static extern ref List<obj?> Adapted(JsonNodeList l);

	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "Dirty")]
	internal static extern ref bool Dirty(JsonNodeList l);
}