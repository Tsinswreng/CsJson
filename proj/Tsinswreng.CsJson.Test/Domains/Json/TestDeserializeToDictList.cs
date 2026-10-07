using System.Text.Json;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsJson.Test.Domains.Json;

// 分片:測 IJsonSerializerExtn.DeserializeToDictList。
// 驗證:DictJson 任意根(對象/數組/標量)、數字 i64 優先、默認單例允許註釋、options 可覆蓋。
public partial class TestDfltJsonSerializer{
	public void RegisterDeserializeToDictList(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestDfltJsonSerializer),
			[typeof(IJsonSerializerExtn)],
			[nameof(IJsonSerializerExtn.DeserializeToDictList)],
			"DeserializeToDictList"
		);
		var R = register.Register;
		var T = Assert.IsTrue;

		R("DeserializeToDictList_ObjectRoot_Returns_JsonNodeDict_I64", async(o)=>{
			var z = JsonSerializer.Inst;
			var dict = z.DeserializeToDictList("""{"a":1,"b":"x","c":true,"d":null,"e":[1,2]}""");
			T(dict is JsonNodeDict);
			var d = (JsonNodeDict)dict!;
			T((i64)d["a"]! == 1);
			T((str)d["b"]! == "x");
			T((bool)d["c"]! == true);
			T(d["d"] is null);
			T(d["e"] is JsonNodeList);
			var lst = (JsonNodeList)d["e"]!;
			T((i64)lst[0]! == 1 && (i64)lst[1]! == 2);
			return NIL;
		});

		R("DeserializeToDictList_ArrayRoot_Returns_JsonNodeList", async(o)=>{
			var z = JsonSerializer.Inst;
			var dict = z.DeserializeToDictList("""[1,2,3]""");
			T(dict is JsonNodeList);
			var lst = (JsonNodeList)dict!;
			T(lst.Count == 3);
			T((i64)lst[2]! == 3);
			return NIL;
		});

		R("DeserializeToDictList_ScalarRoot_Returns_Scalar", async(o)=>{
			var z = JsonSerializer.Inst;
			T((i64)z.DeserializeToDictList("42")! == 42);
			T(Math.Abs((f64)z.DeserializeToDictList("3.5")! - 3.5) < 1e-9);
			T((str)z.DeserializeToDictList("\"hi\"")! == "hi");
			T((bool)z.DeserializeToDictList("true")! == true);
			return NIL;
		});

		R("DeserializeToDictList_DefaultInst_Allows_Comments", async(o)=>{
			// 默認單例 Inst 的 options 已改為允許註釋(Skip),註釋 json 應能解析。
			var z = JsonSerializer.Inst;
			var dict = z.DeserializeToDictList("""{"a":1 /* note */, "b":2}""");
			T(dict is JsonNodeDict);
			var d = (JsonNodeDict)dict!;
			T(d.Count == 2);
			T((i64)d["b"]! == 2);
			return NIL;
		});

		R("DeserializeToDictList_ExplicitOptions_Skips_Comments", async(o)=>{
			// 顯式傳默認(options=null → 官方默認,註釋拒絕):帶註釋 json 應拋 JsonException。
			var z = JsonSerializer.Inst;
			var ok = false;
			try{
				z.DeserializeToDictList("""{"a":1 /* note */}""", new System.Text.Json.JsonSerializerOptions{
					ReadCommentHandling = System.Text.Json.JsonCommentHandling.Disallow,
					TypeInfoResolver = JsonNodeCtx.Default,
				});
			}catch(System.Text.Json.JsonException){
				ok = true;
			}
			T(ok);
			return NIL;
		});

		R("DeserializeToDictList_EmptyOrNull_Returns_Null", async(o)=>{
			var z = JsonSerializer.Inst;
			T(z.DeserializeToDictList(null!) is null);
			T(z.DeserializeToDictList("") is null);
			T(z.DeserializeToDictList("null") is null);
			return NIL;
		});

		R("DeserializeToDictList_RoundTrip_SerializeBack_FromDictChain", async(o)=>{
			// 復現 Ngan.Dict FromDict 鏈:產物(JsonNodeDict,含 JsonNodeList 嵌套)作為
			// IDictionary<str,obj?> 再序列化。業務 options 用 TypeInfoResolverChain:
			// DictJsonCtx(接口 TypeInfo)+ JsonNodeCtx(具體門面 JsonNodeDict/JsonNodeList TypeInfo)。
			var z = JsonSerializer.Inst;
			var dict = z.DeserializeToDictList("""{"a":1,"b":[2,3]}""");
			T(dict is JsonNodeDict);
			var ser = new JsonSerializer(new JsonSerializerOptions{
				TypeInfoResolverChain = { DictJsonCtx.Default, JsonNodeCtx.Default },
			});
			var json = ser.Serialize((IDictionary<str, obj?>)dict!);
			T(json == """{"a":1,"b":[2,3]}""");
			return NIL;
		});
	}
}