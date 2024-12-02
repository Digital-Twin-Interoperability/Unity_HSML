from omni.kit.scripting import BehaviorScript
from pxr import Gf, UsdGeom, Usd
import time
import socket
import omni.usd
import numpy as np
from scipy.spatial.transform import Rotation as R
import re

def parse_message(message):
    data = message.split(',')
    print("Parsed data:", data)
    if len(data) != 8:
        raise ValueError(f"Expected 8 values in the message, but got {len(data)}")
    x, y, z = float(data[1]), float(data[2]), float(data[3])
    rx, ry, rz, w = float(data[4]), float(data[5]), float(data[6]), float(data[7])
    return Gf.Vec3d(x, y, z), Gf.Quatf(w, rx, ry, rz)

def recorder():
    global cleanedTransform2, cleanedTransform3, clientsocket
    print(f"Connection from {address} has been established!")
    clientsocket.send(bytes(f"{cleanedTransform2,cleanedTransform3}", "utf-8"))
    # Prepare data for JSON serialization
    '''transform_data = {
        "transform2": cleaned_transform2,
        "transform3": cleaned_transform3,
        "transform4": cleaned_transform4,
        "transform5": cleaned_transform5
    }
    
    # Serialize to JSON and send
    json_data = json.dumps(transform_data)
    clientsocket.send(bytes(json_data, "utf-8"))'''

    message = clientsocket.recv(1024)
    message_str = message.decode("utf-8")
    print("Received message from client:", message_str)
    return message_str

def get_transform(prim):
    matrix: Gf.Matrix4d = omni.usd.get_world_transform_matrix(prim)
    translate: Gf.Vec3d = matrix.ExtractTranslation()
    rotationBot: Gf.Rotation = matrix.ExtractRotation()
    return (translate, rotationBot)

def clean_transform(transform):
    # Convert the transform (tuple) to a string
    transform_str = str(transform)
    # Remove specific phrases
    transform_str = transform_str.replace("Gf.Vec3d", "").replace("Gf.Rotation", "").replace("))", ")")
    # Use regex to remove anything that isn't a number, comma, or period
    cleaned_str = re.sub(r'[^\d.,-]', '', transform_str)
    return cleaned_str

class OmniControls(BehaviorScript):
    def on_init(self):
        global prim1, prim2, prim3, prim4, prim5
        timeline_stream = self.timeline.get_timeline_event_stream()
        print("CONTROLS TEST INIT")

        stage = omni.usd.get_context().get_stage()
        #Viper
        prim1 = UsdGeom.Xform(stage.GetPrimAtPath("/World/Viper"))
        #Cadre
        prim2 = stage.GetPrimAtPath("/World/Cadre/CADRE_4WD_Controllable/NewCADRE/CADRE_Textured_1/CADRE_Chasis_1/CADRE_Chasis/Body1/Body1")
        #Rocks
        prim3 = stage.GetPrimAtPath("/World/Rocks/World/Anorthosite_Rock__1_meter__1/Anorthosite_Rock__1_meter__1")
        prim4 = stage.GetPrimAtPath("/World/Rocks/World/Anorthosite_Rock__1_meter_001_1/Anorthosite_Rock__1_meter_001_1")
        prim5 = stage.GetPrimAtPath("/World/Rocks/World/Anorthosite_Rock__1_meter_002_1/Anorthosite_Rock__1_meter_002_1")

    def on_destroy(self):
        print(f"{__class__.__name__}.on_destroy()->{self.prim_path}")

    def on_play(self):
        global start_t, clientsocket, address
        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        s.bind((socket.gethostname(), 1234))
        s.listen(5)
        clientsocket, address = s.accept()

        print("CONTROLS TEST PLAY")
        self.Flask = False
        self.roll = 0
        start_t = time.time()

    def on_pause(self):
        print("CONTROLS TEST PAUSE")

    def on_stop(self):
        print("CONTROLS TEST END")

    def on_update(self, current_time: float, delta_time: float):
        global cleanedTransform2, cleanedTransform3, prim2, prim3

        cleanedTransform2 = clean_transform(get_transform(prim2))
        cleanedTransform3 = clean_transform(get_transform(prim3))

        
        print("World Position Cadre:", cleanedTransform2)
        print("World Position Rock1:", cleanedTransform3)
 

        message_str = recorder()

        try:
            position, rotation = parse_message(message_str)

            xform = UsdGeom.Xformable(prim1)
            transform_ops = xform.GetOrderedXformOps()

            if not transform_ops:
                xform.AddTranslateOp().Set(position)
                xform.AddOrientOp().Set(Gf.Quatf(rotation.GetReal(), rotation.GetImaginary()))
            else:
                for op in transform_ops:
                    if op.GetOpType() == UsdGeom.XformOp.TypeTranslate:
                        op.Set(position)
                    elif op.GetOpType() == UsdGeom.XformOp.TypeOrient:
                        op.Set(Gf.Quatf(rotation.GetReal(), rotation.GetImaginary()))
        except ValueError as e:
            print(f"Error parsing message: {e}")
        except Exception as e:
            print(f"Unexpected error: {e}")


