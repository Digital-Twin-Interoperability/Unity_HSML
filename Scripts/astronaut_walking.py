import omni
from pxr import Gf, UsdGeom, Sdf

# Initialize Omniverse
omni.kit.app.get_app().initialize()

# Get the stage
stage = omni.usd.get_context().get_stage()

# Create a new prim (e.g., a Cube)
prim_path = Sdf.Path("/World/Walking")
prim = stage.DefinePrim(prim_path, "Xform")
stage.SetDefaultPrim(prim)

# Create a cube as the object to move
cube_path = prim_path.AppendPath("Cube")
cube = UsdGeom.Cube.Define(stage, cube_path)
cube.GetPrim().GetAttribute("size").Set(100)

# Function to update the object's position
def move_prim(prim, direction, amount):
    transform = prim.GetAttribute("xformOp:transform").Get()
    position = transform.ExtractTranslation()
    if direction == 'forward':
        position[2] -= amount
    elif direction == 'backward':
        position[2] += amount
    elif direction == 'left':
        position[0] -= amount
    elif direction == 'right':
        position[0] += amount
    elif direction == 'up':
        position[1] += amount
    elif direction == 'down':
        position[1] -= amount
    transform.SetTranslate(position)
    prim.GetAttribute("xformOp:transform").Set(transform)

# Keyboard event handler
def on_keyboard_event(event):
    amount = 10
    if event.type == "KEYBOARD_PRESS":
        if event.key == 'W':
            move_prim(prim, 'forward', amount)
        elif event.key == 'S':
            move_prim(prim, 'backward', amount)
        elif event.key == 'A':
            move_prim(prim, 'left', amount)
        elif event.key == 'D':
            move_prim(prim, 'right', amount)
        elif event.key == 'Q':
            move_prim(prim, 'up', amount)
        elif event.key == 'E':
            move_prim(prim, 'down', amount)

# Register the keyboard event handler
input = omni.kit.app.get_app().get_extension_manager().get_extension("omni.kit.editor.core").get_input_manager()
input.add_keyboard_event_handler(on_keyboard_event)

# Main loop to keep the script running
while True:
    omni.kit.app.get_app().update()

# Cleanup
omni.kit.app.get_app().shutdown()
