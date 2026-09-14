//接口:JsonNodeDict 的抽象。實現見 JsonNodeDict.cs / JsonNodeDict.Impl.cs
using Tsinswreng.CsCore;

namespace Tsinswreng.CsJson;

[Doc(@$"
統一 {nameof(IDictionary<str, obj?>)} 門面的契約(底層 JsonObject,讀多寫少場景)。

行為契約(具體實現必須遵守,換實現不影響調用方):
- 讀:已適配的容器返回同一實例;對它原地修改,下次讀到同一實例看到修改。
- 寫:允許放入任意 obj?(含業務對象);序列化規則由序列化層統一處理。
- 內部細節(來源樹、真相等)一律不暴露,只提供 IDictionary 語義。
")]
public interface IJsonNodeDict : IDictionary<str, obj?>{
}