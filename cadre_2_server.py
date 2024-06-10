from omni.kit.scripting import BehaviorScript
from pxr import Gf, UsdGeom, Usd
import math
import time
import socket
import pickle

def quaternion_to_euler(q):
    w, x, y, z = q

    roll = math.atan2(2 * (w * x + y * z), 1 - 2 * (x**2 + y**2))
    pitch = math.asin(2 * (w * y - z * x))
    yaw = math.atan2(2 * (w * z + x * y), 1 - 2 * (y**2 + z**2))

    return roll, pitch, yaw

def recorder():
    global position1, rotation1, position2, rotation2, server_socket, address, clientsocket

    print(f"Connection from {address} has been established!")
    clientsocket.send(bytes(f"{position1,rotation1,position2,rotation2}", "utf-8"))

class OmniControls(BehaviorScript):
    def on_init(self):
        # print(f"{__class__.__name__}.on_init()->{self.prim_path}")
        timeline_stream = self.timeline.get_timeline_event_stream()
        print("CONTROLS TEST INIT")
        self._prim1 = self.stage.GetPrimAtPath("/World/Viper_Dynamic_Version_v4")
        self._prim2 = self.stage.GetPrimAtPath("/World/Cubert")

    def on_destroy(self):
        print(f"{__class__.__name__}.on_destroy()->{self.prim_path}")

    def on_play(self):
        global start_t, server_socket, client_sockets, address, clientsocket

        #s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        #s.bind((socket.gethostname(), 1234))
        #s.listen(5)
        #clientsocket, address = s.accept()

        # print(f"{__class__.__name__}.on_play()->{self.prim_path}")
        print("CONTROLS TEST PLAY")
        self.Flask = False
        self.roll = 0
        start_t = time.time()


    def on_pause(self):
        # print(f"{__class__.__name__}.on_pause()->{self.prim_path}")
        print("CONTROLS TEST PAUSE")

    def on_stop(self):
        # print(f"{__class__.__name__}.on_stop()->{self.prim_path}")
        print("CONTROLS TEST END")

    def on_update(self, current_time: float, delta_time: float):
        # print(f"{__class__.__name__}.on_update(current_time={current_time}, delta_time={delta_time})->{self.prim_path}")
        global position1, rotation1, position2, rotation2, server_socket, client_sockets

        # Get world position for the first prim
        xform1 = UsdGeom.Xformable(self._prim1)
        transform1 = xform1.ComputeLocalToWorldTransform(Usd.TimeCode.Default())
        position1 = transform1.ExtractTranslation()

        # Get values for the first prim
        rotate1 = self._prim1.GetAttribute("xformOp:orient").Get()
        rotation_r1, i1 = rotate1.GetReal(), rotate1.GetImaginary()
        rotation_i1 = list(i1)
        rotation1 = [rotation_r1, rotation_i1[0], rotation_i1[1], rotation_i1[2]]

        # Get world position for the second prim
        xform2 = UsdGeom.Xformable(self._prim2)
        transform2 = xform2.ComputeLocalToWorldTransform(Usd.TimeCode.Default())
        position2 = transform2.ExtractTranslation()

        # Get values for the second prim
        rotate2 = self._prim2.GetAttribute("xformOp:orient").Get()
        rotation_r2, i2 = rotate2.GetReal(), rotate2.GetImaginary()
        rotation_i2 = list(i2)
        rotation2 = [rotation_r2, rotation_i2[0], rotation_i2[1], rotation_i2[2]]

        position1 = str(position1)
        rotation1 = str(rotation1)
        position2 = str(position2)
        rotation2 = str(rotation2)

        # Print world positions and rotations
        print("Prim 1 World Position and Rotation:")
        print("World Position:", position1)
        
        print("Prim 2 World Position and Rotation:")
        print("World Position:", position2)

        #recorder()
