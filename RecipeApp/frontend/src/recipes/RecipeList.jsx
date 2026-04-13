import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getAllRecipes, deleteRecipe } from '../api/recipeApi'
import { useAuth } from '../auth/AuthContext'

export default function RecipeList() {
  const user = useAuth()
  const [recipes, setRecipes] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!user) return
    getAllRecipes(user.uid)
      .then(setRecipes)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false))
  }, [user])

  async function handleDelete(recipeId) {
    if (!window.confirm('Delete this recipe?')) return
    await deleteRecipe(user.uid, recipeId)
    setRecipes((r) => r.filter((x) => x.recipeId !== recipeId))
  }

  if (loading) return <p className="text-center mt-8 text-gray-500">Loading...</p>
  if (error) return <p className="text-center mt-8 text-red-600">{error}</p>

  return (
    <div className="max-w-5xl mx-auto mt-8 px-4">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">My Recipes</h1>
        <Link
          to="/recipes/new"
          className="bg-violet-700 hover:bg-violet-800 text-white font-medium px-4 py-2 rounded-lg transition-colors text-sm"
        >
          New Recipe
        </Link>
      </div>

      <div className="w-full bg-white shadow rounded-xl overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-gray-50 text-gray-600 text-xs uppercase tracking-wide">
              <th className="text-left px-4 py-3 font-medium">Name</th>
              <th className="text-left px-4 py-3 font-medium">Prep Time (mins)</th>
              <th className="text-left px-4 py-3 font-medium">Servings</th>
              <th className="text-left px-4 py-3 font-medium">Cook Time (mins)</th>
              <th className="text-left px-4 py-3 font-medium">Last Updated</th>
              <th className="px-4 py-3"></th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {recipes.length === 0 ? (
              <tr>
                <td colSpan="6" className="px-4 py-12 text-center text-gray-500">
                  No recipes yet — create your first one!
                </td>
              </tr>
            ) : (
              recipes.map((r) => (
                <tr key={r.recipeId} className="hover:bg-gray-50 transition-colors">
                  <td className="px-4 py-3 text-gray-900 font-medium">{r.name}</td>
                  <td className="px-4 py-3 text-gray-600">{r.prepTime}</td>
                  <td className="px-4 py-3 text-gray-600">{r.servings}</td>
                  <td className="px-4 py-3 text-gray-600">{r.cookTime}</td>
                  <td className="px-4 py-3 text-gray-600">
                    {r.lastUpdated ? new Date(r.lastUpdated).toLocaleDateString() : ''}
                  </td>
                  <td className="px-4 py-3 flex gap-3 items-center">
                    <Link to={`/recipes/${r.recipeId}/edit`} className="text-blue-600 hover:underline">
                      Edit
                    </Link>
                    <Link to={`/recipes/${r.recipeId}`} className="text-gray-500 hover:underline">
                      Details
                    </Link>
                    <button
                      className="text-red-600 hover:underline"
                      onClick={() => handleDelete(r.recipeId)}
                    >
                      Delete
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
