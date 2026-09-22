import { initializeApp } from 'firebase/app'
import { getAuth, connectAuthEmulator } from 'firebase/auth'
import { getStorage, connectStorageEmulator } from 'firebase/storage'
import { getFirebaseWebConfig } from './firebase.config.js'

const firebaseConfig = getFirebaseWebConfig(import.meta.env)

const app = initializeApp(firebaseConfig)
export const auth = getAuth(app)
export const storage = getStorage(app)

if (import.meta.env.VITE_USE_EMULATOR === 'true') {
  connectAuthEmulator(auth, 'http://localhost:9099', { disableWarnings: false })
}

if (import.meta.env.VITE_USE_STORAGE_EMULATOR === 'true') {
  connectStorageEmulator(storage, 'localhost', 9199)
}
