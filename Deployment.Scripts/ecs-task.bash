#!/bin/bash
CLUSTER_NAME="cool-ostrich-5ws40d"
SERVICE_NAME="micro-services-dotnet-service-i6yugo63"
TASK_DEF_FILE="task-definition.json"

# existing running tasks
echo "previous tasks..."
TASKS=$(aws ecs list-tasks \
  --cluster $CLUSTER_NAME \
  --service-name $SERVICE_NAME \
  --query "taskArns[]" \
  --output text)

# Register the task definition
TASK_DEF_ARN=$(aws ecs register-task-definition \
  --cli-input-json file://$TASK_DEF_FILE \
  --query "taskDefinition.taskDefinitionArn" \
  --output text)

echo "Registered Task Definition: $TASK_DEF_ARN"

# Update the ECS service
aws ecs update-service \
  --cluster $CLUSTER_NAME \
  --service $SERVICE_NAME \
  --task-definition $TASK_DEF_ARN

# Wait for the service to stabilize
echo "Waiting for ECS to stabilize..."
aws ecs wait services-stable --cluster $CLUSTER_NAME --services $SERVICE_NAME


for TASK in $TASKS; do
  echo "Stopping task: $TASK"
  aws ecs stop-task \
    --cluster $CLUSTER_NAME \
    --task $TASK \
    --reason "Deploying new task definition"
done


echo "Service updated successfully."
