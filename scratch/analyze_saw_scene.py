import re

scene_path = r"C:\Users\User\Documents\GitHub\unity.coremechanism.deonphizzle\Assets\ALL-SCENE-IS HERE\StoneCuttingScene_Classic.unity"

with open(scene_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Let's find all MonoBehaviour components in the scene and see which one is SawArmController
# In Unity scenes, components are separated by "--- !u!114"
components = re.split(r'--- !u!114 &', content)

saw_component = None
for comp in components[1:]:
    if "SawArmController" in comp:
        saw_component = comp
        break

if saw_component:
    print("=== Found SawArmController ===")
    print("\n".join(saw_component.split("\n")[:40]))
else:
    print("SawArmController not found in scene.")
