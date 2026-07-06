# Study 2 WoZ：Leader Trial 提示板与 Experimental Staff Episode 提示板（可直接粘贴到 UI）

## 0. 使用说明

本文件提供两类 UI 提示板内容：

1. **Leader Trial 提示板**：每个 trial 开始前给 leader 查看，说明本 trial 需要做什么，以及每个 episode 对应讨论的 item。
2. **Experimental Staff Episode 提示板**：每个 episode 给 experimental staff 查看，说明该 episode 围绕 item 要做什么、讨论哪些点、以及每位 staff 的角色任务。

提示板不是逐句台词，而是用于保证每个 trial / episode 的讨论结构一致。  
Experimental staff 仍然应自然讨论，不需要背固定台词。

---

# Block 1：Speaking Context — Island Survival（荒岛求生）

## B1-T1 Leader Trial 提示板

```text
Trial：荒岛求生 — Speaking Context
时长：300 秒

你的角色：
你是 leader。请引导小组讨论，解释或总结当前 item，并在合适时机把发言机会交给可能想补充观点的成员。

本 trial 目标：
讨论前四个荒岛求生 items，并判断它们的相对重要性。

本 trial 的 items：
1. Water filter（水过滤器）
2. Knife（刀）
3. Fishing rod / fishing net（鱼竿/渔网）
4. Flare gun（信号枪）

Episode 与 item 对应结构：
Opening phase（0–40s）： Water filter（水过滤器）
Episode 1（40–100s）： Knife（刀）
Episode 2（100–170s）： Fishing rod / fishing net（鱼竿/渔网）
Episode 3（170–240s）： Flare gun（信号枪）
Summary stage（240–300s）： 总结四个 items

请关注：
- 哪些 items 对荒岛生存最重要。
- 每个 item 是否有助于饮水、食物、庇护、安全或求救。
- 是否需要邀请某位成员补充观点。
```

---

## B1-T1-O Staff Episode 提示板 — Opening Phase（开场阶段）

```text
时间： 0–40s
Context： Speaking Context
Scenario： 荒岛求生
本 episode 的 item：
- Water filter（水过滤器）

Episode 任务：
自然开始本 trial。让 leader 介绍荒岛求生任务，并开始讨论是否应优先选择水过滤器。

Discussion focus：
- 饮用水是否是荒岛上最紧急的生存需求？
- Can a water filter help if the group finds unsafe freshwater?
- 如果只有海水可用，它是否仍然有用？
- 它是否应排在工具、食物获取物品或求救物品之前？

Staff 角色任务：
Member A：简短回应 leader，帮助进入讨论。
Member B：简短回应，或自然地表示同意/不同意。
Member C：简短回应并确认理解任务。

注意：
本阶段不要触发正式 speaking-intention event。
不要争抢发言权。
```

---

## B1-T1-E1 Staff Episode 提示板 — 单一成员进入请求

```text
时间： 40–100s
Context： Leader speaking
Scenario： 荒岛求生
本 episode 的 item：
- Knife（刀）

Episode 任务：
Leader 正在解释或总结刀的重要性。Member A 有一个有价值的补充观点，应被视为主要 target。

Discussion focus：
- Can the knife be used to cut branches, prepare food, or make simple tools?
- 它是否有助于安全、急救或搭建庇护所？
- 当它与绳子、捕鱼工具或庇护类物品配合使用时，价值是否更高？
- Should it be ranked before or after the water filter?

Staff 角色任务：
Member A: Target member. Prepare one clear supplementary point, such as “the knife has indirect value because it can help make other tools.” Wait for the leader to invite you. If invited, speak naturally.
Member B：普通参与者。倾听即可，如有需要只做简短回应。
Member C：普通参与者。倾听即可，如有需要只做简短回应。

注意：
Member A 不应打断 leader。如果 leader 邀请 A，则该事件成功。
```

---

## B1-T1-E2 Staff Episode 提示板 — 竞争性进入请求

```text
时间： 100–170s
Context： Leader speaking
Scenario： 荒岛求生
本 episode 的 item：
- Fishing rod / fishing net（鱼竿/渔网）

Episode 任务：
Leader 正在讨论捕鱼工具。两名成员都有可能补充观点，但 Member B 是更强的 target，Member C 是较低优先级的 competitor。

Discussion focus：
- 这个 item 能否解决长期食物需求？
- 捕鱼是否需要技巧、时间和体力？
- 在最初几天，食物是否不如水紧急？
- 这个 item 是否足够可靠，可以排在较高位置？

Staff 角色任务：
Member A：普通参与者。倾听，不加入竞争。
Member B: Primary target. Prepare the main point, such as “food may not be the most urgent need at first, but fishing is important for longer survival.” If invited, speak naturally.
Member C: Secondary competitor. Prepare a weaker or more conditional point, such as “if there are few fish nearby, this item may be uncertain.” If invited, speak naturally but do not appear stronger than B.

注意：
预期路径：leader 邀请 B。
如果 leader 先邀请 C，则记录为 target mismatch。
如果 B 和 C 都没有被邀请，则记录为 missed。
```

---

## B1-T1-E3 Staff Episode 提示板 — 重复单一进入请求

```text
时间： 170–240s
Context： Leader speaking
Scenario： 荒岛求生
本 episode 的 item：
- Flare gun（信号枪）

Episode 任务：
Leader 正在解释或总结信号枪的求救价值。Member C 是主要 target。

Discussion focus：
- Can the flare gun help signal to ships or aircraft?
- 它是否属于有限次数或一次性求救物品？
- 它在夜间还是白天更有效？
- 与水或工具等稳定生存物品相比，它的价值是更高还是更低？

Staff 角色任务：
Member A：普通参与者。倾听即可，只做简短回应。
Member B：普通参与者。倾听即可，只做简短回应。
Member C: Target member. Prepare one supplementary point, such as “the flare gun is very useful if the island is near a shipping or flight route, but less stable otherwise.” Wait for the leader to invite you.

注意：
Member C 不应打断。如果 leader 邀请 C，则该事件成功。
```

---

## B1-T1-S Staff Episode 提示板 — Summary Stage（总结阶段）

```text
时间： 240–300s
Context： Trial wrap-up
Scenario： 荒岛求生
本阶段的 items：
- Water filter（水过滤器）
- Knife（刀）
- Fishing rod / fishing net（鱼竿/渔网）
- Flare gun（信号枪）

Episode 任务：
帮助 leader 总结前四个荒岛求生 items。

Discussion focus：
- Which item should be ranked highest?
- Which items support immediate survival?
- Which items support long-term survival or rescue?
- Are there any final comments before ending the trial?

Staff 角色任务：
Member A：如被邀请，可确认讨论结果或简短补充。
Member B：如被邀请，可确认讨论结果或简短补充。
Member C：如被邀请，可确认讨论结果或简短补充。

注意：
不要开启新的主要 speaking-intention event。
```

---

## B1-T2 Leader Trial 提示板

```text
Trial：荒岛求生 — Speaking Context
时长：300 秒

你的角色：
你是 leader。请引导讨论，解释各个 item，并在合适时机决定是否邀请成员发言。

本 trial 目标：
讨论后四个荒岛求生 items，并判断它们的相对重要性。

本 trial 的 items：
1. First aid kit（急救包）
2. Rope（绳子）
3. Lighter / matches（打火机/火柴）
4. Tent / hammock（帐篷/吊床）

Episode 与 item 对应结构：
Opening phase（0–40s）： First aid kit（急救包）
Episode 1（40–100s）： Rope（绳子）
Episode 2（100–170s）： Lighter / matches（打火机/火柴）
Episode 3（170–240s）： Tent / hammock（帐篷/吊床）
Summary stage（240–300s）： 总结四个 items

请关注：
- 每个 item 是否支持安全、求救、火源、庇护或工具制作。
- 这些 items 与之前讨论过的 items 相比如何。
- 是否需要邀请某位成员补充观点。
```

---

## B1-T2-O Staff Episode 提示板 — Opening Phase（开场阶段）

```text
时间： 0–40s
Context： Speaking Context
Scenario： 荒岛求生
本 episode 的 item：
- First aid kit（急救包）

Episode 任务：
开始第二个荒岛 trial。让 leader 介绍新的 item 集合，并开始讨论急救包。

Discussion focus：
- Is the first aid kit important if someone is injured?
- 它能否处理割伤、感染、摔伤、晒伤或轻微伤口？
- 它是否比水、工具或求救设备更紧急？
- 即使当前没有人受伤，它是否仍应排在较高位置？

Staff 角色任务：
Member A：简短回应，帮助进入讨论。
Member B：简短回应，并确认该 item 的可能价值。
Member C：自然地进行简短回应。

注意：
不要触发正式 speaking-intention event。
```

---

## B1-T2-E1 Staff Episode 提示板 — 单一成员进入请求

```text
时间： 40–100s
Context： Leader speaking
Scenario： 荒岛求生
本 episode 的 item：
- Rope（绳子）

Episode 任务：
Leader 正在解释绳子的用途。Member B 是具有有效补充观点的 target member。

Discussion focus：
- Can rope be used to build shelter or secure objects?
- 它是否可用于制作陷阱、搬运物品或与其他工具组合使用？
- Does rope become more useful when combined with a knife, tent, or hammock?
- 它的通用性是否比单一用途物品更有价值？

Staff 角色任务：
Member A：普通参与者。倾听即可，只做简短回应。
Member B: Target member. Prepare a supplementary point about the multi-purpose value of rope. Wait for the leader to invite you.
Member C：普通参与者。倾听即可，只做简短回应。

注意：
Member B 不应打断。如果 leader 邀请 B，则该事件成功。
```

---

## B1-T2-E2 Staff Episode 提示板 — 竞争性进入请求

```text
时间： 100–170s
Context： Leader speaking
Scenario： 荒岛求生
本 episode 的 item：
- Lighter / matches（打火机/火柴）

Episode 任务：
两名成员都对火源的重要性有观点。Member C 是 primary target，Member A 是 secondary competitor。

Discussion focus：
- 火源是否有助于保暖、煮水、驱虫和产生求救烟雾？
- 在潮湿的海岛环境中，火源是否可靠？
- Is lighter/matches more sustainable than a flare gun?
- Does fire complement the water filter or shelter items?

Staff 角色任务：
Member A: Secondary competitor. Prepare a conditional point, such as “fire may be less reliable if the environment is wet.” Do not appear stronger than C.
Member B：普通参与者。倾听，不参与竞争。
Member C: Primary target. Prepare the main point, such as “fire supports multiple survival needs: boiling water, warmth, and rescue signaling.” Wait for the leader to invite you.

注意：
预期路径：leader 邀请 C。
如果 leader 先邀请 A，则记录为 target mismatch。
```

---

## B1-T2-E3 Staff Episode 提示板 — 重复单一进入请求

```text
时间： 170–240s
Context： Leader speaking
Scenario： 荒岛求生
本 episode 的 item：
- Tent / hammock（帐篷/吊床）

Episode 任务：
Leader 正在讨论庇护/休息相关 item。Member A 是具有补充观点的主要 target。

Discussion focus：
- 这个 item 能否提供庇护、休息、遮阳和防虫保护？
- 在荒岛上，防雨、防晒和防潮是否重要？
- 庇护类 item 是否应排在水、火源和求救工具之后？
- 如果地面潮湿或不安全，吊床是否更有用？

Staff 角色任务：
Member A: Target member. Prepare one clear supplementary point about shelter and rest. Wait for the leader to invite you.
Member B：普通参与者。倾听即可，只做简短回应。
Member C：普通参与者。倾听即可，只做简短回应。

注意：
Member A 不应打断。如果 leader 邀请 A，则该事件成功。
```

---

## B1-T2-S Staff Episode 提示板 — Summary Stage（总结阶段）

```text
时间： 240–300s
Context： Trial wrap-up
Scenario： 荒岛求生
本阶段的 items：
- First aid kit（急救包）
- Rope（绳子）
- Lighter / matches（打火机/火柴）
- Tent / hammock（帐篷/吊床）

Episode 任务：
帮助 leader 总结第二组荒岛求生 items。

Discussion focus：
- Which item is most urgent?
- Which item is most flexible?
- Which item supports long-term survival?
- Are there any final comments before ending the trial?

Staff 角色任务：
Member A：如被邀请，可确认讨论结果或简短补充。
Member B：如被邀请，可确认讨论结果或简短补充。
Member C：如被邀请，可确认讨论结果或简短补充。

注意：
不要开启新的主要 speaking-intention event。
```


---

## B1-T3 Leader Trial 提示板 — 生理感知反馈 Trial

```text
Trial：荒岛求生 — Speaking Context — 生理感知反馈
时长：300 秒

你的角色：
你是 leader。请引导小组讨论，解释或总结当前 item，并在合适时机把发言机会交给可能想补充观点的成员。

反馈条件：
在本 trial 中，系统可能会根据你当前的生理状态调整反馈界面。根据具体情况，反馈可能以轻量提示、更明确的实时提示，或延迟/总结式提示的形式出现。

本 trial 目标：
讨论另外四个荒岛求生 items，并判断它们的相对重要性。请将系统反馈作为决策支持，但仍然自然地管理讨论。

本 trial 的 items：
1. Water container（储水容器）
2. Machete（砍刀）
3. Signal mirror（信号镜）
4. Mosquito net（蚊帐）

Episode 与 item 对应结构：
Opening phase（0–40s）： Water container（储水容器）
Episode 1（40–100s）： Machete（砍刀）
Episode 2（100–170s）： Signal mirror（信号镜）
Episode 3（170–240s）： Mosquito net（蚊帐）
Summary stage（240–300s）： 总结四个 items

请关注：
- Which items are most important for island survival.
- 每个 item 是否支持储水、工具使用、求救、休息或防护。
- 是否需要邀请某位成员补充观点。
- 反馈形式可能发生变化，但讨论目标保持不变。
```

---

## B1-T3-O Staff Episode 提示板 — Opening Phase（开场阶段）

```text
时间： 0–40s
Context： Speaking Context
Scenario： 荒岛求生
反馈条件：
生理感知反馈 trial

本 episode 的 item：
- Water container（储水容器）

Episode 任务：
自然开始本 trial。让 leader 介绍新的荒岛求生 item 集合，并开始讨论是否应优先选择储水容器。

Discussion focus：
- Can a water container help store collected rainwater or filtered freshwater?
- Is storage useful if water sources are found only occasionally?
- Does it work together with a water filter or fire?
- Should it be ranked above rescue or protection items?

Staff 角色任务：
Member A：简短回应 leader，帮助进入讨论。
Member B：简短回应，或自然地表示同意/不同意。
Member C：简短回应并确认理解任务。

注意：
本阶段不要触发正式 speaking-intention event。
不要争抢发言权。
反馈形式可能变化，但 staff 行为应保持自然。
```

---

## B1-T3-E1 Staff Episode 提示板 — 单一成员进入请求

```text
时间： 40–100s
Context： Leader speaking
Scenario： 荒岛求生
反馈条件：
生理感知反馈 trial

本 episode 的 item：
- Machete（砍刀）

Episode 任务：
Leader 正在解释或总结砍刀的用途。Member C 有一个有价值的补充观点，应被视为主要 target。

Discussion focus：
- Can the machete cut branches, clear vegetation, or prepare materials for shelter?
- Is it more powerful but less precise than a small knife?
- Can it help with food preparation or protection?
- Should it be ranked high because it is a multi-purpose tool?

Staff 角色任务：
Member A：普通参与者。倾听即可，如有需要只做简短回应。
Member B：普通参与者。倾听即可，如有需要只做简短回应。
Member C: Target member. Prepare one clear supplementary point, such as “the machete can help build shelter quickly by cutting branches and clearing vegetation.” Wait for the leader to invite you. If invited, speak naturally.

注意：
Member C 不应打断 leader。如果 leader 邀请 C，则该事件成功。
```

---

## B1-T3-E2 Staff Episode 提示板 — 竞争性进入请求

```text
时间： 100–170s
Context： Leader speaking
Scenario： 荒岛求生
反馈条件：
生理感知反馈 trial

本 episode 的 item：
- Signal mirror（信号镜）

Episode 任务：
Leader 正在讨论信号镜。两名成员都有可能补充观点，但 Member A 是更强的 target，Member B 是较低优先级的 competitor。

Discussion focus：
- Can the signal mirror help attract ships or aircraft during the day?
- Is it reusable compared with a flare gun?
- Does it depend on sunlight and visibility?
- Should rescue signaling be ranked above long-term survival tools?

Staff 角色任务：
Member A: Primary target. Prepare the main point, such as “a signal mirror is reusable and can be valuable for daytime rescue.” If invited, speak naturally.
Member B: Secondary competitor. Prepare a weaker or more conditional point, such as “the mirror is less useful at night or in cloudy weather.” If invited, speak naturally but do not appear stronger than A.
Member C：普通参与者。倾听，不加入竞争。

注意：
预期路径：leader 邀请 A。
如果 leader 先邀请 B，则记录为 target mismatch。
如果 A 和 B 都没有被邀请，则记录为 missed。
```

---

## B1-T3-E3 Staff Episode 提示板 — 重复单一进入请求

```text
时间： 170–240s
Context： Leader speaking
Scenario： 荒岛求生
反馈条件：
生理感知反馈 trial

本 episode 的 item：
- Mosquito net（蚊帐）

Episode 任务：
Leader 正在解释或总结蚊帐的防护价值。Member B 是主要 target。

Discussion focus：
- Can the mosquito net protect from insects and improve sleep quality?
- Does preventing bites reduce infection or discomfort?
- Is rest important enough to rank this item highly?
- Is it less urgent than water, fire, or rescue items?

Staff 角色任务：
Member A：普通参与者。倾听即可，只做简短回应。
Member B: Target member. Prepare one supplementary point, such as “better sleep and fewer insect bites may help the group maintain energy over several days.” Wait for the leader to invite you.
Member C：普通参与者。倾听即可，只做简短回应。

注意：
Member B 不应打断。如果 leader 邀请 B，则该事件成功。
```

---

## B1-T3-S Staff Episode 提示板 — Summary Stage（总结阶段）

```text
时间： 240–300s
Context： Trial wrap-up
Scenario： 荒岛求生
反馈条件：
生理感知反馈 trial

本阶段的 items：
- Water container（储水容器）
- Machete（砍刀）
- Signal mirror（信号镜）
- Mosquito net（蚊帐）

Episode 任务：
帮助 leader 总结本生理感知反馈 trial 中的四个 items。

Discussion focus：
- Which item best supports water management?
- Which item is the most flexible tool?
- Which item best supports rescue?
- Which item supports protection and rest?
- Are there any final comments before ending the trial?

Staff 角色任务：
Member A：如被邀请，可确认讨论结果或简短补充。
Member B：如被邀请，可确认讨论结果或简短补充。
Member C：如被邀请，可确认讨论结果或简短补充。

注意：
不要开启新的主要 speaking-intention event。
```


# Block 2：Listening Context — Desert Survival（沙漠求生）

## B2-T1 Leader Trial 提示板

```text
Trial：沙漠求生 — Listening Context
时长：300 秒

你的角色：
你是 leader。请主要观察和协调讨论。让成员展开讨论，注意谁还没有足够发言机会，并在合适时邀请成员加入。

本 trial 目标：
讨论前四个沙漠求生 items，并保持讨论参与的平衡。

本 trial 的 items：
1. Cosmetic mirror（化妆镜/信号镜）
2. Top coat per person（每人一件外套）
3. Water per person（每人一份水）
4. Flashlight（手电筒）

Episode 与 item 对应结构：
Opening phase（0–40s）： Cosmetic mirror（化妆镜/信号镜）
Episode 1（40–100s）： Top coat per person（每人一件外套）
Episode 2（100–170s）： Water per person（每人一份水）
Episode 3（170–240s）： Flashlight（手电筒）
Summary stage（240–300s）： 总结四个 items

请关注：
- 哪些 items 有助于沙漠生存。
- 小组应该留在坠落点等待救援，还是主动移动。
- 是否有成员还没有获得足够发言机会。
```

---

## B2-T1-O Staff Episode 提示板 — Opening Phase（开场阶段）

```text
时间： 0–40s
Context： Listening Context
Scenario： 沙漠求生
本 episode 的 item：
- Cosmetic mirror（化妆镜/信号镜）

Episode 任务：
Leader 介绍沙漠求生任务，并把讨论交给成员。Member A 和 Member B 开始讨论化妆镜/信号镜。

Discussion focus：
- Can the mirror reflect sunlight to signal rescuers?
- Is signaling more important than moving?
- Is the mirror better for daytime rescue than a flashlight?
- Should the group stay near the crash site and signal?

Staff 角色任务：
Member A：active speaker。开始讨论该 item。
Member B：active speaker。回应 A 并继续讨论。
Member C：普通参与者。保持参与感，但不要成为 target。

注意：
不要触发正式 speaking-intention event。
```

---

## B2-T1-E1 Staff Episode 提示板 — 双人讨论中的被压制进入请求

```text
时间： 40–100s
Context： Two-person discussion
Scenario： 沙漠求生
本 episode 的 item：
- Top coat per person（每人一件外套）

Episode 任务：
Member A 和 Member B 正在讨论外套。Member C 有补充观点，但还没有获得发言机会。

Discussion focus：
- Can a top coat reduce sun exposure and water loss during the day?
- Can it provide warmth at night?
- Is it useful even though the desert is hot?
- Should it be ranked above or below water and signaling tools?

Staff 角色任务：
Member A: Active speaker. Discuss why the coat may or may not be useful.
Member B: Active speaker. Respond to A and maintain the two-person discussion.
Member C: Target member. Prepare a point such as “the coat can protect from sun and reduce dehydration.” Wait for the leader to invite you.

注意：
Member C 不应打断。如果 leader 邀请 C，则该事件成功。
```

---

## B2-T1-E2 Staff Episode 提示板 — 主导发言者压制目标成员

```text
时间： 100–170s
Context： Dominant speaker discussion
Scenario： 沙漠求生
本 episode 的 item：
- Water per person（每人一份水）

Episode 任务：
Member A 围绕水说得比其他人更多。Member B 还没有机会表达一个重要观点。

Discussion focus：
- Is water the most urgent desert survival need?
- Should the group conserve water by staying near the crash site?
- Does moving consume too much water?
- How should water be ranked relative to signaling or shade items?

Staff 角色任务：
Member A: Dominant speaker. Lead the discussion and give several reasons, but do not overact.
Member B: Target member. Prepare a point such as “water is essential, but movement strategy affects how quickly it is consumed.” Wait for the leader to invite you.
Member C: Secondary responder. Give short responses to A and do not become the target.

注意：
Member B 是 target。如果 leader 邀请 B，则该事件成功。
```

---

## B2-T1-E3 Staff Episode 提示板 — 重复被压制进入事件

```text
时间： 170–240s
Context： Two-person discussion
Scenario： 沙漠求生
本 episode 的 item：
- Flashlight（手电筒）

Episode 任务：
Member B 和 Member C 正在讨论手电筒。Member A 有补充观点，但还没有获得发言机会。

Discussion focus：
- Can the flashlight help with night signaling?
- Is battery life a limitation?
- Does it complement the mirror, which works during the day?
- Is night movement safe or risky?

Staff 角色任务：
Member A: Target member. Prepare a point such as “the flashlight is useful mainly for night signaling or movement.” Wait for the leader to invite you.
Member B: Active speaker. Discuss the item with C.
Member C: Active speaker. Respond to B and maintain the two-person discussion.

注意：
Member A 不应打断。如果 leader 邀请 A，则该事件成功。
```

---

## B2-T1-S Staff Episode 提示板 — Summary Stage（总结阶段）

```text
时间： 240–300s
Context： Trial wrap-up
Scenario： 沙漠求生
本阶段的 items：
- Cosmetic mirror（化妆镜/信号镜）
- Top coat per person（每人一件外套）
- Water per person（每人一份水）
- Flashlight（手电筒）

Episode 任务：
帮助 leader 总结前四个沙漠求生 items。

Discussion focus：
- Which item best supports rescue?
- Which item best supports survival in heat?
- Should the group prioritize water or signaling?
- Are there any final comments before ending the trial?

Staff 角色任务：
Member A：如被邀请，可确认讨论结果或简短补充。
Member B：如被邀请，可确认讨论结果或简短补充。
Member C：如被邀请，可确认讨论结果或简短补充。

注意：
不要开启新的主要 speaking-intention event。
```

---

## B2-T2 Leader Trial 提示板

```text
Trial：沙漠求生 — Listening Context
时长：300 秒

你的角色：
你是 leader。请主要观察和协调讨论，鼓励平衡参与，并邀请尚未获得发言机会的成员发言。

本 trial 目标：
讨论后四个沙漠求生 items，并保持讨论参与的平衡。

本 trial 的 items：
1. Parachute（降落伞）
2. Jack knife（折叠刀）
3. Sunglasses（太阳镜）
4. Map / compass（地图/指南针）

Episode 与 item 对应结构：
Opening phase（0–40s）： Parachute（降落伞）
Episode 1（40–100s）： Jack knife（折叠刀）
Episode 2（100–170s）： Sunglasses（太阳镜）
Episode 3（170–240s）： Map / compass（地图/指南针）
Summary stage（240–300s）： 总结四个 items

请关注：
- 每个 item 是否有助于遮阳、求救信号、工具使用或导航。
- 留在坠落点或主动移动是否会改变 item 排序。
- 是否有成员还没有获得足够发言机会。
```

---

## B2-T2-O Staff Episode 提示板 — Opening Phase（开场阶段）

```text
时间： 0–40s
Context： Listening Context
Scenario： 沙漠求生
本 episode 的 item：
- Parachute（降落伞）

Episode 任务：
Leader 介绍第二组沙漠求生 items，并让成员开始讨论降落伞。

Discussion focus：
- Can the parachute be used as shade?
- Can its color help signal rescuers from the air?
- Can it become temporary shelter or a ground marker?
- Is it more useful if the group stays near the crash site?

Staff 角色任务：
Member A: Active speaker. Begin discussing the item.
Member B：普通参与者。保持参与感，但不要成为 target。
Member C：active speaker。回应 A 并继续讨论。

注意：
不要触发正式 speaking-intention event。
```

---

## B2-T2-E1 Staff Episode 提示板 — 双人讨论中的被压制进入请求

```text
时间： 40–100s
Context： Two-person discussion
Scenario： 沙漠求生
本 episode 的 item：
- Jack knife（折叠刀）

Episode 任务：
Member A 和 Member C 正在讨论折叠刀。Member B 有补充观点，但还没有获得发言机会。

Discussion focus：
- Can the jack knife be used for cutting, repairing, or making simple tools?
- Can it help process cactus or rope-like materials?
- Is it more useful than a signaling item?
- Is it flexible enough to rank highly?

Staff 角色任务：
Member A: Active speaker. Discuss the item with C.
Member B: Target member. Prepare a point about the flexible tool value of the jack knife. Wait for the leader to invite you.
Member C: Active speaker. Respond to A and maintain the discussion.

注意：
Member B 不应打断。如果 leader 邀请 B，则该事件成功。
```

---

## B2-T2-E2 Staff Episode 提示板 — 主导发言者压制目标成员

```text
时间： 100–170s
Context： Dominant speaker discussion
Scenario： 沙漠求生
本 episode 的 item：
- Sunglasses（太阳镜）

Episode 任务：
Member B 主导了太阳镜的讨论。Member C 还没有表达一个重要观点。

Discussion focus：
- Can sunglasses prevent glare and eye fatigue?
- Are they useful if the group moves during the day?
- Are they less important if the group stays near the crash site?
- How do they compare with shade or clothing items?

Staff 角色任务：
Member A: Secondary responder. Give short responses to B.
Member B: Dominant speaker. Lead the discussion and give several reasons, but do not overact.
Member C: Target member. Prepare a point such as “if we stay near the crash site, sunglasses may be less important than signaling or shade.” Wait for the leader to invite you.

注意：
Member C 是 target。如果 leader 邀请 C，则该事件成功。
```

---

## B2-T2-E3 Staff Episode 提示板 — 重复被压制进入事件

```text
时间： 170–240s
Context： Two-person discussion
Scenario： 沙漠求生
本 episode 的 item：
- Map / compass（地图/指南针）

Episode 任务：
Member B 和 Member C 正在讨论地图/指南针。Member A 有补充观点，但还没有获得发言机会。

Discussion focus：
- Is navigation useful if the group decides to move?
- Is navigation less useful if the group stays at the crash site?
- Should the group first decide “stay vs. move” before ranking this item?
- Can any part of the compass help with signaling or fire?

Staff 角色任务：
Member A: Target member. Prepare a point such as “map/compass is only valuable if we decide to leave the crash site.” Wait for the leader to invite you.
Member B: Active speaker. Discuss the item with C.
Member C: Active speaker. Respond to B and continue the discussion.

注意：
Member A 不应打断。如果 leader 邀请 A，则该事件成功。
```

---

## B2-T2-S Staff Episode 提示板 — Summary Stage（总结阶段）

```text
时间： 240–300s
Context： Trial wrap-up
Scenario： 沙漠求生
本阶段的 items：
- Parachute（降落伞）
- Jack knife（折叠刀）
- Sunglasses（太阳镜）
- Map / compass（地图/指南针）

Episode 任务：
帮助 leader 总结第二组沙漠求生 items。

Discussion focus：
- Which item is best for shade or shelter?
- Which item is best for signaling?
- Which item is only useful if the group moves?
- Are there any final comments before ending the trial?

Staff 角色任务：
Member A：如被邀请，可确认讨论结果或简短补充。
Member B：如被邀请，可确认讨论结果或简短补充。
Member C：如被邀请，可确认讨论结果或简短补充。

注意：
不要开启新的主要 speaking-intention event。
```


---

## B2-T3 Leader Trial 提示板 — 生理感知反馈 Trial

```text
Trial：沙漠求生 — Listening Context — 生理感知反馈
时长：300 秒

你的角色：
你是 leader。请主要观察和协调讨论。让成员展开讨论，注意谁还没有足够发言机会，并在合适时邀请成员加入。

反馈条件：
在本 trial 中，系统可能会根据你当前的生理状态调整反馈界面。根据具体情况，反馈可能以轻量提示、更明确的实时提示，或延迟/总结式提示的形式出现。

本 trial 目标：
讨论另外四个沙漠求生 items，并保持讨论参与的平衡。请将系统反馈作为决策支持，但仍然自然地协调讨论。

本 trial 的 items：
1. Plastic raincoat（塑料雨衣）
2. Pistol（手枪）
3. Alcohol bottle（酒精瓶）
4. Desert animals guidebook（沙漠动物指南）

Episode 与 item 对应结构：
Opening phase（0–40s）： Plastic raincoat（塑料雨衣）
Episode 1（40–100s）： Pistol（手枪）
Episode 2（100–170s）： Alcohol bottle（酒精瓶）
Episode 3（170–240s）： Desert animals guidebook（沙漠动物指南）
Summary stage（240–300s）： 总结四个 items

请关注：
- 每个 item 是否有助于遮阳、信号、安全、医疗使用或食物识别。
- 留在坠落点或主动移动是否会改变 item 排序。
- 是否有成员还没有获得足够发言机会。
- 反馈形式可能发生变化，但讨论目标保持不变。
```

---

## B2-T3-O Staff Episode 提示板 — Opening Phase（开场阶段）

```text
时间： 0–40s
Context： Listening Context
Scenario： 沙漠求生
反馈条件：
生理感知反馈 trial

本 episode 的 item：
- Plastic raincoat（塑料雨衣）

Episode 任务：
Leader 介绍新的沙漠求生 item 集合，并把讨论交给成员。Member A 和 Member B 开始讨论塑料雨衣。

Discussion focus：
- Can the raincoat be used as shade or a ground cover?
- Can it reduce sun exposure even if rain is unlikely?
- Can it help collect condensation or protect supplies?
- Is it less useful than stronger signaling or water-related items?

Staff 角色任务：
Member A：active speaker。开始讨论该 item。
Member B：active speaker。回应 A 并继续讨论。
Member C：普通参与者。保持参与感，但不要成为 target。

注意：
不要触发正式 speaking-intention event。
反馈形式可能变化，但 staff 行为应保持自然。
```

---

## B2-T3-E1 Staff Episode 提示板 — 双人讨论中的被压制进入请求

```text
时间： 40–100s
Context： Two-person discussion
Scenario： 沙漠求生
反馈条件：
生理感知反馈 trial

本 episode 的 item：
- Pistol（手枪）

Episode 任务：
Member A 和 Member B 正在讨论手枪。Member C 有补充观点，但还没有获得发言机会。

Discussion focus：
- Can the pistol be used for sound signaling?
- Is it useful for safety, or is that less relevant than rescue signaling?
- Are bullets limited, making it a short-use item?
- Does noise signaling work better if rescuers are nearby?

Staff 角色任务：
Member A: Active speaker. Discuss why the pistol may or may not be useful.
Member B: Active speaker. Respond to A and maintain the two-person discussion.
Member C: Target member. Prepare a point such as “the pistol may be more useful as a sound signal than as a weapon.” Wait for the leader to invite you.

注意：
Member C 不应打断。如果 leader 邀请 C，则该事件成功。
```

---

## B2-T3-E2 Staff Episode 提示板 — 主导发言者压制目标成员

```text
时间： 100–170s
Context： Dominant speaker discussion
Scenario： 沙漠求生
反馈条件：
生理感知反馈 trial

本 episode 的 item：
- Alcohol bottle（酒精瓶）

Episode 任务：
Member A 围绕酒精瓶说得比其他人更多。Member B 还没有机会表达一个重要观点。

Discussion focus：
- Can alcohol be used for first aid or disinfection?
- Is drinking alcohol dangerous in the desert because it may worsen dehydration?
- Can the bottle itself be used as a container or signaling object?
- Should it be ranked low because of the dehydration risk?

Staff 角色任务：
Member A: Dominant speaker. Lead the discussion and give several reasons, but do not overact.
Member B: Target member. Prepare a point such as “drinking alcohol is risky in the desert, but it may have limited value for disinfection.” Wait for the leader to invite you.
Member C: Secondary responder. Give short responses to A and do not become the target.

注意：
Member B 是 target。如果 leader 邀请 B，则该事件成功。
```

---

## B2-T3-E3 Staff Episode 提示板 — 重复被压制进入事件

```text
时间： 170–240s
Context： Two-person discussion
Scenario： 沙漠求生
反馈条件：
生理感知反馈 trial

本 episode 的 item：
- Desert animals guidebook（沙漠动物指南）

Episode 任务：
Member B 和 Member C 正在讨论沙漠动物指南。Member A 有补充观点，但还没有获得发言机会。

Discussion focus：
- Can the guidebook help identify edible or dangerous desert animals?
- Is food identification less urgent than water and rescue?
- Does the group have enough time and energy to use the book?
- Is it only useful if the group plans to move or forage?

Staff 角色任务：
Member A: Target member. Prepare a point such as “the guidebook may help only in longer survival situations, but it is not as urgent as water or signaling.” Wait for the leader to invite you.
Member B: Active speaker. Discuss the item with C.
Member C: Active speaker. Respond to B and maintain the two-person discussion.

注意：
Member A 不应打断。如果 leader 邀请 A，则该事件成功。
```

---

## B2-T3-S Staff Episode 提示板 — Summary Stage（总结阶段）

```text
时间： 240–300s
Context： Trial wrap-up
Scenario： 沙漠求生
反馈条件：
生理感知反馈 trial

本阶段的 items：
- Plastic raincoat（塑料雨衣）
- Pistol（手枪）
- Alcohol bottle（酒精瓶）
- Desert animals guidebook（沙漠动物指南）

Episode 任务：
帮助 leader 总结本生理感知反馈 trial 中的四个 items。

Discussion focus：
- Which item best supports shade or protection?
- Which item best supports signaling or safety?
- Which item has risks in the desert?
- Which item is only useful for long-term survival?
- Are there any final comments before ending the trial?

Staff 角色任务：
Member A：如被邀请，可确认讨论结果或简短补充。
Member B：如被邀请，可确认讨论结果或简短补充。
Member C：如被邀请，可确认讨论结果或简短补充。

注意：
不要开启新的主要 speaking-intention event。
```


# Block 3：Silence Context — Mountain Survival（深山求生）

## B3-T1 Leader Trial 提示板

```text
Trial：深山求生 — Silence Context
时长：300 秒

你的角色：
你是 leader。请引导讨论，并在小组沉默或讨论失去节奏时帮助重启讨论。

本 trial 目标：
讨论前四个深山求生 items，并维持小组讨论节奏。

本 trial 的 items：
1. Matches / lighter（火柴/打火机）
2. Polythene sheeting / heavy canvas（塑料布/厚帆布）
3. First-aid kit（急救包）
4. Signal flares（信号弹）

Episode 与 item 对应结构：
Opening phase（0–40s）： Matches / lighter（火柴/打火机）
Episode 1（40–100s）： Polythene sheeting / heavy canvas（塑料布/厚帆布）
Episode 2（100–170s）： First-aid kit（急救包）
Episode 3（170–240s）： Signal flares（信号弹）
Summary stage（240–300s）： 总结四个 items

请关注：
- 哪些 items 有助于保暖、庇护、安全、求救或伤害处理。
- 小组讨论是否出现停顿。
- 应该邀请谁来重启讨论。
```

---

## B3-T1-O Staff Episode 提示板 — Opening Phase（开场阶段）

```text
时间： 0–40s
Context： Silence Context
Scenario： 深山求生
本 episode 的 item：
- Matches / lighter（火柴/打火机）

Episode 任务：
开始深山求生讨论。小组在观点之间可以自然出现短暂停顿。

Discussion focus：
- Can fire help with warmth, rescue signaling, and boiling water?
- Is fire more urgent than food in a mountain or cold environment?
- Does fire require dry materials or shelter?
- Should it be ranked very highly?

Staff 角色任务：
Member A：普通参与者。自然加入讨论。
Member B：普通参与者。自然加入讨论。
Member C：普通参与者。自然加入讨论。

注意：
不要强行持续讲话。自然的短暂停顿是可以接受的。
不要触发正式 speaking-intention event。
```

---

## B3-T1-E1 Staff Episode 提示板 — 沉默后的首次明确重启

```text
时间： 40–100s
Context： Group silence / restart
Scenario： 深山求生
本 episode 的 item：
- Polythene sheeting / heavy canvas（塑料布/厚帆布）

Episode 任务：
前一段讨论放缓后，小组出现短暂沉默。Member A 是最适合继续庇护相关讨论的人。

Discussion focus：
- Can sheeting/canvas protect from wind, rain, and cold?
- Can it be used as a temporary shelter, ground cover, or signal marker?
- Does it work well together with fire?
- Is shelter more important than moving to find help?

Staff 角色任务：
Member A: Target member. Prepare a point about shelter value. Wait for the leader to invite you, then restart the discussion.
Member B：waiting member。不要率先重启讨论。
Member C：waiting member。不要率先重启讨论。

注意：
在 leader 采取行动前，允许出现自然的短暂停顿。
```

---

## B3-T1-E2 Staff Episode 提示板 — 竞争性重启尝试

```text
时间： 100–170s
Context： Group silence / competing restart
Scenario： 深山求生
本 episode 的 item：
- First-aid kit（急救包）

Episode 任务：
短暂停顿后，Member B 和 Member C 都可以继续讨论，但 Member B 是 primary restart target。

Discussion focus：
- Is a first-aid kit necessary for cuts, falls, frostbite, or minor injuries?
- Is it essential if no one is currently injured?
- Should it be ranked below fire and shelter?
- Can it improve the safety of team movement?

Staff 角色任务：
Member A: Waiting member. Do not compete for the restart.
Member B: Primary target. Prepare the more relevant continuation point. Wait for the leader to invite you.
Member C: Secondary candidate. Prepare a possible point, but do not appear stronger than B.

注意：
预期路径：leader 邀请 B。
如果 leader 先邀请 C，则记录为 target mismatch。
```

---

## B3-T1-E3 Staff Episode 提示板 — 重复重启事件

```text
时间： 170–240s
Context： Group silence / restart
Scenario： 深山求生
本 episode 的 item：
- Signal flares（信号弹）

Episode 任务：
小组再次出现短暂沉默。Member C 是最适合继续讨论信号弹的人。

Discussion focus：
- Can signal flares attract rescue teams?
- Are they more useful at night or during the day?
- Are they limited-use items?
- Should they be saved until rescue is likely nearby?

Staff 角色任务：
Member A：waiting member。不要率先重启讨论。
Member B：waiting member。不要率先重启讨论。
Member C: Target member. Prepare a point about rescue signaling. Wait for the leader to invite you.

注意：
在 leader 采取行动前，允许出现自然的短暂停顿。
```

---

## B3-T1-S Staff Episode 提示板 — Summary Stage（总结阶段）

```text
时间： 240–300s
Context： Trial wrap-up
Scenario： 深山求生
本阶段的 items：
- Matches / lighter（火柴/打火机）
- Polythene sheeting / heavy canvas（塑料布/厚帆布）
- First-aid kit（急救包）
- Signal flares（信号弹）

Episode 任务：
帮助 leader 总结前四个深山求生 items。

Discussion focus：
- Which item is most important for warmth?
- Which item is most important for shelter?
- Which item is most important for rescue?
- Are there any final comments before ending the trial?

Staff 角色任务：
Member A：如被邀请，可确认讨论结果或简短补充。
Member B：如被邀请，可确认讨论结果或简短补充。
Member C：如被邀请，可确认讨论结果或简短补充。

注意：
不要开启新的主要 speaking-intention event。
```

---

## B3-T2 Leader Trial 提示板

```text
Trial：深山求生 — Silence Context
时长：300 秒

你的角色：
你是 leader。请引导讨论，并在小组沉默或讨论失去节奏时帮助重启讨论。

本 trial 目标：
讨论后四个深山求生 items，并维持小组讨论节奏。

本 trial 的 items：
1. Bottled water（瓶装水）
2. Toolbox / hand axe / knife（工具箱/手斧/刀）
3. Extra clothing / blanket（额外衣物/毯子）
4. Chocolate / high-energy food（巧克力/高能量食物）

Episode 与 item 对应结构：
Opening phase（0–40s）： Bottled water（瓶装水）
Episode 1（40–100s）： Toolbox / hand axe / knife（工具箱/手斧/刀）
Episode 2（100–170s）： Extra clothing / blanket（额外衣物/毯子）
Episode 3（170–240s）： Chocolate / high-energy food（巧克力/高能量食物）
Summary stage（240–300s）： 总结四个 items

请关注：
- 哪些 items 有助于补水、工具使用、保暖和能量补给。
- 小组讨论是否出现停顿。
- 应该邀请谁来重启讨论。
```

---

## B3-T2-O Staff Episode 提示板 — Opening Phase（开场阶段）

```text
时间： 0–40s
Context： Silence Context
Scenario： 深山求生
本 episode 的 item：
- Bottled water（瓶装水）

Episode 任务：
开始第二个深山求生讨论。小组在观点之间可以自然出现短暂停顿。

Discussion focus：
- Is water easy or difficult to find in the mountain environment?
- Is bottled water important for short-term survival?
- Does it need to be combined with fire or filtering?
- Is hydration more urgent than warmth or shelter?

Staff 角色任务：
Member A：普通参与者。自然加入讨论。
Member B：普通参与者。自然加入讨论。
Member C：普通参与者。自然加入讨论。

注意：
自然停顿是可以接受的。
不要触发正式 speaking-intention event。
```

---

## B3-T2-E1 Staff Episode 提示板 — 沉默后的首次明确重启

```text
时间： 40–100s
Context： Group silence / restart
Scenario： 深山求生
本 episode 的 item：
- Toolbox / hand axe / knife（工具箱/手斧/刀）

Episode 任务：
前一段讨论放缓后，小组出现短暂沉默。Member C 是最适合继续工具类讨论的人。

Discussion focus：
- Can tools help cut branches, build shelter, or repair equipment?
- Are tools more flexible than single-purpose items?
- Are they too heavy to carry?
- Can they work together with rope, canvas, and fire?

Staff 角色任务：
Member A：waiting member。不要率先重启讨论。
Member B：waiting member。不要率先重启讨论。
Member C: Target member. Prepare a point about the flexible value of tools. Wait for the leader to invite you.

注意：
在 leader 采取行动前，允许出现自然的短暂停顿。
```

---

## B3-T2-E2 Staff Episode 提示板 — 竞争性重启尝试

```text
时间： 100–170s
Context： Group silence / competing restart
Scenario： 深山求生
本 episode 的 item：
- Extra clothing / blanket（额外衣物/毯子）

Episode 任务：
短暂停顿后，Member A 和 Member B 都可以继续讨论，但 Member A 是 primary restart target。

Discussion focus：
- Can extra clothing or a blanket prevent hypothermia?
- Is warmth more important at night than during the day?
- Does it work together with fire and shelter?
- Should it be ranked above food or tools?

Staff 角色任务：
Member A: Primary target. Prepare the stronger continuation point about warmth and hypothermia prevention. Wait for the leader to invite you.
Member B: Secondary candidate. Prepare a possible point, but do not appear stronger than A.
Member C: Waiting member. Do not compete for the restart.

注意：
预期路径：leader 邀请 A。
如果 leader 先邀请 B，则记录为 target mismatch。
```

---

## B3-T2-E3 Staff Episode 提示板 — 重复重启事件

```text
时间： 170–240s
Context： Group silence / restart
Scenario： 深山求生
本 episode 的 item：
- Chocolate / high-energy food（巧克力/高能量食物）

Episode 任务：
小组再次出现短暂沉默。Member B 是最适合继续讨论能量食物的人。

Discussion focus：
- Can high-energy food maintain physical energy and body heat?
- Is food less urgent than water, fire, or shelter?
- Is chocolate light and easy to share?
- Does it matter more if the group needs to walk or wait for rescue?

Staff 角色任务：
Member A：waiting member。不要率先重启讨论。
Member B: Target member. Prepare a point about energy and body heat. Wait for the leader to invite you.
Member C：waiting member。不要率先重启讨论。

注意：
在 leader 采取行动前，允许出现自然的短暂停顿。
```

---

## B3-T2-S Staff Episode 提示板 — Summary Stage（总结阶段）

```text
时间： 240–300s
Context： Trial wrap-up
Scenario： 深山求生
本阶段的 items：
- Bottled water（瓶装水）
- Toolbox / hand axe / knife（工具箱/手斧/刀）
- Extra clothing / blanket（额外衣物/毯子）
- Chocolate / high-energy food（巧克力/高能量食物）

Episode 任务：
帮助 leader 总结第二组深山求生 items。

Discussion focus：
- Which item is most important for hydration?
- Which item is most flexible?
- Which item best supports warmth?
- Are there any final comments before ending the trial?

Staff 角色任务：
Member A：如被邀请，可确认讨论结果或简短补充。
Member B：如被邀请，可确认讨论结果或简短补充。
Member C：如被邀请，可确认讨论结果或简短补充。

注意：
不要开启新的主要 speaking-intention event。
```

---

## B3-T3 Leader Trial 提示板 — 生理感知反馈 Trial

```text
Trial：深山求生 — Silence Context — 生理感知反馈
时长：300 秒

你的角色：
你是 leader。请引导讨论，并在小组沉默或讨论失去节奏时帮助重启讨论。

反馈条件：
在本 trial 中，系统可能会根据你当前的生理状态调整反馈界面。根据具体情况，反馈可能以轻量提示、更明确的实时提示，或延迟/总结式提示的形式出现。

本 trial 目标：
讨论另外四个深山求生 items，并维持小组讨论节奏。请将系统反馈作为决策支持，但仍然自然地重启和引导讨论。

本 trial 的 items：
1. Whistle（哨子）
2. Sleeping bag（睡袋）
3. Metal cup / cooking pot（金属杯/锅）
4. Headlamp（头灯）

Episode 与 item 对应结构：
Opening phase（0–40s）： Whistle（哨子）
Episode 1（40–100s）： Sleeping bag（睡袋）
Episode 2（100–170s）： Metal cup / cooking pot（金属杯/锅）
Episode 3（170–240s）： Headlamp（头灯）
Summary stage（240–300s）： 总结四个 items

请关注：
- 哪些 items 有助于信号、保暖、水处理、可见性或移动。
- 小组讨论是否出现停顿。
- 应该邀请谁来重启讨论。
- 反馈形式可能发生变化，但讨论目标保持不变。
```

---

## B3-T3-O Staff Episode 提示板 — Opening Phase（开场阶段）

```text
时间： 0–40s
Context： Silence Context
Scenario： 深山求生
反馈条件：
生理感知反馈 trial

本 episode 的 item：
- Whistle（哨子）

Episode 任务：
开始新的深山求生讨论。小组在观点之间可以自然出现短暂停顿。

Discussion focus：
- Can a whistle help signal rescuers without using much energy?
- Is it useful in fog, forest, or poor visibility?
- Does sound carry far enough in a mountain environment?
- Should it be ranked above visual signaling tools?

Staff 角色任务：
Member A：普通参与者。自然加入讨论。
Member B：普通参与者。自然加入讨论。
Member C：普通参与者。自然加入讨论。

注意：
不要强行持续讲话。自然的短暂停顿是可以接受的。
不要触发正式 speaking-intention event。
```

---

## B3-T3-E1 Staff Episode 提示板 — 沉默后的首次明确重启

```text
时间： 40–100s
Context： Group silence / restart
Scenario： 深山求生
反馈条件：
生理感知反馈 trial

本 episode 的 item：
- Sleeping bag（睡袋）

Episode 任务：
前一段讨论放缓后，小组出现短暂沉默。Member C 是最适合继续讨论睡袋的人。

Discussion focus：
- Can the sleeping bag prevent hypothermia at night?
- Is warmth more urgent than food in a mountain environment?
- Is it useful even without a tent?
- Is it bulky but important for rest and survival?

Staff 角色任务：
Member A：waiting member。不要率先重启讨论。
Member B：waiting member。不要率先重启讨论。
Member C: Target member. Prepare a point about warmth and hypothermia prevention. Wait for the leader to invite you.

注意：
在 leader 采取行动前，允许出现自然的短暂停顿。
```

---

## B3-T3-E2 Staff Episode 提示板 — 竞争性重启尝试

```text
时间： 100–170s
Context： Group silence / competing restart
Scenario： 深山求生
反馈条件：
生理感知反馈 trial

本 episode 的 item：
- Metal cup / cooking pot（金属杯/锅）

Episode 任务：
短暂停顿后，Member A 和 Member B 都可以继续讨论，但 Member A 是 primary restart target。

Discussion focus：
- Can a metal cup or cooking pot be used to boil water?
- Can it help melt snow or prepare warm drinks?
- Is it only useful if the group also has fire?
- Does water preparation make it more important than food?

Staff 角色任务：
Member A: Primary target. Prepare the stronger continuation point about boiling water or melting snow. Wait for the leader to invite you.
Member B: Secondary candidate. Prepare a possible point, but do not appear stronger than A.
Member C: Waiting member. Do not compete for the restart.

注意：
预期路径：leader 邀请 A。
如果 leader 先邀请 B，则记录为 target mismatch。
```

---

## B3-T3-E3 Staff Episode 提示板 — 重复重启事件

```text
时间： 170–240s
Context： Group silence / restart
Scenario： 深山求生
反馈条件：
生理感知反馈 trial

本 episode 的 item：
- Headlamp（头灯）

Episode 任务：
小组再次出现短暂沉默。Member B 是最适合继续讨论头灯的人。

Discussion focus：
- Can the headlamp help the group move safely in low light?
- Is hands-free lighting useful for first aid or building shelter?
- Is battery life a limitation?
- Should the group avoid night movement even with a headlamp?

Staff 角色任务：
Member A：waiting member。不要率先重启讨论。
Member B: Target member. Prepare a point about safe movement or hands-free work. Wait for the leader to invite you.
Member C：waiting member。不要率先重启讨论。

注意：
在 leader 采取行动前，允许出现自然的短暂停顿。
```

---

## B3-T3-S Staff Episode 提示板 — Summary Stage（总结阶段）

```text
时间： 240–300s
Context： Trial wrap-up
Scenario： 深山求生
反馈条件：
生理感知反馈 trial

本阶段的 items：
- Whistle（哨子）
- Sleeping bag（睡袋）
- Metal cup / cooking pot（金属杯/锅）
- Headlamp（头灯）

Episode 任务：
帮助 leader 总结本生理感知反馈 trial 中的四个 items。

Discussion focus：
- Which item best supports rescue signaling?
- Which item best supports warmth and rest?
- Which item best supports water preparation?
- Which item best supports safe movement or night work?
- Are there any final comments before ending the trial?

Staff 角色任务：
Member A：如被邀请，可确认讨论结果或简短补充。
Member B：如被邀请，可确认讨论结果或简短补充。
Member C：如被邀请，可确认讨论结果或简短补充。

注意：
不要开启新的主要 speaking-intention event。
```

