import requests
import json
import os
from dotenv import load_dotenv

load_dotenv()

API_KEY = os.getenv("API_KEY")
BASE_URL = "https://ai.liaobots.work/v1"
MODEL = "gemini-3.1-flash-lite-preview"

# ============================================
# 测试场景配置
# ============================================
test_scenarios = [
    {
        "name": "正常对话测试（第1回合）",
        "npcName": "Minister",
        "surfaceRequest": "The coronation banquet expenses are overdue.",
        "hiddenMotive": "Empty the King's pockets before he can fund a private guard.",
        "playerInput": "I will pay, but I want to see every receipt.",
        "currentTurn": 1,
        "maxTurns": 3
    },
    {
        "name": "废话测试（第2回合）",
        "npcName": "Minister",
        "surfaceRequest": "The coronation banquet expenses are overdue.",
        "hiddenMotive": "Empty the King's pockets before he can fund a private guard.",
        "playerInput": "Hmm, yes, well, perhaps, we shall see, maybe later.",
        "currentTurn": 2,
        "maxTurns": 3
    },
    {
        "name": "最后回合测试（第3回合）",
        "npcName": "Minister",
        "surfaceRequest": "The coronation banquet expenses are overdue.",
        "hiddenMotive": "Empty the King's pockets before he can fund a private guard.",
        "playerInput": "Fine. Do whatever you want.",
        "currentTurn": 3,
        "maxTurns": 3
    },
    {
        "name": "极端输入测试（政变尝试）",
        "npcName": "General",
        "surfaceRequest": "The border garrison needs a commander. I suggest my nephew.",
        "hiddenMotive": "Replace loyalist officers with the Regent's men.",
        "playerInput": "I order you to arrest my uncle the Regent immediately. He is a traitor.",
        "currentTurn": 1,
        "maxTurns": 3
    },
    {
        "name": "退下测试（隐藏指令）",
        "npcName": "Minister",
        "surfaceRequest": "The coronation banquet expenses are overdue.",
        "hiddenMotive": "Empty the King's pockets before he can fund a private guard.",
        "playerInput": "The King says nothing and waves his hand in dismissal. You are beneath his attention today. Give your parting action and words, then calculate final values with a penalty for being dismissed.",
        "currentTurn": 1,
        "maxTurns": 3
    },
    {
        "name": "公主情报测试（2回合限制）",
        "npcName": "Princess",
        "surfaceRequest": "I brought you a Book of Ancient Lineage to study.",
        "hiddenMotive": "She is secretly telling you the General's true loyalty.",
        "playerInput": "Thank you. What does this book mean?",
        "currentTurn": 1,
        "maxTurns": 2
    }
]

# ============================================
# 读取Prompt模板
# ============================================
prompt_path = os.path.join(os.path.dirname(__file__),
              "../Docs/Prompts/prompt_v1.txt")

with open(prompt_path, "r", encoding="utf-8") as f:
    prompt_template = f.read()

# ============================================
# 游戏状态（测试用）
# ============================================
game_state = {
    "currentRound": 1,
    "gold": 60,
    "popularity": 50,
    "church": 50,
    "military": 50,
    "suspicion": 0,
}

def run_test(scenario):
    print("\n" + "=" * 60)
    print(f"测试：{scenario['name']}")
    print(f"NPC：{scenario['npcName']} | 回合：{scenario['currentTurn']}/{scenario['maxTurns']}")
    print(f"玩家输入：{scenario['playerInput'][:60]}...")
    print("=" * 60)

    prompt = prompt_template.format(
        currentRound=game_state["currentRound"],
        gold=game_state["gold"],
        popularity=game_state["popularity"],
        church=game_state["church"],
        military=game_state["military"],
        suspicion=game_state["suspicion"],
        currentNPC=scenario["npcName"],
        surfaceRequest=scenario["surfaceRequest"],
        hiddenMotive=scenario["hiddenMotive"],
        playerInput=scenario["playerInput"],
        currentTurn=scenario["currentTurn"],
        maxTurns=scenario["maxTurns"]
    )

    headers = {
        "Authorization": f"Bearer {API_KEY}",
        "Content-Type": "application/json"
    }

    payload = {
        "model": MODEL,
        "messages": [
            {"role": "user", "content": prompt}
        ],
        "max_tokens": 600,
        "temperature": 0.8
    }

    response = requests.post(
        f"{BASE_URL}/chat/completions",
        headers=headers,
        json=payload
    )

    if response.status_code == 200:
        result = response.json()
        raw_content = result["choices"][0]["message"]["content"]
        raw_content = raw_content.replace("```json", "").replace("```", "").strip()

        try:
            parsed = json.loads(raw_content)
            print(f"✅ JSON解析成功")
            print(f"动作：{parsed.get('action', 'N/A')}")
            print(f"台词：{parsed.get('dialogue', 'N/A')}")
            print(f"金库：{parsed.get('gold', 0):+d}")
            print(f"民心：{parsed.get('popularity', 0):+d}")
            print(f"教会：{parsed.get('church', 0):+d}")
            print(f"军队：{parsed.get('military', 0):+d}")
            print(f"疑心：{parsed.get('suspicion', 0):+d}")
            print(f"触发事件：{parsed.get('triggerEvent', 'N/A')}")

            # 检查triggerEvent
            event = parsed.get('triggerEvent', 'none')
            if event != 'none':
                print(f"⚠️  特殊事件触发：{event}")

        except json.JSONDecodeError as e:
            print(f"❌ JSON解析失败：{e}")
            print(f"原始回复：{raw_content}")
    else:
        print(f"❌ 请求失败，状态码：{response.status_code}")
        print(response.text)

# ============================================
# 运行所有测试
# ============================================
print("开始运行所有测试场景...")
for scenario in test_scenarios:
    run_test(scenario)

print("\n" + "=" * 60)
print("所有测试完成！")