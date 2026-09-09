using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsJson.Test.Domains.Json;

// 每個分片只主要測一個被測函數：此分片測 IJsonSerializer.Serialize。
// 重點驗證三種配置來源的行爲：注入默認（零配置直接用）、顯式 options 覆蓋、TypeInfo AOT 路徑。
public partial class TestDfltJsonSerializer{
	public void RegisterSerialize(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestDfltJsonSerializer),
			[typeof(IJsonSerializer)],
			[nameof(IJsonSerializer.Serialize)],
			"Serialize"
		);
		var R = register.Register;
		var T = Assert.IsTrue;

		R("Serialize_ZeroConfig_Uses_Injected_Defaults", async(o)=>{
			var s = new JsonSample{Name = "hello world", Age = 42};
			var json = InjectSerializer.Serialize(s);
			T(json == """{"name":"hello world","age":42}""");
			return NIL;
		});

		R("Serialize_ExplicitOptions_Overrides_Injected_Defaults", async(o)=>{
			var s = new JsonSample{Name = "hello world", Age = 42};
			// 顯式傳 PascalCase options,應覆蓋注入的 camelCase 默認。
			var json = InjectSerializer.Serialize(s, new JsonSerializerOptions{
				TypeInfoResolver = JsonSampleCtx.Default,
				PropertyNamingPolicy = null,
			});
			T(json == """{"Name":"hello world","Age":42}""");
			return NIL;
		});

		R("Serialize_TypeInfo_Uses_AotSafe_Path", async(o)=>{
			var s = new JsonSample{Name = "hello world", Age = 42};
			var ti = (JsonTypeInfo<JsonSample>)JsonSampleCtx.Default.JsonSample;
			var json = InjectSerializer.Serialize(s, ti);
			T(json == """{"Name":"hello world","Age":42}""");
			return NIL;
		});

		R("Serialize_NoInject_Throws_Under_AotReflectionDisabled", async(o)=>{
			// 不注入 options 的實例：AOT 反射禁用下必須拋異常，不能靜默走反射路徑。
			var noInject = new DfltJsonSerializer();
			var s = new JsonSample{Name = "hello world", Age = 42};
			try{
				noInject.Serialize(s);
				T(false);
			}catch(InvalidOperationException){
				T(true);
			}
			return NIL;
		});
	}
}