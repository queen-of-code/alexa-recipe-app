FROM node:20-alpine
RUN apk add --no-cache openjdk21-jre-headless
RUN npm install -g firebase-tools

# Pre-download emulator binaries so startup doesn't fetch them at runtime.
# The auth emulator is bundled in firebase-tools; only Firestore needs a separate JAR.
RUN firebase setup:emulators:firestore

WORKDIR /app
CMD ["firebase", "emulators:start", "--project", "queen-of-code", "--import=/data", "--export-on-exit=/data"]
