using System.Text.Json.Nodes;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsJson.Test.Domains.JsonNodeDictTests;

// 主檔以下劃線開頭:只負責組裝各分片的用例註冊,不在主檔寫測試。
public partial class TestJsonNodeDict:ITester{

	public ITestNode RegisterTestsInto(ITestNode? Node){
		Node ??= new TestNode();
		Node.Ordered = true;
		Node.IsParallelRecursive = false;
		RegisterAdapt(Node);
		RegisterCachedMutate(Node);
		RegisterJsonNodeList(Node);
		return Node;
	}
}