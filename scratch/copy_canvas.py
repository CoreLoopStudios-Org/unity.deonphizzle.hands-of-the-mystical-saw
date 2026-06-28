import re
import sys

# Paths
source_scene_path = r"C:\Users\User\Documents\GitHub\unity.deonphizzle.hands-of-the-mystical-saw\Assets\ALL-SCENE-IS HERE\StoneCuttingScene_Classic.unity"
target_scene_path = r"C:\Users\User\Documents\GitHub\unity.deonphizzle.hands-of-the-mystical-saw\Assets\ALL-SCENE-IS HERE\Game-One\Game-One-StoneCuttingScene.unity"

canvas_go_id = "228145358"
eventsystem_go_id = "1426941398"

print("Parsing source scene...")
with open(source_scene_path, 'r', encoding='utf-8') as f:
    source_content = f.read()

source_docs = source_content.split("--- ")
source_objects = {} # ID -> (class_id, doc_text)

# Helper to find file ID from doc header
def get_doc_info(doc):
    header_match = re.match(r"!u!(\d+) &(\d+)(?:\s+stripped)?", doc)
    if header_match:
        return header_match.group(1), header_match.group(2)
    return None, None

for doc in source_docs:
    class_id, file_id = get_doc_info(doc)
    if file_id:
        source_objects[file_id] = (class_id, doc)

# Build parent-child mapping for transforms
transform_fathers = {} # transform_id -> father_id
transform_to_go = {} # transform_id -> go_id
go_to_transform = {} # go_id -> transform_id
go_components = {} # go_id -> list of file_ids (components)
prefab_instances = {} # prefab_instance_id -> doc

# Second pass: Parse relations
for file_id, (class_id, doc) in source_objects.items():
    lines = doc.splitlines()
    if class_id == "4" or class_id == "224": # Transform or RectTransform
        father = "0"
        go = ""
        for line in lines:
            if "m_Father:" in line:
                m = re.search(r"fileID: (-?\d+)", line)
                if m: father = m.group(1)
            if "m_GameObject:" in line:
                m = re.search(r"fileID: (-?\d+)", line)
                if m: go = m.group(1)
        transform_fathers[file_id] = father
        transform_to_go[file_id] = go
        if go:
            go_to_transform[go] = file_id
            
    elif class_id == "1": # GameObject
        components = []
        for line in lines:
            if "- component:" in line:
                m = re.search(r"fileID: (-?\d+)", line)
                if m: components.append(m.group(1))
        go_components[file_id] = components
        
    elif class_id == "1001": # PrefabInstance
        prefab_instances[file_id] = doc
        # Also parse transform parents within the prefab modifications
        parent_tr = None
        for line in lines:
            if "m_TransformParent:" in line:
                m = re.search(r"fileID: (-?\d+)", line)
                if m: parent_tr = m.group(1)
        # We also associate components modified in PrefabInstance
        # E.g. find all fileIDs defined inside this block
        # PrefabInstance docs define stripped objects in separate blocks, but they reference the PrefabInstance ID: m_PrefabInstance: {fileID: PrefabInstanceID}

# Now recursively find all descendants of the Canvas transform
canvas_tr_id = go_to_transform[canvas_go_id]
canvas_descendant_trs = {canvas_tr_id}

# Also find stripped transforms that are parented to canvas descendants
# We do multiple passes to resolve all parent-child relationships
changed = True
while changed:
    changed = False
    # Check regular fathers
    for tr_id, father in transform_fathers.items():
        if father in canvas_descendant_trs and tr_id not in canvas_descendant_trs:
            canvas_descendant_trs.add(tr_id)
            changed = True
    
    # Check PrefabInstance modifications for parent relations
    for pref_id, doc in prefab_instances.items():
        lines = doc.splitlines()
        current_target = None
        for line in lines:
            if "- target:" in line:
                m = re.search(r"fileID: (-?\d+)", line)
                if m: current_target = m.group(1)
            if "propertyPath: m_TransformParent" in line:
                # Find the next objectReference or value fileID
                pass
            # Let's find any line like: objectReference: {fileID: ParentTransformID} or value: ParentTransformID or m_TransformParent: {fileID: ParentTransformID}
            if "m_TransformParent:" in line or "objectReference:" in line:
                m = re.search(r"fileID: (-?\d+)", line)
                if m and m.group(1) in canvas_descendant_trs:
                    # Let's check what target or stripped transform is being modified
                    # Usually, the PrefabInstance will have modifications targeting the root transform of the prefab
                    # Let's find stripped transforms that reference this PrefabInstance
                    for f_id, (c_id, s_doc) in source_objects.items():
                        if c_id in ("4", "224") and f"m_PrefabInstance: {{fileID: {pref_id}}}" in s_doc:
                            if f_id not in canvas_descendant_trs:
                                canvas_descendant_trs.add(f_id)
                                changed = True

print(f"Total descendant transforms: {len(canvas_descendant_trs)}")

# Let's gather all GameObjects, Components, and PrefabInstances associated with these transforms
file_ids_to_copy = set()

# Canvas itself
file_ids_to_copy.add(canvas_go_id)
file_ids_to_copy.add(canvas_tr_id)
for comp_id in go_components.get(canvas_go_id, []):
    file_ids_to_copy.add(comp_id)

# EventSystem
eventsystem_tr_id = go_to_transform[eventsystem_go_id]
file_ids_to_copy.add(eventsystem_go_id)
file_ids_to_copy.add(eventsystem_tr_id)
for comp_id in go_components.get(eventsystem_go_id, []):
    file_ids_to_copy.add(comp_id)

# Descendants
for tr_id in canvas_descendant_trs:
    file_ids_to_copy.add(tr_id)
    go_id = transform_to_go.get(tr_id)
    if go_id:
        file_ids_to_copy.add(go_id)
        for comp_id in go_components.get(go_id, []):
            file_ids_to_copy.add(comp_id)

# Now find stripped components/gameobjects/prefabs referencing copied objects
# E.g. MonoBehaviours, CanvasRenderers, PrefabInstances
# If an object references a copied PrefabInstance, or a stripped object references a copied PrefabInstance, we copy it.
for f_id, (c_id, doc) in source_objects.items():
    # If it is a stripped GameObject or component, check if it belongs to a copied PrefabInstance
    # Or check if it is a PrefabInstance whose modifications refer to our copied objects
    if c_id == "1001": # PrefabInstance
        # If it modifies any of our copied transforms, copy the PrefabInstance
        ref_copied = False
        for line in doc.splitlines():
            m = re.search(r"fileID: (-?\d+)", line)
            if m and m.group(1) in file_ids_to_copy:
                ref_copied = True
                break
        if ref_copied:
            file_ids_to_copy.add(f_id)

# Now do another pass: if a doc is a stripped component with m_PrefabInstance: {fileID: copied_pref_id}, copy it too!
for f_id, (c_id, doc) in source_objects.items():
    if "m_PrefabInstance:" in doc:
        m = re.search(r"m_PrefabInstance: \{fileID: (-?\d+)\}", doc)
        if m and m.group(1) in file_ids_to_copy:
            file_ids_to_copy.add(f_id)
            # If it's a component/transform, also copy its GameObject if it is stripped
            go_match = re.search(r"m_GameObject: \{fileID: (-?\d+)\}", doc)
            if go_match:
                file_ids_to_copy.add(go_match.group(1))

# Filter out invalid IDs
file_ids_to_copy = {fid for fid in file_ids_to_copy if fid and fid != "0" and fid != "-1"}

print(f"Total file IDs to copy (including stripped components & prefabs): {len(file_ids_to_copy)}")

# 3. Handle ID Conflicts & generate remappings
print("Parsing target scene...")
with open(target_scene_path, 'r', encoding='utf-8') as f:
    target_content = f.read()

target_docs = target_content.split("--- ")
target_file_ids = set()
for doc in target_docs:
    _, file_id = get_doc_info(doc)
    if file_id:
        target_file_ids.add(file_id)

id_mapping = {}
next_id = 2000000000

for src_id in file_ids_to_copy:
    while str(next_id) in target_file_ids or str(next_id) in id_mapping.values():
        next_id += 1
    id_mapping[src_id] = str(next_id)
    next_id += 1

print("Remapping references in copied docs...")
copied_docs = []
for src_id in file_ids_to_copy:
    class_id, doc = source_objects[src_id]
    
    # Replace references
    def replacer(match):
        fid = match.group(1)
        if fid in id_mapping:
            return f"fileID: {id_mapping[fid]}"
        return match.group(0)
    
    new_doc = re.sub(r"fileID: (-?\d+)", replacer, doc)
    
    header_line = new_doc.splitlines()[0]
    # Check if header contains stripped
    is_stripped = "stripped" in header_line
    new_header = f"!u!{class_id} &{id_mapping[src_id]}"
    if is_stripped:
        new_header += " stripped"
    new_doc = new_doc.replace(header_line, new_header, 1)
    
    copied_docs.append(new_doc)

# 4. Find UI component references
# For strikeText: fileID 53307162 in classic scene
# For victoryPanel: fileID 1847227941 in classic scene
# For gameOverPanel: fileID 1384951613 in classic scene
# For strikeIcon1: fileID 278919131 in classic scene
# For strikeIcon2: fileID 397430455 in classic scene
# For strikeIcon3: fileID 1646785200 in classic scene
# For earnedPointsText: fileID 937918431 in classic scene

strike_text_id = id_mapping.get("53307162", "0")
victory_panel_id = id_mapping.get("1847227941", "0")
game_over_panel_id = id_mapping.get("1384951613", "0")
strike_icon_1_id = id_mapping.get("278919131", "0")
strike_icon_2_id = id_mapping.get("397430455", "0")
strike_icon_3_id = id_mapping.get("1646785200", "0")
earned_points_text_id = id_mapping.get("937918431", "0")

print(f"Mapped References:")
print(f"  victoryPanel: {victory_panel_id}")
print(f"  gameOverPanel: {game_over_panel_id}")
print(f"  strikeIcon1: {strike_icon_1_id}")
print(f"  strikeIcon2: {strike_icon_2_id}")
print(f"  strikeIcon3: {strike_icon_3_id}")
print(f"  strikeText: {strike_text_id}")
print(f"  earnedPointsText: {earned_points_text_id}")

# 5. Modify target scene file content
stone_gen_index = -1
for idx, doc in enumerate(target_docs):
    if "!u!114 &1804209492" in doc:
        stone_gen_index = idx
        break

if stone_gen_index == -1:
    print("Error: Could not find StoneGenerator with ID 1804209492 in target scene.")
    sys.exit(1)

stone_gen_doc = target_docs[stone_gen_index]

# Update the prefab to rock02_1.prefab (guid: 2a08acc45cb6b474e83f93172ec6726b, fileID: 2283429534873605795)
stone_gen_doc = re.sub(
    r"stonePrefab: \{fileID: \d+, guid: [a-f0-9]+, type: \d+\}",
    "stonePrefab: {fileID: 2283429534873605795, guid: 2a08acc45cb6b474e83f93172ec6726b, type: 3}",
    stone_gen_doc
)

# Update anchorSizeMultiplier to 1
stone_gen_doc = re.sub(
    r"anchorSizeMultiplier: \d+(\.\d+)?",
    "anchorSizeMultiplier: 1",
    stone_gen_doc
)

# Update UI fields
stone_gen_doc = re.sub(r"strikeText: \{fileID: \d+\}", f"strikeText: {{fileID: {strike_text_id}}}", stone_gen_doc)
stone_gen_doc = re.sub(r"victoryPanel: \{fileID: \d+\}", f"victoryPanel: {{fileID: {victory_panel_id}}}", stone_gen_doc)
stone_gen_doc = re.sub(r"gameOverPanel: \{fileID: \d+\}", f"gameOverPanel: {{fileID: {game_over_panel_id}}}", stone_gen_doc)
stone_gen_doc = re.sub(r"strikeIcon1: \{fileID: \d+\}", f"strikeIcon1: {{fileID: {strike_icon_1_id}}}", stone_gen_doc)
stone_gen_doc = re.sub(r"strikeIcon2: \{fileID: \d+\}", f"strikeIcon2: {{fileID: {strike_icon_2_id}}}", stone_gen_doc)
stone_gen_doc = re.sub(r"strikeIcon3: \{fileID: \d+\}", f"strikeIcon3: {{fileID: {strike_icon_3_id}}}", stone_gen_doc)
stone_gen_doc = re.sub(r"earnedPointsText: \{fileID: \d+\}", f"earnedPointsText: {{fileID: {earned_points_text_id}}}", stone_gen_doc)

target_docs[stone_gen_index] = stone_gen_doc

# Append the copied UI documents to target_docs
target_docs.extend(copied_docs)

# Write back target scene file
print("Writing updated scene content...")
new_target_content = "--- ".join(target_docs)

with open(target_scene_path, 'w', encoding='utf-8') as f:
    f.write(new_target_content)

print("Successfully merged Canvas and EventSystem, updated StoneGenerator fields and stone prefab!")
