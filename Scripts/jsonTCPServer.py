from omni.kit.scripting import BehaviorScript
from pxr import Gf, UsdGeom, Usd
import time
import socket
import omni.usd
import json
import re

def recorder():
    global cleanedTransform2, cleanedTransform3, cleanedTransform4, cleanedTransform5, clientsocket
    # Prepare data for JSON serialization
    transform_data = {
        "cadrePrim": cleanedTransform2,
        "Rock1Prim": cleanedTransform3,
        "Rock2Prim": cleanedTransform4,
        "Rock3Prim": cleanedTransform5
    }
    
    # Serialize to JSON and send
    json_data = json.dumps(transform_data)
    clientsocket.send(bytes(json_data, "utf-8"))

    message = clientsocket.recv(2048)
    message_str = message.decode("utf-8")
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

def parse_position_rotation(data):
    x, y, z = float(data['position']['x']), float(data['position']['y']), float(data['position']['z'])
    rx, ry, rz, w = float(data['rotation']['x']), float(data['rotation']['y']), float(data['rotation']['z']), float(data['rotation']['w'])
    return Gf.Vec3d(x, y, z), Gf.Quatf(w, rx, ry, rz)

class OmniControls(BehaviorScript):
    def on_init(self):
        global prim1, prim2, prim3, prim4, prim5, prim6, prim7, prim8, prim9, prim10, prim11
        timeline_stream = self.timeline.get_timeline_event_stream()
        print("CONTROLS TEST INIT")

        stage = omni.usd.get_context().get_stage()
        # Viper and wheels
        prim1 = UsdGeom.Xform(stage.GetPrimAtPath("/World/Viper"))

        prim8 = UsdGeom.Xform(stage.GetPrimAtPath("/World/Viper/Xform/frontRight"))
        prim9 = UsdGeom.Xform(stage.GetPrimAtPath("/World/Viper/Xform/backRight"))
        prim10 = UsdGeom.Xform(stage.GetPrimAtPath("/World/Viper/Xform/frontLeft"))
        prim11 = UsdGeom.Xform(stage.GetPrimAtPath("/World/Viper/Xform/backLeft"))

        # Chand Rover and Lander
        prim6 = UsdGeom.Xform(stage.GetPrimAtPath("/World/Chandrayaan_Rover"))
        prim7 = UsdGeom.Xform(stage.GetPrimAtPath("/World/Chandrayaan_3_Lander"))
        # Cadre
        prim2 = stage.GetPrimAtPath("/World/Cadre/CADRE_4WD_Controllable/NewCADRE/CADRE_Textured_1/CADRE_Chasis_1/CADRE_Chasis/Body1/Body1")
        # Rocks
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
        global cleanedTransform2, cleanedTransform3, cleanedTransform4, cleanedTransform5

        cleanedTransform2 = clean_transform(get_transform(prim2))
        cleanedTransform3 = clean_transform(get_transform(prim3))
        cleanedTransform4 = clean_transform(get_transform(prim4))
        cleanedTransform5 = clean_transform(get_transform(prim5))

        message_str = recorder()

        try:
            # Parse the JSON message
            message_data = json.loads(message_str)
            print("Received JSON data:", json.dumps(message_data, indent=4))

            # Update prims based on JSON data
            self.update_prims(message_data)

        except json.JSONDecodeError as e:
            print(f"Error parsing JSON message: {e}")
        except Exception as e:
            print(f"Unexpected error: {e}")

    def update_prims(self, message_data):
        # Example mapping: you can change this mapping based on your data structure
        data_mapping = {
            "Viper": prim1,
            "ChandRover": prim6,
            "ChandLander": prim7,
        }


        for key, prim in data_mapping.items():
            if key in message_data:
                try:
                    position, rotation = parse_position_rotation(message_data[key])
                    xform = UsdGeom.Xformable(prim)
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
                    print(f"Error parsing message for {key}: {e}")
                except Exception as e:
                    print(f"Unexpected error for {key}: {e}")