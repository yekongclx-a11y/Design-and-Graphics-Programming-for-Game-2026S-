# 王权 · Crown: The Gilded Cage

> You sit upon the highest throne, yet you are the most powerless person in this palace.

An AI-Driven 2D Throne Simulator — Graduate Game Design Project

---

## Overview

Crown: The Gilded Cage is a 2D visual novel survival game powered by real-time AI interaction. The player takes the role of a young king, confined to the throne, wielding language as the only weapon while navigating the shadows of competing powers.

Every word spoken may tighten the noose.

## 核心机制

- 自然语言交互：玩家自由输入，AI实时解析意图并响应
- 四维资源系统：金库、民心、教会、军队，任意一项到极值即触发死亡
- 铁三角关系：摄政王（审判者）、未婚妻（情报员）、玩家（生存者）
- 平庸即出路：极端选择必死，唯有在各方势力间保持微妙平衡

## 技术栈

- 引擎：Unity 2022 LTS
- AI：Claude API（Anthropic）
- 版本管理：Git / GitHub

## 项目结构

```
Assets/
├── Scenes/          # Unity场景
├── Scripts/         # C#代码
├── Sprites/
│   ├── Backgrounds/ # 背景图
│   ├── Characters/  # NPC立绘
│   └── UI/          # 界面素材
├── Audio/
│   ├── Music/       # 背景音乐
│   └── SFX/         # 音效
└── Fonts/           # 字体

Docs/
├── GDD.md           # 游戏设计文档
├── Prompts/         # System Prompt版本记录
└── Art/             # 美术参考
```

## License

This project is for academic purposes only.