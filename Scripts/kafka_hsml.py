from omni.kit.scripting import BehaviorScript
from pxr import Gf, UsdGeom, Usd
import time
import omni.usd
from confluent_kafka import Producer
import json
from datetime import datetime  # Added for date handling

# Kafka Producer configuration
producer_conf = {
    'bootstrap.servers': '192.168.50.133:9092',  # Kafka server IP
    'client.id': 'isaac-sim-producer'
}

# Create Kafka producer
producer = Producer(producer_conf)

# Kafka topic name
kafka_topic = 'rover-hsml-data'

# Previous state for delta tracking for each prim
previous_states = {}

# Function to send initial full message (schema and all details)
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
        "dateModified": modifiedDate,  # Modified date now gets passed dynamically
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
        "description": "Initial rover data with full schema"
    }
    
    # Send the full message to Kafka
    message_json = json.dumps(hsml_message)
    producer.produce(kafka_topic, key=schema_id, value=message_json)
    producer.poll(0)
    print(f"Sent full message to Kafka for {modelName}: {message_json}")

# Function to send delta updates (only the changed fields)
def send_delta_update(schema_id, rover_name, x, y, z, rx, ry, rz, w, previous_state):
    delta_message = {
        "rover": rover_name,
        "delta": {}
    }

    # Compare current values with previous state, and only send the changed values
    if previous_state["x"] != x:
        delta_message["delta"]["xCoordinate"] = x
    if previous_state["y"] != y:
        delta_message["delta"]["yCoordinate"] = y
    if previous_state["z"] != z:
        delta_message["delta"]["zCoordinate"] = z
    if previous_state["rx"] != rx:
        delta_message["delta"]["rx"] = rx
    if previous_state["ry"] != ry:
        delta_message["delta"]["ry"] = ry
    if previous_state["rz"] != rz:
        delta_message["delta"]["rz"] = rz
    if previous_state["w"] != w:
        delta_message["delta"]["w"] = w

    # Update previous state
    previous_state.update({"x": x, "y": y, "z": z, "rx": rx, "ry": ry, "rz": rz, "w": w})
    
    # Only send the delta if there are changes
    if delta_message["delta"]:
        message_json = json.dumps(delta_message)
        producer.produce(kafka_topic, key=schema_id, value=message_json)
        producer.poll(0)
        print(f"Sent delta to Kafka for {rover_name}: {message_json}")
    else:
        print(f"No changes detected for {rover_name}, delta update not sent.")


# Isaac Sim Class
class OmniControls(BehaviorScript):
    def on_init(self):
        print("CONTROLS TEST INIT")
        stage = omni.usd.get_context().get_stage()
        # List of prims to track
        self.prims = [
            stage.GetPrimAtPath("/World/Rocks/World/Anorthosite_Rock__1_meter__1/Anorthosite_Rock__1_meter__1"),
            #stage.GetPrimAtPath("/World/Rocks/World/Anorthosite_Rock__1_meter_001_1/Anorthosite_Rock__1_meter_001_1")
        ]

    def on_play(self):
        print("CONTROLS TEST PLAY")
        self.first_update = {}
        for prim in self.prims:
            prim_name = prim.GetName()
            schema_id = f"schema_{prim_name}"  # Use the prim's name for schema_id
            self.first_update[prim_name] = True

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

            # Debugging: Print prim name and schema
            print(f"Tracking prim: {prim_name} with schema_id: {schema_id}")

    def get_transform(self, prim):
        matrix: Gf.Matrix4d = omni.usd.get_world_transform_matrix(prim)
        translate: Gf.Vec3d = matrix.ExtractTranslation()
        rotationBot: Gf.Rotation = matrix.ExtractRotation()
        rotation_quaternion = rotationBot.GetQuaternion()
        return {
            "translate": translate,
            "rotation": {
                "rx": rotation_quaternion.GetReal(),
                "ry": rotation_quaternion.GetImaginary()[0],
                "rz": rotation_quaternion.GetImaginary()[1],
                "w": rotation_quaternion.GetImaginary()[2]
            }
        }

    def on_update(self, current_time: float, delta_time: float):
        global previous_states

        # Iterate over all prims and send updates for each one
        for prim in self.prims:
            prim_name = prim.GetName()
            transform = self.get_transform(prim)
            x, y, z = transform["translate"][0], transform["translate"][1], transform["translate"][2]
            rx, ry, rz, w = transform["rotation"]["rx"], transform["rotation"]["ry"], transform["rotation"]["rz"], transform["rotation"]["w"]

            # Generate unique schema_id, modelName, and other details for each prim
            model_name = prim_name
            model_number = "001"
            object_link = f"https://example.com/models/{prim_name}-001"
            creator_name = "Jared Carrillo"
            creation_date = "2024-10-02"
            modified_date = datetime.now().strftime('%Y-%m-%d')  # Dynamic current date

            # Send full message on the first update for each prim
            if self.first_update[prim_name]:
                send_full_message(f"schema_{prim_name}", model_name, model_number, object_link, creator_name, creation_date, modified_date, x, y, z, rx, ry, rz, w)
                self.first_update[prim_name] = False
            else:
                # Send delta update after the first message
                send_delta_update(f"schema_{prim_name}", model_name, x, y, z, rx, ry, rz, w, previous_states[prim_name])
