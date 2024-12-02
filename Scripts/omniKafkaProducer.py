from omni.kit.scripting import BehaviorScript
from pxr import Gf, UsdGeom, Usd
import omni.usd
from confluent_kafka import Producer
import json
from datetime import datetime  # Added for date handling

# Kafka Producer configuration
producer_conf = {
    'bootstrap.servers': '192.168.50.133:9092',  # Kafka server IP
    'client.id': 'omni-sim-producer'
}

# Create Kafka producer
producer = Producer(producer_conf)

# Kafka topic name
kafka_topic = 'omni-hsml-topic'

# Dictionary to store the previous state for all tracked prims
previous_states = {}

# Function to send full message
def send_full_message(schema_id, modelName, modelNumber, objectLink, creatorName, creationDate, modifiedDate, x, y, z, rx, ry, rz, w):
    hsml_message = {
        "@context": "https://schema.org",
        "@type": "3DModel",
        "name": modelName,
        "identifier": {
            "@type": "PropertyValue",
            "propertyID": schema_id,
            "value": f"{modelName}-{modelNumber}"
        },
        "url": objectLink,
        "creator": {
            "@type": "Person",
            "name": creatorName
        },
        "dateCreated": creationDate,
        "dateModified": modifiedDate,
        "encodingFormat": "application/x-obj",
        "contentUrl": "https://example.com/models/3dmodel-001.obj",
        "additionalType": "https://schema.org/CreativeWork",
        "additionalProperty": [
            {"@type": "PropertyValue", "name": "xCoordinate", "value": x},
            {"@type": "PropertyValue", "name": "yCoordinate", "value": y},
            {"@type": "PropertyValue", "name": "zCoordinate", "value": z},
            {"@type": "PropertyValue", "name": "rx", "value": rx},
            {"@type": "PropertyValue", "name": "ry", "value": ry},
            {"@type": "PropertyValue", "name": "rz", "value": rz},
            {"@type": "PropertyValue", "name": "w", "value": w}
        ],
        "description": "Rover data with world position and rotation"
    }

    # Send the full message to Kafka
    message_json = json.dumps(hsml_message)
    producer.produce(kafka_topic, key=schema_id, value=message_json)
    producer.poll(0)
    print(f"Sent message to Kafka for {modelName}: {message_json}")

# Isaac Sim Class
class OmniControls(BehaviorScript):
    def on_init(self):
        print("CONTROLS TEST INIT")
        stage = omni.usd.get_context().get_stage()
        # List of prims to track
        self.prims = [
            stage.GetPrimAtPath("/World/Omni_Cadre/CADRE_Demo/Chassis"),
        ]

    def on_play(self):
        print("CONTROLS TEST PLAY")
        for prim in self.prims:
            prim_name = prim.GetName()
            schema_id = f"schema_{prim_name}"  # Use the prim's name for schema_id

            # Initialize previous state for this prim
            previous_states[prim_name] = {
                "x": None,
                "y": None,
                "z": None,
                "rx": None,
                "ry": None,
                "rz": None,
                "w": None
            }

            print(f"Tracking prim: {prim_name} with schema_id: {schema_id}")

    def get_transform(self, prim):
        # Get the world transformation matrix
        matrix: Gf.Matrix4d = omni.usd.get_world_transform_matrix(prim)
        translate: Gf.Vec3d = matrix.ExtractTranslation()  # Absolute world position
        rotation: Gf.Rotation = matrix.ExtractRotation()  # Absolute world rotation
        rotation_quaternion = rotation.GetQuaternion()
        return {
            "translate": translate,
            "rotation": {
                "rx": rotation_quaternion.GetReal(),
                "ry": rotation_quaternion.GetImaginary()[0],
                "rz": rotation_quaternion.GetImaginary()[1],
                "w": rotation_quaternion.GetImaginary()[2]
            }
        }

    def has_state_changed(self, prim_name, x, y, z, rx, ry, rz, w):
        # Always check and update the state
        prev = previous_states[prim_name]
        has_changed = (prev["x"] != x or prev["y"] != y or prev["z"] != z or
                       prev["rx"] != rx or prev["ry"] != ry or prev["rz"] != rz or prev["w"] != w)

        if has_changed:
            # Update the previous state
            previous_states[prim_name] = {"x": x, "y": y, "z": z, "rx": rx, "ry": ry, "rz": rz, "w": w}

        return has_changed

    def on_update(self, current_time: float, delta_time: float):
        # Iterate over all prims and send updates if state has changed
        for prim in self.prims:
            prim_name = prim.GetName()
            schema_id = f"schema_{prim_name}"
            transform = self.get_transform(prim)
            x, y, z = transform["translate"][0], transform["translate"][1], transform["translate"][2]
            rx, ry, rz, w = (transform["rotation"]["rx"], transform["rotation"]["ry"],
                             transform["rotation"]["rz"], transform["rotation"]["w"])

            # Check for state change before sending a message
            if self.has_state_changed(prim_name, x, y, z, rx, ry, rz, w):
                send_full_message(
                    schema_id=schema_id,
                    modelName=prim_name,
                    modelNumber="001",
                    objectLink="https://example.com/3dmodel",
                    creatorName="Jared Carrillo",
                    creationDate="2024-01-01",
                    modifiedDate=datetime.now().strftime("%Y-%m-%dT%H:%M:%S"),
                    x=x, y=y, z=z, rx=rx, ry=ry, rz=rz, w=w
                )
