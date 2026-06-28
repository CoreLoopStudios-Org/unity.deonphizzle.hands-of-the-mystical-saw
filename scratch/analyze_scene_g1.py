import re

scene_path = r"C:\Users\User\Documents\GitHub\unity.deonphizzle.hands-of-the-mystical-saw\Assets\ALL-SCENE-IS HERE\Game-One\Game-One-StoneCuttingScene.unity"

with open(scene_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Split into YAML documents
documents = content.split("--- ")

game_objects = {}
transforms = {}
monobehaviours = {}

for doc in documents:
    if not doc.strip():
        continue
    header_match = re.match(r"!u!(\d+) &(\d+)", doc)
    if not header_match:
        continue
    class_id, file_id = header_match.groups()
    lines = doc.splitlines()
    
    if class_id == "1": # GameObject
        name = ""
        for line in lines:
            if "m_Name:" in line:
                name = line.split("m_Name:")[1].strip()
        game_objects[file_id] = {"name": name, "components": []}
    elif class_id == "4": # Transform
        father = ""
        game_object = ""
        for line in lines:
            if "m_Father:" in line:
                father = re.search(r"fileID: (-?\d+)", line).group(1)
            if "m_GameObject:" in line:
                game_object = re.search(r"fileID: (-?\d+)", line).group(1)
        transforms[file_id] = {"father": father, "game_object": game_object}
    elif class_id == "114": # MonoBehaviour
        game_object = ""
        guid = ""
        for line in lines:
            if "m_GameObject:" in line:
                game_object = re.search(r"fileID: (-?\d+)", line).group(1)
            if "guid:" in line:
                guid = re.search(r"guid: ([a-f0-9]+)", line).group(1)
        monobehaviours[file_id] = {"game_object": game_object, "guid": guid}

# Connect components to GameObjects
for mb_id, mb in monobehaviours.items():
    go_id = mb["game_object"]
    if go_id in game_objects:
        game_objects[go_id]["components"].append(mb)

# Print all GameObjects with components
print("=== GAMEOBJECTS & MONOBEHAVIOURS ===")
for go_id, go in game_objects.items():
    print(f"GameObject: {go['name']} (ID: {go_id})")
    if go["components"]:
        for comp in go["components"]:
            print(f"  MonoBehaviour Script GUID: {comp['guid']}")
