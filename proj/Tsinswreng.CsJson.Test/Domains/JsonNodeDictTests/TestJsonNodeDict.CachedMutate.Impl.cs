//Impl:分片二 Mutate。聲明見 TestJsonNodeDict.CachedMutate.cs
using System.Text.Json.Nodes;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsJson.Test.Domains.JsonNodeDictTests;

// 簡單業務對象:驗證 dict 能直接放任意 obj?、讀回同一實例。
public class IdUser{
	public str Id = "";
}

public partial class TestJsonNodeDict{
	public partial void RegisterCachedMutate(ITestNode Node){
		var R = Node.MkTestFnRegister(
			typeof(TestJsonNodeDict),
			[typeof(JsonNodeDict)],
			["Item"],
			"CachedMutate"
		).Register;
		R(nameof(Read_Container_Same_Instance_On_Repeat), Read_Container_Same_Instance_On_Repeat!);
		R(nameof(U_Read_Scalar_Not_Cached_In_Adapted), U_Read_Scalar_Not_Cached_In_Adapted!);
		R(nameof(U_Read_Container_Cached_In_Adapted), U_Read_Container_Cached_In_Adapted!);
		R(nameof(Mutate_Taken_Container_Seen_On_ReRead), Mutate_Taken_Container_Seen_On_ReRead!);
		R(nameof(U_Write_Indexer_Only_Adapted_Not_Src), U_Write_Indexer_Only_Adapted_Not_Src!);
		R(nameof(Write_Business_Object_Assignable_And_Read_Back), Write_Business_Object_Assignable_And_Read_Back!);
		R(nameof(Add_New_Key_Goes_To_Adapted), Add_New_Key_Goes_To_Adapted!);
		R(nameof(Add_Duplicate_Key_Throws), Add_Duplicate_Key_Throws!);
		R(nameof(U_Remove_Deletes_Both_Src_And_Adapted), U_Remove_Deletes_Both_Src_And_Adapted!);
		R(nameof(U_Clear_Clears_Both), U_Clear_Clears_Both!);
		R(nameof(Count_Keys_Union_Of_Src_And_Adapted), Count_Keys_Union_Of_Src_And_Adapted!);
		R(nameof(ContainsKey_Sees_Union), ContainsKey_Sees_Union!);
		R(nameof(TryGetValue_Sees_Union), TryGetValue_Sees_Union!);
	}

	// 讀容器兩次:必須同一實例(修改可見)。
	public partial async Task<nil> Read_Container_Same_Instance_On_Repeat(obj? O){
		var T = Assert.IsTrue;
		var node = JsonNode.Parse("""{"user":{"name":"a"}}""")!;
		var dict = new JsonNodeDict((JsonObject)node);
		var first = dict["user"];
		var second = dict["user"];
		T(ReferenceEquals(first, second));
		return NIL;
	}

	// 標量讀:直接返回,不進 Adapted(重適配零成本)。
	public partial async Task<nil> U_Read_Scalar_Not_Cached_In_Adapted(obj? O){
		var T = Assert.IsTrue;
		var node = JsonNode.Parse("""{"n":3}""")!;
		var dict = new JsonNodeDict((JsonObject)node);
		T((i64)dict["n"]! == 3);
		T(!JsonNodeDictAcc.Adapted(dict).ContainsKey("n"));
		return NIL;
	}

	// 容器讀:入 Adapted(同一實例契約)。
	public partial async Task<nil> U_Read_Container_Cached_In_Adapted(obj? O){
		var T = Assert.IsTrue;
		var node = JsonNode.Parse("""{"user":{"name":"a"}}""")!;
		var dict = new JsonNodeDict((JsonObject)node);
		var taken = dict["user"];
		T(ReferenceEquals(JsonNodeDictAcc.Adapted(dict)["user"], taken));
		return NIL;
	}

	// 取出容器改,再讀:同一實例看到修改。
	public partial async Task<nil> Mutate_Taken_Container_Seen_On_ReRead(obj? O){
		var T = Assert.IsTrue;
		var node = JsonNode.Parse("""{"user":{"name":"a"}}""")!;
		var dict = new JsonNodeDict((JsonObject)node);
		var user = (JsonNodeDict)dict["user"]!;
		user["name"] = "b";
		var again = (JsonNodeDict)dict["user"]!;
		T(ReferenceEquals(user, again));
		T((str)again["name"]! == "b");
		return NIL;
	}

	// 寫索引器:只寫 Adapted(Src 不動),置 Dirty。
	public partial async Task<nil> U_Write_Indexer_Only_Adapted_Not_Src(obj? O){
		var T = Assert.IsTrue;
		var tree = (JsonObject)JsonNode.Parse("""{"old":1}""")!;
		var dict = new JsonNodeDict(tree);
		dict["new"] = "abc";
		// 讀回走 Adapted
		T((str)dict["new"]! == "abc");
		// 真相在 Adapted,樹上沒有
		T(JsonNodeDictAcc.Adapted(dict)["new"] is str s && s == "abc");
		T(!JsonNodeDictAcc.Src(dict).ContainsKey("new"));
		T(JsonNodeDictAcc.Dirty(dict));
		return NIL;
	}

	// 業務對象可直接放入、原樣讀回(序列化規則交給序列化層)。
	public partial async Task<nil> Write_Business_Object_Assignable_And_Read_Back(obj? O){
		var T = Assert.IsTrue;
		var dict = new JsonNodeDict();
		var user = new IdUser{ Id = "1ccGi79UEacII-3BAsdq6" };
		dict["Id"] = user;
		T(ReferenceEquals(dict["Id"], user));
		T(ReferenceEquals(JsonNodeDictAcc.Adapted(dict)["Id"], user));
		return NIL;
	}

	// Add:新鍵進 Adapted,置 Dirty。
	public partial async Task<nil> Add_New_Key_Goes_To_Adapted(obj? O){
		var T = Assert.IsTrue;
		var dict = new JsonNodeDict();
		dict.Add("k", "v");
		T((str)dict["k"]! == "v");
		T(JsonNodeDictAcc.Adapted(dict).ContainsKey("k"));
		T(JsonNodeDictAcc.Dirty(dict));
		return NIL;
	}

	// Add 重複鍵:拋 ArgumentException(與索引器覆蓋區分)。
	public partial async Task<nil> Add_Duplicate_Key_Throws(obj? O){
		var T = Assert.IsTrue;
		var dict = new JsonNodeDict();
		dict.Add("k", 1);
		var threw = false;
		try{
			dict.Add("k", 2);
		}catch(ArgumentException){
			threw = true;
		}
		T(threw);
		return NIL;
	}

	// Remove:雙刪樹與真相,置 Dirty。此處用容器鍵(標量讀不入真相)。
	public partial async Task<nil> U_Remove_Deletes_Both_Src_And_Adapted(obj? O){
		var T = Assert.IsTrue;
		var tree = (JsonObject)JsonNode.Parse("""{"user":{"name":"a"},"b":2}""")!;
		var dict = new JsonNodeDict(tree);
		var read = dict["user"]; // 容器,進 Adapted
		T(JsonNodeDictAcc.Adapted(dict).ContainsKey("user"));
		T(dict.Remove("user"));
		T(!JsonNodeDictAcc.Adapted(dict).ContainsKey("user"));
		T(!JsonNodeDictAcc.Src(dict).ContainsKey("user"));
		T(JsonNodeDictAcc.Dirty(dict));
		return NIL;
	}

	// Clear:雙清樹與真相。
	public partial async Task<nil> U_Clear_Clears_Both(obj? O){
		var T = Assert.IsTrue;
		var tree = (JsonObject)JsonNode.Parse("""{"a":1}""")!;
		var dict = new JsonNodeDict(tree);
		dict["b"] = 2;
		dict.Clear();
		T(JsonNodeDictAcc.Src(dict).Count == 0);
		T(JsonNodeDictAcc.Adapted(dict).Count == 0);
		T(dict.Count == 0);
		return NIL;
	}

	// Count/Keys:樹 ∪ 真相,寫的新鍵也要算。
	public partial async Task<nil> Count_Keys_Union_Of_Src_And_Adapted(obj? O){
		var T = Assert.IsTrue;
		var tree = (JsonObject)JsonNode.Parse("""{"a":1,"b":2}""")!;
		var dict = new JsonNodeDict(tree);
		dict["c"] = 3;
		T(dict.Count == 3);
		var keys = dict.Keys;
		T(keys.Contains("a") && keys.Contains("b") && keys.Contains("c"));
		return NIL;
	}

	// ContainsKey:樹與真相並集。
	public partial async Task<nil> ContainsKey_Sees_Union(obj? O){
		var T = Assert.IsTrue;
		var tree = (JsonObject)JsonNode.Parse("""{"a":1}""")!;
		var dict = new JsonNodeDict(tree);
		dict["z"] = 26;
		T(dict.ContainsKey("a"));
		T(dict.ContainsKey("z"));
		return NIL;
	}

	// TryGetValue:樹與真相並集。
	public partial async Task<nil> TryGetValue_Sees_Union(obj? O){
		var T = Assert.IsTrue;
		var tree = (JsonObject)JsonNode.Parse("""{"a":1}""")!;
		var dict = new JsonNodeDict(tree);
		dict["z"] = (i64)26;
		T(dict.TryGetValue("a", out var va) && va is i64 ia && ia == 1);
		T(dict.TryGetValue("z", out var vz) && vz is i64 iz && iz == 26);
		T(!dict.TryGetValue("missing", out _));
		return NIL;
	}
}