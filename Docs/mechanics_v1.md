# Game Mechanics Design Document · 游戏机制设计文档
## Crown: The Gilded Cage · 王权

**Version:** 0.1  
**Status:** Pre-Production

---

## 1. NPC Affinity System · NPC 好感度系统

### Overview · 概述

Every NPC maintains a hidden Affinity value (0–100). The player's dialogue choices influence each NPC's Affinity over time. Two key thresholds trigger special events:

每个 NPC 都有一个隐藏的好感度数值（0–100）。玩家的对话选择会影响各 NPC 的好感度。两个关键节点会触发特殊事件：

- **Hostile Node · 敌对节点：Affinity ≤ 30**
- **Loyal Node · 归顺节点：Affinity ≥ 80**

---

### Minister · 大臣

| | English | 中文 |
|---|---|---|
| **Hostile (≤30)** | **Corruption Investigation** — He begins skimming the Treasury. Next round's gold income is halved. | **贪污调查** — 他开始克扣金库。下一轮金库收益减半。 |
| **Loyal (≥80)** | **The Money Launderer** — He uses his network to hide a private reserve beneath the Regent's watch. Unlocks a one-time Treasury bonus. | **洗钱高手** — 他通过各种手段在舅舅眼皮底下为你藏一笔私房钱。解锁一次性金库奖励。 |

---

### General · 将军

| | English | 中文 |
|---|---|---|
| **Hostile (≤30)** | **Military Pressure** — He surrounds the palace with troops. Popularity and Church values fluctuate wildly this round. | **军演施压** — 他带兵包围宫殿，本轮民心和教会数值大幅波动。 |
| **Loyal (≥80)** | **The Defector** — In the finale, he may turn against the Regent — or even eliminate him directly. | **政变内应** — 在结局时，他可能倒戈向你，甚至直接干掉舅舅。 |

---

### Priest · 教士

| | English | 中文 |
|---|---|---|
| **Hostile (≤30)** | **Heresy Accusation** — He publicly questions your legitimacy. Popularity automatically loses 5 points every round until resolved. | **异端指控** — 他公开质疑你的合法性。民心每轮自动扣除 5 点，直至事件解决。 |
| **Loyal (≥80)** | **Saintly Coronation** — The Church elevates you to near-divine status among the people. Popularity cannot drop below 0 for the rest of the game. | **圣徒加冕** — 教会在民间为你造神。本局游戏中民心不会降至 0。 |

---

### Princess · 公主（未婚妻）

| | English | 中文 |
|---|---|---|
| **Hostile (≤30)** | **Self-Isolation** — She stops providing intelligence. The player returns to the blind state of Act I. | **自我隔离** — 她不再提供情报，玩家回到第一幕的"瞎摸"状态。 |
| **Loyal (≥80)** | **The Golden Hand** — Once per game, if any resource triggers a death condition, she intervenes through her family's political connections to nullify it. Her family pays a significant political price (reflected in a later random event). Affinity remains unchanged; this ability resets each new game. | **金手指** — 每局一次，若某项资源触发死亡条件，她通过娘家的政治关系将其化解。她的家族将付出重大政治代价（体现在后续随机事件中）。好感度不归零，每局重置。 |

---

### Commoner · 平民

| | English | 中文 |
|---|---|---|
| **Hostile (≤30)** | **Street Protests** — Unrest spreads. Popularity automatically loses 3 points each round. | **街头示威** — 民间骚动蔓延，民心每轮自动扣除 3 点。 |
| **Loyal (≥80)** | **People's Shield** — Civilian networks slow the spread of the Regent's influence. Uncle's Suspicion growth rate is reduced by 30%. | **民间护盾** — 民间自发保护国王，舅舅疑心值增长速度降低 30%。 |

---

## 2. Random Events · 随机事件

### Trigger Rules · 触发规则

- 2–3 random events are inserted across 12 main rounds.
- A **Weighted Random System** determines which event fires:
  - If any resource is in the danger zone (0–20 or 80–100), crisis events are prioritized.
  - If any NPC's Affinity is at an extreme, affinity-linked events are prioritized.
  - Otherwise, events are drawn at random from the full pool.

- 12 轮主线中随机插入 2–3 次事件。
- **权重随机系统**决定触发哪个事件：
  - 若某项资源处于危险区间（0–20 或 80–100），优先触发对应危机事件。
  - 若某 NPC 好感度处于极值，优先触发好感度关联事件。
  - 否则从全部事件池中随机抽取。

---

### Category A · A类：NPC 好感度关联事件 (Affinity-Linked)

| Event · 事件 | Trigger · 触发条件 | Description · 剧情 | Resource Impact · 资源影响 |
|---|---|---|---|
| **Secret Orders · 秘密密令** | General Affinity > 70 | The General sends a trusted aide with a private letter, inviting a secret meeting. | +Military, +Uncle Suspicion |
| **Evidence of Corruption · 贪污的把柄** | Minister Affinity < 30 | You uncover proof that the Minister has been selling military grain. Expose him or use it as leverage? | Expose: +Popularity, -Treasury / Leverage: +Treasury, -Minister Affinity |
| **Midnight Confession · 深夜告解** | Priest Affinity > 70 | The Priest holds a private blessing ceremony, hinting the Church can help conceal your finances. | +Church, -Popularity |
| **The Bloodstained Map · 带血的地图** | Princess Affinity > 70 | She bribes a guard to smuggle you a map of the palace's secret passages. | +Survival chance, +Uncle Suspicion |
| **The Poisoned Cup · 绝望的侍女** | Commoner Affinity < 30 | Your handmaid, whose family perished under your policies, slips a sedative into your wine. | -Popularity, triggers a skipped next round |
| **The Deserting Knight · 叛逃的骑士** | Military Affinity < 30 | A knight who once served your father lays down his sword and refuses to serve you further. | -Military |

---

### Category B · B类：舅舅权力阴影 (The Regent's Shadow)

| Event · 事件 | Description · 剧情 | Hidden Motive · 隐藏动机 | Resource Impact · 资源影响 |
|---|---|---|---|
| **Power Vacuum · 权力的真空** | The Regent falls ill and is absent from court for 3 days. | Tests whether you "bare your fangs" when unwatched. | Grants +1 free action this round |
| **The Deadly Compliment · 致命的赞美** | The Regent publicly praises your wisdom and readiness to govern alone. | A trap — he watches for arrogance. | +Popularity, +Uncle Suspicion if player responds proudly |
| **The Empty Throne · 空荡的座椅** | The Regent skips a session; his chair sits vacant. | Psychological pressure — who truly rules this hall? | +Uncle Suspicion (ambient) |
| **The Regent's Gift · 舅舅的礼物** | The Regent sends you a songbird with its tongue cut out. | A direct threat. The meaning is unmistakable. | +Uncle Suspicion +15, -Popularity |

---

### Category C · C类：氛围与偏执 (Atmosphere & Paranoia)

| Event · 事件 | Description · 剧情 | Feedback · 视觉/音效建议 | Resource Impact · 资源影响 |
|---|---|---|---|
| **Blood on the Walls · 墙上的血字** | The words "FALSE KING MUST DIE" appear carved into a throne hall pillar. | Red vignette bleeds in at screen edges. | -Popularity, +Uncle Suspicion |
| **The Spy in the Kitchen · 御厨的秘密** | The royal chef is revealed to be a foreign agent who has been embedded for ten years. | Reinforces the "trust no one" atmosphere. | -Military, -Church |
| **The Broken String · 断弦的竖琴** | A bard's string snaps mid-performance — considered a grave omen by the court. | Discordant sound cue; resource values shift randomly ±5. | Random fluctuation |
| **Midnight Footsteps · 午夜的脚步** | Strange sounds echo through the corridor at night. Stress accumulates. | Eerie ambient audio. | +Uncle Suspicion +10 next round's AI tone becomes more hostile |

---

### Category D · D类：突发危机 (Wildcards)

| Event · 事件 | Description · 剧情 | Core Dilemma · 核心两难 | Resource Impact · 资源影响 |
|---|---|---|---|
| **The Great Fire · 大火灾** | The slum district is burning. | Save them: -Treasury, +Popularity / Ignore: -Popularity | Military vs Popularity |
| **The Foreign Envoy · 外国使节** | A neighboring kingdom proposes a marriage alliance — actually a veiled annexation attempt. | Accept: +Military, -Popularity / Refuse: +Popularity, -Military | Diplomacy vs People |
| **The Assassin · 刺客来袭** | You are attacked in the corridor. If Military < 30, you are wounded — losing one future round of actions. | Survive or suffer consequences based on Military level. | -Military (if low) |
| **The Blizzard · 突然的暴雪** | Supply lines are cut. Soldiers and civilians begin fighting over food. | Feed the army: -Popularity / Feed the people: -Military | Military vs Popularity |
| **The Rumour · 流言蜚语** | Word spreads that you are not the true heir — perhaps the Regent's illegitimate son. | Cannot be directly disproved. | -Popularity, +Uncle Suspicion |
| **The Plague · 瘟疫爆发** | Disease spreads from the lower city. | Quarantine: -Popularity / Ignore: -Military (soldiers fall ill) | Harshest resource test |
| **The Blind Oracle · 神秘的占卜师** | A blind elder predicts you will not survive the next full moon. | Delivers one hint — true or false — about an upcoming ending condition. | Psychological only |
| **The Shattered Mirror · 破碎的镜子** | Your mirror cracks during your morning routine. | Next round's AI responses are noticeably colder and more hostile. | Tone shift only |

---

## 3. The Regent's Coup Mechanics · 舅舅的政变机制

### Hidden Value: Uncle's Suspicion · 隐藏数值：疑心值

Initial value: **0**. The player never sees this number directly — only its effects.

初始值：**0**。玩家永远看不到这个数字，只能感受到它的影响。

---

### Suspicion Triggers · 疑心值增加条件

| Action · 行为 | Suspicion Gained · 疑心值增加 |
|---|---|
| Directly contradicting the Regent's advice twice in a row · 连续两次反驳舅舅的建议 | +20 |
| Caught exchanging intelligence with the Princess · 被发现与公主交换情报 | +30 |
| Attempting to win over the General or Priest · 试图拉拢将军或教士 | +15 |
| Any resource reaches the danger zone (0–20 or 80–100) · 任意资源进入危险区间 | +10 |
| Responding with arrogance to the Regent's compliment · 对舅舅的赞美表现傲慢 | +15 |

---

### Suspicion Thresholds · 疑心值节点

#### Suspicion > 50 · 疑心值超过 50：Voice Suppression · 舅舅盖声

The player types their response — but the Regent's voice overrides it.

玩家正常输入，但舅舅的声音将其盖过。

> *"His Majesty seems fatigued. Allow me to respond on his behalf."*  
> *"陛下似乎有些疲惫，就让本王代为回复吧。"*

The resource values shift according to the Regent's will. The player has no control this round. This mechanic triggers probabilistically when Suspicion is between 50–80 — not every round.

数值按舅舅的意志变化，玩家本轮完全失控。此机制在疑心值 50–80 之间概率性触发，并非每轮必发。

---

#### Suspicion > 80 · 疑心值超过 80：Input Lock · 输入框锁死

The text input field is disabled. The Regent speaks directly to the player.

输入框变灰锁住，舅舅主动向玩家发出文本。

> *"You have nothing left to say, child. The game is over."*  
> *"孩子，你已经没有什么可说的了。游戏结束了。"*

This triggers the forced ending sequence.

强制触发结局流程。

---

### Hard Death Conditions · 硬性死亡条件

| Condition · 条件 | Ending · 结局 |
|---|---|
| Any resource ≤ 0 · 任意资源归零 | Regent removes you "for the protection of the realm" · 舅舅以"保护王国"为由罢黜你 |
| Any resource ≥ 100 · 任意资源满值 | Regent has you assassinated as a threat · 舅舅以"暴君/精神失常"为由暗杀你 |

---

## 4. Victory & Ending Conditions · 胜利与结局条件

### Safe Zone · 安全区间

All four resources must remain between **20 and 80** to avoid hard death triggers.

四项资源必须保持在 **20 至 80** 之间，以避免触发硬性死亡。

---

### Endings · 结局列表

| Ending · 结局 | Trigger · 触发条件 |
|---|---|
| **Resource Death · 资源死亡** | Any resource hits 0 or 100 before Round 12 · 任意资源在第 12 轮前到达极值 |
| **Regent's Coup · 舅舅政变** | Uncle Suspicion > 80, input locked, forced ending · 疑心值超过 80，输入框锁死，强制结局 |
| **Abdication · 退位** | Player accepts exile in Round 12 · 第 12 轮玩家接受流放 |
| **Civil War · 内战** | Player refuses abdication, triggering an uprising · 玩家拒绝退位，引发内战 |
| **Survival · 生存** | Complete all 12 rounds with all resources in 20–80 and Suspicion < 50 · 完成 12 轮，资源全在 20–80，疑心值低于 50 |

---

## 5. Implementation Notes · 技术实现备注

- All NPC Affinity values, Uncle Suspicion, and resource states are managed by a central `GameStateManager`.
- Random events use a `WeightedEventPool` — each round's event is selected based on current game state before the NPC enters.
- Voice Suppression and Input Lock are UI states controlled by `UIManager`, triggered by `GameStateManager` when Suspicion thresholds are crossed.
- The Princess's Golden Hand ability is a one-time flag per save — `princessSaved: bool`.

- 所有 NPC 好感度、疑心值和资源数值由中央 `GameStateManager` 统一管理。
- 随机事件使用 `WeightedEventPool`——每轮 NPC 登场前，根据当前游戏状态选取事件。
- 盖声机制和输入框锁死是由 `UIManager` 控制的 UI 状态，在 `GameStateManager` 检测到疑心值节点时触发。
- 公主金手指为每局一次性标记：`princessSaved: bool`。
