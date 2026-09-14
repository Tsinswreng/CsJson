//Impl:分片一 Adapt。聲明見 TestJsonNodeDict.Adapt.cs
using System.Text.Json.Nodes;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsJson.Test.Domains.JsonNodeDictTests;

public partial class TestJsonNodeDict{
	public partial void RegisterAdapt(ITestNode Node){
		var R = Node.MkTestFnRegister(
			typeof(TestJsonNodeDict),
			[typeof(JsonNodeDict)],
			["Adapt"],
			"Adapt"
		).Register;
		R(nameof(Adapt_JsonObject_Returns_JsonNodeDict), Adapt_JsonObject_Returns_JsonNodeDict!);
		R(nameof(Adapt_JsonArray_Returns_JsonNodeList), Adapt_JsonArray_Returns_JsonNodeList!);
		R(nameof(Adapt_JsonValue_Number_Priority_I64), Adapt_JsonValue_Number_Priority_I64!);
		R(nameof(Adapt_JsonValue_F64_Bool_Str_Null), Adapt_JsonValue_F64_Bool_Str_Null!);
		R(nameof(Adapt_Null_Returns_Null), Adapt_Null_Returns_Null!);
		R(nameof(Adapt_Deep_Nested_Builds_View_Chain), Adapt_Deep_Nested_Builds_View_Chain!);
		R(nameof(U_Ctor_Inits_Internal_State), U_Ctor_Inits_Internal_State!);
	}

	// JsonObject 根適配成 JsonNodeDict。
	public partial async Task<nil> Adapt_JsonObject_Returns_JsonNodeDict(obj? O){
		var T = Assert.IsTrue;
		var node = JsonNode.Parse("""{"a":1,"b":"x"}""")!;
		var adapted = JsonNodeDictAcc.Adapt(null, node);
		T(adapted is JsonNodeDict);
		var dict = (JsonNodeDict)adapted!;
		T(dict.Count == 2);
		T(dict["a"] is i64 a && a == 1);
		T(dict["b"] is str b && b == "x");
		return NIL;
	}

	// JsonArray 根適配成 JsonNodeList。
	public partial async Task<nil> Adapt_JsonArray_Returns_JsonNodeList(obj? O){
		var T = Assert.IsTrue;
		var node = JsonNode.Parse("""[1,2]""")!;
		var adapted = JsonNodeDictAcc.Adapt(null, node);
		T(adapted is JsonNodeList);
		var list = (JsonNodeList)adapted!;
		T(list.Count == 2);
		return NIL;
	}

	// 整數字面量 3 應得 i64 而非 f64。
	public partial async Task<nil> Adapt_JsonValue_Number_Priority_I64(obj? O){
		var T = Assert.IsTrue;
		var node = JsonNode.Parse("""{"n":3}""")!;
		var dict = (JsonNodeDict)JsonNodeDictAcc.Adapt(null, node)!;
		T(dict["n"] is i64 && (i64)dict["n"]! == 3);
		return NIL;
	}

	// 小數得 f64;bool/str/null 各自對應。
	public partial async Task<nil> Adapt_JsonValue_F64_Bool_Str_Null(obj? O){
		var T = Assert.IsTrue;
		var node = JsonNode.Parse("""{"d":3.5,"t":true,"s":"hi","z":null}""")!;
		var dict = (JsonNodeDict)JsonNodeDictAcc.Adapt(null, node)!;
		T(dict["d"] is f64 && (f64)dict["d"]! == 3.5);
		T((bool)dict["t"]! == true);
		T((str)dict["s"]! == "hi");
		T(dict["z"] is null);
		return NIL;
	}

	// null 節點適配為 null。
	public partial async Task<nil> Adapt_Null_Returns_Null(obj? O){
		var T = Assert.IsTrue;
		T(JsonNodeDictAcc.Adapt(null, null) is null);
		return NIL;
	}

	// 深嵌套:一層層都是視圖,讀到底層能取值。
	public partial async Task<nil> Adapt_Deep_Nested_Builds_View_Chain(obj? O){
		var T = Assert.IsTrue;
		var node = JsonNode.Parse("""{"user":{"items":[{"id":1}]}}""")!;
		var dict = (JsonNodeDict)JsonNodeDictAcc.Adapt(null, node)!;
		var user = (JsonNodeDict)dict["user"]!;
		var items = (JsonNodeList)user["items"]!;
		var first = (JsonNodeDict)items[0]!;
		T((i64)first["id"]! == 1);
		return NIL;
	}

	// 構造 JsonNodeDict(JsonObject):Src 是同一棵樹、Adapted 空、Dirty false。
	public partial async Task<nil> U_Ctor_Inits_Internal_State(obj? O){
		var T = Assert.IsTrue;
		var tree = (JsonObject)JsonNode.Parse("""{"a":1}""")!;
		var dict = new JsonNodeDict(tree);
		T(ReferenceEquals(JsonNodeDictAcc.Src(dict), tree));
		T(JsonNodeDictAcc.Adapted(dict).Count == 0);
		T(!JsonNodeDictAcc.Dirty(dict));
		return NIL;
	}
}