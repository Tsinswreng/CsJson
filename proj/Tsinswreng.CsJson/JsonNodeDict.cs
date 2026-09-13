//Decl:JsonNodeDict 的聲明。實現見 JsonNodeDict.Impl.cs
using System.Text.Json.Nodes;
using System.Text.Json;

namespace Tsinswreng.CsJson;

/// 把 JsonObject 適配成 IDictionary&lt;str,obj?&gt; 的視圖(零拷貝)。
/// 讀鍵時「懶適配」:命中內部緩存直接返回同一實例;未命中才把 JsonNode 適配成
/// IDict/List/標量,寫入緩存後再返回。返回的必須是同一實例,調用方對它做的
/// 修改(dict[k].Add(...) / list.Add(...))才能作用到底層樹並被下次讀到。
///
/// 【賦值語義】通過本視圖的索引器/Add 寫入的值會被拷貝進底層樹(ToNode 遞歸重建),
/// 不是「引用同一棵樹」。因此不要這樣用:
/// dict.Obj["child"]... 見下方——
///   outer["child"] = src;   // 拷貝了一份 {a:1} 進 outer 的樹
///   src["a"] = 999;         // 改的是 src 原樹,outer 裏那份拷貝不受影響 ❌
/// 想要修改生效,正確做法是寫入後「從樹裏讀回同一實例」再改:
///   outer["child"] = src;
///   src = (JsonNodeDict)outer["child"]!;  // 從緩存拿回同一實例
///   src["a"] = 999;                       // 改的是樹裏那份,生效 ✅
/// 若確要共享同一棵樹,直接傳節點本人:outer.Obj["child"] = src.Obj;
public partial class JsonNodeDict : IDictionary<str, obj?>{
	// 字段、訪問器、屬性寫在 Decl。
	private readonly JsonObject _obj;
	private readonly Dictionary<str, obj?> _cached = new();

	/// 底層 JsonObject 樹(可變,視圖改它即改樹)。
	/// 賦值/共享的注意事項見本類頂部【賦值語義】。
	public JsonObject Obj{
		get => _obj;
	}

	// ---- 構造函數(實現見 Impl)----
	public partial JsonNodeDict();
	public partial JsonNodeDict(JsonObject obj);

	// ---- IDictionary<str,obj?> ----
	public obj? this[str Key]{
		get => GetCached(Key);
		set => SetCached(Key, value);
	}

	public ICollection<str> Keys{
		get{
			var ans = new List<str>(_obj.Count);
			foreach(var kv in _obj){
				ans.Add(kv.Key);
			}
			return ans;
		}
	}

	public ICollection<obj?> Values{
		get{
			var ans = new List<obj?>(_obj.Count);
			foreach(var kv in _obj){
				ans.Add(GetCached(kv.Key));
			}
			return ans;
		}
	}

	public int Count => _obj.Count;

	public bool IsReadOnly => false;

	public partial void Add(str Key, obj? Value);
	public partial bool ContainsKey(str Key);
	public partial bool Remove(str Key);
	public partial bool TryGetValue(str Key, out obj? Value);
	public partial void Add(KeyValuePair<str, obj?> Item);
	public partial void Clear();
	public partial bool Contains(KeyValuePair<str, obj?> Item);
	public partial void CopyTo(KeyValuePair<str, obj?>[] Array, int ArrayIndex);
	public partial bool Remove(KeyValuePair<str, obj?> Item);
	public partial IEnumerator<KeyValuePair<str, obj?>> GetEnumerator();
	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		=> GetEnumerator();

	// ---- 內部(實現見 Impl)----
	/// 讀鍵:緩存命中返回同一實例;未命中適配後寫入緩存再返回。
	private partial obj? GetCached(str Key);

	/// 寫鍵:同步寫底層樹,並使該鍵緩存失效(下次讀再懶適配)。
	private partial void SetCached(str Key, obj? Value);

	/// 適配單個 JsonNode 為 obj?:
	/// JsonObject → 新 JsonNodeDict;JsonArray → 新 JsonNodeList;
	/// JsonValue → 標量;null → null。數字規則:i64 優先,不行 f64,再不行字符串。
	public static partial obj? Adapt(JsonNode? Node);

	/// 反序列化方向:obj? → JsonNode。
	/// 支持:null/標量/JsonNode/IDictionary&lt;str,obj?&gt;/IEnumerable。
	/// 手寫遞歸,不依賴 JsonSerializer 反射,AOT 安全。
	/// 未知類型(非 JSON 值)拋 NotSupportedException;業務對象請先序列化成 JsonNode 再放入。
	public static partial JsonNode? ToNode(obj? Value);

	public static partial bool AreEqual(obj? Left, obj? Right);
}