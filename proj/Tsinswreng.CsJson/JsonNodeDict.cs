using System.Text.Json.Nodes;
using System.Text.Json;

namespace Tsinswreng.CsJson;

public class JsonNodeDict:IDictionary<str,obj?>{
	private JsonObject _jsonObject;

	public JsonNode JsonNode{
		get => _jsonObject;
		set => _jsonObject = EnsureJsonObject(value);
	}

	public JsonNodeDict(){
		_jsonObject = [];
	}
	public JsonNodeDict(JsonNode jsonNode){
		_jsonObject = EnsureJsonObject(jsonNode);
	}

	public obj? this[str key]{
		get{
			if(!_jsonObject.TryGetPropertyValue(key, out var node)){
				throw new KeyNotFoundException($"Key not found: {key}");
			}
			return NodeToObject(node);
		}
		set => _jsonObject[key] = ObjectToNode(value);
	}

	public ICollection<str> Keys => _jsonObject.Select(x => x.Key).ToList();

	public ICollection<obj?> Values => _jsonObject.Select(x => NodeToObject(x.Value)).ToList();

	public int Count => _jsonObject.Count;

	public bool IsReadOnly => false;

	public void Add(str key, obj? value){
		_jsonObject.Add(key, ObjectToNode(value));
	}

	public bool ContainsKey(str key){
		return _jsonObject.ContainsKey(key);
	}

	public bool Remove(str key){
		return _jsonObject.Remove(key);
	}

	public bool TryGetValue(str key, out obj? value){
		if(_jsonObject.TryGetPropertyValue(key, out var node)){
			value = NodeToObject(node);
			return true;
		}
		value = null;
		return false;
	}

	public void Add(KeyValuePair<str, obj?> item){
		Add(item.Key, item.Value);
	}

	public void Clear(){
		_jsonObject.Clear();
	}

	public bool Contains(KeyValuePair<str, obj?> item){
		if(!TryGetValue(item.Key, out var value)){
			return false;
		}
		return AreEqual(value, item.Value);
	}

	public void CopyTo(KeyValuePair<str, obj?>[] array, int arrayIndex){
		ArgumentNullException.ThrowIfNull(array);
		if(arrayIndex < 0 || arrayIndex > array.Length){
			throw new ArgumentOutOfRangeException(nameof(arrayIndex));
		}
		if(array.Length - arrayIndex < Count){
			throw new ArgumentException("Destination array is too small.", nameof(array));
		}

		foreach(var kv in this){
			array[arrayIndex++] = kv;
		}
	}

	public bool Remove(KeyValuePair<str, obj?> item){
		if(!Contains(item)){
			return false;
		}
		return Remove(item.Key);
	}

	public IEnumerator<KeyValuePair<str, obj?>> GetEnumerator(){
		foreach(var kv in _jsonObject){
			yield return new KeyValuePair<str, obj?>(kv.Key, NodeToObject(kv.Value));
		}
	}

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator(){
		return GetEnumerator();
	}

	private static JsonObject EnsureJsonObject(JsonNode node){
		if(node is JsonObject jsonObject){
			return jsonObject;
		}
		throw new ArgumentException("JsonNode must be a JsonObject.", nameof(node));
	}

	private static JsonNode? ObjectToNode(obj? value){
		if(value is null){
			return null;
		}
		if(value is JsonNode jsonNode){
			return jsonNode;
		}
		return JsonSerializer.SerializeToNode(value);
	}

	private static obj? NodeToObject(JsonNode? node){
		if(node is null){
			return null;
		}

		if(node is JsonValue value){
			if(value.TryGetValue<str>(out var s)) return s;
			if(value.TryGetValue<bool>(out var b)) return b;
			if(value.TryGetValue<i32>(out var i32v)) return i32v;
			if(value.TryGetValue<i64>(out var i64v)) return i64v;
			if(value.TryGetValue<f64>(out var f64v)) return f64v;
			if(value.TryGetValue<decimal>(out var decv)) return decv;
			if(value.TryGetValue<Guid>(out var guidv)) return guidv;
			if(value.TryGetValue<DateTime>(out var dtv)) return dtv;
			return value.ToJsonString();
		}

		return node;
	}

	private static bool AreEqual(obj? left, obj? right){
		if(ReferenceEquals(left, right)){
			return true;
		}
		if(left is null || right is null){
			return false;
		}
		if(left.Equals(right)){
			return true;
		}

		if(left is JsonNode leftNode && right is JsonNode rightNode){
			return leftNode.ToJsonString() == rightNode.ToJsonString();
		}

		return false;
	}
}
