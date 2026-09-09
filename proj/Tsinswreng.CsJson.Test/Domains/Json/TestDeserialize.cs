using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsJson.Test.Domains.Json;

// 每個分片只主要測一個被測函數：此分片測 IJsonSerializer.Deserialize。
// 驗證零配置反序列化用注入默認（camelCase 讀入）、TypeInfo 走 AOT 安全路徑。
public partial class TestDfltJsonSerializer{
	public void RegisterDeserialize(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestDfltJsonSerializer),
			[typeof(IJsonSerializer)],
			[nameof(IJsonSerializer.Deserialize)],
			"Deserialize"
		);
		var R = register.Register;
		var T = Assert.IsTrue;

		R("Deserialize_ZeroConfig_RoundTrip", async(o)=>{
			var json = """{"name":"hello world","age":42}""";
			var back = InjectSerializer.Deserialize<JsonSample>(json);
			T(back is not null);
			T(back!.Name == "hello world");
			T(back.Age == 42);
			return NIL;
		});

		R("Deserialize_TypeInfo_Uses_AotSafe_Path", async(o)=>{
			var json = """{"Name":"hello world","Age":42}""";
			var ti = (JsonTypeInfo<JsonSample>)JsonSampleCtx.Default.JsonSample;
			var back = InjectSerializer.Deserialize(json, ti);
			T(back is not null);
			T(back!.Name == "hello world");
			T(back.Age == 42);
			return NIL;
		});
	}
}