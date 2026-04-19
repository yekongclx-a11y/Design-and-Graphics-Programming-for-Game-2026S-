# Asset Naming Convention · 资源命名规范
## Crown: The Gilded Cage · 王权

**Version:** 1.1  
**Last Updated:** 2026

> ⚠️ 重要：所有Unity资源必须在Unity编辑器内重命名，不得在系统文件管理器中操作，否则会导致.meta文件错位。

---

## 命名总规则 · General Rules

- 全部小写，单词间用下划线 `_` 连接
- 不使用空格、中文、特殊字符
- 文件名必须能一眼看出资源类型和用途
- 图片统一使用 `.png`
- 音频统一使用 `.mp3`
- 脚本统一使用 `.cs`

---

## 前缀规范 · Prefix Convention

| 前缀 | 用途 |
|---|---|
| `npc_` | NPC立绘 |
| `bg_` | 背景图 |
| `ui_` | UI素材 |
| `bgm_` | 背景音乐 |
| `sfx_` | 音效 |
| `evt_` | 事件相关素材 |

---

## NPC立绘 · Characters (`Crown/Assets/Sprites/Characters/`)

### 主线NPC · Main NPCs

| 角色 | 文件名 | 状态 |
|---|---|---|
| 摄政王（舅舅） | `npc_regent.png` | ✅ 已完成 |
| 公主（未婚妻） | `npc_princess.png` | ⬜ 待生成 |
| 大臣（版本1） | `npc_minister_1.png` | ✅ 已完成 |
| 大臣（版本2） | `npc_minister_2.png` | ✅ 已完成 |
| 将军 | `npc_general.png` | ✅ 已完成 |
| 主教 | `npc_bishop.png` | ✅ 已完成 |
| 平民男 | `npc_commoner_male.png` | ✅ 已完成 |
| 平民女 | `npc_commoner_female.png` | ✅ 已完成 |

### 事件NPC · Event NPCs

| 角色 | 文件名 | 状态 | 出现事件 |
|---|---|---|---|
| 贴身侍女 | `npc_handmaid.png` | ⬜ 待生成 | 绝望的侍女 |
| 叛逃的骑士 | `npc_knight_deserter.png` | ⬜ 待生成 | 叛逃的骑士 |
| 刺客 | `npc_assassin.png` | ⬜ 待生成 | 刺客来袭 |
| 瞎眼占卜师 | `npc_oracle.png` | ⬜ 待生成 | 神秘的占卜师 |
| 贵族女 | `npc_noble_female.png` | ⬜ 待生成 | 外国使节/宴会事件 |
| 贵族青年 | `npc_noble_youth.png` | ✅ 已完成 | 外国使节 |
| 男性侍从 | `npc_servant_male.png` | ✅ 已完成 | 御厨/游吟诗人（复用） |

---

## 背景图 · Backgrounds (`Crown/Assets/Sprites/Backgrounds/`)

| 场景 | 文件名 | 状态 |
|---|---|---|
| 王座大厅（主场景） | `bg_throne_room.png` | ⬜ 待导入 |
| 结局-资源死亡 | `bg_ending_death.png` | ⬜ 待生成 |
| 结局-舅舅政变 | `bg_ending_coup.png` | ⬜ 待生成 |
| 结局-生存 | `bg_ending_survival.png` | ⬜ 待生成 |
| 标题页面 | `bg_title.png` | ⬜ 待导入 |

---

## UI素材 · UI (`Crown/Assets/Sprites/UI/`)

| 用途 | 文件名 | 状态 |
|---|---|---|
| 游戏Logo | `ui_logo.png` | ✅ 已完成 |
| 对话框背景 | `ui_dialogue_box.png` | ⬜ 待生成 |
| 资源条边框 | `ui_resource_bar.png` | ⬜ 待生成 |
| 输入框背景 | `ui_input_box.png` | ⬜ 待生成 |

---

## 音频 · Audio

### 背景音乐 (`Crown/Assets/Audio/Music/`)

| 用途 | 文件名 | 状态 |
|---|---|---|
| 主游戏背景音乐 | `bgm_main.mp3` | ✅ 已完成 |
| 主题曲 | `bgm_theme.mp3` | ✅ 已完成 |
| 游戏结束音乐 | `bgm_ending.mp3` | ✅ 已完成 |

### 音效 (`Crown/Assets/Audio/SFX/`)

| 用途 | 文件名 | 状态 |
|---|---|---|
| NPC进场 | `sfx_npc_enter.mp3` | ✅ 已完成 |
| 玩家发送输入 | `sfx_player_send.mp3` | ⬜ 待生成 |
| 数值增加 | `sfx_value_up.mp3` | ✅ 已完成 |
| 数值减少 | `sfx_value_down.mp3` | ✅ 已完成 |
| 游戏结束音效 | `sfx_game_over.mp3` | ⬜ 待生成 |

---

## 脚本 · Scripts (`Crown/Assets/Scripts/`)

| 功能 | 文件名 | 说明 |
|---|---|---|
| 游戏状态管理 | `GameStateManager.cs` | 管理所有资源数值、疑心值、轮数 |
| API调用管理 | `APIManager.cs` | 负责调用AI接口、处理返回JSON |
| UI控制 | `UIManager.cs` | 控制所有界面元素的显示和更新 |
| 对话系统 | `DialogueSystem.cs` | 控制NPC对话框、输入框逻辑 |
| 音频管理 | `AudioManager.cs` | 控制背景音乐和音效播放 |
| 事件管理 | `EventManager.cs` | 权重随机事件系统 |
| NPC数据 | `NPCData.cs` | 存储所有NPC的人设和当前好感度 |
| 结局管理 | `EndingManager.cs` | 判断和触发各种结局 |

---

## 场景 · Scenes (`Crown/Assets/Scenes/`)

| 场景 | 文件名 | 说明 |
|---|---|---|
| 主菜单 | `MainMenu.unity` | 标题页面、开始游戏、退出 |
| 主游戏 | `GameScene.unity` | 核心游戏场景 |
| 结局 | `EndingScene.unity` | 各种结局的展示场景 |

---

## 字体 · Fonts (`Crown/Assets/Fonts/`)

| 用途 | 文件名 | 说明 |
|---|---|---|
| 主字体 | `font_main.ttf` | 正文、对话框使用 |
| 标题字体 | `font_title.ttf` | 标题、强调文字使用 |
