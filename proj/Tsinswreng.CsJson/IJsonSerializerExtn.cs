//Decl:IJsonSerializerExtn 的聲明。實現見 IJsonSerializerExtn.Impl.cs
using Tsinswreng.CsCore;

namespace Tsinswreng.CsJson;

[Doc(@$"
{nameof(IJsonSerializer)} 的擴展:JSON 字符串 → DictJson。

DictJson 的根不限定為對象(i64/f64/str/bool/null/IList/IDict 皆可),故返回 {nameof(obj)};
根為對象時返回 {nameof(JsonNodeDict)}(其已實現 {nameof(IDictionary<str, obj?>)},可原地讀寫)。

options 與接口其他 API 一致:傳入即用,不傳則走 z 自身的 {nameof(IJsonSerializer.Options)}。

數字規則與 {nameof(JsonNodeDict)} 的適配規則一致:i64 優先,不行 f64。
")]
public static partial class IJsonSerializerExtn{
	[Doc(@$"JSON 字符串 → DictJson。輸入為空或 null 時返回 null。")]
	public static partial obj? DeserializeToDictList(this IJsonSerializer z, str Json, JsonSerializerOptions? options = null);
}
