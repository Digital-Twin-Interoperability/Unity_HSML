from omni.kit.scripting import BehaviorScript
from pxr import Gf, UsdGeom, Usd
import math
import time
import socket
import omni.usd

def quaternion_to_euler(q):
    w, x, y, z = q

    roll = math.atan2(2 * (w * x + y * z), 1 - 2 * (x**2 + y**2))
    pitch = math.asin(2 * (w * y - z * x))
    yaw = math.atan2(2 * (w * z + x * y), 1 - 2 * (y**2 + z**2))

    return roll, pitch, yaw

def recorder():
    global finalTransform1, finalRotation1, finalTransform2, finalRotation2, clientsocket

    print(f"Connection from {address} has been established!")
    clientsocket.send(bytes(f"{finalTransform1, finalRotation1}", "utf-8"))

    message = clientsocket.recv(1024)  # Buffer size is 1024 bytes
    message_str = message.decode("utf-8")
    print("Received message from client:", message_str)

    # Parse the message
    parts = message_str.split(',')
    if len(parts) == 8 and parts[0] == "Rover1":
        try:
            x = float(parts[1])
            y = float(parts[2])
            z = float(parts[3])
            rx = float(parts[4])
            ry = float(parts[5])
            rz = float(parts[6])
            w = float(parts[7])
            transform_position_and_rotation(x, y, z, rx, ry, rz, w)
        except ValueError as e:
            print(f"Error parsing float values: {e}")
    else:
        print("Invalid message format")

def transform_position_and_rotation(x, y, z, rx, ry, rz, w):
    global prim2
    #stage = omni.usd.get_context().get_stage()
    #prim2 = stage.GetPrimAtPath("/World/CADRE_2/Chassis")

    # Set the new translation
    #xformable = UsdGeom.Xformable(prim2)
    #translate_op = xformable.GetOrderedXformOps()[0]
    #translate_op.Set(Gf.Vec3d(x, y, z))

    # Convert quaternion to euler
    #euler_rotation = quaternion_to_euler((w, rx, ry, rz))

    # Set the new rotation
    #rotation_op = xformable.GetOrderedXformOps()[1]
    #rotation_op.Set(Gf.Vec3d(math.degrees(euler_rotation[0]), math.degrees(euler_rotation[1]), math.degrees(euler_rotation[2])))

class OmniControls(BehaviorScript):
    def on_init(self):
        global prim, prim2
        timeline_stream = self.timeline.get_timeline_event_stream()
        print("CONTROLS TEST INIT")

        stage = omni.usd.get_context().get_stage()
        prim = stage.GetPrimAtPath("/World/CADRE_Demo/Chassis")
        prim2 = stage.GetPrimAtPath("/World/CADRE_2/Chassis")

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

        recorder()
