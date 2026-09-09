using Tsinswreng.CsJson.Test.Domains.Json;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsJson.Test;

// 每個測試 csproj 都要有一個 TestMgr；
// 上層 TestMgr 只收編下級 TestMgr / RegisterTester,不直接註冊各 tester 的用例。
public class CsJsonTestMgr:DiEtTestMgr{
	public static CsJsonTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? Node){
		Node = this.TestNode;
		this.RegisterTester<TestDfltJsonSerializer>();
		return Node;
	}
}