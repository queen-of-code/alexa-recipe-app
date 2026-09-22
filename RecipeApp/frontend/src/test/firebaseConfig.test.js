import { describe, expect, it } from 'vitest'
import { buildFirebaseConfig } from '../firebaseConfig'

const baseEnv = {
  VITE_FIREBASE_API_KEY: 'test-key',
  VITE_FIREBASE_AUTH_DOMAIN: 'queen-of-code.firebaseapp.com',
  VITE_FIREBASE_PROJECT_ID: 'queen-of-code',
  VITE_FIREBASE_APP_ID: '1:test:web:app',
}

describe('buildFirebaseConfig', () => {
  it('includes storageBucket when VITE_FIREBASE_STORAGE_BUCKET is set', () => {
    const config = buildFirebaseConfig({
      ...baseEnv,
      VITE_FIREBASE_STORAGE_BUCKET: 'queen-of-code.appspot.com',
    })

    expect(config.storageBucket).toBe('queen-of-code.appspot.com')
    expect(config.projectId).toBe('queen-of-code')
  })

  it('fails fast when the Storage bucket env var is missing', () => {
    expect(() => buildFirebaseConfig(baseEnv)).toThrow(/VITE_FIREBASE_STORAGE_BUCKET/)
  })
})
