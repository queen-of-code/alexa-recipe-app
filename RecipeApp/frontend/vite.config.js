import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  // Firebase Hosting serves the app at the root (qoc-recipe-app.web.app/).
  // The queenofcode.dev/recipe-app path is a rewrite on the main site that redirects
  // to this hosting site's root — the app itself always runs at /.
  base: '/',
})
