namespace Tsinswreng.CsJson;

using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

/// IJsonSerializer 的默認實現：逐個方法直接轉發到官方 System.Text.Json.JsonSerializer 靜態 API。
/// 轉發方向與官方一致：
/// 帶 JsonTypeInfo / JsonSerializerContext 的重載是 AOT 安全路徑；
/// 只帶 JsonSerializerOptions 的重載在 AOT 下需要 options 掛源生成 TypeInfoResolver，
/// 否則走反射路徑（官方同款行爲，這些重載照官方標記 RequiresUnreferencedCode / RequiresDynamicCode，
/// AOT 調用方在編譯期收到 IL 警告）。
/// 與官方靜態 API 的唯一差異：所有重載的 options 參數爲 null（即調用方不傳）時，
/// 不退回官方默認 JsonSerializerOptions.Default，而是用構造函數注入的 DfltOptions。
/// 這是讓「DI 進來直接能用、調用方不用每次傳配置」成爲可能的那一點：
/// 應用層注入一份配好的全局 options（如掛 AppJsonCtx.Default 源生成 resolver），
/// 業務代碼就可以直接 Serialize(obj) / Deserialize&lt;T&gt;(json)，不傳任何配置。
public class DfltJsonSerializer : IJsonSerializer{
	/// 構造注入的默認 options；爲 null 時所有重載退回官方默認行爲（JsonSerializerOptions.Default）。
	private readonly JsonSerializerOptions? DfltOptions;

	public DfltJsonSerializer(JsonSerializerOptions? DfltOptions = null){
		this.DfltOptions = DfltOptions;
	}

	// ===== Serialize：object → JSON 字符串 =====

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public string Serialize<TValue>(TValue value, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.Serialize(value, options ?? DfltOptions);
	}
	public string Serialize<TValue>(TValue value, JsonTypeInfo<TValue> jsonTypeInfo){
		return System.Text.Json.JsonSerializer.Serialize(value, jsonTypeInfo);
	}
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public string Serialize(object? value, Type inputType, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.Serialize(value, inputType, options ?? DfltOptions);
	}
	public string Serialize(object? value, JsonTypeInfo jsonTypeInfo){
		return System.Text.Json.JsonSerializer.Serialize(value, jsonTypeInfo);
	}
	public string Serialize(object? value, Type inputType, JsonSerializerContext context){
		return System.Text.Json.JsonSerializer.Serialize(value, inputType, context);
	}

	// ===== Serialize：寫入 Utf8JsonWriter =====

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public void Serialize<TValue>(Utf8JsonWriter writer, TValue value, JsonSerializerOptions? options = null){
		System.Text.Json.JsonSerializer.Serialize(writer, value, options ?? DfltOptions);
	}
	public void Serialize<TValue>(Utf8JsonWriter writer, TValue value, JsonTypeInfo<TValue> jsonTypeInfo){
		System.Text.Json.JsonSerializer.Serialize(writer, value, jsonTypeInfo);
	}
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public void Serialize(Utf8JsonWriter writer, object? value, Type inputType, JsonSerializerOptions? options = null){
		System.Text.Json.JsonSerializer.Serialize(writer, value, inputType, options ?? DfltOptions);
	}
	public void Serialize(Utf8JsonWriter writer, object? value, JsonTypeInfo jsonTypeInfo){
		System.Text.Json.JsonSerializer.Serialize(writer, value, jsonTypeInfo);
	}
	public void Serialize(Utf8JsonWriter writer, object? value, Type inputType, JsonSerializerContext context){
		System.Text.Json.JsonSerializer.Serialize(writer, value, inputType, context);
	}

	// ===== Serialize：寫入 Stream =====

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public void Serialize<TValue>(Stream utf8Json, TValue value, JsonSerializerOptions? options = null){
		System.Text.Json.JsonSerializer.Serialize(utf8Json, value, options ?? DfltOptions);
	}
	public void Serialize<TValue>(Stream utf8Json, TValue value, JsonTypeInfo<TValue> jsonTypeInfo){
		System.Text.Json.JsonSerializer.Serialize(utf8Json, value, jsonTypeInfo);
	}
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public void Serialize(Stream utf8Json, object? value, Type inputType, JsonSerializerOptions? options = null){
		System.Text.Json.JsonSerializer.Serialize(utf8Json, value, inputType, options ?? DfltOptions);
	}
	public void Serialize(Stream utf8Json, object? value, JsonTypeInfo jsonTypeInfo){
		System.Text.Json.JsonSerializer.Serialize(utf8Json, value, jsonTypeInfo);
	}
	public void Serialize(Stream utf8Json, object? value, Type inputType, JsonSerializerContext context){
		System.Text.Json.JsonSerializer.Serialize(utf8Json, value, inputType, context);
	}

	// ===== SerializeAsync（Stream / PipeWriter）=====

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public Task SerializeAsync<TValue>(Stream utf8Json, TValue value, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.SerializeAsync(utf8Json, value, options ?? DfltOptions, cancellationToken);
	}
	public Task SerializeAsync<TValue>(Stream utf8Json, TValue value, JsonTypeInfo<TValue> jsonTypeInfo, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.SerializeAsync(utf8Json, value, jsonTypeInfo, cancellationToken);
	}
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public Task SerializeAsync(Stream utf8Json, object? value, Type inputType, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.SerializeAsync(utf8Json, value, inputType, options ?? DfltOptions, cancellationToken);
	}
	public Task SerializeAsync(Stream utf8Json, object? value, JsonTypeInfo jsonTypeInfo, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.SerializeAsync(utf8Json, value, jsonTypeInfo, cancellationToken);
	}
	public Task SerializeAsync(Stream utf8Json, object? value, Type inputType, JsonSerializerContext context, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.SerializeAsync(utf8Json, value, inputType, context, cancellationToken);
	}

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public Task SerializeAsync<TValue>(PipeWriter writer, TValue value, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.SerializeAsync(writer, value, options ?? DfltOptions, cancellationToken);
	}
	public Task SerializeAsync<TValue>(PipeWriter writer, TValue value, JsonTypeInfo<TValue> jsonTypeInfo, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.SerializeAsync(writer, value, jsonTypeInfo, cancellationToken);
	}
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public Task SerializeAsync(PipeWriter writer, object? value, Type inputType, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.SerializeAsync(writer, value, inputType, options ?? DfltOptions, cancellationToken);
	}
	public Task SerializeAsync(PipeWriter writer, object? value, JsonTypeInfo jsonTypeInfo, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.SerializeAsync(writer, value, jsonTypeInfo, cancellationToken);
	}
	public Task SerializeAsync(PipeWriter writer, object? value, Type inputType, JsonSerializerContext context, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.SerializeAsync(writer, value, inputType, context, cancellationToken);
	}

	// ===== SerializeToUtf8Bytes：object → UTF-8 bytes =====

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public byte[] SerializeToUtf8Bytes<TValue>(TValue value, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value, options ?? DfltOptions);
	}
	public byte[] SerializeToUtf8Bytes<TValue>(TValue value, JsonTypeInfo<TValue> jsonTypeInfo){
		return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value, jsonTypeInfo);
	}
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public byte[] SerializeToUtf8Bytes(object? value, Type inputType, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value, inputType, options ?? DfltOptions);
	}
	public byte[] SerializeToUtf8Bytes(object? value, JsonTypeInfo jsonTypeInfo){
		return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value, jsonTypeInfo);
	}
	public byte[] SerializeToUtf8Bytes(object? value, Type inputType, JsonSerializerContext context){
		return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value, inputType, context);
	}

	// ===== SerializeToElement：object → JsonElement 只讀視圖 =====

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public JsonElement SerializeToElement<TValue>(TValue value, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.SerializeToElement(value, options ?? DfltOptions);
	}
	public JsonElement SerializeToElement<TValue>(TValue value, JsonTypeInfo<TValue> jsonTypeInfo){
		return System.Text.Json.JsonSerializer.SerializeToElement(value, jsonTypeInfo);
	}
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public JsonElement SerializeToElement(object? value, Type inputType, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.SerializeToElement(value, inputType, options ?? DfltOptions);
	}
	public JsonElement SerializeToElement(object? value, JsonTypeInfo jsonTypeInfo){
		return System.Text.Json.JsonSerializer.SerializeToElement(value, jsonTypeInfo);
	}
	public JsonElement SerializeToElement(object? value, Type inputType, JsonSerializerContext context){
		return System.Text.Json.JsonSerializer.SerializeToElement(value, inputType, context);
	}

	// ===== SerializeToNode：object → JsonNode 可變樹 =====

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public JsonNode? SerializeToNode<TValue>(TValue value, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.SerializeToNode(value, options ?? DfltOptions);
	}
	public JsonNode? SerializeToNode<TValue>(TValue value, JsonTypeInfo<TValue> jsonTypeInfo){
		return System.Text.Json.JsonSerializer.SerializeToNode(value, jsonTypeInfo);
	}
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public JsonNode? SerializeToNode(object? value, Type inputType, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.SerializeToNode(value, inputType, options ?? DfltOptions);
	}
	public JsonNode? SerializeToNode(object? value, JsonTypeInfo jsonTypeInfo){
		return System.Text.Json.JsonSerializer.SerializeToNode(value, jsonTypeInfo);
	}
	public JsonNode? SerializeToNode(object? value, Type inputType, JsonSerializerContext context){
		return System.Text.Json.JsonSerializer.SerializeToNode(value, inputType, context);
	}

	// ===== SerializeToDocument：object → JsonDocument =====

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public JsonDocument SerializeToDocument<TValue>(TValue value, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.SerializeToDocument(value, options ?? DfltOptions);
	}
	public JsonDocument SerializeToDocument<TValue>(TValue value, JsonTypeInfo<TValue> jsonTypeInfo){
		return System.Text.Json.JsonSerializer.SerializeToDocument(value, jsonTypeInfo);
	}
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public JsonDocument SerializeToDocument(object? value, Type inputType, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.SerializeToDocument(value, inputType, options ?? DfltOptions);
	}
	public JsonDocument SerializeToDocument(object? value, JsonTypeInfo jsonTypeInfo){
		return System.Text.Json.JsonSerializer.SerializeToDocument(value, jsonTypeInfo);
	}
	public JsonDocument SerializeToDocument(object? value, Type inputType, JsonSerializerContext context){
		return System.Text.Json.JsonSerializer.SerializeToDocument(value, inputType, context);
	}

	// ===== Deserialize：JSON → object（string / utf8 bytes / span<char> / stream / document / element / node / reader）=====

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public TValue? Deserialize<TValue>(string json, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.Deserialize<TValue>(json, options ?? DfltOptions);
	}
	public TValue? Deserialize<TValue>(string json, JsonTypeInfo<TValue> jsonTypeInfo){
		return System.Text.Json.JsonSerializer.Deserialize<TValue>(json, jsonTypeInfo);
	}
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public object? Deserialize(string json, Type returnType, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.Deserialize(json, returnType, options ?? DfltOptions);
	}
	public object? Deserialize(string json, JsonTypeInfo jsonTypeInfo){
		return System.Text.Json.JsonSerializer.Deserialize(json, jsonTypeInfo);
	}
	public object? Deserialize(string json, Type returnType, JsonSerializerContext context){
		return System.Text.Json.JsonSerializer.Deserialize(json, returnType, context);
	}

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public TValue? Deserialize<TValue>(ReadOnlySpan<byte> utf8Json, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.Deserialize<TValue>(utf8Json, options ?? DfltOptions);
	}
	public TValue? Deserialize<TValue>(ReadOnlySpan<byte> utf8Json, JsonTypeInfo<TValue> jsonTypeInfo){
		return System.Text.Json.JsonSerializer.Deserialize<TValue>(utf8Json, jsonTypeInfo);
	}
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public object? Deserialize(ReadOnlySpan<byte> utf8Json, Type returnType, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.Deserialize(utf8Json, returnType, options ?? DfltOptions);
	}
	public object? Deserialize(ReadOnlySpan<byte> utf8Json, JsonTypeInfo jsonTypeInfo){
		return System.Text.Json.JsonSerializer.Deserialize(utf8Json, jsonTypeInfo);
	}
	public object? Deserialize(ReadOnlySpan<byte> utf8Json, Type returnType, JsonSerializerContext context){
		return System.Text.Json.JsonSerializer.Deserialize(utf8Json, returnType, context);
	}

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public TValue? Deserialize<TValue>(ReadOnlySpan<char> json, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.Deserialize<TValue>(json, options ?? DfltOptions);
	}
	public TValue? Deserialize<TValue>(ReadOnlySpan<char> json, JsonTypeInfo<TValue> jsonTypeInfo){
		return System.Text.Json.JsonSerializer.Deserialize<TValue>(json, jsonTypeInfo);
	}
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public object? Deserialize(ReadOnlySpan<char> json, Type returnType, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.Deserialize(json, returnType, options ?? DfltOptions);
	}
	public object? Deserialize(ReadOnlySpan<char> json, JsonTypeInfo jsonTypeInfo){
		return System.Text.Json.JsonSerializer.Deserialize(json, jsonTypeInfo);
	}
	public object? Deserialize(ReadOnlySpan<char> json, Type returnType, JsonSerializerContext context){
		return System.Text.Json.JsonSerializer.Deserialize(json, returnType, context);
	}

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public TValue? Deserialize<TValue>(Stream utf8Json, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.Deserialize<TValue>(utf8Json, options ?? DfltOptions);
	}
	public TValue? Deserialize<TValue>(Stream utf8Json, JsonTypeInfo<TValue> jsonTypeInfo){
		return System.Text.Json.JsonSerializer.Deserialize<TValue>(utf8Json, jsonTypeInfo);
	}
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public object? Deserialize(Stream utf8Json, Type returnType, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.Deserialize(utf8Json, returnType, options ?? DfltOptions);
	}
	public object? Deserialize(Stream utf8Json, JsonTypeInfo jsonTypeInfo){
		return System.Text.Json.JsonSerializer.Deserialize(utf8Json, jsonTypeInfo);
	}
	public object? Deserialize(Stream utf8Json, Type returnType, JsonSerializerContext context){
		return System.Text.Json.JsonSerializer.Deserialize(utf8Json, returnType, context);
	}

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public TValue? Deserialize<TValue>(JsonDocument document, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.Deserialize<TValue>(document, options ?? DfltOptions);
	}
	public TValue? Deserialize<TValue>(JsonDocument document, JsonTypeInfo<TValue> jsonTypeInfo){
		return System.Text.Json.JsonSerializer.Deserialize<TValue>(document, jsonTypeInfo);
	}
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public object? Deserialize(JsonDocument document, Type returnType, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.Deserialize(document, returnType, options ?? DfltOptions);
	}
	public object? Deserialize(JsonDocument document, JsonTypeInfo jsonTypeInfo){
		return System.Text.Json.JsonSerializer.Deserialize(document, jsonTypeInfo);
	}
	public object? Deserialize(JsonDocument document, Type returnType, JsonSerializerContext context){
		return System.Text.Json.JsonSerializer.Deserialize(document, returnType, context);
	}

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public TValue? Deserialize<TValue>(JsonElement element, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.Deserialize<TValue>(element, options ?? DfltOptions);
	}
	public TValue? Deserialize<TValue>(JsonElement element, JsonTypeInfo<TValue> jsonTypeInfo){
		return System.Text.Json.JsonSerializer.Deserialize<TValue>(element, jsonTypeInfo);
	}
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public object? Deserialize(JsonElement element, Type returnType, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.Deserialize(element, returnType, options ?? DfltOptions);
	}
	public object? Deserialize(JsonElement element, JsonTypeInfo jsonTypeInfo){
		return System.Text.Json.JsonSerializer.Deserialize(element, jsonTypeInfo);
	}
	public object? Deserialize(JsonElement element, Type returnType, JsonSerializerContext context){
		return System.Text.Json.JsonSerializer.Deserialize(element, returnType, context);
	}

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public TValue? Deserialize<TValue>(JsonNode? node, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.Deserialize<TValue>(node, options ?? DfltOptions);
	}
	public TValue? Deserialize<TValue>(JsonNode? node, JsonTypeInfo<TValue> jsonTypeInfo){
		return System.Text.Json.JsonSerializer.Deserialize<TValue>(node, jsonTypeInfo);
	}
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public object? Deserialize(JsonNode? node, Type returnType, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.Deserialize(node, returnType, options ?? DfltOptions);
	}
	public object? Deserialize(JsonNode? node, JsonTypeInfo jsonTypeInfo){
		return System.Text.Json.JsonSerializer.Deserialize(node, jsonTypeInfo);
	}
	public object? Deserialize(JsonNode? node, Type returnType, JsonSerializerContext context){
		return System.Text.Json.JsonSerializer.Deserialize(node, returnType, context);
	}

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public TValue? Deserialize<TValue>(ref Utf8JsonReader reader, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.Deserialize<TValue>(ref reader, options ?? DfltOptions);
	}
	public TValue? Deserialize<TValue>(ref Utf8JsonReader reader, JsonTypeInfo<TValue> jsonTypeInfo){
		return System.Text.Json.JsonSerializer.Deserialize<TValue>(ref reader, jsonTypeInfo);
	}
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public object? Deserialize(ref Utf8JsonReader reader, Type returnType, JsonSerializerOptions? options = null){
		return System.Text.Json.JsonSerializer.Deserialize(ref reader, returnType, options ?? DfltOptions);
	}
	public object? Deserialize(ref Utf8JsonReader reader, JsonTypeInfo jsonTypeInfo){
		return System.Text.Json.JsonSerializer.Deserialize(ref reader, jsonTypeInfo);
	}
	public object? Deserialize(ref Utf8JsonReader reader, Type returnType, JsonSerializerContext context){
		return System.Text.Json.JsonSerializer.Deserialize(ref reader, returnType, context);
	}

	// ===== DeserializeAsync（Stream / PipeReader）=====

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public ValueTask<TValue?> DeserializeAsync<TValue>(Stream utf8Json, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.DeserializeAsync<TValue>(utf8Json, options ?? DfltOptions, cancellationToken);
	}
	public ValueTask<TValue?> DeserializeAsync<TValue>(Stream utf8Json, JsonTypeInfo<TValue> jsonTypeInfo, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.DeserializeAsync<TValue>(utf8Json, jsonTypeInfo, cancellationToken);
	}
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public ValueTask<object?> DeserializeAsync(Stream utf8Json, Type returnType, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.DeserializeAsync(utf8Json, returnType, options ?? DfltOptions, cancellationToken);
	}
	public ValueTask<object?> DeserializeAsync(Stream utf8Json, Type returnType, JsonSerializerContext context, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.DeserializeAsync(utf8Json, returnType, context, cancellationToken);
	}
	public ValueTask<object?> DeserializeAsync(Stream utf8Json, JsonTypeInfo jsonTypeInfo, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.DeserializeAsync(utf8Json, jsonTypeInfo, cancellationToken);
	}

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public ValueTask<TValue?> DeserializeAsync<TValue>(PipeReader reader, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.DeserializeAsync<TValue>(reader, options ?? DfltOptions, cancellationToken);
	}
	public ValueTask<TValue?> DeserializeAsync<TValue>(PipeReader reader, JsonTypeInfo<TValue> jsonTypeInfo, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.DeserializeAsync<TValue>(reader, jsonTypeInfo, cancellationToken);
	}
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public ValueTask<object?> DeserializeAsync(PipeReader reader, Type returnType, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.DeserializeAsync(reader, returnType, options ?? DfltOptions, cancellationToken);
	}
	public ValueTask<object?> DeserializeAsync(PipeReader reader, Type returnType, JsonSerializerContext context, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.DeserializeAsync(reader, returnType, context, cancellationToken);
	}
	public ValueTask<object?> DeserializeAsync(PipeReader reader, JsonTypeInfo jsonTypeInfo, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.DeserializeAsync(reader, jsonTypeInfo, cancellationToken);
	}

	// ===== DeserializeAsyncEnumerable：JSON 值序列流式反序列化 =====

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public IAsyncEnumerable<TValue?> DeserializeAsyncEnumerable<TValue>(Stream utf8Json, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.DeserializeAsyncEnumerable<TValue>(utf8Json, options ?? DfltOptions, cancellationToken);
	}
	public IAsyncEnumerable<TValue?> DeserializeAsyncEnumerable<TValue>(Stream utf8Json, JsonTypeInfo<TValue> jsonTypeInfo, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.DeserializeAsyncEnumerable<TValue>(utf8Json, jsonTypeInfo, cancellationToken);
	}
	public IAsyncEnumerable<TValue?> DeserializeAsyncEnumerable<TValue>(Stream utf8Json, JsonTypeInfo<TValue> jsonTypeInfo, bool topLevelValues, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.DeserializeAsyncEnumerable<TValue>(utf8Json, jsonTypeInfo, topLevelValues, cancellationToken);
	}
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public IAsyncEnumerable<TValue?> DeserializeAsyncEnumerable<TValue>(Stream utf8Json, bool topLevelValues, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.DeserializeAsyncEnumerable<TValue>(utf8Json, topLevelValues, options ?? DfltOptions, cancellationToken);
	}

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public IAsyncEnumerable<TValue?> DeserializeAsyncEnumerable<TValue>(PipeReader reader, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.DeserializeAsyncEnumerable<TValue>(reader, options ?? DfltOptions, cancellationToken);
	}
	public IAsyncEnumerable<TValue?> DeserializeAsyncEnumerable<TValue>(PipeReader reader, JsonTypeInfo<TValue> jsonTypeInfo, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.DeserializeAsyncEnumerable<TValue>(reader, jsonTypeInfo, cancellationToken);
	}
	public IAsyncEnumerable<TValue?> DeserializeAsyncEnumerable<TValue>(PipeReader reader, JsonTypeInfo<TValue> jsonTypeInfo, bool topLevelValues, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.DeserializeAsyncEnumerable<TValue>(reader, jsonTypeInfo, topLevelValues, cancellationToken);
	}
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	public IAsyncEnumerable<TValue?> DeserializeAsyncEnumerable<TValue>(PipeReader reader, bool topLevelValues, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default){
		return System.Text.Json.JsonSerializer.DeserializeAsyncEnumerable<TValue>(reader, topLevelValues, options ?? DfltOptions, cancellationToken);
	}
}

/// 與官方 System.Text.Json 一致的 AOT 警告文案（IL2026 / IL3050）。
internal static class JsonAotMsg{
	public const string Unreferenced = "JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.";
	public const string DynamicCode = "JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use System.Text.Json source generation for native AOT applications.";
}