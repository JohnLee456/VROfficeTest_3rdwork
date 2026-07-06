# WoZ 说话意图概率变化配置表

## 1. 文档目的

本文档用于说明 Wizard-of-Oz（WoZ）实验中预先设定的 speaking-intention probability trajectories（说话意图概率变化轨迹）。

在本 WoZ 实验中，speaking-intention values **不是实时检测获得的**，而是由研究团队提前设置，并由 wizard 按照 trial 时间线触发。

本配置基于 Study 2 WoZ 脚本结构：

- 每个 trial 总时长约为 **300 秒**
- 每个 trial 包含：
  - Opening phase：0–40s
  - Episode 1：40–100s
  - Episode 2：100–170s
  - Episode 3：170–240s
  - Summary stage：240–300s

只有三个 episode 阶段包含 speaking-intention events。

---

## 2. 显示规则

### 2.1 阈值规则

只有当 speaking-intention probability 满足以下条件时，才显示说话意图 cue：

```text
speaking-intention probability > 0.70
```

如果 probability **小于或等于 0.70**，则不显示任何 speaking-intention visualization。

### 2.2 显示时长规则

在每个 episode 中，speaking-intention cue 的显示时间约为 **5 秒**。

episode 中其余时间用于对话铺垫、讨论延续或讨论恢复，但不显示 speaking-intention cue。

### 2.3 概率值语义

这些 probability values 表示的是 **recent window-level speaking-intention evidence（近期窗口级说话意图证据）**，而不是实时 next-speaker prediction。

因此：

- 大于 0.70 的值表示该 speaking-intention cue 可以被显示；
- 小于或等于 0.70 的值表示不显示；
- 高概率值不表示该成员一定会成为下一位发言者；
- 这些概率值主要用于在 WoZ 实验中标准化 feedback 呈现时机。

### 2.4 轨迹表示方式

每条 probability trajectory 使用以下格式：

```text
起始秒–结束秒：起始概率 → 结束概率
```

在显示窗口中，probability 会在约 5 秒内保持高于 0.70。

---

## 3. 标准概率变化模式

### 3.1 单目标事件模式

适用于 clear single entry request、suppressed entry event 和 clear restart event。

| 阶段 | 相对事件时间 | 概率变化 | 是否显示 |
|---|---:|---|---|
| 准备阶段 | -4s 到 0s | 0.20 → 0.74 | 不显示 |
| 显示窗口 | 0s 到 +5s | 0.74 → 0.88 → 0.74 | 显示 |
| 衰减阶段 | +5s 到 +8s | 0.74 → 0.20 | 不显示 |

### 3.2 竞争目标事件模式

适用于两名成员都表现出 speaking-intention evidence，但其中一名成员为 primary target 的情况。

| 角色 | 显示窗口内概率变化 | 是否显示 |
|---|---|---|
| Primary target | 0.78 → 0.92 → 0.78 | 显示 |
| Secondary candidate | 0.72 → 0.80 → 0.71 | 显示 |
| Non-target | 0.10–0.35 | 不显示 |

### 3.3 背景概率模式

对于当前 episode 中没有被分配 speaking-intention 角色的成员：

```text
episode period: 0.10–0.35
```

这些成员不显示 visualization。

---

# 4. Block 1：Speaking Context — Island Survival（荒岛求生）

在该 block 中，leader 通常是当前主要发言者。  
cue 的作用是帮助 leader 在自己发言过程中，将话轮更合适地交给具有近期 speaking-intention evidence 的成员。

## Block 1，Trial 1

### Episode 1：Clear Single Entry Request

**Episode 时间窗口：** 40–100s  
**Cue 显示窗口：** 60–65s  
**Target member：** A

| Member | 概率变化配置 | 显示解释 |
|---|---|---|
| A | 56–60s: 0.20 → 0.74; 60–65s: 0.74 → 0.88 → 0.74; 65–68s: 0.74 → 0.20 | 显示 A，持续 5s |
| B | 40–100s: 0.10–0.30 | 不显示 |
| C | 40–100s: 0.10–0.30 | 不显示 |

### Episode 2：Competing Entry Requests

**Episode 时间窗口：** 100–170s  
**Cue 显示窗口：** 130–135s  
**Primary target：** B  
**Secondary candidate：** C

| Member | 概率变化配置 | 显示解释 |
|---|---|---|
| A | 100–170s: 0.10–0.30 | 不显示 |
| B | 126–130s: 0.25 → 0.78; 130–135s: 0.78 → 0.92 → 0.78; 135–138s: 0.78 → 0.25 | 显示 B，作为更高意图 target，持续 5s |
| C | 127–130s: 0.20 → 0.72; 130–135s: 0.72 → 0.80 → 0.71; 135–138s: 0.71 → 0.20 | 显示 C，作为较低意图 competitor，持续 5s |

### Episode 3：Repeated Single Entry Request

**Episode 时间窗口：** 170–240s  
**Cue 显示窗口：** 200–205s  
**Target member：** C

| Member | 概率变化配置 | 显示解释 |
|---|---|---|
| A | 170–240s: 0.10–0.30 | 不显示 |
| B | 170–240s: 0.10–0.30 | 不显示 |
| C | 196–200s: 0.20 → 0.74; 200–205s: 0.74 → 0.86 → 0.74; 205–208s: 0.74 → 0.20 | 显示 C，持续 5s |

---

## Block 1，Trial 2

Trial 2 沿用 Speaking Context 的相同结构，但轮换 target member，以避免重复固定的成员模式。

### Episode 1：Clear Single Entry Request

**Episode 时间窗口：** 40–100s  
**Cue 显示窗口：** 60–65s  
**Target member：** B

| Member | 概率变化配置 | 显示解释 |
|---|---|---|
| A | 40–100s: 0.10–0.30 | 不显示 |
| B | 56–60s: 0.20 → 0.74; 60–65s: 0.74 → 0.88 → 0.74; 65–68s: 0.74 → 0.20 | 显示 B，持续 5s |
| C | 40–100s: 0.10–0.30 | 不显示 |

### Episode 2：Competing Entry Requests

**Episode 时间窗口：** 100–170s  
**Cue 显示窗口：** 130–135s  
**Primary target：** C  
**Secondary candidate：** A

| Member | 概率变化配置 | 显示解释 |
|---|---|---|
| A | 127–130s: 0.20 → 0.72; 130–135s: 0.72 → 0.80 → 0.71; 135–138s: 0.71 → 0.20 | 显示 A，作为较低意图 competitor，持续 5s |
| B | 100–170s: 0.10–0.30 | 不显示 |
| C | 126–130s: 0.25 → 0.78; 130–135s: 0.78 → 0.92 → 0.78; 135–138s: 0.78 → 0.25 | 显示 C，作为更高意图 target，持续 5s |

### Episode 3：Repeated Single Entry Request

**Episode 时间窗口：** 170–240s  
**Cue 显示窗口：** 200–205s  
**Target member：** A

| Member | 概率变化配置 | 显示解释 |
|---|---|---|
| A | 196–200s: 0.20 → 0.74; 200–205s: 0.74 → 0.86 → 0.74; 205–208s: 0.74 → 0.20 | 显示 A，持续 5s |
| B | 170–240s: 0.10–0.30 | 不显示 |
| C | 170–240s: 0.10–0.30 | 不显示 |

---

# 5. Block 2：Listening Context — Desert Survival（沙漠求生）

在该 block 中，leader 主要处于观察与协调位置，而 members 之间展开讨论。  
cue 的作用是帮助 leader 发现尚未获得发言机会的成员。

## Block 2，Trial 1

### Episode 1：One Suppressed Entry in Two-Person Discussion

**Episode 时间窗口：** 40–100s  
**Cue 显示窗口：** 64–69s  
**Active speakers：** A 和 B  
**Target member：** C

| Member | 概率变化配置 | 显示解释 |
|---|---|---|
| A | 40–100s: 0.10–0.30 | 不显示；当前 active speaker |
| B | 40–100s: 0.10–0.30 | 不显示；当前 active speaker |
| C | 60–64s: 0.20 → 0.74; 64–69s: 0.74 → 0.88 → 0.74; 69–72s: 0.74 → 0.20 | 显示 C，持续 5s |

### Episode 2：Dominant Speaker Suppresses Target

**Episode 时间窗口：** 100–170s  
**Cue 显示窗口：** 130–135s  
**Dominant speaker：** A  
**Secondary responder：** C  
**Target member：** B

| Member | 概率变化配置 | 显示解释 |
|---|---|---|
| A | 100–170s: 0.10–0.30 | 不显示；dominant speaker |
| B | 126–130s: 0.20 → 0.74; 130–135s: 0.74 → 0.89 → 0.74; 135–138s: 0.74 → 0.20 | 显示 B，持续 5s |
| C | 100–170s: 0.10–0.35 | 不显示；secondary responder |

### Episode 3：Repeated Suppressed Entry Event

**Episode 时间窗口：** 170–240s  
**Cue 显示窗口：** 200–205s  
**Active speakers：** B 和 C  
**Target member：** A

| Member | 概率变化配置 | 显示解释 |
|---|---|---|
| A | 196–200s: 0.20 → 0.74; 200–205s: 0.74 → 0.86 → 0.74; 205–208s: 0.74 → 0.20 | 显示 A，持续 5s |
| B | 170–240s: 0.10–0.30 | 不显示；active speaker |
| C | 170–240s: 0.10–0.30 | 不显示；active speaker |

---

## Block 2，Trial 2

Trial 2 轮换 target 和 active-speaker 角色。

### Episode 1：One Suppressed Entry in Two-Person Discussion

**Episode 时间窗口：** 40–100s  
**Cue 显示窗口：** 64–69s  
**Active speakers：** A 和 C  
**Target member：** B

| Member | 概率变化配置 | 显示解释 |
|---|---|---|
| A | 40–100s: 0.10–0.30 | 不显示；active speaker |
| B | 60–64s: 0.20 → 0.74; 64–69s: 0.74 → 0.88 → 0.74; 69–72s: 0.74 → 0.20 | 显示 B，持续 5s |
| C | 40–100s: 0.10–0.30 | 不显示；active speaker |

### Episode 2：Dominant Speaker Suppresses Target

**Episode 时间窗口：** 100–170s  
**Cue 显示窗口：** 130–135s  
**Dominant speaker：** B  
**Secondary responder：** A  
**Target member：** C

| Member | 概率变化配置 | 显示解释 |
|---|---|---|
| A | 100–170s: 0.10–0.35 | 不显示；secondary responder |
| B | 100–170s: 0.10–0.30 | 不显示；dominant speaker |
| C | 126–130s: 0.20 → 0.74; 130–135s: 0.74 → 0.89 → 0.74; 135–138s: 0.74 → 0.20 | 显示 C，持续 5s |

### Episode 3：Repeated Suppressed Entry Event

**Episode 时间窗口：** 170–240s  
**Cue 显示窗口：** 200–205s  
**Active speakers：** B 和 C  
**Target member：** A

| Member | 概率变化配置 | 显示解释 |
|---|---|---|
| A | 196–200s: 0.20 → 0.74; 200–205s: 0.74 → 0.86 → 0.74; 205–208s: 0.74 → 0.20 | 显示 A，持续 5s |
| B | 170–240s: 0.10–0.30 | 不显示；active speaker |
| C | 170–240s: 0.10–0.30 | 不显示；active speaker |

---

# 6. Block 3：Silence Context — Mountain Survival（深山求生）

在该 block 中，讨论可能在 item 与 item 之间短暂停滞。  
cue 的作用是帮助 leader 重新启动讨论，并选择合适的下一位发言者。

## Block 3，Trial 1

### Episode 1：Clear First Restart After Silence

**Episode 时间窗口：** 40–100s  
**Cue 显示窗口：** 58–63s  
**Target member：** A

| Member | 概率变化配置 | 显示解释 |
|---|---|---|
| A | 54–58s: 0.20 → 0.74; 58–63s: 0.74 → 0.88 → 0.74; 63–66s: 0.74 → 0.20 | 显示 A，持续 5s |
| B | 40–100s: 0.10–0.25 | 不显示；waiting |
| C | 40–100s: 0.10–0.25 | 不显示；waiting |

### Episode 2：Competing Restart Attempts

**Episode 时间窗口：** 100–170s  
**Cue 显示窗口：** 128–133s  
**Primary target：** B  
**Secondary candidate：** C

| Member | 概率变化配置 | 显示解释 |
|---|---|---|
| A | 100–170s: 0.10–0.25 | 不显示；waiting |
| B | 124–128s: 0.25 → 0.78; 128–133s: 0.78 → 0.92 → 0.78; 133–136s: 0.78 → 0.25 | 显示 B，作为更高意图 target，持续 5s |
| C | 125–128s: 0.20 → 0.72; 128–133s: 0.72 → 0.80 → 0.71; 133–136s: 0.71 → 0.20 | 显示 C，作为较低意图 candidate，持续 5s |

### Episode 3：Repeated Restart Event

**Episode 时间窗口：** 170–240s  
**Cue 显示窗口：** 200–205s  
**Target member：** C

| Member | 概率变化配置 | 显示解释 |
|---|---|---|
| A | 170–240s: 0.10–0.25 | 不显示；waiting |
| B | 170–240s: 0.10–0.25 | 不显示；waiting |
| C | 196–200s: 0.20 → 0.74; 200–205s: 0.74 → 0.86 → 0.74; 205–208s: 0.74 → 0.20 | 显示 C，持续 5s |

---

## Block 3，Trial 2

Trial 2 轮换 restart targets。

### Episode 1：Clear First Restart After Silence

**Episode 时间窗口：** 40–100s  
**Cue 显示窗口：** 58–63s  
**Target member：** C

| Member | 概率变化配置 | 显示解释 |
|---|---|---|
| A | 40–100s: 0.10–0.25 | 不显示；waiting |
| B | 40–100s: 0.10–0.25 | 不显示；waiting |
| C | 54–58s: 0.20 → 0.74; 58–63s: 0.74 → 0.88 → 0.74; 63–66s: 0.74 → 0.20 | 显示 C，持续 5s |

### Episode 2：Competing Restart Attempts

**Episode 时间窗口：** 100–170s  
**Cue 显示窗口：** 128–133s  
**Primary target：** A  
**Secondary candidate：** B

| Member | 概率变化配置 | 显示解释 |
|---|---|---|
| A | 124–128s: 0.25 → 0.78; 128–133s: 0.78 → 0.92 → 0.78; 133–136s: 0.78 → 0.25 | 显示 A，作为更高意图 target，持续 5s |
| B | 125–128s: 0.20 → 0.72; 128–133s: 0.72 → 0.80 → 0.71; 133–136s: 0.71 → 0.20 | 显示 B，作为较低意图 candidate，持续 5s |
| C | 100–170s: 0.10–0.25 | 不显示；waiting |

### Episode 3：Repeated Restart Event

**Episode 时间窗口：** 170–240s  
**Cue 显示窗口：** 200–205s  
**Target member：** B

| Member | 概率变化配置 | 显示解释 |
|---|---|---|
| A | 170–240s: 0.10–0.25 | 不显示；waiting |
| B | 196–200s: 0.20 → 0.74; 200–205s: 0.74 → 0.86 → 0.74; 205–208s: 0.74 → 0.20 | 显示 B，持续 5s |
| C | 170–240s: 0.10–0.25 | 不显示；waiting |

---

# 7. Summary Dashboard 使用说明

对于 summary-dashboard 或 adaptive-delayed 条件，dashboard 应总结同一组 scripted probability events。

建议 summary variables 如下：

| 变量 | 含义 |
|---|---|
| Recent attempts | 某成员 probability 超过 0.70 的事件次数 |
| Latest intention level | 最近一次显示窗口中的 probability value |
| Timeline block color | 使用 continuous color 对显示窗口中的 probability values 编码 |
| Speaking event overlay | 实际 speaking behavior，需要单独记录或按脚本标注 |

对于 D2-style summary dashboard，使用：

```text
member | repeated attempts | latest intention level
```

对于 D3-style timeline dashboard，使用：

```text
member | intention timeline | speaking event timeline
```

speaking events 应与 intention windows 采用不同视觉编码。  
例如：

- intention level：continuous color blocks
- speaking event：dark bar 或 outlined segment

---

# 8. Wizard 操作说明

1. Wizard 只在上述 5 秒显示窗口中触发 speaking-intention display。
2. Probability 小于或等于 0.70 时，不显示 visualization。
3. 如果 leader 在显示窗口结束前邀请 target member，wizard 可以提前终止 cue，并记录邀请时间。
4. 如果 leader 在 competing event 中邀请了 non-target member，则记录为 target mismatch。
5. 对于 summary dashboard 条件，实时显示被抑制，但 scripted events 仍需记录，用于 summary visualization。
6. 同一套 probability schedule 可用于 real-time、directional、summary、adaptive 和 baseline 条件；不同条件只改变 visualization behavior。

---

# 9. Trial 平衡说明

Trial 2 中的 target assignments 会轮换成员，以减少对同一 target pattern 的重复暴露。

该轮换设计用于：

- 避免总是将同一 member 设为 target；
- 确保 A/B/C 都能在不同 trial 中作为 target member 出现；
- 在保持 episode 结构一致的同时改变 target identity；
- 降低 leader 学习到固定成员模式的风险。
