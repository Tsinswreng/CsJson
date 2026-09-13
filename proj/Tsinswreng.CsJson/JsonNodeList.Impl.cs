//Impl:JsonNodeList 的實現。聲明見 JsonNodeList.cs
using System.Text.Json.Nodes;

namespace Tsinswreng.CsJson;

public partial class JsonNodeList{
	// ---- 構造函數 ----
	public partial JsonNodeList(){
		_arr = [];
	}

	public partial JsonNodeList(JsonArray arr){
		ArgumentNullException.ThrowIfNull(arr);
		_arr = arr;
		// 預填充與底層數組對齊的緩存槽位(null=未適配),避免 GetCached 越界。
		_cached.AddRange(Enumerable.Repeat<obj?>(null, arr.Count));
	}

	// ---- IList<obj?> ----
	public partial void Add(obj? Item){
		var node = JsonNodeDict.ToNode(Item);
		_arr.Add(node);
		_cached.Add(JsonNodeDict.Adapt(node)); // 追加位置即新索引,適配結果直接記入緩存
	}

	public partial void Clear(){
		_arr.Clear();
		_cached.Clear();
	}

	public partial bool Contains(obj? Item){
		return IndexOf(Item) >= 0;
	}

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
		for(var i = 0; i < _arr.Count; i++){
			yield return GetCached(i);
		}
	}

	public partial int IndexOf(obj? Item){
		for(var i = 0; i < _arr.Count; i++){
			if(JsonNodeDict.AreEqual(GetCached(i), Item)){
				return i;
			}
		}
		return -1;
	}

	public partial void Insert(int Index, obj? Item){
		_arr.Insert(Index, JsonNodeDict.ToNode(Item));
		_cached.Insert(Index, null); // 槽位插 null 表示未適配,下次讀懶適配
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
		_arr.RemoveAt(Index);
		_cached.RemoveAt(Index);
	}

	// ---- 內部 ----
	private partial obj? GetCached(int Index){
		if(_cached[Index] is not null){
			return _cached[Index];
		}
		var adapted = JsonNodeDict.Adapt(_arr[Index]);
		_cached[Index] = adapted;
		return adapted;
	}

	private partial void SetAt(int Index, obj? Value){
		_arr[Index] = JsonNodeDict.ToNode(Value);
		_cached[Index] = JsonNodeDict.Adapt(_arr[Index]); // 同步緩存,保持同一實例
	}
}