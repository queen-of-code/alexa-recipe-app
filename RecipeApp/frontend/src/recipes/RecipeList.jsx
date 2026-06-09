import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import {
  getAllRecipes,
  deleteRecipe,
  searchRecipes,
  setFavorite,
  clearFavorite,
} from '../api/recipeApi'
import { useAuth } from '../auth/AuthContext'

function FavoriteButton({ isFavorite, onToggle, labelPrefix = '' }) {
  const label = isFavorite
    ? `${labelPrefix}Remove from favorites`.trim()
    : `${labelPrefix}Add to favorites`.trim()

  return (
    <button
      type="button"
      aria-pressed={isFavorite}
      aria-label={label}
      title={label}
      className={`text-lg leading-none ${isFavorite ? 'text-amber-500' : 'text-gray-300 hover:text-amber-400'}`}
      onClick={onToggle}
    >
      {isFavorite ? '★' : '☆'}
    </button>
  )
}

export default function RecipeList() {
  const user = useAuth()
  const [recipes, setRecipes] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [searchHint, setSearchHint] = useState('')
  const [ingredientInput, setIngredientInput] = useState('')
  const [combineMode, setCombineMode] = useState('All')
  const [filterActive, setFilterActive] = useState(false)
  const [favoritesOnly, setFavoritesOnly] = useState(false)

  const displayedRecipes = useMemo(() => {
    if (!favoritesOnly || !filterActive) return recipes
    return recipes.filter((r) => r.isFavorite)
  }, [recipes, favoritesOnly, filterActive])

  const refreshFullList = useCallback(async () => {
    if (!user) return
    setLoading(true)
    setError('')
    try {
      const data = await getAllRecipes(user.uid, { favoritesOnly })
      setRecipes(data)
      setFilterActive(false)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }, [user, favoritesOnly])

  useEffect(() => {
    if (!user || filterActive) return
    let cancelled = false
    setLoading(true)
    setError('')
    getAllRecipes(user.uid, { favoritesOnly })
      .then((data) => {
        if (!cancelled) setRecipes(data)
      })
      .catch((err) => {
        if (!cancelled) setError(err.message)
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [user, favoritesOnly, filterActive])

  async function handleSearch(e) {
    e.preventDefault()
    if (!user) return
    const terms = ingredientInput
      .split(',')
      .map((s) => s.trim())
      .filter(Boolean)
    if (terms.length === 0) {
      setSearchHint('Enter at least one ingredient (comma-separated).')
      return
    }
    setSearchHint('')

    setLoading(true)
    setError('')
    try {
      const data = await searchRecipes(user.uid, {
        ingredients: terms,
        combine: combineMode,
      })
      setRecipes(data)
      setFilterActive(true)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  function handleClearFilter() {
    setIngredientInput('')
    setCombineMode('All')
    setSearchHint('')
    refreshFullList()
  }

  async function handleFavoritesOnlyChange(checked) {
    setFavoritesOnly(checked)
  }

  async function handleToggleFavorite(recipe) {
    if (!user) return
    const nextFavorite = !recipe.isFavorite
    try {
      const updated = nextFavorite
        ? await setFavorite(user.uid, recipe.recipeId)
        : await clearFavorite(user.uid, recipe.recipeId)
      setRecipes((rows) => {
        const next = rows.map((r) => (r.recipeId === recipe.recipeId ? { ...r, ...updated } : r))
        if (favoritesOnly && !nextFavorite) {
          return next.filter((r) => r.recipeId !== recipe.recipeId)
        }
        return next
      })
    } catch (err) {
      setError(err.message)
    }
  }

  async function handleDelete(recipeId) {
    if (!window.confirm('Delete this recipe?')) return
    await deleteRecipe(user.uid, recipeId)
    setRecipes((r) => r.filter((x) => x.recipeId !== recipeId))
  }

  if (loading && recipes.length === 0 && !filterActive) {
    return <p className="text-center mt-8 text-gray-500">Loading...</p>
  }

  const emptyMessage = filterActive
    ? favoritesOnly
      ? 'No favorite recipes match your ingredients.'
      : 'No recipes match your ingredients.'
    : favoritesOnly
      ? 'No favorites yet — star a recipe to see it here.'
      : 'No recipes yet — create your first one!'

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

      {error ? (
        <div
          role="alert"
          className="mb-4 flex flex-wrap items-center justify-between gap-2 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800"
        >
          <span>{error}</span>
          <button
            type="button"
            className="shrink-0 rounded font-medium text-red-700 underline hover:text-red-900"
            onClick={() => setError('')}
          >
            Dismiss
          </button>
        </div>
      ) : null}

      <div className="mb-4 flex items-center gap-2 rounded-xl border border-gray-200 bg-white px-4 py-3 shadow-sm">
        <input
          id="recipe-favorites-only"
          type="checkbox"
          checked={favoritesOnly}
          onChange={(e) => handleFavoritesOnlyChange(e.target.checked)}
          className="h-4 w-4 rounded border-gray-300 text-violet-600 focus:ring-violet-500"
        />
        <label htmlFor="recipe-favorites-only" className="text-sm font-medium text-gray-700">
          Show favorites only
        </label>
      </div>

      <form
        onSubmit={handleSearch}
        className="mb-4 flex flex-wrap items-end gap-3 rounded-xl border border-gray-200 bg-white p-4 shadow-sm"
        aria-label="Filter recipes by ingredients"
      >
        <div className="flex min-w-[12rem] flex-1 flex-col gap-1">
          <label htmlFor="recipe-ingredient-filter" className="text-xs font-medium text-gray-600">
            Ingredients
          </label>
          <input
            id="recipe-ingredient-filter"
            type="text"
            value={ingredientInput}
            onChange={(e) => {
              setIngredientInput(e.target.value)
              if (searchHint) setSearchHint('')
            }}
            placeholder="e.g. tomato, cheddar"
            className="rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-900"
            autoComplete="off"
            aria-describedby={searchHint ? 'recipe-search-hint' : undefined}
          />
          {searchHint ? (
            <p id="recipe-search-hint" className="text-xs text-amber-700" role="status">
              {searchHint}
            </p>
          ) : null}
        </div>
        <div className="flex flex-col gap-1">
          <label htmlFor="recipe-ingredient-combine" className="text-xs font-medium text-gray-600">
            Match
          </label>
          <select
            id="recipe-ingredient-combine"
            value={combineMode}
            onChange={(e) => setCombineMode(e.target.value)}
            className="rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900"
          >
            <option value="All">All (AND)</option>
            <option value="Any">Any (OR)</option>
          </select>
        </div>
        <button
          type="submit"
          className="rounded-lg bg-violet-600 px-4 py-2 text-sm font-medium text-white hover:bg-violet-700"
        >
          Search
        </button>
        <button
          type="button"
          onClick={handleClearFilter}
          className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
        >
          Clear
        </button>
      </form>

      {loading && filterActive ? (
        <p className="mb-4 text-center text-sm text-gray-500">Searching…</p>
      ) : null}

      <div className="w-full bg-white shadow rounded-xl overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-gray-50 text-gray-600 text-xs uppercase tracking-wide">
              <th className="text-left px-4 py-3 font-medium w-10"></th>
              <th className="text-left px-4 py-3 font-medium w-16"></th>
              <th className="text-left px-4 py-3 font-medium">Name</th>
              <th className="text-left px-4 py-3 font-medium">Prep Time (mins)</th>
              <th className="text-left px-4 py-3 font-medium">Servings</th>
              <th className="text-left px-4 py-3 font-medium">Cook Time (mins)</th>
              <th className="text-left px-4 py-3 font-medium">Last Updated</th>
              <th className="px-4 py-3"></th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {displayedRecipes.length === 0 ? (
              <tr>
                <td colSpan="8" className="px-4 py-12 text-center text-gray-500">
                  {emptyMessage}
                </td>
              </tr>
            ) : (
              displayedRecipes.map((r) => (
                <tr key={r.recipeId} className="hover:bg-gray-50 transition-colors">
                  <td className="px-4 py-3 w-10">
                    <FavoriteButton
                      isFavorite={Boolean(r.isFavorite)}
                      onToggle={() => handleToggleFavorite(r)}
                    />
                  </td>
                  <td className="px-4 py-3 w-16">
                    {r.completedImageUrl ? (
                      <img
                        src={r.completedImageUrl}
                        alt=""
                        className="h-10 w-10 rounded object-cover border border-gray-200"
                        loading="lazy"
                      />
                    ) : (
                      <span className="inline-block h-10 w-10 rounded bg-gray-100 border border-gray-100" aria-hidden />
                    )}
                  </td>
                  <td className="px-4 py-3 text-gray-900 font-medium">{r.name}</td>
                  <td className="px-4 py-3 text-gray-600">{r.prepTimeMins ?? r.prepTime}</td>
                  <td className="px-4 py-3 text-gray-600">{r.servings}</td>
                  <td className="px-4 py-3 text-gray-600">{r.cookTimeMins ?? r.cookTime}</td>
                  <td className="px-4 py-3 text-gray-600">
                    {r.lastUpdated || r.lastUpdateTime
                      ? new Date(r.lastUpdated || r.lastUpdateTime).toLocaleDateString()
                      : ''}
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
