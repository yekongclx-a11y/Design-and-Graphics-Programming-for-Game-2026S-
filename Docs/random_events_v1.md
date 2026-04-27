# Random Events System Design · 随机事件系统设计文档
## Crown: The Gilded Cage · 王权

**Version:** 1.0  
**Status:** Design Complete, Implementation Pending

---

## 一、系统概述 · System Overview

随机事件系统在12轮主线对话中随机插入2-3次特殊事件，打破固定节奏，增加游戏深度和不可预测性。

**核心设计原则：**
- 事件不重复：同一局内每个事件最多触发一次
- 权重触发：根据当前游戏状态决定触发哪个事件
- 影响好感度：事件选择同时影响资源和NPC好感度
- 重要事件三选项：大事件提供三个选择，普通事件二选一

---

## 二、触发机制 · Trigger System

### 触发时机
每轮NPC登场**之前**检查一次，决定是否插入随机事件。

### 触发概率
- 第1-3轮：不触发（蜜月期，让玩家熟悉基础玩法）
- 第4-8轮：每轮25%概率触发
- 第9-12轮：每轮15%概率触发（主线剧情紧张，减少干扰）
- 全局上限：一局最多触发3次

### 权重选择逻辑
```
触发事件时：
1. 从未触发过的事件池中筛选
2. 若任意资源在危险区间（0-20或80-100）→ 权重×3触发对应危机事件
3. 若任意NPC好感度在极值（≤30或≥80）→ 权重×2触发对应好感度事件
4. 否则从全部可用事件中随机抽取
```

---

## 三、事件呈现方式 · Presentation

### A类：有人物登场的事件
- 显示对应NPC立绘
- 显示事件描述文字
- 显示选项按钮（2-3个）
- 选择后直接更新资源和好感度，不调用AI

### B类：纯氛围事件
- 黑屏或背景图
- 显示事件描述文字
- 只有Continue按钮
- 触发后直接更新资源

---

## 四、事件清单 · Event List

### Category A · NPC好感度关联事件

#### A1. 秘密密令 · Secret Orders
- **触发条件：** General Affinity > 70
- **立绘：** `npc_general`
- **描述：** The General sends a trusted aide with a private letter, inviting a secret meeting in the barracks after dark.
- **选项：**
  - [Accept] 接受会面 → Military +15, Suspicion +20, General Affinity +10
  - [Decline] 拒绝 → Military -5, General Affinity -10

#### A2. 贪污的把柄 · Evidence of Corruption
- **触发条件：** Minister Affinity < 30
- **立绘：** `npc_minister_1`
- **描述：** You uncover proof that the Minister has been selling military grain. The evidence is damning.
- **选项：**
  - [Expose] 公开揭发 → Popularity +15, Treasury -10, Minister Affinity -20
  - [Blackmail] 以此要挟 → Treasury +15, Minister Affinity -10, Suspicion +10
  - [Ignore] 装作不知 → Suspicion -5, Minister Affinity +5

#### A3. 深夜告解 · Midnight Confession
- **触发条件：** Priest Affinity > 70
- **立绘：** `npc_bishop`
- **描述：** The Bishop holds a private blessing ceremony at midnight, hinting the Church can help conceal your private finances.
- **选项：**
  - [Accept] 接受提议 → Church +15, Popularity -10, Treasury +10
  - [Decline] 婉拒 → Church -5, Popularity +5

#### A4. 带血的地图 · The Bloodstained Map
- **触发条件：** Princess Affinity > 70
- **立绘：** `npc_princess`
- **描述：** She bribes a guard to smuggle you a map of the palace's secret passages. The ink is still fresh.
- **选项：**
  - [Accept] 收下地图 → Suspicion +15（被舅舅察觉）, Princess Affinity +10
  - [Burn it] 烧掉地图 → Suspicion -5, Princess Affinity -5

#### A5. 绝望的侍女 · The Desperate Handmaid
- **触发条件：** Commoner Affinity < 30
- **立绘：** `npc_handmaid`
- **描述：** Your handmaid, whose family perished under your policies, slips a sedative into your wine. She is caught before you drink.
- **选项：**
  - [Forgive] 宽恕她 → Popularity +20, Military -5
  - [Punish] 惩处她 → Popularity -15, Military +5, Church -5

#### A6. 叛逃的骑士 · The Deserting Knight
- **触发条件：** Military Affinity < 30
- **立绘：** `npc_knight_deserter`
- **描述：** A knight who once served your father lays down his sword at your feet and refuses to serve further.
- **选项：**
  - [Let him go] 放他离开 → Military -10, Popularity +5
  - [Detain him] 扣押他 → Military -5, Popularity -10, Suspicion +5

---

### Category B · 舅舅权力阴影

#### B1. 权力的真空 · Power Vacuum
- **触发条件：** 任意轮次，权重正常
- **立绘：** 无（纯文字）
- **描述：** The Regent falls gravely ill and is absent from court for three days. The throne room feels different without his shadow.
- **选项：**
  - [Act boldly] 趁机大胆行事 → 本轮可额外与NPC多交互1次, Suspicion +15
  - [Stay cautious] 保持低调 → Suspicion -10

#### B2. 致命的赞美 · The Deadly Compliment
- **触发条件：** Suspicion > 30
- **立绘：** `npc_regent`
- **描述：** The Regent publicly praises your wisdom and readiness to govern alone — in front of the entire court. Everyone is watching your reaction.
- **选项：**
  - [Accept humbly] 谦虚接受 → Suspicion -10, Popularity +5
  - [Deflect proudly] 傲慢回应 → Suspicion +20, Popularity +10

#### B3. 舅舅的礼物 · The Regent's Gift
- **触发条件：** 任意轮次
- **立绘：** `npc_regent`
- **描述：** The Regent sends you a songbird with its tongue cut out. A servant delivers it with a smile and no explanation.
- **选项：**
  - [Send thanks] 回信致谢（忍下来）→ Suspicion +5, Popularity -5
  - [Return it] 原物奉还 → Suspicion +20, Popularity +10
  - [Say nothing] 沉默不语 → Suspicion +10

#### B4. 空荡的座椅 · The Empty Throne
- **触发条件：** 任意轮次，权重正常
- **立绘：** 无（纯氛围）
- **描述：** The Regent skips today's session. His chair sits empty at the head of the council table. No one mentions it. No one looks at it directly.
- **效果：** Suspicion +10（环境影响，无选择）

---

### Category C · 氛围与偏执

#### C1. 墙上的血字 · Blood on the Walls
- **触发条件：** 任意轮次
- **立绘：** 无（纯氛围）
- **描述：** The words "FALSE KING MUST DIE" appear carved into a throne hall pillar. No one claims responsibility.
- **效果：** Popularity -10, Suspicion +10（无选择，屏幕边缘红色暗角）

#### C2. 御厨的秘密 · The Spy in the Kitchen
- **触发条件：** 任意轮次
- **立绘：** `npc_servant_male`
- **描述：** The royal chef is revealed to be a foreign agent who has been embedded for ten years. He is arrested before he can flee.
- **选项：**
  - [Execute] 处决他 → Military +5, Church -10, Popularity -5
  - [Interrogate] 审讯获取情报 → 下一轮Suspicion增长减半

#### C3. 断弦的竖琴 · The Broken String
- **触发条件：** 任意轮次
- **立绘：** 无（纯氛围）
- **描述：** A bard's string snaps mid-performance during a court gathering — considered a grave omen. The court falls silent.
- **效果：** 资源随机波动±5（无选择）

#### C4. 午夜的脚步 · Midnight Footsteps
- **触发条件：** 任意轮次
- **立绘：** 无（纯氛围）
- **描述：** Strange footsteps echo through the corridor outside your chambers at night. By morning, nothing is found.
- **效果：** Suspicion +10，下一轮AI回复语气更具敌意（无选择）

---

### Category D · 突发危机

#### D1. 大火灾 · The Great Fire
- **触发条件：** 任意轮次
- **立绘：** 无
- **描述：** The slum district near the palace is burning. Thousands are displaced. The court awaits your decision.
- **选项：**
  - [Send aid] 派兵救援 → Popularity +20, Treasury -15, Military -5
  - [Ignore it] 置之不理 → Popularity -20, Church -10
  - [Blame rebels] 嫁祸叛军 → Popularity -10, Military +10, Suspicion +10

#### D2. 外国使节 · The Foreign Envoy
- **触发条件：** 任意轮次
- **立绘：** `npc_noble_female`
- **描述：** A neighboring kingdom proposes a marriage alliance — actually a veiled annexation attempt. The envoy is charming and persistent.
- **选项：**
  - [Accept] 接受联姻 → Military +15, Popularity -10, Suspicion +15
  - [Refuse politely] 礼貌拒绝 → Popularity +10, Military -5
  - [Stall for time] 拖延周旋 → Suspicion +5

#### D3. 刺客来袭 · The Assassin
- **触发条件：** Military < 40
- **立绘：** `npc_assassin`
- **描述：** You are attacked in the corridor by a hooded figure. Your guards intervene, but not before the assassin delivers a message.
- **选项：**
  - [Investigate publicly] 公开调查 → Popularity +10, Suspicion +15
  - [Handle quietly] 秘密处理 → Suspicion +5, Military +5
- **特殊：** Military < 20时自动触发受伤效果，下一轮资源变化幅度减半

#### D4. 突然的暴雪 · The Blizzard
- **触发条件：** 任意轮次
- **立绘：** 无
- **描述：** Supply lines are cut by an unexpected blizzard. Both soldiers and civilians begin fighting over the remaining food stores.
- **选项：**
  - [Feed the army] 优先军队 → Military +10, Popularity -15
  - [Feed the people] 优先百姓 → Popularity +15, Military -10

#### D5. 流言蜚语 · The Rumour
- **触发条件：** 任意轮次
- **立绘：** 无（纯氛围）
- **描述：** Word spreads through the court that you are not the true heir — perhaps the Regent's illegitimate son, placed conveniently on the throne.
- **效果：** Popularity -15, Suspicion +15（无选择，无法直接反驳）

#### D6. 瘟疫爆发 · The Plague
- **触发条件：** 任意轮次，权重正常
- **立绘：** 无
- **描述：** Disease spreads from the lower city. The court physicians disagree on the cause. The people are afraid.
- **选项：**
  - [Quarantine] 封城隔离 → Popularity -20, Church +10, Military -5
  - [Ignore] 顺其自然 → Popularity -10, Military -10（士兵染病）
  - [Burn the district] 焚烧病区 → Popularity -25, Military +5, Church -20

#### D7. 神秘的占卜师 · The Blind Oracle
- **触发条件：** 任意轮次
- **立绘：** `npc_oracle`
- **描述：** A blind elder is brought before you, claiming to have foreseen your fate. The court watches nervously.
- **选项：**
  - [Listen] 倾听预言 → 获得一条关于当前最危险资源的提示
  - [Dismiss] 驱逐他 → Popularity -5, Church -10

#### D8. 军饷拖欠 · Unpaid Wages
- **触发条件：** Treasury < 30
- **立绘：** 无
- **描述：** Soldiers stationed outside the city threaten to mutiny if they are not paid within the day. The treasury is nearly empty.
- **选项：**
  - [Pay immediately] 立即筹钱 → Treasury -20, Military +15
  - [Promise later] 承诺延后发放 → Military -10, Suspicion +10

---

## 五、系统参数 · System Parameters

```csharp
// 触发控制
int maxEventsPerGame = 3;        // 每局最多触发次数
int eventsTriggered = 0;         // 已触发次数
int minRoundToTrigger = 4;       // 最早触发轮次
float earlyRoundChance = 0.25f;  // 第4-8轮触发概率
float lateRoundChance = 0.15f;   // 第9-12轮触发概率

// 权重倍率
float dangerZoneWeight = 3f;     // 资源危险区间权重倍率
float affinityWeight = 2f;       // 好感度极值权重倍率

// 已触发事件记录
HashSet<string> triggeredEvents = new HashSet<string>();
```

---

## 六、NPC好感度更新规则 · Affinity Update Rules

随机事件选择后，除资源变化外，同步更新对应NPC好感度：

| 事件 | 好感度影响 |
|------|-----------|
| 秘密密令 - 接受 | General Affinity +10 |
| 秘密密令 - 拒绝 | General Affinity -10 |
| 贪污的把柄 - 揭发 | Minister Affinity -20 |
| 贪污的把柄 - 要挟 | Minister Affinity -10 |
| 贪污的把柄 - 忽视 | Minister Affinity +5 |
| 深夜告解 - 接受 | Bishop Affinity +15 |
| 深夜告解 - 拒绝 | Bishop Affinity -5 |
| 带血的地图 - 收下 | Princess Affinity +10 |
| 带血的地图 - 烧掉 | Princess Affinity -5 |
| 绝望的侍女 - 宽恕 | Commoner Affinity +15 |
| 绝望的侍女 - 惩处 | Commoner Affinity -10 |
| 叛逃的骑士 - 放行 | General Affinity +5 |
| 叛逃的骑士 - 扣押 | General Affinity -15 |

---

## 七、技术实现 · Implementation Notes

**EventData结构：**
```csharp
[System.Serializable]
public class EventData
{
    public string eventId;
    public string title;
    public string description;
    public string npcPortraitName;  // 对应立绘名，空字符串=无立绘
    public EventChoice[] choices;
    public bool hasChoices;         // false=纯氛围事件，自动触发效果
}

[System.Serializable]
public class EventChoice
{
    public string buttonText;
    public int goldChange;
    public int popularityChange;
    public int churchChange;
    public int militaryChange;
    public int suspicionChange;
    public int affinityChange;      // 对应NPC好感度变化
    public string affinityTarget;   // "minister"/"general"/"bishop"/"princess"/"commoner"
}
```

**触发流程：**
```
每轮开始前：
1. 检查eventsTriggered < maxEventsPerGame
2. 检查currentRound >= minRoundToTrigger
3. 根据轮次决定触发概率
4. 概率检查通过 → 从未触发事件池中按权重选取
5. 触发事件 → 显示EventPanel → 等待玩家选择
6. 处理选择结果（资源+好感度）
7. 关闭EventPanel → 正常开始当前轮NPC觐见
```
