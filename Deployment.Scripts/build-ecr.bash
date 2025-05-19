REGION="us-east-1"
REPO_NAME="document-service/ingestionservice"
DOCKERFILE_PATH="../Document.Services.IngestionManagementAPI/Dockerfile"  # Replace with your Dockerfile path
ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
LATEST_TAG=$(aws ecr describe-images --repository-name "$REPO_NAME" --region "$REGION" --query 'sort_by(imageDetails, &imagePushedAt)[-1].imageTags[0]' --output text 2>/dev/null || echo "0")
NEW_TAG=$((LATEST_TAG + 1))
ECR_URI="$ACCOUNT_ID.dkr.ecr.$REGION.amazonaws.com/$REPO_NAME:$NEW_TAG"
aws ecr get-login-password --region "$REGION" | docker login --username AWS --password-stdin "$ACCOUNT_ID.dkr.ecr.$REGION.amazonaws.com"
aws ecr create-repository --repository-name "$REPO_NAME" --region "$REGION" >/dev/null 2>&1 || true
docker build -t "$REPO_NAME:$NEW_TAG" -f "$DOCKERFILE_PATH" "$(dirname "$DOCKERFILE_PATH")"
docker tag "$REPO_NAME:$NEW_TAG" "$ECR_URI"
docker push "$ECR_URI"