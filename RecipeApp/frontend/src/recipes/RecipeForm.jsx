import { useState, useEffect } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { createRecipe, updateRecipe, getRecipe } from '../api/recipeApi'
import { useAuth } from '../auth/AuthContext'

const emptyForm = {
  name: '',
  prepTime: '',
  servings: '',
  cookTime: '',
  ingredients: '',
  steps: '',
}

export default function RecipeForm() {
  const { recipeId } = useParams()
  const isEditing = Boolean(recipeId)
  const user = useAuth()
  const navigate = useNavigate()
  const [form, setForm] = useState(emptyForm)
  const [error, setError] = useState('')

  useEffect(() => {
    if (isEditing && user) {
      getRecipe(user.uid, recipeId).then((r) => {
        setForm({
          name: r.name ?? '',
          prepTime: r.prepTime ?? '',
          servings: r.servings ?? '',
          cookTime: r.cookTime ?? '',
          ingredients: Array.isArray(r.ingredients) ? r.ingredients.join('\n') : (r.ingredients ?? ''),
          steps: Array.isArray(r.steps) ? r.steps.join('\n') : (r.steps ?? ''),
        })
      })
    }
  }, [isEditing, user, recipeId])

  function handleChange(e) {
    const { name, value } = e.target
    setForm((f) => ({ ...f, [name]: value }))
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')
    try {
      const payload = {
        name: form.name,
        prepTime: Number(form.prepTime),
        servings: Number(form.servings),
        cookTime: Number(form.cookTime),
        ingredients: form.ingredients.split('\n').filter(Boolean),
        steps: form.steps.split('\n').filter(Boolean),
      }
      if (isEditing) {
        await updateRecipe(user.uid, recipeId, payload)
      } else {
        await createRecipe(user.uid, payload)
      }
      navigate('/recipes')
    } catch (err) {
      setError(err.message)
    }
  }

  return (
    <div className="container mt-4">
      <h1>{isEditing ? 'Edit Recipe' : 'New Recipe'}</h1>

      {error && (
        <div className="alert alert-danger" role="alert">
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="name">Name</label>
          <input
            id="name"
            name="name"
            type="text"
            className="form-control"
            value={form.name}
            onChange={handleChange}
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="prepTime">Prep Time (mins)</label>
          <input
            id="prepTime"
            name="prepTime"
            type="number"
            className="form-control"
            value={form.prepTime}
            onChange={handleChange}
            min="0"
          />
        </div>

        <div className="form-group">
          <label htmlFor="servings">Servings</label>
          <input
            id="servings"
            name="servings"
            type="number"
            className="form-control"
            value={form.servings}
            onChange={handleChange}
            min="1"
          />
        </div>

        <div className="form-group">
          <label htmlFor="cookTime">Cook Time (mins)</label>
          <input
            id="cookTime"
            name="cookTime"
            type="number"
            className="form-control"
            value={form.cookTime}
            onChange={handleChange}
            min="0"
          />
        </div>

        <div className="form-group">
          <label htmlFor="ingredients">Ingredients (one per line)</label>
          <textarea
            id="ingredients"
            name="ingredients"
            className="form-control"
            rows="5"
            value={form.ingredients}
            onChange={handleChange}
          />
        </div>

        <div className="form-group">
          <label htmlFor="steps">Steps (one per line)</label>
          <textarea
            id="steps"
            name="steps"
            className="form-control"
            rows="8"
            value={form.steps}
            onChange={handleChange}
          />
        </div>

        <button type="submit" className="btn btn-primary mr-2">
          {isEditing ? 'Save' : 'Create'}
        </button>
        <Link to="/recipes" className="btn btn-secondary">
          Back to List
        </Link>
      </form>
    </div>
  )
}
