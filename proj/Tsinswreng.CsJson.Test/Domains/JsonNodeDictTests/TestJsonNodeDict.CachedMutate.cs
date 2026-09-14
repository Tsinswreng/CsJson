//Decl:分片二 Mutate——JsonNodeDict 的讀寫語義(寫只進 Adapted、標量不入真相、雙刪雙清)。實現見 TestJsonNodeDict.CachedMutate.Impl.cs
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsJson.Test.Domains.JsonNodeDictTests;

// 測新模型的讀寫契約;U 開頭的是用 UnsafeAccessor 驗內部狀態的用例。
public partial class TestJsonNodeDict{
	public partial void RegisterCachedMutate(ITestNode Node);

	public partial Task<nil> Read_Container_Same_Instance_On_Repeat(obj? O);
	public partial Task<nil> U_Read_Scalar_Not_Cached_In_Adapted(obj? O);
	public partial Task<nil> U_Read_Container_Cached_In_Adapted(obj? O);
	public partial Task<nil> Mutate_Taken_Container_Seen_On_ReRead(obj? O);
	public partial Task<nil> U_Write_Indexer_Only_Adapted_Not_Src(obj? O);
	public partial Task<nil> Write_Business_Object_Assignable_And_Read_Back(obj? O);
	public partial Task<nil> Add_New_Key_Goes_To_Adapted(obj? O);
	public partial Task<nil> Add_Duplicate_Key_Throws(obj? O);
	public partial Task<nil> U_Remove_Deletes_Both_Src_And_Adapted(obj? O);
	public partial Task<nil> U_Clear_Clears_Both(obj? O);
	public partial Task<nil> Count_Keys_Union_Of_Src_And_Adapted(obj? O);
	public partial Task<nil> ContainsKey_Sees_Union(obj? O);
	public partial Task<nil> TryGetValue_Sees_Union(obj? O);
}