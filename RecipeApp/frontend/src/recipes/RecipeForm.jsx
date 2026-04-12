import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { createRecipe, updateRecipe, getRecipe } from '../api/recipeApi'
import { useAuth } from '../auth/AuthContext'

const emptyForm = {
  name: '',
  servings: 1,
  prepTimeMins: 0,
  cookTimeMins: 0,
  ingredients: [''],
  steps: [''],
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
      getRecipe(user.uid, recipeId).then(r => {
        setForm({
          name: r.name,
          servings: r.servings,
          prepTimeMins: r.prepTimeMins,
          cookTimeMins: r.cookTimeMins,
          ingredients: r.ingredients?.length ? r.ingredients : [''],
          steps: r.steps?.length ? r.steps : [''],
        })
      })
    }
  }, [isEditing, user, recipeId])

  function updateList(field, index, value) {
    setForm(f => {
      const list = [...f[field]]
      list[index] = value
      return { ...f, [field]: list }
    })
  }

  function addListItem(field) {
    setForm(f => ({ ...f, [field]: [...f[field], ''] }))
  }

  function removeListItem(field, index) {
    setForm(f => ({ ...f, [field]: f[field].filter((_, i) => i !== index) }))
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')
    try {
      const payload = {
        ...form,
        ingredients: form.ingredients.filter(Boolean),
        steps: form.steps.filter(Boolean),
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
    <div style={{ maxWidth: 700, margin: '40px auto', padding: 24 }}>
      <h1>{isEditing ? 'Edit Recipe' : 'New Recipe'}</h1>
      <form onSubmit={handleSubmit}>
        <div>
          <label>Name</label>
          <input
            value={form.name}
            onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
            required
            style={{ display: 'block', width: '100%', marginBottom: 12 }}
          />
        </div>
        <div style={{ display: 'flex', gap: 16, marginBottom: 12 }}>
          <label>Servings <input type="number" min="1" value={form.servings}
            onChange={e => setForm(f => ({ ...f, servings: Number(e.target.value) }))} /></label>
          <label>Prep (min) <input type="number" min="0" value={form.prepTimeMins}
            onChange={e => setForm(f => ({ ...f, prepTimeMins: Number(e.target.value) }))} /></label>
          <label>Cook (min) <input type="number" min="0" value={form.cookTimeMins}
            onChange={e => setForm(f => ({ ...f, cookTimeMins: Number(e.target.value) }))} /></label>
        </div>

        <h3>Ingredients</h3>
        {form.ingredients.map((ing, i) => (
          <div key={i} style={{ display: 'flex', gap: 8, marginBottom: 8 }}>
            <input value={ing} onChange={e => updateList('ingredients', i, e.target.value)}
              style={{ flex: 1 }} placeholder={`Ingredient ${i + 1}`} />
            <button type="button" onClick={() => removeListItem('ingredients', i)}>✕</button>
          </div>
        ))}
        <button type="button" onClick={() => addListItem('ingredients')}>+ Add Ingredient</button>

        <h3>Steps</h3>
        {form.steps.map((step, i) => (
          <div key={i} style={{ display: 'flex', gap: 8, marginBottom: 8 }}>
            <textarea value={step} onChange={e => updateList('steps', i, e.target.value)}
              style={{ flex: 1 }} placeholder={`Step ${i + 1}`} rows={2} />
            <button type="button" onClick={() => removeListItem('steps', i)}>✕</button>
          </div>
        ))}
        <button type="button" onClick={() => addListItem('steps')}>+ Add Step</button>

        {error && <p style={{ color: 'red', marginTop: 12 }}>{error}</p>}

        <div style={{ marginTop: 24 }}>
          <button type="submit">{isEditing ? 'Save Changes' : 'Create Recipe'}</button>
          <button type="button" onClick={() => navigate('/recipes')} style={{ marginLeft: 8 }}>
            Cancel
          </button>
        </div>
      </form>
    </div>
  )
}
