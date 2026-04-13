import { useState, useEffect } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { createRecipe, updateRecipe, getRecipe } from '../api/recipeApi'
import { useAuth } from '../auth/AuthContext'

export default function RecipeForm() {
  const { recipeId } = useParams()
  const isEditing = Boolean(recipeId)
  const user = useAuth()
  const navigate = useNavigate()

  const [name, setName] = useState('')
  const [prepTime, setPrepTime] = useState('')
  const [cookTime, setCookTime] = useState('')
  const [servings, setServings] = useState('')
  const [ingredients, setIngredients] = useState([''])
  const [steps, setSteps] = useState([''])
  const [error, setError] = useState('')

  useEffect(() => {
    if (isEditing && user) {
      getRecipe(user.uid, recipeId).then((r) => {
        setName(r.name ?? '')
        setPrepTime(r.prepTime != null ? String(r.prepTime) : '')
        setCookTime(r.cookTime != null ? String(r.cookTime) : '')
        setServings(r.servings != null ? String(r.servings) : '')
        setIngredients(Array.isArray(r.ingredients) && r.ingredients.length ? r.ingredients : [''])
        setSteps(Array.isArray(r.steps) && r.steps.length ? r.steps : [''])
      })
    }
  }, [isEditing, user, recipeId])

  function addIngredient() {
    setIngredients((prev) => [...prev, ''])
  }
  function removeIngredient(i) {
    setIngredients((prev) => prev.filter((_, idx) => idx !== i))
  }
  function updateIngredient(i, val) {
    setIngredients((prev) => prev.map((v, idx) => (idx === i ? val : v)))
  }

  function addStep() {
    setSteps((prev) => [...prev, ''])
  }
  function removeStep(i) {
    setSteps((prev) => prev.filter((_, idx) => idx !== i))
  }
  function updateStep(i, val) {
    setSteps((prev) => prev.map((v, idx) => (idx === i ? val : v)))
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')
    try {
      const payload = {
        name,
        prepTimeMins: parseInt(prepTime) || 0,
        cookTimeMins: parseInt(cookTime) || 0,
        servings: parseInt(servings) || 0,
        ingredients: ingredients.filter((s) => s.trim()),
        steps: steps.filter((s) => s.trim()),
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
    <div className="max-w-2xl mx-auto mt-8 px-4 pb-8">
      <div className="bg-white rounded-xl shadow p-8">
        <h2 className="text-2xl font-bold text-gray-900 mb-6">
          {isEditing ? 'Edit Recipe' : 'Create Recipe'}
        </h2>

        {error && (
          <div className="mb-4 p-3 bg-red-50 border border-red-300 text-red-700 rounded-lg" role="alert">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Basic info */}
          <div>
            <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-1">
              Recipe Name
            </label>
            <input
              id="name"
              type="text"
              className="border border-gray-300 rounded-lg px-3 py-2 w-full focus:outline-none focus:ring-2 focus:ring-violet-500"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
            />
          </div>

          <div className="grid grid-cols-3 gap-4">
            <div>
              <label htmlFor="prepTime" className="block text-sm font-medium text-gray-700 mb-1">
                Prep Time (mins)
              </label>
              <input
                id="prepTime"
                type="number"
                min="0"
                className="border border-gray-300 rounded-lg px-3 py-2 w-full focus:outline-none focus:ring-2 focus:ring-violet-500"
                value={prepTime}
                onChange={(e) => setPrepTime(e.target.value)}
              />
            </div>
            <div>
              <label htmlFor="cookTime" className="block text-sm font-medium text-gray-700 mb-1">
                Cook Time (mins)
              </label>
              <input
                id="cookTime"
                type="number"
                min="0"
                className="border border-gray-300 rounded-lg px-3 py-2 w-full focus:outline-none focus:ring-2 focus:ring-violet-500"
                value={cookTime}
                onChange={(e) => setCookTime(e.target.value)}
              />
            </div>
            <div>
              <label htmlFor="servings" className="block text-sm font-medium text-gray-700 mb-1">
                Servings
              </label>
              <input
                id="servings"
                type="number"
                min="1"
                className="border border-gray-300 rounded-lg px-3 py-2 w-full focus:outline-none focus:ring-2 focus:ring-violet-500"
                value={servings}
                onChange={(e) => setServings(e.target.value)}
              />
            </div>
          </div>

          {/* Ingredients */}
          <div>
            <h3 className="text-sm font-semibold text-gray-800 mb-2">Ingredients</h3>
            <div className="space-y-2">
              {ingredients.map((ing, i) => (
                <div key={i} className="flex gap-2 items-center">
                  <input
                    type="text"
                    aria-label={`Ingredient ${i + 1}`}
                    className="border border-gray-300 rounded-lg px-3 py-2 flex-1 focus:outline-none focus:ring-2 focus:ring-violet-500"
                    value={ing}
                    onChange={(e) => updateIngredient(i, e.target.value)}
                    placeholder={`Ingredient ${i + 1}`}
                  />
                  <button
                    type="button"
                    onClick={() => removeIngredient(i)}
                    className="text-gray-400 hover:text-red-500 text-lg font-bold px-2"
                    aria-label="Remove ingredient"
                  >
                    &times;
                  </button>
                </div>
              ))}
            </div>
            <button
              type="button"
              onClick={addIngredient}
              className="mt-2 text-sm text-violet-700 hover:text-violet-900 font-medium"
            >
              + Add Ingredient
            </button>
          </div>

          {/* Steps */}
          <div>
            <label htmlFor="step-0" className="block text-sm font-semibold text-gray-800 mb-2">
              Steps
            </label>
            <div className="space-y-2">
              {steps.map((step, i) => (
                <div key={i} className="flex gap-2 items-start">
                  <span className="text-xs font-bold text-violet-700 mt-3 min-w-[1.25rem]">{i + 1}.</span>
                  <input
                    id={i === 0 ? 'step-0' : undefined}
                    type="text"
                    aria-label={`Step ${i + 1}`}
                    className="border border-gray-300 rounded-lg px-3 py-2 flex-1 focus:outline-none focus:ring-2 focus:ring-violet-500"
                    value={step}
                    onChange={(e) => updateStep(i, e.target.value)}
                    placeholder={`Step ${i + 1}`}
                  />
                  <button
                    type="button"
                    onClick={() => removeStep(i)}
                    className="text-gray-400 hover:text-red-500 text-lg font-bold px-2 mt-1"
                    aria-label="Remove step"
                  >
                    &times;
                  </button>
                </div>
              ))}
            </div>
            <button
              type="button"
              onClick={addStep}
              className="mt-2 text-sm text-violet-700 hover:text-violet-900 font-medium"
            >
              + Add Step
            </button>
          </div>

          {/* Actions */}
          <div className="flex gap-3 pt-2">
            <button
              type="submit"
              className="bg-violet-700 hover:bg-violet-800 text-white font-medium py-2 px-6 rounded-lg transition-colors"
            >
              {isEditing ? 'Save' : 'Create'}
            </button>
            <Link
              to="/recipes"
              className="border border-gray-300 text-gray-700 hover:bg-gray-50 font-medium py-2 px-6 rounded-lg transition-colors"
            >
              Back to List
            </Link>
          </div>
        </form>
      </div>
    </div>
  )
}
