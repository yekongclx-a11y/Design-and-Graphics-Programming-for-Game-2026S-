# Prompt Template Documentation · Prompt 模板说明文档
## Crown: The Gilded Cage · 王权

**Version:** 1.0  
**Status:** Active  
**Language:** English (AI) / 中文 (Notes)

---

## 概述 · Overview

本文档解释 `prompt_v1.txt` 的每个模块的作用和设计理由。  
This document explains the purpose and design rationale of each module in `prompt_v1.txt`.

Prompt 由四个模块拼接而成，前两个固定不变，后两个每轮动态替换：

```
[WORLD]      ← 固定：世界观与氛围
[RULES]      ← 固定：游戏规则与输出格式
[CHARACTER]  ← 动态：当前登场的 NPC 角色卡
[STATE]      ← 动态：当前游戏数值状态
```

---

## Module 1 · [WORLD] 世界观模块

**作用：** 告诉 AI 它存在于一个什么样的世界，建立氛围基调。  
**Purpose:** Establishes the world, tone, and atmosphere for the AI.

**设计原则：**
- 用感官细节描述世界，而不是干巴巴的设定说明
- 明确告诉 AI 玩家的身份和处境
- 建立"窒息感"基调，让 AI 的所有回复都带着这种压力

**关键句子解析：**

> *"You are the narrative engine of a dark medieval court survival game."*

告诉 AI 它的身份——不是聊天机器人，是叙事引擎。这句话锚定了它的职责。

> *"The young King is the most powerless person in the palace."*

确立玩家的无力感，AI 扮演的所有 NPC 都应该从这个前提出发。

---

## Module 2 · [RULES] 规则模块

**作用：** 告诉 AI 必须做什么、不能做什么，以及输出格式。  
**Purpose:** Defines AI behavior constraints and mandatory output format.

**设计原则：**
- 规则要绝对清晰，不留模糊空间
- 输出格式用示例说明，比文字描述更有效
- 明确数值范围，防止 AI 返回超出范围的数字

**为什么用 JSON：**
代码需要稳定解析 AI 的输出。JSON 是最可靠的结构化格式，任何编程语言都能解析。

**字段说明：**

| 字段 | 类型 | 说明 |
|------|------|------|
| `action` | string | NPC 的动作描写，斜体显示给玩家，1句话 |
| `dialogue` | string | NPC 说的话，正常字体显示，1-3句 |
| `gold` | int | 金库变化，范围 -20 到 +20 |
| `popularity` | int | 民心变化，范围 -20 到 +20 |
| `church` | int | 教会变化，范围 -20 到 +20 |
| `military` | int | 军队变化，范围 -20 到 +20 |
| `suspicion` | int | 舅舅疑心值变化，范围 0 到 +30 |

**为什么限制单次变化在 -20 到 +20：**
防止玩家一句话就把某项资源打到极值，保证游戏有足够的呼吸空间。

---

## Module 3 · [CHARACTER] 角色卡模块

**作用：** 告诉 AI 这一轮它在扮演谁，以及这个角色的一切。  
**Purpose:** Injects the current NPC's identity, motivation, and voice.

**设计原则：**
- 每个 NPC 有独立的角色卡
- 包含表面诉求和隐藏动机——AI 知道真相，但 NPC 不会说破
- 用示例台词（Few-shot examples）锁定语气，比描述更有效

**角色卡结构：**

```
- Identity: 身份和外貌
- Personality: 性格特征
- Surface Request: 这轮来访的表面诉求
- Hidden Motive: 真实目的，AI知道但不能直说
- Speaking Style: 说话风格描述
- Example Lines: 2-3句示例台词，锁定语感
```

**为什么加 Example Lines：**
AI 看到具体的例句，比看一百字的风格描述更能准确把握语气。这是 Few-shot prompting 的核心原理。

**公主的特殊规则：**
公主是情报员，她的角色卡里有额外指令——必须在对话中夹带一条真实情报，但要包装成日常对话，不能直接说破。

**舅舅的特殊规则：**
舅舅是观察者，他的角色卡里有额外指令——必须评估玩家的回答是否触发疑心值增加，并在 JSON 里体现。

---

## Module 4 · [STATE] 游戏状态模块

**作用：** 告诉 AI 当前游戏的数值状态，让它的回复符合当前局势。  
**Purpose:** Provides current game state so AI responses reflect the actual situation.

**设计原则：**
- 每轮调用 API 前动态生成
- 包含所有资源数值和疑心值
- 告诉 AI 当前轮数，让它感知剧情进度

**代码实现：**
在 Unity 里，这个模块用字符串拼接动态生成：

```csharp
string stateModule = $@"[STATE]
Current Round: {currentRound}/12
Treasury: {gold}
Popularity: {popularity}
Church: {church}
Military: {military}
Uncle Suspicion: {suspicion}
";
```

---

## 调试指南 · Debugging Guide

**AI 返回的 JSON 格式不对：**
在 `[RULES]` 模块的格式要求前加一句：  
*"You MUST return ONLY the JSON object. No extra text, no markdown, no explanation."*

**AI 角色扮演跑偏（说了不符合人设的话）：**
在对应 NPC 的 `Example Lines` 里加更多例句，3-5句比2-3句效果好。

**数值变化幅度太大或太小：**
在 `[RULES]` 里明确说明：  
*"Each value change must feel proportional to the player's response. A mild answer: ±5. A strong answer: ±10~15. An extreme answer: ±20."*

**AI 说了游戏外的内容（比如提到自己是AI）：**
在 `[RULES]` 里加一条硬性规则：  
*"Never break character. Never acknowledge you are an AI. Never reference anything outside this game world."*

---

## 版本记录 · Version History

| 版本 | 日期 | 变更 |
|------|------|------|
| v1.0 | 2026 | 初始版本，包含6个NPC角色卡 |
