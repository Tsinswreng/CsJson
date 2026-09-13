using System.Text.Json.Nodes;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsJson.Test.Domains.JsonNodeDictTests;

// 分片一:JsonNodeDict 的適配(Adapt / ToNode / 數字規則)。
public partial class TestJsonNodeDict{
	public void RegisterAdapt(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestJsonNodeDict),
			[],
			[nameof(JsonNodeDict.Adapt)],
			"Adapt"
		);
		var R = register.Register;
		var T = Assert.IsTrue;

		R("Adapt_JsonObject_Root_Returns_JsonNodeDict", async(_)=>{
			var node = JsonNode.Parse("""{"a":1,"b":"x"}""")!;
			var adapted = JsonNodeDict.Adapt(node);
			T(adapted is JsonNodeDict);
			var dict = (JsonNodeDict)adapted!;
			T(dict.Count == 2);
			T(dict["a"] is i64 a && a == 1);
			T(dict["b"] is str b && b == "x");
			return NIL;
		});

		R("Adapt_Number_Priority_I64_Over_F64", async(_)=>{
			// 整數字面量 3 應得 i64 而非 f64
			var node = JsonNode.Parse("""{"n":3}""")!;
			var dict = (JsonNodeDict)JsonNodeDict.Adapt(node)!;
			T(dict["n"] is i64 && (i64)dict["n"]! == 3);
			return NIL;
		});

		R("Adapt_F64_When_Not_Integral", async(_)=>{
			var node = JsonNode.Parse("""{"n":3.5}""")!;
			var dict = (JsonNodeDict)JsonNodeDict.Adapt(node)!;
			T(dict["n"] is f64 && (f64)dict["n"]! == 3.5);
			return NIL;
		});

		R("Adapt_Bool_Str_Null", async(_)=>{
			var node = JsonNode.Parse("""{"t":true,"f":false,"s":"hi","z":null}""")!;
			var dict = (JsonNodeDict)JsonNodeDict.Adapt(node)!;
			T((bool)dict["t"]! == true);
			T((bool)dict["f"]! == false);
			T((str)dict["s"]! == "hi");
			T(dict["z"] is null);
			return NIL;
		});

		R("Adapt_Nested_Object_Is_JsonNodeDict_View", async(_)=>{
			var node = JsonNode.Parse("""{"user":{"name":"a"}}""")!;
			var dict = (JsonNodeDict)JsonNodeDict.Adapt(node)!;
			var user = dict["user"];
			T(user is JsonNodeDict);
			T(((JsonNodeDict)user!)["name"] is str name && name == "a");
			return NIL;
		});

		R("ToNode_Dict_Becomes_JsonObject", async(_)=>{
			var d = new Dictionary<str, obj?>{
				["k"] = "v",
				["n"] = 2,
			};
			var node = JsonNodeDict.ToNode(d);
			T(node is JsonObject obj && obj.Count == 2);
			T(node!.ToJsonString() == """{"k":"v","n":2}""");
			return NIL;
		});

		R("ToNode_Unsupported_Type_Throws", async(_)=>{
			var threw = false;
			try{
				JsonNodeDict.ToNode(new object());
			}catch(NotSupportedException){
				threw = true;
			}
			T(threw);
			return NIL;
		});
	}
}