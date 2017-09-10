run the RabbitMQ container

docker run -d --hostname my-rabbit --name rabbitMQ -p 15672:15672 -p 5672:5672 rabbitmq:3-management
or (if was already ran before)
docker restart rabbitMQ 

browse to http://localhost:15672 to see the management console