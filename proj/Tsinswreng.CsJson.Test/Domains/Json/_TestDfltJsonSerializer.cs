using System.Text.Json;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsJson.Test.Domains.Json;

// 一個 tester 類對應一個 testee 類(DfltJsonSerializer/IJsonSerializer),放在獨立文件夾。
// 主檔以下劃線開頭:只負責組裝各分片的用例註冊,不在主檔寫測試。
public partial class TestDfltJsonSerializer:ITester{
	// 注入默認 options 的實例:模擬應用層 DI 註冊「掛源生成 resolver + camelCase」的全局配置,
	// 測「調用方零配置直接能用」的路徑。
	private readonly IJsonSerializer InjectSerializer;

	public TestDfltJsonSerializer(){
		InjectSerializer = new DfltJsonSerializer(new JsonSerializerOptions{
			TypeInfoResolver = JsonSampleCtx.Default,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		});
	}

	public ITestNode RegisterTestsInto(ITestNode? Node){
		Node ??= new TestNode();
		Node.Ordered = true;
		Node.IsParallelRecursive = false;
		RegisterSerialize(Node);
		RegisterDeserialize(Node);
		return Node;
	}
}