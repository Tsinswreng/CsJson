//Decl:JsonNodeList 的聲明。實現見 JsonNodeList.Impl.cs
using System.Text.Json.Nodes;

namespace Tsinswreng.CsJson;

/// 把 JsonArray 適配成 IList&lt;obj?&gt; 的視圖(零拷貝)。
/// 讀索引時「懶適配」:命中內部緩存(與底層數組對齊的槽位)直接返回同一實例;
/// 未命中才適配後寫入緩存再返回。同一實例保證調用方對它 Add/改索引/嵌套 dict[k]=v
/// 的修改能作用到底層樹並被下次讀到。
public partial class JsonNodeList : IList<obj?>{
	// 字段、訪問器、屬性寫在 Decl。
	private readonly JsonArray _arr;
	// 緩存槽位,與 _arr 保持同長對齊:已適配的索引存實例,未適配的存 null(重複適配 null 無害)。
	private readonly List<obj?> _cached = new();

	/// 底層 JsonArray 樹(可變,視圖改它即改樹)。
	public JsonArray Arr{
		get => _arr;
	}

	// ---- 構造函數(實現見 Impl)----
	public partial JsonNodeList();
	public partial JsonNodeList(JsonArray arr);

	// ---- IList<obj?> ----
	public obj? this[int Index]{
		get => GetCached(Index);
		set => SetAt(Index, value);
	}

	public int Count => _arr.Count;

	public bool IsReadOnly => false;

	/// 追加:適配寫入底層數組,並把原值記入緩存(追加位置即新索引)。
	public partial void Add(obj? Item);

	/// 清空:底層數組與緩存一併清空。
	public partial void Clear();

	public partial bool Contains(obj? Item);

	public partial void CopyTo(obj?[] Array, int ArrayIndex);

	/// 枚舉:逐索引走懶適配緩存。
	public partial IEnumerator<obj?> GetEnumerator();
	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		=> GetEnumerator();

	public partial int IndexOf(obj? Item);

	/// 插入:底層數組與緩存槽位同步插入(緩存槽位插 null 表示未適配)。
	public partial void Insert(int Index, obj? Item);

	public partial bool Remove(obj? Item);

	/// 移除:底層數組與緩存槽位同步移除。
	public partial void RemoveAt(int Index);

	// ---- 內部(實現見 Impl)----
	/// 讀索引:緩存命中返回同一實例;未命中適配後寫入緩存再返回。
	private partial obj? GetCached(int Index);

	/// 寫索引:同步寫底層數組與緩存。
	private partial void SetAt(int Index, obj? Value);
}