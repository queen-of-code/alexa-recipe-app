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
      .catch(err => setError(err.message))
  }, [user, recipeId])

  async function handleDelete() {
    if (!confirm('Delete this recipe?')) return
    await deleteRecipe(user.uid, recipeId)
    navigate('/recipes')
  }

  if (error) return <p style={{ color: 'red' }}>{error}</p>
  if (!recipe) return <p>Loading...</p>

  return (
    <div style={{ maxWidth: 700, margin: '40px auto', padding: 24 }}>
      <Link to="/recipes">← Back</Link>
      <h1>{recipe.name}</h1>
      <p>
        {recipe.servings} servings · {recipe.prepTimeMins} min prep · {recipe.cookTimeMins} min cook
      </p>
      <h2>Ingredients</h2>
      <ul>
        {recipe.ingredients?.map((ing, i) => <li key={i}>{ing}</li>)}
      </ul>
      <h2>Steps</h2>
      <ol>
        {recipe.steps?.map((step, i) => <li key={i}>{step}</li>)}
      </ol>
      <div style={{ marginTop: 24 }}>
        <Link to={`/recipes/${recipeId}/edit`}>
          <button>Edit</button>
        </Link>
        <button onClick={handleDelete} style={{ marginLeft: 8, color: 'red' }}>
          Delete
        </button>
      </div>
    </div>
  )
}
