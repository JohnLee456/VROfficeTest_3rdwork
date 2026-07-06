# Study 2 WoZ 实验：Experimental Staff Episode Prompt Boards（含具体讨论 Item 版本）

## 0. 使用说明

本文件用于给 experimental staff 在 Study 2 WoZ 实验中快速查看每个阶段/episode 的执行提示。

提示板不是逐句台词脚本，而是提醒 staff：

- 当前 episode 持续多久；
- 当前 discussion context 是什么；
- 当前具体讨论哪个 survival item；
- 当前 item 需要讨论哪些方面；
- 本 episode 的任务是什么；
- 谁是 target member / active speaker / waiting member；
- 被 leader 邀请后应如何自然回应。

本实验脚本不要求 staff 记忆固定台词，也不要求表演复杂非语言行为。Staff 只需围绕当前 survival item 的用途、重要性、优先级、风险和取舍进行自然讨论，并遵守当前 episode 的角色分工。

---

# 1. Item 选择原则

本版本将每个 episode 绑定到一个具体 survival item。这样做的目的不是规定唯一正确答案，而是让 experimental staff 在每个 episode 中有明确、稳定、可训练的话题锚点。

每个 block 的 item 选择原则如下：

- **Island survival**：偏向荒岛生存中的饮水、工具、求救、食物获取与避难。
- **Desert survival**：偏向沙漠中“stay at crash site / conserve water / signal for rescue / avoid exposure”的讨论。
- **Mountain survival**：偏向山地或寒冷环境中的保暖、火源、遮蔽、急救、信号和能量补给。

每个 trial 讨论 4 个 items：

- Opening phase 引入本 trial 的 item subset，并开始讨论第一个 item；
- Episode 1–3 分别围绕一个具体 item 展开；
- Summary stage 总结该 trial 的 4 个 items。

---

# 2. Block 1: Speaking Context — Island Survival（荒岛求生）

## Block 1 总目标

Leader 通常是当前主要发言者。  
本 block 关注：当 leader 正在讲话时，系统反馈是否帮助 leader 更合适地把发言机会交给有说话意图的 member。

## Block 1 Item Set

### Trial 1：讨论前 4 个 items

1. Water filter（水过滤器）
2. Knife（刀）
3. Fishing rod / fishing net（鱼竿或渔网）
4. Flare gun（信号枪）

### Trial 2：讨论后 4 个 items

5. First aid kit（急救包）
6. Rope（绳子）
7. Lighter / matches（打火机或火柴）
8. Tent / hammock（帐篷或吊床）

---

## Trial 1：讨论前 4 个 survival items

### Prompt Board B1-T1-O: Opening Phase

**Time:** 0–40s  
**Context:** Speaking Context  
**Scenario:** Island survival  
**Current item:** Water filter（水过滤器）  
**Topic:** 开始荒岛求生讨论；讨论水过滤器是否应被优先选择。  
**Main Task:** 让 leader 开启讨论，并给出当前 item 的初步判断标准。

**Discussion focus:**

- 在荒岛上，饮用水是否是首要问题；
- water filter 是否比食物、工具或求救设备更重要；
- 如果附近有海水或不明淡水，water filter 的作用是什么；
- 是否需要和容器、火源等其他 item 配合。

**What staff should do:**

- A/B/C 正常进入讨论。
- 给出简短回应，例如同意、补充一个简单理由、确认任务理解。
- 不主动制造 speaking-intention event。
- 不抢 leader 的开场发言。

**Expected flow:** Leader 介绍任务 → 简短回应 → 进入 water filter 的讨论。

---

### Prompt Board B1-T1-E1: Clear Single Entry Request

**Time:** 40–100s  
**Context:** Leader speaking  
**Scenario:** Island survival  
**Current item:** Knife（刀）  
**Topic:** Leader 正在解释 knife 的重要性。  
**Main Task:** 测试 leader 是否会把话轮转交给单一 target member。

**Target assignment:**

- Target member: A
- B: normal participant
- C: normal participant

**Discussion focus:**

- knife 是否适合切割树枝、制作简易工具、处理食物；
- knife 是否有防身或急救辅助作用；
- knife 和 rope / fishing gear / shelter item 是否存在组合价值；
- 该 item 是否应排在 water filter 之前或之后。

**What staff should do:**

- A: 准备一个自然补充观点，例如“knife 可以用来制作其他工具，因此它的间接价值很高”；不要打断 leader。若 leader 邀请 A，A 正常表达观点。
- B/C: 正常听，可简短回应，不争取主要发言机会。

**Expected event:** A 是唯一明显 target。若 leader 邀请 A，则进入 expected path；若未邀请，则该机会记为 missed/unaddressed。

---

### Prompt Board B1-T1-E2: Competing Entry Requests

**Time:** 100–170s  
**Context:** Leader speaking  
**Scenario:** Island survival  
**Current item:** Fishing rod / fishing net（鱼竿或渔网）  
**Topic:** Leader 正在推进 fishing item 的讨论，两名 members 都有补充观点。  
**Main Task:** 测试 leader 是否能在两个有意图的 members 中选择更合适的发言者。

**Target assignment:**

- Higher speaking-intention target: B
- Lower speaking-intention competitor: C
- A: normal participant

**Discussion focus:**

- fishing item 是否能解决长期食物问题；
- 捕鱼是否需要技巧、时间和体力；
- 与 water filter、knife 相比，食物获取是否更紧急；
- fishing item 是否适合短期 survival ranking。

**What staff should do:**

- B: 准备主要补充观点，例如“食物不是最前几天最紧急的问题，但 fishing item 对长期生存很重要”。
- C: 准备次要补充观点，例如“如果附近鱼少，fishing item 的不确定性较高”。
- A: 正常听，可短暂回应，不加入竞争。

**Expected event:** B 是 primary target。若 leader 邀请 B，为 expected path；若邀请 C，为 target mismatch；若两者都未邀请，为 missed。

---

### Prompt Board B1-T1-E3: Repeated Single Entry Request

**Time:** 170–240s  
**Context:** Leader speaking  
**Scenario:** Island survival  
**Current item:** Flare gun（信号枪）  
**Topic:** Leader 正在总结或解释 flare gun 的求救价值。  
**Main Task:** 重复单一 entry request 结构，但更换 target member。

**Target assignment:**

- Target member: C
- A/B: normal participants

**Discussion focus:**

- flare gun 是否适合向船只或飞机求救；
- 它是否属于一次性或有限次数资源；
- 白天/夜晚使用效果是否不同；
- flare gun 与 mirror、radio 等其他求救工具相比的优先级。

**What staff should do:**

- C: 准备一个自然补充观点，例如“如果岛屿靠近航线，flare gun 的价值会更高；否则可能不如 water filter 稳定”。
- A/B: 正常听，可简短回应，不争取主要机会。

**Expected event:** C 是唯一 target。若 leader 邀请 C，则 expected path；若未邀请，则 missed/unaddressed。

---

### Prompt Board B1-T1-S: Summary Stage

**Time:** 240–300s  
**Context:** Trial wrap-up  
**Scenario:** Island survival  
**Items to summarize:** Water filter, Knife, Fishing rod/net, Flare gun  
**Topic:** 总结前 4 个 items 的阶段性排序或选择理由。  
**Main Task:** 配合 leader 收尾。

**What staff should do:**

- 简短确认讨论结果。
- 如 leader 发出最后补充邀请，可自然回应。
- 不开启新的主要 speaking-intention event。

---

## Trial 2：讨论后 4 个 survival items

### Prompt Board B1-T2-O: Opening Phase

**Time:** 0–40s  
**Context:** Speaking Context  
**Scenario:** Island survival  
**Current item:** First aid kit（急救包）  
**Topic:** 进入后 4 个 items 的讨论；讨论 first aid kit 的必要性。  
**Main Task:** 让 leader 重新组织讨论，开始新的 item subset。

**Discussion focus:**

- 如果有人受伤，first aid kit 是否优先；
- 它对感染、割伤、跌倒、晒伤等风险的作用；
- 与 water filter、knife 等 item 相比是否更紧急。

**What staff should do:**

- A/B/C 正常回应。
- 不触发正式 speaking-intention event。
- 保持自然讨论节奏。

---

### Prompt Board B1-T2-E1: Clear Single Entry Request

**Time:** 40–100s  
**Target member:** B  
**Current item:** Rope（绳子）  
**Topic:** Leader 正在解释 rope 的用途；B 准备补充观点。  
**Main Task:** 测试 leader 是否把话轮转交给 B。

**Discussion focus:**

- rope 是否可用于搭建 shelter、固定物品、制作陷阱；
- rope 是否需要和 knife / tent / hammock 配合；
- rope 的通用性是否让它比单一功能 item 更重要。

**What staff should do:**

- B: 等待机会，被邀请后自然表达。
- A/C: 正常听和简短回应。

---

### Prompt Board B1-T2-E2: Competing Entry Requests

**Time:** 100–170s  
**Primary target:** C  
**Secondary competitor:** A  
**Current item:** Lighter / matches（打火机或火柴）  
**Topic:** 两名 members 对火源的重要性都有补充意见。  
**Main Task:** 测试 leader 是否优先邀请更合适的 C。

**Discussion focus:**

- 火源是否用于取暖、煮水、驱虫、求救烟雾；
- 在潮湿海岛环境下火源是否可靠；
- lighter/matches 是否比 flare gun 更持续可用；
- 火源与 water filter 是否互补。

**What staff should do:**

- C: 准备主要观点，例如“火源能同时支持煮水、保暖和求救，因此功能复合”。
- A: 准备次要补充观点，例如“如果环境潮湿，火源的可靠性要考虑”。
- B: 普通参与，不竞争。

---

### Prompt Board B1-T2-E3: Repeated Single Entry Request

**Time:** 170–240s  
**Target member:** A  
**Current item:** Tent / hammock（帐篷或吊床）  
**Topic:** Leader 推进或总结 shelter item；A 准备补充。  
**Main Task:** 重复单一 target 事件，但 target 为 A。

**Discussion focus:**

- tent / hammock 是否能提供遮蔽、休息和防虫；
- 在荒岛上，防晒、防雨、防潮的重要性；
- shelter item 是否应排在水、火、求救工具之后。

**What staff should do:**

- A: 等待 leader 邀请并表达。
- B/C: 正常听，不作为 target。

---

### Prompt Board B1-T2-S: Summary Stage

**Time:** 240–300s  
**Items to summarize:** First aid kit, Rope, Lighter/matches, Tent/hammock  
**Topic:** 总结后 4 个 items 的阶段性排序。  
**Main Task:** 配合 leader 收尾，不再开启新事件。

---

# 3. Block 2: Listening Context — Desert Survival（沙漠求生）

## Block 2 总目标

Leader 主要作为观察者和协调者。  
本 block 关注：当两位 members 正在讨论时，leader 是否能识别尚未获得发言机会的人，并适时邀请其加入讨论。

## Block 2 Item Set

### Trial 1：讨论前 4 个 items

1. Cosmetic mirror（化妆镜/信号镜）
2. Top coat per person（每人一件外套）
3. Water per person（每人一份水）
4. Flashlight（手电筒）

### Trial 2：讨论后 4 个 items

5. Parachute（红白降落伞）
6. Jack knife（折叠刀）
7. Sunglasses（太阳镜）
8. Map / compass（地图或指南针）

---

## Trial 1：讨论前 4 个 survival items

### Prompt Board B2-T1-O: Opening Phase

**Time:** 0–40s  
**Context:** Listening Context  
**Scenario:** Desert survival  
**Current item:** Cosmetic mirror（化妆镜/信号镜）  
**Topic:** Leader 引入沙漠求生任务，并把 cosmetic mirror 的讨论交给 members。  
**Main Task:** 让 A/B 开始讨论，C 保持普通参与。

**Discussion focus:**

- mirror 是否主要用于白天反射阳光求救；
- 在沙漠中等待救援时，信号工具是否优先；
- mirror 与 flashlight、flare、pistol 等信号工具相比的优缺点。

**What staff should do:**

- A/B: 可先开始讨论当前 item。
- C: 保持普通参与，不主动抢话。
- 不触发正式 speaking-intention event。

---

### Prompt Board B2-T1-E1: One Suppressed Entry in Two-Person Discussion

**Time:** 40–100s  
**Context:** Two-person discussion  
**Scenario:** Desert survival  
**Current item:** Top coat per person（每人一件外套）  
**Topic:** A 和 B 正在讨论外套是否适合沙漠环境，C 有观点但尚未获得机会。  
**Main Task:** 测试 leader 是否会邀请被忽视的 C 加入讨论。

**Role assignment:**

- Active speakers: A and B
- Target member: C

**Discussion focus:**

- 沙漠白天炎热，但外套是否可以减少晒伤和水分流失；
- 夜间降温时外套是否能保暖；
- 它是否比水或信号工具更优先。

**What staff should do:**

- A: 作为当前讨论者，主动发表观点。
- B: 回应或补充 A，维持双边讨论。
- C: 准备一个未被表达的观点，例如“外套看似不适合白天，但可以遮阳并减少水分流失”；若 leader 邀请，则正常加入。

**Expected event:** C 是尚未表达的 target。若 leader 邀请 C，为 expected path；若未邀请，则 A/B 继续推进。

---

### Prompt Board B2-T1-E2: Dominant Speaker Suppresses Target

**Time:** 100–170s  
**Context:** Dominant speaker discussion  
**Scenario:** Desert survival  
**Current item:** Water per person（每人一份水）  
**Topic:** A 在 water 的重要性上说得较多，B 尚未获得表达机会。  
**Main Task:** 测试 leader 是否识别讨论失衡，并邀请 B。

**Role assignment:**

- Dominant speaker: A
- Secondary responder: C
- Target member: B

**Discussion focus:**

- 水是否是沙漠中最直接的生存需求；
- 如果水量有限，应该节省体力还是尝试移动；
- 水与 shelter / signal item 的优先级关系；
- 是否应留在原地等待救援。

**What staff should do:**

- A: 主导讨论，多说话，但不要过度表演。
- B: 准备一个尚未表达的观点，例如“水重要，但如果移动会消耗更多水，因此策略上应考虑等待救援”。
- C: 简短回应 A，不争取主要机会。

**Expected event:** B 是被压制的 target。若 leader 邀请 B，为 expected path；若未邀请，A 主导该段结束。

---

### Prompt Board B2-T1-E3: Repeated Suppressed Entry Event

**Time:** 170–240s  
**Context:** Two-person discussion  
**Scenario:** Desert survival  
**Current item:** Flashlight（手电筒）  
**Topic:** B 和 C 正在讨论 flashlight，A 有观点但尚未获得机会。  
**Main Task:** 重复 suppressed entry 结构，但 target 为 A。

**Role assignment:**

- Active speakers: B and C
- Target member: A

**Discussion focus:**

- flashlight 是否适合夜间求救；
- 电池是否有限；
- 与 mirror 的白天求救功能如何互补；
- 夜间移动是否安全。

**What staff should do:**

- B/C: 维持双边讨论。
- A: 准备补充观点，例如“flashlight 的价值可能取决于是否选择夜间发信号或行动”；若 leader 邀请，则加入讨论。

---

### Prompt Board B2-T1-S: Summary Stage

**Time:** 240–300s  
**Items to summarize:** Cosmetic mirror, Top coat, Water, Flashlight  
**Topic:** 总结前 4 个 desert survival items 的当前讨论结果。  
**Main Task:** 配合 leader 收尾；如有 summary dashboard 条件，可接受 leader 的最后确认或补充邀请。

---

## Trial 2：讨论后 4 个 survival items

### Prompt Board B2-T2-O: Opening Phase

**Time:** 0–40s  
**Context:** Listening Context  
**Scenario:** Desert survival  
**Current item:** Parachute（红白降落伞）  
**Topic:** Leader 引入后 4 个 desert survival items，并让 members 开始讨论 parachute。  
**Main Task:** A/C 可先开始讨论；B 保持普通参与。

**Discussion focus:**

- parachute 是否可用于遮阳；
- 红白颜色是否可作为空中搜救标记；
- 是否可作为临时 shelter 或 signal marker。

**What staff should do:** A/C 可先开始讨论；B 保持普通参与；不触发正式 event。

---

### Prompt Board B2-T2-E1: One Suppressed Entry in Two-Person Discussion

**Time:** 40–100s  
**Active speakers:** A and C  
**Target member:** B  
**Current item:** Jack knife（折叠刀）  
**Topic:** A 和 C 正在讨论 jack knife，B 有观点但尚未获得机会。  
**Main Task:** 测试 leader 是否邀请 B。

**Discussion focus:**

- jack knife 是否可用于切割、制作工具、处理仙人掌或绳索；
- 在沙漠中它是否比信号工具更重要；
- 是否适合应急修理或急救辅助。

**What staff should do:**

- A/C: 维持双边讨论。
- B: 准备补充观点，等待 leader 邀请。

---

### Prompt Board B2-T2-E2: Dominant Speaker Suppresses Target

**Time:** 100–170s  
**Dominant speaker:** B  
**Secondary responder:** A  
**Target member:** C  
**Current item:** Sunglasses（太阳镜）  
**Topic:** B 主导 sunglasses 的讨论，C 尚未表达。  
**Main Task:** 测试 leader 是否邀请 C 平衡讨论。

**Discussion focus:**

- sunglasses 是否能防止眩光和眼睛疲劳；
- 如果需要白天移动，保护眼睛是否重要；
- 与外套、遮阳 shelter 相比是否优先。

**What staff should do:**

- B: 当前主要发言者，多说话。
- A: 简短回应。
- C: 准备补充观点，例如“如果选择留在 crash site，太阳镜可能不如信号和遮阳工具重要”；被邀请后补充。

---

### Prompt Board B2-T2-E3: Repeated Suppressed Entry Event

**Time:** 170–240s  
**Active speakers:** B and C  
**Target member:** A  
**Current item:** Map / compass（地图或指南针）  
**Topic:** B/C 正在讨论 map/compass，A 有观点但尚未获得机会。  
**Main Task:** 重复 suppressed entry 结构，target 为 A。

**Discussion focus:**

- 如果决定移动，map/compass 是否重要；
- 如果决定留在 crash site，导航工具是否价值下降；
- compass 的放大镜或金属部分是否可用于信号或生火；
- 是否应先讨论 stay vs move 的策略。

**What staff should do:**

- B/C: 维持双边讨论。
- A: 准备补充观点，例如“map/compass 的价值取决于是否离开 crash site”；等待 leader 邀请后加入。

---

### Prompt Board B2-T2-S: Summary Stage

**Time:** 240–300s  
**Items to summarize:** Parachute, Jack knife, Sunglasses, Map/compass  
**Topic:** 总结后 4 个 desert survival items 的当前讨论结果。  
**Main Task:** 配合 leader 收尾。

---

# 4. Block 3: Silence Context — Mountain Survival（深山求生）

## Block 3 总目标

讨论在 item 与 item 之间可能出现短暂停顿。  
本 block 关注：当讨论停下来时，leader 是否能顺利重启讨论，并决定由谁继续下一段发言。

## Block 3 Item Set

### Trial 1：讨论前 4 个 items

1. Matches / lighter（火柴或打火机）
2. Polythene sheeting / heavy canvas（塑料布或厚帆布）
3. First-aid kit（急救包）
4. Signal flares（信号弹）

### Trial 2：讨论后 4 个 items

5. Bottled water（瓶装水）
6. Toolbox / hand axe / knife（工具箱、手斧或刀）
7. Extra clothing / blanket（额外衣物或毯子）
8. Chocolate / high-energy food（巧克力或高能量食物）

---

## Trial 1：讨论前 4 个 survival items

### Prompt Board B3-T1-O: Opening Phase

**Time:** 0–40s  
**Context:** Silence Context  
**Scenario:** Mountain survival  
**Current item:** Matches / lighter（火柴或打火机）  
**Topic:** Leader 介绍深山求生任务，并开始讨论火源的重要性。  
**Main Task:** 开启讨论，并允许自然停顿出现。

**Discussion focus:**

- 火源是否用于保暖、求救、煮水；
- 在山区或寒冷环境中，火源是否比食物更紧急；
- 火源是否需要干燥材料配合。

**What staff should do:**

- A/B/C 正常进入讨论。
- 不主动制造正式 speaking-intention event。
- 保持自然节奏，不需要一直填满沉默。

---

### Prompt Board B3-T1-E1: Clear First Restart After Silence

**Time:** 40–100s  
**Context:** Group silence / restart  
**Scenario:** Mountain survival  
**Current item:** Polythene sheeting / heavy canvas（塑料布或厚帆布）  
**Topic:** 当前 item 暂时讨论结束，小组出现短暂停顿。A 是最适合继续讨论 shelter item 的人。  
**Main Task:** 测试 leader 是否打破停顿并邀请 A 继续。

**Role assignment:**

- Target member: A
- B/C: waiting members

**Discussion focus:**

- 塑料布/帆布是否可用于防风、防雨、保暖；
- 是否可作为临时 shelter、地垫或信号标记；
- 与 fire source 的组合价值；
- shelter 是否比移动寻找救援更重要。

**What staff should do:**

- A: 准备继续讨论 shelter 的观点；等待 leader 邀请；被邀请后继续讨论。
- B/C: 保持等待，不主动抢先恢复讨论。

**Expected event:** 若 leader 邀请 A，则讨论顺利恢复；若未邀请，则 group 可自然恢复或转向下一 item。

---

### Prompt Board B3-T1-E2: Competing Restart Attempts

**Time:** 100–170s  
**Context:** Group silence / competing restart  
**Scenario:** Mountain survival  
**Current item:** First-aid kit（急救包）  
**Topic:** 上一个 item 讨论结束后出现短暂停顿。B 和 C 都可以继续 first-aid kit 的讨论，但 B 是 primary target。  
**Main Task:** 测试 leader 是否选择更合适的人恢复讨论。

**Role assignment:**

- Primary target: B
- Secondary candidate: C
- A: waiting member

**Discussion focus:**

- 山地环境中割伤、摔伤、冻伤或擦伤风险；
- first-aid kit 是否是必要但不一定最高优先级的 item；
- 如果没有严重伤员，急救包是否应低于火源和 shelter；
- 是否能提高团队行动安全性。

**What staff should do:**

- B: 准备更贴合当前讨论的观点；等待 leader 邀请；被邀请后继续讨论。
- C: 也可接续，但不表现为首要 target。
- A: 等待，不争取当前机会。

**Expected event:** 邀请 B 为 expected path；邀请 C 为 target mismatch；未介入则 group 可自然恢复。

---

### Prompt Board B3-T1-E3: Repeated Restart Event

**Time:** 170–240s  
**Context:** Group silence / restart  
**Scenario:** Mountain survival  
**Current item:** Signal flares（信号弹）  
**Topic:** 再次出现短暂停顿，C 是最适合继续讨论 signal flares 的人。  
**Main Task:** 重复 restart 结构，但 target 为 C。

**Role assignment:**

- Target member: C
- A/B: waiting members

**Discussion focus:**

- signal flares 是否可用于引起救援人员注意；
- 白天和夜晚使用效果；
- 与火源、镜子、枪声等 signal 方式相比；
- 是否应在救援可能靠近时保留使用。

**What staff should do:**

- C: 等待 leader 邀请并继续讨论。
- A/B: 保持等待，不主动抢先。

---

### Prompt Board B3-T1-S: Summary Stage

**Time:** 240–300s  
**Items to summarize:** Matches/lighter, Polythene sheeting/canvas, First-aid kit, Signal flares  
**Topic:** 总结前 4 个 mountain survival items 的阶段性结论。  
**Main Task:** 跟随 leader 收尾；如有 summary/adaptive-delayed dashboard，可配合最后确认。

---

## Trial 2：讨论后 4 个 survival items

### Prompt Board B3-T2-O: Opening Phase

**Time:** 0–40s  
**Context:** Silence Context  
**Scenario:** Mountain survival  
**Current item:** Bottled water（瓶装水）  
**Topic:** Leader 介绍后 4 个 mountain survival items，并开始讨论水。  
**Main Task:** 正常进入讨论，允许自然停顿，不触发正式 event。

**Discussion focus:**

- 山地环境下是否容易找到水；
- bottled water 是否用于短期维持体力；
- 是否需要配合火源或过滤方式；
- 与保暖、shelter 相比的优先级。

**What staff should do:** 正常进入讨论，允许自然停顿，不触发正式 event。

---

### Prompt Board B3-T2-E1: Clear First Restart After Silence

**Time:** 40–100s  
**Target member:** C  
**Current item:** Toolbox / hand axe / knife（工具箱、手斧或刀）  
**Topic:** 当前 item 暂时讨论结束，小组短暂停顿，C 是最适合继续工具类 item 讨论的人。  
**Main Task:** 测试 leader 是否邀请 C 重启讨论。

**Discussion focus:**

- 工具是否可用于砍树枝、搭 shelter、修理设备；
- 工具是否比单一用途 item 更灵活；
- 工具重量和可携带性是否影响优先级；
- 是否能与 rope、canvas、fire source 配合。

**What staff should do:**

- C: 等待 leader 邀请；被邀请后继续。
- A/B: 等待，不主动抢先。

---

### Prompt Board B3-T2-E2: Competing Restart Attempts

**Time:** 100–170s  
**Primary target:** A  
**Secondary candidate:** B  
**Current item:** Extra clothing / blanket（额外衣物或毯子）  
**Topic:** 停顿后 A 和 B 都可以继续讨论保暖 item，但 A 更适合先发言。  
**Main Task:** 测试 leader 是否选择 A。

**Discussion focus:**

- 额外衣物/毯子对防止失温的重要性；
- 白天行动与夜间保暖的不同需求；
- 与火源、shelter 的组合价值；
- 是否优先于食物或工具。

**What staff should do:**

- A: primary restart candidate，准备主要观点。
- B: secondary restart candidate，准备次要观点。
- C: waiting member。

---

### Prompt Board B3-T2-E3: Repeated Restart Event

**Time:** 170–240s  
**Target member:** B  
**Current item:** Chocolate / high-energy food（巧克力或高能量食物）  
**Topic:** 再次出现短暂停顿，B 是最适合继续讨论能量补给的人。  
**Main Task:** 重复 restart 结构，target 为 B。

**Discussion focus:**

- 高能量食物是否能维持体力和体温；
- 食物与水、火源、shelter 相比是否更低优先级；
- 巧克力是否轻便、易分配；
- 如果需要等待救援或短距离移动，能量补给的作用。

**What staff should do:**

- B: 等待 leader 邀请并继续讨论。
- A/C: 保持等待，不主动抢先。

---

### Prompt Board B3-T2-S: Summary Stage

**Time:** 240–300s  
**Items to summarize:** Bottled water, Toolbox/hand axe/knife, Extra clothing/blanket, Chocolate/high-energy food  
**Topic:** 总结后 4 个 mountain survival items 的阶段性结论。  
**Main Task:** 跟随 leader 收尾。

---

# 5. Staff 使用提醒

每个提示板只用于提醒当前阶段的执行重点，不是固定台词。

Staff 在每个 episode 中应重点记住：

1. 当前 block/context；
2. 当前 episode 持续时间；
3. 当前具体讨论的 survival item；
4. 当前 item 的主要讨论角度；
5. 自己是否为 target member；
6. 自己是 active speaker、secondary responder、normal participant 还是 waiting member；
7. 被 leader 邀请后，围绕当前 item 自然表达观点。

---

# 6. 参考 item 来源说明

本提示板中的具体 item 参考了常见 survival ranking / team-building exercises 中的物品类型，并根据本研究的三个场景进行了筛选和改写：

- Island survival items 参考荒岛求生活动中的 water filter, knife, flare gun, fishing rod, rope, tent 等常见选项；
- Desert survival items 参考 desert survival exercise 中的 cosmetic mirror, water, top coat, parachute, jack knife, sunglasses, map/compass 等常见选项；
- Mountain / cold survival items 参考 mountain survival 与 winter survival exercises 中的 matches/lighter, canvas/sheeting, first-aid kit, signal flares, water, tools, clothing, chocolate 等常见选项。

