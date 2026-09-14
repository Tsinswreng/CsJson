//Impl:JsonNodeList 的實現。聲明見 JsonNodeList.cs
using System.Text.Json.Nodes;

namespace Tsinswreng.CsJson;

public partial class JsonNodeList{
	// ---- 構造函數 ----
	public partial JsonNodeList(){
		Src = [];
	}

	public partial JsonNodeList(JsonArray arr){
		ArgumentNullException.ThrowIfNull(arr);
		Src = arr;
		// 預填與來源等長的槽位(null = 未適配),Count 立即反映數組長度。
		Adapted.AddRange(Enumerable.Repeat<obj?>(null, arr.Count));
	}

	// ---- IList<obj?> ----
	public partial void Add(obj? Item){
		EnsureMaterialized();
		Adapted.Add(Item);
	}

	public partial void Clear(){
		Src.Clear();
		Adapted.Clear();
		Dirty = true;
	}

	public partial bool Contains(obj? Item)
		=> IndexOf(Item) >= 0;

	public partial void CopyTo(obj?[] Array, int ArrayIndex){
		ArgumentNullException.ThrowIfNull(Array);
		if(ArrayIndex < 0 || ArrayIndex > Array.Length){
			throw new ArgumentOutOfRangeException(nameof(ArrayIndex));
		}
		if(Array.Length - ArrayIndex < Count){
			throw new ArgumentException("Destination array is too small.", nameof(Array));
		}
		foreach(var item in this){
			Array[ArrayIndex++] = item;
		}
	}

	public partial IEnumerator<obj?> GetEnumerator(){
		for(var i = 0; i < Adapted.Count; i++){
			yield return GetCached(i);
		}
	}

	public partial int IndexOf(obj? Item){
		for(var i = 0; i < Adapted.Count; i++){
			if(EqualityComparer<obj?>.Default.Equals(GetCached(i), Item)){
				return i;
			}
		}
		return -1;
	}

	public partial void Insert(int Index, obj? Item){
		EnsureMaterialized();
		Adapted.Insert(Index, Item);
	}

	public partial bool Remove(obj? Item){
		var index = IndexOf(Item);
		if(index < 0){
			return false;
		}
		RemoveAt(index);
		return true;
	}

	public partial void RemoveAt(int Index){
		EnsureMaterialized();
		Adapted.RemoveAt(Index);
	}

	// ---- 內部 ----
	private partial obj? GetCached(int Index){
		if(Index < 0 || Index >= Adapted.Count){
			throw new IndexOutOfRangeException($"Index {Index} out of range 0..{Adapted.Count - 1}");
		}
		var v = Adapted[Index];
		if(v is not null){
			return v;
		}
		if(Index < Src.Count && !Dirty){
			// 純讀階段:從來源適配;容器入槽(同一實例),標量直接返回。
			var adapted = JsonNodeDict.Adapt(Src[Index]);
			if(adapted is JsonNodeDict or JsonNodeList){
				Adapted[Index] = adapted;
			}
			return adapted;
		}
		// Src 已退役(寫過)或索引超出來源(Add 追加區):槽位即真相。
		return v;
	}

	private partial void SetAt(int Index, obj? Value){
		EnsureMaterialized();
		if(Index < 0 || Index >= Adapted.Count){
			throw new IndexOutOfRangeException($"Index {Index} out of range 0..{Adapted.Count - 1}");
		}
		Adapted[Index] = Value;
	}

	// 首次寫入時把來源未適配的槽位全部物化(標量也入槽):
	// 之後 Insert/RemoveAt 只動 Adapted 也不會與 Src 錯位,後續讀/序列化全以槽為準。
	private void EnsureMaterialized(){
		if(Dirty){
			return;
		}
		for(var i = 0; i < Src.Count && i < Adapted.Count; i++){
			if(Adapted[i] is null){
				Adapted[i] = JsonNodeDict.Adapt(Src[i]);
			}
		}
		Dirty = true;
	}
}