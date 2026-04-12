import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  // In production the app is served at queenofcode.dev/recipe-app via the qoc-recipe-app
  // Firebase Hosting site.  The base path ensures asset URLs resolve correctly.
  base: process.env.NODE_ENV === 'production' ? '/recipe-app/' : '/',
})
