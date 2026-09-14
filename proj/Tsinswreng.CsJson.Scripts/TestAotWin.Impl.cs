using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;

namespace Tsinswreng.CsJson.Scripts;

/// TestAotWin 命令的流程實現；NativeAOT 開關由倉庫的 Directory.Build.props 統一提供。
internal static partial class TestAotWin{
	internal static async partial Task Main(ISCtx Ctx, CT Ct){
		var TestProject = Ctx.RootDir/"proj/Tsinswreng.CsJson.Test/Tsinswreng.CsJson.Test.csproj";

		// step 1: 以 Release 配置發布 win-x64 NativeAOT 測試 executable。
		// -m:1 --disable-build-servers:避免沙盒下 ILLink 任務宿主(MSB4216)連不上 MSBuild 節點。
		await Exe("dotnet", ["publish", TestProject, "-c", "Release", "-r", "win-x64", "-m:1", "--disable-build-servers", "-p:RestoreSources=\"\""], Ct);

		// step 2: 運行 publish 目錄中的原生測試程式；目錄結構固定為 bin/Release/{Tfm}/{Rid}/publish。
		var PublishExe = Ctx.RootDir/"proj/Tsinswreng.CsJson.Test/bin/Release"/Ctx.Tfm/"win-x64/publish/Tsinswreng.CsJson.Test.exe";
		await Exe(PublishExe, [], Ct);
	}
}