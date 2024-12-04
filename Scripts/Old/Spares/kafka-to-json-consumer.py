import json
import os
import time
from confluent_kafka import Consumer, KafkaException, KafkaError

# Kafka configuration
kafka_broker = '192.168.50.133:9092'
topic = 'unity-hsml-topic'
group_id = 'my_consumer_group'

# File path to write the JSON data
file_path = r'C:\Users\Jared\Desktop\kafkaOmni.json'

# Create Consumer instance with updated offset reset to 'latest'
consumer = Consumer({
    'bootstrap.servers': kafka_broker,
    'group.id': group_id,
    'auto.offset.reset': 'latest'  # Only consume the most recent messages
})

# Function to write data to a JSON file
def write_to_file(data):
    try:
        # Open the file in write mode to overwrite with the latest data
        with open(file_path, 'w') as f:
            json.dump(data, f, indent=4)
        print("Data written to:", file_path)
    except Exception as e:
        print(f"Error writing to file: {e}")

# Subscribe to Kafka topic
consumer.subscribe([topic])

# Continuously consume messages from Kafka and write to file when data changes
try:
    while True:
        # Poll for new messages
        msg = consumer.poll(timeout=1.0)  # Timeout in seconds

        if msg is None:
            # No new message, continue polling
            continue
        elif msg.error():
            # Handle Kafka error
            if msg.error().code() == KafkaError._PARTITION_EOF:
                continue
            else:
                raise KafkaException(msg.error())
        else:
            # New message received
            print("Received message: ", msg.value().decode('utf-8'))
            
            # Parse the message value (assuming it's JSON)
            try:
                data = json.loads(msg.value().decode('utf-8'))
                write_to_file(data)
            except json.JSONDecodeError as e:
                print(f"Error decoding JSON: {e}")
        
        time.sleep(1)  # Optional: Add a small delay to avoid high CPU usage

except KeyboardInterrupt:
    print("Consumer interrupted, closing...")
finally:
    # Close the consumer gracefully
    consumer.close()
