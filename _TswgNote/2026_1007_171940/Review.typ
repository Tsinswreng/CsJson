-T[2026_1007_171956][
	看skill和記憶。
	看 common-code-review, csharp
	`E:\_code\Ngan\Tsinswreng.CsJson`
	對這個項目做代碼審查
][
````md
# Tsinswreng.CsJson 代碼審查報告

審查對象：`E:\_code\Ngan\Tsinswreng.CsJson`，工作樹狀態 = HEAD `bbdb95b`（僅 `_TswgNote/` 未跟蹤）。
讀過的範圍：庫（12 個 .cs）、測試（13 個 .cs）、Scripts（5 個）、`Directory.Build.props` / 3 個 csproj / TypeAlias.cs / .gitignore；另看了消費者 `Ngan.Dict`（`AppJsonS.cs`、`Tools/Json/JsonSerializer.cs`、`DiCore.cs`）與記憶 `CsJsonRedesign.typ`、`CsRefl/ReviewBatch.typ`。
依據規範：`tsinswreng-common-code-review`、`tsinswreng-csharp`、`tsinswreng-separate-decl-and-impl`、`tsinswreng-csharp-doc-attr`、`tsinswreng-write-comment`、`tsinswreng-cstreetest`、`tsinswreng-markdown`。
**未做的驗證**：沒有執行 `dotnet build` / `Test` / `TestAotWin`——審查屬純讀取任務，規範禁止我執行會產生項目輸出的建置命令。因此凡標「待覈實」的，需要一次 build 才能定論。

---

## 一、功能正確性

1. **`JsonNodeDict.Dirty` 是只寫不讀的死狀態（已確認）。** 寫入在 `JsonNodeDict.Impl.cs:23/33/53/108` 共 4 處，庫內**零處讀取**（只有測試用 UnsafeAccessor 斷言它）。對比 `JsonNodeList.Dirty` 有兩處真讀（`JsonNodeList.Impl.cs:90/113`）。這說明 Dict 側「雙軌模型」只做了一半。

2. **類 Doc 承諾的能力沒有對應 API。** `JsonNodeDict.cs:23`、`JsonNodeList.cs:23` 都寫「序列化:本類不自帶序列化器;要 JsonNode 時,未 `Dirty` 可 O(1) 交 `Src`」，但兩類都**沒有任何返回 `JsonNode`/`JsonObject` 的公開成員**，`Src` 是 `internal readonly`，`Adapt` 是 `internal static`。這句許諾目前不可達；要麼補 `ToJsonNode()`（未 Dirty 直接交 `Src`、Dirty 則重建），要麼把註釋改成事實。

3. **`JsonNodeCtx` 的 resolver 註冊疑不足以支撐它自己的 Doc（高置信、待覈實）。** `JsonNodeCtx.cs:22-24` 只註冊 `JsonNode`/`JsonNodeDict`/`JsonNodeList`，而它 `JsonNodeCtx.cs:19-20` 聲稱「`JsonNodeDict` 產物的再序列化…在 AOT 下也有 TypeInfo 可用」。但 DictJson 的值是 `object?`，序列化時按**運行時類型**查 metadata——你自己的測試 `JsonSample.cs:21-28` 的 `DictJsonCtx` 就**顯式補了 `i64`/`f64`/`str`/`bool`** 並在註釋裏寫明理由（「枚舉 `IDictionary<string,object?>` 的值時按運行時類型查 TypeInfo」）。`JsonNodeCtx` 沒有這四個 → 用「只有 `JsonNodeCtx` 的 options」序列化一個含數字的 `JsonNodeDict`，預期拋 `InvalidOperationException: Metadata for type 'System.Int64' was not provided by TypeInfoResolver`。而且**沒有任何測試走這條路**（round-trip 測試用的是測試域自造的 `DictJsonCtx`，不是庫自己的 `JsonNodeCtx`）。建議：把四個標量類型（以及 `IDictionary<str,obj?>`/`IList<obj?>`）補進 `JsonNodeCtx`，並補一條「只用 `JsonNodeCtx` 序列化 DictJson」的用例。

4. **`Keys`/`Values` 返回的「可變快照」破壞 `IDictionary` 視圖語義（已確認）。** `JsonNodeDict.cs:46-74` 每次 new 一個 `List<str>`/`List<obj?>` 返回：其 `IsReadOnly == false`，但 `dict.Keys.Add("x")` 會**成功且不生效**（`dict.Keys.Remove("a")` 同理）；而官方 `Dictionary<K,V>.Keys` 是活視圖、寫入拋 `NotSupportedException`。另外 `foreach(var k in dict.Keys){ dict.Remove(k); }` 在這裏不會拋（官方會拋 `InvalidOperationException`）。這屬於 LSP 味道的契約含糊：拿到 `ICollection` 的人無從判斷它是不是活的。建議返回只讀包裝（`IsReadOnly == true`）並在 Doc 明說「快照」。

5. **`DeserializeToDictList` 的輸入契約粗糙。** `IJsonSerializerExtn.Impl.cs:9-11`：「空串」與合法 JSON 字面量 `"null"` 都返回 `null`（不可區分）；純空白 `" "` 走解析路徑拋 `JsonException`；`options` 傳入但沒掛 resolver 時（反射禁用環境）會拋 `InvalidOperationException`，Doc 沒提醒。建議：Doc 寫明三分支，或讓「輸入非法」與「解析出 null」用不同信號。

6. **數字規則的邊界會悄悄改型別。** `JsonNodeDict.Impl.cs:121-135` 的 i64→f64→str→bool→`ToJsonString()` 兜底：`> i64.Max` 的整數走 f64 丟精度（記憶裏已知的 2^53 問題）；超 f64 範圍（如 `1e400`）四個 `TryGetValue` 全失敗 → 落到 `value.ToJsonString()`，**結果是一個字符串 `"1e400"`**，而不是數字。這與「值拿出即用、數字不用猜 long/double」的目標相衝突，至少應在 Doc 舉例點明。

7. **索引器越界異常型別不符 `IList<T>` 慣例。** `JsonNodeList.Impl.cs:83-85/104-106` 拋 `IndexOutOfRangeException`；`List<T>` 與 `IList<T>` 慣例是 `ArgumentOutOfRangeException`。包 `IJsonNodeList` 的調用方按介面契約寫 `catch(ArgumentOutOfRangeException)` 會漏接。

8. **`JsonNodeList` 用 `null` 當「未適配」哨兵是脆弱設計。** `JsonNodeList.Impl.cs:86-89/117`：槽位 `null` 同時表示「未適配」與「合法 JSON null」。我把 `[null,1]`、`Remove(null)`、`SetAt(i,null)`、`Add(null)`、寫後 `RemoveAt` 等序列都推演過，**當前沒有找出必然錯誤**（因為 `Dirty` 置位後 `GetCached` 直接返回槽值，恰好正確），但兩義哨兵靠的是「`Dirty` 一定會在讀之前置位」這條隱式不變式；只要有人加一個「純讀取槽位」或「延遲物化」的改動就會踩。建議改成顯式 `bool[] Materialized` 或哨兵對象。

9. **測試有兩處恆真斷言，測不出東西（已確認）。** `TestJsonNodeDict.JsonNodeList.Impl.cs:119` 與 `:143` 的 `T(!list.Contains(2))`：字面量 `2` 是 `i32`，槽裏是 boxed 的 `i64`，`EqualityComparer<obj?>.Default` 永不相等 → 這兩條在「刪除前」也會通過（`(i64)2` 才對）。`List_Remove_By_Item` 與 `List_Contains_IndexOf_Cover_Adapted` 因此各自少了一大半效力。

10. **相等語義未寫進契約。** `JsonNodeDict.Contains`/`Remove(KeyValuePair)`（`JsonNodeDict.Impl.cs:56-61/76-81`）與 `JsonNodeList.IndexOf`（`:53-60`）用默認相等：標量按值、容器按引用、數字按**類型**（`i64 2 ≠ i32 2`）。`IJsonNodeDict`/`IJsonNodeList` 的 Doc 完全沒提，而這正是調用方最容易踩的一條。

---

## 二、覈對需求

記憶 `CsJsonRedesign.typ` 的需求草案（js 便利 7 條：一行 parse/stringify 免註冊、鏈式訪問缺鍵不炸、寫入自動建中間節點、創建嵌套不囉嗦、值拿出即用少 cast、`delete`/`in`/`typeof` 對應、合併/深拷貝/深相等；外加 dict⇄強型別實體互轉、DictJson⇄JsonNode 直接互轉）與現狀對照：

- 已落地：單包 `Tsinswreng.CsJson` 收納 Dict+Json（**與記憶拍板一致**）；`IJsonSerializer` 抽介面 + `JsonSerializer` 實現（**同名接口分裂已合流**：`Ngan.Dict.Core/Tools/Json/JsonSerializer.cs:7` 明寫「舊 `IJsonSerializer` 已退役」，`AppJsonS.cs:11`、`DiCore.cs:20` 已改用 `Tsinswreng.CsJson.IJsonSerializer`）；DictJson 讀寫門面（`JsonNodeDict`/`JsonNodeList`）；`string → DictJson`。
- **未落地**：js 便利層全部（路徑讀寫、缺鍵不炸訪問器、自動建中間節點、深拷貝/深相等/深合併）；**DictJson ⇄ `JsonNode` 的公開互轉**（`Adapt` 是 internal，且沒有任何「DictJson → JsonNode」方向，記憶裏「現走字符串/JsonElement 兩次中轉」的問題原封不動）；dict⇄實體互轉（`Srefl.DictSerializer.Deserialize` 仍是 `NotImplementedException`）。
- **重複代碼仍在**：記憶寫「CsTools 相關搬走刪除」，但 `Tsinswreng.CsTools` 仍有 `ToolDict.cs`、`ToolJson.cs`、`JsonNode.cs`、`ToolMultiDict.cs`；`Ngan.Dict` 的 `IDictJsonSerializer.ToDictJson` 還在走手寫的 `ToolJson.ParseJsonElement`。即兩套 Dict/Json 實現並存。

---

## 三、工程設計

1. **`JsonSerializer` 這個類名與官方 `System.Text.Json.JsonSerializer` 同名，是已經埋到消費者腳下的雷。** 證據就在倉庫裏：`Ngan.Dict.Core/Tools/Json/JsonSerializer.cs:26/31` 的 `JsonSerializer.SerializeToElement(...)` 指的是**官方那個**（該檔只 `using System.Text.Json;`）；只要有人往這個檔加一行 `using Tsinswreng.CsJson;`，這兩行立刻變成 CS0104 歧義或語義漂移。而測試檔名/註釋一直是 `DfltJsonSerializer`（`_TestDfltJsonSerializer.cs:6-7`「tester 類對應一個 testee 類(DfltJsonSerializer/IJsonSerializer)」、`CsJsonTestMgr.cs:13` `RegisterTester<TestDfltJsonSerializer>()`）——說明原本就打算叫 `DfltJsonSerializer`。**建議改名。**

2. **DictJson↔JsonNode 的適配邏輯寄生在 `JsonNodeDict.Adapt`，且 `JsonNodeList` 反向依賴 `JsonNodeDict`。** `JsonNodeDict.Impl.cs:111-137` 明明同時負責 `JsonArray` 分派，卻掛在 Dict 類上。建議抽成獨立 `JsonNodeAdaptr`（或 `ToolJsonNode`，符合 `ToolXxx`/`MkrXxx` 命名），兩類都只依賴它——這同時是把它公開（補上「DictJson⇄JsonNode 互轉」需求）的天然位置。

3. **同一個包的兩半對「寫操作是否碰來源樹」給出相反答案，是本包最需要定案的設計點。**
   - Dict：索引器/`Add` 只寫 `Adapted` 不碰樹，但 `Remove`/`Clear` **雙刪雙清來源樹**（`JsonNodeDict.Impl.cs:29-36/50-54`）。
   - List：索引 set/`Add`/`Insert`/`RemoveAt`/`Remove` 只動槽位不碰樹，只有 `Clear` 雙清（`JsonNodeList.Impl.cs:20-23/62-79/102-108`）。
   - 根因是 Dict 側**沒有 tombstone 的表達能力**（鍵的「刪除」無處記錄，只能落回樹）。後果是：調用方若同時持有來源 `JsonObject`（或序列化來源樹），Dict 的刪除會「神奇地」作用在樹上，而 List 的刪除不會。
   - 兩條出路：(a) 統一「寫不碰來源樹」→ Dict 加一個 tombstone 集合；(b) 統一「寫就落樹」→ 去掉 `Adapted`/`Dirty` 雙軌（回到 Thin wrapper）。現狀的混合態是最難維護的。

4. **`JsonSerializer.Inst` 的形態比倉庫既有慣例倒退。** 它是 `public static JsonSerializer Inst{get;set;} = new(...)`（`JsonSerializer.cs:29`）——**可寫**的進程內全局單例；而本倉庫既有寫法是只讀惰性（`Ngan.Dict` 的 `AppJsonS.cs:11` `=> field ??= new(...)`、`AppJsonSerializer.Inst => field ??= new()`）。它與「方便 DI 注入、調用方零配置」的目標也相衝；倉庫內除 CsJson 自己的測試外**沒有任何消費者**，業務走 `AppJsonS`/`DiCore`。建議改成只讀，或在 DI 場景下直接刪。

5. **`IJsonSerializer` 的「不增不減」是不準確的自我描述。** `IJsonSerializer.cs:12-16` 聲稱「與官方公開 API 面逐一對應…方法名、重載、參數形狀照抄官方，不增不減」，實際：**增了 `Options` 成員**，且把「options 為 null」的語義從官方 `JsonSerializerOptions.Default` 改成注入的 `Options`（`JsonSerializer.cs:49` 等 60 餘處 `options ?? Options`）。這個 delta 恰恰是本包最有價值的部分（DI 零配置），應該在 Doc 主說明裏顯式列出，而不是埋在「不增不減」底下。另外手抄 ~120 個重載的維護成本（每個 .NET 版本都要對帳）也該在設計文檔裏認領。

6. **`IJsonSerializerExtn` 只有 `string` 一個入口，且命名與業務用語不一致。** `DeserializeToDictList`（DictList）vs 記憶/`Ngan.Dict` 統一的「DictJson」；同時缺 `JsonNode`/`Utf8JsonReader`/`Stream` 入口——而「DictJson⇄JsonNode 免中轉」正是記憶裏點名的痛點。

7. **缺文檔面。** 倉庫沒有 `.Doc` 程序集，也沒有 README.typ（`CsRefl` 用倉庫根 `README.typ` 當文檔面）。這個包已經有外部消費者了，「三種內存形態（DictJson / `JsonNode` / `JsonElement`）各自何時用、AOT 下要註冊什麼」是必須寫下來的東西。

8. **`IJsonNodeSerializer` 是死碼。** `IJsonNodeSerializer.cs:7` 零實現、零引用（全倉庫 grep 只命中宣告處），且留著註釋掉的成員 `//public object? Deserialize(...){}`（`:11`）、無任何註釋。建議刪除。

9. **線程安全契約缺失。** `JsonNodeDict`/`JsonNodeList` 都不是線程安全的，且「讀會改 `Adapted`（緩存容器視圖）」——連「只讀」操作都會寫內部狀態。前端 VM / 後端 Svc 共享一份解析結果的場景必須知道這點。

10. **可測試性被 internal 逼到 UnsafeAccessor。** `UnsafeAccessors.cs` 用 `UnsafeAccessor` 摸 3 個字段 + 1 個靜態方法；`MkTestFnRegister(..., ["Adapt"], ...)` 因為 `Adapt` 是 internal 而**只能用硬編碼字符串**（`nameof` 不可達）。若 `Adapt` 按第 2 點公開，測試就能回到 `nameof`。

---

## 四、代碼規範

1. **`[Doc]` vs `///`：硬違規。** 合格：`JsonNodeDict.cs`、`JsonNodeList.cs`、`IJsonNodeDict.cs`、`IJsonNodeList.cs`、`JsonNodeCtx.cs`、`IJsonSerializerExtn.cs`。不合格：**`IJsonSerializer.cs`（251 行、整個 API 面的說明全用 `///`）**、`JsonSerializer.cs`、`IJsonNodeSerializer.cs`（完全無註釋）、Scripts 三個檔（`Test.cs`/`TestAotWin.cs`/`Program.cs` 的 `///`）。按 `tsinswreng-csharp-doc-attr`「使用 `[Doc]` 這個 Attr 為類型/成員 添加註釋，不使用常規的斜槓註釋」，這是形式上的硬不合規。

2. **`[Doc]` 內容質量不合格（按你自己定的判據）。** `CsRefl/ReviewBatch.typ` 記著判據：必須有示例、例子＝調用＋實參＋緊跟一行結果、凡符號名一律 `nameof` 插值（含圍欄內）、不准寫「實測」。以這個判據看新檔：`JsonNodeCtx`、`IJsonSerializerExtn`、`IJsonSerializer` 的說明都是散文，**無一處 `#Eg`**，提到 `JsonTypeInfo`/`JsonSerializerOptions`/`JsonSerializerContext`/`JsonNode` 時全是純文字（f12 跳不過去）。`JsonNodeDict.cs` 是你自己指定的樣板——新檔應向它對齊，而不是向它相反的方向走。

3. **Decl 缺口（與 CsRefl 那條硬違規同一條）。** `JsonNodeList.Impl.cs:112` 的 `private void EnsureMaterialized()`：**Decl 完全沒聲明**，且兩側都沒標 `partial`。`JsonNodeDict` 側全數合規，所以這是漏的單點。檢查手法可直接沿用：掃 Impl 裏「簽名以 `)` 結尾且有 `partial`」的行/所有函數，回 Decl 找對應宣告。

4. **普通方法用 `=>`。** `JsonNodeDict.cs:104-105`、`JsonNodeList.cs:58-59` 的顯式介面 `GetEnumerator()` 用表示式主體寫；skill 規定除 getter/setter/lambda 外的普通方法**禁止** `=>`。附帶：這兩個是「實現」，卻寫在 Decl（`(*.cs)`）而非 Impl。

5. **硬編碼字符串 / 兩套寫法並存。**
   - 測試用例名：`Domains/Json/` 三個分片用裸字符串 `R("Serialize_ZeroConfig_Uses_Injected_Defaults", ...)`；同一專案的 `Domains/JsonNodeDictTests/` 用 `R(nameof(Add_New_Key_Goes_To_Adapted), ...)`。同一包兩種風格。
   - `JsonAotMsg.Unreferenced/DynamicCode`（`JsonSerializer.cs:482-484`）抄了官方警告文案——可接受（目的是讓 IL 警告文案一致），但屬「與框架文案耦合」，建議註明來源與對應 .NET 版本，官方改詞時要同步。

6. **測試域的 Decl/Impl 分離不統一。** `JsonNodeDictTests` 照 `_TestXxx.cs` + `TestXxx.Impl.cs` 分離 ✓（合規）；但 `Domains/Json/` 的三個分片把 `RegisterXxx` 直接寫在分片檔、**用例用匿名 lambda 內聯**（`TestSerialize.cs:20` 起），與 `tsinswreng-cstreetest`「每個測試用例函數用常態函數聲明和實現、不用匿名函數」以及「用 `nameof` 指明被測函數+用例名」的要求不符（被測函數名倒是用了 `nameof(IJsonSerializer.Serialize)` ✓）。

7. **命名易二義。** `JsonNodeDict`/`JsonNodeList` 其實不是「一種 `JsonNode`」，而是「`JsonNode` 樹上的 DictJson / IList 門面」；`DeserializeToDictList` 的 DictList 也與 DictJson 並存。

8. **`TypeAlias.cs:1-5` 的 `#if false` 裏內嵌 XML**（`<ItemGroup>`）——模板產物，非本專案引入，列出供你判斷是否要在模板層修掉。

9. `.gitignore` 忽略 `*.sln`，所以倉庫沒有 sln，但同時保留了 `UpdSln.sh`——兩者看起來矛盾，確認是否刻意。

---

## 五、性能

1. **`JsonNodeDict.Count`/`Keys`/枚舉是反復的全量 O(n) + 多份分配。** `Count`（`JsonNodeDict.cs:76-88`）每次 new `HashSet` 並遍歷 `Src`+`Adapted`；`Keys`（`:46-63`）再一次 `List`+`HashSet`；`GetEnumerator`（`Impl:83-88`）又調一次 `Keys`；`CopyTo`（`Impl:63-74`）先 `Count`（再一次 HashSet）後枚舉（再一次 Keys）。對「讀多寫少」的門面來說這是主要開銷來源。若 `Adapted` 只增不減（現狀正是：容器緩存會不斷寫入），可以維護一份增量鍵集合，把 `Count`/`Keys` 降到 O(1) 視圖。

2. **「標量不入真相（重適配零成本）」這條理由不成立。** `JsonNodeDict.cs:16-17`、`Impl:100-103` 的依據是「重適配零成本」，但 `Adapt` 對標量要做 `JsonValue.TryGetValue<i64>`（JsonElement-backed 值要**解析原文**）再**裝箱成 `obj?`**。所以反復讀同一標量鍵 = 反復解析 + 反復裝箱，成本不是零；`JsonNodeList` 每個標量槽在 `Contains`/`IndexOf`/枚舉時同樣每次重解析+重裝箱。要麼把理由改成「省內存（代價是重解析）」，要麼老實緩存標量。

3. **`EnsureMaterialized` 讓「追加一個元素」退化成全量。** `JsonNodeList.Impl.cs:112-122`：首次寫會把**所有**來源元素全部適配（含解析所有標量、裝箱、為每個容器建視圖）。對一個大數組 `Add` 一個元素 = O(n) 全量解析。Doc 有寫「首次寫物化」但沒點明代價，建議用示例寫清。

4. **整條路徑無流式。** `DeserializeToDictList(str)` → 整份字串進內存 → 整棵 `JsonNode` → 視圖；沒有 `Stream`/`Utf8JsonReader`/`IAsyncEnumerable` 入口（官方有 `DeserializeAsyncEnumerable`）。按 skill 的流式懶加載要求，string 版應是**擴展**而不是唯一入口。

5. `JsonNodeList.Impl.cs:16-17` 用 `Enumerable.Repeat<obj?>(null, arr.Count)` + `AddRange` 預填，可直接 `new List<obj?>(arr.Count)` 後循環 Add（小）。

---

## 六、安全性

1. **默認單例的 `Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping`（`JsonSerializer.cs:31`）是注入面。** 它讓 `<`/`>`/`&` 原樣輸出，產物若嵌進 HTML/`<script>` 就是 XSS 通道。作為**庫的默認單例**選這個值偏危險（`Ngan.Dict` 是為了中文不轉義，那應該是業務側配置的事）。
2. **無大小/可取消性說明。** `DeserializeToDictList` 接任意長 `str`、同步解析、無 `CancellationToken`（長輸入無法中斷）；`MaxDepth` 走默認 64（算是有 DoS 防護）但沒寫進 Doc。
3. `public static JsonSerializer Inst{get;set;}` 可被進程內任意代碼整體替換（誤配置/供應鏈面）。若改成只讀可消除。
4. 沒有發現反射回退、類型混淆、未註冊類型靜默降級等問題——`TypeInfoResolver` 明確、未註冊類型直接拋，AOT 姿勢是加分的（見第八節的警告泄漏除外）。

---

## 七、值得保留的部分

- `JsonNodeDict.cs`/`JsonNodeList.cs` 的兩份檔案頭 + 成員契約說明（本倉庫註釋樣板）與 `Decl`/`Impl` 分離本身是清楚的，「讀容器返回同一實例、寫只寫真相、來源樹可 O(1) 交出」的雙軌模型方向成立——只是 Dict 側沒做完（第一節第 1、2 點）。
- AOT 姿勢：源生成 context + `RequiresUnreferencedCode`/`RequiresDynamicCode` 照官方標註 + `TestAotWin` 腳本 + 反射禁用的 `Serialize_NoInject_Throws_Under_AotReflectionDisabled` 用例（它成立是因為 `Directory.Build.props` 的 `PublishAot=true` 讓 runtimeconfig 帶 `System.Text.Json.JsonSerializer.IsReflectionEnabledByDefault=false`）。
- Scripts 用 `CallerFilePath` 定位倉庫根、不依賴 cwd（`Program.cs:64-66`），`TestAotWin` 註明 `-m:1 --disable-build-servers` 是為了沙盒下的 MSB4216——這些坑寫進代碼註釋了，很好。

---

## 八、待覈實（一次 build / publish 即可定論）

1. **AOT 警告泄漏（預期 IL2026 + IL3050）。** `IJsonSerializerExtn.Impl.cs:12` 調用帶 `[RequiresUnreferencedCode]`/`[RequiresDynamicCode]` 的 `Deserialize<JsonNode>(str, options)` 重載，而 `DeserializeToDictList` **自身沒標註**（`IJsonSerializerExtn.cs:19`），所以警告不會傳播給調用方——這正是需要修的地方；測試側 `TestSerialize.cs:30/51`、`TestDeserializeToDictList.cs:21` 等直接調用註解重載的地方同樣預期有警告。修法二選一：(a) 給 extn 標同款兩條註解；(b) 改走 `JsonTypeInfo`/`JsonSerializerContext` 重載（但那樣就吃不到調用方 `options` 的 `ReadCommentHandling`/`Encoder` 設定，需要你拍板取捨）。
2. `JsonNodeCtx` 單獨序列化含數字的 `JsonNodeDict` 是否真的拋「Metadata for type `System.Int64` was not provided」。
3. 用例總數與結果（Debug `dotnet run` 與 `TestAotWin` 兩套是否都全綠）。
4. 構建是否有 CS1998（測試用例 `async` 無 `await`，幾乎每個用例都是）等警告——`tsinswreng-cstreetest` 的樣例本身也是 `async` 無 `await`，若是倉庫既有慣例則可忽略。

---

## 九、我的問題（需要你拍板／確認）

1. **`Tsinswreng.CsJson/_TswgNote/2026_1007_171940/Review.typ`**（建立時間 17:19:40，正好在 HEAD `bbdb95b` 提交後 6 秒；git 未跟蹤）。按核心規則我不得讀寫你的筆記（`_Note/`、`_Tswg/` 一類），所以**沒有打開**。若那是本輪的批註／要求，請授權我讀取並在內回覆，我再據此補做或調整。
2. 記憶 `CsJsonRedesign.typ` 裏的需求清單（js 便利 7 條、DictJson⇄JsonNode 直接互轉、dict⇄實體互轉）現在還算數嗎？還是本輪範圍已縮到「抽序列化介面 + DictJson 讀寫門面」？這決定第二節的缺口算不算問題。
3. 第三節第 3 點的 Dict/List 寫入語義（`Remove`/`Clear` 是否碰來源樹）要往哪邊統一？
4. `JsonSerializer` 改名為 `DfltJsonSerializer` 可行嗎（測試檔名與註釋本來就這麼寫，且能消除與官方同名的歧義）？
5. 要不要我實際跑 `dotnet build` / `Test` / `TestAotWin`，把第八節四條覈實掉？（我沒跑是因為審查屬純讀取，規範禁止執行會產生項目輸出的建置命令。）
6. 要不要我更新記憶 `.Tsinswreng/AiMemory/CsJson/CsJsonRedesign.typ`？它目前仍寫「`Tsinswreng.CsJson`（net10.0 空殼，只有 3 個文件）…尚未進入設計與實作」，與現狀嚴重不符，會誤導後續會話。

本次為純讀取審查，未修改或新建任何文件；以上也只提出問題與判斷，未動代碼。
````
]


-T[2026_1007_173515][
	你寫得亂刺八糟的
	能不能好好看common-code-review的輸出要求?

][
````md
# Tsinswreng.CsJson 代碼審查報告

審查對象：`E:\_code\Ngan\Tsinswreng.CsJson`（HEAD `bbdb95b`，庫 12 檔 / 測試 13 檔 / Scripts 5 檔）。
依據：`tsinswreng-common-code-review`（按維度分節、逐項給位置與示例）、`tsinswreng-csharp`、`tsinswreng-separate-decl-and-impl`、`tsinswreng-csharp-doc-attr`、`tsinswreng-cstreetest`。
本輪未執行 `dotnet build` / `Test` / `TestAotWin`（審查屬純讀取，規範禁止執行產生項目輸出的建置命令）；凡標【待覈實】者需一次構建定論。

---

## 功能正確性

**1. `JsonNodeDict.Dirty` 只寫不讀，雙軌模型只做了一半**
位置：`proj/Tsinswreng.CsJson/JsonNodeDict.cs:33`（宣告）、`JsonNodeDict.Impl.cs:23/33/53/108`（四處寫）
```cs
// JsonNodeDict.Impl.cs —— 只有寫，沒有任何讀取
Adapted[Key] = Value;
Dirty = true;
```
庫內 `Dirty` 零處讀取（對比 `JsonNodeList.Impl.cs:90/113` 有真讀）。後果見第 2 條。

**2. 類 Doc 承諾的能力沒有對應 API**
位置：`JsonNodeDict.cs:23`、`JsonNodeList.cs:23`
```cs
序列化:本類不自帶序列化器;要 JsonNode 時,未 {nameof(Dirty)} 可 O(1) 交 {nameof(Src)}。
```
兩類都沒有任何返回 `JsonNode`/`JsonObject` 的公開成員，`Src` 是 `internal readonly`，`Adapt` 是 `internal static`。這條許諾目前不可達。

**3. `JsonNodeCtx` 的 resolver 註冊疑不足以支撐它自己的 Doc【待覈實】**
位置：`JsonNodeCtx.cs:22-24` 對比測試域的 `Test/Domains/Json/JsonSample.cs:21-28`
```cs
// JsonNodeCtx.cs：只有三個
[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(JsonNodeDict))]
[JsonSerializable(typeof(JsonNodeList))]

// JsonSample.cs：自己補了標量，並寫明理由
// 枚舉 IDictionary<string,object?> 的值時按運行時類型查 TypeInfo
[JsonSerializable(typeof(i64))]
[JsonSerializable(typeof(f64))]
[JsonSerializable(typeof(str))]
[JsonSerializable(typeof(bool))]
```
`JsonNodeDict` 的值是 `object?`，序列化時按運行時類型查 metadata；用「只有 `JsonNodeCtx` 的 options」序列化含數字的 DictJson，預期拋 `InvalidOperationException: Metadata for type 'System.Int64' was not provided by TypeInfoResolver`。且無測試覆蓋（round-trip 用例用的是測試域自造的 `DictJsonCtx`）。

**4. `Keys`/`Values` 返回可變快照，破壞 `IDictionary` 視圖語義**
位置：`JsonNodeDict.cs:46-63`、`:65-74`
```cs
var ans = new List<str>(Count);   // IsReadOnly == false
...
return ans;
```
`dict.Keys.Add("x")` / `dict.Keys.Remove("a")` 會成功且不生效；官方 `Dictionary<K,V>.Keys` 是活視圖、寫入拋 `NotSupportedException`。

**5. `DeserializeToDictList` 的輸入契約粗糙**
位置：`IJsonSerializerExtn.Impl.cs:9-11`
```cs
if(str.IsNullOrEmpty(Json)){
    return null;
}
```
`""` 與合法 JSON 字面量 `"null"` 同結果 `null`（不可區分）；`" "` 走解析路徑拋 `JsonException`；`options` 未掛 resolver 時（反射禁用）拋 `InvalidOperationException`，Doc 未提醒。

**6. 數字規則的邊界會悄悄改型別**
位置：`JsonNodeDict.Impl.cs:121-135`
```cs
if(value.TryGetValue<i64>(out var i64v)){ return i64v; }
if(value.TryGetValue<f64>(out var f64v)){ return f64v; }
...
return value.ToJsonString(); // 兜底:不丟信息
```
`{"n":1e400}` 四個 `TryGetValue` 全失敗 → 落到 `ToJsonString()`，讀出來是**字符串** `"1e400"`；`> i64.Max` 的整數走 f64 丟精度（記憶已知的 2^53 問題）。

**7. 索引器越界異常型別不符 `IList<T>` 慣例**
位置：`JsonNodeList.Impl.cs:83-85`、`:104-106`
```cs
throw new IndexOutOfRangeException($"Index {Index} out of range 0..{Adapted.Count - 1}");
```
`List<T>` / `IList<T>` 慣例是 `ArgumentOutOfRangeException`；按介面契約 catch 的調用方會漏接。

**8. `JsonNodeList` 用 `null` 當「未適配」哨兵，與合法 JSON null 同形**
位置：`JsonNodeList.Impl.cs:86-89`、`:117`
```cs
var v = Adapted[Index];
if(v is not null){ return v; }
if(Index < Src.Count && !Dirty){ ... }
```
當前推演（`[null,1]`、`Remove(null)`、`SetAt(i,null)`、寫後 `RemoveAt`）**沒有必然錯誤**，但它依賴「`Dirty` 一定在讀之前置位」這條隱式不變式；加一個「純讀槽位」或延遲物化的改動就會踩。建議 `bool[] Materialized` 或哨兵對象。

**9. 測試有兩處恆真斷言**
位置：`Test/Domains/JsonNodeDictTests/TestJsonNodeDict.JsonNodeList.Impl.cs:119`、`:143`
```cs
T(!list.Contains(2));   // 2 是 i32；槽裏是 boxed i64，EqualityComparer.Default 永不相等
```
這兩條在「刪除前」也會通過（應為 `(i64)2`），`List_Remove_By_Item` 與 `List_Contains_IndexOf_Cover_Adapted` 因此少了大半效力。

**10. 相等語義未寫進契約**
位置：`JsonNodeDict.Impl.cs:56-61`、`:76-81`、`JsonNodeList.Impl.cs:53-60`
```cs
return EqualityComparer<obj?>.Default.Equals(value, Item.Value);
```
標量按值、容器按引用、數字按**類型**（`i64 2 ≠ i32 2`）；`IJsonNodeDict`/`IJsonNodeList` 的 Doc 完全沒提，而這是調用方最易踩的一條。

---

## 覈對需求

**1. js 便利層（記憶需求 7 條）全部未落地**
位置：`IJsonNodeDict.cs:14`（公開面只有 `IDictionary<str, obj?>`）、`IJsonNodeList.cs:14`
```cs
public interface IJsonNodeDict : IDictionary<str, obj?>{ }
```
缺：路徑讀寫、缺鍵不炸訪問器、寫入自動建中間節點、`delete`/`in`/`typeof` 對應、深拷貝／深相等／深合併。現狀 `dict["missing"]` 直接拋 `KeyNotFoundException`。

**2. DictJson ⇄ `JsonNode` 沒有公開互轉（記憶點名的痛點未解）**
位置：`JsonNodeDict.Impl.cs:111`
```cs
internal static partial obj? Adapt(JsonNode? Node){
```
只有 `string → DictJson`（`IJsonSerializerExtn.cs:19`），反向與 `JsonNode → DictJson` 都沒有公開入口，仍被迫走字符串/`JsonElement` 中轉。

**3. 需求草案中的 dict ⇄ 強型別實體互轉仍未實現**
依記憶 `CsJsonRedesign.typ`，`Srefl` 的 `DictSerializer.Deserialize` 是 `NotImplementedException`（本輪未讀該檔，此條**未覈實**）。

**4. 舊實現未刪，「兩套並存」問題仍在**
位置：`Tsinswreng.CsTools/proj/Tsinswreng.CsTools/`（`ToolDict.cs`、`ToolJson.cs`、`JsonNode.cs`、`ToolMultiDict.cs`）；消費處 `Ngan.Dict/Ngan.Dict.Core/proj/Ngan.Dict.Core/Tools/Json/JsonSerializer.cs:19`
```cs
return ToolJson.ParseJsonElement(jsonElement);
```
記憶寫「CsTools 相關搬走刪除」，實際未搬。

**5. 已達成項**：單包收納 Dict+Json（與記憶拍板一致）；同名接口分裂已合流（`Ngan.Dict.Core/Tools/Json/JsonSerializer.cs:7` 明寫舊 `IJsonSerializer` 退役，`AppJsonS.cs:11`、`DiCore.cs:20` 已改用 `Tsinswreng.CsJson.IJsonSerializer`）。

---

## 工程設計

**1. `JsonSerializer` 類名與官方 `System.Text.Json.JsonSerializer` 同名，雷已埋在消費者腳下**
位置：`JsonSerializer.cs:23`；受害現場 `Ngan.Dict/Ngan.Dict.Core/proj/Ngan.Dict.Core/Tools/Json/JsonSerializer.cs:26/31`
```cs
var jsonElement = JsonSerializer.SerializeToElement(dict, AppJsonCtx.Inst.IDictionaryStringObject);
```
該檔只 `using System.Text.Json;`，此處 `JsonSerializer` 指官方那個；只要有人加一行 `using Tsinswreng.CsJson;` 即 CS0104 或語義漂移。測試檔名/註釋本來就寫 `DfltJsonSerializer`（`Test/Domains/Json/_TestDfltJsonSerializer.cs:6`、`CsJsonTestMgr.cs:13`）→ 建議改名 `DfltJsonSerializer`。

**2. 適配邏輯寄生在 `JsonNodeDict`，且 `JsonNodeList` 反向依賴它**
位置：`JsonNodeDict.Impl.cs:111-137`（同時負責 `JsonArray` 分派）、`JsonNodeList.Impl.cs:92`、`:118`
```cs
var adapted = JsonNodeDict.Adapt(Src[Index]);
```
應抽獨立適配器（`JsonNodeAdaptr`/`ToolJsonNode`），兩類只依賴它；這也是把 `Adapt` 公開（補需求 2）的天然位置。

**3. Dict 與 List 對「寫操作是否碰來源樹」給出相反答案**
位置：`JsonNodeDict.Impl.cs:29-36`、`:50-54` 對比 `JsonNodeList.Impl.cs:62-79`、`:102-108`
```cs
// Dict：雙刪來源樹
removed |= Src.Remove(Key);
// List：只動槽位
Adapted.RemoveAt(Index);
```
根因是 Dict 側沒有 tombstone 的表達能力（鍵的刪除無處記錄）。現狀混合態最難維護；建議二選一：統一「寫不碰樹」（Dict 加 tombstone），或統一「寫就落樹」（去掉 `Adapted`/`Dirty` 雙軌）。

**4. `Inst` 是可寫全局單例，比倉庫既有慣例倒退**
位置：`JsonSerializer.cs:29` 對比 `Ngan.Dict.Core/Tools/Json/AppJsonS.cs:11`
```cs
public static JsonSerializer Inst{get;set;} = new(...);      // CsJson：可寫
public static Tsinswreng.CsJson.IJsonSerializer Inst => field??= new JsonSerializer(...); // 倉庫慣例：只讀
```
且倉庫內除 CsJson 自己的測試外無消費者（業務走 `AppJsonS`/`DiCore`）。

**5. 「與官方不增不減」的自我描述不實**
位置：`IJsonSerializer.cs:12-16` 對比 `JsonSerializer.cs:20`、`:49`
```cs
/// 方法名、重載、參數形狀照抄官方，不增不減。
...
return System.Text.Json.JsonSerializer.Serialize(value, options ?? Options);
```
實際增了 `Options` 成員，並把「options 為 null」從官方 `JsonSerializerOptions.Default` 改成注入的 `Options`。這正是本包最有價值的 delta，應在主說明顯式列出；另 ~120 個重載手抄的跨 .NET 版本維護成本也應認領。

**6. `IJsonSerializerExtn` 只有 `string` 一個入口**
位置：`IJsonSerializerExtn.cs:19`
```cs
public static partial obj? DeserializeToDictList(this IJsonSerializer z, str Json, JsonSerializerOptions? options = null);
```
缺 `JsonNode`/`Utf8JsonReader`/`Stream` 入口。

**7. internal 把測試逼到 `UnsafeAccessor` + 硬編碼字符串**
位置：`Test/Domains/JsonNodeDictTests/UnsafeAccessors.cs:19-20`、`TestJsonNodeDict.Adapt.Impl.cs:12`
```cs
[UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "Adapt")]
internal static extern obj? Adapt(JsonNodeDict? d, JsonNode? node);
...
[typeof(JsonNodeDict)], ["Adapt"],   // nameof 不可達，只能硬編碼
```
測試綁死了內部形狀；`Adapt` 公開後可回到 `nameof`。

**8. 線程安全契約缺失，且「讀」會寫內部狀態**
位置：`IJsonNodeDict.cs:6-13`（Doc 未提）、`JsonNodeDict.Impl.cs:99-102`
```cs
if(adapted is JsonNodeDict or JsonNodeList){
    Adapted[Key] = adapted;   // 讀操作寫入 Adapted
}
```
前端 VM / 後端 Svc 共享一份解析結果時必須知道這點。

---

## 代碼規範

**1. `[Doc]` vs `///`：硬違規**
位置：`IJsonSerializer.cs:1-251`（整檔、整個 API 面）、`JsonSerializer.cs:12-22` 等、`IJsonNodeSerializer.cs`（零註釋）、`Scripts/Test.cs:3`、`Scripts/TestAotWin.cs:2`、`Scripts/Program.cs:7-26`
```cs
/// 完整 JSON 序列化接口：與官方 System.Text.Json.JsonSerializer 的公開 API 面逐一對應，
```
`tsinswreng-csharp-doc-attr`：類型/成員註釋一律用 `[Doc]`，不用斜槓註釋。合格的是 `JsonNodeDict.cs`、`JsonNodeList.cs`、`IJsonNodeDict.cs`、`IJsonNodeList.cs`、`JsonNodeCtx.cs`、`IJsonSerializerExtn.cs`。

**2. `[Doc]` 內容不合格：無示例、無 `nameof` 插值**
位置：`JsonNodeCtx.cs:13-21`、`IJsonSerializerExtn.cs:7-16`
```cs
[Doc(@$"
{nameof(IJsonSerializer)} 的擴展:JSON 字符串 → DictJson。
...
options 與接口其他 API 一致:傳入即用,不傳則走 z 自身的 {nameof(IJsonSerializer.Options)}。
")]
```
全無 `#Eg`，示例判據（例子＝調用＋實參＋緊跟一行結果）未落。你自己指定的樣板是本倉庫的 `JsonNodeDict.cs`，新檔應向它對齊。

**3. Decl 缺口（CsRefl 同一條硬違規）**
位置：`JsonNodeList.Impl.cs:112`
```cs
private void EnsureMaterialized(){   // Decl(JsonNodeList.cs) 無宣告，兩側均未標 partial
```
`JsonNodeDict.Impl.cs` 全部合規，只此一處漏。

**4. 普通方法用 `=>`，且實現寫在 Decl**
位置：`JsonNodeDict.cs:104-105`、`JsonNodeList.cs:58-59`
```cs
System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    => GetEnumerator();
```
skill 規定除 getter/setter/lambda 外普通方法禁止 `=>`（一行也要大括號 + `return`）。

**5. 硬編碼用例名，同專案兩套寫法**
位置：`Test/Domains/Json/TestSerialize.cs:20/27/38/46`、`TestDeserializeToDictList.cs:19/34/44/53/64/80/88`
```cs
R("Serialize_ZeroConfig_Uses_Injected_Defaults", async(o)=>{ ... });
```
對比同專案的 `TestJsonNodeDict.CachedMutate.Impl.cs:20` 用的是 `R(nameof(Read_Container_Same_Instance_On_Repeat), ...)`。

**6. 測試域 Decl/Impl 分離不統一，用例用匿名 lambda**
位置：`Test/Domains/Json/TestSerialize.cs:20-25` 對比 `Test/Domains/JsonNodeDictTests/TestJsonNodeDict.CachedMutate.Impl.cs:36`
```cs
public partial async Task<nil> Read_Container_Same_Instance_On_Repeat(obj? O){   // 常態函數宣告
```
`tsinswreng-cstreetest` 要求用例函數常態宣告實現、不用匿名函數。

**7. 命名易二義**
位置：`IJsonSerializerExtn.cs:19`
```cs
DeserializeToDictList   // 業務/記憶統一叫 DictJson；且 JsonNodeDict 並非「一種 JsonNode」
```

**8. 死檔案：無註釋 + 殘留註釋掉的代碼**
位置：`IJsonNodeSerializer.cs:7-11`
```cs
public interface IJsonNodeSerializer{
	public JsonNode? SerializeToNode(...);
	//public object? Deserialize(JsonNode? node, Type returnType, JsonSerializerOptions? options = null){}
}
```
全倉庫零實現、零引用（僅宣告處命中）。

**9. 缺文檔面**
位置：倉庫根（無 `<項目>.Doc` 程序集、無 `README.typ`）。`CsRefl` 用倉庫根 `README.typ` 當文檔面；本包已有外部消費者，「三種內存形態何時用、AOT 要註冊什麼」需寫下來。

**10. `JsonAotMsg` 抄官方警告文案**
位置：`JsonSerializer.cs:482-484`
```cs
public const string Unreferenced = "JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, ...";
```
可接受（目的是 IL 警告文案一致），但與框架文案耦合，建議註明來源與對應 .NET 版本。

**11. `TypeAlias.cs:1-5` 的 `#if false` 內嵌 XML**（`<ItemGroup>`）——模板產物，非本專案引入。

---

## 性能

**1. `Count`/`Keys`/枚舉反復全量 O(n) 且多次分配**
位置：`JsonNodeDict.cs:46-63`、`:76-88`，`JsonNodeDict.Impl.cs:63-74`、`:83-88`
```cs
public int Count{
    get{
        var seen = new HashSet<str>(Src.Count + Adapted.Count);
        ...
```
`Count` 每次 new `HashSet` 並遍歷兩份存儲；`Keys` 再一次 `List`+`HashSet`；`CopyTo` 先 `Count`（HashSet）再枚舉（再一次 `Keys`）。「讀多寫少」門面下應維護增量鍵集合，把 `Count`/`Keys` 降為 O(1) 視圖。

**2. 「標量不入真相＝重適配零成本」這條理由不成立**
位置：`JsonNodeDict.cs:16-17`、`JsonNodeDict.Impl.cs:100-103`、`JsonNodeList.Impl.cs:91-96`
```cs
// 容器才緩存同一實例;標量直接返回,不入真相(重適配零成本)。
```
`Adapt` 對標量要 `JsonValue.TryGetValue<i64>`（JsonElement-backed 值要解析原文）再裝箱成 `obj?`；反復讀同一標量鍵＝反復解析＋反復裝箱。要麼把理由改成「省內存」，要麼緩存標量。

**3. `EnsureMaterialized` 讓「追加一個元素」退化成全量**
位置：`JsonNodeList.Impl.cs:112-122`
```cs
for(var i = 0; i < Src.Count && i < Adapted.Count; i++){
    if(Adapted[i] is null){
        Adapted[i] = JsonNodeDict.Adapt(Src[i]);   // 解析+裝箱全部來源元素
    }
}
```
大數組 `Add` 一個元素 = O(n) 全量解析。Doc 有寫「首次寫物化」但未點明代價。

**4. 整條路徑無流式入口**
位置：`IJsonSerializerExtn.cs:19`
```cs
... DeserializeToDictList(this IJsonSerializer z, str Json, ...)
```
`str` → 整棵 `JsonNode` → 視圖，無 `Stream`/`Utf8JsonReader`/`IAsyncEnumerable`；按 skill 流式要求，string 版應是**擴展**而非唯一入口。

**5. `JsonNodeList` 預填用 `Enumerable.Repeat`**
位置：`JsonNodeList.Impl.cs:16-17`
```cs
Adapted.AddRange(Enumerable.Repeat<obj?>(null, arr.Count));
```
可 `new List<obj?>(arr.Count)` 後循環 Add（小）。

---

## 安全性

**1. 默認單例用不安全編碼器，是注入面**
位置：`JsonSerializer.cs:31`
```cs
Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 允許原樣輸出
```
`<`/`>`/`&` 不轉義；產物若嵌進 HTML/`<script>` 即 XSS 通道。作為**庫的默認**偏危險（`Ngan.Dict` 為中文不轉義，應由業務側配置）。

**2. 無大小上限與取消通道**
位置：`IJsonSerializerExtn.cs:19`
```cs
public static partial obj? DeserializeToDictList(this IJsonSerializer z, str Json, JsonSerializerOptions? options = null);
```
接受任意長 `str`、同步解析、無 `CancellationToken`；`MaxDepth` 走默認 64（有防護）但未寫進 Doc。

**3. 可寫全局單例可被整體替換**
位置：`JsonSerializer.cs:29`
```cs
public static JsonSerializer Inst{get;set;} = new(...);
```
（同工程設計第 4 條，此處為風險面。）改只讀即可消除。

**4. 其餘未發現問題**：`TypeInfoResolver` 明確、未註冊類型直接拋、無反射回退、`RequiresUnreferencedCode`/`RequiresDynamicCode` 照官方標註——AOT 姿勢正確。唯一例外見下方【待覈實】。

---

## 待覈實（需一次 build / publish）

**1. AOT 警告泄漏（預期 IL2026 + IL3050）**
位置：`IJsonSerializerExtn.Impl.cs:12`（呼叫端未標註）
```cs
var node = z.Deserialize<JsonNode>(Json, options);   // 被呼叫者是 [RequiresUnreferencedCode]
```
`DeserializeToDictList`（`IJsonSerializerExtn.cs:19`）自身沒標同款註解，警告不會傳播給調用方。修法二選一：(a) 補標兩條註解；(b) 改走 `JsonTypeInfo`/`JsonSerializerContext` 重載（但會吃不到調用方 `options` 的 `ReadCommentHandling`/`Encoder`，需拍板）。測試側 `TestSerialize.cs:30/51`、`TestDeserializeToDictList.cs:21` 直接呼叫註解重載，同樣預期有警告。

**2.** 功能正確性第 3 條（`JsonNodeCtx` 單獨序列化含數字的 DictJson 是否真拋 `Metadata for type 'System.Int64' ...`）。
**3.** 用例總數與結果（Debug `dotnet run` 與 `TestAotWin` 兩套是否全綠）。
**4.** 是否有 CS1998（用例 `async` 無 `await`）等警告——`tsinswreng-cstreetest` 樣例本身即如此，若是倉庫既有慣例可忽略。

## 需你確認

1. `Tsinswreng.CsJson/_TswgNote/2026_1007_171940/Review.typ`（17:19:40 建立，正好在 HEAD 提交後 6 秒；git 未跟蹤）。按核心規則我不得讀寫你的筆記，**沒有打開**；若那是本輪批註，請授權我讀取並在內回覆。
2. 記憶 `CsJsonRedesign.typ` 的需求清單（js 便利 7 條、DictJson⇄JsonNode 互轉、dict⇄實體互轉）現在還算數嗎？這決定「覈對需求」缺口的定性。
3. 工程設計第 3 條（Dict/List 寫入是否碰來源樹）要往哪邊統一？
4. 第 5 條改名 `DfltJsonSerializer` 是否可行？
5. 是否要我實際跑 `dotnet build` / `Test` / `TestAotWin` 把「待覈實」四條消掉？
6. 是否要我更新記憶（`CsJsonRedesign.typ` 目前仍寫「空殼、尚未進入設計與實作」，與現狀嚴重不符）？

本次為純讀取審查，未修改或新建任何文件。
````
]

-T[2026_1007_174519][
	那先不管Json和Dict互轉這一塊 就說 IJsonSerializer.cs 這一系列的 現在有甚麼問題?
][
````md
# IJsonSerializer 系列 代碼審查報告

審查範圍（用戶指定，Dict/Json 互轉那條線不談）：`proj/Tsinswreng.CsJson/IJsonSerializer.cs`、`JsonSerializer.cs`、`IJsonSerializerExtn.cs` + `.Impl.cs`、`JsonNodeCtx.cs`、`IJsonNodeSerializer.cs`，以及其測試 `Test/Domains/Json/` 四檔。
下文「位置」一律相對 `proj/Tsinswreng.CsJson/`。本輪未執行 `dotnet build` / `Test` / `TestAotWin`（純讀取，規範禁止執行產生項目輸出的建置命令），標【待覈實】者需一次構建定論。

---

## 功能正確性

**1. `DeserializeToDictList` 把「沒給輸入」與「JSON 字面量 null」混為一談**
位置：`IJsonSerializerExtn.Impl.cs:9-13`
```cs
if(str.IsNullOrEmpty(Json)){
    return null;
}
var node = z.Deserialize<JsonNode>(Json, options);
return JsonNodeDict.Adapt(node);
```
示例：`z.DeserializeToDictList("")` 與 `z.DeserializeToDictList("null")` 都返回 `null`，調用方無法區分；而 `z.DeserializeToDictList(" ")` 又會走解析路徑拋 `JsonException`。三種輸入、兩種結果，契約不成形。（測試 `Test/Domains/Json/TestDeserializeToDictList.cs:80-86` 正是把這三種寫成同一條斷言，等於把這個含糊固化了。）

**2. `JsonNodeCtx` 作為 options-only 重載的 resolver 不完整【待覈實】**
位置：`JsonNodeCtx.cs:22-24`，對比 `Test/Domains/Json/JsonSample.cs:25-28`
```cs
// JsonNodeCtx.cs —— 只有這三個
[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(JsonNodeDict))]
[JsonSerializable(typeof(JsonNodeList))]

// JsonSample.cs —— 測試自己補了標量，並寫明理由
// 枚舉 IDictionary<string,object?> 的值時按運行時類型查 TypeInfo
[JsonSerializable(typeof(i64))]
[JsonSerializable(typeof(f64))]
[JsonSerializable(typeof(str))]
[JsonSerializable(typeof(bool))]
```
`object?` 值序列化時按**運行時類型**查 metadata，所以缺 i64/f64/str/bool 時預期拋 `InvalidOperationException: Metadata for type 'System.Int64' was not provided by TypeInfoResolver`；而 `JsonNodeCtx.cs:19-20` 的 Doc 聲稱「`JsonNodeDict` 產物的再序列化…在 AOT 下也有 TypeInfo 可用」。且無任何測試只掛 `JsonNodeCtx` 走這條路（round-trip 用例 `TestDeserializeToDictList.cs:88-101` 用的是測試域自造的 `DictJsonCtx`）。

**3. 其餘未發現功能錯誤**：103 個方法在介面與實現逐一對應（實測計數：介面 103、實現 103），重載形狀/參數順序/`ref`/`ValueTask`/`IAsyncEnumerable` 返回型別與官方一致；`Serialize<TValue>(TValue, JsonTypeInfo<TValue>)` 等 AOT 安全重載未誤標 `RequiresUnreferencedCode`，不安全重載全部正確標註。

---

## 覈對需求

**1. 「與官方公開 API 面逐一對應」已達成**
位置：`IJsonSerializer.cs:17-251` 對 `JsonSerializer.cs:23-479`。可對帳：Serialize 15 / SerializeAsync 10 / SerializeToUtf8Bytes 5 / SerializeToElement 5 / SerializeToNode 5 / SerializeToDocument 5 / Deserialize 40 / DeserializeAsync 10 / DeserializeAsyncEnumerable 8 = **103 方法 + 1 屬性**，兩側 1:1，無缺無多。

**2. 「不增不減」是不實的自我描述**
位置：`IJsonSerializer.cs:12-16` 對比 `:19-20`
```cs
/// ...方法名、重載、參數形狀照抄官方，不增不減。
...
[Doc(@$"API函數 不傳options參數時默認用此")]
public JsonSerializerOptions? Options{get;set;}
```
實際有兩個 delta：增了 `Options` 成員；把「options 為 null」從官方 `JsonSerializerOptions.Default` 改成注入的 `Options`（`JsonSerializer.cs:49` 等 103 處 `options ?? Options`）。這個 delta 是本包存在的理由，應寫在主說明裏，而不是壓在「不增不減」底下。

**3. 「方便 DI」只到一半：庫側沒有註冊擴展**
位置：消費端自己手寫（`Ngan.Dict/Ngan.Dict.Core/proj/Ngan.Dict.Core/DiCore.cs:20`）
```cs
z.AddSingleton<Tsinswreng.CsJson.IJsonSerializer>(_ => new Tsinswreng.CsJson.JsonSerializer(AppJsonCtx.Inst.Options));
```
`CsSql` 已有 L1 模式（`AddCsSqlSqlite/Postgres` 把固定配方收進庫），本包沒有對應的 `AddCsJson(...)`；應用專屬 context 由應用給，但「建構 + 生命週期 + 只註冊介面」這段樣板可以收斂。

---

## 工程設計

**1. 104 成員的單一介面，違反介面隔離，堵死未來擴展**
位置：`IJsonSerializer.cs:17-251`
```cs
public interface IJsonSerializer{ ... 103 個方法 ... }
```
示例：想加一個日誌裝飾器、一個測試替身、或一個把請求轉發到遠端的代理，都必須實作 104 個成員（倉庫內目前唯一實現者是 `JsonSerializer.cs:23`）。這正是「未來擴展點」被堵死：`CsSql` 那套 `IXxxExtn` / 分層介面的做法在這裏沒用上。建議按能力拆 facet（同步字串字節 / Stream·PipeWriter / AsyncEnumerable）再給一個聚合介面；若決定就是要照抄官方，則應在 Doc 明確認領這個代價。

**2. 類名 `JsonSerializer` 與官方 `System.Text.Json.JsonSerializer` 同名，雷已埋到消費者腳下**
位置：`JsonSerializer.cs:23`；受害現場 `Ngan.Dict/Ngan.Dict.Core/proj/Ngan.Dict.Core/Tools/Json/JsonSerializer.cs:26/31`
```cs
var jsonElement = JsonSerializer.SerializeToElement(dict, AppJsonCtx.Inst.IDictionaryStringObject);
```
該檔只 `using System.Text.Json;`，此處 `JsonSerializer` 指官方那個；只要有人加一行 `using Tsinswreng.CsJson;`，這兩行立刻變 CS0104 歧義或語義漂移。而測試檔名與註釋本來就假設叫 `DfltJsonSerializer`（`Test/Domains/Json/_TestDfltJsonSerializer.cs:6-7`、`Test/CsJsonTestMgr.cs:13`）。

**3. `Options` 為 null 的實例沒有 fail-fast，AOT 誤配置只在運行時炸**
位置：`JsonSerializer.cs:40-42`、`:49`
```cs
public JsonSerializer(JsonSerializerOptions? Options = null){ this.Options = Options; }
...
return System.Text.Json.JsonSerializer.Serialize(value, options ?? Options);
```
示例：`new JsonSerializer().Serialize(s)` 完全合法編譯，運行時才 `InvalidOperationException`——測試 `Test/Domains/Json/TestSerialize.cs:46-57` 就是**把這個拋出當成正確行為**來斷言。建議構造或首次使用時 fail-fast（或提供 `TryEnsureUsable()`），把這個誤配置從運行時提前到啟動時；否則「零配置能用」與「AOT 必須注入」兩種狀態在型別上完全無法區分。

**4. `Inst` 是可寫全局單例，且與 DI 形成兩套並行配置源**
位置：`JsonSerializer.cs:29`；對比倉庫慣例 `Ngan.Dict/Ngan.Dict.Core/proj/Ngan.Dict.Core/Tools/Json/AppJsonS.cs:11`
```cs
public static JsonSerializer Inst{get;set;} = new(...);                 // CsJson：可寫
public static Tsinswreng.CsJson.IJsonSerializer Inst => field??= new(...); // 倉庫慣例：只讀
```
同一進程裏「改 `Inst`」與「DI 注入」兩條路都能改全局序列化行為。

**5. 無日誌、無可診斷性**
位置：`JsonSerializer.cs` 全檔（無 `ILogger`/`AppLog` 依賴）。`tsinswreng-csharp`：「多打日誌，方便依賴注入時就注入 `ILogger`」。目前「走官方 `Options.Default`（反射路徑）」「`Options` 為 null」這些關鍵分支完全靜默，出問題只能靠拋異常反查。

**6. `[Impl]` 只標了一個成員，判據不明**
位置：`JsonSerializer.cs:37`（唯一一處）對比倉庫他處標在實現成員上（`Tsinswreng.Avln.Grid/proj/Tsinswreng.Avln.Grid/GridStack.cs:30/47/52`）
```cs
[Impl]
public JsonSerializerOptions? Options{get;set;}
```
另外 103 個實現成員都沒標。要麼統一，要麼請你明確 `[Impl]` 的判據（我按現有用法推不出來）。

**7. `JsonAotMsg` 與 `JsonSerializer` 同檔**
位置：`JsonSerializer.cs:481-485`
```cs
internal static class JsonAotMsg{ ... }
```
一個檔案兩個型別，且此型別無 `[Doc]`。建議拆檔或至少補註釋說明其用途（複製官方警告文案，讓 IL 警告文本一致）。

**8. `IJsonNodeSerializer` 是死介面，且與 `IJsonSerializer` 概念重疊**
位置：`IJsonNodeSerializer.cs:7-11`
```cs
public interface IJsonNodeSerializer{
	public JsonNode? SerializeToNode(obj? Value, Type InputType, JsonSerializerOptions? options = null);
	//public object? Deserialize(JsonNode? node, Type returnType, JsonSerializerOptions? options = null){}
}
```
全倉庫零實現、零引用（grep 僅命中宣告處）；它宣告的能力 `SerializeToNode` 已在 `IJsonSerializer` 裏有 5 個重載。

---

## 代碼規範

**1. `[Doc]` vs `///`：硬違規，且同一檔內混用**
位置：`IJsonSerializer.cs:12-16`（型別說明用 `///`）、`:19`（`Options` 用 `[Doc]`）；`JsonSerializer.cs:12-22`（型別說明用 `///`）
```cs
/// 完整 JSON 序列化接口：與官方 System.Text.Json.JsonSerializer 的公開 API 面逐一對應，
```
`IJsonSerializer.cs` 整檔 104 個成員裏**只有 `Options` 一個**用了 `[Doc]`。`tsinswreng-csharp-doc-attr`：類型/成員一律用 `[Doc]`，不用斜槓註釋。（`IJsonSerializerExtn.cs`、`JsonNodeCtx.cs` 是合格的。）

**2. `[Doc]` 無示例、無 `nameof` 插值**
位置：`IJsonSerializer.cs:19`、`JsonNodeCtx.cs:13-21`、`IJsonSerializerExtn.cs:7-16`
```cs
[Doc(@$"API函數 不傳options參數時默認用此")]
```
`IJsonSerializerExtn.cs:10` 提「DictJson 的根不限定為對象」卻沒給 `#Eg`；`JsonNodeCtx.cs:16-17` 用純文字寫 `JsonSerializerOptions`/`JsonSerializer`，沒用 `{nameof(...)}`，f12 跳不過去。按 CsRefl 的批註判據（例子＝調用＋實參＋緊跟一行結果），這批新 `[Doc]` 不合格。

**3. 參數命名與 `CT` 約定未遵守**
位置：`IJsonSerializer.cs:26`、`:62`；`JsonSerializer.cs:48`、`:114`
```cs
string Serialize<TValue>(TValue value, JsonSerializerOptions? options = null);
Task SerializeAsync<TValue>(Stream utf8Json, TValue value, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default);
```
`tsinswreng-csharp`：「除函數中的局部變量外、所有標識符（包括函數參數）都用大駝峯」；「約定最後一個參數聲明爲 `CT Ct` 的爲異步函數」。此處 `value`/`options`/`jsonTypeInfo`/`cancellationToken` 全小駝峯，`CT` 別名（`proj/TypeAlias.cs:21` 已全局可用）沒用。

**4. `Task` vs `Task<nil>`**
位置：`IJsonSerializer.cs:62-68`、`JsonSerializer.cs:114-130`
```cs
Task SerializeAsync<TValue>(Stream utf8Json, ...);
```
skill：「異步函數不返回值時、返回值聲明爲 `Task<nil>`、不用無泛型的 `Task`」。此處照抄官方用了裸 `Task`。

**5. 異步實現沒有 `async`**
位置：`JsonSerializer.cs:114-150`（10 個 `SerializeAsync`）、`:406-442`（10 個 `DeserializeAsync`）
```cs
public Task SerializeAsync<TValue>(Stream utf8Json, TValue value, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default){
    return System.Text.Json.JsonSerializer.SerializeAsync(utf8Json, value, options ?? Options, cancellationToken);
}
```
skill：「異步函數實現（有方法體的即函數實現）一定要加 `async`」。這 20 個純轉發方法都沒有。此處若真加 `async` 反而要 `await` 包一層、改變異常時序，**所以我不建議直接改**——請你裁定「照抄官方」是否豁免本條（若要豁免，建議在檔案頭註釋寫明豁免理由）。

**6. 缺檔頭註釋**
位置：`IJsonSerializer.cs:1`、`JsonSerializer.cs:1`、`IJsonNodeSerializer.cs:1`
```cs
namespace Tsinswreng.CsJson;
```
其餘檔都有 `//Decl:... / //Impl:...` 檔頭，`IJsonSerializer.cs`/`JsonSerializer.cs` 直接從 `namespace` 開始。因是 interface+class 模式不需 partial，但檔頭說明「這是聲明/實現」仍應有。

**7. 測試用例名硬編碼 + 用匿名 lambda**
位置：`Test/Domains/Json/TestSerialize.cs:20/27/38/46`、`TestDeserialize.cs:20/29`、`TestDeserializeToDictList.cs:19/34/44/53/64/80/88`
```cs
R("Serialize_ZeroConfig_Uses_Injected_Defaults", async(o)=>{ ... });
```
`tsinswreng-cstreetest`：每個用例用常態函數聲明與實現、不用匿名函數；用例名用 `nameof(...)`。對比同專案的合規寫法 `Test/Domains/JsonNodeDictTests/TestJsonNodeDict.CachedMutate.Impl.cs:20/36`。被測函數名倒是用了 `nameof(IJsonSerializer.Serialize)` ✓。

**8. `[Doc]` 措辭與風格不統一**
位置：`IJsonSerializer.cs:19`
```cs
[Doc(@$"API函數 不傳options參數時默認用此")]
```
「API函數」用語突兀；缺 `#Sum`/`#Rtn`，與 `JsonNodeDict.cs` 的 `#Sum[]`/`#Descr[]` 風格不一致。

**9. Decl/Impl 分離本身合規（此項無問題）**：`IJsonSerializerExtn.cs` / `IJsonSerializerExtn.Impl.cs` 一對一、`partial` 齊全；`JsonSerializer` 走 interface+class，依 skill 不需 partial。

---

## 性能

逐項檢查後**未發現實質性能問題**，三點確認過：

**1. 介面轉發的開銷可忽略**：`JsonSerializer.cs` 103 個方法都是一次 interface dispatch + 官方靜態調用，無自身分配（`JsonSerializer.cs:48-479`）。

**2. `Options` 為 null 時退回官方 `JsonSerializerOptions.Default`**（`JsonSerializer.cs:49`），該實例是靜態緩存的，不會每次重建 metadata。

**3. 唯一的暖機成本來自可變 `Options`**：`JsonSerializerOptions` 首次使用後才凍結並緩存 metadata（這是官方機制），把 `JsonSerializer.Options` 當成熱路徑可變配置反復替換，每次都要重付暖機成本。
位置：`JsonSerializer.cs:37-38`
```cs
[Impl]
public JsonSerializerOptions? Options{get;set;}
```

---

## 安全性

**1. 默認單例用不安全編碼器，是注入面**
位置：`JsonSerializer.cs:31`
```cs
Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 允許原樣輸出
```
`<`/`>`/`&` 不轉義；產物若嵌進 HTML/`<script>` 即 XSS 通道。作為**庫的默認單例**選這個值偏危險（`Ngan.Dict` 為中文不轉義，那應由業務側配置）。

**2. 可寫全局單例：可被整體替換，也可被設成 null**
位置：`JsonSerializer.cs:29`
```cs
public static JsonSerializer Inst{get;set;} = new(...);
```
進程內任意代碼可換掉它；設成 null 後 `JsonSerializer.Inst.DeserializeToDictList(...)` 直接 `NullReferenceException`。改成只讀即可消除。

**3. 無數量上限與取消通道**
位置：`IJsonSerializerExtn.cs:19`
```cs
public static partial obj? DeserializeToDictList(this IJsonSerializer z, str Json, JsonSerializerOptions? options = null);
```
接受任意長 `str`、同步解析、無 `CancellationToken`（長輸入無法中斷）。`MaxDepth` 走默認 64（有 DoS 防護）但 Doc 未寫。

**4. 未發現類型混淆／反射回退類問題**：`TypeInfoResolver` 明確、未註冊類型直接拋、無反射 fallback、`RequiresUnreferencedCode`/`RequiresDynamicCode` 標註與官方一致——AOT 姿勢是這一系列做得最好的部分。

---

## 待覈實（需一次 build / publish）

**1. AOT 警告泄漏（預期 IL2026 + IL3050）**
位置：`IJsonSerializerExtn.Impl.cs:12`（呼叫端未標註）
```cs
var node = z.Deserialize<JsonNode>(Json, options);   // 被呼叫者是 [RequiresUnreferencedCode]
```
`DeserializeToDictList`（`IJsonSerializerExtn.cs:19`）自身沒標同款兩條註解，警告不會傳播給調用方；測試側 `Test/Domains/Json/TestSerialize.cs:30/51`、`TestDeserializeToDictList.cs:21` 直接呼叫註解重載，同樣預期有警告。修法二選一：(a) 補標兩條註解；(b) 改走 `JsonTypeInfo`/`JsonSerializerContext` 重載（但會吃不到調用方 `options` 的 `ReadCommentHandling`/`Encoder`，需拍板）。

**2.** 功能正確性第 2 條：只掛 `JsonNodeCtx` 序列化含數字的產物，是否真拋 `Metadata for type 'System.Int64' was not provided`。

**3.** 是否有 CS1998（測試用例 `async` 無 `await`）等警告——`tsinswreng-cstreetest` 樣例本身即如此，若是倉庫既有慣例可忽略。

## 需你裁定

1. 工程設計第 1 條：`IJsonSerializer` 要不要按能力拆 facet，還是明確認領「照抄官方」的 104 成員代價？
2. 工程設計第 2 條：改名 `DfltJsonSerializer` 是否可行？
3. 代碼規範第 3~5 條（`CT Ct`／`Task<nil>`／`async`）：照抄官方是否豁免本倉庫的異步與命名約定？若豁免，是否在檔頭註釋寫明？
4. 工程設計第 6 條：`[Impl]` 的判據是什麼（只標 `Options` 是否有意為之）？
5. 是否要我跑 `dotnet build` / `Test` / `TestAotWin` 把「待覈實」三條消掉？

本次為純讀取審查，未修改或新建任何文件。
````
]
