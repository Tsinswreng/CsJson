using System.Text.Json.Nodes;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsJson.Test.Domains.JsonNodeDictTests;

// 分片二:懶適配 + 緩存——取值拿到的必須是同一實例,修改要作用到底層樹並被下次讀到。
public partial class TestJsonNodeDict{
	public void RegisterCachedMutate(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestJsonNodeDict),
			[],
			[nameof(JsonNodeDict.Adapt)],
			"CachedMutate"
		);
		var R = register.Register;
		var T = Assert.IsTrue;

		R("Cached_Same_Instance_On_Repeat_Read_Nested_Dict", async(_)=>{
			var node = JsonNode.Parse("""{"user":{"name":"a"}}""")!;
			var dict = (JsonNodeDict)JsonNodeDict.Adapt(node)!;
			var first = dict["user"];
			var second = dict["user"];
			T(ReferenceEquals(first, second));
			return NIL;
		});

		R("Cached_Same_Instance_On_Repeat_Read_List", async(_)=>{
			var node = JsonNode.Parse("""{"tags":["x","y"]}""")!;
			var dict = (JsonNodeDict)JsonNodeDict.Adapt(node)!;
			var first = dict["tags"];
			var second = dict["tags"];
			T(ReferenceEquals(first, second));
			return NIL;
		});

		R("Mutate_LazyAdapted_List_Add_Then_ReRead_Sees_Change", async(_)=>{
			// 從 dict 取出 list 再 Add:必須生效(緩存同一實例 + 改底層樹)
			var node = JsonNode.Parse("""{"tags":["x","y"]}""")!;
			var dict = (JsonNodeDict)JsonNodeDict.Adapt(node)!;
			var tags = (JsonNodeList)dict["tags"]!;
			tags.Add("z");
			T(tags.Count == 3);
			// 再讀依然是同一實例,且能看到 z
			var again = (JsonNodeList)dict["tags"]!;
			T(ReferenceEquals(tags, again));
			T(again.Count == 3);
			T((str)again[2]! == "z");
			// 底層樹也變了
			T(dict.Obj["tags"]!.ToJsonString() == """["x","y","z"]""");
			return NIL;
		});

		R("Mutate_Nested_Dict_Set_Key_Then_ReRead_Sees_Change", async(_)=>{
			var node = JsonNode.Parse("""{"user":{"name":"a"}}""")!;
			var dict = (JsonNodeDict)JsonNodeDict.Adapt(node)!;
			var user = (JsonNodeDict)dict["user"]!;
			user["name"] = "b";
			var again = (JsonNodeDict)dict["user"]!;
			T(ReferenceEquals(user, again));
			T((str)again["name"]! == "b");
			T(dict.Obj["user"]!.ToJsonString() == """{"name":"b"}""");
			return NIL;
		});

		R("Set_Data_Then_Read_Writes_Tree", async(_)=>{
			var node = JsonNode.Parse("""{"old":1}""")!;
			var dict = (JsonNodeDict)JsonNodeDict.Adapt(node)!;
			dict["new"] = "abc";
			T(dict["new"] is str s && s == "abc");
			T(dict.Obj.ContainsKey("new"));
			T(dict.Obj["new"]!.GetValue<str>() == "abc");
			return NIL;
		});

		R("Remove_And_Add_Rebuild_Cache", async(_)=>{
			var node = JsonNode.Parse("""{"a":1,"b":2}""")!;
			var dict = (JsonNodeDict)JsonNodeDict.Adapt(node)!;
			dict.Remove("a");
			T(!dict.ContainsKey("a"));
			T(dict.Count == 1);
			dict.Add("c", 3);
			T((i64)dict["c"]! == 3);
			return NIL;
		});

		R("Assign_View_Then_ReadBack_Same_Instance_To_Mutate", async(_)=>{
			// 賦值語義:outer["child"] = src 是拷貝值(不是引用同一棵樹)。
			// 正確用法:賦值後「從樹裏讀回同一實例」再改,修改才寫進樹。
			var srcNode = JsonNode.Parse("""{"a":1}""")!;
			var src = new JsonNodeDict((JsonObject)srcNode);
			var outerNode = JsonNode.Parse("""{"x":0}""")!;
			var outer = new JsonNodeDict((JsonObject)outerNode);

			outer["child"] = src;
			var back = (JsonNodeDict)outer["child"]!;
			back["a"] = 999;   // 改讀回來的同一實例
			T((i64)((JsonNodeDict)outer["child"]!)["a"]! == 999);

			// 對照:直接改 src(拷貝語義),不影響樹裏那份
			src["a"] = -1;
			T((i64)((JsonNodeDict)outer["child"]!)["a"]! == 999); // 仍 999,拷貝不受影響
			return NIL;
		});

		R("Assign_Node_By_Obj_Property_Shares_Tree", async(_)=>{
			// 若真要共享同一棵樹:直接傳節點本人 outer.Obj["child"] = src.Obj
			var srcNode = JsonNode.Parse("""{"a":1}""")!;
			var src = new JsonNodeDict((JsonObject)srcNode);
			var outerNode = JsonNode.Parse("""{"x":0}""")!;
			var outer = new JsonNodeDict((JsonObject)outerNode);

			outer.Obj["child"] = src.Obj;
			src["a"] = 999;
			T((i64)outer.Obj["child"]!["a"]!.GetValue<i64>() == 999);
			return NIL;
		});
	}
}