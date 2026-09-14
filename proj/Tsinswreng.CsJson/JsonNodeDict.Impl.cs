//Impl:JsonNodeDict 的實現。聲明見 JsonNodeDict.cs
using System.Text.Json.Nodes;

namespace Tsinswreng.CsJson;

public partial class JsonNodeDict{
	// ---- 構造函數 ----
	public partial JsonNodeDict(){
		Src = [];
	}

	public partial JsonNodeDict(JsonObject obj){
		ArgumentNullException.ThrowIfNull(obj);
		Src = obj;
	}

	// ---- IDictionary<str,obj?> ----
	public partial void Add(str Key, obj? Value){
		if(ContainsKey(Key)){
			throw new ArgumentException($"Key already exists: {Key}", nameof(Key));
		}
		Adapted[Key] = Value;
		Dirty = true;
	}

	public partial bool ContainsKey(str Key)
		=> Src.ContainsKey(Key) || Adapted.ContainsKey(Key);

	public partial bool Remove(str Key){
		var removed = Adapted.Remove(Key);
		removed |= Src.Remove(Key);
		if(removed){
			Dirty = true;
		}
		return removed;
	}

	public partial bool TryGetValue(str Key, out obj? Value){
		if(!ContainsKey(Key)){
			Value = null;
			return false;
		}
		Value = GetCached(Key);
		return true;
	}

	public partial void Add(KeyValuePair<str, obj?> Item)
		=> Add(Item.Key, Item.Value);

	public partial void Clear(){
		Src.Clear();
		Adapted.Clear();
		Dirty = true;
	}

	public partial bool Contains(KeyValuePair<str, obj?> Item){
		if(!TryGetValue(Item.Key, out var value)){
			return false;
		}
		return EqualityComparer<obj?>.Default.Equals(value, Item.Value);
	}

	public partial void CopyTo(KeyValuePair<str, obj?>[] Array, int ArrayIndex){
		ArgumentNullException.ThrowIfNull(Array);
		if(ArrayIndex < 0 || ArrayIndex > Array.Length){
			throw new ArgumentOutOfRangeException(nameof(ArrayIndex));
		}
		if(Array.Length - ArrayIndex < Count){
			throw new ArgumentException("Destination array is too small.", nameof(Array));
		}
		foreach(var kv in this){
			Array[ArrayIndex++] = kv;
		}
	}

	public partial bool Remove(KeyValuePair<str, obj?> Item){
		if(!Contains(Item)){
			return false;
		}
		return Remove(Item.Key);
	}

	public partial IEnumerator<KeyValuePair<str, obj?>> GetEnumerator(){
		// 並集枚舉:鍵序與 Keys 一致,值一律經真相(Adapted 命中或懶適配)。
		foreach(var key in Keys){
			yield return new KeyValuePair<str, obj?>(key, GetCached(key));
		}
	}

	// ---- 內部 ----
	private partial obj? GetCached(str Key){
		if(Adapted.TryGetValue(Key, out var hit)){
			return hit;
		}
		if(!Src.TryGetPropertyValue(Key, out var node)){
			throw new KeyNotFoundException($"Key not found: {Key}");
		}
		var adapted = Adapt(node);
		if(adapted is JsonNodeDict or JsonNodeList){
			// 容器才緩存同一實例;標量直接返回,不入真相(重適配零成本)。
			Adapted[Key] = adapted;
		}
		return adapted;
	}

	private partial void SetCached(str Key, obj? Value){
		Adapted[Key] = Value;
		Dirty = true;
	}

	internal static partial obj? Adapt(JsonNode? Node){
		if(Node is null){
			return null;
		}
		if(Node is JsonObject obj){
			return new JsonNodeDict(obj);
		}
		if(Node is JsonArray arr){
			return new JsonNodeList(arr);
		}
		if(Node is JsonValue value){
			if(value.TryGetValue<i64>(out var i64v)){
				return i64v;
			}
			if(value.TryGetValue<f64>(out var f64v)){
				return f64v;
			}
			if(value.TryGetValue<str>(out var sv)){
				return sv;
			}
			if(value.TryGetValue<bool>(out var bv)){
				return bv;
			}
			return value.ToJsonString(); // 兜底:不丟信息
		}
		throw new NotSupportedException($"Unsupported JsonNode kind: {Node.GetType().Name}");
	}
}