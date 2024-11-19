from omni.kit.scripting import BehaviorScript
from pxr import Gf, UsdGeom, Usd
import time
import omni.usd
import re
from confluent_kafka import Producer

# Kafka Producer configuration
producer_conf = {
    'bootstrap.servers': '192.168.50.133:9092',  # Kafka server IP
    'client.id': 'isaac-sim-producer'
}

# Create Kafka producer
producer = Producer(producer_conf)

# Kafka topic name
kafka_topic = 'isaac-sim-data'

# Function to convert quaternion (rotation) to HSML format
def quaternion_to_hsml(rotation_quaternion):
    return {
        "rx": rotation_quaternion.GetReal(),
        "ry": rotation_quaternion.GetImaginary()[0],
        "rz": rotation_quaternion.GetImaginary()[1],
        "w": rotation_quaternion.GetImaginary()[2]
    }

# Create HSML-compliant message for rock data
def create_hsml_message(translate, rotation_quaternion):
    hsml_message = {
        "@context": "https://schema.org",
        "@type": "3DModel",
        "name": {"@type": "Text", "value": "Anorthosite Rock"},
        "identifier": {
            "@type": "PropertyValue",
            "propertyID": {"@type": "Text", "value": "Anorthosite_Rock__1_meter__1"},
            "value": {"@type": "Text", "value": "Rock1"}
        },
        "creator": {
            "@type": "Person",
            "name": {"@type": "Text", "value": "Isaac Sim"}
        },
        "dateModified": {"@type": "Date", "value": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime())},
        "additionalProperty": [
            {"@type": "PropertyValue", "name": "xCoordinate", "value": {"@type": "Number", "value": translate[0]}},
            {"@type": "PropertyValue", "name": "yCoordinate", "value": {"@type": "Number", "value": translate[1]}},
            {"@type": "PropertyValue", "name": "zCoordinate", "value": {"@type": "Number", "value": translate[2]}},
            {"@type": "PropertyValue", "name": "rx", "value": {"@type": "Number", "value": rotation_quaternion["rx"]}},
            {"@type": "PropertyValue", "name": "ry", "value": {"@type": "Number", "value": rotation_quaternion["ry"]}},
            {"@type": "PropertyValue", "name": "rz", "value": {"@type": "Number", "value": rotation_quaternion["rz"]}},
            {"@type": "PropertyValue", "name": "w", "value": {"@type": "Number", "value": rotation_quaternion["w"]}}
        ],
        "description": {"@type": "Text", "value": "Rock's spatial data from Isaac Sim"}
    }
    return hsml_message

def recorder():
    global cleanedTransform3
    
    try:
        # Create HSML message for rock
        hsml_message = create_hsml_message(
            translate=cleanedTransform3["translate"],
            rotation_quaternion=cleanedTransform3["rotation"]
        )
        
        # Convert message to a string for Kafka
        hsml_message_str = str(hsml_message)
        
        # Send the message to Kafka
        producer.produce(kafka_topic, value=hsml_message_str)
        
        # Ensure the message is sent
        producer.poll(0)
        
        print(f"Sent to Kafka: {hsml_message_str}")
    except Exception as e:
        print(f"Error sending message to Kafka: {e}")

def get_transform(prim):
    matrix: Gf.Matrix4d = omni.usd.get_world_transform_matrix(prim)
    translate: Gf.Vec3d = matrix.ExtractTranslation()
    rotationBot: Gf.Rotation = matrix.ExtractRotation()
    rotation_quaternion = rotationBot.GetQuaternion()
    return {"translate": translate, "rotation": quaternion_to_hsml(rotation_quaternion)}

def clean_transform(transform):
    return transform

class OmniControls(BehaviorScript):
    def on_init(self):
        global prim3
        print("CONTROLS TEST INIT")
        stage = omni.usd.get_context().get_stage()
        # Rock
        prim3 = stage.GetPrimAtPath("/World/Rocks/World/Anorthosite_Rock__1_meter__1/Anorthosite_Rock__1_meter__1")
        
    def on_play(self):
        print("CONTROLS TEST PLAY")
        self.roll = 0

    def on_update(self, current_time: float, delta_time: float):
        global cleanedTransform3, prim3

        # Get the rock's position and rotation
        cleanedTransform3 = clean_transform(get_transform(prim3))
        
        print("World Position Rock1:", cleanedTransform3)

        # Record the transform by sending it to Kafka
        recorder()
