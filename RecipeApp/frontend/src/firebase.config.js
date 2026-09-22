/**
 * Pure Firebase web config for tests and `firebase.js`.
 * @param {ImportMeta['env']} env - typically `import.meta.env`
 */
export function getFirebaseWebConfig(env) {
  const storageBucket = env.VITE_FIREBASE_STORAGE_BUCKET
  if (storageBucket == null || String(storageBucket).trim() === '') {
    throw new Error(
      'VITE_FIREBASE_STORAGE_BUCKET is required. Firebase Storage has no default bucket without storageBucket (storage/no-default-bucket).'
    )
  }

  return {
    apiKey: env.VITE_FIREBASE_API_KEY,
    authDomain: env.VITE_FIREBASE_AUTH_DOMAIN,
    projectId: env.VITE_FIREBASE_PROJECT_ID,
    appId: env.VITE_FIREBASE_APP_ID,
    storageBucket,
  }
}
