//JsonNodeCtx:為 JsonNode 及 JsonNodeDict/JsonNodeList 提供源生成 TypeInfo 的上下文。
//解決「反射禁用(AOT/JsonSerializerIsReflectionEnabledByDefault=false)時,
//JsonSerializer 的 options-only 重載需要 options 掛 resolver」的問題。
//- JsonNode:STJ 內建類型,生成器為它產生的 metadata 直接走內建 JsonNodeConverter,無需自定義序列化器;
//- JsonNodeDict/JsonNodeList:具體門面類型,生成器按「實現 IDictionary/IList」生成枚舉 converter,
//  序列化 JsonNodeDict 產物(含嵌套 JsonNodeList 值)時按運行時類型查 TypeInfo 不會缺。
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Tsinswreng.CsCore;

namespace Tsinswreng.CsJson;

[Doc(@$"
為 {nameof(JsonNode)}、{nameof(JsonNodeDict)}、{nameof(JsonNodeList)} 提供源生成 TypeInfo 的 {nameof(JsonSerializerContext)}。

用途:在反射被禁用的環境(AOT 發布、或 runtimeconfig 設 {nameof(System.Text.Json.JsonSerializerOptions)} 反射開關為 false)
下,{nameof(System.Text.Json.JsonSerializer)} 的 options-only 重載要求 options 掛一個
TypeInfoResolver;本 context 是「JSON 樹與 DictJson 門面」的最小 resolver。
{nameof(JsonNodeDict)}/{nameof(JsonNodeList)} 註冊後,{nameof(JsonNodeDict)} 產物的再序列化
(如 IDictJsonSerializer.FromDictJson 鏈)在 AOT 下也有 TypeInfo 可用。
")]
[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(JsonNodeDict))]
[JsonSerializable(typeof(JsonNodeList))]
public partial class JsonNodeCtx:JsonSerializerContext{
}