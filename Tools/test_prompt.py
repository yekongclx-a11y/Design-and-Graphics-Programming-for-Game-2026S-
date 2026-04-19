import requests
import json
import os
from dotenv import load_dotenv

load_dotenv()

API_KEY = os.getenv("API_KEY")
BASE_URL = "https://ai.liaobots.work/v1"
MODEL = "gemini-3.1-flash-lite-preview"

test_state = {
    "currentRound": 1,
    "gold": 60,
    "popularity": 50,
    "church": 50,
    "military": 50,
    "suspicion": 0,
    "currentNPC": "Minister",
    "surfaceRequest": "The coronation banquet expenses are overdue.",
    "hiddenMotive": "Empty the King's pockets before he can fund a private guard.",
    "playerInput": "好吧，付钱。但我要看清楚每一笔账。"
}

prompt_path = os.path.join(os.path.dirname(__file__), "../Docs/Prompts/prompt_v1.txt")

with open(prompt_path, "r", encoding="utf-8") as f:
    prompt_template = f.read()

prompt = prompt_template.format(**test_state)

headers = {
    "Authorization": f"Bearer {API_KEY}",
    "Content-Type": "application/json"
}

payload = {
    "model": MODEL,
    "messages": [
        {"role": "user", "content": prompt}
    ],
    "max_tokens": 500,
    "temperature": 0.8
}

print("=" * 50)
print(f"NPC: {test_state['currentNPC']}")
print(f"玩家输入: {test_state['playerInput']}")
print("=" * 50)

response = requests.post(
    f"{BASE_URL}/chat/completions",
    headers=headers,
    json=payload
)

if response.status_code == 200:
    result = response.json()
    raw_content = result["choices"][0]["message"]["content"]
    
    print("\nAI 原始回复:")
    print(raw_content)
    print("\n" + "=" * 50)
    
    try:
        parsed = json.loads(raw_content)
        print("JSON 解析成功!")
        print(f"动作: {parsed.get('action', 'N/A')}")
        print(f"台词: {parsed.get('dialogue', 'N/A')}")
        print(f"金库变化: {parsed.get('gold', 0)}")
        print(f"民心变化: {parsed.get('popularity', 0)}")
        print(f"教会变化: {parsed.get('church', 0)}")
        print(f"军队变化: {parsed.get('military', 0)}")
        print(f"疑心值变化: {parsed.get('suspicion', 0)}")
    except json.JSONDecodeError:
        print("JSON 解析失败！AI没有返回正确格式。")
else:
    print(f"请求失败，状态码: {response.status_code}")
    print(response.text)