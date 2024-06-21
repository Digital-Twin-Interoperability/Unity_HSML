from omni.kit.scripting import BehaviorScript
from pxr import Gf, UsdGeom, Usd
import time
import socket
import omni.usd
import numpy as np
from scipy.spatial.transform import Rotation as R

def recorder():
    global finalTransform1, finalRotation1, clientsocket

    print(f"Connection from {address} has been established!")
    clientsocket.send(bytes(f"{finalTransform1, finalRotation1}", "utf-8"))

 
class OmniControls(BehaviorScript):
    def on_init(self):
        global prim, prim2, prim3
        timeline_stream = self.timeline.get_timeline_event_stream()
        print("CONTROLS TEST INIT")

        stage = omni.usd.get_context().get_stage()
        prim = stage.GetPrimAtPath("/World/rover_v1/rover_v1/Body1/Body1")

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
        global finalTransform1, finalRotation1

        matrix: Gf.Matrix4d = omni.usd.get_world_transform_matrix(prim)
        translate: Gf.Vec3d = matrix.ExtractTranslation()
        rotationBot1: Gf.Rotation = matrix.ExtractRotation()
        finalTransform1 = str(translate)
        finalRotation1 = str(rotationBot1)
        print("World Position 1:", finalTransform1)
        print("World Rotation 1:", finalRotation1)

        recorder()


