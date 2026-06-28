import re

scene_path = r"C:\Users\User\Documents\GitHub\unity.coremechanism.deonphizzle\Assets\ALL-SCENE-IS HERE\StoneCuttingScene_Classic.unity"

with open(scene_path, 'r', encoding='utf-8') as f:
    content = f.read()

# We need to trace the child transforms of the Saw_rigged -newclassic prefab instance
# Let's find the prefab instance first.
# PrefabInstance:
#   m_CorrespondingSourceObject: {fileID: 100100000, guid: d2a1fd97f8f1a1e4b874ad58fa5ec59e, type: 3}
# Let's find all MonoBehaviour / Transform modifications on this prefab instance.

# Let's print the modifications for d2a1fd97f8f1a1e4b874ad58fa5ec59e
modifications_pattern = r'm_SourcePrefab: \{fileID: 100100000, guid: d2a1fd97f8f1a1e4b874ad58fa5ec59e, type: 3\}(.*?)(?=\n\w|\Z)'
match = re.search(modifications_pattern, content, re.DOTALL)

if match:
    print("=== Found Prefab Instance Modifications ===")
    # Print the modifications
    mods = match.group(1)
    print(mods[:2000]) # Print first 2000 chars of modifications
else:
    print("No prefab modifications found for saw.")
