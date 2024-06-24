from omni.kit.scripting import BehaviorScript
from pxr import Gf, UsdGeom, Usd
import time
import socket
import omni.usd
import numpy as np
from scipy.spatial.transform import Rotation as R

def parse_message(message):
    data = message.split(',')
    print("Parsed data:", data)
    if len(data) != 8:
        raise ValueError(f"Expected 8 values in the message, but got {len(data)}")
    x, y, z = float(data[1]), float(data[2]), float(data[3])
    rx, ry, rz, w = float(data[4]), float(data[5]), float(data[6]), float(data[7])
    return Gf.Vec3d(x, y, z), Gf.Quatf(w, rx, ry, rz)

def recorder():
    global finalTransform1, finalRotation1, clientsocket
    print(f"Connection from {address} has been established!")
    clientsocket.send(bytes("Hello", "utf-8"))

    message = clientsocket.recv(1024)
    message_str = message.decode("utf-8")
    print("Received message from client:", message_str)
    return message_str

 
class OmniControls(BehaviorScript):
    def on_init(self):
        global prim, prim2, prim3
        timeline_stream = self.timeline.get_timeline_event_stream()
        print("CONTROLS TEST INIT")

        stage = omni.usd.get_context().get_stage()
        prim2 = UsdGeom.Xform(stage.GetPrimAtPath("/World/Chandrayaan_3_Lander"))

    def on_destroy(self):
        print(f"{__class__.__name__}.on_destroy()->{self.prim_path}")

    def on_play(self):
        global start_t, clientsocket, address
        #s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        #s.bind((socket.gethostname(), 1234))
        #s.listen(5)
        #clientsocket, address = s.accept()

        print("CONTROLS TEST PLAY")
        self.Flask = False
        self.roll = 0
        start_t = time.time()

    def on_pause(self):
        print("CONTROLS TEST PAUSE")

    def on_stop(self):
        print("CONTROLS TEST END")

    def on_update(self, current_time: float, delta_time: float):
        global finalTransform1, finalRotation1

        '''message_str = recorder()

        try:
            position, rotation = parse_message(message_str)

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
            print(f"Unexpected error: {e}")'''


