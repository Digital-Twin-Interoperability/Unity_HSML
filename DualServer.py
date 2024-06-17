from omni.kit.scripting import BehaviorScript
from pxr import Gf, UsdGeom, Usd
import time
import socket
import omni.usd

def parse_message(message):
    # Split the message by comma and discard the first value (title)
    data = message.split(',')
    print("Parsed data:", data)
    if len(data) != 8:
        raise ValueError(f"Expected 8 values in the message, but got {len(data)}")
    # Convert the remaining values to floats
    x, y, z = float(data[1]), float(data[2]), float(data[3])
    rx, ry, rz, w = float(data[4]), float(data[5]), float(data[6]), float(data[7])
    return Gf.Vec3d(x, y, z), Gf.Quatf(w, rx, ry, rz)

def recorder():
    global finalTransform1, finalRotation1, finalTransform2, finalRotation2, clientsocket

    print(f"Connection from {address} has been established!")
    clientsocket.send(bytes(f"{finalTransform1, finalRotation1}", "utf-8"))

    message = clientsocket.recv(1024)  # Buffer size is 1024 bytes
    message_str = message.decode("utf-8")
    print("Received message from client:", message_str)
    return message_str

class OmniControls(BehaviorScript):
    def on_init(self):
        global prim, prim2
        timeline_stream = self.timeline.get_timeline_event_stream()
        print("CONTROLS TEST INIT")

        stage = omni.usd.get_context().get_stage()
        prim = stage.GetPrimAtPath("/World/CADRE_Demo/Chassis")
        prim2 = UsdGeom.Xform(stage.GetPrimAtPath("/World/CADRE_2"))

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
        global finalTransform1, finalRotation1, finalTransform2, finalRotation2

        matrix: Gf.Matrix4d = omni.usd.get_world_transform_matrix(prim)
        translate: Gf.Vec3d = matrix.ExtractTranslation()
        rotationBot1: Gf.Rotation = matrix.ExtractRotation()

        finalTransform1 = str(translate)
        finalRotation1 = str(rotationBot1)

        print("World Position 1:", finalTransform1)
        print("World Rotation 1:", finalRotation1)

        message_str = recorder()

        try:
            # Parse the message to get position and rotation
            position, rotation = parse_message(message_str)

            # Apply the translation and rotation to prim2
            xform = UsdGeom.Xformable(prim2)
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
