from omni.kit.scripting import BehaviorScript
from pxr import Gf, Usd
import math
import omni.usd
import omni.ui as ui

global first_time, position, rotation
first_time = True

# speed parameters
min_dist = 50 # distance from endpoint before moving to next one
max_speed = 200 # max linear speed
min_speed = 50 # min linear speed
max_turn = 100 # max turn speed
min_turn = 50 # min turn speed
max_deviation = 10 # max angle deviation current to target

class WheelTest(BehaviorScript):
    def on_init(self):
        print(f"{__class__.__name__}.on_init()->{self.prim_path}")
        print("QINFO: INITIALIZE ---------------------------------------")
        global position, rotation

        self._prim = self.stage.GetPrimAtPath(self.prim_path)
        self._wheelFR = self.stage.GetPrimAtPath('/World/Viper_Substance_File__1_/Body1_1/Body1_1/RevoluteJoint')#FR
        self._wheelBR = self.stage.GetPrimAtPath('/World/Viper_Substance_File__1_/Body1/Body1/RevoluteJoint')#BR
        self._wheelFL = self.stage.GetPrimAtPath('/World/Viper_Substance_File__1_/Body1_2/Body1_2/RevoluteJoint')#FL
        self._wheelBL = self.stage.GetPrimAtPath('/World/Viper_Substance_File__1_/Body1_3/Body1_3/RevoluteJoint')#BL
        self._chassis = self.stage.GetPrimAtPath('/World/Viper_Substance_File__1_/Body1_4/Body1_4')

        # Register key events
        ui.Workspace.set_keyboard_events_enabled(True)
        ui.Workspace.get_window_event_stream().sub(self.handle_keyboard_event)

    def on_destroy(self):
        print("QINFO: DESTROY -----------------------------------")

    def on_play(self):
        print(f"{__class__.__name__}.on_play()->{self.prim_path}")
        print("QINFO: PLAY ---------------------------------------")

        global position, rotation, first_time
        if first_time == True:
            world_transform = omni.usd.get_world_transform_matrix(self._chassis)
            position = list(world_transform.ExtractTranslation())
            rotation = self._chassis.GetAttribute("xformOp:orient").Get()
            rotation = [rotation.GetReal(), rotation.GetImaginary()[0], rotation.GetImaginary()[1], rotation.GetImaginary()[2]]

            first_time = False

        self.past_typ = '-1'

    def on_pause(self):
        print("QINFO: PAUSE ---------------------------------------")

    def on_stop(self):
        print("QINFO: STOP ---------------------------------------")
        global first_time
        first_time = True

    def on_update(self, current_time: float, delta_time: float):
        print("QINFO: UPDATE ---------------------------------------")
        global position, rotation

        world_transform = omni.usd.get_world_transform_matrix(self._chassis)
        position = list(world_transform.ExtractTranslation())
        rotation = self._chassis.GetAttribute("xformOp:orient").Get()
        rotation = [rotation.GetReal(), rotation.GetImaginary()[0], rotation.GetImaginary()[1], rotation.GetImaginary()[2]]

        print(position)
        extension_record(self)

    def handle_keyboard_event(self, event):
        thrust_0 = 0
        thrust_1 = 0

        if event.type == ui.WindowEventType.KEY_DOWN:
            if event.key == ui.KeyboardKey.UP:
                thrust_0 = max_speed
                thrust_1 = max_speed
            if event.key == ui.KeyboardKey.DOWN:
                thrust_0 = -max_speed
                thrust_1 = -max_speed
            if event.key == ui.KeyboardKey.LEFT:
                thrust_0 = -max_turn
                thrust_1 = max_turn
            if event.key == ui.KeyboardKey.RIGHT:
                thrust_0 = max_turn
                thrust_1 = -max_turn

        self._wheelFR.GetAttribute("drive:angular:physics:targetVelocity").Set(thrust_1)
        self._wheelBR.GetAttribute("drive:angular:physics:targetVelocity").Set(thrust_1)
        self._wheelFL.GetAttribute("drive:angular:physics:targetVelocity").Set(thrust_0)
        self._wheelBL.GetAttribute("drive:angular:physics:targetVelocity").Set(thrust_0)

def extension_record(self):
    print("QINFO: RECORD ---------------------------------------")
    cur_pos = list(self._chassis.GetAttribute("xformOp:translate").Get())
    world_transform = omni.usd.get_world_transform_matrix(self._chassis)
    rotation = world_transform.ExtractRotation()
    rotation = rotation.GetQuaternion()
    rotation = [rotation.GetReal(), rotation.GetImaginary()[0], rotation.GetImaginary()[1], rotation.GetImaginary()[2]]

    cur_heading, roll_angle = rotation_about_z_axis(rotation)

    self.past_pos = cur_pos

def quaternion_to_rotation_matrix(w, x, y, z):
    r11 = 1 - 2*y*y - 2*z*z
    r12 = 2*x*y + 2*w*z
    r13 = 2*x*z - 2*w*y

    r21 = 2*x*y - 2*w*z
    r22 = 1 - 2*x*x - 2*z*z
    r23 = 2*y*z + 2*w*x

    r31 = 2*x*z + 2*w*y
    r32 = 2*y*z - 2*w*x
    r33 = 1 - 2*x*x - 2*y*y

    return [[r11, r12, r13], [r21, r22, r23], [r31, r32, r33]]

def rotation_about_z_axis(quaternion):
    w, x, y, z = quaternion

    yaw_rad = math.atan2(2 * (w * z + x * y), 1 - 2 * (y * y + z * z))
    yaw_deg = math.degrees(yaw_rad)

    while yaw_deg > 180:
        yaw_deg -= 360
    while yaw_deg < -180:
        yaw_deg += 360

    roll_rad = math.atan2(2 * (w * x + y * z), 1 - 2 * (x * x + y * y))
    roll_deg = math.degrees(roll_rad)

    return yaw_deg, roll_deg

def angle_about_z_axis(point1, point2):
    directional_vector = [point2[0] - point1[0], point2[1] - point1[1]]

    angle_rad = math.atan2(-directional_vector[0], directional_vector[1])
    angle_deg = math.degrees(angle_rad)
    angle_deg += 90

    if angle_deg > 180:
        angle_deg -= 360
    elif angle_deg < -180:
        angle_deg += 360
    
    return angle_deg
