# Development Progress

此檔案是 AegisRTS FrameworkLab 的唯一正式開發進度紀錄。格式與更新規則見 [`docs/09_DevelopmentProgress_開發進度紀錄規範.md`](docs/09_DevelopmentProgress_開發進度紀錄規範.md)。最新紀錄置頂。

## Current Status

- Current Phase：PlayablePrototype_01 的 `fortified-city` 玩家攻城垂直切片與步兵／弓兵 Prototype L3 已可玩；依Phase 03.5 Revision 01任務已從受保護的P035建立`CHR_Infantry_A_v004_P035R1`，只修正手臂實際長度、手掌與頭部比例並完成隔離Unity evidence，狀態為`READY FOR PHASE03_5 REVISION REVIEW`；19項Gate仍待人工決定，未進Phase 04，也不是Production Ready。
- Active Branch：`main`
- Last Trusted Runtime Validation：FrameworkLab EditMode 180/180、PlayMode 40/40；攻擊取消 domain targeted 2/2、軍團移動取消 1/1、弓兵 Attack-Move presentation 1/1、步兵實際移動直立檢查 1/1；Infantry／Archer Unity Builder PASS；步兵來源 manifest 26/26；Windows Development Build PASS（BuildReport 187,670,429 bytes），`.exe` 啟動 10 秒 process responding、Player log error scan 0 hits；solution build 0 warnings／0 errors。2026-08-13 P035R1另完成Unity 6000.5.7f1 isolated A-Pose／L1Pose review import、C# compile、scene／prefab build與4張capture，batchmode return code 0；未執行EditMode／PlayMode／Windows build且未修改正式Runtime Prefab，歷史runtime結果不代表P035R1通過。
- Unity Project Version：`6000.5.7f1`
- Highest Priority：Art／Design／Technical Art審核`docs/ArtProduction/ReviewPackages/Infantry_Phase03_5_Revision01_Review/05_Review_Checklist.md`、P035 vs P035R1 ArmFocus、final L1 overlay與Unity L1Pose RTS Normal；明確批准前不得建立PRE-UV GEOMETRY LOCK、進Phase 04或替換Runtime Prefab。
- Specification Difference：Unity 產品場景已改用 runtime-baked NavMesh 並讓 Gate 真實阻擋內院；純 C# tests 仍使用 deterministic `INavigationAdapter`，這是刻意的測試 seam。新 production 規格的 20k–35k LOD0、LOD3／Impostor、packed team mask 與多 influence deformation 是 `PROPOSED` 量產 gate，不是目前低模 Prototype 已實作的 runtime baseline。

## 2026-08-13 — Infantry Phase 03.5 Revision 01 Arm Proportion v004_P035R1

- Status：`READY FOR PHASE03_5 REVISION REVIEW`（versioned source、比例修正、posed measurements、DCC／Unity evidence、reports、19-item pending gate、folder、ZIP與逐檔驗證完成；未自行宣告PASS、PRE-UV lock或進Phase 04）
- Goal：完整執行`mission/Infantry_Remaster_Phase03_5_Revision01_Arm_Proportion_Task.md`；以P035為唯一輸入建立`CHR_Infantry_A_v004_P035R1.blend`，只修正upper arm／forearm實際長度、hand與head比例，保留P035其他已批准比例與secondary forms，輸出隔離review assets／evidence／ZIP；不覆寫baseline、不改正式Runtime Prefab、不commit／push。
- Baseline：
  - Branch／HEAD：`main`／`0422d6b98907095ffe8b47a7673a90aa72c8bea0`；本次開始時延續前序未提交成果，依P035結案紀錄為304 all-file entries／22 collapsed，均保留且未清理。
  - Task：1,126 lines／16,199 bytes／SHA-256`3A29BDDC9E2AD30269907A07996347330FDFAFB3AADC618939A2C8F0C8EB42C4`；指定preferred upper arm 0.176H、forearm 0.165H、combined 0.334–0.346H、hand width 0.057–0.062H、head width 0.118–0.122H。
  - Protected inputs：P035 410,145 bytes／SHA-256`234383811F66F26DE29C8DBEF5E31C1B65D58B6B1E07C3FD08F3FF0AAF46422B`；P03R1 SHA-256`C6429918EA147E65713B31EF9D6940EC313C46DA1D2F4404032CA253B4B72F31`；v004 SHA-256`E1B6B3DFF07258184484186E5AEF8A380457A04E7899107D77908ECABCAA0046`。
- Scope In：
  - 完整讀取任務、前序Phase 03.5、必讀00／03／04／09／27、DevelopmentProgress與受影響Production Spec；鎖定P035 3D measurements及L1 front landmarks。
  - 以Blender 5.2.0 LTS從exact-hash P035產生新P035R1；實際改變雙側upper-arm／forearm rest lengths，重新貼合upper-arm、elbow、forearm、bracer、hand／thumb與手持裝備；head收至0.120H、neck寬度局部縮4%。
  - 建立Source A-Pose與非active、fake-user的`REVIEW_ONLY_POSE_L1_COMPARE`；輸出A-Pose／L1Pose／ArmDetail／Overlay／Comparison／64 px／32 px及Unity Close／RTS evidence，並建立review-only A-Pose／L1Pose FBX、Prefabs與Scene。
- Scope Out／Deferred：Phase 04、Final UV／packing、Final Texture／BaseColor／Normal／AO／ORM／Team Mask／Shader、Final Skinning／deformation proof、Animation Polish、正式LOD、gameplay/API change、Runtime Prefab replacement、PRE-UV lock、Golden Sample lock與Production Ready。
- Major Files／Assets：
  - 新增`ArtSource/Units/Infantry/CHR_Infantry_A/v004/Source/CHR_Infantry_A_v004_P035R1.blend`：412,843 bytes／SHA-256`A0CCC9771CD7A62D966891784745F138A9DCBF1230DF5E71A4F5D60900A84D0A`；新增build／render／comparison／FBX export／audit／finalize共6個工具並更新v004 README。
  - 新增`Assets/AegisRTS/Review/InfantryPhase035Revision01/`：2 review FBX、7 materials、2 Prefabs、1 Scene；新增`Assets/AegisRTS/Editor/InfantryPhase035Revision01ReviewBuilder.cs`。兩個Unity FBX與package copy逐檔SHA-256相同，正式runtime paths未替換。
  - 新增`docs/ArtProduction/ReviewPackages/Infantry_Phase03_5_Revision01_Review/`：60 files，含8 top-level Markdown、24 PNG（Screenshots 23＋L1 reference 1）、3 measurement JSON、Blend＋6 tools、2 FBX、7 specification snapshots與9 manifests／log；同名ZIP 13,291,270 bytes／SHA-256`FAAC04B53875A05A4F7449CDF0A598007C2B2161799D537D6E019AD113229302`。
  - 同步Production Spec 08／09／16／17／99，重建`RTS_Asset_Production_Spec_v1.zip`：21 entries／64,015 bytes／SHA-256`471422A3D19B2AA3DBCDE0BD6A0F3B75E2DAB204F320532DCC0374A7FC55B242`，folder／ZIP逐檔hash一致。
- Behavior Before／After：
  - Before P035：upper arm 0.154943H、forearm 0.154943H、combined 0.309886H、hand width 0.069423H、head width 0.126651H；L1 pose中wrist約比2D reference高2.30%H、hand end高3.25%H，手臂與手掌讀形偏短厚。
  - After P035R1：upper arm 0.176000H、forearm 0.165000H、combined 0.341000H、hand width 0.060652H、hand length 0.057828H、head width 0.120000H，均落入任務target；L1 pose wrist差約−0.72%H、hand end差約+0.71%H。
  - Preserved：總高1.824010849 m、anatomical shoulder 0.260856H、hip 0.356358H、knee 0.249450H、torso 0.401128H、shield 0.600000×0.862424 m、sword 1.061217 m、boots／armor／cloth secondary forms、98 meshes／16,858 verts／33,248 tris與23-bone hierarchy。
- Architecture／API／Data：
  - Definition／Runtime／View、Player／AI Command、UI/gameplay authority完全不變；新增C#只在Editor與隔離Review namespace，無production runtime參照或資產替換。
  - Existing public API、Animator parameters、events、socket／anchor與bone names無變更；review FBX無gameplay animation clips。Source A-Pose仍是neutral，comparison Action不是active action或final Idle。
  - Build／measurement／geometry／Unity manifests記錄input／output SHA、normalized ratios、pose landmarks、topology、preserved dimensions、Unity import bounds與`runtime_prefab_replaced=false`。
- Tests／Validation：
  - Blender deterministic build及封裝內`.blend` read-only reopen：output SHA與build result相符；98 meshes、16,858 vertices、33,248 triangles、1 armature、23 bones、1 review-only action、active action null、height 1.824010849 m。
  - Topology audit：non-manifold 0、boundary 0、loose 0、zero-area 0；33,248 triangles落在32K–35K target；P035／P03R1／v004 protected hashes未變。
  - Posed landmark audit：shoulder 0.757485H保持；elbow 0.607784→0.587439H、wrist 0.454726→0.424446H、hand end 0.392712→0.367393H；與L1 2D front reference的手腕／手端誤差明顯縮小。
  - Visual evidence：Apose 2、ArmDetail 3、Comparison 7、L1Pose 2、Overlay 3、ScreenSize 2、Unity 4，共23 screenshots；final front／3Q、arm focus、64／32 px、skeleton overlay與Unity side-by-side均已人工目視，未見新的明顯穿插或裝備脫手。
  - Unity 6000.5.7f1 graphics-enabled batch：C# compile、2 FBX import、2 Prefabs與Scene建立成功；A-Pose／L1Pose height皆1.824011 m、boot ground Y皆0、各98 renderers、L1 overall minimum Y 0、process return code 0且log有`Exiting batchmode successfully now!`。初次L1 sword review angle造成tip低於地面及首張材質warm-up色偏，已修正角度並加warm-up capture；final manifest／captures反映修正後結果。
  - Scripts／docs／data：Python syntax 4/4、PowerShell parse／execution 2/2、JSON parse 6/6、CSV parse 4/4、top-level Markdown H1 8/8、unmatched fences 0；checklist 19 unchecked／0 checked；`.blend1/.blend2`與`__pycache__`皆0。
  - Review ZIP：folder 60 files＝ZIP 60 file entries；required 12/12、missing 0、hash mismatch 0。Production Spec ZIP：21 files＝21 entries、hash mismatch 0。`git diff --check` PASS。
  - Unity EditMode／PlayMode、Windows build、Profiler與battle-count test：`NOT RUN`；本次為隔離art revision review，未用歷史runtime結果冒充P035R1 gameplay acceptance。
- Acceptance：versioned P035R1、protected baselines、actual arm rest-length correction、target ratios、preserved dimensions／topology／budget／bones、DCC／Unity evidence、folder／ZIP與spec sync為machine-delivery PASS；19-item visual／art direction gate為`REVIEW PENDING`；Phase04與所有deferred項目為`NOT RUN / NOT AUTHORIZED`。
- Completed：required reading、baseline hash locks、focused bilateral arm／hand／head correction、equipment refit、before／after measurements、23 screenshots＋reference、isolated Unity import／capture、reports／pending checklist／manifests、review folder／ZIP、Production Spec同步與兩個ZIP逐檔驗證。
- Not Completed／Deferred：human Phase03.5 Revision decision、PRE-UV geometry lock、Phase04、final UV／texture／shader／team mask、final skin／deformation、animation polish、formal LOD／profile、provenance／license、Golden Sample lock與Production Ready。
- Known Issues／Risks：L1是具透視與裝甲遮擋的2D stylized reference，shoulder／elbow landmarks仍有判讀誤差；static A-Pose／review pose及無動畫FBX不是final skin deformation proof；shield／sword握持與手掌可讀性仍需Art／Technical Art在19項人工gate判定。Final evidence沒有已知ground penetration，初版review-pose issue未帶入交付。
- Git：結案仍為`main`／HEAD`0422d6b98907095ffe8b47a7673a90aa72c8bea0`；`git status --porcelain=v1 --untracked-files=all`為401 entries（7 modified／394 untracked）／27 collapsed，包含開始前未提交的Phase 03／03R1／03.5成果；未commit、未push、未reset或清理使用者既有變更。
- Next（排序）：
  1. Art／Design／Technical Art逐項審核`05_Review_Checklist.md`的19項，重點比較ArmFocus、L1 front overlay、hand／equipment contact及Unity RTS Normal。
  2. 若change requested，只建立下一個versioned focused revision並保留P035／P035R1；若批准，仍須明確授權PRE-UV GEOMETRY LOCK與Phase04。
  3. 後續另立gate處理Final UV／Texture／Team Color shader、skin／animation、LOD／performance與provenance，不得把本次READY狀態當作Production Ready。

## 2026-08-13 — Infantry Phase 03.5 L1 Proportion Alignment v004_P035

- Status：`READY FOR PHASE03_5 REVIEW`（diagnosis、versioned source、L1／3D measurements、A-Pose／L1Pose evidence、Unity review、reports、19-item pending gate、folder、ZIP與逐檔驗證完成；沒有自行宣告PASS或進Phase 04）
- Goal：完整執行`mission/Infantry_Remaster_Phase03_5_L1_Proportion_Alignment_Task.md`；保留P03R1，建立`CHR_Infantry_A_v004_P035.blend`，依DIAGNOSE→CORRECT順序量化Pose／Proportion／Armor差異，只修正實際mismatch，建立同名Review folder與ZIP，不做Final UV／Texture／Skinning／Animation Polish／LOD，不commit／push。
- Baseline：
  - Branch／HEAD：`main`／`0422d6b98907095ffe8b47a7673a90aa72c8bea0`；開始時dirty worktree為188 all-file entries／18 collapsed，含前序Phase成果，全部保留。
  - Task：1,912 lines／23,121 bytes／SHA-256`9C6301C28C900896AFEFC687E4396F47DB050524D6DD93E3A1CA9ACEC9EC4E57`；明確指定P03R1為approved Secondary Forms baseline與輸出P035。
  - Protected P03R1：408,192 bytes／SHA-256`C6429918EA147E65713B31EF9D6940EC313C46DA1D2F4404032CA253B4B72F31`；protected v004 SHA-256`E1B6B3DFF07258184484186E5AEF8A380457A04E7899107D77908ECABCAA0046`；L1 reference SHA-256`C6BD352DE06854F8CD59AC92C891E43A0D40070D8F1AB72613358845847F461F`。
- Scope In：
  - 完整讀取43份／14,661 lines mandatory、Phase 01–03.5、P03R1 review與Production Spec文件；視覺檢查L1原圖與P03R1 comparison。
  - 從L1 friendly-blue front／side實際建立L00–L10、雙臂landmarks與width estimates，明列confidence／estimated；從Blender armature bone heads／tails與mesh bounds直接量測3D。
  - 先以unchanged P03R1建立Source A-Pose與temporary L1 Compare Pose；再依量測做piecewise vertical correction與controlled local scales；重建A-Pose／L1Pose、front／side／back overlays、annotated、before／after、64／32 px證據。
  - 建立兩個無動畫review FBX、isolated A-Pose／L1Pose Prefabs與Scene，以Unity 6000.5.7f1拍攝A-Pose Close、L1Pose Close、L1Pose RTS Normal。
- Scope Out／Deferred：Phase 04、Final UV／packing、Final Texture／BaseColor／Normal／AO／ORM／Team Mask／Shader、Final Skinning、Animation Polish、正式LOD、Runtime Prefab replacement、gameplay/API change、PRE-UV lock與Production Ready。
- Major Files／Assets：
  - 新增`CHR_Infantry_A_v004_P035.blend`：SHA-256`234383811F66F26DE29C8DBEF5E31C1B65D58B6B1E07C3FD08F3FF0AAF46422B`；新增build／render／overlay／FBX export／audit／finalize工具、P035_BUILD_RESULT與v004 README紀錄。
  - 新增`Assets/AegisRTS/Review/InfantryPhase035/`：A-Pose／L1Pose FBX、7 review materials、2 Prefabs、1 Scene；新增`Assets/AegisRTS/Editor/InfantryPhase035ReviewBuilder.cs`。正式runtime paths未替換。
  - 新增`docs/ArtProduction/ReviewPackages/Infantry_Phase03_5_L1_Proportion_Alignment_Review/`：80 files，含7 top-level reports、43 PNG、3 measurement JSON、Blend＋7 tools、2 FBX、6 task snapshots、L1 reference、manifests／Unity log。
  - 新增ZIP：21,205,494 bytes／SHA-256`31E5726A2BEA7A824717BD1B1DE0B12C35989021FAA0B01B11800FFBC7223501`。同步Production Spec 08／09／16／17／99並重建spec ZIP：21 entries／63,504 bytes／SHA-256`CE60E533C70C83E7F44EE86F975873100A4D310258F865E885DAF393A6FAB683`。
- Behavior Before／After：
  - Before：A-Pose外張使arm／shoulder差異被放大；normalized hip 0.4414 vs L1 estimate 0.3634、knee 0.2859 vs 0.2578、torso 0.3160 vs 0.4161、upper leg 0.1556 vs 0.1056；chest／head mesh／hand／boot有寬度警示。
  - After：hip 0.3564、knee 0.2495、torso 0.4011、upper leg 0.1070、lower leg 0.1742；chest width 0.2939→0.2762、head 0.1362→0.1267、boot 0.1177→0.1107。Arm bone lengths、anatomical shoulder、Helmet、Shield 0.600×0.862 m、Sword 1.061 m與1.824011 m總高保留。
  - A-Pose仍是正式neutral source；`REVIEW_ONLY_POSE_L1_COMPARE`有fake-user／review metadata但不是active action、bind pose或final Idle。
- Architecture／API／Data：
  - Definition／Runtime／View、Player／AI Command與UI/gameplay authority不變。新增C#只在Editor與isolated Review folder，無production runtime參照。
  - Existing API／Animator／event／socket／anchor／bone name／hierarchy無變更；23 bones不增不刪。Review FBX不含animation clips。
  - Scene data記錄SourceBaseline／SHA、SourceVersion、ReviewStatus、neutral/review pose名稱與所有deferred flags；6 MATID仍非final material contract。
- Tests／Validation：
  - Blender deterministic build＋source/package read-only reopen：98 meshes、16,858 verts、33,248 tris、23 bones、1 review-only action、active action null、height 1.824010849 m；source/package SHA相同。
  - Topology：non-manifold 0、boundary 0、loose 0、zero-area 0；P03R1與v004 protected hashes等於baseline。
  - L1／3D data：Front＋Side landmark JSON、Before＋After 3D JSON與correction table存在；Shield／Sword明示dimension gates通過。
  - Evidence：43 PNG＝Diagnostic 8、Apose 4、L1Pose 4、Overlay 9、Annotated 1、Comparison 12（8張before／after原圖＋4張composite）、ScreenSize 2、Unity 3；關鍵final front／3Q／overlay與Unity三圖已人工目視。
  - Unity：builder compile與graphics-enabled batch成功；A-Pose 1.824011 m、L1Pose posed bounds 1.841581 m、各98 renderers、boot ground Y皆0、runtime prefab replaced=false。L1Pose sword tip最低Y−0.017570 m已記錄為review-pose clearance issue。
  - Package ZIP：folder 80 files＝80 entries，15 required 15/15，missing／extra／hash mismatch皆0。Spec ZIP 21＝21且hash mismatch 0。
- Acceptance：new P035、P03R1 preserved、Source A-Pose、review pose、measurements、overlays、height／budget／bones／topology、Unity、folder／ZIP為machine-delivery PASS；19-item human visual gate為REVIEW PENDING；Phase04與所有禁止項目NOT RUN／NOT AUTHORIZED。
- Completed：required reading、baseline locks、diagnostic pose pair、L1／3D measurements、classification、measured corrections、armor／cloth refit、final evidence、Unity isolated review、reports／checklist／manifests、folder／ZIP、spec sync與parity verification。
- Not Completed／Deferred：human Phase03.5 decision、PRE-UV geometry lock、Phase04、final UV／texture／shader、skin／deformation、animation polish、formal LOD／profile、provenance／license、Golden Sample lock與Production Ready。
- Known Issues／Risks：L1 hip及部分arm joints受armor遮擋而low-confidence；UpperArm／Forearm ratio超過初始threshold但pose evidence不支持直接改骨長；hand在−7%後仍大於L1 estimate但再縮可能傷害RTS readability；review-only sword tip低於ground 1.757 cm；static object-bound source不等於deformation proof。
- Git：結案仍`main`／HEAD`0422d6b98907095ffe8b47a7673a90aa72c8bea0`；`git status --porcelain=v1 --untracked-files=all`為304 entries／collapsed 22；未commit、未push。新Unity狀態限Editor P035 builder與`Assets/AegisRTS/Review/InfantryPhase035/`；既有dirty changes均保留。Blender自動生成的P035 `.blend1`與Python cache已移出workspace，正式source／package不受影響。
- Next：
  1. Reviewer逐項檢視19項Gate，特別決定arm ratio uncertainty、hand readability與final overlays。
  2. 若change requested，建立新versioned focused P035 revision；若批准，需明確授權PRE-UV GEOMETRY LOCK與Phase04。
  3. 後續獨立處理UV／Texture／Team Color shader、skin／animation、LOD／performance與provenance。

## 2026-08-13 — Infantry Phase 03 Revision 01 v004_P03R1 與 Unity Review Package

- Status：`READY FOR PHASE03 REVISION REVIEW`（focused source、37張DCC／Unity evidence、required reports、18-item pending gate、manifests、folder、ZIP與逐檔驗證完成；沒有自行宣告Phase 03 PASS）
- Goal：完整執行`mission/Infantry_Remaster_Phase03_Revision01_Task.md`；保留且不覆寫`CHR_Infantry_A_v004`，建立`CHR_Infantry_A_v004_P03R1.blend`，只修訂Front Waist Cloth、Scarf、Upper Arm Cloth、Shield Back與Boot integration，提供Unity RTS preview，建立`Infantry_Phase03_Revision01_Review/`及同名ZIP；不進Phase 04、不commit、不push。
- Baseline：
  - Branch／HEAD：`main`／`0422d6b98907095ffe8b47a7673a90aa72c8bea0`；開始時worktree已有前序Phase 03未commit成果，`git status`為76 all-file entries／11 collapsed entries，全部保留。
  - Revision task：1,540 lines／19,087 bytes／SHA-256 `2544094A06A6CC23738F34D7210CFC798990A01AB1F665BD8C3672B6928AB3E5`；Reviewer結論要求focused change，不得自行PASS。
  - Protected v004：1.824010849 m／106 meshes／17,193 vertices／33,898 triangles；input SHA-256 `E1B6B3DFF07258184484186E5AEF8A380457A04E7899107D77908ECABCAA0046`。Protected P02R1 SHA-256仍為`6569A14825B53393B72306F79BF8B29F9DEA5C0FFAB8E30686E901D61964220F`。
- Scope In：
  - 完整讀取Revision 01 task、Phase 01／02／P02R1／03 tasks及review結果、必讀00／03／04／09／27、DevelopmentProgress與相關RTS production specs；依task逐項鎖定preserve／modify／defer範圍。
  - 以Blender 5.2.0 LTS從exact-hash v004建立新P03R1；局部處理waist cloth、scarf、upper arm、shield back、boots，輸出Clay／Detail／Comparison／L1／Silhouette／ScreenSize／Material-ID evidence與review-only FBX。
  - 在`Assets/AegisRTS/Review/InfantryPhase03Revision01/`建立隔離Prefab／Scene／neutral clay與Material-ID materials，以Unity 6000.5.7f1輸出Close／RTS Normal／Far／Material-ID Normal capture；正式Runtime Prefab不連接、不替換。
- Scope Out／Deferred：
  - Phase 04、Final UV、Final Texture／bake／BaseColor／Normal／ORM／Team Color texture、Final Skinning、Animation Polish、正式LOD／impostor、gameplay integration與Runtime Prefab replacement。
  - 未改寫原v004、P02R1或更早來源；未修改既有正式Blender／FBX／Texture／Material／Prefab／Animation／Shader／Scene；未commit、未push。
- Major Files／Assets：
  - 新增`ArtSource/Units/Infantry/CHR_Infantry_A/v004/Source/CHR_Infantry_A_v004_P03R1.blend`：408,192 bytes／SHA-256 `C6429918EA147E65713B31EF9D6940EC313C46DA1D2F4404032CA253B4B72F31`；新增revision／render／FBX export／comparison／finalize scripts及`Documentation/P03R1_BUILD_RESULT.json`，更新v004 README。
  - 新增`Assets/AegisRTS/Review/InfantryPhase03Revision01/`的review-only FBX、8 materials、Prefab與Scene；新增`Assets/AegisRTS/Editor/InfantryPhase03Revision01ReviewBuilder.cs`。Review FBX為782,732 bytes／SHA-256 `954637DD4600941981E8727C2B9264EEF91C6C346021EF32A388A3DFE7EF2DA3`。
  - 新增`docs/ArtProduction/ReviewPackages/Infantry_Phase03_Revision01_Review/`：77 files／30,231,758 bytes；8 top-level Markdown、37 PNG、revision blend＋5 tools、review FBX、5 task snapshots、L1＋exact v004 baseline references與manifests／logs。
  - 新增同名ZIP：29,517,753 bytes／SHA-256 `6A2C65E38C4E7B297F2EF8282CDE2C7F14E13337F082A31FCF71E30159E3682B`。
  - 更新Production Spec 08／09／16／17／99並重建`RTS_Asset_Production_Spec_v1.zip`：21 entries／62,937 bytes／SHA-256 `3A61048B60CE70C95C5EE0FB217780144650631C2784320C4302459E623DF5ED`。
- Behavior Before／After：
  - Before：v004 front waist讀成有中央硬條的平板、scarf截面偏圓管、upper arm偏完美圓柱、shield back brace零碎且strap／grip不清楚、boot panels像黏貼件；原Phase 03 package沒有Unity capture。
  - After：waist成為有薄厚度、中央broad fold、兩側主平面與輕微不對稱hem的cloth panel；scarf採寬扁貼胸drape；upper arm成為planar cloth volume；shield back只保留清楚主／次brace並建立broad strap、palm-aligned grip與attachment pads；boot toe／upper transitions直接整合進primary mesh且sole保留。
  - Visual QA：初版發現scarf過大偏三角、waist fold過弱、upper arm仍圓；第二輪縮窄scarf並加強planar arm；第三輪重建封閉waist cloth並提高fold profile。過程中兩次捕捉到scarf／waist topology boundary，均在final save前修正為0。
- Architecture／API／Data：
  - Architecture：Definition／Runtime／View、Player／AI Command、UI/gameplay authority完全未動；新增Unity資產只在明確Review namespace／folder內，不參與runtime truth。
  - API：沒有修改既有C# public API、Command／Event／Query、Animator parameters、`AttackImpact`、socket／anchor或Prefab ID；新增Editor entry point只負責隔離匯入、量測與截圖。
  - Data：P03R1 scene記錄`SourceVersion=CHR_Infantry_A_v004_P03R1`、input SHA、`ReviewStatus=READY FOR PHASE03 REVISION REVIEW`與deferred flags；6個`MATID_*`與Unity neutral materials只供review。
- Tests／Validation：
  - Blender final build／read-only reopen source與package copy：兩者皆98 meshes／16,858 vertices／33,248 triangles／1 armature／23 bones／0 Actions，SHA-256完全相同；height 1.824010849 m。
  - Geometry audit：non-manifold 0、boundary 0、loose 0、zero-area 0；6 Material IDs、10 empties；33,248 triangles位於task建議32K–36K範圍。
  - Visual evidence：37 PNG＝Clay 5、Detail 6、Comparison 7、L1 Comparison 2、Material-ID 3、Silhouette 4、ScreenSize 6、Unity 4；final waist、shield-back、Unity Close／Normal／Material-ID已人工目視。
  - Unity：C# compile成功；review FBX輸入與Unity copy hash相同；imported height 1.824011 m、ground minimum Y 0、98 renderers、animation import disabled、Runtime Prefab replaced=false。第一次`-nographics` Null Device產生空白圖，已判定無效並由graphics-enabled hidden batch run成功重拍；兩次log均保留。
  - Script／docs：Python syntax 3/3；new `__pycache__` 0；8/8 top-level Markdown exactly one H1、unmatched fences 0；Revision checklist 18 pending／0 checked。
  - Review ZIP：folder 77 files＝ZIP 77 file entries；required entries 9/9；missing 0、extra 0、hash mismatch 0。Production Spec ZIP：21＝21 entries；missing 0、hash mismatch 0。`git diff --check` PASS；`.blend1/.blend2` 0。
  - Unity EditMode／PlayMode／Windows build／Profiler：`NOT RUN`；本次只驗證review import與camera evidence，不以歷史結果冒充P03R1 gameplay acceptance。
- Acceptance：
  - New P03R1／original v004 exact hash preserved／height／32K–36K／topology／required detail／comparison／Unity／folder／ZIP：`PASS`（機器與交付完整性gate）。
  - 18-item Phase 03 Revision visual gate：`REVIEW PENDING`；18/18維持unchecked，沒有agent-issued Phase 03 PASS。
  - Phase 04／Final Texture／Final UV／Animation Polish／formal runtime replacement：`NOT RUN / NOT AUTHORIZED`。
- Completed：required reading、version safety、三輪focused geometry修訂、final topology audit、37 captures、Unity isolated import/capture、reports、pending checklist、manifests、source／review folders、review ZIP、Production Spec同步與兩個ZIP逐檔驗證。
- Not Completed／Deferred：human Phase 03 Revision decision；Phase 04；final UV／texture／shader／team mask；skin／deformation；animation／durable Actions；formal LOD／profile；provenance／license；Golden Sample lock與Production Ready。
- Known Issues／Risks：
  - Reviewer仍須判斷waist fold布感、scarf寬扁語彙、upper-arm planar讀形、shield grip尺寸／接觸位置與boot整合是否達標。
  - Unity只用neutral clay／Material-ID review shader；不代表final lighting或material acceptance。FBX不含animation clips，靜態shield-hand contact不代表final deformation。
  - Source仍為static A-pose review candidate；Final UV／texture／skin／animation／LOD與battle-count profile均不存在。
- Git：結案為`main`／HEAD `0422d6b98907095ffe8b47a7673a90aa72c8bea0`；`git status --porcelain=v1 --untracked-files=all`為187 entries／collapsed 17，包含開始前前序Phase工作及本次成果。Assets新增status只限Editor builder與`Assets/AegisRTS/Review/InfantryPhase03Revision01/`；原v004 SHA保持`E1B6...0046`，未commit、未push。
- Next（排序）：
  1. Art／Design／Technical Art依`05_Review_Checklist.md`檢視五部位comparison、L1／64 px、Unity Close／RTS Normal／Far並做人工決定。
  2. 若change requested，只建立下一個versioned focused revision且保留v004／P03R1；若批准，仍須由使用者明確授權後才可進Phase 04。
  3. 後續另立gate處理Final UV／Texture／Team Color shader、skin／animation、LOD／performance與provenance，不得把本次READY狀態當作Production Ready。

## 2026-08-13 — Infantry Phase 03 Secondary Forms v004 與 Review Package

- Status：`READY FOR PHASE03 REVIEW`（Secondary Forms source、32張DCC evidence、required reports、manifests、資料夾、ZIP與逐檔驗證已完成；依任務要求未自行宣告Phase 03 PASS）
- Goal：完整執行`mission/Infantry_Remaster_Phase03_SecondaryForms_Task.md`；保留`CHR_Infantry_A_v003_P02R1`，建立新的`CHR_Infantry_A_v004.blend`，只處理裝甲／布料／裝備的Secondary Forms，建立`Infantry_Phase03_SecondaryForms_Review_v001/`與同名ZIP，不修改正式Runtime Prefab、不commit、不push。
- Baseline：
  - Branch／HEAD：`main`／`0422d6b98907095ffe8b47a7673a90aa72c8bea0`；開始時`git status`為clean（collapsed 0／all files 0）。
  - Phase 03 task：1,873 lines／22,200 bytes／SHA-256 `A86BB963B45C1972577FB50FE57270F64E3005EE2D9DE9035AB98DFD73E4C1AE`。
  - Protected P02R1：339,535 bytes／SHA-256 `6569A14825B53393B72306F79BF8B29F9DEA5C0FFAB8E30686E901D61964220F`；v002、initial v003與P02R1 review ZIP hashes亦在開始時鎖定。
  - Authorization：使用者本次明確要求讀取已通過的Phase 01／02／P02R1並直接執行Phase 03，故把該指示記為P02R1 downstream authorization；既有P02R1 checklist沒有回寫成agent-issued approval。
- Scope：
  - In：完整讀取Phase 01、Phase 02、Revision 01、Phase 03、必讀00／03／04／09／27、DevelopmentProgress與13份相關Production Spec；以Blender 5.2.0 LTS從P02R1建立versioned v004；加入chest／shoulder／helmet／plume／scarf／bracer／hand／belt／waist／leg wrap／knee／boot／shield／sword中尺度construction；建立六類Material-ID planning、DCC captures、reports、manifests與ZIP。
  - Out／Deferred：Final Texture、Final UV、Final Skinning、Animation Polish、正式LOD／impostor、FBX export、production shader、Runtime Prefab replacement、Unity import／captures／tests／build／profile、Phase 04、Golden Sample lock與Production Ready。
- Changed Files／Assets：
  - 新增`ArtSource/Units/Infantry/CHR_Infantry_A/v004/`：`CHR_Infantry_A_v004.blend`、build／render／comparison／finalize scripts、README與`P03_BUILD_RESULT.json`；final source SHA-256 `E1B6B3DFF07258184484186E5AEF8A380457A04E7899107D77908ECABCAA0046`。
  - 新增`docs/ArtProduction/ReviewPackages/Infantry_Phase03_SecondaryForms_Review_v001/`：60 files／21,391,097 bytes；9 top-level Markdown、32 PNG、v004 source＋scripts、4 task snapshots、L1 reference與7 manifest/checksum files。
  - 新增同名ZIP：21,253,238 bytes／SHA-256 `18EBD4EECB1C4A59396FF1DAE0D7E16C7D677C7F419758187AB6EACF174E7523`。
  - 更新Production Spec 08／09／16／17／99並重建`RTS_Asset_Production_Spec_v1.zip`：21 entries／62,466 bytes／SHA-256 `2229D69E25F8108EF369DE1643F1FFEAE17B4862A281DDFEF990B5EC8C4B5E1C`。
- Behavior Before／After：
  - Before：P02R1有可讀的Primary Forms，但equipment construction仍是clean blockout／large armor masses；沒有可信shield back、belt attachment、boot panels或material-region plan。
  - After：v004保留1.824 m silhouette並加入42個logical Secondary objects；chest有row divisions／side returns，shoulder有anchor／under-plate，helmet／scarf／waist／boots有構造過渡，shield具front／back logic，sword具blade spine／grip wraps；仍維持WIP review source。
  - Visual QA三輪：首輪發現劍脊未沿斜刀、眉帶像visor、chest／boot凸出過強；第二輪縮減形體並重建toe panel；第三輪將blade spine對齊既有短劍並重生全部captures。
- Architecture／API／Data：
  - Architecture：Definition／Runtime／View、Player／AI Command與gameplay authority完全未動；v004只存在ArtSource／ReviewPackages，不參與runtime truth。
  - API：N/A；C# public API、Command／Event／Query、Animator parameters、`AttackImpact`、socket／anchor name與Prefab ID無變更。
  - Data：scene metadata新增`SourceVersion=CHR_Infantry_A_v004`、`SourceBaseline`／SHA、`ReviewStatus=READY FOR PHASE03 REVIEW`及明確deferred flags；6個`MATID_*`為preview-only，不是final material contract。
- Tests／Validation：
  - Deterministic Blender build：1.824010849 m、106 meshes、17,193 vertices、33,898 triangles、6 material IDs、1 armature／23 bones、10 empties、0 Actions。
  - Geometry audit：non-manifold 0、boundary 0、loose 0、zero-area 0；no generic names；collections為`CHR_Infantry_A_v004`、`GEO_BODY`、`GEO_ARMOR`、`GEO_CLOTH`、`GEO_WEAPONS`、`RIG`、`REVIEW`。
  - Visual evidence：32 PNG＝Clay 5、MaterialID 2、Silhouette 4、Wireframe 3、Detail 7、Comparison 5、ScreenSize 6；final clay／MaterialID／shield back／L1／P02R1 comparisons已人工檢視。
  - Review ZIP：folder 60 files＝ZIP 60 entries；missing 0、extra 0、hash mismatch 0；source與package blend copies須在結案read-only reopen再次確認。
  - Unity：`NOT RUN / MANUAL REQUIRED`；提供隔離Review Prefab與四距離capture步驟，未以DCC evidence冒充runtime acceptance。
- Acceptance：
  - New v004／P02R1 preserved／height／28K–38K budget／six Material IDs／required evidence／reports／ZIP：`PASS`（機器／檔案交付gate）。
  - 18-item Secondary Forms visual reviewer gate：`PARTIAL / REVIEW PENDING`；所有row維持`PENDING`，沒有agent-issued Phase 03 PASS。
  - Unity RTS preview：`NOT RUN / MANUAL REQUIRED`；Runtime Prefab replacement：`NOT AUTHORIZED / NOT DONE`。
- Completed：required reading、clean baseline lock、v004 deterministic build、三輪visual QA、32 captures、Material ID plan、9 reports、manifests、source folder、review folder、ZIP、Production Spec同步。
- Not Completed／Deferred：human Phase 03 decision；manual Unity review；Phase 04／final UV／texture／shader；final retopo／skin／deformation；animation／durable Actions；formal LOD／profile；provenance／license；Golden Sample lock與Production Ready。
- Known Issues／Risks：
  - v004仍是static A-Pose object parenting；shoulder／elbow／hip／knee、shield strap與sword grip尚未通過deformation review。
  - 33,898 tris位於recommended range上緣；沒有representative battle-count profiler evidence。
  - L1 rear attachment資訊有限；rear scarf與shield back construction含合理化推導，須由Art reviewer確認。
  - Final UV／maps／Team Color mask／shader／Actions／LOD／Unity evidence均不存在；provenance／distribution rights仍是P0。
- Git：開始時clean；結案為`main`／HEAD `0422d6b98907095ffe8b47a7673a90aa72c8bea0`，`git status --porcelain=v1 --untracked-files=all`為75 entries（7 modified／68 untracked；collapsed 10）；v001／v002／v003（含P02R1）、Unity `Assets/`、`Library/`、`Temp/`、`obj/` protected paths新增status entries為0。未commit、未push。
- Next：
  1. Art／Design／Technical Art檢視Detail、MaterialID、L1／P02R1 comparison與64 px evidence，填寫`06_Review_Checklist.md`。
  2. 在isolated Unity review path建立`PF_Unit_Infantry_v004_Review`並補Close／Medium／RTS Normal／Far；不得替換正式Prefab。
  3. 只有人工批准並明確授權後才進Phase 04；若`CHANGE REQUESTED`，建立focused revision，持續保留P02R1與v004。

## 2026-08-13 — Infantry Phase 02 Revision 01 Primary Forms Gate Fix

- Status：`READY FOR PHASE02 REVISION REVIEW`（Revision source、DCC evidence、required comparisons、review documents、manifests、資料夾、ZIP與逐檔驗證均完成；不自行宣告Revision PASS或進Phase 03）
- Goal：完整執行 `mission/Infantry_Remaster_Phase02_Revision01_Task.md` 的`CHANGE REQUESTED`；保留v002與v003 initial，以局部Primary Forms reshape／rebuild修正toy-like head／helmet／plume／arms／chest／waist／legs／wraps／boots及shield boss比例，建立`CHR_Infantry_A_v003_P02R1.blend`與`Infantry_Phase02_PrimaryForms_Revision01_Review.zip`。
- Baseline：
  - Branch／HEAD：`main`／`ec0560192863a763d6beb3be6b9c0c642b1d4137`；開始時`git status --short --untracked-files=all`為266 entries，包含前述Production Spec、review packages、mission move與v003初版，全部保留，未reset／cleanup。
  - Revision task：967 lines／13,508 bytes；SHA-256 `2D93F02F688CE6D158DB707CA42756A6780D7ADB8C927A17130D0F435A976B25`；review decision明確為`CHANGE REQUESTED`。
  - Protected v002：blend SHA-256 `5D9D93F9559D2A1608FB4B57A7BC0AC284C4F3ED99BA826F0A5E98E1D5F51632`；master FBX `16B698ACC8797C80E0153D51916A9825D26453CCFF6A7289C46F5EF99F846468`。
  - Protected v003 initial：`CHR_Infantry_A_v003.blend` SHA-256 `5F7C799C3B57A64B6E289A38FF2AA6E5509247C547FD4B2DE0EC47FF63AA4DBE`；1.830 m／28,138 tris／72 meshes。
  - L1 reference：`Unit_03_Infantry_L1_Concept_Final.png` SHA-256 `C6BD352DE06854F8CD59AC92C891E43A0D40070D8F1AB72613358845847F461F`。
- Scope In：
  - 完整讀取Revision 01、Phase 01 target、Phase 02 initial task／reports、AGENTS要求的00／03／04／09／27／DevelopmentProgress／current Phase，以及Production Spec character／silhouette／Golden Sample／audit／naming／Unity acceptance／master checklist／open issues。
  - 以Blender 5.2.0 LTS從exact-hash v003 initial建立versioned P02R1；保留armature、socket／anchor、materials、shield／sword主要尺寸與三層shoulder概念。
  - 移除六個貼附式face pieces，把brow／nose／cheek／jaw大面整合進單一Head mesh；縮窄helmet crown、提高rim face clearance並重建向後／側向curved plume。
  - UpperArm radial volume減13%；shoulder outer drop增加且採hard planar read；胸甲row加高重疊、降低gap、加入plate segmentation節奏並縮小center strap。
  - Front／rear waist cloth改成有厚度的tapered shaped hem；左右各以main＋overlap兩塊side plate取代單一box；thigh／calf增加plane方向，八個donut wrap改成每腿兩條broader tilted wrap；boots縮窄toe／ankle並加入內外側差異。
  - Shield boss直徑減12.5%，board/rim加入0.014 m內的subtle convex bow；sword主尺寸與readability保留，只收斂pommel球形。
  - 產生5 Clay、4 silhouette、3 wireframe、128／64／32 px、2 initial comparison、2 L1 comparison；建立reports、checklist、Open Issues、manifests、review folder與ZIP。
- Scope Out／Deferred：
  - 未做Phase 03 Secondary Forms／ornament、Final UV、Final Texture、Final Team Color Mask、Final Skinning、Animation Polish、正式LOD／impostor、FBX export、Unity import/capture/test/build、Runtime Prefab replacement、shader、gameplay/API或Content change。
  - 未覆寫、刪除或修改v001／v002／v003 initial及任何既有Blender／FBX／Texture／Material／Prefab／Animation／Shader／Scene／C#／`.meta` Production Asset；未commit、未push。
- Major Files／Assets：
  - `ArtSource/Units/Infantry/CHR_Infantry_A/v003/Source/CHR_Infantry_A_v003_P02R1.blend`：339,535 bytes；SHA-256 `6569A14825B53393B72306F79BF8B29F9DEA5C0FFAB8E30686E901D61964220F`。
  - 新增`revise_infantry_v003_p02r1.py`、`render_infantry_v003_p02r1_review.py`、`compose_p02r1_comparisons.ps1`、`finalize_p02r1_review_package.ps1`；更新v003 README並新增`Documentation/P02R1_BUILD_RESULT.json`。
  - 新增`docs/ArtProduction/ReviewPackages/Infantry_Phase02_PrimaryForms_Revision01_Review/`：44 files／13,694,385 bytes；含7 Markdown、19 PNG、revision blend/scripts、L1 reference、three task snapshots與7 manifest/checksum files。
  - 新增`Infantry_Phase02_PrimaryForms_Revision01_Review.zip`：13,558,770 bytes；SHA-256 `11A4B858BD7D035138F63B1D49AB03BB142DF1293C5C4C459518CF58F381191D`。
  - 同步Production Spec 08／09／16／17／99至P02R1 pending review；重建`RTS_Asset_Production_Spec_v1.zip`為62,269 bytes／SHA-256 `8EE7B95BCA702FD91632C692EDD0A09F895D5183C350F630B9089F131FC50195`。
- Behavior Before／After：
  - Before：v003 initial雖已脫離方塊Prototype，但Clay仍有球形helmet/head、貼附式face pieces、inflated upper arms、floating chest bars、box waist、四圈donut wraps、bell-like boots及偏大shield boss；人工決定為`CHANGE REQUESTED`。
  - After：P02R1保留成功的盾劍、三層肩甲與heavy-infantry mass，在25,106 tris內把face整合成單一連續Head、以helmet/rim/plume形成更接近L1的頭部輪廓、收斂arm／boot roundness、把chest轉為nested segmented lamellar mass、把waist與wraps改為overlap language並調整shield curvature/boss。
  - Visual QA共三輪：first build 25,394 tris；首次render發現face被scarf/helmet遮擋、wrap半埋、chest segmentation弱；第二輪修正clearance與row/wrap後仍認為三條wrap偏stacked；final改為每腿兩條broader tilted wraps，25,106 tris。
- Architecture／API／Data：
  - Architecture：Definition／Runtime／View、Player／AI Command與gameplay authority完全不變；P02R1仍是ArtSource DCC review candidate，不參與runtime。
  - API：N/A；C# public API、Command／Event／Query、Animator parameter、`AttackImpact`、Asset／Prefab ID、socket／anchor name均無變更。
  - Data：scene `SourceVersion=CHR_Infantry_A_v003_P02R1`、`ProductionStatus=WIP_MODEL`、`ReviewStatus=READY FOR PHASE02 REVISION REVIEW`；保留6 review-only materials、1 armature／23 bones、10 empties、0 Actions。沒有save schema、Content Pack、Prefab YAML或Unity importer變更。
- Tests／Validation：
  - Deterministic revision build：input hash gate通過；final 1.824011 m、64 meshes、12,671 vertices、25,106 triangles、6 materials、1 armature／23 bones、10 empties／0 Actions。
  - Geometry audit：non-manifold 0、boundary 0、loose 0、zero-area faces 0；required collections只有`CHR_Infantry_A_v003`／`GEO_BODY`／`GEO_ARMOR`／`GEO_WEAPON`／`RIG`／`REVIEW`。
  - Visual evidence：19 PNG＝Clay 5、Silhouette 4、Wireframe 3、ScreenSize 3、Comparison 4；final front／left／back／3Q、64／32 px、initial comparison與L1 comparison已人工檢視。
  - Review ZIP：folder 44 files＝ZIP 44 entries；top-level只含`Infantry_Phase02_PrimaryForms_Revision01_Review/`；missing 0、extra 0、hash mismatch 0。
  - Production Spec ZIP：folder 21 files＝ZIP 21 entries；missing 0、extra 0、hash mismatch 0。
  - Unity Editor／EditMode／PlayMode／Builder／Windows Build／Profiler：`NOT RUN`，因Revision禁止修改正式Runtime Prefab；歷史runtime結果未冒充本次通過。
- Acceptance：
  - Version safety／v002＋v003 initial unchanged：`PASS` — exact hashes與protected-path Git scan提供證據。
  - Required new P02R1 source／24K–30K／1.80–1.85 m／evidence／package／ZIP：`PASS` — machine-readable metrics與ZIP verification完成。
  - 16-item visual Revision gate：`PARTIAL / REVIEW PENDING` — agent已完成對應geometry與evidence，但只由human reviewer決定是否通過。
  - Phase 03 authorization／Production Ready：`NOT RUN / NOT AUTHORIZED`。
- Completed：required reading、baseline hash lock、P02R1 deterministic source、三輪visual QA、required captures/comparisons、five required reports＋checklist/index、manifests、folder、ZIP、Production Spec同步與hash verification。
- Not Completed／Deferred：人工Revision decision；Phase 03；final topology／UV／texture／team-color shader；skin／deformation／equipment binding；animation／durable Actions；formal LOD；Unity integration/captures/tests/build/profile；provenance／license；Golden Sample lock與Production Ready。
- Known Issues／Risks：
  - P02R1仍是static object-parent A-pose；shoulder/scarf/waist/shield deformation clearance尚未驗證。
  - Chest plate segmentation與兩條calf wraps仍是Primary Forms簡化，不應被解讀為final lamellar／cloth topology。
  - Face只做大型連續planes，沒有eyes／lips／facial rig；此gate只審proportion與continuity。
  - No final UV／texture／team mask／shader／Actions／LOD／Unity capture；DCC evidence不能取代runtime acceptance。
  - Upstream provenance／commercial distribution rights仍未完整，會阻擋Production Ready。
- Git：Branch `main`／HEAD `ec0560192863a763d6beb3be6b9c0c642b1d4137`；未commit、未push。開始前 `git status --porcelain=v1 --untracked-files=all` 為266 entries，結案為317 entries（collapsed status 6）；v001／v002、Unity Infantry與`Library/`／`Temp/`／`obj/` protected paths本任務新增status entries為0。既有dirty worktree變更均保留。
- Next（排序）：
  1. Art／Design／Technical Art檢視四張Comparison、五張Clay、silhouette與64 px，填寫`04_Review_Checklist.md`。
  2. 若`APPROVE`／`CONDITIONAL APPROVE`且明確授權，才另開Phase 03 Secondary Forms；若`CHANGE REQUESTED`，只建立focused P02R2，不覆寫initial或P02R1。
  3. 後續依獨立gate處理topology／UV／texture／skin／animation／LOD／Unity integration與provenance，不把它們混入本Revision。

## 2026-08-13 — Infantry Phase 02 Primary Forms v003 與 Review Package

- Status：`READY FOR REVIEW`（建模、DCC evidence、review documents、manifests、原始資料夾、ZIP與逐檔驗證已完成；依任務要求未自行宣告Phase 02 PASS）
- Goal：完整執行 `mission/Infantry_Remaster_Phase02_PrimaryForms_Task.md`；保留`CHR_Infantry_A_v002`，建立新的`CHR_Infantry_A_v003`，只處理Primary Forms／Silhouette／Major Geometry，產生`Infantry_Phase02_PrimaryForms_Review_v001/`與同名ZIP，不修改正式Runtime Prefab、不commit、不push。
- Baseline：
  - Branch／HEAD：`main`／`ec0560192863a763d6beb3be6b9c0c642b1d4137`；開始時worktree已有前述docs packages、ZIP、mission move與progress等216 entries，全部保留，未reset／cleanup。
  - Phase 02 task SHA-256：`3229829154D75BA2ACA6C7BF18C7DA9C7EB88EE0911A3DFEA5E10B0ED8609B02`。
  - Protected v002 source：`CHR_Infantry_A_v002.blend` SHA-256 `5D9D93F9559D2A1608FB4B57A7BC0AC284C4F3ED99BA826F0A5E98E1D5F51632`；master FBX SHA-256 `16B698ACC8797C80E0153D51916A9825D26453CCFF6A7289C46F5EF99F846468`。
  - Stable contracts：`unit.infantry`、`PF_Unit_Infantry`、1.83 m target、Humanoid／23 bones、Root Motion Off、sockets／anchors、Animator parameters與`AttackImpact`；本階段不改runtime binding。
- Scope In：
  - 完整讀取Phase 02 task、Phase 01 target、AGENTS要求的00／03／04／09／27／DevelopmentProgress／current Phase／affected API docs，以及RTS production spec的Primary Forms、geometry、rig、naming、Unity acceptance、Golden Sample、audit、checklist、backlog與Open Issues。
  - 以Blender 5.2.0 LTS從唯讀v002 contract baseline建立全新的versioned v003 source；重建body/head、helmet、三層shoulders、chest／lamellar bands、waist、legs、rounded boots、shield與tapered sword。
  - 保留armature與empty/socket/anchor contract；採static A-pose／object-parent review binding，並把equipment planned attachment記為metadata，不冒充final skinning。
  - 產生六向Clay、四向black silhouette、三向wireframe、128／64／32 px silhouette與兩張v002 comparison；輸出object／bone／geometry／screenshot／file manifests及SHA256SUMS。
  - 將使用者直接要求執行Phase 02視為Phase 01 target批准，更新15-item approval record、entry gate及受影響的production-spec索引；Phase 02 reviewer decision仍維持pending。
- Scope Out／Deferred：
  - 未做Final UV、Final Texture、Final Team Color Mask、production shader、Final Skinning、Animation Polish、正式LOD／impostor、FBX export、Unity import/capture/test/build、Runtime Prefab replacement或gameplay/API change。
  - 未修改／刪除／移動v001／v002、任何現有FBX／Texture／Material／Prefab／Animation／Shader／Scene／C#／`.meta` Production Asset；未commit、未push。
  - 未進Phase 03，未鎖Golden Sample，未宣告Production Ready或Phase 02 PASS。
- Major Files／Assets：
  - 新增`ArtSource/Units/Infantry/CHR_Infantry_A/v003/`：`CHR_Infantry_A_v003.blend`、README、build／render／comparison／finalize scripts與`BUILD_RESULT.json`；source SHA-256 `5F7C799C3B57A64B6E289A38FF2AA6E5509247C547FD4B2DE0EC47FF63AA4DBE`。
  - 新增`docs/ArtProduction/ReviewPackages/Infantry_Phase02_PrimaryForms_Review_v001/`：41 files／9,990,770 bytes；7 top-level Markdown reports、18 PNG、Blender source copy與scripts、2 task snapshots、comparison provenance note及7 manifest/checksum files。
  - 新增`Infantry_Phase02_PrimaryForms_Review_v001.zip`：9,871,667 bytes；SHA-256 `F55BB2CDA64EBDDA4CF47D6B706BE7083AECD1DF71089AC7ECD144B47E6546CA`。
  - 更新Phase 01 README／Approval Checklist／Entry Gate及Production Spec 08／09／16／17／99；重建`RTS_Asset_Production_Spec_v1.zip`為61,993 bytes／SHA-256 `3AF26269992C2981EBD0365E906B38FFAC4CA06827DE84F97E71A04ED4510FE9`。
- Behavior Before／After：
  - Before：v002是1.83 m、4,376-triangle playable prototype；Phase 01 target已完成但待使用者批准，沒有v003 actual geometry或Phase 02 exact-geometry review evidence。
  - After：Phase 01 target獲授權；v003以新的curved／custom geometry取代方塊形體，具可辨識頭臉、曲面helmet、wide layered shoulders、shaped chest、tapered waist、leg rhythm、rounded boots、constructed shield與tapered sword；v002仍是immutable baseline。
  - Initial bone-parent attempt因bone-space transform造成2.179808 m outlier而在save前停止；改為static review parenting後先得30,946 tris，再降低shoulder／wrap密度。視覺QA拒絕方塊boot與錯誤comparison framing；完成rounded boot rebuild及immutable-capture comparison後，final為28,138 tris／1.830 m。
- Architecture／API／Data：
  - Architecture：Definition／Runtime／View與Player／AI Command邊界完全未動；v003是DCC review candidate，不參與runtime authority。
  - API：C# public API、Command／Event／Query、Animator parameters、`AttackImpact`、socket／anchor name、Asset／Prefab ID均無變更。
  - Data：新blend含scene properties `AssetId=unit.infantry`、`SourceVersion=CHR_Infantry_A_v003`、`ProductionStatus=WIP_MODEL`；72 meshes分入`GEO_BODY`／`GEO_ARMOR`／`GEO_WEAPON`，並保留`RIG`／`REVIEW` collections。六個材質僅為review ID colors，不含final texture／UV contract。
- Tests／Validation：
  - Deterministic Blender build：final save成功；1.830000 m、72 meshes、14,211 vertices、28,138 triangles、6 materials、1 armature／23 bones／0 Actions。
  - Blender read-only reopen package copy：`CHR_Infantry_A_v003 / WIP_MODEL / 72 / 14211 / 28138 / 1 armature / 23 bones / 0 actions`；source與package copy SHA-256完全相同。
  - Geometry audit：non-manifold 0、boundary 0、loose 0、zero-area faces 0；object／bone manifests完整。此結果只代表generated Primary Forms封閉性，不代表final deformation topology。
  - Visual evidence：18 PNG＝Clay 6、Silhouette 4、Wireframe 3、ScreenSize 3、Comparison 2；final front／3Q／side／wireframe／32 px及v002 comparison均已人工檢視，boot與comparison問題修正後重新產出。
  - Review structure：required files missing 0；7/7 Markdown各一個H1、unmatched code fences 0；Phase 01 checklist 15/15 `APPROVED`；Python build／render scripts `py_compile` exit 0。
  - Review ZIP：folder 41 files＝ZIP 41 entries；top-level只含`Infantry_Phase02_PrimaryForms_Review_v001/`；missing 0、extra 0、hash mismatch 0。
  - Production Spec ZIP：folder 21 files＝ZIP 21 entries；missing 0、extra 0、hash mismatch 0。
  - Protected baseline：v002 blend與master FBX final SHA-256等於開始值；v001／v002及Unity Infantry protected paths的`git status` entries為0。
  - Unity Editor／EditMode／PlayMode／Builder／Windows Build／Profiler：`NOT RUN`，因本階段禁止修改正式Runtime Prefab；歷史runtime結果未冒充本次測試。
- Acceptance／Review Readiness：
  - `[READY FOR REVIEW]` 新v003存在且v002未覆寫；身高、triangle range、collection／object naming與contract preservation有machine-readable evidence。
  - `[READY FOR REVIEW]` Primary Forms不是單純Subdivision／Bevel；helmet、shoulders、chest、waist、leg／boots、shield、sword均是實際重建geometry。
  - `[READY FOR REVIEW]` Clay／silhouette／wireframe／screen-size／comparison與required reports／Open Issues／checklist均已打包。
  - `[PENDING REVIEW]` 12-item Phase 02 reviewer checklist與Reviewer Record；沒有Phase 02 PASS、Phase 03 authorization或Runtime replacement。
- Completed：required reading、Phase 01 authorization record、v002 safety lock、v003 Primary Forms build、visual QA修正、DCC evidence、source documentation、review folder、manifests、ZIP、production-spec sync與逐檔hash verification。
- Not Completed／Deferred：人工Phase 02 decision；Secondary Forms；final topology／UV／texture／Team Color shader；skin／deformation／equipment bind；animation／Actions；formal LOD；Unity integration／captures／tests／build／profile；provenance／license；Golden Sample lock與Production Ready。
- Known Issues／Risks：
  - v003為static review binding，未做weights；shoulder layers、shield grip與arm clearance必須在後續deformation phase驗證。
  - 28,138 tris在20K–30K內但高於preferred 24K–27K；是否保留由reviewer決定，不在Phase 02自行優化成formal LOD。
  - zero saved Actions、no final UV／texture／team mask／shader／LOD及no Unity capture都是明確deferred，不可把DCC render視為runtime acceptance。
  - v001／v002 upstream provenance與commercial distribution evidence仍不完整，會阻擋Production Ready。
- Git：`main`／HEAD `ec0560192863a763d6beb3be6b9c0c642b1d4137`；未commit、未push。Final `git status --short --untracked-files=all`為265 entries，包含開始前既有變更、新v003與review package；protected v001／v002／Unity Infantry paths為0 entries。
- Next（排序）：
  1. Art／Design／Technical Art檢視comparison、六向Clay、silhouette、128／64／32 px與wireframe，填寫`05_Review_Checklist.md`為`APPROVE`或`CHANGE REQUESTED`。
  2. 若change requested，只修改new versioned v003 Primary Forms並重新產生同一review revision或明確升版；持續保護v002與Runtime Prefab。
  3. 只有Phase 02人工批准後才另開Phase 03 Secondary Forms；texture／skin／animation／LOD／Unity integration仍各自進gate。

## 2026-08-13 — Infantry Phase 01 Production L2 Target 稽核與使用者批准閘門

- Status：Completed（agent-side稽核先以`READY FOR USER APPROVAL`交付；使用者後續明確要求直接執行Phase 02，因此15項target於2026-08-13獲批准並解鎖versioned v003工作）
- Goal：完整執行 `mission/Infantry_Phase01_Production_L2_Remaster_Target.md` 的Phase 01範圍，把既有L1與construction target轉成可審核、可追蹤、可交給後續v003 Primary Forms工作的正式記錄；不自行批准、不生成另一套AI turnaround、不覆寫v002或current Unity runtime。
- Baseline：
  - Branch／HEAD：`main`／`ec0560192863a763d6beb3be6b9c0c642b1d4137`。
  - 開始時worktree已有前兩項documentation packages、ZIP、`DevelopmentProgress.md`與使用者移入`mission/`的deleted＋untracked狀態；全部保留，沒有reset／cleanup／commit／push。
  - Task source：1,157 lines；SHA-256 `DBD67A4021ED8FD56AD7F4F2B197BB430185B1817647445460753DCDB5316540`；Status原為`READY FOR USER APPROVAL → PHASE 02`。
  - Current baseline：`CHR_Infantry_A_v002.blend`、`SK_Infantry_A_v002.fbx`、`PF_Unit_Infantry`；1.83 m、LOD0 4,376 tris、23-bone Humanoid、five runtime clips、current slot-based Team Color。
  - Core protected production extensions（Infantry v001＋v002 ArtSource及Unity Shared Art）：55 files／15,835,267 bytes；開始combined manifest SHA-256 `B381E4C720ECFF02AB4BCB2C71DAE5608C12274C2288CC0705B73E913D47AC0F`。
- Scope In：
  - 完整讀取AGENTS必讀00／03／04／09／27、DevelopmentProgress、目前34–38 Phase docs、affected API／ArtSpecs、RTS production 02–09／13／15–17／99及Review Package core reports。
  - 對15個Phase 01 decisions做target-to-repository conformance audit；釐清asset-specific LOD0 20K–30K與legacy 2.5K–6K、formal L2 orthographic與Phase 02 actual-geometry Clay的優先序。
  - 建立Phase 01 README、Audit、Approval Checklist、Evidence Index、Open Issues與Phase 02 Entry Gate；同步Infantry ArtSpec、Golden Sample、Remaster Audit、Master Checklist、Backlog及Open Issues。
  - 因production-spec內容被同步，重新產生並逐檔驗證`RTS_Asset_Production_Spec_v1.zip`。
- Scope Out／Deferred：
  - 不新增、修改、刪除、移動或重新儲存任何Blender、FBX、GLB、Texture、Material、Prefab、Animation、Animator、Shader、Scene、C#、`.meta`或current production runtime asset。
  - 不建立`CHR_Infantry_A_v003.blend`、不建Phase 02 executable task、不做Primary Forms、texture、UV、rig／skin、animation、LOD chain、Unity import/capture/test/build或Profiler。
  - 不自行代表user／Art／Technical Art簽核；不把`READY`、`PENDING`、`NOT RUN`或`CANNOT VERIFY`寫成PASS；不commit、不push。
- Major Files／Assets：
  - 新增`docs/ArtProduction/Infantry_Remaster/Phase01_Production_L2_v001/`：6 Markdown files／18,674 bytes，含README、Conformance Audit、15-item Approval Checklist、Evidence Index、11-item Open Issues與Phase 02 Entry Gate。
  - 更新`docs/ArtSpecs/Unit_03_步兵.md`，明確標出v003 target與legacy Prototype triangle budget的適用範圍。
  - 更新`docs/ArtProduction/RTS_Asset_Production_Spec_v1/08_...`、`09_...`、`16_...`、`17_...`、`99_...`，登記Phase 01 target已存在但尚未批准，Golden Sample gate仍unchecked。
  - 重新產生`docs/ArtProduction/RTS_Asset_Production_Spec_v1.zip`：61,730 bytes；SHA-256 `2AB133E103413743D0000AC62E119D9512A151A2BE172674C87574C1E649CE8D`。
  - 更新本`DevelopmentProgress.md`；Task source、Review Package與production assets未改動。
- Behavior Before／After：
  - Before：Phase 01 task本身有完整造型／施工值，但沒有repository正式批准記錄、逐項conformance、證據索引、Phase-specific open issues或禁止提前進Phase 02的machine-reviewable boundary；Production Spec仍寫Infantry formal L2完全不存在。
  - After：初始交付時15項全部為`READY / PENDING`；使用者後續以直接執行Phase 02的明確指示批准同一target revision。Phase 02 preconditions、required evidence與must-not-do已獨立成檔，且Phase 02本身仍需另行人工審核。
  - Before：legacy Unit 03 LOD0 2.5K–6K可能與new target 20K–30K互相覆蓋；After：v003採20K–30K／preferred 24K–27K，legacy值只屬v002 Prototype／較低LOD參考。
- Architecture／API／Data：
  - Architecture：runtime無變更；規格繼續鎖定Definition／Runtime／View分離，Animator／Animation Event只做presentation，UI／Prefab不擁有gameplay truth。
  - API：N/A；C# public API、Command／Event／Query、Animator contract、socket、`AttackImpact`與Prefab ID都未修改。Phase 02必須preserve既有contracts。
  - Data：ContentPack、Definition、Prefab YAML、FBX importer、save schema與production source均無變更。新增Markdown只作review/governance，不由Unity runtime載入。
- Tests／Validation：
  - Phase 01 package structure：6/6 Markdown存在且non-empty；18,674 bytes；6/6 exactly one H1；0 unmatched code fences；Approval table 15/15 rows，PASS。
  - Conformance matrix：height、proportion、armored width、shield、sword、team color、LOD0、skinning、A-Pose、readability、runtime contracts及versioning均有target/baseline/result；0 unresolved core modelling value，PASS for readiness。
  - Production Spec ZIP：folder 21 files／116,409 bytes＝ZIP 21 entries；0 missing、0 extra、0 hash mismatch；61,730-byte ZIP SHA-256如上，PASS。
  - Core production asset integrity：55 files／15,835,267 bytes；final combined manifest SHA-256 `B381E4C720ECFF02AB4BCB2C71DAE5608C12274C2288CC0705B73E913D47AC0F`與baseline完全一致；protected ArtSource／Unity Infantry paths Git changes 0，PASS。
  - Task source integrity：SHA-256仍為`DBD67A4021ED8FD56AD7F4F2B197BB430185B1817647445460753DCDB5316540`；Review Package ZIP仍為17,271,921 bytes／SHA-256 `BEF645BFAC7BAC190744CC4858559FF2541C9C94766C93FE56D6F99E31DA8F20`，兩者未被本次改動，PASS。
  - `git diff --check`：PASS；Phase 01 required files／H1／fences／15-row checklist／reference path scan均PASS。Final `git status --short --untracked-files=all`為215 entries，包含開始前181-entry Review Package、21-file Production Spec／ZIP／mission move，以及本次6-file Phase 01 folder、ArtSpec／Production Spec sync與progress；protected production paths 0 entries。
  - Unity Editor／EditMode／PlayMode／DCC save／Windows Build／Profiler：`NOT RUN`；docs-only Phase 01不把歷史runtime結果冒充本次通過。
- Acceptance：
  - `[PASS]` Existing L1＋asset-specific construction target已形成可執行Phase 01 reference；沒有生成另一套AI turnaround。
  - `[PASS]` 15項approval decisions皆有單一locked target與evidence／standard對照，無需modeler自行猜值。
  - `[PASS]` v002 protected baseline、stable IDs、scale、skeleton、sockets、Animator、events、Root Motion與anchors列入Preserve／Must Not Do。
  - `[PASS]` Phase 02 scope只限new versioned v003 Primary Forms＋Clay／Silhouette／Wireframe／128-64-32 px evidence；final texture等排除。
  - `[PASS]` Golden Sample／Master Checklist／Backlog／Open Issues沒有把target存在誤記為approved或Production Ready。
  - `[APPROVED BY USER DIRECTION]` 15項使用者決策與Approval Record已於後續Phase 02直接執行指示完成；此批准不等於Phase 02 PASS。
- Completed：required reading、repository/evidence cross-audit、6-file Phase 01 record、15-item approval gate、11-item open issues、Phase 02 boundary、affected spec synchronization、production-spec ZIP regeneration/validation及後續使用者批准紀錄。
- Not Completed／Deferred：Phase 02人工reviewer decision；source provenance；durable Actions；production shader/material；topology/UV/texture/skin/animation；LOD1–3/impostor；Unity captures/tests/build/profile；Golden Sample lock與Production Ready。
- Known Issues／Risks：
  - 原`INF-P01-001` user approval blocker已由使用者直接授權Phase 02而關閉；目前視覺gate是Phase 02 reviewer decision。
  - Asset-specific L2採「existing L1＋construction spec＋Phase 02 actual-geometry orthographic Clay」路徑；若Art／Technical Art要求Phase 02前先交獨立2D orthographic，需把它寫成change request，不可由modeler猜測。
  - Provenance／license、team-color shader、source Actions、完整LOD與target-hardware profile仍會阻擋Production Ready；批准visual direction不會自動關閉這些issue。
- Git：`main`／HEAD `ec0560192863a763d6beb3be6b9c0c642b1d4137`；未commit、未push。Final status 215 entries；開始前production-spec/review-package/mission changes全部保留，本次只新增／同步docs與重建production-spec ZIP；protected Infantry production paths 0 changes，`git diff --check` PASS。
- Next（排序）：
  1. 審查Phase 02 Primary Forms package並明確批准或提出change request。
  2. Phase 02批准後才拆分Secondary Forms、topology／UV／texture/material、rig/skin、animation、LOD與Unity integration tasks；provenance／license平行處理，但不得省略。

## 2026-08-13 — Infantry Remaster Review 資料收集、唯讀 DCC 證據與 ZIP

- Status：Completed（任務限定的 scan／read／copy／review-data generation／documentation／package／ZIP verification 全部完成；沒有執行 remaster、production asset 修改、Unity reimport、commit 或 push）
- Goal：完整執行 `mission/Infantry_Remaster_Review_Data_Collection_Task.md`，收集足以讓後續 Visual／Technical Art Reviewer 判斷 Infantry 應 Preserve、Modify、Partial Rebuild 或 Rebuild 的 L1、L2 現況、current L3 source/runtime、Unity、material/texture、rig、animation/event、team color、LOD、script、specification 與 visual evidence，保留資料夾並建立可驗證 ZIP。
- Baseline：
  - Branch／HEAD：`main`／`ec0560192863a763d6beb3be6b9c0c642b1d4137`。
  - 開始時 worktree 已包含上一項 `RTS_Asset_Production_Spec_v1` 21 documents／ZIP／本進度檔變更，以及使用者把根目錄 `AegisRTS_Codex_3D_Asset_Production_Spec_Task.md` 移入 `mission/` 的 deleted＋untracked 狀態；這些 pre-existing changes 全部保留，沒有清理、還原或混作本次 production asset 修改。
  - Current chain：`unit.infantry` → `PF_Unit_Infantry` → `SK_Infantry_A_v002.fbx`；current source `CHR_Infantry_A_v002.blend`，legacy v001 GLBs 仍存在但 current Prefab 不引用。
  - 本次受保護範圍為 `ArtSource/Units/Infantry`、Infantry Unity Shared Art、`Assets/AegisRTS/Editor`、`Assets/AegisRTS/Demo/PlayablePrototype` 中 production-like extensions：123 files／15,899,635 bytes，combined manifest SHA-256 `E1FE28875FBC49CFA206F5230BD903025970FE3A963D3E9603D29488104A4A17`。
- Scope In：
  - 完整讀取 collection task、AGENTS、00／03／04／09／27、目前 phase、latest progress、affected presentation/API/art docs；全 repository 搜尋 Infantry／步兵／CHR／Sword／Shield／AttackImpact 及要求副檔名。
  - 辨識 current vs legacy；複製 current `.blend`、source/runtime FBX、legacy GLB、Prefab/meta、model/animation importer metas、Controller、materials、textures、direct scripts、source records、legacy/new specs、existing previews 與 historical Unity validation PNGs。
  - 只以 Blender 5.2 background mode 開啟 Review Package 內 `.blend` 副本，輸出 object/bone/mesh/weights/actions technical data，並在 memory 中建立 neutral lights/camera，產生 actual-material、Clay、wireframe review images；從未呼叫 save。
  - 建立 README、00–08 reports、copy/file/texture/visual/object/bone manifests、source/copy checksum evidence、SHA256SUMS、原始資料夾和 ZIP。
- Scope Out／Deferred：
  - 不修改、刪除、移動、重新匯入、重建或覆寫任何既有 `.blend`、FBX、GLB、Texture、Material、Prefab、Animation、Animator、Shader、Scene、C# 或 `.meta` production asset。
  - 不建立新的 L2 art、不 retopology、不 bake/repaint texture、不改 shader/team color、不調 LOD/rig/animation/importer、不執行 Unity capture／tests／build。
  - 不把整個 Unity project、Library、Temp、Logs、obj、`.git`、Packages cache 或 build binaries 放入 ZIP；不 commit、不 push。
- Major Files／Assets：
  - 新增 `docs/ArtProduction/ReviewPackages/Infantry_Remaster_Review_Package_v001/`：180 files／24,844,001 bytes；含 README + 00–08 共 10 top-level reports、148 copied-source rows、40 indexed visual evidence images、45 specification/source-record files、12 FBX、8 GLB、1 `.blend`、review tool／manifests。
  - 新增 `docs/ArtProduction/ReviewPackages/Infantry_Remaster_Review_Package_v001.zip`：17,271,921 bytes；SHA-256 `BEF645BFAC7BAC190744CC4858559FF2541C9C94766C93FE56D6F99E31DA8F20`。
  - Review-generated evidence：12 DCC PNGs（6 actual material、4 Clay、2 wireframe）、`Blender_Technical_Summary.json`、object/bone CSV、texture/visual CSV、148-row source-copy integrity CSV、102-entry SHA256SUMS。
  - 更新本 `DevelopmentProgress.md`；Production assets 只被讀取／複製，沒有寫回。
- Behavior Before／After：
  - Before：Infantry 證據分散於 v001/v002 ArtSource、Unity Shared Art、ArtSpecs、new production specs 與 external validation folder，current/legacy 和 `L2` 語意容易混淆；After：單一 package 以 status、original/copy path、hash 和 cross-reference 整理，可交給 reviewer 離線判斷且不含整個 Unity project。
  - Before：source reports 描述 Actions／frame ranges，但缺少本次 reopening verification；After：read-only Blender 證明 0 saved Actions，Unity `.meta`／builder 證明實際 Idle 0–89、Move 0–24、Attack 0–26、Hit 0–9、Death 0–38，並標出舊 import doc 的 Idle／Attack／Hit ranges 漂移。
  - Before：既有影像混合 mathematical previews 與 external Unity captures；After：明確分為 legacy mathematical、historical Unity、current copied-DCC generated evidence，缺 standardized current Unity set 的項目仍標 `MANUAL CAPTURE REQUIRED`。
- Architecture／API／Data：
  - Architecture：N/A for runtime mutation；文件確認 Definition／Runtime／View 分離、Player／AI shared Command、Animation Event 只做 presentation timing，Prefab 不擁有 gameplay truth。
  - API：N/A；沒有新增、修改或移除 C# public API。`PrototypeUnitArtView`／`PrototypeUnitAnimatorView` 等只被複製供 review。
  - Data：N/A for production data；ContentPack、Prefab YAML、import `.meta`、material/texture/FBX/BLEND 都保持原檔。Review CSV/JSON 是派生索引，不會被 Unity runtime 載入。
- Tests／Validation：
  - Source-copy integrity：148/148 original/copy pairs SHA-256 match，0 missing／0 mismatch，PASS（`Manifests/Source_Copy_Integrity.csv`）。
  - Blender attempt 1：opened package copy successfully；render step FAIL because Blender 5.2 exposes engine enum `BLENDER_EEVEE`, not `BLENDER_EEVEE_NEXT`。只修正 package-owned review script，未改 production file；沒有把此輪誤記通過。
  - Blender final：Blender 5.2.0 LTS read-only run PASS；12/12 renders saved，23 objects、12 meshes、1 armature、23 bones、3 materials、0 Actions；all-LOD/equipment 3,635 vertices／6,430 triangles，LOD0 2,400 vertices／4,376 triangles，body 100% weighted／max influences 1，角色 Z height 1.8300 m。
  - Package structure：180 files、50 Markdown；10/10 required top reports present、12/12 core category folders present、0 empty files、0 unmatched Markdown fences；current BLEND/runtime FBX/Prefab/Unity info/Specifications all present，PASS。
  - Visual/manifest：40 review images indexed；4/4 textures have width/height/channels/import summary；`File_Manifest.csv` 179 payload rows and excludes only its own self-referential row；102 important binary/Unity/image SHA entries，PASS。
  - ZIP byte/hash validation：folder 180 files＝ZIP 180 entries；0 missing、0 extra、0 hash mismatch、0 bad top-level path、0 required core missing；24,844,001 uncompressed bytes fully streamed，PASS。
  - Production integrity final：protected 123 files／15,899,635 bytes，combined manifest SHA-256 remains `E1FE28875FBC49CFA206F5230BD903025970FE3A963D3E9603D29488104A4A17`；protected Git path changes 0，PASS。
  - Unity Editor／EditMode／PlayMode／Builder／clean import／Windows build／Profiler：`NOT RUN`（explicit read/copy-only scope）；historical runtime results not claimed as current validation。
- Acceptance：
  - `[PASS]` 已掃描 Infantry 相關資料；已找到並收集 L1 final／alternate；已搜尋 L2 並明標 production L2 `NOT FOUND`。
  - `[PASS]` 已收集 Current L3 Blender；已收集 Current source/runtime master FBX及 legacy/input models；已收集 Unity Prefab／meta。
  - `[PASS]` 已整理 Model Import Settings；已收集兩個 current Materials／meta；已收集四張 current Textures／meta。
  - `[PASS]` 已整理 current material-slot＋MaterialPropertyBlock Team Color；已整理 23-bone Rig、Humanoid importer、body weights、sword／shield parent/socket。
  - `[PASS]` 已收集／整理五段 animation source/runtime FBX、Controller、import metadata；已確認 `AttackImpact` 位於 Unity ModelImporter event、frame 13／0.43333334 s、presentation-only receiver。
  - `[PASS]` 已收集 deterministic Blender build、Unity builder/view/binding/validator/test 等直接相關 Scripts；已收集 45 Specifications／source records。
  - `[PASS]` 已收集 6 legacy previews與16 historical Unity screenshots；在安全條件下由 package `.blend` 副本產生 12 review screenshots。
  - `[PASS]` 已建立 Asset Inventory、Blender Summary、Unity Summary、Texture Summary、Animation Summary、Visual Evidence Index、Missing Data、Source Spec Index、README。
  - `[PASS]` 已保留 `Infantry_Remaster_Review_Package_v001/`；已產生 ZIP；已逐檔讀取/hash 驗證 ZIP。
  - `[PASS]` Production Asset 前後 combined checksum 不變且 Git changes 0；沒有 git commit；沒有 git push。
  - `[MANUAL REQUIRED]` current standardized Unity Close／Medium／RTS Normal／Far、128／64／32 px、blue/red paired、LOD captures；task permits marking missing and continuing。
- Completed：Infantry current/legacy identification、148 source copies、review DCC metrics/renders、10 core reports、all manifests/checksums、folder/ZIP、exact integrity proof and missing-data handoff。
- Not Completed／Deferred：正式 L2、new Unity captures、durable source Actions、original v001 editable source/provenance、production material/team mask、multi-influence skinning、LOD3/impostor、animation polish、Unity revalidation/performance、Golden Sample/remaster implementation。
- Known Issues／Risks：
  - Critical：正式 L2 Production Character Sheet absent；0 saved Actions；original v001 commercial/distribution rights incomplete；standardized current Unity evidence missing。
  - Material：Base only connects BaseColor＋Normal，ORM/TeamColorMask unused；current team color is material slots, no custom mask shader。
  - Geometry/skin：prototype LOD0/1/2 = 4,376/1,512/542 triangles；body max influence 1；armour mostly fused into body meshes，sword/shield separate rigid objects but not standalone prefabs。
  - Evidence：external Unity PNGs are historical and lack immutable commit/camera/LOD manifest；new Blender renders are static LOD0 A-pose and cannot certify Unity or animated deformation。
  - Documentation drift：`UNITY_IMPORT_SETTINGS.md` 的 Idle/Attack/Hit ranges 與 actual `.meta`／build script 不一致；review package 如實保留，不自動更改來源。
- Git：`main`／HEAD `ec0560192863a763d6beb3be6b9c0c642b1d4137`；未 commit、未 push。最終預期 `git status --short --untracked-files=all` 207 entries，其中本 Review Package 181 entries（180 folder files + 1 ZIP）；其餘為開始前的 production-spec、mission move與已修改 progress。Production Asset paths 0 changes。
- Next（排序）：
  1. Reviewer 使用 package 的 L1、material／Clay／wireframe、historical Unity 和 technical summaries，先填人工 Preserve／Modify／Partial Rebuild／Rebuild decision；不要直接開始 mass remaster。
  2. Art／Technical Art 補正式 L2 Production Sheet，Producer／Legal 補 v001→v002 rights/provenance，Technical Art 決定 durable Action/source library。
  3. 從 exact candidate Unity revision 補 Close／Medium／31 m Normal／Far、128／64／32 px、blue/red、LOD／animation captures與 metadata。
  4. 完成 team-color shader/material、production topology/weight/LOD 策略的人工作業決策後，另立 versioned remaster 任務；保留 current source/runtime作 comparison/rollback。

## 2026-08-13 — RTS 3D Asset Production Specification v1、Golden Sample Audit 與封裝

- Status：Completed（本任務授權的 repository 全面唯讀盤點、21 份 Markdown 規格／稽核／清單文件、原始輸出資料夾、ZIP 與 ZIP 驗證均完成；Infantry／Archer 的 production remaster 與 Production Ready gate 明確未完成且未冒充通過）
- Goal：完整執行 `AegisRTS_Codex_3D_Asset_Production_Spec_Task.md`，把現有 Prototype ArtSpecs、Infantry、Archer、ContentPack、Unity runtime art 與 Blender source 整理為可重複使用的 L1→L4 production pipeline、Golden Sample gate、驗收清單、backlog、Agent guide、Open Issues 與可攜式 ZIP；保持所有既有 production asset 不變且不 push。
- Baseline：
  - Branch／HEAD：`main`／`ec0560192863a763d6beb3be6b9c0c642b1d4137`；開始時 `git status --short` clean。
  - Unity `6000.5.7f1`、URP `17.5.0`、glTFast `6.19.0`、AI Navigation `2.0.14`、Input System `1.20.0`；目前 Phase 是 PlayablePrototype_01，Infantry／Archer Prototype L3 可玩，正式 production art deferred。
  - Repository 盤點（排除 generated folders）副檔名基線：`.asset` 38、`.blend` 2、`.controller` 2、`.fbx` 26、`.json` 29、`.mat` 6、`.md` 98、`.meta` 391、`.png` 26、`.prefab` 7、`.txt` 5。
  - `ArtSource`：73 files／15,787,828 bytes；`Assets/AegisRTS/Content/Shared/Art`：86 files／9,886,843 bytes。受保護 production extensions 共 71 files／24,898,052 bytes，combined manifest SHA-256 `A00DB4E5A733D90B021CCCE3F2CBBB660798EDB70F4B6D2FE4215DFBD5461C05`。
  - Infantry v2 實況：LOD 4,376／1,512／542 triangles、Humanoid valid、23 bones、5 animation FBXs、1K prototype textures、Prefab／Controller／anchors 已整合；Archer v1：3,344／1,280／542 triangles、Humanoid valid、23 bones、5 animation FBXs、獨立 0.82 m Z+ arrow、Prefab／Controller／Projectile socket 已整合。
- Scope In：
  - 完整讀取任務文件、必讀架構／命名／進度／DoD、目前 phase、相關 API／ArtSpecs；掃描 source/runtime/content registration、檔案種類、材質引用、team-color path、LOD、rig、animation、building／hero placeholder 現況。
  - 以 Blender 5.2 唯讀開啟兩個 `.blend`，量測 objects／meshes／triangles／armature／bones／weights／Actions；不儲存檔案。
  - 建立 `docs/ArtProduction/RTS_Asset_Production_Spec_v1/` 的 README、01–18 核心文件、99 Open Issues 與額外 Legacy migration；建立並讀取驗證 `RTS_Asset_Production_Spec_v1.zip`。
  - 對 Golden Sample 現況做證據分級、remaster preserve／modify／partial-rebuild 決策、量產禁止、後續 unit／hero／special／building backlog 與未決資訊登錄。
- Scope Out／Deferred：
  - 不修改、刪除、移動、重新匯出或重新儲存任何 Unity、Blender、FBX、GLB、Texture、Material、Animation、Animator、Prefab、Scene 或其他 production asset。
  - 不實作 shader、重拓樸、重蒙皮、補動畫、L1/L2 美術稿、LOD3／Impostor、正式材質、Unity captures 或 production remaster。
  - 不升級 Unity packages、不修改 gameplay/API/data、不執行 Unity tests/build、不 commit、不 push。
- Major Files／Assets：
  - 新增 `docs/ArtProduction/RTS_Asset_Production_Spec_v1/README.md`、`01_Project_Asset_Audit.md` 至 `18_Agent_Execution_Guide.md`、`99_Open_Issues_and_Missing_Information.md` 與 `Legacy_Spec_Migration.md`，合計 21 Markdown files／114,787 bytes。
  - 新增 `docs/ArtProduction/RTS_Asset_Production_Spec_v1.zip`：60,914 bytes、SHA-256 `D5D1EF47E3C07771756768D18F45B6945E85013186B73BA3F4A7252ACF0749D7`。
  - 更新本 `DevelopmentProgress.md`；所有 `ArtSource/` 與 `Assets/AegisRTS/Content/Shared/Art/` 檔案保持原樣。
- Behavior Before／After：
  - Before：個別 ArtSpecs 可支援 Prototype L1/L3 與 Unity 整合，但 `L2` 語意、production quality、LOD3、texture packing、team color、rig/skin、Golden Sample、license、acceptance、backlog 與量產 gate 沒有一套整合規格。
  - After：有單一 versioned package 定義 L1 Concept／L2 Production Sheet／L3 DCC Source／L4 Unity Integration、Production Ready evidence、Infantry／Archer Golden Sample lock 與後續受阻 backlog；所有新目標明確標為 `PROPOSED`，現況／缺證據分別標為 `CURRENT`／`CANNOT VERIFY`。
  - Before：既有 Infantry／Archer 容易因「Prototype L3 已可玩」被誤認為 production sample；After：兩者均是 preserved prototype baseline／partial-remaster candidate，Golden Sample 未鎖，規格明示 `DO NOT MASS PRODUCE`。
- Architecture／API／Data：
  - Runtime code、asmdef、public API、Definition ID、ContentPack JSON、save schema、Prefab GUID 與 package dependency 均無變更。
  - 新規格強制 Definition／Runtime／View 分離、Player／AI 共用 Command、美術 Prefab 不持有 gameplay truth、不建立 God Manager；stable IDs `unit.infantry`／`unit.archer` 與現有 presentation contracts 在未核准 migration 前保持。
  - Backlog 中 Spearman、Heavy Infantry、Mage 只有 planning label 且明標 runtime ID `TBD`，沒有捏造 Definition；Cavalry／Siege／Hero／Building 只引用 repository 已存在的 ID／placeholder。
- Tests／Validation：
  - Mandatory-document validator：21 files、114,787 bytes；20/20 task-required core filenames present，0 missing、0 empty、0 odd code fences、0 unindexed core documents、21/21 files exactly one H1，PASS。
  - ZIP structural/read validation：21/21 entries 可完整串流讀取，20/20 required core files加 `Legacy_Spec_Migration.md` 全部存在，uncompressed 114,787 bytes、stream bytes read 114,787，PASS。Windows `Compress-Archive` 使用反斜線 entry separator；以 `RTS_Asset_Production_Spec_v1[\\/]` 驗證全部位於單一頂層資料夾。
  - Production asset integrity：前後均為 71 files／24,898,052 bytes，combined manifest SHA-256 前後皆 `A00DB4E5A733D90B021CCCE3F2CBBB660798EDB70F4B6D2FE4215DFBD5461C05`；`git status -- ArtSource Shared/Art` 0 entries，PASS。
  - Blender 5.2 read-only audit：Infantry 23 objects／12 meshes／6,430 total all-LOD/equipment tris／1 armature／23 bones／0 saved Actions；Archer 36 objects／25 meshes／5,232 total all-LOD/equipment/projectile tris／1 armature／23 bones／0 saved Actions；body vertices 100% weighted、max influences 1。兩份 source 未儲存，結果已列入 Open Issues。
  - Material/YAML static audit：5/5 materials 使用 URP Lit；0 custom shader／Shader Graph。Infantry Base 只接 BaseColor＋Normal，ORM／TeamColorMask 未接；Archer materials 無 texture reference；現行 team color 是 material-name slot＋MaterialPropertyBlock，PASS as audit／FAIL as production gate。
  - Unity EditMode／PlayMode、Builder、clean import、Windows build、Profiler、owner playtest：`NOT RUN`（docs-only scope）；沒有沿用歷史結果冒充本次通過。
  - `git diff --check`：PASS；只檢查 tracked `DevelopmentProgress.md`，新 package/ZIP 是 untracked，另由上述結構與串流驗證涵蓋。
- Acceptance（對任務 Success Criteria 逐項）：
  - `[PASS]` 已掃描實際專案；已整理 Infantry；已整理 Archer；已確認目前 L1／L2／L3／L4 現況。
  - `[PASS]` 已建立正式 L1→L4 Pipeline；Character Production Standard；Silhouette Standard；Texture／Material／Team Color Standard；Rig／Skinning／Animation Standard；LOD Standard。
  - `[PASS]` 已建立 Golden Sample Standard；Infantry／Archer Remaster Audit；Hero／Special Unit Standard；Modular Character Standard；獨立 Building Standard。
  - `[PASS]` 已建立 Asset Naming／Folder Standard；AI Provenance／License Standard；Unity Acceptance Checklist；Master Production Checklist。
  - `[PASS]` 已建立 Production Backlog；Agent Execution Guide；Open Issues；README；額外 Legacy Specification Migration。
  - `[PASS]` 已保留原始輸出資料夾；已產生 ZIP；已逐 entry 驗證 ZIP。
  - `[PASS]` production asset 前後 checksum 相同且 asset paths 無 Git change；未自動 Git Push。
  - `[DEFERRED — by design]` Golden Sample Production Ready、實際 remaster、Unity current acceptance 與量產；這些是文件定義的後續 gate，不是本次規格任務應假造的完成項。
- Completed：完整 repository／DCC／Unity YAML／content 靜態稽核；21 份規格文件；Golden Sample 現況、remaster 決策與禁止量產 gate；checklists／backlog／agent guide／open issues／legacy migration；原始資料夾與已驗證 ZIP；production asset integrity 證明。
- Not Completed／Deferred：Infantry／Archer 正式 L1/L2、來源權利、source Actions、production topology/skin/texture/shader/LOD/animation、Unity captures/tests/build/profiling、Art Bible、target hardware、Golden Sample sign-off 與所有後續資產量產。
- Known Issues／Risks：
  - P0：來源／商用散布權利未完成；Stylized Fantasy 與既有 East-Asian-inspired low-poly prototype 的 Art Bible 方向未決；Infantry 正式 L2、Archer L1/L2 缺失。
  - P0：兩個 reopened `.blend` 都是 0 Actions，與舊報告「Actions stored」不一致；獨立 animation FBX 與 rebuild scripts 尚可重建，但不是 durable source-action pass。
  - P0/P1：沒有 production team-color shader；ORM／mask 未接，Archer 無 runtime textures；目前模型是 prototype poly/skin/LOD，不是 production budget。
  - P1/P2：repository 缺 current Unity acceptance captures、DCC neutral/game-like renders、target hardware／battle count performance baseline、Golden Sample approvers；詳見 `99_Open_Issues_and_Missing_Information.md`。
- Git：`main`／HEAD `ec0560192863a763d6beb3be6b9c0c642b1d4137`；未 commit、未 push。預期最終 `git status --short` 為 tracked `DevelopmentProgress.md` modified，加 untracked `docs/ArtProduction/`；展開 untracked files 為本任務 23 個變更路徑（21 Markdown、1 ZIP、1 progress file），沒有 production asset path。
- Next（排序）：
  1. Producer／legal 補齊 Infantry／Archer provenance、license 與可散布證據；Art／Design 鎖定 world-neutral Stylized Fantasy 方向及 Golden Sample approvers。
  2. 依新語意審核 Infantry L1、製作 Infantry L2；為 Archer 製作 L1/L2，未通過前不做 mass production。
  3. Technical Art 修正／確認 Blender Action 保存，並在兩個 samples 驗證 shared skeleton/socket、URP team-color shader、packed map 與 production LOD policy。
  4. 建立 target hardware／camera／battle-count matrix，對 versioned remaster 跑 Unity acceptance、tests、build、capture、Profiler 與 owner playtest，再決定 Golden Sample lock。
  5. Golden Sample 雙方鎖定後，才依 backlog 逐一解鎖 Spearman、Heavy Infantry、Cavalry、Mage、Special、Hero 與 Buildings；runtime 功能另維持現有 Attack-Move owner playtest 與守城／AI 攻城優先序。

## 2026-08-13 — 步兵 Move 姿勢修正與全角色 Attack-Move／取消後搖

- Status：Completed（步兵移動保持直立、弓兵與所有 prototype combat roles 套用相同 Attack-Move 語意、數值規格、domain／presentation／army tests、完整回歸與 Windows build 均完成；最終平衡仍須玩家手感測試）
- Goal：修正步兵 Move 時整個 Humanoid 橫躺；讓弓兵與其他角色在收到移動命令時可取消尚未出手的攻擊，或在出手後取消剩餘動畫後搖，同時保持完整攻擊冷卻，支援不額外增加 DPS 的拉打操作。
- Baseline：
  - Branch／HEAD：`main`／`fd4b91461c301496ac2cd1b170dccb888855c09f`；開始本項時 worktree 已包含尚未提交的 Unit 04 弓兵 L3 整合，該既有範圍保留且一併回歸，沒有覆寫或拆除。
  - 步兵舊的實際移動 capture 顯示整個角色旋轉成水平；原測試只確認 Animator state／位移／事件，沒有檢查 Head 與雙腳的世界座標關係，因此產生 false pass。
  - `CombatSystem.NotifyMoveOrder` 已會清除攻擊目標，但 Animator Attack state 只能在約 95% clip 長度後退出，所以 gameplay 已接受 Move、畫面仍等待攻擊動畫完成。
  - Prototype 的 Hero／Infantry／Archer／Cavalry／Siege cadence 分散硬編碼於 composition，只有 cooldown／windup，沒有公開 APS、recovery、可取消後搖或 animation blend 規格。
- Scope In：
  - 修正 Infantry Blender master／五個 clips 的站立 baseline 與 Unity visual basis；重建 `.blend`、FBX、Animator Controller、Prefab、manifest。
  - 建立所有 prototype combat roles 的集中式攻速／前搖／後搖／取消 blend；讓 Animator event 與 authoritative windup 對齊。
  - 實作個別 Move 與軍團 Move／Defend／Retreat 的前搖取消、出手後 presentation backswing 取消、cooldown 保留；Player／AI 共用既有 command path。
  - 新增 domain、army、PlayMode animation cancel 與實際 Humanoid standing regression tests；更新 Framework API、L3 report、正式 Attack-Move 規格與本進度紀錄。
- Scope Out／Deferred：
  - 不改 damage、range、projectile speed、命中演算法、交戰模式、世界觀資料或最終競技平衡；不製作騎兵／攻城／英雄正式 Attack clips。
  - 不加入移動中同時射擊；目前是攻擊出手後立即移動、冷卻完成後再攻擊的 stop-and-go orb walking。
  - 正式 Content Pack cadence schema、攻速 buff／debuff、網路同步與高頻長時間 DPS performance 測試留待後續產品階段。
- Major Files／Assets：
  - `ArtSource/Units/Infantry/CHR_Infantry_A/v002/Source/build_unit03_l3_blender.py`、重新產生的 `.blend`、Master／五個 FBX、`Documentation/L3_DELIVERY_REPORT.md` 與 `MANIFEST.json`。
  - Unity Infantry FBX、`AC_Infantry.controller`、`PF_Unit_Infantry.prefab`、`InfantryL3PrefabBuilder.cs`；Archer controller／prefab 與 `ArcherL3PrefabBuilder.cs` 增加 `AttackRate` 安全預設。
  - `PrototypeCombatTuning.cs`、`PrototypeSystemComposition.cs`、`PrototypeUnitAnimatorView.cs`、`PlayablePrototypeBootstrap.cs`。
  - Package `CombatModels.cs`、`ArmyOrderExecutor.cs`；tests：`CombatAbilityTests.cs`、`HeroArmyCommandTests.cs`、`PlayablePrototypePlayModeTests.cs`。
  - 新增 `docs/45_Attack_Cadence_OrbWalking_攻速與取消後搖規範.md`，同步更新 `docs/26_Framework_API_目標介面.md`。
- Behavior Before／After：
  - Before：Infantry Move 會以水平姿勢沿地面滑動；After：source master 清除 active action／pose，所有 clips 明確 key 站立 body baseline，Unity visual root 使用 identity，實際 Move capture 與 bone assertion 均保持直立。
  - Before：弓兵收到 Move 後雖然 gameplay target 被清除，仍要等待 Attack clip 幾乎播完；After：出手前 Move 不生成箭、不消耗未成立攻擊的 cooldown，重新 Attack 會重跑完整 windup；出手後 Move 保留已發射箭與 cooldown，Animator 以 0.06 秒 cross-fade 立即進 Move。
  - Before：不同角色數值散落且 clip event 未依 gameplay attack point 縮放；After：`AttackRate` 對齊 clip event 與 authoritative windup，所有 prototype role 從單一 tuning table 取得 cadence。
- Architecture／API／Data：
  - `CombatSystem` 保持 authoritative gameplay truth；View 只根據 Combat／Movement snapshot 觸發或取消動畫，不套用傷害、不建立 authoritative projectile、不清除 cooldown。
  - `AttackProfile` 新增唯讀衍生 API：`AttackIntervalSeconds`、`AttacksPerSecond`、`RecoverySeconds`、`MoveCancelableBackswingSeconds`；constructor 與既有 serialized data 未改，無 migration。
  - `GameplayArmyOrderExecutor` 在 movement accepted 後通知 Combat，確保 army Move／Defend／Retreat 與單位 Move 相同；Prototype army executor 原已有相同 seam，Player／AI 仍共用 Command／Army flow。
  - Prototype cadence：Hero 0.80／0.25／0.55 s，Infantry 0.95／0.30／0.65 s，Archer 1.10／0.38／0.72 s，Cavalry 1.25／0.40／0.85 s，Siege 2.20／1.05／1.15 s（順序為 interval／windup／recovery；詳細 APS／blend 見 docs/45）。
- Tests／Validation：
  - Blender 5.2 Infantry rebuild：PASS；LOD 4,376／1,512／542 triangles、5 clips、Move 0–24、AttackImpact frame 13；重新產生來源與 FBX。
  - Infantry／Archer Unity Builder：PASS；valid Humanoid、5 clips、Animator／Prefab contracts；`AttackRate` default=1。
  - Combat domain targeted：2/2 PASS；驗證 Move during windup 零傷害且退回未成立攻擊的 cooldown，以及 impact 後 Move 保留 cooldown／無額外攻擊。
  - Army targeted EditMode：1/1 PASS；`GameplayArmyOrderExecutor.Move` 清除每個 member 的 active attack。
  - Archer Attack-Move targeted PlayMode：1/1 PASS；驗證 0.38 秒 release 對齊、出手前取消不發射、出手後取消切 Move；Infantry actual movement standing targeted：1/1 PASS，並人工檢查 `Move_00.png`、`Move_02.png` 為直立。
  - Full EditMode：180/180 PASS，0 failed／skipped／inconclusive；出手前 cooldown 語意修正後再次 180/180 PASS（`Logs/OrbWalkFullEditModeAcceptance.xml`）。
  - Full PlayMode 初次：38/40，兩項直接 Prefab event 測試因新增 `AttackRate` default=0 失敗；修正後 40/40。之後把 animation events 從錯誤的 clip-length 除數改為正式 30 FPS frame time，完整輪先後暴露測試等待窗不足（38/40、39/40）；擴充等待窗但保留 0.25 秒前不得放箭的精確 assertion 後，最終 40/40 PASS（`Logs/OrbWalkFullPlayModeAcceptance.xml`）。未將任何失敗輪誤記為通過。
  - `dotnet restore`＋solution build：PASS，0 warnings／0 errors；`git diff --check`：PASS（僅 line-ending warnings）；Infantry manifest：26 files、0 missing／mismatch。
  - Gameplay cancel 語意最終修正後 Full PlayMode 再次 40/40 PASS（`Logs/OrbWalkFullPlayModeFinalAcceptance.xml`）。
  - Windows Development Build：PASS，BuildReport 187,670,429 bytes（`Logs/OrbWalkWindowsBuildAcceptance.log`）；Player 啟動 10 秒仍 responding，1920×1080 native fullscreen，log exception／crash／error scan 0 hits。
- Acceptance：
  - `[PASS]` Infantry 實際 Move 不再橫躺；Head 高於雙腳、Y 為主要高度軸、root motion 保持關閉。
  - `[PASS]` 出手前 Move 取消攻擊且不產生 melee damage／projectile；出手後 Move 只取消 visual backswing。
  - `[PASS]` 出手前取消退回未成立攻擊的 cooldown 但重新攻擊須重跑完整 windup；出手後取消不清除 cooldown，因此理論 APS 不被 Attack-Move 提高；個別與 army order 使用一致語意。
  - `[PASS]` Hero／Infantry／Archer／Cavalry／Siege cadence 與 cancel 參數有單一程式來源及詳細文件。
  - `[PASS]` 完整 EditMode／PlayMode、builders、solution、manifest、Windows build／launch 通過。
  - `[DEFERRED]` 使用者在實際 Player 對弓兵 0.38 秒前搖、0.06 秒切換與各角色節奏做主觀手感調整；正式平衡不以 automated test 代替。
- Completed：步兵 source／runtime 姿勢修復、集中 cadence、AttackRate event alignment、個別／軍團移動取消、冷卻保護、規格／API／L3 文件、tests、build 與 launch smoke。
- Not Completed／Deferred：正式各兵種動畫與最終平衡、Content Pack cadence schema、攻速狀態效果、網路同步、玩家守城與隨機武將功能。
- Known Issues／Risks：
  - 目前「所有角色」共用 gameplay cancel contract；只有 Infantry／Archer 有正式連接的 L3 Attack clips，Hero／Cavalry／Siege 仍使用原型外觀，需在交付正式 clip 時設定正確 event time。
  - Animator cancel 是 presentation cross-fade；很短時間內密集 alternate Attack／Move 的最終視覺流暢度仍需真人操作判斷，但 domain cooldown／damage 已由 tests 鎖定。
  - Worktree 同時包含前一項尚未 commit 的 Archer L3 大型交付與本項修改，以及既有未追蹤 `AegisRTS_Codex_3D_Asset_Production_Spec_Task.md`；提交前需由專案擁有者確認是否一起納入。
- Git：`main`／HEAD `fd4b91461c301496ac2cd1b170dccb888855c09f`；尚未 commit／push；最終 `git status --short` 58 entries（包含 prior Archer L3、此次 Infantry／Attack-Move、Unity build/import settings 與既有 untracked spec）。
- Next（依序）：
  1. 在最新 Windows Player 操作弓兵 Attack→Move→Attack，主觀確認攻擊前搖與切換速度；只從 `PrototypeCombatTuning` 微調並同步 docs/45／tests。
  2. 確認 55 個 worktree entries 的提交邊界；若接受將 Archer L3 與本修正合併交付，再 commit／push，勿遺漏 FBX／Prefab／`.meta`／docs。
  3. 為下一個正式 L3 角色建立 clip event frame 與 authoritative windup 對齊驗收，不複製散落 cadence 數字。
  4. 回到玩家守城／AI 正式攻城／城門維修成本，或依 Batch A 製作指揮官／城門美術。

## 2026-08-13 — Unit 04 弓兵 Prototype L3 與箭矢整合

- Status：Completed（可玩 Prototype、可重建來源、Unity 資產、事件驅動箭矢、完整測試、畫面檢查與 Windows Build 均完成；正式弓弦動畫與 production art release review 為 Deferred）
- Goal：依步兵的來源／Runtime 分流方式，讓 `unit.archer` 從 Capsule placeholder 升級為可辨識、可移動、可攻擊的 Humanoid 弓兵，並以獨立箭矢呈現既有 authoritative ranged combat event。
- Baseline：
  - Branch／HEAD：`main`／`fd4b91461c301496ac2cd1b170dccb888855c09f`；開始時 worktree clean。
  - `unit.archer.prefabId` 原為 `PF_Unit_Placeholder`；場景弓兵是 Capsule，沒有 Animator、LOD、Team Color、弓／箭袋、發射 socket 或箭矢畫面。
  - Package `CombatSystem` 已正確發布 `ProjectileLaunchedEvent` 並 authoritative 計算命中／傷害；Prototype composition 只記錄通知，既有 `UnityCombatDriver` 同時 Tick Combat，不能直接加入場景，否則會與 composition 重複推進 simulation。
- Scope In：
  - world-neutral 低模弓兵來源包、三段 LOD、Humanoid、五個 In Place clips、Team Color、獨立箭矢、Unity import／Prefab builder、ContentPack 綁定、pooled flight／impact presentation、automated tests、近距離 capture、規格／操作／架構文件。
- Scope Out／Deferred：
  - 最終世界觀造型、production PBR 貼圖、正式弓弦 deform／release animation、音效、弓兵數值重平衡、修改 Combat damage timing、homing projectile gameplay、玩家守城／隨機武將模式。
- Major Files／Assets：
  - `ArtSource/Units/Archer/CHR_Archer_A/v001`：18 files／4,776,751 bytes；含 Blender 5.2 rebuild script、`.blend`、Master／5 clips／arrow FBX、prompt、版本／授權紀錄、BUILD_RESULT 與 SHA-256 manifest。
  - `Assets/AegisRTS/Content/Shared/Art/Units/Archer`：34 files／4,811,615 bytes；含 FBX／meta、三個 runtime materials、`AC_Archer.controller`、`PF_Unit_Archer.prefab`、`PRJ_Arrow_Basic_v001.prefab`。
  - 新增 `ArcherL3PrefabBuilder.cs`、`PrototypeProjectileVisualController.cs`；更新 Art Catalog／View／Animator、Bootstrap、ContentPack、Smoke Validation 與 PlayMode tests。
  - 新增 `docs/ArtSpecs/Unit_04_弓兵_L3實作交付與驗收.md`，同步更新 Unit 04、Batch gap、操作手冊與架構維護文件。
- Behavior Before／After：
  - Before：弓兵與其他未製作兵種都是 Capsule；遠程攻擊雖有 gameplay event／damage，但 Prototype 只顯示文字通知，沒有飛行物或 impact。
  - After：玩家與敵方弓兵載入同一 `PF_Unit_Archer`，以 runtime Team Color 區分；Idle／Move／Attack_Ranged／Hit／Death 由 authoritative snapshots 驅動且 Root Motion Off。每個 `ProjectileLaunchedEvent` 從弓兵 socket 租用 Z+ 箭矢，以小幅拋物線飛向目標，抵達後歸還 pool 並租用短暫 impact flash。
  - 第一輪近距離 capture 發現 Attack clip 下半身因未 key Humanoid muscles 而折疊；來源腳本加入明確 grounded full-body baseline，重新生成／匯入後，Idle 與 4 個 Attack samples 均站立。測試也改用 Humanoid 頭腳骨骼，避免弓的 renderer bounds 掩蓋姿勢錯誤。
- Architecture：
  - `CombatSystem` 保持唯一 gameplay truth；`PrototypeProjectileVisualController` 只訂閱 event，不 Tick Combat、不含 Collider、不套用 damage、不回寫 domain state。
  - 箭矢與 impact 使用 Core `ObjectPool<GameObject>`；Gameplay／package Runtime 未新增 Unity dependency，Player／AI 仍共用 Command／Combat pipeline。
  - `PrototypeUnitArtView` 只新增可選 `ProjectileSocket` presentation anchor；正式美術可在保持 Prefab ID／Animator parameters／socket／event／Z+ contract 下替換，不需要改 gameplay code。
- API／Data：
  - Demo public API 新增 `PrototypeUnitArtCatalog.ArcherPrefabId／ArcherResourcePath`、`PrototypeUnitArtView.ProjectileSocket`、`PrototypeUnitAnimatorView.ProjectileReleaseCount／ProjectileRelease()`、`PlayablePrototypeBootstrap.ProjectileVisuals` 與 projectile diagnostics counters。
  - Content data：`unit.archer.prefabId` 由 `PF_Unit_Placeholder` 改為 `PF_Unit_Archer`；save schema、package public API、combat stats 與 scenario JSON 無變更。
- Tests／Validation：
  - Blender 5.2 rebuild：PASS；1.78 m、LOD0／1／2 = 3,344／1,280／542 triangles、5 clips、release frame 22 @30 FPS、0.82 m arrow。
  - Unity `ArcherL3PrefabBuilder.BuildAndValidate` 最終重跑：PASS；Humanoid `isHuman=True`／`isValid=True`、5 clips、LOD／equipment／arrow size／Z+／center pivot／no collider gates 全通過。
  - Archer targeted PlayMode：2/2 PASS；Archer close-inspection：1/1 PASS。capture 共 7 張，人工檢查修正版 Idle／Attack 不折疊。
  - FrameworkLab EditMode：177/177 PASS，0 failed／skipped／inconclusive。
  - FrameworkLab PlayMode：初跑 37/39；2 failures 是舊測試把所有 UnitArt 都當步兵。測試改為依 Bow／Shield 分類後重跑 39/39 PASS。
  - Windows Development Build：PASS，BuildReport 187,657,769 bytes；輸出 `C:\projects\Unity\AegisRTS.BuildValidation\PlayablePrototype_01.exe`。啟動 10 秒：process responding，Player log `NullReferenceException／MissingReferenceException／Unhandled Exception／Crash／error CS` = 0 hits。
  - `dotnet build AegisRTS.FrameworkLab.slnx`：PASS，0 warnings／0 errors；`git diff --check`：PASS。
- Acceptance：
  - `[PASS]` `unit.archer` 使用獨立 `PF_Unit_Archer`，玩家／敵方各一名，且可與盾劍步兵辨識。
  - `[PASS]` Humanoid、Root Motion Off、Idle／Move／Attack_Ranged／Hit／Death、`ProjectileRelease`、三段 LOD、Team Color 與 anchors。
  - `[PASS]` 箭矢是獨立 0.82 m、local Z+、center-pivot、無 Collider Prefab，flight／impact pool 可回收且不影響 gameplay damage。
  - `[PASS]` 初版 Attack 折疊已由畫面檢查抓出、修正並加 regression；完整 tests／build／launch 通過。
  - `[DEFERRED]` `ProjectileRelease` 與正式弓弦放開相差不超過一 frame、手臂／弓弦／箭袋 production clipping review；本版沒有可變形弓弦，不誤標正式美術通過。
- Completed：可玩的弓兵 vertical slice、來源重建、Unity integration、content binding、projectile presentation、debug counters、automated validation、visual review 與文件。
- Not Completed／Deferred：final art／textures、弓弦骨架動畫、音效、目標設備長時間 profiling、專案擁有者主觀手感驗收。
- Known Issues／Risks：
  - 角色身體衍生自步兵 Prototype；原始來源權利未確認前不可當成 production release asset。
  - 弓弦目前是 rigid visual，攻擊姿勢足以表達 Prototype 射擊但不具正式拉弦細節；近距離可能看出手部／弓弦簡化。
  - 視覺箭矢使用 event 發射當下／目前 view 目的地，不追蹤飛行中的目標；這不影響 authoritative 命中，但高速移動目標可能看到視覺落點差異。
- Git：結束時仍在 `main`，未 commit／未 push；本筆紀錄加入後為 20 個 status entries，僅含本任務 archer source／runtime／code／tests／docs。Unity build 造成的 Scene fileID、URP prefilter、Connect 與 ProjectSettings 序列化副作用已移除；TestResults XML 未納入 repository。
- Next（排序）：
  1. 專案擁有者在 Windows Player 實際下達弓兵攻擊，確認箭速、弧度、impact、陣營辨識與近距離穿模手感。
  2. 若手感通過，依 Batch A 優先製作指揮官或城門 Prototype L1→L3；不需要先做地圖美術。
  3. 另立 production archer art 任務時，補正式弓弦 deform／release、貼圖與來源權利 review，但維持既有 Runtime contract。
  4. 系統面回到玩家守城、AI 正式攻城與維修成本，或依最新產品優先級選擇。

## 2026-08-13 — 步兵停止後 Idle 站立姿勢修正

- Status：Completed（程式、骨骼驗證、正／側面 capture、完整測試與 Windows Player 啟動完成；專案擁有者主觀動作驗收待確認）
- Goal：修正步兵停止移動後雙腿向前平伸、側面看起來像坐倒／躺下的問題，確保初始 Idle 與 Move→Idle 轉場最後都呈現直立站姿。
- Baseline：
  - Branch／HEAD：`main`／`1048f1aa5cf3c0feb5e7fde6fe5696f19fe1d9e4`；開始時 14 個 status entries，包含尚未提交的原生全螢幕、主選單退出與滾輪倍率工作，本次全部保留。
  - 實際以 `AEGIS_CAPTURE_INFANTRY_DETAIL=1` 執行既有 close-inspection PlayMode test：測試雖 PASS 1/1，但 `InfantryDetail_Side.png` 清楚顯示 Idle 雙腳在身體前方、腿部近水平。舊測試只在播放 Idle 後取得 renderer bounds，實際站立 envelope assertion 僅套在 Move 四個 samples；盾牌／武器又會撐大整體 bounds，因此錯誤被漏掉。
  - Humanoid 診斷值：錯誤 Idle 的 Head 到雙腳平均垂直距離 1.264 m、XZ planar lean 0.462 m；正面容易誤認直立，側面才能清楚重現。
- Scope：
  - In：Idle-only Humanoid leg stance correction、Move→Idle transition、頭／腳骨骼站立 regression、正面／側面 capture、Demo architecture／操作／L3 delivery 文件、manifest integrity、solution/full tests、Windows build／launch。
  - Out／Deferred：重做 Blender rig／skin／Idle FBX、修改 Move／Attack／Hit／Death、專業 foot IK、重新建模、Gameplay 移動／碰撞／傷害、其他兵種動畫。
- Changed：
  - `Assets/AegisRTS/Demo/PlayablePrototype/PrototypeUnitAnimatorView.cs`：保存目前 moving 狀態；只在 Animator 已進入 `Idle`、不在 transition、非 moving、非 death 時於 `LateUpdate` 校正左右腿。每腿將 thigh→shin、shin→foot 的世界方向對齊 `Vector3.down`，並在祖先旋轉後還原 foot world rotation，讓腳掌方向不被連帶扭轉。新增 `IdleStanceCorrectionCount` 作 debug／regression 證據。
  - `Assets/AegisRTS/Tests/PlayMode/PlayablePrototypePlayModeTests.cs`：既有 L3 prefab test 新增初始 rendered Idle 與 Move→Idle 驗證；close-inspection test 新增 Idle correction count 與 `AssertHumanoidStanding`。判斷改用 Head／LeftFoot／RightFoot 骨骼，要求 head-to-feet height >1.2 m、planar lean <0.25 m，不再被盾牌 renderer bounds 誤導。
  - `docs/37...操作與驗收手冊.md`：更新已整合 Humanoid L3 與停止後直立預期；`docs/38...架構與維護.md`：更新已過時的「靜態 L2」敘述，記錄 Animator state boundary 與 Idle 相容層。
  - `ArtSource/.../L3_DELIVERY_REPORT.md`：如實記錄 v002 Humanoid Idle leg baseline 缺陷、目前 presentation correction 與正式美術替換條件；`MANIFEST.json` 同步更新 bytes／SHA-256。
- Behavior Before／After：
  - Before：步兵初始 Idle 或由 Move 停止後，torso 大致直立但雙腿向前伸；正面不明顯，側面像坐在空中或躺倒。
  - After：完成 Animator transition 進 Idle 後，左右大腿與小腿垂直落在身體下方，腳的世界旋轉保持；Move、Attack、Hit、Death 不套用校正，Gameplay root、位置、碰撞與 Root Motion contract 不變。
- Architecture／API／Data：
  - Architecture：相容修正位於 Demo Presentation bridge，讀 Animator state 並只改 View bones；Movement／Combat snapshots 仍是 authoritative truth，沒有反向寫入 Gameplay Transform、Command 或 domain state。
  - API：Demo public `PrototypeUnitAnimatorView` 新增 read-only `IdleStanceCorrectionCount`；package Framework public API 無變更。
  - Data：Content Pack、Scenario、Definition、Save schema、Input Actions 均無變更；只更新 L3 delivery report 的 manifest hash，26/26 entries 相符。
- Tests／Validation：
  - Baseline close-inspection capture：PASS 1/1，但人工檢視 `C:\projects\Unity\AegisRTS.BuildValidation\InfantryDetailReview\InfantryDetail_Side.png` 判定 Idle 失敗；證明原 regression 有 false negative。
  - 新骨骼 diagnostic 首次 Unity compile：FAIL，測試暫時使用 `Debug.Log` 時與 `System.Diagnostics.Debug` 名稱衝突，CS0104；改成完整限定名取得數值後移除 diagnostic log，產品 runtime 未受影響。
  - Corrected Idle close-inspection targeted PlayMode：PASS 1/1；0 failed／0 skipped；`Logs/IdleStandingFinalInspection.xml`。正／側面 capture 已人工檢視，雙腿位於身體下方。
  - Move→Idle／Avatar／events／Root Motion targeted PlayMode：PASS 1/1；0 failed／0 skipped；`Logs/IdleStandingFinalTransition.xml`。
  - `dotnet restore AegisRTS.FrameworkLab.slnx`＋`dotnet build --no-restore`：PASS，0 warnings／0 errors。
  - FrameworkLab Full EditMode：PASS 177/177；0 failed／0 skipped；`Logs/IdleStandingFullEditMode.xml`。
  - FrameworkLab Full PlayMode：PASS 36/36；0 failed／0 skipped；`Logs/IdleStandingFullPlayMode.xml`。
  - Clean package validation：`NOT RUN` — 本次沒有修改 package Runtime；沿用前一筆實際 6/6、3/3 證據，不冒充本次重跑。
  - L3 delivery manifest：PASS，26 entries、0 missing／hash mismatch。
  - Windows Development Build：PASS；BuildReport 186,829,217 bytes；輸出 332 files／187,185,803 bytes；`Logs/IdleStandingWindowsBuild.log`。
  - Windows Player smoke：PASS；process 執行 10 秒未提前退出且 responding，1920×1080 native fullscreen，Player log error scan 0 hits，之後由驗證流程關閉。
- Acceptance：
  - 初始靜止單位直立：`PASS` — rendered Idle 骨骼 regression 與 corrected 正／側面 capture。
  - Move→Idle 後直立：`PASS` — 0.25 秒 transition 後 head／feet stance gate 通過。
  - 不影響非 Idle animations：`PASS` — correction 明確排除 moving、transition、Attack／Hit／Death state；events／Root Motion targeted 與 full PlayMode 通過。
  - 不修改 gameplay truth：`PASS` — 只改 Animator 子骨骼；既有 root position <0.001 m regression 通過。
  - 實際玩家主觀停止動作：`NOT RUN` — automated player 以 hidden window 驗啟動與 log，最終手感仍需專案擁有者操作。
- Completed：已重現側面錯誤、補足測試盲點、加入 Idle-only 站立校正與 debug counter、完成 capture／文件／manifest／full tests／Windows build／launch。
- Not Completed／Deferred：沒有重製來源 Blender rig／Idle FBX；目前是 Prototype presentation compatibility fix。正式 L2.1／final-art asset 應在 DCC 直接交付正確 Unity Humanoid Idle，再刪除本相容層。
- Known Issues／Risks：Idle 校正讓腿部保持直立，會降低原生成 clip 在下肢的細微擺動；上半身 Idle breathing／持盾姿勢仍由 clip 保留。每個 Idle 步兵每 frame 只處理 6 個 Humanoid bone transforms，成本低但仍應在 G09 300+ visible animated units profiling 中量測。若未來 rig 缺少標準 leg bones，校正會安全跳過，但 `IdleStanceCorrectionCount` 與 regression 將暴露缺口。
- Git：Branch `main`；本次 Animator／test／docs 與前面三筆累積變更均納入本筆紀錄所在 commit，commit message 為 `fix Infantry pose`；push 未執行。Commit 前共有 18 個 status entries；Unity build 自動重寫的 Scene fileIDs、URP／Volume、batching 與 Unity Connect 設定已逐項還原；`git diff --check` PASS（只有既有 line-ending 提示）。
- Next：
  1. 啟動 `C:\projects\Unity\AegisRTS.BuildValidation\IdleStandingBuild\PlayablePrototype_01.exe`，選一名步兵走 3～5 m 後停止，從遊戲固定視角與近距離 `F` 聚焦確認站姿。
  2. 若站姿接受，保留此 Prototype correction 並回系統工作；若要求自然 Idle 腿部微動，另立 DCC Idle clip 重製，不在 LateUpdate 疊更多動畫規則。
  3. 後續 G09 效能階段量測大量可見 Humanoid units 的 Animator／LateUpdate 成本。

## 2026-08-13 — 滾輪縮放加速與 +/- 即時速度調整

- Status：Completed（機械性、自動化與 Windows Player 啟動驗證完成；實體滾輪主觀手感待專案擁有者確認）
- Goal：把原本過慢的滑鼠滾輪縮放提高數倍，並讓玩家不必離開遊戲即可用 `+`／`-` 調整縮放速度。
- Baseline：
  - Branch／HEAD：`main`／`1048f1aa5cf3c0feb5e7fde6fe5696f19fe1d9e4`；開始時有前兩筆原生解析度與主選單退出工作的 9 個尚未提交 tracked／untracked entries，本次全部保留。
  - `RtsCameraController.zoomSpeed` 為 `0.025`，Windows 常見一格 scroll delta `120` 只改變 3 m（20→17 m）；沒有靈敏度倍率、快捷鍵或遊戲內目前倍率提示。
- Scope：
  - In：預設縮放加速、1～6 倍離散倍率、主鍵盤與數字鍵盤 `+`／`-`、設定面板倍率顯示、Input Action 資產、camera public API、targeted／full tests、clean package validation、操作／API／畫面規格文件、Windows build／launch。
  - Out／Deferred：改變 2.5～40 m 距離邊界、相機旋轉、平滑／慣性 zoom、設定持久化、可拖曳 slider、重新設計 UI、修改 gameplay 或 save schema。
- Changed：
  - `RtsCameraController.cs`：保留 `zoomSpeed=0.025` 作為 base speed，新增預設 `ZoomSensitivity=3`，scroll delta 乘上倍率；新增 `IncreaseZoomSensitivity()`／`DecreaseZoomSensitivity()` 並 clamp 在 1～6，倍率改變時輸出 camera debug log。
  - `UnityRtsInputAdapter.cs`：runtime Input Map 新增縮放速度加／減 action；綁定 `<Keyboard>/equals`、`<Keyboard>/numpadPlus`、`<Keyboard>/minus`、`<Keyboard>/numpadMinus`，每次按下委派 controller 調整。
  - `AegisRTS_RTS.inputactions`：同步加入 `ZoomSensitivityIncrease`／`ZoomSensitivityDecrease` 與四個鍵盤 binding，避免資產規格和 runtime map 漂移。
  - `PlayablePrototypeBootstrap.cs`：保留 controller reference；設定面板顯示目前 `×N` 並增加高度，教學快捷鍵補 `+／-`。
  - `PlayablePrototypePlayModeTests.cs`：擴充 display／wheel regression，驗證預設 ×3 的 20→11→20 m、×4 與 ×2 數值，以及連續按鍵調整時 1／6 上下限。
  - `Packages/.../Documentation~/FrameworkApi.md`、`docs/26...API.md`、`docs/37...操作與驗收手冊.md`、`docs/ArtSpecs/02...解析度相機與安全區.md`：同步記錄 public surface、預設倍率、快捷鍵、範圍與設定面板提示。
- Behavior Before／After：
  - Before：一格 `120` 的滾輪量只縮放 3 m，速度固定，玩家只能要求工程端修改 serialized base speed。
  - After：預設 ×3，因此同一格縮放 9 m；`+` 每次增加一級、`-` 每次減少一級，範圍 ×1～×6。縮放距離仍由 `RtsCameraRigModel` 限制在 2.5～40 m，設定面板可看目前倍率。
- Architecture／API／Data：
  - Architecture：鍵盤裝置讀取留在 Presentation input adapter，倍率與 clamp 留在 Presentation camera controller，距離真相仍由 camera model 擁有；Gameplay、Player／AI Command、UI→gameplay 邊界與 God Manager 規則均未改。
  - API：package public `RtsCameraController` 新增 read-only `ZoomSensitivity`／`ZoomSensitivitySummary`，以及回傳是否真的改變倍率的 `IncreaseZoomSensitivity()`／`DecreaseZoomSensitivity()`。
  - Data：修改 Demo `.inputactions` 控制規格；Content Pack、Scenario、Definition、Save schema 與既有存檔均無變更。倍率目前是 session presentation state，不寫入存檔。
- Tests／Validation：
  - `.inputactions` JSON parse：PASS；PowerShell `ConvertFrom-Json` 無錯誤。
  - `dotnet restore AegisRTS.FrameworkLab.slnx`＋`dotnet build --no-restore`：PASS，0 warnings／0 errors。
  - Targeted PlayMode `DisplayAndMouseWheel_UseNativeFullscreenPolicyAndZoomBothDirections`：PASS 1/1；`Logs/ZoomSensitivityTargeted.xml`。
  - FrameworkLab Full EditMode：PASS 177/177，0 failed／0 skipped；`Logs/ZoomSensitivityFullEditMode.xml`。
  - FrameworkLab Full PlayMode：PASS 36/36，0 failed／0 skipped；`Logs/ZoomSensitivityFullPlayMode.xml`。
  - Clean package project `C:\projects\Unity\AegisRTS.PackageValidation`：EditMode PASS 6/6、PlayMode PASS 3/3；file-installed package 可編譯並執行 package tests。
  - Windows Development Build：PASS；BuildReport 186,827,749 bytes；輸出 332 files／187,184,348 bytes；`Logs/ZoomSensitivityWindowsBuild.log`。
  - Windows Player smoke：PASS；process 執行 10 秒未提前退出且 responding，Player log 顯示 `Native Fullscreen · 1920×1080`，掃描 `Unhandled`／`NullReferenceException`／`MissingReferenceException`／`error CS`／`Aborting`／`InvalidOperationException` 為 0 hits，之後由驗證流程關閉。
- Acceptance：
  - 預設比原本快數倍：`PASS` — 預設 ×3，常見 scroll 120 從 3 m 提高為 9 m，targeted test 鎖定數值。
  - `+` 加快、`-` 減慢：`PASS` — runtime action map 與 Input Action 資產均有主鍵盤／數字鍵盤 binding；controller API regression 通過。
  - 速度有安全範圍：`PASS` — 重複調整 clamp 在 ×1～×6，test 覆蓋兩端。
  - 不破壞 zoom 距離邊界：`PASS` — 仍使用既有 `RtsCameraRigModel.ZoomBy` 與 2.5～40 m clamp；full suite 通過。
  - 玩家能看到倍率：`PASS` — 顯示設定面板即時讀 controller summary。
  - 實體鍵盤／滾輪主觀操作：`NOT RUN` — 自動化驗證數值、binding、build 與 player lifecycle，但未以桌面輸入自動化代替使用者手感。
- Completed：預設 ×3、1～6 倍即時調整、四個鍵盤 binding、設定提示、debug、API／操作文件、完整 regression、clean package validation 與 Windows build／launch 均完成。
- Not Completed／Deferred：沒有把倍率持久化到設定檔；沒有加入 slider／文字 toast；沒有改 zoom inertia。主鍵盤的 `+` 與 `=` 共用同一實體 key，因此不按 Shift 的 `=` 也會加速，數字鍵盤 `+` 則為獨立鍵。
- Known Issues／Risks：預設 ×3 在高解析度 free-spin 滾輪上可能仍偏快或偏慢，可先用 ×1～×6 現場調整；若未來需要裝置／語系鍵盤專屬 mapping，應改為 rebindable settings，而不是繼續硬加 control path。倍率離開 Player 後會回到預設 ×3。
- Git：Branch `main`；14 個 status entries，包含前兩筆尚未提交的 display／application adapter，以及本次 camera／input／test／docs／progress；未 commit／push。Unity build 自動重寫的 Scene fileIDs、URP／Volume、batching 與 Unity Connect 設定已逐項還原；`git diff --check` PASS（只有既有 `.inputactions` LF→CRLF 提示）。
- Next：
  1. 啟動 `C:\projects\Unity\AegisRTS.BuildValidation\ZoomSensitivityBuild\PlayablePrototype_01.exe`，各滾一格向上／向下，再按 `+`、`-` 確認自己最舒服的倍率。
  2. 若 ×1～×6 仍不夠，只調整倍率上限或 base speed，避免同時改距離邊界造成鏡頭規格混亂。
  3. 手感接受後，回到玩家守城／AI 正式攻城／維修成本；如需保留倍率，再新增獨立 user settings persistence，不塞進 gameplay save。

## 2026-08-13 — 主選單離開遊戲

- Status：Completed
- Goal：修正主選單只有開始／載入、沒有正常離開遊戲入口的 UI／application lifecycle 缺口；同一功能需在 Windows Player 與 Unity Editor 都有合理行為。
- Baseline：
  - Branch／HEAD：`main`／`1048f1aa5cf3c0feb5e7fde6fe5696f19fe1d9e4`；開始時有前一筆原生解析度／滾輪工作的 7 個尚未提交 tracked／untracked entries，本次保留並在其上修改。
  - `DrawMainMenu` 只有「開始新遊戲」與「載入進度」；legacy debug menu 同樣只有 New／Load。專案沒有 `Application.Quit`，`GameSessionController` 也刻意只負責 session state，不應承擔 platform application lifecycle。
- Scope：
  - In：中文主選單與 legacy menu 的離開按鈕、Player／Editor 分流、離開前 session cleanup、返回主選單 cleanup 共用途徑、PlayMode regression、操作文件、Windows build／launch。
  - Out／Deferred：退出確認對話框、未保存進度警告、自動存檔、鍵盤快捷鍵、作業系統關閉視窗事件、多平台 console／mobile quit policy。
- Changed：
  - `PrototypeApplicationAdapter.cs`：新增 Demo application boundary；Player 呼叫 `Application.Quit()`，`UNITY_EDITOR` 分支呼叫 `EditorApplication.ExitPlaymode()`，另提供純函式 `ResolveExitAction` 供 lifecycle test。
  - `PlayablePrototypeBootstrap.cs`：新增 `QuitNow()`，先 `DisposeSession()` 再委派 application adapter；`DrawMainMenu` 增加「離開遊戲」，legacy menu 增加 `Quit Game`。新增 `ReturnToMenuNow()` 統一遊戲中、結果畫面與 legacy menu 的 cleanup＋`Session.ReturnToMenu()`，避免測試或 UI 重複撰寫 cleanup sequence。
  - `PlayablePrototypePlayModeTests.cs`：新增 `MainMenuQuit_CleansSessionAndResolvesEditorAndPlayerLifecycle`，驗證 Editor／Player action mapping，載入真實 scene 後返回主選單並確認 `GameSessionState.MainMenu` 與 `Composition == null`。
  - `docs/37...操作與驗收手冊.md`：記錄主選單三個入口，以及 Player 正常結束／Editor 停止 Play Mode 的差異。
- Behavior：
  - Before：Player 進入主選單後只能開始或載入，只能用 Alt+F4／作業系統視窗手段離開；Editor 只能手動按上方 Stop。
  - After：主選單顯示「離開遊戲」。Windows Player 按下會先釋放 Composition、NavMesh、views、selection、input 與 subscriptions，再要求 Unity 正常退出；Editor 按下只停止 Play Mode，不關閉 Unity Editor。
- Architecture／API／Data：
  - Architecture：Application lifecycle 留在 Demo／Unity adapter；Gameplay `GameSessionController`、CommandBus、simulation state ownership 與 package dependency 都沒有改。Bootstrap 只組裝 cleanup 與 adapter 呼叫。
  - API：新增 Demo public `PrototypeApplicationAdapter.ResolveExitAction(bool)`／`Quit()`、`PrototypeExitAction`，以及 Bootstrap `ReturnToMenuNow()`／`QuitNow()`。Framework package public API 未改。
  - Data：Content Pack、Scenario、Save schema、Input Actions 與 Player Settings 無變更。
- Tests／Validation：
  - Unity batch import：PASS；無 compile error，正常 exit。
  - `dotnet restore`＋`dotnet build --no-restore`：PASS，0 warnings／0 errors；最終 refactor 後再跑一次同結果。
  - 初版 targeted lifecycle test：PASS 1/1，驗證 Editor／Player action mapping。
  - 首次主選單 scene/capture targeted test：FAIL 0/1；原因為 Unity batchmode 不會觸發 `WaitForEndOfFrame`，測試框架明確拋錯，並非產品 quit path 失敗。
  - 將 `WaitForEndOfFrame` 改成一般 frame yield 後 targeted scene test：PASS 1/1；batchmode 不可靠輸出 IMGUI screenshot，因此移除未產生證據的 optional capture 分支，不把它記為視覺通過。
  - Final Full EditMode：PASS 177/177；0 failed／0 skipped；`Logs/MainMenuQuitFinalEditMode.xml`。
  - Final Full PlayMode：PASS 36/36；0 failed／0 skipped；`Logs/MainMenuQuitFinalPlayMode.xml`。
  - Final Windows Development Build：PASS；BuildReport 186,826,473 bytes；輸出 332 files／187,183,067 bytes；`Logs/MainMenuQuitFinalWindowsBuild.log`。
  - Final Windows Player smoke：PASS；process 執行 6 秒未提前退出，1920×1080 FullScreen log 正常，掃描 `Unhandled`／`NullReferenceException`／`MissingReferenceException`／`error CS`／`Aborting` 為 0 hits，之後由驗證流程關閉。
- Acceptance：
  - 主選單有中文「離開遊戲」：`PASS` — `DrawMainMenu` 實際按鈕已加入。
  - Windows Player 正常 quit path：`PASS` — non-Editor build 編譯 `Application.Quit()` 分支，Windows build／launch 成功。
  - Unity Editor 不被整個關閉：`PASS` — compile-time Editor 分支使用 `ExitPlaymode()`。
  - 離開／返回主選單前清理 session：`PASS` — `QuitNow`／`ReturnToMenuNow` 都先呼叫 `DisposeSession`；scene regression 確認 Composition 已釋放。
  - 實際人工點擊 Windows Player 按鈕：`NOT RUN` — automated run 驗證 build／branch／cleanup，但本輪未用桌面滑鼠自動化代替使用者操作。
- Completed：主選單與 legacy menu 均有離開入口；Player／Editor lifecycle、cleanup、tests、文件與最終 Windows build 已完成。
- Not Completed／Deferred：未做退出確認或未存檔警告；未直接以 GUI automation 點擊 final Player 按鈕，留給專案擁有者做一次人工 UI 驗收。
- Known Issues／Risks：目前按「離開遊戲」立即退出，尚未確認玩家是否有未儲存進度；正式產品加入 autosave／dirty-state 後應先顯示確認視窗。IMGUI 的按鈕 label 沒有可由無頭 Test Runner 查詢的 semantic tree，因此自動測試驗 lifecycle 與 source wiring，視覺存在仍需人工看一眼。
- Git：Branch `main`；9 個 status entries，包含前一筆尚未提交的 display adapter／docs，以及本次 application adapter／Bootstrap／tests／progress；未 commit／push。Unity build 自動重寫的 Scene fileIDs、URP／Volume、batching 與 Unity Connect 設定已還原；`git diff --check` PASS。
- Next：
  1. 啟動 `MainMenuQuitFinalBuild/PlayablePrototype_01.exe`，從遊戲返回主選單後按「離開遊戲」，確認程式正常關閉。
  2. 決定正式版本離開前是否要「未儲存進度」確認；目前 Prototype 採立即退出。
  3. 人工 UI 驗收完成後再進玩家守城／AI 正式攻城／維修成本或 Infantry L2.1。

## 2026-08-12 — 原生解析度全螢幕與滑鼠滾輪相機縮放

- Status：Completed（自動化與 Windows Player 啟動驗證完成；實體滑鼠主觀手感待專案擁有者確認）
- Goal：讓 Windows 遊戲啟動時使用目前主螢幕相同解析度的全螢幕畫面，並保證滑鼠滾輪能雙向縮放固定上帝視角。
- Baseline：
  - Branch／HEAD：`main`／`1048f1aa5cf3c0feb5e7fde6fe5696f19fe1d9e4`；開始時工作樹乾淨。
  - `ProjectSettings.asset` 已有 `defaultIsNativeResolution: 1` 與 `fullscreenMode: 1`，但 Prototype 啟動流程沒有 runtime display adapter，也沒有 Player log 能證明它實際向哪個顯示器尺寸提出要求。
  - `UnityRtsInputAdapter` 已把 `<Mouse>/scroll` 的 Y 值傳給 `RtsCameraController.ProcessInput`，controller 也會呼叫 `RtsCameraRigModel.ZoomBy`；操作文件已有滾輪說明，但沒有直接鎖定方向與距離變化的 regression test。
- Scope：
  - In：Windows Player 原生解析度無邊框全螢幕、Editor Game View 不受強制切換、runtime display diagnostics、滾輪向上拉近／向下拉遠、2.5～40 m clamp 沿用、設定面板與操作／畫面規格文件、targeted／full tests、Windows build／launch。
  - Out／Deferred：獨佔式 `ExclusiveFullScreen`、解析度選單、多螢幕選擇、UI scale slider、自由旋轉相機、修改 zoom 範圍、實體滑鼠主觀靈敏度調校。
- Changed：
  - `PrototypeDisplayAdapter.cs`：新增 Demo presentation adapter；Player 讀 `Display.main.systemWidth/systemHeight`，無有效值時退回 `Screen.currentResolution`，再呼叫 `Screen.SetResolution(..., FullScreenMode.FullScreenWindow)`。Editor 只回報 Game View 尺寸，不搶占桌面全螢幕。
  - `PlayablePrototypeBootstrap.cs`：composition root 在建立 camera／session 前套用 display policy；顯示設定面板增加目前 display summary。
  - `PlayablePrototypePlayModeTests.cs`：新增 `DisplayAndMouseWheel_UseNativeFullscreenPolicyAndZoomBothDirections`，驗證主顯示器尺寸優先、fallback，以及 Windows 常見 ±120 scroll delta 對 20→17→20 m 的雙向 zoom。
  - `docs/ArtSpecs/02...解析度相機與安全區.md`：明定 Player／Editor 行為、主顯示器尺寸來源、fallback 與 `<Mouse>/scroll` data flow。
  - `docs/37...操作與驗收手冊.md`：補原生解析度全螢幕啟動、設定面板顯示、滾輪方向、Player log 與 `Alt+Enter` 說明。
- Behavior：
  - Before：只依賴 Player Settings 預設值；不同啟動環境是否仍以桌面解析度全螢幕不易確認。滾輪程式路徑存在，但方向與縮放量沒有 regression。
  - After：Windows executable 每次進入 Prototype 都以主顯示器原生尺寸要求 `FullScreenWindow`；本機實際回報 1920×1080。Unity Editor 維持使用者設定的 Game View。滾輪向上拉近、向下拉遠，距離由 `RtsCameraRigModel` 限制在 2.5～40 m。
- Architecture／API／Data：
  - Architecture：Display 屬 Demo presentation／platform adapter，只由 Bootstrap composition root 呼叫；不進 Gameplay、不建立 God Manager，也不影響 Player／AI Command、simulation tick 或 save state。
  - API：新增 Demo public static `PrototypeDisplayAdapter.ResolveNativeSize`、`ApplyNativeFullscreen` 與 read-only `LastSummary`。Package Framework public API 未改。
  - Data：Content Pack、Scenario JSON、Save schema、Input Action asset格式均未改；既有 runtime action map 的 `<Mouse>/scroll` binding 保留。
- Tests／Validation：
  - 首次 `dotnet restore`＋build：FAIL，2 errors；Unity 尚未重新產生 `.csproj`，新 `PrototypeDisplayAdapter.cs` 不在 Compile 清單。以 Unity 6000.5.7f1 batch import 更新 generated project 後修正；不是 runtime／namespace defect。
  - Unity batch import：PASS，無 `error CS`／`Compilation failed`，正常 exit。
  - 第二次 `dotnet restore`＋`dotnet build --no-restore`：PASS，0 warnings／0 errors。
  - Targeted PlayMode `DisplayAndMouseWheel_UseNativeFullscreenPolicyAndZoomBothDirections`：PASS 1/1；result `Logs/DisplayWheelTargeted.xml`。
  - Full EditMode：PASS 177/177；0 failed／0 skipped；result `Logs/DisplayFullEditMode.xml`。
  - Full PlayMode：PASS 35/35；0 failed／0 skipped；result `Logs/DisplayFullPlayMode.xml`。
  - Windows Development Build：PASS；BuildReport 186,825,517 bytes；輸出 332 files／187,182,121 bytes；log `Logs/DisplayWindowsBuild.log`。
  - Windows Player launch：PASS；不傳 `-screen-width`／`-screen-height`／windowed override，process 執行 7 秒未提前退出；Player log 實際輸出 `[PlayablePrototype Display] Native Fullscreen · 1920×1080`，之後由驗證流程關閉。
- Acceptance：
  - 主顯示器原生解析度：`PASS` — runtime 讀到本機 1920×1080 並寫入 Player log。
  - 無邊框全螢幕：`PASS` — runtime 明確呼叫 `FullScreenMode.FullScreenWindow`，既有 Player Settings 同為 native/fullscreen。
  - Editor 不被強制全螢幕：`PASS` — `Application.isEditor` 分支只回報 Game View。
  - 滾輪向上拉近、向下拉遠：`PASS` — targeted test 驗證 20→17→20 m。
  - Zoom 不超過 2.5～40 m：`PASS` — 沿用既有 model clamp 與既有 camera tests。
  - 實體滑鼠在使用者桌面上的主觀靈敏度：`NOT RUN` — automated test 能驗 data flow／數值，無法代替使用者手感。
- Completed：Player display policy、diagnostics、滾輪方向契約、完整 regression、Windows build／launch與兩份規格文件均已完成。
- Not Completed／Deferred：未加入遊戲內解析度／螢幕選擇器；未改成 exclusive fullscreen；未調整每格滾輪的 3 m 靈敏度，等使用者實際試玩回饋再做。
- Known Issues／Risks：`Display.main` 只代表主顯示器；多螢幕選擇尚未提供。無邊框全螢幕採桌面 refresh rate／系統顯示模式，不會切換獨佔解析度。`Alt+Enter` 可暫時切換，但下次啟動仍依規格回到原生解析度全螢幕。
- Git：Branch `main`；結束時 7 個 tracked/untracked status entries（display adapter＋meta、Bootstrap、PlayMode test、2 份 docs、`DevelopmentProgress.md`）；未 commit／push。Unity build 自動重寫的 Scene fileIDs、URP／Volume、batching 與 Unity Connect 設定已還原；`git diff --check` PASS。
- Next：
  1. 專案擁有者直接啟動最新 `PlayablePrototype_01.exe`，確認畫面覆蓋 1920×1080 螢幕，並滾輪上下各操作 5 次確認方向與速度。
  2. 若每格 3 m 太快／太慢，只調 `RtsCameraController.zoomSpeed` 並重跑 targeted test，不改 zoom 邊界。
  3. 顯示與相機手感接受後，回到玩家守城／AI 正式攻城／維修成本或 Infantry L2.1 的優先級選擇。

## 2026-08-12 — Infantry L3 人物細節、材質與 Unity 軸向修正

- Status：Completed（現有 L2 外觀的 L3 保真修正；L1 概念圖等級的正式角色製作為 Deferred）
- Goal：處理「L1／L2 看起來不錯，但加入動畫後人物細節和動作出現大量問題」的回報；先確認問題來自模型資料、Rig、Unity 匯入、材質、燈光或鏡頭，再讓 L3 在不重新設計角色的前提下忠實呈現 L2。
- Baseline：
  - Branch／HEAD：`main`／`7ab232d Add the infantry L2`；工作樹包含前一輪尚未提交的 L3 Blender／FBX／Animator／相機修正，均保留。
  - L1 是精修概念圖；L2 報告明載其為程式化 `production-blockout / low-poly L2`，LOD0 為 4,376 triangles、Flat Normal、簡單 atlas，不是 L1 等級的人工雕模。L3 沒有降 LOD0 triangle count，但 Unity 呈現仍明顯比 L2 差。
  - 實際近距離 Front／Side／Back capture 揭露 L3 visual child 被 Prefab builder 重設為 identity rotation；Blender Z-up／-Y-forward 網格在 Unity 幾乎沿地面躺倒。舊高角度相機掩蓋了錯誤，也是移動方式看起來不合理的主要原因。
  - `PrototypeUnitArtView` 與 `UnitySelectableView` 以 Renderer 為單位寫 `_BaseColor`，使盾牌 Base 木／鐵 slot 一起被隊伍色或選取色覆蓋；空場景也沒有 Directional Light。2.5 m 鏡頭仍維持 55°，臉與胸甲被俯視壓縮。
- Scope In：保留 L2 LOD0 幾何／UV／貼圖、Unity basis 與落地修正、站立 bounds gate、Base／Normal／ORM importer、材質槽級 Team Color／selection、規格藍紅色、燈光、近距離 inspection pitch、三方向／四 Move pose capture、完整 regression、文件與 Windows build。Scope Out：重新設計角色、提高 triangle budget、重畫 L1 等級貼圖、人工雕模／重拓樸、自由旋轉相機、其他兵種、gameplay 規則與數值。
- Changed：
  - `InfantryL3PrefabBuilder` 明確把 imported visual 旋轉 `X=-90°`，轉成 Unity Y-up／Z-forward，再按 combined renderer `bounds.min.y` 對齊 gameplay ground；新增 standing orientation gate，拒絕身高低於 1.65 m、Y 不是身體主軸、沉入地面或超出 1.95 m 的 Prefab。
  - BaseColor 設 sRGB；Normal 設 NormalMap／linear；ORM 與 TeamColor Mask 設 linear；全部開 mipmaps、1024 max size、驗收階段不壓縮。Base material 接回 BaseMap／NormalMap；Team material 保持單一白底、GPU instancing。
  - `PrototypeUnitArtView.ApplyTeamColor` 與 `UnitySelectableView.SetSelected` 改為逐 material slot 寫 MaterialPropertyBlock；只有 material name 含 `TeamColor` 的 slot 會收到隊伍／選取色，Base 木盾、鐵邊與人物本色不再被覆蓋。沒有命名 contract 的舊 placeholder 才使用 renderer-wide fallback。
  - Bootstrap 使用精確我方 `#4AA3D8`／敵方 `#D94A45`，並建立暖中性 Directional Light、soft shadows 與 trilight ambient；這只改善 URP Lit 可讀性，不改資產幾何。
  - `RtsCameraController` 在 2.5～8 m 將 pitch 由 55°漸變到 38°，配合既有 body-height focus；Yaw 仍鎖定，保持不可旋轉的固定上帝視角。
  - 新增近距離 detail regression：Front／Side／Back、Idle 與 Move 0／25／50／75% pose；測試同時檢查站立 bounds、in-place drift、材質槽 tint、燈光與 camera pitch，避免「技術上有 Animator」卻視覺躺倒再次通過。
- Behavior Before／After：Before 為人物 visual 躺倒、盾面整片被隊伍色染掉、Lit 材質缺光且近距離仍過度俯視；After 為同一套 L2 4,376-triangle 外觀在 Unity 正確直立落地，木盾 Base 與藍／紅 team panel 分離，頭盔、臉、札甲、圍巾、盾、劍和四肢能在 2.5 m／38° 檢查，Move 四個相位維持站立且 gameplay root 不漂移。
- Architecture／API／Data：
  - Definition／Runtime／View 分層不變；修正都在 Editor import/build 與 Presentation／Demo view。Combat、Movement、Player／AI Command、Content Pack、Save schema 均未改。
  - `UnitySelectableView` public API 未變；只收窄 `SetSelected(bool)` 寫色範圍。`PrototypeUnitArtView` public API 未變。
  - `RtsCameraController` 新增 private serialized `closeInspectionPitch=38`；既有 camera model 與 public API 未改。
  - 沒有新增世界觀 hardcode；隊伍色仍由 runtime presentation 套用同一套幾何。
- Files／Assets：主要修改 `InfantryL3PrefabBuilder.cs`、`PrototypeUnitArtView.cs`、`UnitySelectableView.cs`、`PlayablePrototypeBootstrap.cs`、`RtsCameraController.cs`、`PlayablePrototypePlayModeTests.cs`、Infantry materials／texture import metadata／Prefab、`ArtSource/.../v002/Documentation`、ArtSpecs 與操作手冊。
- Tests／Validation：
  - Unity orientation builder attempt 1：FAIL；只保留 imported rotation 時 bounds size=`(1.85, 1.40, 1.88)`，Y 仍不是主軸。
  - Unity orientation builder attempt 2：FAIL；明確 `X=-90°` 後直立，但未 ground-align，bounds Y=`-1.367…0.508`。
  - Unity orientation builder attempt 3：PASS；`X=-90°`＋combined bounds ground alignment，Humanoid `isHuman=True`／`isValid=True`、clips=5、Prefab 完成。
  - 新增 test 首次編譯：FAIL，遺漏 `System.Collections.Generic` 導致 `IReadOnlyList` 找不到；補 using 後修正。
  - Targeted close detail／material／camera test：PASS 1/1；Move 四 phase standing bounds test：PASS 1/1；真實場景 movement test：PASS 1/1。
  - Blender 5.2 manifest-only rebuild：PASS；正式交付 26 files，SHA-256 26/26 相符。
  - `dotnet restore`＋`dotnet build --no-restore`：PASS，0 warnings／0 errors。先前單獨 `--no-restore` 因 Unity 清除 `Temp/obj` 得到 NETSDK1004，按正確 restore 流程重跑後通過。
  - Full EditMode：PASS 177/177；Full PlayMode：PASS 34/34。
  - Game View smoke：PASS；2 infantry、LOD、Team Color、anchors、valid Humanoid Avatar；輸出 `C:/projects/Unity/AegisRTS.BuildValidation/Infantry_GameView.png`。
  - Windows Development Build：PASS，BuildReport 186,823,929 bytes；輸出 331 files／187,176,482 bytes。`PlayablePrototype_01.exe` 實際啟動 5 秒仍正常執行，再由驗證流程關閉。
- Acceptance：
  - `[PASS]` L3 使用 L2 的 LOD0 4,376 triangles／UV／貼圖，沒有以較低階 Mesh 取代近距模型。
  - `[PASS]` Unity 中人物 Y-up 直立、腳底落地，Move 四個 phase 不倒下、不大幅 planar drift。
  - `[PASS]` 只有 TeamColor slot 接受 `#4AA3D8`／`#D94A45`；盾牌木／鐵 Base 保留。
  - `[PASS]` BaseColor／Normal／ORM import 與 URP Lit lighting 生效；2.5 m close inspection 使用約 38° 且 Yaw 鎖定。
  - `[PASS]` 完整 tests、Game View、Windows build／launch。
  - `[DEFERRED]` 由 L2 blockout 升級到 L1 概念圖的雕模、重拓樸、手繪材質、更多甲片／布料結構與 final animation polish。
- Completed：已找出並修正真正的 Unity 軸向、落地、材質覆蓋、燈光與鏡頭問題；增加可自動阻擋同類錯誤的 bounds／pose／material tests，並更新完整交付與操作文件。
- Not Completed／Deferred：目前模型仍是收到的 L2 程式化 low-poly blockout；沒有憑空生成 L1 圖中未存在於 Mesh／texture 的護肩分層、綁腿、盾牌木紋、五官雕刻等細節。若使用者所說的「L1 精細度」是字面要求，需要另立 L2.1 正式角色美術任務。
- Known Issues／Risks：現有近距離畫面能忠實暴露 L2 的 blockout 感，這是來源資產限制而非動畫降模；固定上帝視角仍不能像模型檢視器自由旋轉；deterministic Move 已有 pose／drift gate，但 professional foot-lock、重量感和 shield-leg intersection 仍需人工美術審查。
- Git：Branch `main`；32 個 tracked/untracked status entries，包含同一批尚未提交的 L3 工作；本任務未 commit／push。Unity build 自動重寫的 Scene fileIDs、URP prefilter／Volume、batching 與 Unity Connect 設定已逐項還原；`git diff --check` PASS（只有既有 LF→CRLF 提示）。
- Next：
  1. 專案擁有者在 `PlayablePrototype_01` 選取步兵，按 `F` 後滾輪放大到 2.5～4 m，檢查直立、木盾、胸甲與連續 Move。
  2. 若接受現在就是 L2 的預期外觀，停止在步兵上追加 patch，回到玩家守城／AI 正式攻城／維修成本等系統工作。
  3. 若要求真正接近 L1 概念圖，建立 `CHR_Infantry_A_v003`／L2.1 正式美術任務：以 L1 正側背為造型基準，人工雕模／重拓樸／UV／PBR atlas，再沿用現有 Humanoid、動畫與 Unity validation pipeline。

## 2026-08-12 — Infantry L3 移動修正與近距離檢視

- Status：Completed（prototype correction；使用者最終視覺接受與 professional animation polish 仍為 gate）
- Goal：針對「人物移動方式完全不對」的回報，用 `PlayablePrototype_01` 真實移動命令重現並修正滑行／僵硬步態，同時讓固定上帝視角可縮放到足以檢查單位建模的距離。
- Baseline：
  - Branch／HEAD：`main`／`7ab232d Add the infantry L2`；工作樹已有本次尚未提交的 L3 整合與文件，全部保留並在其上修正。
  - 實際 capture 顯示 GameplayRoot 方向正確，但舊 Move 約 0.87 秒、腿部幅度低且上半身近乎不動；遊戲以 4.5 m/s 平移時產生明顯 moonwalk／滑行感。相機最小 zoom 8 m，也不足以檢查頭盔、裝甲、盾牌與武器。
  - 原有 automated L3 test 只驗 Avatar／events／Root Motion，無法判斷連續畫面的動作品質；這是前一筆技術驗收過早宣告視覺完成的缺口。
- Scope In：Move 動作重建、步頻與真實速度同步、實際場景移動 capture、2.5–40 m zoom、close-inspection framing、操作文件、交付 metadata／manifest、回歸測試與 Windows build。Scope Out：自由旋轉相機、改 gameplay movement speed/pathfinding、專業 mocap／手工動畫 polish、重新建模、其他兵種與地圖美術。
- Changed：
  - `build_unit03_l3_blender.py` 將 Move 重建為 0–24 frames 的 grounded stride：左右 heel contact、左右 passing pose、34° thigh stride、passing／trailing knee、foot articulation、hips／chest counter-twist，以及受控盾牌／短劍手臂動作；Root 仍完全不設 key。
  - 用 Blender 5.2 `--factory-startup` 重建 `.blend`、master FBX 與五個 animation FBX；Move 事件改到實際接觸 frames 1／13，並把 locomotion frame range、poses、4.5 m/s reference speed 與 1.8 reference rate 寫入 `BUILD_RESULT.json`。
  - 新增 Animator `MoveRate`，`PrototypeUnitAnimatorView.Refresh` 接收 authoritative `MovementStateSnapshot.Velocity` 換算的 world speed；4.5 m/s 使用 1.8 倍 clip rate，其他速度 clamp 0.65～2.4。`Speed` 仍只控制 Idle／Move，Root Motion 仍關閉。
  - Playable camera model 改為 2.5～40 m；2.5～8 m 逐步把 pivot 抬到角色身體高度，近距離不會只對著地面。相機 pitch／yaw 固定，保持原本不可旋轉的 2.5D／上帝視角設計。
  - 教學與操作文件補上「滾輪縮放 2.5～40 m」與 `F` 聚焦選取；新增真實場景 PlayMode test，可選擇輸出八張 960×540 movement review frames。
- Behavior Before／After：Before 為角色以正確方向在世界座標平移，但腿部低幅度慢循環造成滑行感，最接近只能看 8 m；After 為腿／膝／腳左右交替並有軀幹 counter-motion，clip rate 跟隨實際速度，單位仍由 gameplay movement 擁有位置；選取單位後按 `F` 並滾輪放大可到 2.5 m 查看模型。
- Architecture／API／Data：
  - Gameplay movement／combat truth 未移入 Animator；Bootstrap 只把 snapshot 投影到 presentation view，Player／AI Command、Content Pack schema、Save schema 均未改。
  - `PrototypeUnitAnimatorView.Refresh` presentation API 由 `(bool, CombatantSnapshot)` 擴充為 `(bool, double worldSpeed, CombatantSnapshot)`；Animator Controller 新增 float `MoveRate`。
  - `RtsCameraController` 只新增 private serialized close-inspection focus height；既有 `RtsCameraRigModel` public API 未改，prototype 初始化改用其既有 min／max 參數。
- Files／Assets：主要修改 `PlayablePrototypeBootstrap.cs`、`PrototypeUnitAnimatorView.cs`、`RtsCameraController.cs`、`InfantryL3PrefabBuilder.cs`、`PlayablePrototypePlayModeTests.cs`、重建後 Infantry `.blend`／FBX／Prefab、L3 Documentation／ArtSpecs、`docs/37...操作與驗收手冊.md`。
- Tests／Validation：
  - Blender clean rebuild：PASS，exit 0；fatal pattern 0；LOD0／1／2 = 4376／1512／542；manifest 26/26 hashes 相符。Blender 5.2 對 `Material.use_nodes` 有兩個未來棄用 warning，不影響本版輸出。
  - Unity L3 prefab builder：PASS；Humanoid `isHuman=True`／`isValid=True`，clips=5，Prefab 重建成功。
  - Actual movement＋close zoom targeted PlayMode：PASS 1/1；真實命令位移 >1 m、面向與位移方向 dot >0.98、Animator Speed >0.5、zoom=2.5 m；八張畫面位於 `C:/projects/Unity/AegisRTS.BuildValidation/InfantryMovementReview/Move_00.png`～`Move_07.png`，逐格檢查可見 contact／passing 與左右腿交替。
  - 初次 final capture 指令：FAIL 0/1，原因是同時指定 `-nographics` 和 RenderTexture capture；移除 `-nographics` 後同一測試 PASS。此失敗是驗證環境衝突，不是產品路徑失敗。
  - `dotnet restore`＋`dotnet build --no-restore`：PASS，0 warnings／0 errors。
  - Full EditMode：PASS 177/177；Full PlayMode：PASS 33/33。
  - Game View smoke：PASS，2 infantry、Humanoid Avatar、LOD／Team Color／anchors；`C:/projects/Unity/AegisRTS.BuildValidation/Infantry_GameView.png`。
  - Windows Development Build：PASS，BuildReport 177,551,533 bytes；輸出 341 files／178,697,614 bytes。Player 以 960×540 視窗實際啟動、取得 input idle，3 秒後仍正常執行，再由驗證流程關閉。
- Acceptance：
  - `[PASS]` 真實場景不是反向／側向滑動；GameplayRoot 朝實際行進方向。
  - `[PASS]` Move 有左右接觸與 passing poses，並按 4.5 m/s reference 同步步頻。
  - `[PASS]` Root Motion Off，動畫不改寫 movement truth。
  - `[PASS]` 可縮放至 2.5 m 並把單位身體置於 close-inspection framing；`F` 可先聚焦選取。
  - `[PASS]` 完整 tests、Game View、Windows build／launch。
  - `[DEFERRED]` professional animator 的 foot-lock、weight、secondary motion 與 final-art polish；不冒充 production animation。
- Completed：已重現原始視覺缺陷、重建可再生 locomotion asset、同步 runtime rate、加入 close zoom、補 capture regression、文件與完整建置驗證。
- Not Completed／Deferred：使用者仍需在自己螢幕上確認主觀動作手感；未加入自由旋轉／模型展示模式；未調整 gameplay 4.5 m/s balance；未導入 mocap 或專業手工動畫。
- Known Issues／Risks：固定 55° 上帝視角在最接近時仍會壓縮腿部深度，這是維持不可旋轉 RTS 視角的取捨；近距離血條會靠近畫面上緣；deterministic prototype 動畫已消除明顯滑行，但仍不是 commercial final-quality locomotion。Blender 6 前需更新 `Material.use_nodes` 已棄用用法。
- Git：Branch `main`；26 個 tracked/untracked status entries；本任務未 commit／push。Unity build 自動重寫的 Scene fileIDs、URP prefilter、Volume、batching 與 Unity Connect 設定已逐項還原；`git diff --check` PASS（僅 `.gitignore` LF→CRLF 提示）。
- Next：
  1. 在 Unity 開 `PlayablePrototype_01`，選步兵後按 `F`、滾輪放大到 2.5～4 m，連續走 5～10 秒確認主觀節奏；若仍不接受，請指出是腳步、速度、身體重心、盾劍或轉向，我會只針對該層迭代。
  2. 若此 prototype locomotion 接受，先回到系統優先：玩家守城、AI 正式攻城、維修資源成本。
  3. 下一個美術垂直切片再做弓兵 L3＋projectile／命中 placeholder，沿用本次實際場景 capture gate。

## 2026-08-12 — Infantry L3 Blender 建置與 Unity Humanoid 整合

- Status：Completed（Prototype L3 technical acceptance；Commercial／final-art release 尚有 gate）
- Goal：接收 `Unit_03_Infantry_L3_v002_CORRECTED`，在已安裝 Blender 的環境補齊真實 BLEND／FBX，整理到 `ArtSource` 與 Unity Runtime 邊界，完成 Humanoid、五段動畫、事件、LOD2、Team Color、Prefab 與遊戲實際驗證。
- Baseline：
  - Branch／HEAD：`main`／`7ab232d Add the infantry L2`；原工作樹已有 L2／L3 規格與文件變更，均保留。
  - Incoming：19 個來源檔，含 v001 GLB／貼圖、Blender Python 與 blocked 文件；沒有實體 `.blend`／`.fbx`，腳本在 Blender 5.2 首次執行因 `Action.fcurves` API 變更出錯，且文件聲稱的 LOD2 未實作。
  - Runtime：`PF_Unit_Infantry` 為靜態 L2，只有 LOD0／LOD1；Bootstrap 沒有 Animator presentation bridge，Idle／Move／Attack／Hit／Death 不會播放。
- Scope In：來源包分類、Blender 5 相容修正、LOD2、A-Pose Humanoid、rigid shield／sword、五段 In-Place 動畫、事件、Team Color Mask、FBX／BLEND、Unity Importer／Avatar／Controller／Prefab、presentation 串接、tests、smoke、Windows build、交付／授權／checksum 文件。Scope Out：重新設計角色、修改 authoritative combat damage、正式手工動畫 polish、其他兵種、美術來源權利的最終法律確認。
- Changed：
  - 將收到的包移至 `ArtSource/Units/Infantry/CHR_Infantry_A/v002/`，保留 `Input_v001`、Reference、Documentation 與可重建 Source；Runtime 只複製 master／animation FBX 與 Team Color Mask 至 `Assets/AegisRTS/Content/Shared/Art/Units/Infantry/`。
  - 修正 `build_unit03_l3_blender.py` 的 Blender 5 Action API、實作 LOD2 deterministic decimation、保留盾牌 Team material slot、加入 gameplay anchors／bone sockets、依 action frame range 匯出、補 UTF-8 Prompt／build metadata／SHA-256 manifest；加入 `--factory-startup` build 流程。
  - 實際用 Blender 5.2.0 LTS 生成 `CHR_Infantry_A_v002.blend`、master FBX 與 Idle／Move／Attack_A／Hit／Death 五個 FBX；三角面為 4,376／1,512／542，30 FPS，A-Pose，Root 不設 key。
  - 新增 `InfantryL3PrefabBuilder`：master 採 Humanoid／Create From This Model，五個 clip Copy From Other Avatar，建立 `AC_Infantry.controller`、URP materials、LODGroup、anchors、collider 與 L3 Prefab，並輸出 import warnings debug。
  - 新增 `PrototypeUnitAnimatorView`，Bootstrap 只把 authoritative movement／combat snapshot 投影成 Animator 參數；動畫事件只記錄 Footstep／AttackImpact／DeathSettled 視覺時序，不改 gameplay damage truth。死亡先播放後延遲移除 view。
  - 修正 Unity importer event contract：`ModelImporterClipAnimation.events.time` 必須使用 normalized 0～1，而非秒；當時 Move 為 4/24、17/24，Attack 13/30，Death 35/38；後續滑行修正版已把 Move contact events 更新為 1/24、13/24。
  - `PlayablePrototypeSmokeValidation` 從 L2「renderer 數量剛好 2」改為驗證每個 L3 至少跨三層 LOD 提供 Team Color renderer；`.gitignore` 加入 Python／Blender cache。
- Behavior Before／After：Before 為靜態步兵模型隨 GameplayRoot 位移；After 為同一 v001 造型的藍／紅 Humanoid 步兵，依移動、戰鬥受擊與死亡播放五段 Animator state，Root Motion 關閉且 gameplay root 不被動畫改寫，盾劍跟隨左右手骨骼，LOD2 可在遠距使用。
- Architecture／API／Data：
  - Definition／Runtime／View 邊界保持不變；`PrototypeUnitAnimatorView` 位於 Demo presentation，`CombatSystem` 仍唯一擁有傷害與生命真相。
  - 新增的 public presentation surface 為 `PrototypeUnitArtView.AnimatorView` 與 animation event receivers；沒有修改 package gameplay API、Content Pack schema 或 Save schema。
  - Player／AI 仍透過共同 Command／Combat state；Animator Controller 不直接派送攻擊命令或扣血。
- Files／Assets：主要為 `ArtSource/.../v002`、Infantry `Models/Animations/Textures`、`PF_Unit_Infantry.prefab`、`PrototypeUnitAnimatorView.cs`、`PrototypeUnitArtView.cs`、`PlayablePrototypeBootstrap.cs`、`InfantryL3PrefabBuilder.cs`、Smoke／PlayMode tests 與 ArtSpecs／交付文件。
- Tests／Validation：
  - Blender clean build：PASS，exit 0 且 log scan 無 Traceback／AttributeError／Error／missing asset；Blender 5.2.0，LOD0／1／2 = 4376／1512／542，AttackImpact frame 13；清理後交付包為 26 個正式檔，manifest hash 26/26 相符。
  - Unity L3 builder：PASS；Avatar `isHuman=True`、`isValid=True`、5 clips、Prefab 成功。修正 normalized event 後 build log 不再產生新的 animation import warning。
  - L3 targeted PlayMode：PASS 1/1；驗證 Avatar、Animator、LODGroup、Footstep、AttackImpact、DeathSettled 與 root position 不變。
  - Full Unity EditMode：PASS 177/177；Full PlayMode：PASS 32/32。
  - `dotnet restore`＋`dotnet build --no-restore`：PASS，0 warnings／0 errors。
  - Game View smoke：PASS；2 個 Infantry、雙方 Team Color、LOD、anchors 可見，screenshot 為 `C:/projects/Unity/AegisRTS.BuildValidation/Infantry_GameView.png`。
  - Windows Development Build：PASS；`PlayablePrototype_01.exe` 667,648 bytes，輸出資料夾合計 332 files／178,096,464 bytes。
- Acceptance：真實 BLEND／FBX、基於 v001、A-Pose Humanoid、分離盾劍、五動畫、In Place／Root Motion Off、AttackImpact、單一 Team Mask、LOD2、Unity Valid Avatar、Prefab runtime 與機械性回歸測試 PASS；本紀錄當時未完成連續移動的 visual acceptance，後續由「Infantry L3 移動修正與近距離檢視」補正。
- Completed：來源包整理、DCC 重建、Unity runtime 整合、動畫事件時序修正、測試、build、完整交付與授權狀態文件。
- Not Completed／Deferred：人工 animator 對動作自然度的 final-art review；原 v001 生成來源與商用權利仍須專案擁有者保存／確認；弓兵／指揮官／建築尚未 L3。
- Known Issues／Risks：Unity `.meta` 仍保存第一次錯誤匯入的舊 `animationImportWarnings` 字串，但修正後的 importer log 無新 warning，event time 已是 35/38，且 1.25 秒內 DeathSettled runtime test 已通過；生成動畫仍需人工 final-art review。
- Git：Branch `main`；本任務未 commit／push；保留既有未提交文件與美術規格變更。最終 `git diff --check` 於交付前執行。
- Next：
  1. 由人工在 Unity Animator Preview／Game View 做 Idle、Move、Attack、Hit、Death 的視覺品質評分；只調 animation presentation，不改 combat truth。
  2. 以相同 L3 pipeline 製作弓兵，並先接 projectile／命中 VFX placeholder，建立遠程戰鬥完整視覺垂直切片。
  3. 商用發布前補齊／確認 v001 的生成 Prompt、工具、Job ID 與權利紀錄；通過後才把狀態改為 Production Accepted。

## 2026-08-12 — Infantry L3 骨架動畫交付規格

- Status：Completed（規格完成；L3 資產尚未製作）
- Goal：建立一份可直接交給 3D／動畫 AI 的步兵 L3 規格，確保其升級既有 v001 L2，而不是重新生成不同角色，並能穩定接回目前 Unity Prefab。
- Baseline：步兵 Prototype L2 已接入遊戲；現有 GLB 有 LOD0／LOD1 與 Team Color，但沒有 rig、skin、animation clips、LOD2、可編輯 DCC 原始檔或完整授權追溯。
- Changed：
  - 新增 `docs/ArtSpecs/Unit_03_步兵_L3骨架動畫交付規格.md`。
  - 完整鎖定 v001 的 1.80 m 尺寸、bounds、盾／劍、Pivot、軸向、材質、Team Color、anchors 與三角面預算。
  - 定義 Unity Humanoid 骨架、Root／Hips 契約、最多 4 weights、共用 Avatar／Bind Pose、rigid 武器盾牌與必要 sockets。
  - 定義 30 fps 的 Idle／Move／Attack_A／Hit／Death 長度、frame 區段、Loop、Root Motion 容差及 `Footstep_L/R`、`AttackImpact`、`DeathSettled` 事件。
  - 定義 LOD2、FBX 首選／GLB 條件式備選、Prefab hierarchy、Animator 參數、交付資料夾、事件 JSON、Unity screenshots、生成／授權紀錄與退件條件。
  - 更新步兵摘要、ArtSpecs 索引、製作流程與開發總覽連結。
- Behavior：N/A；本次只新增交付規格，未修改 runtime。規格明確要求動畫事件只表達視覺時機，不得在動畫資產內擁有傷害 truth。
- Tests / Validation：
  - 檢查文件包含五個必要 clips、Humanoid／Root Motion、LOD2、Team Color、Socket／Anchor、FBX／GLB、事件 JSON、授權、驗收與完整英文 Prompt：PASS。
  - Markdown 連結與 `git diff --check`：PASS。
  - Unity tests／build：NOT RUN；docs-only，不改 runtime／assets／package。
- Known Issues / Risks：
  - 規格完成不等於 L3 資產完成；目前遊戲中的步兵仍是無動畫的靜態 L2。
  - 若製作 AI 不能對既有 GLB 做 rig，必須回報 blocked，不能重新生成相似角色替代。
  - Humanoid L3 正式交付首選 FBX；只有 GLB 時仍須實際證明 Unity Avatar Valid，否則只能標記 Generic／Conditional。
- Git：Branch `main`；Commit／Push 未執行。
- Next：把 L3 規格連同 v001 LOD0／LOD1 Blue GLB、三張材質、L1 Concept Final 與 L2 Delivery Report 交給可編輯既有 mesh、Rig 並輸出 FBX 的工具或製作者。

## 2026-08-12 — Infantry GLB Prototype 整合

- Status：Completed（Prototype L2；Release／L3 尚未完成）
- Goal：在使用者完成 GLB 方案 A（安裝 glTFast）後，將步兵 LOD0／LOD1 套入 `PlayablePrototype_01`，完成單一 Team Color 流程、Content prefab ID、選取／血條與實機畫面驗證。
- Baseline：
  - Branch：`main`；工作樹已有尚未提交的 ArtSpecs、ArtSource 與文件變更，全部保留。
  - Package：使用者新增 `com.unity.cloud.gltfast` 6.19.0；Unity 能成功匯入四個 GLB。
  - Asset：四個 GLB 的紅／藍幾何相同；LOD0 4,376 triangles、LOD1 1,512 triangles，每個來源拆成 Base 與 Team Color 材質。沒有 rig、skin 或 animation clips。
- Scope：
  - In：Runtime L2 資產、LOD、URP materials、Team Color、Prefab、Content Pack／spawn 串接、朝向、選取與血條、tests、smoke tool 與文件。
  - Out／Deferred：骨架／動畫、正式 Normal／ORM、弓兵與其他單位、授權資料補齊、發布驗收。
- Changed：
  - 將 Blue LOD0／LOD1 與 BaseColor／Normal／ORM 複製至 `Assets/AegisRTS/Content/Shared/Art/Units/Infantry/`；Red 版本只保留於 `ArtSource` 追溯，避免 Runtime 重複幾何。
  - 新增 `InfantryArtPrefabBuilder`：把 GLB 的 106／52 個 mesh parts 合併成每 LOD 兩個 renderer，建立四個衍生 mesh、兩個 URP Lit material、LODGroup、anchors、collider 與 `PF_Unit_Infantry` Resources Prefab。
  - 新增 `PrototypeUnitArtCatalog`／`PrototypeUnitArtView`；Content Pack 的 `unit.infantry.prefabId` 改為 `PF_Unit_Infantry`，未知 ID 仍安全回退 primitive。
  - `PrototypeEntityRegistry` 保存 `prefabId`；Bootstrap 依 prefab ID 建立美術、套用雙方陣營色、使用 Prefab health-bar anchor，並讓單位朝移動方向旋轉。
  - `UnitySelectableView` 支援多個 LOD renderer，選取高亮不再只處理單一 renderer。
  - 新增 Unity menu `AegisRTS/Playable Prototype/Run Infantry Smoke Validation`，可在既有 Editor 中做 Play Mode runtime 驗證與截圖並回復原 scene。
- Behavior：
  - Before：所有步兵以 Capsule primitive 顯示，Content Pack 的 prefab ID 是 placeholder，沒有 LOD 或模型陣營色。
  - After：開局玩家與敵方步兵使用同一份靜態 L2 Prefab，分別呈藍／紅 Team Color，支援選取、移動轉向、血條與 LOD；英雄及其他兵種仍保持 placeholder。
- Architecture / API / Data：
  - Gameplay definition 只攜帶穩定 `prefabId`；Unity Resources 與材質處理留在 Demo／Content presentation boundary，package Gameplay 沒有新增 Unity dependency。
  - Team Color 與選取使用 `MaterialPropertyBlock`，不為每個單位複製 material。
  - Save schema 未變更；視覺由 restore 後 registry 的 definition／prefab ID 重建。
- Tests / Validation：
  - `dotnet restore AegisRTS.FrameworkLab.slnx` + `dotnet build ... --no-restore`：PASS，0 warnings、0 errors（修正 Unity 6 已棄用的 FindObjects overload 後）。
  - Unity Prefab builder：PASS；LOD0 4,376 triangles、LOD1 1,512 triangles、每 LOD 2 renderers。
  - Unity Editor Play Mode smoke：PASS；2 個 `PrototypeUnitArtView`、每個 2 個 Team Color renderers、LODGroup 與 Selection／HealthBar anchors 全部存在。
  - 實際 Game View screenshot：PASS，1920×1152，可見我方藍色與敵方紅色步兵及血條；輸出於 repository 同層 `AegisRTS.BuildValidation/Infantry_GameView.png`。
- Acceptance：
  - GLB 匯入、尺寸、Pivot、LOD triangles、合併 renderer、Prefab 載入、雙方生成、Team Color、selection／health anchors：PASS。
  - Runtime 仍能在其他 unit art 未交付時以 primitive fallback 啟動：PASS。
  - Production／Release asset：BLOCKED，缺少授權追溯、完整生成紀錄、正式材質與 L3 rig／animation。
- Known Issues / Risks：
  - 目前模型不含骨架與動畫；移動時只有 transform 轉向與位移，Idle／Move／Attack／Hit／Death 不會播放。
  - BaseColor 是色帶、Normal／ORM 是近乎均值的 placeholder；目前以 URP Lit 常數材質呈現，視覺品質不是最終版。
  - Resources catalog 目前只有步兵映射；後續單位應逐一新增，不應把產品資產引用放進 Framework package。
- Git：
  - Branch：`main`。
  - Commit／Push：未執行；本次使用者沒有要求。
- Next：
  1. 向美術 AI 取得同一造型、同一比例的 Rigged L3 檔，至少含 Idle／Move／Attack／Hit／Death clips 與武器 socket。
  2. 接 Animator Controller 與 combat／movement presentation events；再驗證移動、攻擊、死亡與 20～100 單位效能。
  3. 補齊生成工具、Prompt／Seed／Job ID、人工修改與商用授權後，才能將狀態由 Release Blocked 改為 Production Accepted。

## 2026-08-12 — Infantry AI 美術交付分類入庫

- Status：Completed（來源檔分類完成；Unity 匯入與遊戲整合未開始）
- Goal：將 Repository Root 的 `Unit_03_Infantry_Full_v001` AI 交付包移到可長期追溯的美術來源區，依概念、模型、貼圖、預覽、UV、文件與工具分類；避免尚未驗證的 GLB、預覽圖與 Editor script 直接進入 Unity `Assets`。
- Baseline：
  - Branch／Git：`main`；開始時前一筆 ArtSpecs／README／DevelopmentProgress 變更仍未 commit，另有未追蹤 `Unit_03_Infantry_Full_v001/`。
  - Incoming Delivery：25 files、4,201,771 bytes；包含 L1 兩張概念圖、L2 四個 GLB、五張材質貼圖、九張預覽／UV 圖、四份報告資料與一個 Unity Editor validator。
  - Import Capability：`Packages/manifest.json` 沒有 GLB／glTF importer；交付沒有 FBX、`.blend`／Maya 原始檔、L3 rig／animation 或完整 AI 生成／授權紀錄。
- Scope：
  - In：建立 `ArtSource` 規範、搬移並分類原始交付、建立單資產 Manifest、更新專案結構與 AI Art Pipeline 文件、驗證檔案完整性與 Git diff。
  - Out／Deferred：安裝 glTF importer、轉換 FBX、修改／啟用交付方 Validator、Unity Model Import、Material／Prefab、Team Color runtime、L3 骨架動畫、Content Pack／Bootstrap 串接與實機畫面驗收。
- Changed：
  - `ArtSource/README.md`：定義來源交付與 Unity Runtime Asset 分流、版本目錄與狀態詞彙。
  - `ArtSource/Units/Infantry/CHR_Infantry_A/v001/`：將 25 個原始交付檔分類至 `Concepts`、`Models`、`Textures`、`Previews/Camera`、`Previews/Dimensions`、`UV`、`Documentation`、`Tools/UnityEditor`。
  - `ArtSource/Units/Infantry/CHR_Infantry_A/v001/ASSET_MANIFEST.md`：記錄檔案分類、交付數據、阻塞原因、驗收狀態與進入 `Assets` 前的必要工作。
  - `docs/02_Project_Structure_完整目錄結構.md`：加入 `ArtSource` 與 `Assets/AegisRTS/Content/Shared/Art` 的責任邊界。
  - `docs/50_AI_Art_Pipeline.md`：補上 AI／外包來源檔的收件、驗收與 Runtime 分流流程。
- Behavior：
  - Before：完整 AI 包平放 Repository Root，L1／L2、Runtime 候選與驗收資料混在同一交付階層，且容易整包拖進 Unity。
  - After：來源包位於 `ArtSource/Units/Infantry/CHR_Infantry_A/v001` 並依用途分類；Unity `Assets` 沒有被未支援 GLB 或非 Runtime 圖片污染。
- Architecture / API / Data：
  - Architecture：新增 source-art boundary；`ArtSource` 不參與 Unity import／assembly，正式共用美術日後才進 `Assets/AegisRTS/Content/Shared/Art`。
  - API：N/A，未修改 C# API；交付方 `InfantryL2Validator.cs` 只保存於 `ArtSource`，不參與編譯。
  - Data：N/A，未修改 Content Pack、Prefab ID 或 Save schema。
- Tests / Validation：
  - Source inventory：搬移前 25 files／4,201,771 bytes；分類後排除新建 Manifest 仍為 25 files／4,201,771 bytes，PASS。
  - Classification scan：所有原始檔均落入指定分類，原 Repository Root 交付路徑已移走，PASS。
  - `git diff --check`：於本筆完成前執行。
  - Unity EditMode／PlayMode／Build：NOT RUN；本次沒有將資產放入 `Assets`、沒有 runtime 或編譯變更。
- Acceptance：
  - 建立合適來源資料夾：PASS — `ArtSource/Units/Infantry/CHR_Infantry_A/v001`。
  - 依檔案用途分類：PASS — 8 個明確分類與單資產 Manifest。
  - 原始交付無遺失：PASS — 數量與總 bytes 一致。
  - 不誤啟用未驗證資產：PASS — GLB 與 Validator 均保持在 `Assets` 外。
  - Unity 遊戲內顯示 Infantry：NOT RUN — 缺 importer／FBX 與後續整合，不在本次範圍。
- Completed：來源檔入庫、分類、狀態標記、專案規範與後續整合清單。
- Not Completed / Deferred：格式匯入決策、Unity 實測、單一 Team Color 網格／材質、L3、Prefab 與 Prototype 替換。
- Known Issues / Risks：四個 GLB 分為藍／紅重複網格，正式 Runtime 應改成單一幾何與可替換隊伍色；交付 Preview 是數學投影而非 Unity 截圖；授權與生成紀錄不完整，正式發布前必須補齊。
- Git：
  - Branch：`main`
  - Working Tree：`ArtSource/`、本筆文件與前一筆 ArtSpecs 仍為未提交變更。
  - Commit／Push：未執行；使用者本次未要求。
- Next：
  1. 向交付 AI 索取 FBX 或可編輯來源檔，以及完整 Prompt／Tool／Seed／License 紀錄；這比立即增加 importer 風險低。
  2. 決定 GLB importer 或 FBX 流程後，先匯入單一 LOD0／LOD1 候選並做 Unity 尺寸、Pivot、材質與 Game View 驗收。
  3. 驗收通過後建立共用 Infantry Runtime 資產與 Prefab，再處理 L3 動畫。

## 2026-08-12 — AI 可委派美術規格包

- Status：Completed（文件規格完成；正式美術資產與 Unity 整合未開始）
- Goal：建立可逐份交給其他 AI 的 RTS 美術製作規格，鎖定單位／建築世界尺寸、960×540 至 2560×1440 畫面適配、相機可讀性、材質／陣營色、動畫／VFX、輸出與驗收條件；保留後續世界觀替換空間。
- Baseline：
  - Branch／Git：`main`；開始時本任務尚無美術規格變更。
  - Existing Evidence：Playable Prototype 使用 Primitive Graybox；一般單位 NavMeshAgent 半徑 0.38 m、高 2.0 m；相機 Pitch 55°、Zoom 8–40、預設 31；Game View／IMGUI 讀取 `Screen.width/height`。
  - Player Settings：`fullscreenMode: 1`、`defaultIsNativeResolution: 1`、`resizableWindow: 0`、default fields 1024×768；全螢幕可採原生解析度，任意可調視窗尚未完成。
- Scope：
  - In：共同技術／視覺規格、解析度／Camera／安全區、尺寸總表、AI L1／L2／L3 交付、全部現有 Hero／Unit 美術原型的獨立規格（敵方指揮官第一版共用指揮官模型與換色）、fortified-city 建築、經濟／訓練預備建築、UI 肖像與圖示規格、製作批次與整合缺口。
  - Out／Deferred：生成圖片或 3D 模型、匯入 Unity、修改 PlayerSettings、Canvas/UI Toolkit 重構、依兵種 Agent 尺寸、Anchor 驅動血條、正式 Projectile／動畫／VFX／音效與世界觀美術。
- Changed：
  - `docs/00_README_開發總覽.md`：把交戰模式、選取驅動面板與 ArtSpecs 入口補入正式文件閱讀索引。
  - `docs/ArtSpecs/00_美術製作總覽與AI任務索引.md`：規格包入口、文件索引、交付層級與完成定義。
  - `docs/ArtSpecs/01_技術規格_比例座標與輸出.md` 至 `08_UI肖像圖示與選取標記.md`：公尺尺度、軸向／Pivot、網格／貼圖預算、相機／解析度、安全區、風格／隊伍色、動畫／VFX、AI 交付、尺寸總表、批次與 UI 配套規格。
  - `docs/ArtSpecs/Unit_01_指揮官.md` 至 `Unit_06_攻城兵器.md`：每個 Hero／Unit 的獨立 AI 任務文件、尺寸、輪廓、材質、動畫、Prompt 與驗收。
  - `docs/ArtSpecs/Building_01_我方主堡.md` 至 `Building_07_訓練營_預備.md`：目前要塞模式與未來建造模式的獨立建築規格。
- Behavior：
  - Before：只有通用 AI Art Pipeline／Art Bible 模板，沒有能直接約束模型大小、視窗可讀性或逐資產交付的文件。
  - After：可將單一 Unit／Building 文件獨立交給 AI；所有資產統一以 1 Unity Unit = 1 m 製作，並以 960×540、55° Pitch、60° FOV、31 m 作最低可讀性驗收。
- Architecture / API / Data：
  - Architecture：只新增文件；明確區分 `GameplayRoot`／NavMesh 碰撞與 `VisualRoot`，未修改 runtime ownership 或 assembly dependency。
  - API：N/A，沒有程式 API 變更。
  - Data：N/A，沒有修改 Content Pack 或 Save schema；文件中的 Asset ID 對應現有 `hero.*`、`unit.*`、`building.*`、`settlement.*` 與 `structure.gate`。
- Tests / Validation：
  - 文件一致性：檢查 ArtSpecs 索引、Asset ID、尺寸、解析度、Agent 與現有 ContentPack／Bootstrap／Camera／PlayerSettings 對照。
  - `git diff --check`：於本筆完成前執行；本次為 docs-only，不執行 EditMode、PlayMode 或 Windows Build，不改寫上次可信 runtime 測試結果。
- Acceptance：
  - 每個單位獨立 `.md`：PASS — 指揮官、副官、步兵、弓兵、騎兵、攻城兵器各一份。
  - 單位與建築公尺尺寸：PASS — 個別文件與 `06_尺寸總表.md`。
  - 當前視窗解析度適配規格：PASS — Camera 全視窗、UI reference／anchor、安全區與四種解析度驗收已定義。
  - 可直接交給其他 AI：PASS — 每份資產含自足摘要、技術條件、Prompt、禁止項與驗收。
  - 正式資產與遊戲整合：NOT RUN — 本次只建立規格，不生成或匯入資產。
- Completed：13 份資產獨立規格與 9 份共同／管理規格；優先 Batch A／B／C 已排序。
- Not Completed / Deferred：正式圖片／模型與程式整合；騎兵／攻城兵器較大 Agent；Prefab Anchor 血條；可調視窗與多比例實機驗收。
- Known Issues / Risks：其他 AI 若只產 PNG，只能算 L1；AI 生成 3D 仍需人工檢查拓撲、授權、尺寸與 Unity 匯入。現有所有單位共用 0.38 m Agent，不能直接以大體積騎兵／衝車模型宣稱整合完成。
- Git：
  - Branch：`main`
  - Working Tree：本任務新增 `docs/ArtSpecs/` 並修改 `DevelopmentProgress.md`，尚未 commit／push。
  - Commit／Push：未執行；使用者本次未要求。
- Next：
  1. 將 `Unit_03_步兵.md` 與 `Unit_04_弓兵.md` 分別交給 AI 先產 L1 四視圖與剪影。
  2. 人工確認輪廓後，再委派指揮官、城門、城牆、要塞主堡 L1。
  3. L1 通過後只挑步兵與弓兵進 L2，先完成 Unity 尺寸、Team Color 與 Projectile 整合，再擴大產量。

## 2026-08-12 — 選取內容自動切換指揮面板

- Status：Completed
- Goal：框選／點選不同類型物件後，指揮面板自動顯示對應操作；我方建築→內政、兵種／英雄→兵種設定、敵方建築→攻城行動。
- Baseline：`_commandTab` 只能由 HUD toggle 手動修改；Player City、Neutral Village、Fortress Gate、Fortress Stronghold 只是有 Collider 的 graybox marker，未註冊 `UnitySelectableView`，所以建築實際上不能進入 Selection model。
- Scope In：Selection revision、descriptor context resolver、建築／據點 selectable、selection-driven tab、混合框選優先、建築 selection projection、world command actor filtering、佔領後 affiliation 更新、測試、操作文件與實機 UI 驗證。Scope Out：正式建築 UI、各建築不同生產面板、多選群組詳細 inspector。
- Changed：新增 `SelectionCommandContext` 與 pure `SelectionCommandContextResolver`；`SelectionService.Revision` 僅在 selected set 實際改變時遞增；新增 `PrototypeCommandTab`，Bootstrap 在 `LateUpdate` 偵測 revision 後切頁，因此 selection 不變時玩家仍能手動看其他頁籤。
- Selection Rules：任一 Unit／Hero → UnitSettings；否則 Friendly Structure／Settlement → Domestic；否則 Enemy Structure／Settlement → Siege；Neutral-only／empty 保持目前頁；建築＋兵種混合框選由 UnitSettings 優先。
- World Integration：Player City、Neutral Village、Fortress Gate、Fortress Stronghold 都註冊正式 `UnitySelectableView`；敵方主堡佔領後透過 `SetAffiliation` 更新為 Friendly。Command dock 與 `PrototypeHudAdapter` 現在能顯示建築 definition、owner、defense 或 descriptor。
- Command Safety：`UnityRtsInputAdapter` 從 selection 建立 actor list 時只接受 Friendly Unit／Hero；建築仍可選取，但不會收到 Move／Stop／Hold／context Attack 等兵種指令。
- Architecture：Selection truth 與 revision 仍由 pure Presentation `SelectionService` 擁有；resolver 只讀 `ISelectionQuery` descriptor，不用 GameObject name 或 gameplay hardcode；Bootstrap 只做產品 tab enum 映射，未修改 Gameplay authoritative state 或 Save format。
- Tests：`RtsInputSelectionCameraTests` 新增 revision 與 Domestic／UnitSettings／Siege／mixed priority cases；PP06 PlayMode 驗證 4 個建築 selectable、三類自動切頁、混合框選優先、姿態 HUD click 保留 selection。
- Validation：`dotnet build AegisRTS.FrameworkLab.slnx` PASS（0 warning／0 error）；targeted EditMode PASS 11/11；targeted PP06 PlayMode PASS 1/1；完整 EditMode PASS 177/177；完整 PlayMode PASS 31/31；Windows Development Build PASS（173,603,484 bytes）；960×540 executable 實際點選 Player City 顯示「我方主堡」並切到內政，點選 Player Hero 顯示「指揮官」並切到兵種設定，點選 Enemy Stronghold 顯示「敵方主堡」並切到攻城行動；證據 `selection_context_city.png`、`selection_context_unit2.png`、`selection_context_enemy_building.png`；Player log 0 error hits；`git diff --check` PASS。
- Known Issues：Neutral-only 選取刻意保持目前頁籤；混合框選目前只選一個主要 command context，尚未提供 split inspector；敵方一般 Unit 會進入兵種設定作為資訊情境，但我方專用按鈕不會接受它。
- Files：`SelectionService.cs`、`UnitySelectableView.cs`、`UnityRtsInputAdapter.cs`、`PlayablePrototypeBootstrap.cs`、`PrototypeHudAdapter.cs`、Selection EditMode／Prototype PlayMode tests、docs 37／38／44、`DevelopmentProgress.md`。
- Git：未 commit、未 push；保留所有既有未提交工作與 `AGENTS.md`。
- Next：下一步可將 Domestic 再依主堡／資源建築／訓練建築分成各自 action set；正式 UI 應改用同一 `SelectionCommandContextResolver`，不要在 Canvas／UI Toolkit 另寫分類規則。

## 2026-08-12 — 兵種四種交戰模式與自主追擊

- Status：Completed
- Goal：依需求加入堅守陣地、普通、攻擊、反擊四種兵種防守模式；前三種主動攻擊與追擊，反擊模式只有受擊後才鎖定攻擊者。
- Baseline：`CombatSystem` 只接受既有 `AttackTargetCommand`，沒有自主索敵、追擊 leash、返回防守點、受擊反擊或可見 UI；Phase 05 文件也將 attack-range approach 列為未完成 coordinator 缺口。
- Scope In：四種 mode、0.5／1.0／1.5 倍範圍、反擊受擊鎖定、deterministic 目標選擇、Movement 追擊與返回、防守原點更新、CommandBus、HUD、snapshot、event、Save／Load、API 文件與測試。Scope Out：完整外交 hostile query、spatial partition、combat order queue、正式動畫／VFX。
- Changed：新增 `UnitEngagementMode`、`EngagementTargetReason`、`UnitEngagementRules`、`SetUnitEngagementModeCommand`、mode／target events 與 snapshot 欄位；`CombatSystem` 現在管理 mode、origin、目標來源、自主索敵、leash 與 retaliation；`CombatMovementCoordinator` 把 chase／return intent 轉成 Movement orders，明確攻擊仍有優先權。
- Product Integration：Prototype 生成單位時明確設為 `Normal`；非 queue Move、Stop／Hold 會更新防守原點並取消現有目標；HUD 新增「單位姿態」頁籤與四個中文按鈕，Selection panel 顯示 mode；`PrototypeEntitySaveData` round-trip mode、target reason、origin。舊 v3 欄位缺失時以 `Normal` 和存檔位置補值。
- Input Fix：960×540 實機檢查發現 HUD click 同時穿透到 RTS world selection；新增 `UnityRtsInputAdapter.SetPointerBlocker` 可注入區域，Prototype 保留上方 cards 與 command dock，姿態命令套用後 selection 不再被清除。
- Behavior：HoldGround／Normal／Aggressive 只會在 engagement origin 的 0.5／1.0／1.5 倍 attack range 內取得最近合法敵人並追擊；目標越界時清除並返回 origin。Retaliate 不主動索敵，受到敵方傷害後鎖定來源；目標死亡／失效後返回 origin。Player／AI 明確 `AttackTargetCommand` 不受模式阻擋。
- Architecture：authoritative mode／origin／target 只存在 `CombatSystem`；coordinator 無 state，只讀 Combat／Movement snapshots；HUD 經 `IHudCommandSink` → CommandBus；Player／AI／Scenario／Test 共用相同 command 與倍率規則，未新增 God Manager 或 Bootstrap gameplay truth。
- API / Data：新增 `CombatSystem.SetEngagementMode`、`NotifyMoveOrder`、`NotifyHoldOrder`、`CompleteReturnToOrigin`；`CombatantSnapshot` 新增 `EngagementMode`、`TargetReason`、`EngagementOrigin`、`DefenseRange`、`ShouldReturnToOrigin`；新增 `UnitEngagementModeChangedEvent` 與 `EngagementTargetChangedEvent`。
- Tests：`CombatAbilityTests` 新增倍率邊界、主動索敵、反擊受擊、leash 返回、明確攻擊優先；Prototype PlayMode 新增 Combat／Movement 整合 pursuit test，PP06 補 HUD stance command／selection preservation，PP07 Save／Load 補 engagement round-trip；package API contract 鎖定 command、system method 與 coordinator public surface。
- Validation：`dotnet build AegisRTS.FrameworkLab.slnx` PASS（0 warning／0 error）；Unity EditMode PASS 176/176；Unity PlayMode PASS 31/31；Windows Development Build PASS（173,599,720 bytes）；960×540 executable 實際關閉 modal、點選我方指揮官、開啟姿態頁籤、點 Aggressive，畫面顯示 `1/1 unit(s)` 且 selection 保留；證據 `engagement_hud_release.png`；Player log 0 error hits；`git diff --check` PASS。
- Known Issues：自主 hostile 暫以 faction 不同判斷，尚未接 Faction diplomacy；索敵目前 O(n²) 全量掃描；queue Move 不立刻改寫 engagement origin；IMGUI 文字與 event notification 尚未全面中文化。
- Files：`CombatModels.cs`、`CombatSystem.cs`、`UnityRtsInputAdapter.cs`、Prototype composition／HUD／save／bootstrap、Combat EditMode／Prototype PlayMode／package contract tests、docs 14／26／37／38／43、package FrameworkApi／CHANGELOG。
- Git：未 commit、未 push；保留既有未提交工作，不處理 `AGENTS.md` 與無關修改。
- Next：若下一輪先強化戰鬥可讀性，建議加入 placeholder projectile／melee hit feedback；若先擴展規則，優先注入外交 hostility query，再以 spatial index 支援超大地圖多勢力。

## 2026-08-12 — 新手說明 modal 點擊穿透修正

- Status：Completed
- Goal：修正 Windows Player 按「我了解了，開始遊戲」後說明不消失、底層 HUD 反而收到點擊而造成卡住的問題。
- Baseline：說明視窗最後繪製，但底層 GUILayout controls 先取得 MouseDown hot control；可重現為 overlay 留在畫面、玩家材料由 200 變 190 並錯誤排入 Economy construction。
- Changed：`PlayablePrototypeBootstrap.OnGUI` 在 modal 顯示時停用底層 HUD；更重要的是，在繪製任何 HUD 前以 `WelcomeDismissHitRect` 攔截說明按鈕區 MouseDown、立即關閉說明並 `Event.Use()`，避免 MouseUp 再穿透。另加入 Enter／KeypadEnter／Esc 關閉備援；操作手冊同步更新。
- Behavior：Before 為可見按鈕無反應且觸發底層命令；After 為單次點擊立即關閉 overlay、材料維持 200、無 construction queue，simulation 與 RTS input 正常開始。
- Architecture：修正只在 Bootstrap presentation／input arbitration；不修改 gameplay state、CommandBus 或 domain rules。
- Tests / Validation：`dotnet build AegisRTS.Demo.csproj` PASS（0 warning／0 error）；PP06 scene／HUD PlayMode targeted test PASS 1/1；Windows Development Build PASS（173,574,388 bytes）；960×540 executable 以實際滑鼠 MouseDown／MouseUp 點擊驗證，證據 `aegis_modal_clicked_r2.png`；Player log 0 error hits；`git diff --check` PASS。
- Known Issues：IMGUI 仍是 Prototype UI；正式 UI 建議改用 UI Toolkit 或 Canvas EventSystem，以標準 modal input blocker 取代手動 hit rect。
- Git：未 commit／push；本次沒有收到相關指令。
- Next：加入 placeholder combat feedback：遠程投射物色塊、近戰突進／閃光、受擊閃色與死亡縮放，保持 CombatSystem 為 authoritative state。

## 2026-08-12 — GameMode 與 `fortified-city` 要塞城市第一個可玩切片

- Status：Completed（玩家進攻要塞的第一個完整垂直切片）；玩家守城、隨機武將分配與 `constructed-base` 仍為 Deferred。
- Goal：重新定義兩種對局模式與兩種主堡規則，先把類《三國霸業》的主堡＋固定城牆＋可修城門＋佔領主堡玩法落到 Prototype，而非只留在聊天或文件。
- Baseline：Prototype 使用 `destructibleWalls=true`，攻城只破 Gate 後直接 Capture；攻城兵器要求先建兵營；沒有 Stronghold Core、Gate Repair API、模式／據點正式規格，Save v2 也不保存修復倒數與主堡狀態。
- Scope：
  - In：正式規格、`fortified-city` rule metadata、主堡直募、固定不可破壞城牆、可破壞／修復 Gate、Stronghold Core 壓制與 owner transfer、HUD／onboarding、Save v3、tests、package docs、Windows build／畫面 smoke。
  - Out：超大劇情地圖內容、隨機指揮官選擇與武將分發、玩家守城操作、AI 對玩家城門的完整攻城、維修資源成本、工程單位、`constructed-base` 建築放置／全建築摧毀、正式世界觀與美術。
- Changed：
  - `docs/39_GameMode_據點與武將分配規則.md`：新增 `story-grand-war`／`random-commander-war` 與 `constructed-base`／`fortified-city` 正式規格、資料邊界、流程、未完成項目、順序與驗收條件。
  - `PrototypeNeutral/ContentPack.json`：切換為 `fortified-city`；`destructibleWalls=false`，啟用 Gate repair、Stronghold recruitment、capture-instead-of-destroy；新增 repairable Gate 與 Stronghold Core；Siege Unit 移除兵營 prerequisite，保留 Siege Technology。
  - package `GameRuleSet`／JSON loader：新增世界中立的 settlement archetype、Gate Repair、Stronghold Recruitment、Capture Stronghold switches，舊 constructor 透過 optional defaults 維持相容。
  - package `SiegeSystem`／models／router：新增 defender-only `RepairDefenseStructureCommand`、`Repairable` profile、Repaired／BreachSealed events；Gate 從 0 HP 修回正值時 Closed，外圈狀態回到 Active。
  - `PrototypeSystemComposition`：註冊 repair command router、Fortified City capture rule、Stronghold Core；守軍 8 秒 repair cadence，每次 45 HP；進入內城後停止修門；攻擊 Core 至 0 才執行 Settlement／Territory owner transaction。另修正 AI 主堡直募增援的 Army membership restore 對帳。
  - `PlayablePrototypeBootstrap`：主堡生產 HUD、修門倒數／固定城牆提示、Attack Stronghold 流程、可見 Stronghold primitive 與 capture 後 team color；onboarding 改為主堡直募與接管主堡。
  - `PrototypeGameStateAdapter`：升級 schema／extension 至 v3，保存 Gate HP、Stronghold HP 與 repair countdown；舊 v2 明確不相容，不 silent migrate。
  - `SiegeSystemTests`／`PlayablePrototypePlayModeTests`：新增守方修復與 breach seal、rule metadata、無兵營直募、Stronghold Core capture、AI 增援 restore regression；同步移除舊兵營 queue 假設。
  - `docs/00`、`26`、`34`、`35`、`37`、`38`、package API／CHANGELOG：同步規格、API、操作、保存與維護契約。
- Behavior：
  - Before：Build Economy → Build Barracks → Research Siege → Recruit Siege → break Gate → enter → manual Capture；牆規則資料標成可摧毀，Gate 一旦摧毀不可修。
  - After：一般兵種直接由主堡招募；研究科技後由主堡製造攻城兵器；固定城牆沒有可受擊 runtime structure；Gate 可破壞且守方可修復封回通路；玩家必須在修門前進內城並攻擊 Stronghold Core，核心歸零後主堡保留、城市與領土 owner 一致轉移並 Victory。
- Architecture：Definitions／Content 宣告規則；Siege／Settlement／Territory 擁有 authoritative HP、capture condition 與 owner；Bootstrap 只投影 Gate blocker／Stronghold team color；Player／AI defender／tests 都經 CommandBus／SiegeSystem，沒有 HUD 直接改 HP 或 owner。故事名稱與產品規則沒有進 package Runtime。
- API / Data：
  - `GameRuleSet` 新增 `SettlementArchetypeId`、`GateRepairEnabled`、`StrongholdRecruitmentEnabled`、`CaptureStrongholdInsteadOfDestroy`。
  - `DefenseStructureProfile` 新增 `Repairable`；Content tag `repairable` 會映射此能力。
  - 新增 `RepairDefenseStructureCommand`、`DefenseStructureRepairedEvent`、`BreachSealedEvent`。
  - Prototype save v3 新增 `strongholdHealth`／`gateRepairRemainingSeconds`；v2 save 需重新建立。
- Tests / Validation：
  - `dotnet build AegisRTS.Tests.EditMode.csproj`／`AegisRTS.Tests.PlayMode.csproj`：PASS，0 warning／0 error。
  - FrameworkLab Unity EditMode：PASS，166/166，`aegis_fortified_editmode.xml`。
  - FrameworkLab Unity PlayMode：PASS，30/30，`aegis_fortified_playmode_final.xml`；涵蓋 Unity NavMesh、要塞攻城、Save／Load、AI 與 E2E。
  - Clean package validation：EditMode PASS 6/6、PlayMode PASS 3/3，`aegis_fortified_package_editmode.xml`／`aegis_fortified_package_playmode.xml`。
  - Windows Development Build：PASS，`C:\projects\Unity\AegisRTS.BuildValidation\PlayablePrototype_01.exe`，Unity build report 173,573,595 bytes；`AegisRTS.Demo.dll`／Gameplay assemblies 於 13:17 更新。
  - Executable smoke：960×540 實際啟動並檢查 onboarding 與 gameplay；最終畫面 `aegis_fortified_player_final.png`／`aegis_fortified_gameplay_final2.png`，Player log 0 個 NullReference／MissingReference／Argument／InvalidOperation／Assertion／Crash／Unhandled／Error hits。
  - `git diff --check`：PASS；Content JSON parse PASS；Unity build 自動改寫的 URP／ProjectSettings 雜訊已還原，只保留既有 intentional `EditorBuildSettings.asset`。
- Acceptance：玩家進攻 `fortified-city` 的 first playable slice 為 PASS：主堡直募、科技攻城兵器、固定牆、可修 Gate、breach navigation、Core-gated capture、owner transaction、Save／Load 與 Windows build 均有自動或實機證據。不可宣稱玩家守城、隨機模式或建造型基地完成。
- Known Issues / Deferred：目前修 Gate 由守軍 AI timer 使用 Enemy Hero 作 repairer，尚無資源成本、工程單位、施工動畫或玩家守城 UI；玩家左側主城尚未建立對稱的 Gate／Wall defense siege；Core 0 HP 是「防禦被壓制」的 domain condition，視圖不銷毀。
- Git：Branch `main`；本次未收到 commit／push 指令，因此 NOT DONE。工作樹包含先前尚未提交的 Prototype／package 變更；`AGENTS.md` 是既有未追蹤環境檔，不屬於本成果。
- Next：先實作玩家守城＋AI 攻門＋付費維修，再做 `MatchSetupDefinition`、seeded hero allocation 與 `random-commander-war` 最小選角／分發畫面；`story-grand-war` 等 G01 世界觀確定後再填正式勢力。

## 2026-08-12 — PlayablePrototype_01 實機可用性修正與完整通關

- Status：Completed
- Goal：直接啟動 Windows Development Build、以實際畫面與玩家操作檢查「打開後看不懂」問題，修正阻擋理解與完整遊玩的 P0 缺陷，並完成一局勝利與正常戰敗／重開驗收。
- Baseline：自動測試與 process smoke 已通過，但初始 executable 是除錯儀表板式 HUD；大量內部 ID／訊息遮住戰場、敵軍會在玩家理解介面前擊殺指揮官，且 runtime 動態 Shader 在 Windows build 被剝除後呈現紫色材質。先前沒有真實畫面與完整按鈕流程證據。
- Changed：
  - `PlayablePrototypeBootstrap.cs`：重做玩家向 IMGUI；加入中文資源列、目前任務、戰況、世界標籤、底部三分頁指令台、disabled prerequisite、設定／說明、勝敗 modal、F1 說明與 F3 debug 分離。
  - 新遊戲顯示三步 onboarding 並暫停模擬與 RTS input；關閉說明後才開始操作。AI 增加 90 秒進攻保護期，頂端顯示倒數。
  - Restart 從 Victory／Defeat 正確經 `GameSessionController.Restart()` 回到 Playing，不再保留戰敗遮罩。
  - Save／Load 改為排程 scene reload；新場景先建立乾淨世界，下一個 Update 才套用權威存檔，避免在 IMGUI／Awake 期間重建 NavMesh、camera 與 renderer。實機確認載入後戰場、單位、資源與 HUD 都保留。
  - `RtsCameraController` 初始化後忽略前兩幀輸入；`UnityRtsInputAdapter` 在視窗失焦時不處理輸入；說明頁顯示時停用 RTS input，避免背景 edge-scroll 把鏡頭捲走。
  - 建立並序列化 `PrototypeUnlit.mat`，Windows build 不再依賴可能被 shader stripping 移除的 runtime `Shader.Find`。
  - 960×540 時隱藏與 HUD 重疊的世界標籤，將主要建設、研究、招募、Save／Load 與勝利事件訊息中文化。
  - PP06 PlayMode regression 新增 onboarding pause、Defeat → Restart state、Save → scene reload → world／views／camera 可見性的驗證。
- Tests / Validation：
  - FrameworkLab EditMode：PASS，165/165，`aegis_ux_final_editmode.xml`。
  - FrameworkLab PlayMode：PASS，30/30，`aegis_ux_final_playmode.xml`。
  - Clean package validation：EditMode PASS 6/6、PlayMode PASS 3/3，`aegis_ux_final_package_editmode.xml`／`aegis_ux_final_package_playmode.xml`。
  - Windows Development Build：PASS，`C:\projects\Unity\AegisRTS.BuildValidation\PlayablePrototype_01.exe`，173,558,063 bytes。
  - Visual QA：1280×720 與 960×540 均實際啟動檢查；主要證據為 `aegis_ux_1280_game_final.png`、`aegis_ux_verified_started.png`、`aegis_ux_twostage_actual.png`、`aegis_ux_rapid_victory.png`。
  - Executable flow：PASS。實際按 UI 完成 onboarding → Economy → Recruitment → Siege Tech → Siege Unit → Hero Army → Start Siege → Breach → Enter → Capture → Victory；另實際等待敵軍反攻產生 Defeat，再從 modal Restart 並完成下一局。
  - Save／Load：PASS。實際按 Save／Load 後 scene reload，世界、單位、資源、人口、HUD 與倒數恢復；最新相關 Player logs 0 個 NullReference／MissingReference／Assertion／Crash／Unhandled Error hits。
- Acceptance：PP00～PP08 為 PASS；系統優先 Prototype 可理解、可操作、可存取、可勝利、可戰敗並可重新開始。Unity Editor 以 PlayMode 30/30 驗證，Windows executable 以實際 UI 完整通關驗證。
- Known Issues / Deferred：仍是 primitive graybox 與 IMGUI prototype；selection／camera／戰鬥手感、正式 UI Toolkit、accessibility、production profiling、世界觀、美術與音訊仍屬後續，不再是「能否啟動與完成一局」的 blocker。
- Git：尚未 commit／push；`AGENTS.md` 為既有未追蹤環境檔，不屬於本成果。
- Next：先建立本次 Prototype checkpoint commit，再做一輪針對操作手感與 UI 架構的 polish；G01 世界觀與 production art 可繼續延後。

## 2026-08-11～2026-08-12 — PlayablePrototype_01 PP00～PP08

- Status：Partial（程式、自動 gate、build 與 process smoke 完成；人工 playable gate NOT RUN）
- Goal：建立一個由玩家 input 驅動、使用中立 placeholder content 的完整 Prototype，補齊 PP00 前置並依 PP01～PP08 完成 selection／movement／combat、economy／production／recruitment、Hero／Army、AI、siege／capture／victory、HUD／session、Save／Load、tests／performance／Windows build。
- Baseline：
  - Branch／Git：`main`；開始時 `HEAD`／`origin/main` 同為 `fd94ccf Add PlayablePrototype`，工作樹乾淨。
  - Existing Evidence：Framework DoD 已通過；`Sandbox_RTS` 有玩家 selection／movement，但 Attack 只記錄 command；各 domain sandbox 與自動 Vertical Slice 通過，沒有單一玩家 full loop。
- Scope：
  - In：PP01～PP08 全部 requirements；因 PP00 assets／composition 不存在，補齊它們作為不可省略的依賴。
  - Out／Deferred：正式世界觀、production art、完整 Campaign、multiplayer、商城與 G01～G12。
- Changed：
  - `Assets/AegisRTS/Content/PrototypeNeutral/`：新增 valid neutral Content Pack、Vertical Slice binding、Scenario 與 UI Theme；提供兩種資源、Infantry／Archer／Cavalry／Siege、三名 Hero、Economy／Recruitment buildings、Siege technology、三個 settlements、Gate、AI 與 scenario rules。
  - `Assets/AegisRTS/Demo/PlayablePrototype/PrototypeSystemComposition.cs`：組合 Economy、Building、Technology、Recruitment、Movement、Combat、Faction、Territory、Settlement、Hero、Army、AI、Siege、Scenario、CommandBus 與 EventBus；建立固定 IDs、tick order、共用 Player／AI commands、spawn／death lifecycle、siege validation、capture、read side 與 restore。
  - `PrototypeEntityRegistry.cs`、`PrototypeNavigationAdapter.cs`：建立單一 EntityId registry 與 deterministic navigation／position adapter。
  - `PrototypeUnityNavigationAdapter.cs`：新增 Unity `NavMeshSurface`／`NavMeshAgent` product adapter；封閉 Gate 使內院不可達，破門後 rebuild NavMesh 並重新派送目的地。
  - `PrototypeGameStateAdapter.cs`：改以 `GameStateCoordinator` 建立 checksum／version／content／scenario envelope；單一 PlayerPrefs slot 可保存 active production queues、movement／combat／Army／AI／random transient state與 SHA-256 fingerprint。
  - `PlayablePrototypeBootstrap.cs`、`PrototypeHudAdapter.cs`、`PlayablePrototype_01.unity`：建立封閉城堡 graybox、Unity RTS input／selection／camera、primitive units／health bars、`IHudQuery`／`IHudCommandSink` HUD、theme/settings、notifications、menu、pause／resume／restart／save／load 與 Debug Defeat control。
  - `Assets/AegisRTS/Editor/`、`ProjectSettings/EditorBuildSettings.asset`：新增 scene rebuild／Windows Development Build method，並把 Prototype scene 加為第一個 build scene。
  - `PlayablePrototypePlayModeTests.cs`：新增 11 個 PP00～PP08 tests；涵蓋 boot、player combat、economy rollback、Hero／Army 全命令、AI 真實生產／招募、Unity NavMesh gate、siege／victory／正常 defeat、HUD interfaces／session、完整 Save／Load、E2E／long-run 與 300-unit smoke。
  - package `BuildingSystem`、`TechnologySystem`、`RecruitmentSystem`、`MovementSystem`、`CombatSystem`、`ArmySystem`、`AiSystem`、`EconomySystem`：新增通用 runtime snapshot／restore API；Recruitment spawn 失敗會原子回滾成本與人口。新增 5 項 EditMode regression。
  - `HeroArmyCommandTests.cs`、package `ArmySystem.cs`：新增已分配 member 移除 regression；修正 `UnregisterMember` 未同步 Army snapshot／commander／Hero／Combat ArmyId 的通用 lifecycle defect。
  - `docs/37_PlayablePrototype_01_操作與驗收手冊.md`、`38_PlayablePrototype_01_架構與維護.md`：新增完整啟動、操作、manual gate、現況、限制、建議順序、ownership、tick、entity、position、save／restore 與維護規則；總覽索引同步更新。
- Behavior：
  - Before：沒有 `PlayablePrototype_01` scene／Content／runtime／tests；Vertical Slice 由自動 stage executor 完成。
  - After：單一 scene／build 可 New Game，透過 input 或 HUD 完成戰鬥、經濟、招募、Army、AI 反攻、攻城、佔領與勝負；Gate 會實際阻擋 NavMesh，破門後才可進入；Pause 停止 simulation，Restart 清理並重建 session，Save／Load 可在 active queues／orders 中途精確還原 gameplay fingerprint 與 views。人工操作完整一局尚未由專案擁有者執行。
- Architecture / API / Data：
  - Architecture：產品層留在 `Assets/AegisRTS/Demo/PlayablePrototype`，Bootstrap 只做 Unity composition／presentation；domain truth 由 package systems 擁有。Player、HUD、AI、tests 共用 CommandBus；query／snapshot／event 驅動 UI。Entity spawn、death、restart、load 與 dispose 都有固定 lifecycle。
  - API：新增 Demo composition、navigation runtime、HUD query／sink 與 save surface；package 新增 production queue snapshots／restore、movement orders、combat／Army／AI runtime restore。`ArmySystem.UnregisterMember(EntityId)` 真正移除已分配成員並同步 commander／Hero／Combat state。
  - Data：新增 `prototype.neutral` Content、`scenario.prototype-conquest` 與 Prototype save schema v2。Save 僅含純資料，不含 Unity references；使用 `GameStateCoordinator` checksum 與 content／scenario metadata 拒絕 corrupt／incompatible data。
- Tests / Validation：
  - Prototype-only Unity PlayMode：PASS，11/11，結果 `aegis_pp_audit8.xml`。
  - FrameworkLab Unity EditMode：PASS，165/165，結果 `aegis_editmode_pp_final.xml`；新增 queue／movement／combat／Army／AI restore regression。
  - FrameworkLab Unity PlayMode：PASS，30/30，0 failed／0 skipped，最終結果 `C:\projects\Unity\AegisRTS.ValidationResults\aegis_playmode_pp_final2.xml`；包含載入後 AI economy 繼續模擬仍 deterministic 的驗證。
  - Clean package project `C:\projects\Unity\AegisRTS.PackageValidation`：EditMode 6/6、PlayMode 3/3 PASS；包含 package Army lifecycle fix。
  - 最終 Windows Development Build：PASS，輸出 `C:\projects\Unity\AegisRTS.BuildValidation\PlayablePrototype_01.exe`，Unity build report 173,516,501 bytes；executable 在 1280×720 與 960×540 各啟動 8 秒持續運行，`aegis_pp_player_final2_1280x720.log`／`aegis_pp_player_final2_960x540.log` 未發現 NullReference、MissingReference、Argument／InvalidOperation、assertion 或 crash，之後由驗證程序停止。
  - Deterministic E2E：New Game → Recruit → Army → Battle → Siege → Capture → Victory PASS；1800 simulation seconds long-run PASS；300 active units／120 ticks 在 5 秒 ceiling 內 PASS。
  - Manual Editor／executable A～E：NOT RUN；不可用自動測試或 process boot 冒充。
- Acceptance：
  - PP00～PP07 自動 acceptance：PASS。
  - PP08 automated regression／performance smoke／build／process boot：PASS。
  - PP08 executable 完整人工通關與 1920×1080＋較小解析度：NOT RUN，因此 PP08 整體 `PARTIAL`。
  - PA-01、PA-03～PA-15 automated evidence：PASS；PA-02 的自動 movement 與 input wiring PASS，manual 操作部分 NOT RUN。
  - Package Runtime world-specific hardcode boundary：維持 package tests／static architecture規則；本次新增世界中立資料只在 Lab Content／Demo。
- Completed：
  - 完成 PP00～PP08 所需 Content、scene、composition、input／HUD、Unity NavMesh gate、所有 domain integration、mid-action Save／Load、tests、performance smoke 與 Development Build。
  - 修正死亡 member 留在 Army／Hero／Combat state 的通用 package defect並加入 regression。
  - 建立足以重現操作、驗收、架構與後續維護的詳細文件。
- Not Completed / Deferred：
  - P0：專案擁有者尚未完成一次 Editor 與 executable A～E 人工通關；完成前 Prototype 整體維持 `Partial`。
  - P2：目前 HUD 為功能完整的 IMGUI prototype，尚未進行正式 UX、accessibility 或 production UI Toolkit 視覺整理。
  - Deferred：G01～G12、正式世界觀、production art／audio、tutorial、localization、accessibility、target hardware production profiling。
- Known Issues / Risks：
  - Unity NavMesh 是 runtime bake；若未來加入大型動態障礙或多層地圖，需重新評估 rebuild cost、agent carving 與 path reissue policy。
  - Save schema v2 對舊的 prototype schema v1 採明確不相容，不做 silent migration；舊測試存檔需重建。
  - 300-unit test 是 simulation regression ceiling，不代表 production hardware 的 frame／render／NavMesh budget。
- Git：
  - Branch：`main`。
  - Working Tree：本工作包含新增 Prototype Content／Demo／Editor／tests／docs 與 package bug fix，全部尚未提交；`AGENTS.md` 是既有未追蹤檔，不屬於本成果。
  - Commit／Push：NOT DONE；本次沒有收到 commit／push 指令。
- Next：
  1. 依 `docs/37_PlayablePrototype_01_操作與驗收手冊.md` 在 Editor 與最新 Windows build 完成 A～E 人工通關及兩種解析度驗收，將實際結果補回本紀錄。
  2. 修正人工 gate 發現的 P0 blocker，優先處理 selection／camera／command feedback、NavMesh path feedback 與 mid-action Save usability。
  3. 人工 gate PASS 後才建立 Prototype checkpoint commit／tag，再開始 G01／G02；production art 繼續延後。

## 2026-08-11 — PlayablePrototype_01 詳細規劃與紀錄規範

- Status：Completed
- Goal：建立 system-first、world-neutral、placeholder 的玩家可操作 Prototype 詳細規格，盤點 Framework 已完成能力與產品層缺口，排定實作優先級，並把未來每次開發都必須留下詳細紀錄寫入正式治理文件。
- Baseline：
  - Branch／Git：`main`；開始時 `HEAD`／`origin/main` 同為 `aa0bfce Finish Definition of Done`，工作樹乾淨。
  - Existing Evidence：Framework Phase 01～16、API contract 與 DoD 已完成；最近可信結果為 FrameworkLab EditMode 159/159、PlayMode 19/19，package validation EditMode 6/6、PlayMode 3/3。
- Scope：
  - In：Prototype 目標／非目標、Definition of Playable、中立 defaults、產品層架構、PP00～PP08、現況矩陣、優先級、acceptance、debug／test requirements、詳細進度紀錄規則與 Agent 流程。
  - Out／Deferred：本次不建立 Unity scene、Content Pack、C#、tests、Windows build、正式世界觀或美術；PP00～PP08 均尚未實作。
- Changed：
  - `docs/34_PlayablePrototype_01_總覽與範圍.md`：定義 system-first 決策、現況、完整玩家流程、Definition of Playable、scope、PrototypeNeutral defaults、產品層 components 與禁止事項。
  - `docs/35_PlayablePrototype_01_分階段實作計畫.md`：建立 M1～M5 與 PP00～PP08，每階段包含 Goal、Tasks、Acceptance 及第一個建議執行 prompt。
  - `docs/36_PlayablePrototype_01_現況缺口與驗收矩陣.md`：逐項標示 Completed／Partial／Missing／Deferred，列出 P0～P3、15 項 end-to-end acceptance、debug 與測試最低要求。
  - `docs/09_DevelopmentProgress_開發進度紀錄規範.md`：新增 12 項詳細紀錄最低標準、Current Status 維護規則與擴充範本。
  - `docs/40_Agent_總執行規則.md`、`41_Agent_Phase執行Prompt.md`、`42_Agent_CodeReview與驗收Prompt.md`：強制現況矩陣、詳細紀錄、未完成項目與 Git evidence；資料不足不可判定 PASS。
  - `docs/00_README_開發總覽.md`、`30_GameProduction_總覽.md`、`60_第一階段實際執行順序.md`：把 PlayablePrototype 文件與先系統、後 G01／Art 的決策加入正式閱讀／執行順序。
  - `DevelopmentProgress.md`：新增本詳細紀錄並更新 Current Status。
- Behavior：
  - Before：Framework 各系統與自動 Vertical Slice 已完成，但 Game Production 文件會直接從 G01 開始，沒有描述如何先把系統接成玩家可操作流程；進度規範只要求摘要欄位。
  - After：正式流程先執行 world-neutral PP00～PP08，再進入 G01；文件明確指出 Attack 仍是 RTS Sandbox log、各 systems 尚未整合、Save／Load 尚未覆蓋整體 Prototype，並要求未來留下可重現的詳細紀錄。
- Architecture / API / Data：
  - Architecture：Prototype 留在 `Assets/AegisRTS/Demo/PlayablePrototype` 的產品層，composition 不成為 God Manager；Player／AI／HUD／Scenario 共用 CommandBus，query／snapshot 為 read side，package Runtime 只有通用 defect 才允許修改。
  - API：N/A；本次只建立規格，未修改 runtime public API。文件規劃重用既有 Commands、Routers、Queries、Events、`IUnitSpawnSink` 與 persistence boundaries。
  - Data：規劃新的 `PrototypeNeutral` Content Pack／Scenario／Theme 與 role-based IDs；本次未建立或變更 JSON schema。
- Tests / Validation：
  - 文件完整讀取：00、03、04、09、27、30～33、40～42 與最新 `DevelopmentProgress.md`。
  - Repository inventory：確認 `Sandbox_RTS` 玩家操作、各 system sandboxes、19 個 PlayMode tests、Vertical Slice public composition、command routers、HUD／Save boundaries。
  - 新文件規模：34 共 179 lines、35 共 257 lines、36 共 147 lines。
  - `git diff --check`：PASS（全部 tracked changes）。
  - Markdown integrity：11 個變更／新增文件皆無 trailing whitespace、code fences 成對；11 個 `docs/*.md` references 全部存在。
  - Unity EditMode／PlayMode：NOT RUN；本次沒有 runtime、scene、asset、package 或 JSON 變更，沿用結果只列為 baseline，不宣稱為本次測試。
- Acceptance：
  - 詳細 Prototype 規格：PASS — 目標、scope、架構、defaults 與 Definition of Playable 已文件化。
  - 現在哪些已做／未做：PASS — 19 類能力矩陣與五項 integration blockers 已列出。
  - 建議先做哪些：PASS — P0～P3、M1～M5 與 PP00 first prompt 已列出。
  - 多階段可執行計畫：PASS — PP00～PP08 均有 Tasks／Acceptance。
  - 未來詳細紀錄規範：PASS — 09、40、41、42 已同步強制要求。
  - Runtime playable implementation：NOT RUN — 本次明確為規劃工作，PP00 尚未開始。
- Completed：
  - 完成 Prototype 規格、roadmap、gap／priority／acceptance matrix 與 detailed-record governance。
  - 明確決定先整合系統、延後世界觀與 production art。
- Not Completed / Deferred：
  - PP00～PP08：Missing／未開始，P0 起點為 PP00。
  - G01～G12：Deferred，待 Playable Prototype PASS 後執行。
  - Art Bible／production assets：Deferred，待正式勢力與視覺需求確定。
- Known Issues / Risks：
  - 規格很完整但尚未由 Unity scene 證明；第一個技術風險是跨 system entity registration／cleanup，Priority P0。
  - `VerticalSliceSimulation` 是自動 regression composition，不應直接改成玩家 God Manager；應建立獨立 Prototype composition。
  - Save DTO 已完成，但跨全部 Prototype systems 的 restore ordering 尚未實作，Priority P1。
  - package 仍為 `UNLICENSED`，不阻擋內部 Prototype，但阻擋正式公開散布。
- Git：
  - Branch：`main`。
  - Working Tree：本筆紀錄所在文件變更尚未提交；實際為 3 個新增 Prototype docs 與 8 個治理／總覽／進度文件，沒有 runtime／asset／package 變更。
  - Commit／Push：NOT DONE；等待使用者指定或後續明確要求。
- Next：
  1. 執行 `docs/35_PlayablePrototype_01_分階段實作計畫.md` 的 PP00，且只完成 PP00 scope。
  2. PP00 通過 Code Review 後執行 PP01，優先取得玩家可操作的真實戰鬥閉環。

## 2026-08-11 — Definition of Done 總驗收

- Status：Completed
- Goal：執行 `docs/27_Definition_of_Done_總驗收.md`，重新驗證 11 個 Phase gates 與兩種背景、攻守城、Save／Load、AI 完整循環、第二專案 package 安裝等 Framework final gates。
- Changed：
  - 在 DoD 文件新增逐項 release-gate 矩陣、Framework 驗收證據、static validation 與 remaining release risks。
  - 更新本進度文件；本次沒有修改 runtime code、API signature、scene、asset 或資料格式。
- Architecture / API / Data：
  - Core／Gameplay／Persistence 的 Pure C# 邊界維持不變；Core 無 references、Gameplay 只依賴 Core、Persistence 只依賴 Core／Gameplay。
  - Package Runtime 世界觀字串掃描 0 hits，世界觀仍只由 Lab Content JSON 提供。
  - N/A（API／Data）；本次是 release-gate 驗證，沒有新增或變更公開介面及資料 schema。
- Tests / Validation：
  - FrameworkLab Unity EditMode：PASS，159/159；PlayMode：PASS，19/19。
  - `C:\projects\Unity\AegisRTS.PackageValidation` Unity EditMode：PASS，6/6；PlayMode：PASS，3/3。
  - 四份 Unity logs 掃描未處理例外、compile errors、runner abort／failure：0 hits。
  - Package：ID／SemVer／Unity version／3 samples 驗證 PASS；Basic RTS／Combat／Siege 各有 scene。
  - Static architecture：Pure C# layers UnityEngine hits 0；Runtime 世界觀 hits 0；God Manager hits 0；舊 Assets Framework source 不存在。
  - Asset integrity：307 GUID、0 duplicates、0 missing file `.meta`；六個 Demo scenes 已加入 Build Settings。
  - Debug／read model：25 個 public `GetDebugSummary()`、21 個 public Snapshot types。
  - 驗收前 Git baseline：`HEAD`／`origin/main` 同為 `bb80db2`，工作樹乾淨。
- Known Issues / Risks：
  - package 仍為 `UNLICENSED`，公開散布前需選定授權。
  - Git install URL 目前使用 `#main`；正式 release 建議建立 immutable tag。
  - Demo／samples 為 acceptance visuals，不包含 production art／完整 UX；目標硬體 Player build profiling 尚未執行。
  - 規格文件的 Unity 6.3 LTS 與實際 `6000.5.7f1` 命名對應仍待確認。
- Next：
  - 將 DoD 與本進度紀錄一起 commit／push；其後由擁有者決定授權、release tag 與目標硬體 profiling。

## 2026-08-11 — Framework API 目標介面契約

- Status：Completed
- Goal：執行 `docs/26_Framework_API_目標介面.md`，把 CreateFaction、CreateSettlement、SpawnUnit、CreateArmy、IssueCommand、Recruit、Build、Research、StartSiege、CaptureSettlement、AddResource、StartScenario、Save、Load 對應到可安裝 package 的穩定公共入口。
- Changed：
  - 在目標介面文件新增 15 個目標操作到 subsystem／CommandBus／Persistence API 的對照表。
  - 新增 package `Documentation~/FrameworkApi.md`，並由 package README 與 Getting Started 導向此文件。
  - 新增 3 個 `FrameworkApiContractTests`，鎖定 setup／spawn／resource、共用 commands、save／load 的 public package contracts。
  - 更新 package changelog。
- Architecture / API / Data：
  - 不新增同時擁有 Combat、Economy、AI、Persistence、Presentation 狀態的全域 façade；setup 由 composition root 負責，runtime intent 繼續透過既有 CommandBus 與 routers。
  - `IUnitSpawnSink` 保留為產品層 spawn adapter，避免 Framework 假設每種遊戲的 unit 需要註冊哪些 optional systems／views。
  - 本次未修改 runtime method signature 或資料格式，新增的是可發布文件與 API 相容性測試。
- Tests / Validation：
  - Unity EditMode：PASS，159/159。
  - Unity PlayMode：PASS，19/19。
  - `C:\projects\Unity\AegisRTS.PackageValidation` EditMode：PASS，6/6；確認 file-installed package 可編譯並執行新增契約測試。
  - 三份 Unity logs 掃描 `Unhandled`、`NullReferenceException`、`Compilation failed`、`error CS`、`Aborting batchmode`：無匹配。
  - `git diff --check`：PASS。
- Known Issues / Risks：
  - 本次未重跑乾淨安裝專案 PlayMode；改動僅限文件與 Editor-only package contract tests，原專案完整 PlayMode 19/19 已通過。
  - package 仍為 `UNLICENSED`，正式公開散布前需選定授權。
- Next：
  - 執行 `docs/27_Definition_of_Done_總驗收.md` 的 release 前複核；若要發布 immutable release，再由使用者授權建立 tag。

## 2026-08-11 — Phase 16 Package / Framework 化

- Status：Completed
- Goal：輸出可透過 UPM 安裝的 `com.boyi.aegis-rts`，提供三個 samples，並在第二個乾淨 Unity project 完成 install／import／compile／play／custom content 驗收。
- Changed：
  - 將 Core／Gameplay／Presentation／Persistence 從 Assets 搬至 `Packages/com.boyi.aegis-rts/Runtime`，package 成為唯一 Framework source of truth。
  - 新增 SemVer `1.0.0` `package.json`、CHANGELOG、README、Getting Started 與 Git／disk 安裝方式。
  - 新增 Editor Content Pack validation menu 與 2 個 package smoke tests。
  - 新增 Basic RTS、Basic Combat、Basic Siege 三個可匯入 samples，各含 asmdef、scene、bootstrap 與說明。
  - 更新 package lock、專案結構／Phase 16／API／DoD／README 與本進度文件。
- Architecture / API / Data：
  - `Runtime` 不包含 Three Kingdoms／Fantasy Content；背景 JSON／assets 與 Demo composition 留在 Lab 專案。
  - package assemblies 保留原名稱與 dependency direction，既有 Demo／Tests 不需改 namespace 或 API。
  - `Samples~` 不參與 package runtime compile，匯入後以各自 sample asmdef 編譯。
- Tests / Validation：
  - 原專案 Unity EditMode：PASS，156/156（含 2 package smoke）；PlayMode：PASS，19/19。
  - 新建 `C:\projects\Unity\AegisRTS.PackageValidation` 並以 `file:` dependency 安裝：PASS。
  - Package Manager import Basic RTS／Basic Combat／Basic Siege：PASS；三個 scenes compile／play，PlayMode 3/3。
  - 消費端建立 `consumer.my-first-pack`，`ContentPackJsonLoader`＋`ContentPackValidator`：PASS；乾淨專案 EditMode 3/3。
  - Runtime world-specific string scan：PASS；package structure／SemVer／JSON／GUID／Git diff 稽核待提交前完成。
- Known Issues / Risks：
  - package 目前標記 `UNLICENSED`；正式公開散布前需由擁有者選定授權條款。
  - Git URL 安裝將追蹤指定 branch；正式 release 建議改用 immutable tag，例如 `#v1.0.0`。
  - Samples 使用 primitive／IMGUI acceptance visuals，定位為 integration examples，不是 production art。
- Next：
  - 建立 `v1.0.0` release tag／release notes（需使用者明確授權後執行），並在目標硬體進行 Player build profiling。

## 2026-08-11 — Phase 15 Vertical Slice

- Status：Completed
- Goal：用同一套 Framework 完成 Player City→Village→Enemy Fortress 的端到端可玩流程，並以 Three Kingdoms／Fantasy 兩套資料證明世界觀可替換。
- Changed：
  - 新增 pure C# `VerticalSliceDefinition`、JSON loader／validator、deterministic `VerticalSliceLoop` 與 `GameSessionController`。
  - 新增共用 `VerticalSliceSimulation`，組合既有 Faction／Territory／Settlement／Economy／Recruitment／Hero／Army／Combat／Siege／AI 系統。
  - 新增兩套完整 vertical-slice Content Pack／Scenario binding：各含 2 resources、4 unit roles、2 heroes、2 buildings、3 settlements、gate 與 AI profile。
  - 新增 `VerticalSlice_01` 場景及可視化 composition root，納入 Build Settings。
  - 完成 Start→Income→Recruit→Army→Move→Field Battle→Siege→Break Gate→Enter→Capture→Victory，以及 AI 反攻玩家主城。
  - 完成 New Game、Load、Pause／Resume、minimum Settings、Victory／Defeat／Restart 狀態 API。
  - 新增 8 個 EditMode cases與 2 個 PlayMode cases，更新 07、24、26 與本進度文件。
- Architecture / API / Data：
  - 世界觀差異只存在 JSON definition／semantic binding；兩套 demo 共用同一 `VerticalSliceSimulation`，未複製 Combat／Siege／AI 核心。
  - Vertical Slice 是 composition orchestration，不取得任何 domain state ownership；勝負與佔領仍由既有 authoritative systems 判定。
  - `IGameSessionBackend` 隔離 Unity scene／save slot 行為，session state machine 保持 pure C#。
- Tests / Validation：
  - Unity EditMode Test Runner：PASS，154/154 passed、0 failed；Phase 15 新增 8 cases。
  - Unity PlayMode Test Runner：PASS，19/19 passed、0 failed；Phase 15 新增 2 scene acceptance cases。
  - 兩個 Content Pack validation、兩個完整 loop、AI counterattack、field battle、gate breach、capture、world restart／load／pause／settings：PASS。
  - `git diff --check`：PASS；Gameplay VerticalSlice 無 `UnityEngine` reference。
- Known Issues / Risks：
  - 場景目前使用 primitive placeholder visual 與 IMGUI diagnostics；正式 UI／art／VFX 仍需產品層資產。
  - Demo Load backend 驗證 session load path並重建 data-defined simulation；完整 disk save round-trip 已由 Phase 13 persistence sandbox 覆蓋。
  - 自動流程是 acceptance slice，不取代玩家輸入、難度平衡與長局 playtest。
- Next：
  - 進入 Phase 16 Package / Install Validation，在第二個乾淨 Unity project 驗證 package export／install 與範例場景。

## 2026-08-11 — Phase 14 Performance

- Status：Completed
- Goal：先建立 profiling／metrics baseline，再完成 tick throttling、pooling、spatial query、LOD／culling decisions 與 100～1000 unit exploratory stress。
- Changed：
  - 新增 bounded `PerformanceMetricsCollector`、FPS／P95／subsystem／count／GC／memory snapshot。
  - 新增 external `PerformanceBudget` 與 named violation evaluator，不硬寫目標硬體門檻。
  - 新增 deterministic multi-frequency `TickScheduler` 與 catch-up cap。
  - 新增 bounded `ObjectPool<T>`；`UnityCombatDriver` projectile visuals 已實際改用 pool。
  - 新增 `SpatialHash<T>` insert／update／remove／radius query，支援 deterministic ordering。
  - 新增 Full／Reduced／Coarse／Culled `SimulationLodPolicy`。
  - 新增 100／300／500／1000 `PerformanceStressHarness` 與 Sandbox acceptance。
  - 新增 10 個 EditMode cases、1 個 PlayMode case，補強 Combat projectile pool PlayMode assertions，更新 07、23、26 與本進度文件。
- Architecture / API / Data：
  - Core Performance 全部 Pure C#；Unity／Profiler adapter 只負責提供 samples 與套用 LOD decisions。
  - Tick cadence、budget、cell size、pool cap、LOD distances 都是 composition／benchmark inputs。
  - SpatialHash 是 query broad phase；Combat、Navigation 與 Physics authoritative responsibilities 不變。
- Tests / Validation：
  - Unity EditMode Test Runner：PASS，146/146 passed、0 failed；Phase 14 新增 10 cases。
  - Unity PlayMode Test Runner：PASS，17/17 passed、0 failed；Phase 14 新增 1 Sandbox case。
  - metrics、budgets、30／5／10 Hz、catch-up、pool reuse、projectile pool、spatial index、LOD、四種 stress scale：PASS。
- Known Issues / Risks：
  - exploratory elapsed／memory 不是正式 hardware benchmark；尚未指定 production target machine 與 quality／resolution。
  - GPU instancing、render batching、occlusion、Animator LOD 與 NavMesh-specific profiling 尚需 Unity production adapter。
  - Core stress harness 是 deterministic structural baseline，不取代 Player build Profiler capture 與 long-session soak。
- Next：
  - 進入 Phase 15 Vertical Slice，組合完整 Start→Income→Recruit→Army→Battle→Siege→Capture→Victory loop。

## 2026-08-11 — Phase 13 Save / Replay / Debug / Test

- Status：Completed
- Goal：完成 pure GameState save/load、metadata、deterministic replay、development debug console 與 battle-state reload acceptance。
- Changed：
  - 新增 typed `GameStateDocument`，涵蓋 faction／settlement／unit／hero／army／resource／building／technology／objective／clock／random。
  - 新增 `SaveEnvelope`／`SaveMetadata`、SHA-256 integrity、strict version compatibility 與 fingerprint。
  - 新增 capture source／restore sink／coordinator，以及 memory／atomic file stores。
  - `SeededRandom` 與 `GameClock` 新增 state capture／restore。
  - 新增 Replay InitialState／Seed／Tick／Sequence／Command、recorder、serializer 與 player。
  - 新增九種 Debug Console commands、quoted tokenizer、enable gate 與 executor boundary。
  - Persistence assembly 改為 `noEngineReferences=true`；`Sandbox_AI` 加入 battle save/reload acceptance。
  - 新增 11 個 EditMode cases、1 個 PlayMode case，更新 07、22、26 與本進度文件。
- Architecture / API / Data：
  - Persistence 只依賴 Core／Gameplay contracts，不保存 concrete manager 或 Unity reference。
  - capture／restore 聚合由 composition root 負責；各 authoritative system 不被 Save service 取代。
  - Replay 保存 command data 與 deterministic order，實際 command reconstruction 由 injected sink 完成。
  - Debug Console 預設 disabled，只產生 validated request 並委派 executor。
- Tests / Validation：
  - Unity EditMode Test Runner：PASS，136/136 passed、0 failed；Phase 13 新增 11 cases。
  - Unity PlayMode Test Runner：PASS，16/16 passed、0 failed；Phase 13 新增 1 Sandbox case。
  - Battle HP／resources／objective／clock／random mutation 後 restore fingerprint：PASS。
  - checksum tamper、version rejection、Replay stable order、Random continuation、Clock restore、九種 debug commands：PASS。
- Known Issues / Risks：
  - Save compatibility 目前採 exact version；正式 release 前需建立 explicit migration chain 與 compatibility fixtures。
  - 尚未實作 async／compressed／cloud saves、incremental checkpoint 或 replay seek snapshots。
  - 正式 game composition 仍需為每個 authoritative system 實作 capture／restore adapter 與 replay command factory。
- Next：
  - 進入 Phase 14 Performance，建立 budgets、pooling、spatial partition、LOD／tick throttling 與 profiling acceptance。

## 2026-08-11 — Phase 12 UI / UX

- Status：Completed
- Goal：完成十個 RTS HUD panels、Query／Event／Command UI boundary 與資料驅動 themes，驗收替換世界觀 Theme 不修改 Gameplay。
- Changed：
  - 新增 `HudSnapshot`、`HudPanelViewModel`、`HudEntry`、`RtsHudViewModel` 與十個 `HudPanelId`。
  - 新增 `IHudQuery`／`IHudCommandSink`，UI refresh 與 intent 派送不直接寫 Gameplay state。
  - 新增 event-driven invalidation、bounded notification queue、dismiss 與 command result。
  - 新增 `HudThemeDefinition`／`HudThemeJsonLoader` 與 Neutral／Three Kingdoms／Fantasy JSON themes。
  - 新增 `RtsHudPresenter`，以同一 layout 顯示 Resource、Selection、Command、Ability、Army、Settlement、Minimap、Notification、Objective、Pause。
  - `Sandbox_AI` 加入 `HudSandboxBootstrap` 與 Theme assets，驗證三次 theme swap。
  - 新增 8 個 EditMode cases、1 個 PlayMode case，更新 07、21、26 與本進度文件。
- Architecture / API / Data：
  - UI layer 只讀 immutable query snapshot、訂閱 event、派送 command；authoritative gameplay state 沒有移入 Presentation。
  - Theme data 僅包含 visual tokens，不包含世界觀 gameplay rules 或 runtime values。
  - Notification 是 presentation-owned transient state；resource、selection、army、settlement、objective 仍由來源 system 查詢。
- Tests / Validation：
  - Unity EditMode Test Runner：PASS，125/125 passed、0 failed；Phase 12 新增 8 cases。
  - Unity PlayMode Test Runner：PASS，15/15 passed、0 failed；Phase 12 新增 1 Sandbox case。
  - 三 theme、固定十 panel layout、query cache／invalidation、notification、command delegation、theme swap no mutation：PASS。
  - Theme JSON syntax、`git diff --check`：PASS。
- Known Issues / Risks：
  - FrameworkLab renderer 使用 IMGUI placeholder；正式產品可保留 ViewModel 並替換 UI Toolkit／uGUI view。
  - Minimap 目前只提供 query panel，尚未接 render texture、fog overlay、ping 與 click-to-world。
  - Localization、gamepad focus、screen reader、safe area 與完整 responsive breakpoints 留給 production UX polish。
- Next：
  - 進入 Phase 13 Save／Replay／Debug／Test，序列化 authoritative snapshots 與 Scenario metadata。

## 2026-08-11 — Phase 11 GameMode / Scenario / Objective

- Status：Completed
- Goal：建立資料驅動 GameMode、Scenario、Objective、Trigger／Action 與勝敗流程，驗收不修改 C# 即可用資料完成至少四種不同關卡。
- Changed：
  - 新增 `GameModeDefinition`、`ScenarioDefinition`、`ObjectiveDefinition`、`TriggerDefinition`、`ScenarioActionDefinition` 與 immutable runtime snapshots。
  - 新增 `ScenarioSystem`，管理 facts、elapsed time、objective lifecycle、continuous hold、failure、trigger/action cascade 與 Victory／Defeat。
  - 完整宣告八種 default GameMode 與十種 Objective type。
  - 新增 `ScenarioJsonLoader`，支援 data validation、enum normalization 與 cross-reference validation。
  - 新增 Start／SetFact／AddFact commands 與 `ScenarioCommandRouter`，Player／AI／Scenario／Test 共用 CommandBus flow。
  - 新增 scenario lifecycle／objective／action events；`EmitSignal` 提供 start setup、劇情與 Gameplay command composition hook。
  - 新增 Conquest、Siege、Defense、Survival 四份 JSON 關卡，不含任何關卡專屬 C#。
  - `Sandbox_AI` 加入 `ScenarioSandboxBootstrap` 與四個 TextAsset references，以 generic driver 完成四種 modes。
  - 新增 11 個 EditMode cases、1 個 PlayMode case，更新 07、20、26 與本進度文件。
- Architecture / API / Data：
  - Scenario core 只擁有流程 facts 與 objective truth；Combat／Economy／Siege／Settlement 等仍是各自 authoritative owner。
  - Game-specific events 由 composition adapter 轉成 stable fact ID；scenario actions 以 event／CommandBus 邊界驅動外部系統。
  - GameMode allowed systems 是 composition gate；核心不直接引用或停用具體 gameplay service。
  - JSON definition 與 runtime state 分離；snapshot 可直接供後續 Objective UI、Save／Replay 與 debug tools。
- Tests / Validation：
  - Unity EditMode Test Runner：PASS，117/117 passed、0 failed；Phase 11 新增 11 cases。
  - Unity PlayMode Test Runner：PASS，14/14 passed、0 failed；Phase 11 新增 1 Sandbox case。
  - 四 JSON 載入與 generic completion、Siege trigger chain、Defense hold reset／defeat、Survival timer、CommandBus／events：PASS。
  - JSON syntax、`git diff --check`、Gameplay UnityEngine reference scan：PASS。
- Known Issues / Risks：
  - 尚未提供 Scenario custom editor、graph view 或 JSON schema autocomplete；目前由 loader 與 tests 驗證資料。
  - 核心一次管理一個 active scenario；campaign graph、parallel scenario instances 與 checkpoint 尚未實作。
  - 外部 Gameplay event 到 fact ID 的 mapping 由 composition layer 定義；正式 vertical slice 仍需建立 production mappings。
- Next：
  - 進入 Phase 12 UI／UX，使用 ScenarioSnapshot／events 實作 Objective panel、Victory／Defeat 與 notification presentation。

## 2026-08-11 — Phase 10 AI

- Status：Completed
- Goal：完成 Strategic／Operational／Tactical／Unit 四層 Utility AI，驗收 AI 自主經濟、招兵、組軍、移動、攻城、破口、佔領，並長時間無 deadlock。
- Changed：
  - 新增 `AiSystem`、`UtilityAiPlanner`、AI profiles、world blackboard、action scores、agent snapshots 與 decision events。
  - 新增 Economy／Expand／Attack／Defend／Recover strategic goals，以及四層共 15 種 actions。
  - 新增 `AiProfileDefinition`，Content Pack／JSON loader／validator／catalog 與三個 demo packs 接入 personality data。
  - 新增 `IAiWorldQuery` 與 `IAiActionExecutor`，AI 只讀 Query 並由 composition adapter 派送既有 commands。
  - 新增 `AiStrategicMapAnalyzer`，依 territory value 選擇目標並用 deterministic BFS 產生 route。
  - 新增 interval throttling、stable tie-break、progress tracking 與 stall threshold Recover 機制。
  - `Sandbox_AI` 實際組裝 Economy、Recruitment、Army、Combat、Siege、Settlement、Territory 與 CommandBus，並新增 goal／scores／target／strength／threat／route HUD。
  - 新增 11 個 EditMode cases、2 個 PlayMode cases，更新 07、19、26 與本進度文件。
- Architecture / API / Data：
  - 四層 AI 是 action responsibility taxonomy；單一 Utility planner 評分，不建立四個互相耦合的 managers。
  - AI core 不直接依賴任何具體 Gameplay system；world query／action executor 是 authoritative state 與 commands 的 adapter boundary。
  - AI personality 全部由 Content Pack data 控制；Three Kingdoms aggressive warlord 與 Fantasy arcane siege AI 共用核心。
  - Debug snapshot 完整公開 goal、scores、target、strength、threat、route 與 stalled count，方便調整與回放診斷。
- Tests / Validation：
  - Unity EditMode Test Runner：PASS，106/106 passed、0 failed；Phase 10 新增 11 cases。
  - Unity PlayMode Test Runner：PASS，13/13 passed、0 failed；Phase 10 新增 2 Sandbox_AI cases。
  - 1000 decision 純 C# 長跑與 Sandbox 5 秒長跑：PASS；capture 後 HoldPosition、stall count=0。
  - Profile validation、四層 scores、interval、target／route、Recover、完整 economy→capture command flow：PASS。
- Known Issues / Risks：
  - Strength／threat 是可替換的聚合值，尚未納入兵種相剋、terrain、morale、supply 與 technology modifiers。
  - Tactical actions 目前輸出高階 intent；focus fire、flank、cover 與局部 micro 留給後續 adapter／production tuning。
  - Utility score curves 目前是 deterministic baseline，尚未提供 Editor 曲線調整與難度 presets。
- Next：
  - 進入 Phase 11 GameMode／Scenario／Objective，使用 AI、Siege、Economy events 組合勝敗與劇本流程。

## 2026-08-11 — Phase 09 Siege / 城池攻防

- Status：Completed
- Goal：完成資料驅動城防結構、Gate 狀態、破口與 navigation refresh、攻城區域推進及 Settlement capture，驗收 Attacker 破門、入城、佔領與 owner change。
- Changed：
  - 新增 `SiegeSystem`、`ISiegeQuery`、Siege profiles／snapshots、七個 areas、七種 lifecycle states 與六種 game modes。
  - 新增 Wall／Gate／Tower／Barricade／Trap／Core／Extension 防禦結構、runtime HP／armor 與 Gate state machine。
  - 新增 `DefenseStructureDefinition`，Content Pack／JSON loader／validator／catalog 與三個 demo packs 接入世界觀專屬 gate data。
  - 新增七種 Siege commands 與 `SiegeCommandRouter`；Player／AI／Scenario／Test 共用 validation／handler flow。
  - 新增 `CombatSiegeAttackerQuery`，以既有 Unit tags＋AttackProfile 攻擊結構，不建立第二套 SiegeUnit combat state。
  - 新增 `BreachCreatedEvent` 與 `ISiegeNavigationSink`；Gate／Wall 摧毀後要求 navigation backend refresh。
  - 新增 `SiegeCombatEventBridge`，把 defender／commander deaths 轉成 capture conditions。
  - 新增 `SettlementSiegeCaptureSink`，重用 Phase 07 capture transaction 同步 Settlement／Faction／Territory owner。
  - `Sandbox_Siege` 加入攻城城牆、破壞後 Gate、capture objective、自動攻城 acceptance 與 HUD。
  - 新增 15 個 EditMode cases、2 個 PlayMode cases，更新 07、18、26 與本進度文件。
- Architecture / API / Data：
  - `SiegeSystem` 擁有 siege／structure runtime truth；Combat 仍擁有 unit combat，Settlement 仍擁有 capture／ownership transaction。
  - 跨系統只經 attacker query、navigation sink、capture sink 與 event bridge；Gameplay 維持 Pure C#、`noEngineReferences=true`。
  - DefenseStructure type 支援 extension ID；Three Kingdoms city gate 與 Fantasy arcane gate 共用同一 runtime code。
  - Assault／Defense／WaveDefense／Survival／EscortSiege／BossSiege 是 profile data；scenario-specific 行為由 `ISiegeRule` 與既有系統組合。
- Tests / Validation：
  - Unity EditMode Test Runner：PASS，95/95 passed、0 failed；Phase 09 新增 15 cases。
  - Unity PlayMode Test Runner：PASS，11/11 passed、0 failed；Phase 09 新增 2 Sandbox_Siege cases。
  - Break Gate→Breach event→Navigation refresh→InnerArea→CaptureObjective→Settlement／Territory owner change：PASS。
  - Gate transitions、armor damage、target tags、death conditions、Wave／Survival completion、六種 mode、router disposal：PASS。
- Known Issues / Risks：
  - Sandbox navigation 使用 recording sink；正式 NavMesh carve／surface rebuild 與所有移動中單位 repath 尚待 Unity adapter。
  - Gate opening／closing 動畫時間、collider 與視覺狀態尚未接 Presentation；核心狀態轉移已完成。
  - Trap、Tower targeting、Escort payload 與 Boss mechanics 需由 Combat／Movement／Scenario composition 實作。
- Next：
  - 進入 Phase 10 AI，讓 AI 讀取 siege／territory／economy query 並執行攻防決策。

## 2026-08-11 — Phase 08 Economy / Recruitment / Building / Technology

- Status：Completed
- Goal：完成資料驅動的資源錢包、週期產出、建造、研究與招募流程，並驗收不同世界觀的資源 ID 不需修改核心程式。
- Changed：
  - 新增 `ResourceWallet` 與 `EconomySystem`，以 `DefinitionId` 管理原子成本扣除、帳戶、週期產出與 optional population accounting。
  - 新增 `BuildingSystem`、`TechnologySystem`、`RecruitmentSystem`，分別實作 request／validate／cost／queue／timer／completion 流程。
  - 新增 Build、Research、Recruit commands 與各自的 CommandBus routers，供 Player／AI／Scenario／Test 共用。
  - 建築支援 building／technology prerequisites、resource production 與 population capacity effects；已建成狀態作為後續內容 unlock 條件。
  - 科技支援 DAG prerequisite 驗證、每 Faction 完成狀態與 additive／multiplicative modifier registry。
  - `UnitDefinition`、`BuildingDefinition`、`TechnologyDefinition`、JSON loader／validator 與三個 demo Content Packs 新增時間、人口、前置條件、產出與 modifier authoring data。
  - 新增 `GameplayEconomyStateBridge`，把資源、建築與科技完成結果投影回既有 Faction／Settlement read models。
  - `Sandbox_Siege` 加入自動建造→研究→招募 acceptance 與 Phase 08 debug HUD。
  - 新增 8 個 EditMode cases 與 1 個 PlayMode case，更新 07、26 與本進度文件。
- Architecture / API / Data：
  - `EconomySystem` 是資源與人口規則的 authoritative owner；Faction／Settlement 狀態透過 sink bridge 同步，不反向依賴 Unity。
  - Definition 保存 immutable authoring data，runtime systems 保存 queue／timer／completion state，Presentation 只負責 spawn／visual adapter。
  - Building／Technology／Recruitment 彼此僅依賴 query／sink interface，沒有集中成 God Manager；全部 Gameplay 程式維持 Pure C# 與 `noEngineReferences=true`。
  - Resource ID 只來自 Content Pack；Neutral／Three Kingdoms／Fantasy 分別使用 supplies／provisions／mana，但共用同一套核心流程。
- Tests / Validation：
  - Unity EditMode Test Runner：PASS，80/80 passed、0 failed；Phase 08 新增 8 cases。
  - Unity PlayMode Test Runner：PASS，9/9 passed、0 failed；新增 Sandbox_Siege 完整 production pipeline case。
  - Atomic spend、resource income、building effects、technology DAG／modifier、population switch、command rejection、timed spawn：PASS。
  - Fantasy acceptance：PASS；`fantasy.mana` 完成建造、研究、招募，不含世界觀分支。
- Known Issues / Risks：
  - Unit completion 目前經 `IUnitSpawnSink` 交給 composition layer；尚未串接正式 entity factory、spawn point 與 rally point。
  - Upkeep 未啟用；Population 已由 Phase 08 rule switch 控制，Supply 延用 Phase 06 optional Army rule。
  - Production queues 目前可平行推進；若遊戲設計要求單一建造／研究槽，需在後續加入 queue lane policy。
- Next：
  - 進入 Phase 09 Siege / Defense，將建築、城池防禦與戰鬥目標串成攻城流程。

## 2026-08-11 — Phase 07 Faction / Settlement / Territory

- Status：Completed
- Goal：完成 Faction runtime state、Settlement ownership／capture、Territory graph／visibility／value，並驗收三座 settlement 變更 owner 後 Faction territory 自動更新。
- Changed：
  - 新增 `FactionSystem`、Faction profiles／snapshots、resources、technology、diplomacy、AI profile 與 ownership indices。
  - 新增 `FactionArmyEventBridge`，從 Army create／split／merge events 維護 Faction army index。
  - 新增 `TerritorySystem`、territory node／connection、owner、visibility、value 與 settlement mapping。
  - 新增 `SettlementSystem`、settlement runtime state、五種 capture rules、`CaptureSettlementCommand` 與 router。
  - 新增 `SettlementArmyTargetValidator`，補強 AttackSettlement 的 existence／ownership／diplomacy validation。
  - `SettlementDefinition`、JSON loader／validator 與三個 Content Packs 新增 population、defense、capture rule／conditions。
  - `Sandbox_Siege` 新增三座 settlement、三個 connected territory nodes、自動 capture acceptance 與 debug HUD。
  - 新增 12 個 Phase 07 EditMode cases、2 個 PlayMode cases，更新 07、16、26 與本進度文件。
- Architecture / API / Data：
  - Settlement capture 是 ownership transaction entry；Settlement、Faction settlement index、Territory、Faction territory index 依序同步。
  - Faction／Settlement／Territory 都是 Pure C#，Gameplay 保持 `noEngineReferences=true`；Sandbox 只負責 composition 與 visual。
  - Capture conditions 由上游 Combat／Siege 系統提供 flags；capture rule 不依賴世界觀名稱。
  - Army settlement target validation 經 interface 注入，不讓 ArmySystem 直接依賴 SettlementSystem concrete type。
- Tests / Validation：
  - Unity EditMode Test Runner：PASS，72/72 passed、0 failed；Phase 07 新增 12 cases。
  - Unity PlayMode Test Runner：PASS，8/8 passed、0 failed；Phase 07 新增 2 Sandbox_Siege cases。
  - 三 settlement ownership acceptance：PASS；Faction A 的 settlement／territory 清空，Faction B 自動取得三座 settlement 與三個 territory nodes。
  - Capture rules、invalid command、Faction state、territory graph／visibility、settlement state、army bridge、AttackSettlement diplomacy validation：PASS。
- Known Issues / Risks：
  - Settlement resources／buildings／recruitment 尚未執行成本、時間或產出規則，待 Phase 08。
  - Territory visibility 是明確設定的狀態，尚未串 Fog of War 探索／視野傳播。
  - Capture completed conditions 尚未由實際 Siege objectives 產生，待 Phase 09。
- Next：
  - 進入 Phase 08 Economy / Recruit / Build / Tech，使用既有 Faction／Settlement runtime state 實作成本與生產流程。

## 2026-08-11 — Phase 06 Hero / Army / Command

- Status：Completed
- Goal：以 unit entity 上的 Hero component 建立 leadership／ability 資料，完成 Army composition、commander、optional morale／supply 與九種共用 commands，驗收 Hero + 20 infantry 建軍、拆分、合併與換 commander。
- Changed：
  - 新增 `HeroProfile`、`HeroSnapshot`、`IHeroQuery` 與 `HeroSystem`，不建立第二套 Combat。
  - `HeroDefinition`、JSON loader、validator 與三個 demo Content Packs 新增 world-neutral `leadership`。
  - 新增 `ArmySystem`、army models／snapshots／events、unit membership、commander 與 optional morale／supply。
  - 新增 Create／Merge／Split／AssignCommander／Move／Attack／AttackSettlement／Defend／Retreat commands 與 `ArmyCommandRouter`。
  - 新增 `IArmyOrderExecutor`、`GameplayArmyOrderExecutor`，串接既有 Movement／Combat API。
  - 新增 `IArmyMembershipSink` 與 `CombatArmyMembershipSink`，讓軍團拆分／合併後 Combat snapshot 的 ArmyId 保持同步。
  - `Sandbox_Combat` 加入獨立 `ArmySandboxBootstrap`、21 actors、command/event counters 與 debug HUD。
  - 新增 8 個 Phase 06 EditMode cases、2 個 PlayMode cases，更新 07、15、26 與本進度文件。
- Architecture / API / Data：
  - Hero 是 unit entity 的 supplementary component；Combat／Movement state 仍由既有系統擁有。
  - ArmySystem 是 composition authoritative owner，跨系統同步透過 sink，不直接持有 Unity object。
  - 所有 army commands 走同一個 CommandBus validator／handler flow；非法跨 faction merge、非 hero commander、重複 membership 在 mutation 前拒絕。
  - Army order execution 經 `IArmyOrderExecutor` adapter；state-only tests／sandbox 與 production Movement＋Combat coordinator 可替換。
- Tests / Validation：
  - Unity EditMode Test Runner：PASS，60/60 passed、0 failed；Phase 06 新增 8 cases。
  - Unity PlayMode Test Runner：PASS，6/6 passed、0 failed；Phase 06 新增 2 Sandbox cases。
  - Hero + 20 infantry acceptance：PASS；21 members 建軍、10 members 拆分、合併回 21 members、commander change、membership propagation 全部通過。
  - 九種 command routing、invalid validation、optional morale／supply、event flow、router disposal：PASS。
- Known Issues / Risks：
  - Defend 保存 defense order 並移動至指定點；arrival 後 hold／engagement policy 尚待 coordinator。
  - Morale／Supply 消耗與潰退門檻尚未接 Economy／AI／Scenario rules。
  - AttackSettlement 目前由 Phase 05 Combat target 處理；settlement type／ownership validation 待 Phase 07。
- Next：
  - 進入 Phase 07 Faction / Settlement / Territory，建立 ownership query 並補強 AttackSettlement validation。

## 2026-08-11 — Phase 05 Unit Combat / Ability

- Status：Completed
- Goal：完成 unit runtime combat state、近戰／遠程攻擊、projectile／splash、傷害管線、能力目標與啟動分類、status effects 與 death flow。
- Changed：
  - 新增 Pure C# `CombatSystem`、`ICombatQuery`、combat profiles／snapshots 與 combat events。
  - 完成 Base → modifier → armor → resistance → shield → final damage → HP → death pipeline。
  - 完成 melee range／windup／cooldown、ranged projectile travel、enemy-only splash 與 target tag filtering。
  - 完成 buff、debuff、stun、slow、root、shield、DoT；修正 DoT 與 shield 同 tick 改動狀態清單的安全性。
  - 新增 `AbilityProfile`、ability target／activation enums、`UseAbilityCommand` 與 `AbilityUsedEvent`；Active／Toggle 支援手動施放與 cooldown。
  - 新增 `UnityCombatDriver`、`UnityCombatView`，提供 event-driven projectile visual、血條、受傷與死亡外觀。
  - 完成 `Sandbox_Combat` composition root 與自動 acceptance scenario，共 6 個 combatants。
  - 新增 8 個 EditMode combat／ability tests 與 2 個 PlayMode Sandbox tests。
  - 更新 07、14、26 與本進度文件。
- Architecture / API / Data：
  - Gameplay 保持 `noEngineReferences=true` 且只依賴 Core；authoritative HP、status、cooldown、projectile 全部位於 `CombatSystem`。
  - Unity view 只讀取 `CombatantSnapshot`，不持有 HP truth；projectile GameObject 是 simulation event 的短期視覺回饋。
  - FactionId 用於敵我篩選，ArmyId 保留歸屬；不提前耦合 Phase 06／07 service。
  - 大 delta 下先推進既有 projectile，再建立新 projectile，避免新投射物同 tick 消耗完整 delta 而瞬移命中。
- Tests / Validation：
  - Unity EditMode Test Runner：PASS，52/52 passed、0 failed；Phase 05 新增 8 cases。
  - Unity PlayMode Test Runner：PASS，4/4 passed、0 failed，含既有 50-unit movement acceptance 與新 combat scene acceptance。
  - Combat acceptance：PASS；melee、ranged、projectile、splash、target tags、status、ability cooldown、DoT、death event 均有測試。
  - Unity scripts compile：PASS；Gameplay forbidden `UnityEngine` reference scan：PASS；`git diff --check`：PASS。
- Known Issues / Risks：
  - Passive／Aura／Triggered 目前定義 activation type，但自動觸發／refresh policy 留給 Hero／Army／AI 規則層注入。
  - Direction target 已保存方向資料；cone／line shape 尚待 spatial query。
  - Combat out-of-range state 會維持 `Targeting`；追擊至攻擊距離仍需 Movement／Combat coordinator。
  - Projectile simulation 追蹤移動目標；目前 Unity projectile visual 使用發射當下 destination，production VFX 可改為追蹤 view。
- Next：
  - 進入 Phase 06 Hero / Army / Command，並建立 Movement／Combat 的上層協調與能力觸發來源。

## 2026-08-11 — Phase 04 Movement / Navigation / Formation

- Status：Completed
- Goal：建立 destination validation、unreachable、queue、repath、stuck detection、local avoidance 與 Line／Box formation，並驗證 50 units 可繞過障礙且不大量永久卡死。
- Changed：
  - 新增 Pure C# `FormationPlanner`、Line／Box formation types 與 deterministic `FormationSlot`。
  - 新增 `INavigationAdapter`、navigation result／snapshot contracts、`MovementSystem` 與 movement state snapshots。
  - `MoveUnitsCommand` 新增 optional `FormationType`；context resolver 與 input adapter 支援 formation，`Tab` 切換 Box／Line。
  - 新增 `NavMeshMovementAdapter` 與 `UnityMovementDriver`，完成 destination sampling、complete-path validation、NavMeshAgent local avoidance、repath／stuck feedback 與 path／destination／velocity gizmos。
  - `Sandbox_RTS` 改為 runtime NavMeshSurface、3 組 obstacle、50 friendly agents、movement HUD 與可由測試派發的 acceptance command。
  - Demo asmdef 新增 `Unity.AI.Navigation` reference；package 版本未調整。
  - 新增 7 個 Phase 04 EditMode cases，並將 PlayMode 擴充為 50-agent composition 與跨障礙 acceptance，共 2 cases。
  - Unity Editor 首次載入產生標準 `ProjectSettings/SceneTemplateSettings.json`，納入版本控制以維持 ProjectSettings 完整性。
  - 更新 13、26 與本進度文件。
- Architecture / API / Data：
  - Gameplay 只依賴 Core 且保持 `noEngineReferences=true`；`MovementSystem` 不持有 Transform、GameObject、NavMeshAgent。
  - `INavigationAdapter` 讓未來 grid／flow-field／server navigation 可替換 Unity NavMesh，不改 command 或 movement state API。
  - formation assignment 依 EntityId 排序且每 actor 使用 distinct slot；Box 避免大量單位集中同一 destination。
  - Bootstrap 只組合 runtime NavMesh、services 與 demo actors；frame tick 由獨立 `UnityMovementDriver` 負責。
  - local avoidance 留在 Unity adapter；order queue、arrival、unreachable、repath 與 stuck transition 由 Gameplay 擁有。
- Tests / Validation：
  - Unity 6000.5.7f1 netstandard 2.1 compatibility build：PASS；Core、Gameplay、Presentation、Demo、全部 EditMode／PlayMode source，0 warnings、0 errors。
  - `dotnet format ... --verify-no-changes`：PASS；`git diff --check`：PASS。
  - Unity EditMode Test Runner：PASS，44/44 passed、0 failed；Phase 04 新增 7 cases，涵蓋 Line／Box、50 distinct slots、unreachable、queue、repath／stuck、stop／hold。
  - Unity PlayMode Test Runner：PASS，2/2 passed、0 failed，總時間約 15.69 秒。
  - 50-agent acceptance：PASS；所有 agents 收到 distinct Box destinations，15 秒後至少 40 個已跨過中央障礙，`Stuck`／`Unreachable` 不超過 5。
  - 靜態 dependency／asset validation：PASS；Gameplay Unity references=0、Gameplay asmdef 只依賴 Core、Demo 明確依賴 `Unity.AI.Navigation`、177 unique asset GUIDs。
- Known Issues / Risks：
  - Sandbox 為驗收方便在 runtime synchronous build NavMesh；大型 production map 應使用 pre-baked data 或受控 async update，避免載入尖峰。
  - Unity NavMeshAgent local avoidance 並非 deterministic simulation；Replay／lockstep 策略需在 Phase 13 明確界定記錄層級。
  - Attack／Follow／Interact 目前仍只派發 intent；追擊、跟隨與接近互動目標會在 Combat／Army phase 接入 MovementSystem。
  - 50-unit Line formation 可能超出小型可走區域而被 adapter 拒絕；production formation planner 後續可加入 bounds-aware wrapping。
  - Unity 專案版本 `6000.5.7f1` 與文件中的 Unity 6.3 LTS 名稱仍需確認是否為同一發行基線。
- Next：
  - 進入 Phase 05 Unit Combat / Ability，將 attack range approach 與 movement stop conditions 接入共用 command flow。

## 2026-08-11 — Phase 03 RTS Input / Selection / Camera

- Status：Completed
- Goal：完成 RTS 相機、click／box selection、Shift add/remove、double-click same type、control groups 與 context command，並建立 20 debug units 的可操作驗收場景。
- Changed：
  - 在 Gameplay 新增 Unity-independent `WorldPoint` 與 Move／Attack／Follow／Interact／Stop／Hold commands，維持 Player／AI 共用 `CommandBus`。
  - 在 Presentation 新增 pure C# `SelectionService`、`ContextCommandResolver`、`RtsCameraRigModel`，以及 Unity selectable、input、camera adapters。
  - 新增 `AegisRTS_RTS.inputactions`，包含 Point、Select、AddSelection、Command、CameraMove、CameraZoom、ControlGroup、QueueCommand、Stop、Hold 與 FocusSelected。
  - 將 Presentation asmdef 加入 `Unity.InputSystem`，EditMode tests 加入 Presentation reference。
  - `Sandbox_RTS` 新增 composition bootstrap，runtime 建立 ground、20 friendly debug units、friendly／enemy／settlement context targets、camera、input 與 command diagnostics UI。
  - 新增 10 個 Phase 03 EditMode test cases 與 1 個 PlayMode scene composition test；更新 12、26 與本進度文件。
- Architecture / API / Data：
  - Gameplay commands 不依賴 Unity；選取狀態與相機 bounds 可脫離 MonoBehaviour 測試。
  - `UnityRtsInputAdapter` 專責 Input System、screen-space box、raycast 與 adapter dispatch；`RtsSandboxBootstrap` 僅負責 composition 與 acceptance actors。
  - Context mapping 固定為 Ground→Move、Enemy→Attack、Friendly→Follow、Settlement→Interact；Stop／Hold 走同一 CommandBus。
  - Control groups 保存 EntityId snapshot，recall 時自動忽略已 unregister 的 entity。
  - 本次正式 Unity EditMode 結果涵蓋 Phase 01–03，因此關閉 Phase 01／02 舊紀錄中的 Test Runner 阻塞；舊紀錄保留當時狀態，不回寫歷史。
- Tests / Validation：
  - Pure C# validation harness：PASS，smoke 9/9；反射執行全部實際 EditMode NUnit cases：PASS，37/37，其中 Phase 03 為 10 cases。
  - Unity 6000.5.7f1 netstandard 2.1 compatibility build：PASS；Core、Gameplay、Presentation、Demo、全部 EditMode／PlayMode test source，0 warnings、0 errors。
  - `dotnet format ... --verify-no-changes`：PASS。
  - Input Actions JSON 與 asmdef JSON parse：PASS；11 actions（含額外 FocusSelected）與 10 個必要 action names 齊全。
  - 靜態 scene／asset acceptance：PASS；`Sandbox_RTS` 引用 bootstrap GUID、scene 位於 Build Settings、170 unique asset GUIDs、inputactions importer GUID 正確。
  - Unity EditMode Test Runner：PASS，37/37 passed、0 failed／skipped／inconclusive；同時正式複驗 Phase 01／02 cases。
  - Unity PlayMode Test Runner：PASS，`SandboxRts_ComposesTwentyDebugUnitsSelectionInputAndCamera` 1/1 passed、0 failed；場景載入後沒有未處理 exception。
  - 初次 Unity batch import 曾卡在 Bee ScriptAssemblies rebuild，已停止程序；完成 import cache 後重跑即正常編譯並完成兩種 Test Runner。
- Known Issues / Risks：
  - 自動測試已覆蓋 selection／command／camera domain 與 Sandbox composition；滑鼠框選、edge pan 與 middle drag 的實際操作手感仍建議在互動式 Editor 做 exploratory tuning。
  - 目前 command handler 只顯示已派發的 debug summary；實際 pathfinding、movement 與 formation execution 屬 Phase 04。
  - Input action asset 是 authoring contract；Sandbox runtime 建立同名 action map 以免依賴 Inspector wiring。後續若調整 bindings，兩者需同步，或導入 generated wrapper 作單一來源。
  - Unity 專案版本 `6000.5.7f1` 與文件中的 Unity 6.3 LTS 名稱仍需確認是否為同一發行基線。
- Next：
  - 可先在互動式 Unity Editor 做 Sandbox 操作手感複驗，再進入 Phase 04 Movement / Pathfinding / Formation。

## 2026-08-11 — Phase 02 Data-Driven / Content Pack

- Status：Blocked
- Goal：建立通用 Definition、GameRuleSet、JSON Content Pack、typed catalog 與完整資料驗證，證明同一 Framework 可切換三種世界觀資料。
- Changed：
  - 在 `AegisRTS.Gameplay.Content` 新增 25 個 Pure C# source files，包含 immutable definitions、GameRuleSet、JSON loader、validator、typed catalog 與 atomic pack service。
  - 將 `AegisRTS.Gameplay.asmdef` 設為 `noEngineReferences: true`，依賴維持只有 `AegisRTS.Core`。
  - 新增 `DemoNeutral`、`DemoThreeKingdoms`、`DemoFantasy` 三個 `ContentPack.json`；每個 pack 各含 7 個 definitions 與一套 rules。
  - 新增 4 個共用 placeholder prefab assets，供 prefab ID existence validation。
  - 新增 5 個 Phase 02 EditMode test files（含 test factory），並更新 02、07、08、26 文件。
  - 為所有新增 Unity folders、scripts、JSON 與 prefab 建立並驗證 `.meta`。
- Architecture / API / Data：
  - Gameplay 僅理解通用 Definition、Tag、typed reference 與 prefab asset ID；世界觀名稱、數值與 rules 只存在 JSON Content Pack data。
  - `DefinitionId` 與 `ContentTag` 正規化成穩定 lowercase value；reference 不依賴 display name。
  - `ContentPackValidator` 回報 duplicate ID、missing typed reference、invalid stat／cost、technology cycle、missing prefab／tag，不在第一個錯誤停止。
  - `ContentPackService.Load` 驗證成功才切換 `ActiveCatalog`；invalid pack 保留前一個 catalog。
  - `IContentAssetCatalog` 是 Unity asset lookup adapter boundary，definitions 不持有 GameObject。
  - Phase 01 Unity Test Runner 尚未複驗；依使用者明確指示先繼續 Phase 02，原有阻塞紀錄保留。
- Tests / Validation：
  - `dotnet build Temp/Phase01Validation/Phase01Validation.csproj --configuration Release --no-restore`：PASS；Core、Gameplay 與實際 NUnit source 一起編譯，0 warnings、0 errors。
  - Unity netstandard 2.1 compatibility build（使用 Unity 6000.5.7f1 隨附的 `System.Text.Json` reference）：PASS，0 warnings、0 errors。
  - validation harness smoke tests：PASS，9/9。
  - 反射執行實際 NUnit `[Test]`／`[TestCase]`：PASS，27/27；其中 Phase 02 為 12 cases。
  - `dotnet format ... --verify-no-changes`：PASS。
  - 三個實際 JSON packs deserialize、完整 validation、依序切換與 typed lookup：PASS。
  - 靜態 Acceptance：PASS；25 Gameplay files、3 packs、160 unique asset GUIDs，Gameplay asmdef 只依賴 Core、`noEngineReferences=true`，沒有 Unity／Demo／Presentation dependency 或世界觀 hardcode。
  - Unity EditMode Test Runner：未執行；本次工作階段中 Unity CLI 已確認在進入 runner 前受 Licensing Client IPC 逾時阻擋。
- Known Issues / Risks：
  - 需在已登入 Unity Hub 的互動式 Editor 中確認 Console 無 error，並執行全部 27 個 tests；完成前不宣稱 Phase 01／02 完整符合 Definition of Done。
  - 四個 prefab 是只有 root Transform 的資料驗證 placeholder，尚未包含正式 Visual、Collider 或 View Components。
  - Unity 專案版本 `6000.5.7f1` 與規格基準 Unity 6.3 LTS 仍不一致。
- Next：
  - 從 Unity Hub 開啟專案，執行全部 EditMode tests 並確認 Bootstrap Console；通過後將 Phase 01／02 Status 改為 `Completed`，再進入 Phase 03。

## 2026-08-11 — Phase 01 Core 基礎設施

- Status：Blocked
- Goal：實作 Entity ID、GameClock、Seeded Random、Command Bus、Event Bus、State Machine 與 Diagnostics，並完成 Phase 01 測試與驗收。
- Changed：
  - 在 `AegisRTS.Core` 新增 20 個 Pure C# source files，完成 Entity ID、GameClock、Seeded Random、Command Bus、Event Bus、State Machine 與 Diagnostics。
  - 將 `AegisRTS.Core.asmdef` 設為 `noEngineReferences: true`。
  - 新增 7 個 EditMode test files，共 15 個 NUnit test／test case attributes。
  - 更新 `docs/26_Framework_API_目標介面.md`，記錄 Phase 01 public API、生命週期、determinism 與 threading 邊界。
  - 為所有新增 Unity assets 建立並驗證 `.meta`。
- Architecture / API / Data：
  - `AegisRTS.Core` 無 assembly references 且禁止 UnityEngine reference；Core 沒有 Gameplay／Presentation using。
  - `ICommand`／`CommandBus` 提供 Player、AI、Scenario、Test 共用的 validation 與 dispatch flow。
  - `IEvent`／`EventBus` 提供同步、依註冊順序且可安全 unsubscribe 的 event flow。
  - `IRandomSource`／`SeededRandom` 使用固定 PCG sequence，並以 reference vector test 防止演算法意外變更。
  - `IDiagnosticSink` 隔離 logging adapter；`DiagnosticBuffer` 是 bounded、thread-safe history。
- Tests / Validation：
  - `dotnet build Temp/Phase01Validation/Phase01Validation.csproj --configuration Release --no-restore`：PASS，Core 與實際 NUnit source 一起編譯，0 warnings、0 errors。
  - `dotnet run ... --no-build`：PASS，7/7 等價行為測試通過，涵蓋 Phase 01 所有指定測試面向。
  - `dotnet format ... --verify-no-changes`：PASS。
  - 靜態 Acceptance：PASS；20 Core files、7 test files、119 unique asset GUIDs，Core asmdef references=0、`noEngineReferences=true`、沒有 forbidden using。
  - Unity EditMode Test Runner：未執行；Unity 在進入 test runner 前因 Licensing Client IPC 連線逾時而持續重試，沒有產生 test result XML。
- Known Issues / Risks：
  - 需在已登入 Unity Hub 的互動式 Editor 中確認 Console 無 error，並執行 15 個 NUnit test／test cases；完成前不宣稱 Phase 01 完整符合 Definition of Done。
  - Unity 專案版本 `6000.5.7f1` 與規格基準 Unity 6.3 LTS 仍不一致。
- Next：
  - 從 Unity Hub 開啟專案，執行 EditMode tests 並確認 Bootstrap Console；通過後把本筆 Status 改為 `Completed`，再進入 Phase 02。

## 2026-08-11 — 建立 Development Progress 規範

- Status：Completed
- Goal：建立每次 repository 開發都必須同步留下可驗證進度的統一規範與紀錄檔。
- Changed：
  - 新增 `docs/09_DevelopmentProgress_開發進度紀錄規範.md`。
  - 新增根目錄 `DevelopmentProgress.md`。
  - 更新開發總覽、Git 規範、Definition of Done 與 Agent prompts。
- Architecture / API / Data：
  - N/A；此工作只調整開發治理與文件，不修改 runtime architecture、public API 或資料格式。
- Tests / Validation：
  - 確認 `DevelopmentProgress.md` 位於 Repository Root，且未被 `.gitignore` 排除：PASS。
  - 確認總覽、Git、DoD 與 Agent 規則都有連結或強制更新條款：PASS。
- Known Issues / Risks：
  - Unity 專案版本 `6000.5.7f1` 與規格基準 Unity 6.3 LTS 不一致，尚未決定升降版策略。
- Next：
  - 開始 Phase 01 前先閱讀最新進度，並在每次實作完成時同步更新本檔。

## 2026-08-11 — Project Initialization

- Status：Completed
- Goal：依 01–08 規格完成 Unity FrameworkLab 初始目錄、assembly、場景與 Git repository。
- Changed：
  - 建立 `Assets/AegisRTS` Framework、Content、Demo 與 Tests 目錄。
  - 建立 8 個 asmdef 與 5 個 Bootstrap／Sandbox 場景。
  - 建立根目錄 README、Git ignore／attributes，並初始化 `main` branch。
- Architecture / API / Data：
  - 建立 `Core → Gameplay → Presentation/Persistence → Demo/Tools` 的初始 assembly dependency。
  - 尚無 runtime public API 或核心資料模型實作。
- Tests / Validation：
  - 目錄、asmdef、scene GUID 與 Build Settings 靜態驗證：PASS。
  - `dotnet build AegisRTS.FrameworkLab.slnx --no-restore`：PASS，0 warnings、0 errors。
  - Unity CLI Console／Play Mode：未完成；本機 Licensing Client 連線逾時。
- Known Issues / Risks：
  - Unity 專案版本與規格基準不一致。
  - 仍需使用已登入 Unity Hub 的互動式 Editor 驗證 Bootstrap Play Mode。
- Next：
  - 確認 Unity 版本策略與 Editor 驗證後進入 Phase 01。
- Related Commit：`196975d Finish project init.`
