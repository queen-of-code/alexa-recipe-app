/**
 * Pure Firebase web config for tests and `firebase.js`.
 * @param {ImportMeta['env']} env - typically `import.meta.env`
 */
export function getFirebaseWebConfig(env) {
  return {
    apiKey: env.VITE_FIREBASE_API_KEY,
    authDomain: env.VITE_FIREBASE_AUTH_DOMAIN,
    projectId: env.VITE_FIREBASE_PROJECT_ID,
    appId: env.VITE_FIREBASE_APP_ID,
    storageBucket: env.VITE_FIREBASE_STORAGE_BUCKET,
  }
}
