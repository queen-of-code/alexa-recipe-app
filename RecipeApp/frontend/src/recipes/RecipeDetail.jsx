import { useEffect, useState } from 'react'
import { useParams, Link, useNavigate } from 'react-router-dom'
import { getRecipe, deleteRecipe } from '../api/recipeApi'
import { useAuth } from '../auth/AuthContext'

export default function RecipeDetail() {
  const { recipeId } = useParams()
  const user = useAuth()
  const navigate = useNavigate()
  const [recipe, setRecipe] = useState(null)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!user) return
    getRecipe(user.uid, recipeId)
      .then(setRecipe)
      .catch((err) => setError(err.message))
  }, [user, recipeId])

  async function handleDelete() {
    if (!window.confirm('Delete this recipe?')) return
    await deleteRecipe(user.uid, recipeId)
    navigate('/recipes')
  }

  if (error) return <p className="text-center mt-8 text-red-600">{error}</p>
  if (!recipe) return <p className="text-center mt-8 text-gray-500">Loading...</p>

  return (
    <div className="max-w-3xl mx-auto mt-8 px-4">
      <div className="bg-white rounded-xl shadow p-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-4">{recipe.name}</h1>

        {recipe.completedImageUrl ? (
          <div className="mb-6">
            <img
              src={recipe.completedImageUrl}
              alt=""
              className="max-h-64 w-auto rounded-lg border border-gray-200 object-contain"
            />
          </div>
        ) : null}

        <div className="flex gap-3 mb-6">
          <span className="text-xs bg-violet-100 text-violet-700 font-medium px-3 py-1 rounded-full">
            Prep: {recipe.prepTimeMins ?? recipe.prepTime} mins
          </span>
          <span className="text-xs bg-indigo-100 text-indigo-700 font-medium px-3 py-1 rounded-full">
            Cook: {recipe.cookTimeMins ?? recipe.cookTime} mins
          </span>
          <span className="text-xs bg-gray-100 text-gray-700 font-medium px-3 py-1 rounded-full">
            Servings: {recipe.servings}
          </span>
        </div>

        <div className="mb-6">
          <h2 className="text-lg font-semibold text-gray-800 mb-2">Ingredients</h2>
          <ul className="list-disc list-inside space-y-1 text-gray-600 text-sm">
            {recipe.ingredients?.map((ing, i) => (
              <li key={i}>{ing}</li>
            ))}
          </ul>
        </div>

        <div className="mb-8">
          <h2 className="text-lg font-semibold text-gray-800 mb-2">Steps</h2>
          <ol className="space-y-2">
            {recipe.steps?.map((step, i) => (
              <li key={i} className="flex gap-3 items-start">
                <span className="text-xs font-bold text-violet-700 mt-1 min-w-[1.5rem]">{i + 1}.</span>
                <span className="p-3 bg-gray-50 rounded text-sm text-gray-700 flex-1">{step}</span>
              </li>
            ))}
          </ol>
        </div>

        <div className="flex gap-3">
          <Link
            to={`/recipes/${recipeId}/edit`}
            className="bg-violet-700 hover:bg-violet-800 text-white font-medium px-4 py-2 rounded-lg transition-colors text-sm"
          >
            Edit
          </Link>
          <Link
            to="/recipes"
            className="border border-gray-300 text-gray-700 hover:bg-gray-50 font-medium px-4 py-2 rounded-lg transition-colors text-sm"
          >
            Back to List
          </Link>
        </div>
      </div>
    </div>
  )
}
