import { BrowserRouter, Routes, Route, Navigate, Outlet } from 'react-router-dom'
import { AuthProvider, useAuth } from './auth/AuthContext'
import Layout from './components/Layout'
import HomePage from './pages/HomePage'
import AboutPage from './pages/AboutPage'
import ContactPage from './pages/ContactPage'
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

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Layout />}>
            <Route index element={<HomePage />} />
            <Route path="about" element={<AboutPage />} />
            <Route path="contact" element={<ContactPage />} />
            <Route path="login" element={<LoginPage />} />
            <Route path="recipes" element={<RequireAuth><RecipeList /></RequireAuth>} />
            <Route path="recipes/new" element={<RequireAuth><RecipeForm /></RequireAuth>} />
            <Route path="recipes/:recipeId" element={<RequireAuth><RecipeDetail /></RequireAuth>} />
            <Route path="recipes/:recipeId/edit" element={<RequireAuth><RecipeForm /></RequireAuth>} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}
