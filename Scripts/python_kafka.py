from confluent_kafka import Producer
import socket
import time

# Kafka configuration
conf = {
    'bootstrap.servers': '192.168.50.133:9092',  # Your Kafka server IP
    'client.id': socket.gethostname()
}

# Create Producer instance
producer = Producer(conf)

# Callback function for delivery reports
def delivery_report(err, msg):
    if err is not None:
        print(f"Message delivery failed: {err}")
    else:
        print(f"Message delivered to {msg.topic()} [{msg.partition()}]")

# Topic name
topic = 'unity-data'

try:
    message_count = 0
    while True:
        # Generate message
        message = f"Message number {message_count} from Python"
        
        # Produce a message to the 'unity-data' topic
        producer.produce(topic, value=message, callback=delivery_report)
        
        # Wait for any outstanding messages to be delivered
        producer.poll(0)
        
        # Log the message number
        print(f"Sent: {message}")
        
        # Increment message count
        message_count += 1
        
        # Wait for 1 second before sending the next message
        time.sleep(1)
finally:
    # Wait for all messages to be delivered
    producer.flush()
