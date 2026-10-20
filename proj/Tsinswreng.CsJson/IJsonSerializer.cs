namespace Tsinswreng.CsJson;

using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsCore;






[Doc("""
完整 JSON 序列化接口：與官方 System.Text.Json.JsonSerializer 的公開 API 面逐一對應，
只是把靜態類改為實例接口（方便 DI 注入）。方法名、重載、參數形狀照抄官方，不增不減。
TypeInfo/context 的來源由調用方以 JsonTypeInfo / JsonSerializerContext / JsonSerializerOptions 傳入。
只帶 JsonSerializerOptions 的重載與官方一樣標記 RequiresUnreferencedCode / RequiresDynamicCode：
AOT 下必須掛源生成 TypeInfoResolver，否則走反射路徑（編譯期 IL 警告）。
""")]
public interface IJsonSerializer{
	
	[Doc(@$"API函數 不傳options參數時默認用此")]
	public JsonSerializerOptions? Options{get;set;}

	// ===== Serialize：object → JSON 字符串 =====

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	string Serialize<TValue>(TValue value, JsonSerializerOptions? options = null);
	string Serialize<TValue>(TValue value, JsonTypeInfo<TValue> jsonTypeInfo);
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	string Serialize(object? value, Type inputType, JsonSerializerOptions? options = null);
	string Serialize(object? value, JsonTypeInfo jsonTypeInfo);
	string Serialize(object? value, Type inputType, JsonSerializerContext context);

	// ===== Serialize：寫入 Utf8JsonWriter =====

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	void Serialize<TValue>(Utf8JsonWriter writer, TValue value, JsonSerializerOptions? options = null);
	void Serialize<TValue>(Utf8JsonWriter writer, TValue value, JsonTypeInfo<TValue> jsonTypeInfo);
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	void Serialize(Utf8JsonWriter writer, object? value, Type inputType, JsonSerializerOptions? options = null);
	void Serialize(Utf8JsonWriter writer, object? value, JsonTypeInfo jsonTypeInfo);
	void Serialize(Utf8JsonWriter writer, object? value, Type inputType, JsonSerializerContext context);

	// ===== Serialize：寫入 Stream =====

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	void Serialize<TValue>(Stream utf8Json, TValue value, JsonSerializerOptions? options = null);
	void Serialize<TValue>(Stream utf8Json, TValue value, JsonTypeInfo<TValue> jsonTypeInfo);
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	void Serialize(Stream utf8Json, object? value, Type inputType, JsonSerializerOptions? options = null);
	void Serialize(Stream utf8Json, object? value, JsonTypeInfo jsonTypeInfo);
	void Serialize(Stream utf8Json, object? value, Type inputType, JsonSerializerContext context);

	// ===== SerializeAsync（Stream / PipeWriter）=====

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	Task SerializeAsync<TValue>(Stream utf8Json, TValue value, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default);
	Task SerializeAsync<TValue>(Stream utf8Json, TValue value, JsonTypeInfo<TValue> jsonTypeInfo, CancellationToken cancellationToken = default);
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	Task SerializeAsync(Stream utf8Json, object? value, Type inputType, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default);
	Task SerializeAsync(Stream utf8Json, object? value, JsonTypeInfo jsonTypeInfo, CancellationToken cancellationToken = default);
	Task SerializeAsync(Stream utf8Json, object? value, Type inputType, JsonSerializerContext context, CancellationToken cancellationToken = default);

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	Task SerializeAsync<TValue>(PipeWriter writer, TValue value, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default);
	Task SerializeAsync<TValue>(PipeWriter writer, TValue value, JsonTypeInfo<TValue> jsonTypeInfo, CancellationToken cancellationToken = default);
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	Task SerializeAsync(PipeWriter writer, object? value, Type inputType, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default);
	Task SerializeAsync(PipeWriter writer, object? value, JsonTypeInfo jsonTypeInfo, CancellationToken cancellationToken = default);
	Task SerializeAsync(PipeWriter writer, object? value, Type inputType, JsonSerializerContext context, CancellationToken cancellationToken = default);

	// ===== SerializeToUtf8Bytes：object → UTF-8 bytes =====

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	byte[] SerializeToUtf8Bytes<TValue>(TValue value, JsonSerializerOptions? options = null);
	byte[] SerializeToUtf8Bytes<TValue>(TValue value, JsonTypeInfo<TValue> jsonTypeInfo);
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	byte[] SerializeToUtf8Bytes(object? value, Type inputType, JsonSerializerOptions? options = null);
	byte[] SerializeToUtf8Bytes(object? value, JsonTypeInfo jsonTypeInfo);
	byte[] SerializeToUtf8Bytes(object? value, Type inputType, JsonSerializerContext context);

	// ===== SerializeToElement：object → JsonElement 只讀視圖 =====

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	JsonElement SerializeToElement<TValue>(TValue value, JsonSerializerOptions? options = null);
	JsonElement SerializeToElement<TValue>(TValue value, JsonTypeInfo<TValue> jsonTypeInfo);
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	JsonElement SerializeToElement(object? value, Type inputType, JsonSerializerOptions? options = null);
	JsonElement SerializeToElement(object? value, JsonTypeInfo jsonTypeInfo);
	JsonElement SerializeToElement(object? value, Type inputType, JsonSerializerContext context);

	// ===== SerializeToNode：object → JsonNode 可變樹 =====

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	JsonNode? SerializeToNode<TValue>(TValue value, JsonSerializerOptions? options = null);
	JsonNode? SerializeToNode<TValue>(TValue value, JsonTypeInfo<TValue> jsonTypeInfo);
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	JsonNode? SerializeToNode(object? value, Type inputType, JsonSerializerOptions? options = null);
	JsonNode? SerializeToNode(object? value, JsonTypeInfo jsonTypeInfo);
	JsonNode? SerializeToNode(object? value, Type inputType, JsonSerializerContext context);

	// ===== SerializeToDocument：object → JsonDocument =====

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	JsonDocument SerializeToDocument<TValue>(TValue value, JsonSerializerOptions? options = null);
	JsonDocument SerializeToDocument<TValue>(TValue value, JsonTypeInfo<TValue> jsonTypeInfo);
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	JsonDocument SerializeToDocument(object? value, Type inputType, JsonSerializerOptions? options = null);
	JsonDocument SerializeToDocument(object? value, JsonTypeInfo jsonTypeInfo);
	JsonDocument SerializeToDocument(object? value, Type inputType, JsonSerializerContext context);

	// ===== Deserialize：JSON → object（string / utf8 bytes / span<char> / stream / document / element / node / reader）=====

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	TValue? Deserialize<TValue>(string json, JsonSerializerOptions? options = null);
	TValue? Deserialize<TValue>(string json, JsonTypeInfo<TValue> jsonTypeInfo);
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	object? Deserialize(string json, Type returnType, JsonSerializerOptions? options = null);
	object? Deserialize(string json, JsonTypeInfo jsonTypeInfo);
	object? Deserialize(string json, Type returnType, JsonSerializerContext context);

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	TValue? Deserialize<TValue>(ReadOnlySpan<byte> utf8Json, JsonSerializerOptions? options = null);
	TValue? Deserialize<TValue>(ReadOnlySpan<byte> utf8Json, JsonTypeInfo<TValue> jsonTypeInfo);
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	object? Deserialize(ReadOnlySpan<byte> utf8Json, Type returnType, JsonSerializerOptions? options = null);
	object? Deserialize(ReadOnlySpan<byte> utf8Json, JsonTypeInfo jsonTypeInfo);
	object? Deserialize(ReadOnlySpan<byte> utf8Json, Type returnType, JsonSerializerContext context);

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	TValue? Deserialize<TValue>(ReadOnlySpan<char> json, JsonSerializerOptions? options = null);
	TValue? Deserialize<TValue>(ReadOnlySpan<char> json, JsonTypeInfo<TValue> jsonTypeInfo);
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	object? Deserialize(ReadOnlySpan<char> json, Type returnType, JsonSerializerOptions? options = null);
	object? Deserialize(ReadOnlySpan<char> json, JsonTypeInfo jsonTypeInfo);
	object? Deserialize(ReadOnlySpan<char> json, Type returnType, JsonSerializerContext context);

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	TValue? Deserialize<TValue>(Stream utf8Json, JsonSerializerOptions? options = null);
	TValue? Deserialize<TValue>(Stream utf8Json, JsonTypeInfo<TValue> jsonTypeInfo);
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	object? Deserialize(Stream utf8Json, Type returnType, JsonSerializerOptions? options = null);
	object? Deserialize(Stream utf8Json, JsonTypeInfo jsonTypeInfo);
	object? Deserialize(Stream utf8Json, Type returnType, JsonSerializerContext context);

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	TValue? Deserialize<TValue>(JsonDocument document, JsonSerializerOptions? options = null);
	TValue? Deserialize<TValue>(JsonDocument document, JsonTypeInfo<TValue> jsonTypeInfo);
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	object? Deserialize(JsonDocument document, Type returnType, JsonSerializerOptions? options = null);
	object? Deserialize(JsonDocument document, JsonTypeInfo jsonTypeInfo);
	object? Deserialize(JsonDocument document, Type returnType, JsonSerializerContext context);

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	TValue? Deserialize<TValue>(JsonElement element, JsonSerializerOptions? options = null);
	TValue? Deserialize<TValue>(JsonElement element, JsonTypeInfo<TValue> jsonTypeInfo);
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	object? Deserialize(JsonElement element, Type returnType, JsonSerializerOptions? options = null);
	object? Deserialize(JsonElement element, JsonTypeInfo jsonTypeInfo);
	object? Deserialize(JsonElement element, Type returnType, JsonSerializerContext context);

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	TValue? Deserialize<TValue>(JsonNode? node, JsonSerializerOptions? options = null);
	TValue? Deserialize<TValue>(JsonNode? node, JsonTypeInfo<TValue> jsonTypeInfo);
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	object? Deserialize(JsonNode? node, Type returnType, JsonSerializerOptions? options = null);
	object? Deserialize(JsonNode? node, JsonTypeInfo jsonTypeInfo);
	object? Deserialize(JsonNode? node, Type returnType, JsonSerializerContext context);

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	TValue? Deserialize<TValue>(ref Utf8JsonReader reader, JsonSerializerOptions? options = null);
	TValue? Deserialize<TValue>(ref Utf8JsonReader reader, JsonTypeInfo<TValue> jsonTypeInfo);
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	object? Deserialize(ref Utf8JsonReader reader, Type returnType, JsonSerializerOptions? options = null);
	object? Deserialize(ref Utf8JsonReader reader, JsonTypeInfo jsonTypeInfo);
	object? Deserialize(ref Utf8JsonReader reader, Type returnType, JsonSerializerContext context);

	// ===== DeserializeAsync（Stream / PipeReader）=====

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	ValueTask<TValue?> DeserializeAsync<TValue>(Stream utf8Json, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default);
	ValueTask<TValue?> DeserializeAsync<TValue>(Stream utf8Json, JsonTypeInfo<TValue> jsonTypeInfo, CancellationToken cancellationToken = default);
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	ValueTask<object?> DeserializeAsync(Stream utf8Json, Type returnType, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default);
	ValueTask<object?> DeserializeAsync(Stream utf8Json, Type returnType, JsonSerializerContext context, CancellationToken cancellationToken = default);
	ValueTask<object?> DeserializeAsync(Stream utf8Json, JsonTypeInfo jsonTypeInfo, CancellationToken cancellationToken = default);

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	ValueTask<TValue?> DeserializeAsync<TValue>(PipeReader reader, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default);
	ValueTask<TValue?> DeserializeAsync<TValue>(PipeReader reader, JsonTypeInfo<TValue> jsonTypeInfo, CancellationToken cancellationToken = default);
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	ValueTask<object?> DeserializeAsync(PipeReader reader, Type returnType, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default);
	ValueTask<object?> DeserializeAsync(PipeReader reader, Type returnType, JsonSerializerContext context, CancellationToken cancellationToken = default);
	ValueTask<object?> DeserializeAsync(PipeReader reader, JsonTypeInfo jsonTypeInfo, CancellationToken cancellationToken = default);

	// ===== DeserializeAsyncEnumerable：JSON 值序列流式反序列化 =====

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	IAsyncEnumerable<TValue?> DeserializeAsyncEnumerable<TValue>(Stream utf8Json, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default);
	IAsyncEnumerable<TValue?> DeserializeAsyncEnumerable<TValue>(Stream utf8Json, JsonTypeInfo<TValue> jsonTypeInfo, CancellationToken cancellationToken = default);
	IAsyncEnumerable<TValue?> DeserializeAsyncEnumerable<TValue>(Stream utf8Json, JsonTypeInfo<TValue> jsonTypeInfo, bool topLevelValues, CancellationToken cancellationToken = default);
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	IAsyncEnumerable<TValue?> DeserializeAsyncEnumerable<TValue>(Stream utf8Json, bool topLevelValues, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default);

	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	IAsyncEnumerable<TValue?> DeserializeAsyncEnumerable<TValue>(PipeReader reader, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default);
	IAsyncEnumerable<TValue?> DeserializeAsyncEnumerable<TValue>(PipeReader reader, JsonTypeInfo<TValue> jsonTypeInfo, CancellationToken cancellationToken = default);
	IAsyncEnumerable<TValue?> DeserializeAsyncEnumerable<TValue>(PipeReader reader, JsonTypeInfo<TValue> jsonTypeInfo, bool topLevelValues, CancellationToken cancellationToken = default);
	[RequiresUnreferencedCode(JsonAotMsg.Unreferenced)]
	[RequiresDynamicCode(JsonAotMsg.DynamicCode)]
	IAsyncEnumerable<TValue?> DeserializeAsyncEnumerable<TValue>(PipeReader reader, bool topLevelValues, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default);
}
