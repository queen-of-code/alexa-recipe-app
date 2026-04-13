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

  if (error) return <p className="container mt-4 text-danger">{error}</p>
  if (!recipe) return <p className="container mt-4">Loading...</p>

  return (
    <div className="container mt-4">
      <dl className="dl-horizontal">
        <dt>Name</dt>
        <dd>{recipe.name}</dd>

        <dt>Prep Time (mins)</dt>
        <dd>{recipe.prepTime}</dd>

        <dt>Servings</dt>
        <dd>{recipe.servings}</dd>

        <dt>Cook Time (mins)</dt>
        <dd>{recipe.cookTime}</dd>

        <dt>Ingredients</dt>
        <dd>
          <ul>
            {recipe.ingredients?.map((ing, i) => (
              <li key={i}>{ing}</li>
            ))}
          </ul>
        </dd>

        <dt>Steps</dt>
        <dd>
          <ol>
            {recipe.steps?.map((step, i) => (
              <li key={i}>{step}</li>
            ))}
          </ol>
        </dd>
      </dl>

      <Link to={`/recipes/${recipeId}/edit`} className="btn btn-primary mr-2">
        Edit
      </Link>
      <Link to="/recipes" className="btn btn-secondary">
        Back to List
      </Link>
    </div>
  )
}
