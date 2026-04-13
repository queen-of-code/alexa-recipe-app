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

  if (loading) return <p className="container mt-4">Loading...</p>
  if (error) return <p className="container mt-4 text-danger">{error}</p>

  return (
    <div className="container mt-4">
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h1>My Recipes</h1>
        <Link to="/recipes/new" className="btn btn-primary">
          Create New
        </Link>
      </div>

      <table className="table table-bordered table-striped">
        <thead className="thead-light">
          <tr>
            <th>Name</th>
            <th>Prep Time (mins)</th>
            <th>Servings</th>
            <th>Cook Time (mins)</th>
            <th>Last Updated</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {recipes.length === 0 ? (
            <tr>
              <td colSpan="6">No recipes found. Create one!</td>
            </tr>
          ) : (
            recipes.map((r) => (
              <tr key={r.recipeId}>
                <td>{r.name}</td>
                <td>{r.prepTime}</td>
                <td>{r.servings}</td>
                <td>{r.cookTime}</td>
                <td>{r.lastUpdated ? new Date(r.lastUpdated).toLocaleDateString() : ''}</td>
                <td>
                  <Link to={`/recipes/${r.recipeId}/edit`} className="mr-2">
                    Edit
                  </Link>
                  {' | '}
                  <Link to={`/recipes/${r.recipeId}`} className="mx-2">
                    Details
                  </Link>
                  {' | '}
                  <button
                    className="btn btn-link text-danger p-0 ml-2"
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
  )
}
