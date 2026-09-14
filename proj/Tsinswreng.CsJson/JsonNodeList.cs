//Decl:JsonNodeList 的聲明。實現見 JsonNodeList.Impl.cs
using System.Text.Json.Nodes;
using Tsinswreng.CsCore;

namespace Tsinswreng.CsJson;

[Doc(@$"
統一 {nameof(IList<obj?>)} 門面(底層 JsonArray,讀多寫少場景)。

內部維持兩份數據:
- {nameof(Src)}:來源數組,構造時給入,僅在「對應索引尚未被寫過」時作為初始數據來源;
- {nameof(Adapted)}:真相列表,存放「讀過或寫過」的槽位(與索引一一對應)。

行為契約:
- 讀:{nameof(Adapted)} 該索引已適配 → 返回同一實例;未適配且索引在 {nameof(Src)} 範圍內 →
  適配 {nameof(Src)} 節點:標量直接返回、不入 {nameof(Adapted)};容器(JsonNodeDict/JsonNodeList)
  入 {nameof(Adapted)} 後返回。保證「取出容器、原地修改、下次讀到同一實例看到修改」。
- 寫(索引 set / Add / Insert / RemoveAt):只動 {nameof(Adapted)},不碰 {nameof(Src)}——允許放入
  任意 obj?(含業務對象)。結構修改後 {nameof(Adapted)} 與 {nameof(Src)} 不再對齊,{nameof(Src)} 退役,
  後續讀/序列化全以 {nameof(Adapted)} 為準。
- Clear:{nameof(Adapted)} 清空;{nameof(Src)} 一併作廢。

序列化:本類不自帶序列化器;要 JsonNode 時,未 {nameof(Dirty)} 可 O(1) 交 {nameof(Src)}。
公共面只暴露 IList 語義;內部細節全部 internal/private,換內部實現不影響調用方。
")]
public partial class JsonNodeList : IJsonNodeList{
	// 字段、訪問器、屬性寫在 Decl。
	[Doc("來源數組(僅初始數據來源)。")]
	internal readonly JsonArray Src;
	[Doc("真相:已讀/已寫的槽位,與索引一一對應。")]
	private readonly List<obj?> Adapted = new();
	[Doc("是否被寫過:寫即置位(索引 set/Add/Insert/RemoveAt/Clear)。")]
	internal bool Dirty;

	// ---- 構造函數(實現見 Impl)----
	[Doc(@$"空列表:{nameof(Src)} 為空數組。")]
	public partial JsonNodeList();
	[Doc(@$"包裝給定數組:{nameof(Adapted)} 預填與 arr.Count 等長的 null 槽位(未適配標記),使 {nameof(Count)} 立即反映數組長度;槽位在首次讀時才適配。")]
	public partial JsonNodeList(JsonArray arr);

	// ---- IList<obj?> ----
	public obj? this[int Index]{
		get => GetCached(Index);
		set => SetAt(Index, value);
	}

	[Doc(@$"元素數:以真相 {nameof(Adapted)} 為準(索引對齊,與 {nameof(Src)} 無關)。")]
	public int Count => Adapted.Count;

	public bool IsReadOnly => false;

	public partial void Add(obj? Item);
	[Doc(@$"清空:{nameof(Adapted)} 清空;{nameof(Src)} 一併作廢,置 {nameof(Dirty)}。")]
	public partial void Clear();
	public partial bool Contains(obj? Item);
	public partial void CopyTo(obj?[] Array, int ArrayIndex);
	public partial IEnumerator<obj?> GetEnumerator();
	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		=> GetEnumerator();
	public partial int IndexOf(obj? Item);
	public partial void Insert(int Index, obj? Item);
	public partial bool Remove(obj? Item);
	public partial void RemoveAt(int Index);

	// ---- 內部(實現見 Impl)----
	[Doc(@$"讀索引:{nameof(Adapted)} 已適配 → 返回同一實例;未適配且索引在 {nameof(Src)} 範圍內 → 適配 {nameof(Src)},標量直接返回、容器入 {nameof(Adapted)} 後返回;超出 {nameof(Src)} → 只從 {nameof(Adapted)} 讀。")]
	private partial obj? GetCached(int Index);

	[Doc(@$"寫索引:{nameof(Adapted)} 同步該槽位並置 {nameof(Dirty)};不碰 {nameof(Src)}。允許任意 obj?(含業務對象)。")]
	private partial void SetAt(int Index, obj? Value);
}