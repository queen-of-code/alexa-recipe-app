import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { signOut } from 'firebase/auth'
import { auth } from '../firebase'
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
      .catch(err => setError(err.message))
      .finally(() => setLoading(false))
  }, [user])

  async function handleDelete(recipeId) {
    if (!confirm('Delete this recipe?')) return
    await deleteRecipe(user.uid, recipeId)
    setRecipes(r => r.filter(x => x.recipeId !== recipeId))
  }

  if (loading) return <p>Loading...</p>
  if (error) return <p style={{ color: 'red' }}>{error}</p>

  return (
    <div style={{ maxWidth: 700, margin: '40px auto', padding: 24 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1>My Recipes</h1>
        <div>
          <Link to="/recipes/new">
            <button>+ New Recipe</button>
          </Link>
          <button onClick={() => signOut(auth)} style={{ marginLeft: 8 }}>
            Sign Out
          </button>
        </div>
      </div>

      {recipes.length === 0 ? (
        <p>No recipes yet. Create one!</p>
      ) : (
        <ul style={{ listStyle: 'none', padding: 0 }}>
          {recipes.map(r => (
            <li key={r.recipeId} style={{ borderBottom: '1px solid #eee', padding: '12px 0' }}>
              <strong>{r.name}</strong>
              <span style={{ marginLeft: 16, color: '#888' }}>
                {r.servings} servings · {r.prepTimeMins + r.cookTimeMins} min
              </span>
              <span style={{ float: 'right' }}>
                <Link to={`/recipes/${r.recipeId}`}>View</Link>
                {' · '}
                <Link to={`/recipes/${r.recipeId}/edit`}>Edit</Link>
                {' · '}
                <button
                  onClick={() => handleDelete(r.recipeId)}
                  style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'red' }}
                >
                  Delete
                </button>
              </span>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
