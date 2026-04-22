# Game Mechanics Design Document · 游戏机制设计文档
## Crown: The Gilded Cage · 王权

**Version:** 1.1  
**Updated:** 2026 — 加入三回合法则、triggerEvent系统、特殊NPC规则

---

## 1. NPC Affinity System · NPC 好感度系统

### Overview · 概述

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
| **Loyal (≥80)** | **Saintly Coronation** — The Church elevates you to near-divine status. Popularity cannot drop below 0 for the rest of the game. | **圣徒加冕** — 教会在民间为你造神。本局游戏中民心不会降至 0。 |

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

## 2. The 3-Turn Rule · 三回合法则

**v1.1新增**

每轮NPC觐见，玩家最多进行3次交互（公主为2次）。

### 回合节奏 · Turn Rhythm

| 回合 | 剧情节奏 | NPC行为 |
|------|----------|---------|
| Turn 1 · 试探 | NPC抛出表层诉求，观察国王反应 | 温和，试探性 |
| Turn 2 · 博弈 | 根据玩家态度，暴露部分隐藏动机或施压 | 施压，利益诱惑或道德绑架 |
| Turn 3 · 最后通牒 | 逼宫，给出最终判决式台词后离场 | 强硬，有画面感的离场台词 |

### 结束触发方式 · End Triggers

**方式一：回合耗尽**
第3次发送后输入框自动锁死，AI强制结算并生成离场台词。

**方式二：玩家主动退下**
输入框旁加`[退下]`按钮。系统发送隐藏指令：
> *"The King says nothing and waves his hand in dismissal. Give your parting action, a final line, and settle the values."*

NPC会因受辱产生额外惩罚（民心下降，或疑心值上升）。可在第1或第2回合触发。

**方式三：triggerEvent自动触发**
AI判断玩家输入触发特殊事件，自动结束当前回合并进入对应处理流程。

### 惩罚机制 · Penalty for Indecision

第3回合结束后，若玩家一直给出模棱两可的废话，直接触发舅舅盖声：
- 舅舅代为回答，数值按最恶劣情况扣除
- 疑心值 +20（他发现你是个可以加速推倒的废物）

---

## 3. triggerEvent System · 事件触发系统

**v1.1新增**

AI在每次回复里自主判断是否触发特殊事件，无需穷举规则。

### 事件码 · Event Codes

| 事件码 | 触发条件 | 游戏处理 |
|--------|----------|----------|
| `none` | 正常政治对话 | 继续当前回合，等待下一次输入 |
| `end_round` | 对话自然结束，或玩家退下 | NPC离场，推进到下一轮NPC |
| `coup_attempt` | 玩家试图逮捕舅舅、夺权、或直接挑战权力结构 | 触发特殊政治危机，疑心值大幅上升 |
| `game_over` | 玩家退位、当众羞辱所有势力、做出任何国王无法存活的宣言 | 直接触发对应结局 |
| `uncle_intervene` | 玩家反复废话、软弱无力 | 触发舅舅盖声，玩家失去本轮决策权 |

### AI判断原则

AI使用自主判断框架：

> "Would a real medieval court consider this normal?"  
> 一个真实的中世纪宫廷会认为这是正常的吗？

如果不正常——升级triggerEvent。

---

## 4. Special NPC Rules · 特殊NPC规则

**v1.1新增**

### 公主 · Princess（最多2回合）

公主送递的是致命情报，不是来闲聊的。

- 最大回合数：**2**（不是3）
- 如果玩家输入废话或时间过长，公主强制离场
- 离场台词暗示紧迫性：*"He is coming. Burn this."*
- 营造信息差紧张感，玩家必须快速理解情报

### 摄政王（第12轮）· Regent — Final Round

第12轮舅舅亲自登场，规则完全反转：

- **隐藏`[退下]`按钮** — 玩家无法让舅舅离场
- **输入框可能被覆盖** — 舅舅的文字盖过玩家输入
- **回合压缩** — 不给玩家太多思考时间
- **三个结局分支：**
  - 接受退位 → 流放结局
  - 拒绝并反抗 → 内战结局
  - 用语言周旋到底 → 隐藏生存结局

---

## 5. The Regent's Coup Mechanics · 舅舅的政变机制

### Hidden Value: Uncle's Suspicion · 隐藏数值：疑心值

初始值：**0**。玩家永远看不到这个数字，只能感受到它的影响。

### Suspicion Triggers · 疑心值增加条件

| 行为 | 疑心值增加 |
|------|-----------|
| 连续两次反驳舅舅的建议 | +20 |
| 被发现与公主交换情报 | +30 |
| 试图拉拢将军或教士 | +15 |
| 任意资源进入危险区间（0–20或80–100） | +10 |
| 对舅舅的赞美表现傲慢 | +15 |
| 触发coup_attempt事件 | +20 |
| 第3回合废话触发惩罚 | +20 |

### Suspicion Thresholds · 疑心值节点

#### Suspicion > 50：Voice Suppression · 舅舅盖声

玩家正常输入，但舅舅的声音将其盖过：

> *"His Majesty seems fatigued. Allow me to respond on his behalf."*

数值按舅舅的意志变化，玩家本轮完全失控。此机制在疑心值50–80之间概率性触发：概率 = (suspicion - 50) × 2%

#### Suspicion > 80：Input Lock · 输入框锁死

输入框变灰锁住，舅舅主动发出文本：

> *"You have nothing left to say, child. The game is over."*

强制触发结局流程。

### Hard Death Conditions · 硬性死亡条件

| 条件 | 结局 |
|------|------|
| 任意资源 ≤ 0 | 舅舅以"保护王国"为由罢黜你 |
| 任意资源 ≥ 100 | 舅舅以"暴君/精神失常"为由暗杀你 |

---

## 6. Random Events · 随机事件

### 触发规则

- 12轮主线中随机插入2–3次事件
- **权重随机系统**决定触发哪个事件：
  - 若某项资源处于危险区间（0–20或80–100），优先触发对应危机事件
  - 若某NPC好感度处于极值，优先触发好感度关联事件
  - 否则从全部事件池中随机抽取

### Category A · NPC好感度关联事件

| 事件 | 触发条件 | 剧情 | 资源影响 |
|------|----------|------|----------|
| 秘密密令 | General Affinity > 70 | 将军派亲信送来密信，邀你私下会面 | +Military, +Suspicion |
| 贪污的把柄 | Minister Affinity < 30 | 你抓到大臣倒卖军粮的证据，揭发还是要挟？ | 揭发:+Popularity,-Treasury / 要挟:+Treasury,-Minister Affinity |
| 深夜告解 | Priest Affinity > 70 | 教士为你举行深夜祈福，暗示教会可以帮你洗钱 | +Church, -Popularity |
| 带血的地图 | Princess Affinity > 70 | 她买通守卫送来标注皇宫暗道的地图 | +Survival, +Suspicion |
| 绝望的侍女 | Commoner Affinity < 30 | 你的贴身侍女在酒中掺入迷药 | -Popularity, 跳过下一轮 |
| 叛逃的骑士 | Military Affinity < 30 | 一名骑士决定解甲归田，不再效忠 | -Military |

### Category B · 舅舅权力阴影

| 事件 | 剧情 | 隐藏动机 | 资源影响 |
|------|------|----------|----------|
| 权力的真空 | 舅舅突发恶疾，暂离朝廷3天 | 测试你在没有监视时是否会露出獠牙 | 自由行动次数+1 |
| 致命的赞美 | 舅舅当众夸你聪慧过人 | 这是陷阱，看你是否表现傲慢 | +Popularity, +Suspicion（若玩家傲慢回应） |
| 空荡的座椅 | 舅舅缺席会议，主座空置 | 心理压迫 | +Suspicion（环境影响） |
| 舅舅的礼物 | 舅舅送来一只被拔了舌头的夜莺 | 直接威胁 | +Suspicion +15, -Popularity |

### Category C · 氛围与偏执

| 事件 | 剧情 | 资源影响 |
|------|------|----------|
| 墙上的血字 | 立柱上出现"伪王必死"涂鸦，屏幕边缘红色暗角 | -Popularity, +Suspicion |
| 御厨的秘密 | 御厨被发现是邻国细作，潜伏十年 | -Military, -Church |
| 断弦的竖琴 | 游吟诗人演奏时琴弦崩断，视为不祥之兆 | 随机波动±5 |
| 午夜的脚步 | 走廊传来奇怪脚步声，下一轮AI回复更具敌意 | +Suspicion +10 |

### Category D · 突发危机

| 事件 | 剧情 | 核心两难 | 资源影响 |
|------|------|----------|----------|
| 大火灾 | 贫民窟着火 | 救（-Treasury,+Popularity）还是不救（-Popularity） | 军队vs民心 |
| 外国使节 | 邻国提议联姻，实为吞并 | 答应（+Military,-Popularity）还是拒绝 | 外交vs人心 |
| 刺客来袭 | 走廊被刺，Military<30则受伤，减少后续回合 | 根据军队数值决定后果 | -Military（若过低） |
| 突然的暴雪 | 补给线断绝，士兵和民众抢夺物资 | 喂军队（-Popularity）还是喂百姓（-Military） | 最严峻资源考验 |
| 流言蜚语 | 传言你不是亲生的 | 无法直接反驳 | -Popularity, +Suspicion |
| 瘟疫爆发 | 疾病从下城区蔓延 | 封城（-Popularity）还是顺其自然（-Military） | 最残酷测试 |
| 神秘的占卜师 | 瞎眼老头预测你活不过下次月圆 | 提供一条真实或虚假的结局提示 | 仅心理影响 |
| 破碎的镜子 | 洗漱时镜子碎裂 | 下一轮AI回复明显更冷漠敌意 | 仅语气影响 |

---

## 7. Victory & Ending Conditions · 胜利与结局条件

### Safe Zone · 安全区间

四项资源必须保持在 **20 至 80** 之间，以避免触发硬性死亡。

### Endings · 结局列表

| 结局 | 触发条件 |
|------|----------|
| **Resource Death · 资源死亡** | 任意资源在第12轮前到达极值 |
| **Regent's Coup · 舅舅政变** | 疑心值超过80，输入框锁死，强制结局 |
| **Abdication · 退位** | 第12轮玩家接受流放 |
| **Civil War · 内战** | 玩家拒绝退位，引发内战 |
| **Survival · 生存** | 完成12轮，资源全在20–80，疑心值低于50 |
| **game_over触发** | 玩家在任意轮次做出不可挽回的行为 |

---

## 8. Implementation Notes · 技术实现备注

**新增字段（v1.1）：**
- `NPCData`里加`maxTurns`字段（普通NPC=3，公主=2）
- `DialogueSystem`里加`currentTurnInRound`变量，每次发送后+1
- `GameStateManager`的`[STATE]`传入`{currentTurn}`和`{maxTurns}`
- `UIManager`加`ShowDismissButton(bool)`方法控制退下按钮显示/隐藏
- `triggerEvent`处理优先级：先检查事件码，再检查回合数

**triggerEvent处理逻辑：**
```
收到AI回复后：
1. 检查triggerEvent值
2. 若为coup_attempt → 触发危机剧情，疑心值+20
3. 若为game_over → 直接调用EndingManager
4. 若为uncle_intervene → 触发ShowUncleOverride
5. 若为end_round → 调用NextRound()
6. 若为none且currentTurn >= maxTurns → 强制end_round
7. 若为none且currentTurn < maxTurns → 解锁输入框，等待下一次输入
```

---

## 版本记录 · Version History

| 版本 | 日期 | 变更 |
|------|------|------|
| v1.0 | 2026 | 初始版本 |
| v1.1 | 2026 | 加入三回合法则、triggerEvent系统、特殊NPC规则、退下按钮机制、惩罚机制 |
