//Decl:分片三 JsonNodeList——IList 門面的讀寫語義(槽位真相、首次寫物化、Src 退役)。實現見 TestJsonNodeDict.JsonNodeList.Impl.cs
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsJson.Test.Domains.JsonNodeDictTests;

// 測 JsonNodeList 的契約;U 開頭的是用 UnsafeAccessor 驗內部狀態的用例。
public partial class TestJsonNodeDict{
	public partial void RegisterJsonNodeList(ITestNode Node);

	public partial Task<nil> List_Ctor_Prefills_Slots_Count_Matches(obj? O);
	public partial Task<nil> List_Read_Container_Same_Instance_On_Repeat(obj? O);
	public partial Task<nil> U_List_Read_Scalar_Not_Stored_In_Slot(obj? O);
	public partial Task<nil> U_List_Add_Appends_And_Materializes(obj? O);
	public partial Task<nil> U_List_Index_Set_Only_Adapted_Not_Src(obj? O);
	public partial Task<nil> List_Insert_Shifts_Adapted(obj? O);
	public partial Task<nil> List_RemoveAt_Shifts_Adapted(obj? O);
	public partial Task<nil> List_Remove_By_Item(obj? O);
	public partial Task<nil> U_List_Clear_Clears_Both(obj? O);
	public partial Task<nil> List_Contains_IndexOf_Cover_Adapted(obj? O);
}