//Decl:JsonNodeDict 的聲明。實現見 JsonNodeDict.Impl.cs
using System.Text.Json.Nodes;
using Tsinswreng.CsCore;

namespace Tsinswreng.CsJson;

[Doc(@$"
統一 {nameof(IDictionary<str, obj?>)} 門面(底層 JsonObject,讀多寫少場景)。

內部維持兩份數據:
- {nameof(Src)}:來源樹,構造時給入,寫操作不碰它;
- {nameof(Adapted)}:真相字典,存放「讀過或寫過」的鍵值。

行為契約:
- 讀:{nameof(Adapted)} 命中返回同一實例;未命中才適配 {nameof(Src)} 節點——
  標量適配後直接返回、不入 {nameof(Adapted)};容器(JsonNodeDict/JsonNodeList)
  入 {nameof(Adapted)} 後返回。保證「取出容器、原地修改、下次讀到同一實例看到修改」。
- 寫(索引器/Add):只寫 {nameof(Adapted)},置 {nameof(Dirty)},不碰樹——允許放入
  任意 obj?(含業務對象);序列化規則由序列化層統一處理。
- Remove/Clear:{nameof(Src)} 與 {nameof(Adapted)} 雙刪/雙清,置 {nameof(Dirty)}。
- Keys/Values/Count/枚舉:{nameof(Src)}∪{nameof(Adapted)},鍵在 {nameof(Adapted)} 優先。

序列化:本類不自帶序列化器;要 JsonNode 時,未 {nameof(Dirty)} 可 O(1) 交 {nameof(Src)}。
公共面只暴露 IDictionary 語義;內部細節全部 internal/private,換內部實現不影響調用方。
")]
public partial class JsonNodeDict : IJsonNodeDict{
	// 字段、訪問器、屬性寫在 Decl。
	[Doc("來源樹(寫操作不碰它)。")]
	internal readonly JsonObject Src;
	[Doc("真相:讀過/寫過的鍵值。")]
	private readonly Dictionary<str, obj?> Adapted = new();
	[Doc("是否被寫過:寫即置位(索引器/Add/Remove/Clear)。")]
	internal bool Dirty;

	// ---- 構造函數(實現見 Impl)----
	public partial JsonNodeDict();
	[Doc(@$"包裝給定樹:{nameof(Src)} 即該樹,寫操作不碰它。")]
	public partial JsonNodeDict(JsonObject obj);

	// ---- IDictionary<str,obj?> ----
	public obj? this[str Key]{
		get => GetCached(Key);
		set => SetCached(Key, value);
	}

	[Doc(@$"鍵集合:{nameof(Src)}∪{nameof(Adapted)},先樹序(僅當沒有被讀/寫過的新鍵時)。")]
	public ICollection<str> Keys{
		get{
			var ans = new List<str>(Count);
			var seen = new HashSet<str>();
			foreach(var kv in Src){
				if(seen.Add(kv.Key)){
					ans.Add(kv.Key);
				}
			}
			foreach(var key in Adapted.Keys){
				if(seen.Add(key)){
					ans.Add(key);
				}
			}
			return ans;
		}
	}

	[Doc(@$"值集合:與 {nameof(Keys)} 同序,鍵在 {nameof(Adapted)} 用其值,否則適配 {nameof(Src)}。")]
	public ICollection<obj?> Values{
		get{
			var ans = new List<obj?>(Count);
			foreach(var key in Keys){
				ans.Add(GetCached(key));
			}
			return ans;
		}
	}

	[Doc(@$"鍵數:{nameof(Src)}∪{nameof(Adapted)} 的數量。")]
	public int Count{
		get{
			var seen = new HashSet<str>(Src.Count + Adapted.Count);
			foreach(var kv in Src){
				seen.Add(kv.Key);
			}
			foreach(var key in Adapted.Keys){
				seen.Add(key);
			}
			return seen.Count;
		}
	}

	public bool IsReadOnly => false;

	public partial void Add(str Key, obj? Value);
	public partial bool ContainsKey(str Key);
	[Doc(@$"移除:雙刪 {nameof(Src)} 與 {nameof(Adapted)},置 {nameof(Dirty)}。")]
	public partial bool Remove(str Key);
	public partial bool TryGetValue(str Key, out obj? Value);
	public partial void Add(KeyValuePair<str, obj?> Item);
	[Doc(@$"清空:雙清 {nameof(Src)} 與 {nameof(Adapted)},置 {nameof(Dirty)}。")]
	public partial void Clear();
	public partial bool Contains(KeyValuePair<str, obj?> Item);
	public partial void CopyTo(KeyValuePair<str, obj?>[] Array, int ArrayIndex);
	public partial bool Remove(KeyValuePair<str, obj?> Item);
	public partial IEnumerator<KeyValuePair<str, obj?>> GetEnumerator();
	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		=> GetEnumerator();

	// ---- 內部(實現見 Impl)----
	[Doc(@$"讀鍵:{nameof(Adapted)} 命中返回同一實例;未命中適配 {nameof(Src)},標量直接返回,容器寫入 {nameof(Adapted)} 後返回。")]
	private partial obj? GetCached(str Key);

	[Doc(@$"寫鍵:只寫 {nameof(Adapted)}、置 {nameof(Dirty)};不碰 {nameof(Src)}。允許任意 obj?(含業務對象)。")]
	private partial void SetCached(str Key, obj? Value);

	[Doc(@$"適配單個 JsonNode 為 obj?:JsonObject → 新 JsonNodeDict;JsonArray → 新 JsonNodeList;JsonValue → 標量;null → null。數字規則:i64 優先,不行 f64,再不行字符串。")]
	internal static partial obj? Adapt(JsonNode? Node);
}