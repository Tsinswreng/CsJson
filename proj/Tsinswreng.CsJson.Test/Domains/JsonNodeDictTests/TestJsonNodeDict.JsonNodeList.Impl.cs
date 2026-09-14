//Impl:分片三 JsonNodeList。聲明見 TestJsonNodeDict.JsonNodeList.cs
using System.Text.Json.Nodes;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsJson.Test.Domains.JsonNodeDictTests;

public partial class TestJsonNodeDict{
	public partial void RegisterJsonNodeList(ITestNode Node){
		var R = Node.MkTestFnRegister(
			typeof(TestJsonNodeDict),
			[typeof(JsonNodeList)],
			["Item"],
			"JsonNodeList"
		).Register;
		R(nameof(List_Ctor_Prefills_Slots_Count_Matches), List_Ctor_Prefills_Slots_Count_Matches!);
		R(nameof(List_Read_Container_Same_Instance_On_Repeat), List_Read_Container_Same_Instance_On_Repeat!);
		R(nameof(U_List_Read_Scalar_Not_Stored_In_Slot), U_List_Read_Scalar_Not_Stored_In_Slot!);
		R(nameof(U_List_Add_Appends_And_Materializes), U_List_Add_Appends_And_Materializes!);
		R(nameof(U_List_Index_Set_Only_Adapted_Not_Src), U_List_Index_Set_Only_Adapted_Not_Src!);
		R(nameof(List_Insert_Shifts_Adapted), List_Insert_Shifts_Adapted!);
		R(nameof(List_RemoveAt_Shifts_Adapted), List_RemoveAt_Shifts_Adapted!);
		R(nameof(List_Remove_By_Item), List_Remove_By_Item!);
		R(nameof(U_List_Clear_Clears_Both), U_List_Clear_Clears_Both!);
		R(nameof(List_Contains_IndexOf_Cover_Adapted), List_Contains_IndexOf_Cover_Adapted!);
	}

	// 構造即預填槽位:Count 立即反映數組長度。
	public partial async Task<nil> List_Ctor_Prefills_Slots_Count_Matches(obj? O){
		var T = Assert.IsTrue;
		var arr = (JsonArray)JsonNode.Parse("""[1,2,3]""")!;
		var list = new JsonNodeList(arr);
		T(list.Count == 3);
		T(JsonNodeListAcc.Adapted(list).Count == 3);
		return NIL;
	}

	// 讀容器兩次:同一實例。
	public partial async Task<nil> List_Read_Container_Same_Instance_On_Repeat(obj? O){
		var T = Assert.IsTrue;
		var arr = (JsonArray)JsonNode.Parse("""[{"a":1},{"b":2}]""")!;
		var list = new JsonNodeList(arr);
		var first = list[0];
		var second = list[0];
		T(ReferenceEquals(first, second));
		return NIL;
	}

	// 標量讀:直接返回,不寫槽位。
	public partial async Task<nil> U_List_Read_Scalar_Not_Stored_In_Slot(obj? O){
		var T = Assert.IsTrue;
		var arr = (JsonArray)JsonNode.Parse("""[1,"x"]""")!;
		var list = new JsonNodeList(arr);
		T(list[0] is i64 n && n == 1);
		T(JsonNodeListAcc.Adapted(list)[0] is null);
		T(list[1] is str s && s == "x");
		T(JsonNodeListAcc.Adapted(list)[1] is null);
		return NIL;
	}

	// 首次寫(Add)觸發物化:來源標量全部落槽,之後一切以槽為準。
	public partial async Task<nil> U_List_Add_Appends_And_Materializes(obj? O){
		var T = Assert.IsTrue;
		var arr = (JsonArray)JsonNode.Parse("""[1,2]""")!;
		var list = new JsonNodeList(arr);
		list.Add((i64)3);
		T(list.Count == 3);
		T(JsonNodeListAcc.Dirty(list));
		// 物化:未讀過的來源標量也進槽了
		T(JsonNodeListAcc.Adapted(list)[0] is i64 a && a == 1);
		T(JsonNodeListAcc.Adapted(list)[1] is i64 b && b == 2);
		T(JsonNodeListAcc.Adapted(list)[2] is i64 c && c == 3);
		return NIL;
	}

	// 索引 set:只寫槽位,不碰來源。
	public partial async Task<nil> U_List_Index_Set_Only_Adapted_Not_Src(obj? O){
		var T = Assert.IsTrue;
		var arr = (JsonArray)JsonNode.Parse("""[1,2,3]""")!;
		var list = new JsonNodeList(arr);
		list[1] = "two";
		T(list[1] is str s && s == "two");
		T(JsonNodeListAcc.Adapted(list)[1] is str t && t == "two");
		T(JsonNodeListAcc.Src(list)[1]!.GetValue<i64>() == 2);
		return NIL;
	}

	// Insert:只動槽位,順序正確。
	public partial async Task<nil> List_Insert_Shifts_Adapted(obj? O){
		var T = Assert.IsTrue;
		var arr = (JsonArray)JsonNode.Parse("""[1,3]""")!;
		var list = new JsonNodeList(arr);
		list.Insert(1, (i64)2);
		T(list.Count == 3);
		T(list[0] is i64 a && a == 1);
		T(list[1] is i64 b && b == 2);
		T(list[2] is i64 c && c == 3);
		return NIL;
	}

	// RemoveAt:只動槽位,順序正確。
	public partial async Task<nil> List_RemoveAt_Shifts_Adapted(obj? O){
		var T = Assert.IsTrue;
		var arr = (JsonArray)JsonNode.Parse("""[1,2,3]""")!;
		var list = new JsonNodeList(arr);
		list.RemoveAt(1);
		T(list.Count == 2);
		T(list[0] is i64 a && a == 1);
		T(list[1] is i64 c && c == 3);
		return NIL;
	}

	// Remove(obj):按值刪除。
	public partial async Task<nil> List_Remove_By_Item(obj? O){
		var T = Assert.IsTrue;
		var arr = (JsonArray)JsonNode.Parse("""[1,2,3]""")!;
		var list = new JsonNodeList(arr);
		T(list.Remove((i64)2));
		T(list.Count == 2);
		T(!list.Contains(2));
		return NIL;
	}

	// Clear:槽與來源雙清。
	public partial async Task<nil> U_List_Clear_Clears_Both(obj? O){
		var T = Assert.IsTrue;
		var arr = (JsonArray)JsonNode.Parse("""[1,2,3]""")!;
		var list = new JsonNodeList(arr);
		list.Clear();
		T(list.Count == 0);
		T(JsonNodeListAcc.Adapted(list).Count == 0);
		T(JsonNodeListAcc.Src(list).Count == 0);
		return NIL;
	}

	// Contains/IndexOf:覆蓋槽位真相。
	public partial async Task<nil> List_Contains_IndexOf_Cover_Adapted(obj? O){
		var T = Assert.IsTrue;
		var arr = (JsonArray)JsonNode.Parse("""[1,2,3]""")!;
		var list = new JsonNodeList(arr);
		list[1] = "two";
		T(list.Contains("two"));
		T(list.IndexOf("two") == 1);
		T(!list.Contains(2));
		return NIL;
	}
}