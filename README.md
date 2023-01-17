# Note: there are two background services running at container
  1. SampleService retrieves twitter sample stream, and publishs with Kafka topic.\
  2. ProcessService retrieves sample stream with SampleService Kafka topic and process the data, then publish the process data with another Kafka topic.\
  3. AppConsole retrieves the process data from Kafka topic.
  
# Prerequisites:
1. docker
2. .net6

# How to run the app
1. run powershell command: docker-compose up --build
2. run project AppConsole

