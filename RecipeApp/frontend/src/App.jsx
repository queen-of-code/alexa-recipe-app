import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider, useAuth } from './auth/AuthContext'
import LoginPage from './auth/LoginPage'
import RecipeList from './recipes/RecipeList'
import RecipeDetail from './recipes/RecipeDetail'
import RecipeForm from './recipes/RecipeForm'

function RequireAuth({ children }) {
  const user = useAuth()
  if (user === undefined) return <p>Loading...</p>
  if (user === null) return <Navigate to="/login" replace />
  return children
}

function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/recipes" element={<RequireAuth><RecipeList /></RequireAuth>} />
      <Route path="/recipes/new" element={<RequireAuth><RecipeForm /></RequireAuth>} />
      <Route path="/recipes/:recipeId" element={<RequireAuth><RecipeDetail /></RequireAuth>} />
      <Route path="/recipes/:recipeId/edit" element={<RequireAuth><RecipeForm /></RequireAuth>} />
      <Route path="*" element={<Navigate to="/recipes" replace />} />
    </Routes>
  )
}

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <AppRoutes />
      </BrowserRouter>
    </AuthProvider>
  )
}
