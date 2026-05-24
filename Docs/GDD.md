# Game Design Document · 游戏设计文档
## Crown: The Gilded Cage · 王权

**Version:** 1.2  
**Status:** Production Complete  
**Updated:** 2026-05

---

## 1. Game Vision · 游戏愿景

Crown: The Gilded Cage is a 2D AI-driven visual novel survival game set in a medieval court. The player takes the role of a sixteen-year-old King who has just inherited the crown. He is, by design, the most powerless person in the palace.

The true ruler is the Regent — the King's uncle — who stands in the shadows of every conversation, watching, waiting for one mistake.

**Design thesis:** *Mediocrity is the only path. Extremes kill.*

The player's only weapon is language. There are no armies to command, no gold to spend freely, no allies to trust. Every word spoken to every visitor is a political act with mechanical consequences.

This is not a story of glory. This is a story of survival.

---

## 2. Core Concept · 核心立意

### The Trap of Power · 权力的陷阱

The player sits on the highest throne but controls nothing. All apparent agency is constrained by invisible power structures the player cannot see — only feel. The game explores the gap between the symbolism of authority and its reality.

### The Language Weapon · 语言即武器

Unlike traditional choice-based games, the player types free-form natural language responses. The AI (Gemini) processes intent in real time and returns structured mechanical outcomes. There are no "correct" answers — only consequences that ripple through five resource dimensions simultaneously.

### The Balance Imperative · 平衡法则

Four visible resources (Treasury, Popularity, Church, Military) and one hidden metric (Uncle's Suspicion) define survival. The winning strategy is never maximizing any one dimension — it is navigating the center. Resources that rise too high invite destruction from a different direction than resources that fall too low.

---

## 3. Core Loop · 核心循环

```
Round Start (1 of 12)
       ↓
[Random Event Check] — may interrupt with a choice event
       ↓
NPC Enters — opens with surface request
       ↓
Player types response (up to maxTurns: 3 for most NPCs, 2 for Princess)
       ↓
Gemini AI returns JSON → resource deltas applied → suspicion adjusted
       ↓
triggerEvent check → normal / end_round / coup_attempt / game_over / uncle_intervene
       ↓
Round ends → suspicion decays -3 → next round or game end
       ↓
Repeat × 12 rounds → Victory check
```

**One "round" = one NPC audience of 1–3 player turns.**  
A full game is 12 rounds. Random events insert between rounds, not inside them.

---

## 4. Resource System · 资源系统

| 资源 | Resource | 过高后果 | 过低后果 |
|------|----------|----------|----------|
| 金库 | Treasury | 舅舅认为你在积蓄势力 | 国家财政崩溃 |
| 民心 | Popularity | 舅舅认为你收买人心 | 暴民冲入皇宫 |
| 教会 | Church | 教会凌驾于王权之上 | 被宣布为异端 |
| 军队 | Military | 将军拥兵自重 | 无力镇压叛乱 |

所有资源范围：**0 — 100**  
安全区间：**20 — 80**  
极值（0或100）触发对应死亡结局。

**Uncle's Suspicion · 疑心值**  
范围：0 — 100。隐藏值，玩家不可直接观察。  
到达 100 → 触发「高塔之囚」结局。  
每轮结束后自然衰减 3 点（给玩家喘息空间）。  
每次对话单轮上限：AI返回值在客户端钳制至 ±8。

---

## 5. Characters · 角色

### 摄政王（舅舅）· The Regent
- **职能：** 绝对的审判者
- **行为逻辑：** 监视所有资源变动，疑心值达到临界时触发政变
- **对话风格：** 表面温和，每句话都暗含威胁；永远面带微笑，但微笑从不抵达眼睛
- **特殊规则：** 第12轮亲自登场，为终局模式；不接受退下指令

### 未婚妻（公主）· The Princess
- **职能：** 战术情报员
- **行为逻辑：** 每次登场必须在对话中嵌入一条真实情报，包装成日常对话
- **对话风格：** 克制、简洁，把危险信息藏在比喻和天气里
- **特殊机制：** 好感度≥80时，可一次性化解任意资源死亡条件（每局限一次）

### 大臣 · The Chancellor
- **诉求：** 税收、财政调整、贸易政策
- **资源影响：** 主要影响金库和民心
- **隐藏目的：** 将财富引流至摄政王亲信网络

### 将军 · The General
- **诉求：** 军费、战争授权、军官任命
- **资源影响：** 主要影响军队和金库
- **隐藏目的：** 将忠于国王的军官替换为摄政王的人

### 教士 · The Bishop
- **诉求：** 宗教政策、土地捐赠、异端审判
- **资源影响：** 主要影响教会和民心
- **隐藏目的：** 使教会对王权不可或缺

### 平民 · The Commoner
- **诉求：** 减税、粮食、公正
- **资源影响：** 主要影响民心
- **特殊规则：** 说话直白，无政治包装；是唯一真正在受苦的NPC

---

## 6. Endings · 结局

### 普通资源死亡（8种）
任意资源归零或达到100，触发对应死亡结局（详见 `endings_v1.md`）。

### 特殊机制结局（3种）
- **高塔之囚 · The Tower** — 疑心值达到100，摄政王发动政变，国王幽禁于高塔
- **狂妄的代价 · The Last Word** — AI触发`game_over`，玩家说出了不可挽回的话
- **王权破晓 · The True Coronation** — 完成12轮，全部资源在20–80区间，疑心值<50

---

## 7. AI Integration · AI 集成

### System Prompt Architecture

Prompt 由四个模块组成，前两个固定，后两个每轮动态填充：

```
[SYSTEM INSTRUCTIONS]  ← 规则锚定
[WORLD]                ← 世界观与氛围（固定）
[RULES]                ← 输出格式与行为约束（固定）
[CHARACTER]            ← 当前NPC角色卡（动态）
[STATE]                ← 当前游戏数值状态（动态）
```

### Mandatory Output Format · 强制输出格式

```json
{
  "action": "NPC动作描写，第三人称过去时，1句，≤20词",
  "dialogue": "NPC台词，1-3句，≤50词总计",
  "gold": 0,
  "popularity": 0,
  "church": 0,
  "military": 0,
  "suspicion": 0,
  "triggerEvent": "none",
  "historySummary": ""
}
```

### Field Constraints · 字段约束

| Field | Range | Notes |
|-------|-------|-------|
| `gold` / `popularity` / `church` / `military` | -20 to +20 | Hard limit; larger values break game balance |
| `suspicion` | 0 to +20 | Can only increase via AI; client clamps to ±8 before applying |
| `triggerEvent` | `none` / `end_round` / `coup_attempt` / `game_over` / `uncle_intervene` | AI uses autonomous judgment |
| `historySummary` | string, 10-15 words or "" | Political consequence for long-term memory chain |

### Long-Term Memory Chain · 长期记忆链

每轮对话完成后，`historySummary` 字段记录本轮的核心政治后果（10-15词），注入后续轮次的上下文。这使AI在第12轮仍能感知第1轮的决策影响，形成叙事连贯性。

示例：`"King defied General's request — military loyalty weakened, Regent's suspicion rose."`

空字符串（`""`）表示本次交互无重要政治影响，不写入记忆链。

### Prompt Version Management

所有System Prompt存放于 `Docs/Prompts/`，活跃版本同步至 `Crown/Assets/StreamingAssets/prompt_v1.txt`。

---

## 8. Art Direction · 美术风格

- **风格：** 暗色写实油画风
- **分辨率：** 1920 × 1080（强制锁定，全屏窗口模式）
- **NPC呈现：** 半身像，叠加于背景之上
- **UI风格：** 哥特边框，羊皮纸对话框，金色细节
- **美术工具：** Seendance 5.1（图像生成）

---

## 9. Audio Direction · 音效风格

- **背景音乐：** 中世纪宫廷风，低沉压抑（Mureka V9生成）
- **配音：** ElevenLabs（NPC角色语音）
- **音效乐器：** 大提琴拨弦（pizzicato）
- **音效列表：**
  - NPC进场：沉重木门
  - 玩家发送：羽毛笔划纸
  - 资源上升：大提琴拨弦上行
  - 资源下降：大提琴拨弦下行
  - 游戏结束：戏剧性弦乐

---

## 10. Development Milestones · 开发里程碑

| 阶段 | 内容 | 状态 |
|------|------|------|
| Pre-Production | 概念设计、美术素材、GDD | ✅ 完成 |
| Prototype | Unity框架、API接入、核心循环 | ✅ 完成 |
| Alpha | 全NPC、结局系统、音效接入 | ✅ 完成 |
| Polish | UI打磨、Prompt优化、随机事件系统、测试 | ✅ 完成 |
