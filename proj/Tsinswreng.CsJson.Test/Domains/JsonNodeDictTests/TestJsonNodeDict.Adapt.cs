//Decl:分片一 Adapt——JsonNodeDict.Adapt 的適配行為。實現見 TestJsonNodeDict.Adapt.Impl.cs
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsJson.Test.Domains.JsonNodeDictTests;

// 測 JsonNodeDict.Adapt 的類型分派與數字規則;U 開頭的是用 UnsafeAccessor 驗內部狀態的用例。
public partial class TestJsonNodeDict{
	public partial void RegisterAdapt(ITestNode Node);

	public partial Task<nil> Adapt_JsonObject_Returns_JsonNodeDict(obj? O);
	public partial Task<nil> Adapt_JsonArray_Returns_JsonNodeList(obj? O);
	public partial Task<nil> Adapt_JsonValue_Number_Priority_I64(obj? O);
	public partial Task<nil> Adapt_JsonValue_F64_Bool_Str_Null(obj? O);
	public partial Task<nil> Adapt_Null_Returns_Null(obj? O);
	public partial Task<nil> Adapt_Deep_Nested_Builds_View_Chain(obj? O);
	public partial Task<nil> U_Ctor_Inits_Internal_State(obj? O);
}