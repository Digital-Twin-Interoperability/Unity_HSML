from omni.kit.scripting import BehaviorScript
from pxr import Gf, UsdGeom, Usd
import math
import time
import socket
import pickle

# Test

s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.bind((socket.gethostname(), 1234))
s.listen(5)
clientsocket, address = s.accept()

def quaternion_to_euler(q):
    w, x, y, z = q

    roll = math.atan2(2 * (w * x + y * z), 1 - 2 * (x**2 + y**2))
    pitch = math.asin(2 * (w * y - z * x))
    yaw = math.atan2(2 * (w * z + x * y), 1 - 2 * (y**2 + z**2))

    return roll, pitch, yaw

def recorder():
    global position1, rotation1, position2, rotation2, server_socket

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
        global start_t, server_socket, client_sockets

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

        # Get world transform for the first prim
        xform1 = UsdGeom.Xformable(self._prim1)
        transform1 = xform1.ComputeLocalToWorldTransform(Usd.TimeCode.Default())
        position1 = transform1.ExtractTranslation()
        rotation_matrix1 = transform1.ExtractRotationMatrix()
        rotation_quat1 = Gf.Quatf(rotation_matrix1.GetQuat())
        rotation1 = [rotation_quat1.GetReal(), rotation_quat1.GetImaginary()[0], rotation_quat1.GetImaginary()[1], rotation_quat1.GetImaginary()[2]]
        roll1, pitch1, yaw1 = quaternion_to_euler(rotation1)

        # Get world transform for the second prim
        xform2 = UsdGeom.Xformable(self._prim2)
        transform2 = xform2.ComputeLocalToWorldTransform(Usd.TimeCode.Default())
        position2 = transform2.ExtractTranslation()
        rotation_matrix2 = transform2.ExtractRotationMatrix()
        rotation_quat2 = Gf.Quatf(rotation_matrix2.GetQuat())
        rotation2 = [rotation_quat2.GetReal(), rotation_quat2.GetImaginary()[0], rotation_quat2.GetImaginary()[1], rotation_quat2.GetImaginary()[2]]
        roll2, pitch2, yaw2 = quaternion_to_euler(rotation2)

        # Print world positions and rotations
        print("Prim 1 World Position and Rotation:")
        print("World Position:", position1)
        print("Roll:", roll1, "Pitch:", pitch1, "Yaw:", yaw1)
        
        print("Prim 2 World Position and Rotation:")
        print("World Position:", position2)
        print("Roll:", roll2, "Pitch:", pitch2, "Yaw:", yaw2)

        recorder()
