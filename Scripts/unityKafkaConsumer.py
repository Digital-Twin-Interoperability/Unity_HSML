from omni.kit.scripting import BehaviorScript
from pxr import Gf, UsdGeom, Usd
import omni.usd
from confluent_kafka import Consumer, KafkaException, KafkaError
import json
from datetime import datetime  # Added for date handling

# Kafka Consumer configuration
consumer_config = {
    'bootstrap.servers': '192.168.50.133:9092',  # Kafka broker address
    'group.id': 'unity-hsml-consumer-group',     # Consumer group ID
    'auto.offset.reset': 'earliest',            # Start reading at the earliest message
}

# Create Consumer instance
consumer = Consumer(consumer_config)

# Subscribe to the topic
topic = 'unity-hsml-topic'


print(f"Listening to messages on topic '{topic}'...")


# Isaac Sim Class
class OmniControls(BehaviorScript):
    def on_init(self):
        print("CONTROLS TEST INIT")
        stage = omni.usd.get_context().get_stage()

    def on_play(self):
        print("CONTROLS TEST PLAY")
        consumer.subscribe([topic])

    def on_update(self, current_time: float, delta_time: float):
        try:
            # Poll for new messages
            msg = consumer.poll(0.1)  # Poll timeout in seconds

            if msg is None:
                return  # No new message, continue the simulation

            if msg.error():
                # Handle Kafka errors
                if msg.error().code() == KafkaError._PARTITION_EOF:
                    # End of partition event
                    print(f"Reached end of partition: {msg.topic()}[{msg.partition()}] at offset {msg.offset()}")
                else:
                    raise KafkaException(msg.error())
            else:
                # Print the message key and value
                print(f"Received message: {msg.value().decode('utf-8')} (key: {msg.key()})")

        except Exception as e:
            print(f"Error while consuming Kafka messages: {e}")

    def on_stop(self):
        print("Stopping Kafka Consumer and cleaning up...")
        consumer.close()  # Close the consumer when the simulation stops 