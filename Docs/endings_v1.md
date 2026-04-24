# Endings Design Document · 结局设计文档
## Crown: The Gilded Cage · 王权

**Version:** 1.0  
**Status:** Design Complete

---

## 结局总览 · Overview

游戏共有 **11 种结局**，分为三大类：
- 资源枯竭结局（4种）
- 资源溢出结局（4种）
- 特殊机制结局（3种）

---

## 一、资源枯竭结局 · Resource Depletion Endings

### 1. 【国库 = 0】无饷之军 · The Unpaid Guard

**触发条件：** `gold <= 0`

**剧情：**
禁卫军因长期欠薪发动哗变。没有金币，就没有忠诚。你死于乱军的刀剑之下，倒在自己的王座室里。

**英文标题：** *The Unpaid Guard*

**结局文案：**
> *The treasury runs dry. The guards have not been paid in weeks.*
> *Tonight, they remember they are men before they are soldiers.*
> *The throne room falls silent by dawn.*

**推荐背景图：** 黑暗宫廷内景，刀光剑影

**代码触发：** `gold <= 0` → `EndingManager.TriggerEnding("unpaid_guard")`

---

### 2. 【民心 = 0】暴民之怒 · The Mob's Verdict

**触发条件：** `popularity <= 0`

**剧情：**
忍无可忍的饥民用农具撞开了宫门。你被愤怒的群众从王座上拖下来，绞死在广场的灯柱上。

**英文标题：** *The Mob's Verdict*

**结局文案：**
> *They did not hate you. They were simply hungry.*
> *And hunger, left unanswered long enough, becomes something else entirely.*
> *The palace gates did not hold.*

**推荐背景图：** 愤怒农民持火把冲入宫殿

**代码触发：** `popularity <= 0` → `EndingManager.TriggerEnding("mob_verdict")`

---

### 3. 【教会 = 0】异端审判 · The Heretic's Pyre

**触发条件：** `church <= 0`

**剧情：**
教廷宣布你为被恶魔蛊惑的异端。广场上的火刑柱早已准备好，民众列队观看，大主教亲自宣读判词。

**英文标题：** *The Heretic's Pyre*

**结局文案：**
> *The Church does not need evidence. It needs a symbol.*
> *You were convenient.*
> *The smoke rose for three days. The Regent wept publicly.*

**推荐背景图：** 火刑柱、宗教审判场景

**代码触发：** `church <= 0` → `EndingManager.TriggerEnding("heretic_pyre")`

---

### 4. 【军队 = 0】破城之灾 · The Fallen Gates

**触发条件：** `military <= 0`

**剧情：**
边境防线彻底崩溃，敌国铁蹄踏破首都城门。你成为了敌国国王的阶下囚，或死于入城的第一波箭雨之中。

**英文标题：** *The Fallen Gates*

**结局文案：**
> *The border held for three hundred years.*
> *It held for three hundred years, and then it did not.*
> *The enemy king was not cruel. He was simply thorough.*

**推荐背景图：** 城墙破碎、敌军入城

**代码触发：** `military <= 0` → `EndingManager.TriggerEnding("fallen_gates")`

---

## 二、资源溢出结局 · Resource Overflow Endings

### 5. 【国库 = 100】引狼入室 · The Golden Target

**触发条件：** `gold >= 100`

**剧情：**
王国的财富已是天下皆知的秘密。富得流油却无力自保，强邻的大军以"保护贸易"为名越境，将国库洗劫一空。你的财富成了你的墓志铭。

**英文标题：** *The Golden Target*

**结局文案：**
> *Word of your treasury reached every court in the continent.*
> *The neighboring king sent his warmest regards.*
> *And then he sent his army.*

**推荐背景图：** 外敌入侵、金库被洗劫

**代码触发：** `gold >= 100` → `EndingManager.TriggerEnding("golden_target")`

---

### 6. 【民心 = 100】鸩毒之杯 · The Poisoned Cup

**触发条件：** `popularity >= 100`

**剧情：**
你太受爱戴了。贵族阶级感到了前所未有的威胁——一个真正被人民爱戴的国王比任何军队都危险。晚宴上，酒杯里漂浮着细不可察的粉末。

**英文标题：** *The Poisoned Cup*

**结局文案：**
> *You were loved. Genuinely, dangerously loved.*
> *The nobility could not allow that.*
> *The wine at the banquet was excellent. You said so yourself, just before.*

**推荐背景图：** 宫廷晚宴、毒酒场景

**代码触发：** `popularity >= 100` → `EndingManager.TriggerEnding("poisoned_cup")`

---

### 7. 【教会 = 100】神权圣子 · The Living Saint

**触发条件：** `church >= 100`

**剧情：**
教廷的权力极度膨胀。大主教宣布你为"降世圣子"，将你永久供奉在修道院最深处。国家彻底沦为神权政体，而你在烛光与祷告声中度过了余生，再未见过阳光。

**英文标题：** *The Living Saint*

**结局文案：**
> *They called it an honor.*
> *The finest cell in the monastery. Silk sheets. Three meals a day.*
> *The Archbishop visited every Sunday. He always smiled.*
> *You never left.*

**推荐背景图：** 修道院、烛光、软禁场景

**代码触发：** `church >= 100` → `EndingManager.TriggerEnding("living_saint")`

---

### 8. 【军队 = 100】黄袍加身 · The General's Crown

**触发条件：** `military >= 100`

**剧情：**
军队的权力不受节制。手握重兵的将军不再满足于臣子的地位。某个深夜，禁卫军将皇冠戴在了将军头上。你在睡梦中被悄悄处理，连挣扎的机会都没有。

**英文标题：** *The General's Crown*

**结局文案：**
> *He never wanted the throne, he said.*
> *He only wanted order.*
> *The crown fit him well, in the end.*
> *You were not there to see it.*

**推荐背景图：** 将军加冕、宫廷政变

**代码触发：** `military >= 100` → `EndingManager.TriggerEnding("generals_crown")`

---

## 三、特殊机制结局 · Special Trigger Endings

### 9. 【疑心爆表】高塔之囚 · The Tower

**触发条件：** `suspicion >= 80` → 输入框锁死

**剧情：**
你表现得太聪明，或者太叛逆。摄政王的耐心终于耗尽。他不再伪装，直接发动政变。没有审判，没有公告——你只是悄悄地从公众视野中消失了。高塔的窗口朝北，冬天很冷。

**英文标题：** *The Tower*

**结局文案：**
> *He came in the night, as you knew he would.*
> *"For your own safety," he said. He was always so considerate.*
> *The tower has a fine view of the city.*
> *You have had years to memorize it.*

**推荐背景图：** 高塔、铁窗、夜景

**代码触发：** `suspicion >= 80` → `EndingManager.TriggerEnding("the_tower")`

---

### 10. 【祸从口出】狂妄的代价 · The Last Word

**触发条件：** `triggerEvent == "game_over"`

**剧情：**
有些话，说出口就再也收不回来。你的一句话越过了所有人能够容忍的底线。结局来得比你预想的更快，更直接。

**英文标题：** *The Last Word*

**结局文案：**
> *Some words cannot be unsaid.*
> *You learned this the hard way.*
> *The court has a very long memory.*

**推荐背景图：** 根据具体触发情况动态选择

**代码触发：** `triggerEvent == "game_over"` → `EndingManager.TriggerEnding("last_word")`

---

### 11. 【胜利结局】王权破晓 · The True Coronation

**触发条件：** 完成12轮，所有资源在20-80之间，疑心值<50

**剧情：**
你在钢丝上走完了全程。没有人相信你能做到——包括你自己。但你利用各方势力的相互牵制，一点一点地蚕食着舅舅的权力基础。在第十二轮朝会结束后的那个夜晚，舅舅第一次，向你真正地低下了头。

**英文标题：** *The True Coronation*

**结局文案：**
> *You survived.*
> *Not through strength. Not through cunning alone.*
> *But because you understood, at last, what the throne truly was:*
> *not a seat of power, but a cage — and you had learned to make the cage work for you.*
> *The Regent bowed. Slowly. For the first time, without a smile.*

**推荐背景图：** 黎明、王座、胜利场景

**代码触发：** `CheckVictory()` 通过 → `EndingManager.TriggerEnding("true_coronation")`

---

## 图片资源规划 · Art Assets

| 结局 | 背景图文件名 | 可复用 |
|------|------------|--------|
| 无饷之军 | `bg_ending_guard.png` | 可与政变共用 |
| 暴民之怒 | `bg_ending_mob.png` | 独立 |
| 异端审判 | `bg_ending_pyre.png` | 独立 |
| 破城之灾 | `bg_ending_siege.png` | 独立 |
| 引狼入室 | `bg_ending_invasion.png` | 可与破城共用 |
| 鸩毒之杯 | `bg_ending_poison.png` | 独立 |
| 神权圣子 | `bg_ending_saint.png` | 独立 |
| 黄袍加身 | `bg_ending_general.png` | 可与政变共用 |
| 高塔之囚 | `bg_ending_tower.png` | 独立 |
| 狂妄的代价 | `bg_ending_lastword.png` | 可与政变共用 |
| 王权破晓 | `bg_ending_victory.png` | 独立 |

**最少需要生成：6-7张独立背景图**

---

## 技术实现 · Implementation

**EndingManager接收结局ID字符串：**
```
"unpaid_guard"
"mob_verdict"  
"heretic_pyre"
"fallen_gates"
"golden_target"
"poisoned_cup"
"living_saint"
"generals_crown"
"the_tower"
"last_word"
"true_coronation"
```

**场景跳转：**
`GameStateManager` → 触发结局 → `PlayerPrefs.SetString("EndingType", endingId)` → `SceneManager.LoadScene("EndingScene")` → `EndingScene`读取EndingType显示对应内容
