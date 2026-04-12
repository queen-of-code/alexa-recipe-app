FROM node:20-alpine
RUN apk add --no-cache openjdk21-jre-headless
RUN npm install -g firebase-tools
WORKDIR /app
CMD ["firebase", "emulators:start", "--project", "queen-of-code", "--import=/data", "--export-on-exit=/data"]
