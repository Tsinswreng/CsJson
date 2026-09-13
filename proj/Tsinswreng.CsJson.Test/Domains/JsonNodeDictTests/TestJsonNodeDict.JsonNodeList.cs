using System.Text.Json.Nodes;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsJson.Test.Domains.JsonNodeDictTests;

// 分片三:JsonNodeList(JsonArray 的 IList 視圖)懶適配 + 緩存 + 同步變更。
public partial class TestJsonNodeDict{
	public void RegisterJsonNodeList(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestJsonNodeDict),
			[],
			[nameof(JsonNodeDict.Adapt)],
			"JsonNodeList"
		);
		var R = register.Register;
		var T = Assert.IsTrue;

		R("List_Adapt_Nested_Items_Are_Views", async(_)=>{
			var node = JsonNode.Parse("""[{"a":1},"x",2,null]""")!;
			var list = (JsonNodeList)JsonNodeDict.Adapt(node)!;
			T(list.Count == 4);
			T(list[0] is JsonNodeDict);
			T(list[1] is str s && s == "x");
			T(list[2] is i64 n && n == 2);
			T(list[3] is null);
			return NIL;
		});

		R("List_Same_Instance_On_Repeat_Read_Index", async(_)=>{
			var node = JsonNode.Parse("""[{"a":1},{"b":2}]""")!;
			var list = (JsonNodeList)JsonNodeDict.Adapt(node)!;
			var first = list[0];
			var second = list[0];
			T(ReferenceEquals(first, second));
			return NIL;
		});

		R("List_Index_Set_Writes_Tree", async(_)=>{
			var node = JsonNode.Parse("""[1,2,3]""")!;
			var list = (JsonNodeList)JsonNodeDict.Adapt(node)!;
			list[1] = "two";
			T(list[1] is str s && s == "two");
			T(list.Arr.ToJsonString() == """[1,"two",3]""");
			return NIL;
		});

		R("List_Insert_Shifts_And_Syncs", async(_)=>{
			var node = JsonNode.Parse("""[1,3]""")!;
			var list = (JsonNodeList)JsonNodeDict.Adapt(node)!;
			list.Insert(1, 2);
			T(list.Count == 3);
			T(list.Arr.ToJsonString() == """[1,2,3]""");
			return NIL;
		});

		R("List_RemoveAt_Syncs", async(_)=>{
			var node = JsonNode.Parse("""[1,2,3]""")!;
			var list = (JsonNodeList)JsonNodeDict.Adapt(node)!;
			list.RemoveAt(1);
			T(list.Count == 2);
			T(list.Arr.ToJsonString() == """[1,3]""");
			return NIL;
		});

		R("List_Add_Nested_Dict_Then_Mutate_It_Through_Tree", async(_)=>{
			var node = JsonNode.Parse("""[]""")!;
			var list = (JsonNodeList)JsonNodeDict.Adapt(node)!;
			var nd = new JsonNodeDict();
			nd["k"] = "v";
			list.Add(nd);
			T(list.Count == 1);
			// 取出的項是同一視圖實例
			var got = (JsonNodeDict)list[0]!;
			T(got["k"] is str s && s == "v");
			return NIL;
		});

		R("List_From_Dict_Chain_All_The_Way_Down", async(_)=>{
			var node = JsonNode.Parse("""{"data":{"items":[{"id":1},{"id":2}]}}""")!;
			var dict = (JsonNodeDict)JsonNodeDict.Adapt(node)!;
			var data = (JsonNodeDict)dict["data"]!;
			var items = (JsonNodeList)data["items"]!;
			var first = (JsonNodeDict)items[0]!;
			T((i64)first["id"]! == 1);
			// 最深層修改作用到樹
			first["id"] = 99;
			T(dict.Obj.ToJsonString() == """{"data":{"items":[{"id":99},{"id":2}]}}""");
			return NIL;
		});
	}
}