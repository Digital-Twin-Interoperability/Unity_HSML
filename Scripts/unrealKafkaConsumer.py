from omni.kit.scripting import BehaviorScript
from pxr import Gf, UsdGeom, Usd
import omni.usd
from confluent_kafka import Consumer, KafkaException
import json

# Kafka Consumer configuration
consumer_conf = {
    'bootstrap.servers': '192.168.50.133:9092',  # Kafka server IP
    'group.id': 'omni-sim-consumer-cadre',
    'auto.offset.reset': 'earliest',  # Start from the earliest message if no offset is stored
}

# Create Kafka consumer
consumer = Consumer(consumer_conf)

# Kafka topic name
kafka_topic = 'unreal-hsml-topic'

# Subscribe to Kafka topic
consumer.subscribe([kafka_topic])

# Isaac Sim Class
class OmniControls(BehaviorScript):
    def on_init(self):
        print("CONSUMER SCRIPT INIT")
        stage = omni.usd.get_context().get_stage()
        # List of prims to control
        self.prims = {
            "Chassis": stage.GetPrimAtPath("/World/Unreal_Cadre/CADRE_4WD_Controllable/NewCADRE/CADRE_Textured_1/CADRE_Chasis_1/CADRE_Chasis/Body1/Body1"),
        }

    def on_play(self):
        print("CONSUMER SCRIPT PLAY")
        # Verify that all prims exist in the stage
        for prim_name, prim in self.prims.items():
            if not prim.IsValid():
                print(f"Error: Prim '{prim_name}' not found at path.")
            else:
                print(f"Tracking prim: {prim_name} at path {prim.GetPath()}")

    def move_prim(self, prim, x, y, z, rx, ry, rz, w):
        """
        Move the given prim to the specified world position and orientation.
        """
        if prim.IsValid():
            # Set position
            prim_xform = UsdGeom.Xformable(prim)
            transform_matrix = Gf.Matrix4d()
            transform_matrix.SetTranslate(Gf.Vec3d(x, y, z))

            # Set rotation using quaternion
            rotation_quat = Gf.Quatd(w, rx, ry, rz)
            rotation_matrix = Gf.Matrix4d()
            rotation_matrix.SetRotate(rotation_quat)

            # Combine position and rotation matrices
            final_matrix = transform_matrix * rotation_matrix
            prim_xform.SetLocalTransformation(final_matrix)

    def on_update(self, current_time: float, delta_time: float):
        try:
            # Poll for Kafka messages
            msg = consumer.poll(0.1)  # Timeout in seconds

            if msg is None:
                return  # No new message

            if msg.error():
                raise KafkaException(msg.error())

            # Parse the incoming message
            message = json.loads(msg.value().decode('utf-8'))
            model_name = message.get("name", "")
            position = {prop["name"]: prop["value"] for prop in message.get("additionalProperty", [])}

            # Extract position and quaternion values
            x, y, z = position.get("xCoordinate", 0), position.get("yCoordinate", 0), position.get("zCoordinate", 0)
            rx, ry, rz, w = position.get("rx", 0), position.get("ry", 0), position.get("rz", 0), position.get("w", 1)

            # Find the prim and move it
            if model_name in self.prims:
                self.move_prim(self.prims[model_name], x, y, z, rx, ry, rz, w)
                print(f"Moved prim '{model_name}' to position ({x}, {y}, {z}) with rotation ({rx}, {ry}, {rz}, {w})")
            else:
                print(f"Warning: No prim found with name '{model_name}'")

        except Exception as e:
            print(f"Error processing Kafka message: {e}")

    def on_shutdown(self):
        print("CONSUMER SCRIPT SHUTDOWN")
        consumer.close()  # Clean up Kafka consumer
