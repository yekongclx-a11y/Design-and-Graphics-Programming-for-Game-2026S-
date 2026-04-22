# Prompt Template Documentation · Prompt 模板说明文档
## Crown: The Gilded Cage · 王权

**Version:** 1.1  
**Status:** Active  
**Updated:** 2026 — 加入triggerEvent系统和三回合机制  
**Language:** English (AI) / 中文 (Notes)

---

## 概述 · Overview

本文档解释 `prompt_v1.txt` 的每个模块的作用和设计理由。  
This document explains the purpose and design rationale of each module in `prompt_v1.txt`.

Prompt 由四个模块拼接而成，前两个固定不变，后两个每轮动态替换：

```
[SYSTEM INSTRUCTIONS]  ← 强调AI必须遵守规则
[WORLD]                ← 固定：世界观与氛围
[RULES]                ← 固定：游戏规则与输出格式
[CHARACTER]            ← 动态：当前登场的 NPC 角色卡
[STATE]                ← 动态：当前游戏数值状态
```

---

## Module 1 · [SYSTEM INSTRUCTIONS] 强调模块

**作用：** 告诉 AI 这是一套必须严格遵守的指令，不是普通对话。  
**Purpose:** Anchors the AI in its role as a rule-bound narrative engine, not a chatbot.

放在最顶部，因为AI读取时会优先处理开头的内容。

---

## Module 2 · [WORLD] 世界观模块

**作用：** 告诉 AI 它存在于一个什么样的世界，建立氛围基调。  
**Purpose:** Establishes the world, tone, and atmosphere for the AI.

**设计原则：**
- 用感官细节描述世界，而不是干巴巴的设定说明
- 明确告诉 AI 玩家的身份和处境
- 建立"窒息感"基调，让 AI 的所有回复都带着这种压力

---

## Module 3 · [RULES] 规则模块

**作用：** 告诉 AI 必须做什么、不能做什么，以及输出格式。  
**Purpose:** Defines AI behavior constraints and mandatory output format.

### 输出格式 · Output Format

```json
{
  "action": "NPC的动作描写，斜体显示给玩家，1句话",
  "dialogue": "NPC说的话，1-3句",
  "gold": -10,
  "popularity": 5,
  "church": 0,
  "military": -5,
  "suspicion": 10,
  "triggerEvent": "none"
}
```

### 字段说明 · Field Reference

| 字段 | 类型 | 说明 |
|------|------|------|
| `action` | string | NPC动作描写，第三人称过去时，1句 |
| `dialogue` | string | NPC台词，1-3句，有个性，有文学性 |
| `gold` | int | 金库变化，**硬性限制 -20 到 +20** |
| `popularity` | int | 民心变化，**硬性限制 -20 到 +20** |
| `church` | int | 教会变化，**硬性限制 -20 到 +20** |
| `military` | int | 军队变化，**硬性限制 -20 到 +20** |
| `suspicion` | int | 疑心值变化，**硬性限制 0 到 +20** |
| `triggerEvent` | string | 事件触发码，见下方说明 |

### 数值规则 · Value Rules

**硬性限制**：所有数值变化必须在 -20 到 +20 之间，绝对不能超出。比例参考：
- 温和回应：±3~5
- 明确回应：±8~12
- 强烈回应：±15~20
- 超出±20：**严重违规，破坏游戏平衡**

---

## Module 4 · triggerEvent 系统

**v1.1新增**

**作用：** 让AI自主判断玩家输入是否触发特殊游戏事件，实现真正的自由度。  
**Purpose:** Enables AI-driven detection of player actions that break the normal flow.

### 事件码说明 · Event Codes

| 事件码 | 触发条件 | 游戏后果 |
|--------|----------|----------|
| `none` | 正常政治对话 | 继续当前回合 |
| `end_round` | 对话自然结束，或玩家挥手退下 | NPC离场，进入下一轮 |
| `coup_attempt` | 玩家试图逮捕舅舅、夺权、或任何直接挑战权力结构的行为 | 触发特殊政治危机剧情 |
| `game_over` | 玩家做出不可挽回的事——退位、当众羞辱所有势力、做出任何国王无法存活的宣言 | 直接触发结局 |
| `uncle_intervene` | 玩家反复废话、拖延、软弱无力，摄政王会自然填补权力真空 | 触发舅舅盖声机制 |

### AI判断框架 · AI Judgment Framework

AI使用自主判断，不依赖穷举规则。判断标准：

> "一个真实的中世纪宫廷会认为这是正常的吗？"  
> "Would a real medieval court consider this normal?"

如果不正常——升级triggerEvent。AI是裁判，不是执行者。

---

## Module 5 · [CHARACTER] 角色卡模块

**作用：** 告诉 AI 这一轮它在扮演谁。  
**Purpose:** Injects the current NPC's identity, motivation, and voice.

### 角色卡结构

```
- Identity: 身份和外貌
- Personality: 性格特征
- Surface Request: 表面诉求（玩家看到的）
- Hidden Motive: 真实目的（AI知道但不能直说）
- Speaking Style: 说话风格
- Example Lines: 2-3句示例台词（Few-shot锁定语感）
```

### 特殊NPC规则

**公主：** 角色卡里有额外指令——必须在对话中夹带一条真实情报，但要包装成日常对话。最多2回合。

**摄政王（舅舅）：** 必须在每次回复里评估疑心值变化。第12轮登场时为终局模式，不接受退下指令。

---

## Module 6 · [STATE] 游戏状态模块

**作用：** 告诉 AI 当前游戏的实时状态。  
**Purpose:** Provides current game state so AI responses reflect the actual situation.

### 动态字段

```
Current Round: {currentRound}/12
Current Turn in Round: {currentTurn}/{maxTurns}
Treasury: {gold}/100
Popularity: {popularity}/100
Church: {church}/100
Military: {military}/100
Uncle Suspicion: {suspicion}/100
Current NPC: {currentNPC}
Surface Request: {surfaceRequest}
Hidden Motive: {hiddenMotive}
Player's response: {playerInput}
```

**新增字段：**
- `{currentTurn}` — 当前回合内的第几次交互（1/2/3）
- `{maxTurns}` — 该NPC的最大交互次数（普通NPC=3，公主=2）

当 `currentTurn == maxTurns` 时，AI知道这是最后一次交互，必须生成有画面感的离场台词。

---

## 三回合法则 · The 3-Turn Rule

**v1.1新增**

每轮NPC最多交互3次（公主2次，特殊情况除外）。

| 回合 | 剧情节奏 | AI行为 |
|------|----------|--------|
| 第1回合 | 试探 | NPC抛出表层诉求 |
| 第2回合 | 博弈 | NPC暴露部分隐藏动机或施压 |
| 第3回合 | 最后通牒 | NPC逼宫，生成有画面感的离场台词 |

第3回合结束后输入框自动锁死，AI强制结算并触发`end_round`。

---

## 调试指南 · Debugging Guide

**AI返回JSON格式不对：**
加一句：*"You MUST return ONLY the JSON object. No extra text, no markdown, no explanation."*

**数值超出±20：**
检查RULES里的HARD LIMIT描述是否够强硬，可以加粗或重复强调。

**triggerEvent判断不准：**
在[STATE]里加更多上下文，帮助AI理解当前局势的严重性。

**AI角色跑偏：**
在对应NPC的Example Lines里加更多例句，3-5句比2-3句效果好。

---

## 版本记录 · Version History

| 版本 | 日期 | 变更 |
|------|------|------|
| v1.0 | 2026 | 初始版本，6个NPC角色卡 |
| v1.1 | 2026 | 加入triggerEvent系统、三回合法则、AI自主判断框架、currentTurn字段 |
