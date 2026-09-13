//Impl:JsonNodeDict 的實現。聲明見 JsonNodeDict.cs
using System.Collections;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace Tsinswreng.CsJson;

public partial class JsonNodeDict{
	// ---- 構造函數 ----
	public partial JsonNodeDict(){
		_obj = [];
	}

	public partial JsonNodeDict(JsonObject obj){
		ArgumentNullException.ThrowIfNull(obj);
		_obj = obj;
	}

	// ---- IDictionary<str,obj?> ----
	public partial void Add(str Key, obj? Value){
		var node = ToNode(Value);
		_obj.Add(Key, node);
		_cached[Key] = Adapt(node); // 緩存適配實例:讀回同一視圖,修改生效
	}

	public partial bool ContainsKey(str Key){
		return _obj.ContainsKey(Key);
	}

	public partial bool Remove(str Key){
		_cached.Remove(Key);
		return _obj.Remove(Key);
	}

	public partial bool TryGetValue(str Key, out obj? Value){
		if(!_obj.TryGetPropertyValue(Key, out _)){
			Value = null;
			return false;
		}
		Value = GetCached(Key);
		return true;
	}

	public partial void Add(KeyValuePair<str, obj?> Item)
		=> Add(Item.Key, Item.Value);

	public partial void Clear(){
		_obj.Clear();
		_cached.Clear();
	}

	public partial bool Contains(KeyValuePair<str, obj?> Item){
		if(!TryGetValue(Item.Key, out var value)){
			return false;
		}
		return AreEqual(value, Item.Value);
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
		foreach(var kv in _obj){
			yield return new KeyValuePair<str, obj?>(kv.Key, GetCached(kv.Key));
		}
	}

	// ---- 內部 ----
	private partial obj? GetCached(str Key){
		if(_cached.TryGetValue(Key, out var hit)){
			return hit;
		}
		if(!_obj.TryGetPropertyValue(Key, out var node)){
			throw new KeyNotFoundException($"Key not found: {Key}");
		}
		var adapted = Adapt(node);
		_cached[Key] = adapted;
		return adapted;
	}

	private partial void SetCached(str Key, obj? Value){
		_obj[Key] = ToNode(Value);
		_cached.Remove(Key); // 使該鍵緩存失效:下次讀重新懶適配,保證與樹一致
	}

	public static partial obj? Adapt(JsonNode? Node){
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

	public static partial JsonNode? ToNode(obj? Value){
		switch(Value){
			case null:
				return null;
			case JsonNode node:
				return node;
			case str s:
				return JsonValue.Create(s);
			case bool b:
				return JsonValue.Create(b);
			case i64 l:
				return JsonValue.Create(l);
			case u64 ul:
				// 超過 i64 範圍的 u64 無法用 TryGetValue<i64> 讀回,統一轉 f64 存,保證 Adapt 數字鏈可命中。
				if(ul <= i64.MaxValue){
					return JsonValue.Create((i64)ul);
				}
				return JsonValue.Create((f64)ul);
			case i32 i:
				// 統一存 i64:Adapt 數字優先 i64,避免 i32 值 TryGetValue<i64> 落空掉到字符串兜底。
				return JsonValue.Create((i64)i);
			case f64 d:
				return JsonValue.Create(d);
			case f32 f:
				return JsonValue.Create((f64)f);
			case decimal dec:
				return JsonValue.Create((f64)dec);
			case IDictionary<str, obj?> dict:{
				var objNode = new JsonObject();
				foreach(var kv in dict){
					objNode[kv.Key] = ToNode(kv.Value);
				}
				return objNode;
			}
			case IEnumerable enumrb:{
				var arrNode = new JsonArray();
				foreach(var item in enumrb){
					arrNode.Add(ToNode(item));
				}
				return arrNode;
			}
			default:
				throw new NotSupportedException(
					$"Unsupported value type: {Value.GetType().FullName}. 僅支持 JSON 值 / JsonNode;業務對象請先序列化成 JsonNode 再放入。"
				);
		}
	}

	public static partial bool AreEqual(obj? Left, obj? Right){
		if(ReferenceEquals(Left, Right)){
			return true;
		}
		if(Left is null || Right is null){
			return false;
		}
		if(Left.Equals(Right)){
			return true;
		}
		if(Left is JsonNode leftNode && Right is JsonNode rightNode){
			return leftNode.ToJsonString() == rightNode.ToJsonString();
		}
		return false;
	}
}