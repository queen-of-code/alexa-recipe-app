FROM amazon/dynamodb-local:latest as builder

# needed for intialization
ENV DYNAMO_ACCESS_KEY=whatever \
    DYNAMO_SECRET_KEY=whocares \
    AWS_REGION=us-west-2

#RUN ["aws", "dynamodb", "list-tables", "--endpoint-url", "http://localhost:8000"]
