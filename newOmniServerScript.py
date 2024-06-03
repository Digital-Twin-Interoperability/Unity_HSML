from omni.kit.scripting import BehaviorScript
from pxr import Gf, UsdGeom, Usd
import math
import time
import socket
import pickle

#position, orientation = prim.get_world_pose()



def quaternion_to_euler(q):
    w, x, y, z = q

    roll = math.atan2(2 * (w * x + y * z), 1 - 2 * (x**2 + y**2))
    pitch = math.asin(2 * (w * y - z * x))
    yaw = math.atan2(2 * (w * z + x * y), 1 - 2 * (y**2 + z**2))

    return roll, pitch, yaw

def recorder():
    global position1, rotation1, position2, rotation2, server_socket, address, clientsocket

    print(f"Connection from {address} has been established!")
    clientsocket.send(bytes(f"{position1[0], position1[1], position1[2], rotation1[0], rotation1[1], rotation1[2], rotation1[3], position2[0], position2[1], position2[2], rotation2[0], rotation2[1], rotation2[2], rotation2[3]}", "utf-8"))

class OmniControls(BehaviorScript):
    def on_init(self):
        # print(f"{__class__.__name__}.on_init()->{self.prim_path}")
        timeline_stream = self.timeline.get_timeline_event_stream()
        print("CONTROLS TEST INIT")
        self._prim1 = self.stage.GetPrimAtPath("/World/CADRE_Demo/Chassis")
        self._prim2 = self.stage.GetPrimAtPath("/World/CADRE_2/Chassis")

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


        position3, orientation3 = self.get_world_pose(self._prim1)


        print("World Position:", position3)

        #recorder()
