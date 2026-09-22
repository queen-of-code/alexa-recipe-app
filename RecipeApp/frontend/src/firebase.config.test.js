import { describe, it, expect } from 'vitest'
import { getFirebaseWebConfig } from './firebase.config.js'

describe('getFirebaseWebConfig', () => {
  it('includes storageBucket so getStorage(app) can resolve the default bucket', () => {
    const env = {
      VITE_FIREBASE_API_KEY: 'test-key',
      VITE_FIREBASE_AUTH_DOMAIN: 'proj.firebaseapp.com',
      VITE_FIREBASE_PROJECT_ID: 'proj',
      VITE_FIREBASE_APP_ID: '1:1:web:abc',
      VITE_FIREBASE_STORAGE_BUCKET: 'proj.appspot.com',
    }
    const cfg = getFirebaseWebConfig(env)
    expect(cfg.storageBucket).toBe('proj.appspot.com')
    expect(cfg.projectId).toBe('proj')
  })
})
