import { useEffect, useRef, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { getAllRecipes, deleteRecipe, searchRecipes, setFavorite, clearFavorite } from '../api/recipeApi'
import { useAuth } from '../auth/AuthContext'
import FavoriteButton from './FavoriteButton'
import { listFilterSearch, parseListFilter } from './recipeListFilter'

function updatedMillis(recipe) {
  const raw = recipe.lastUpdated || recipe.lastUpdateTime
  const parsed = raw ? Date.parse(raw) : 0
  return Number.isNaN(parsed) ? 0 : parsed
}

function orderFavoritesFirst(rows) {
  return [...rows].sort((a, b) => {
    const favoriteDelta = Number(Boolean(b.isFavorite)) - Number(Boolean(a.isFavorite))
    if (favoriteDelta !== 0) return favoriteDelta
    return updatedMillis(b) - updatedMillis(a)
  })
}

export default function RecipeList() {
  const user = useAuth()
  const [searchParams, setSearchParams] = useSearchParams()
  const searchParamsRef = useRef(searchParams)
  searchParamsRef.current = searchParams
  const applied = parseListFilter(searchParams)
  const appliedKey = listFilterSearch(applied)
  const filterActive = applied.active

  const [recipes, setRecipes] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [searchHint, setSearchHint] = useState('')
  const [ingredientInput, setIngredientInput] = useState(
    applied.active ? applied.ingredients.join(', ') : '',
  )
  const [combineMode, setCombineMode] = useState(applied.combine)
  const [favoritesOnly, setFavoritesOnly] = useState(false)
  const favoritesOnlyRef = useRef(false)
  favoritesOnlyRef.current = favoritesOnly
  const requestFailedRef = useRef(false)
  const [retryNonce, setRetryNonce] = useState(0)

  useEffect(() => {
    if (!user) return
    const current = parseListFilter(searchParamsRef.current)
    let cancelled = false
    setLoading(true)
    setError('')
    requestFailedRef.current = false
    if (current.active) {
      setIngredientInput(current.ingredients.join(', '))
      setCombineMode(current.combine)
    } else {
      setIngredientInput('')
      setCombineMode('All')
    }

    const onlyFavorites = favoritesOnlyRef.current
    const request = current.active
      ? searchRecipes(user.uid, {
          ingredients: current.ingredients,
          combine: current.combine,
        })
      : onlyFavorites
        ? getAllRecipes(user.uid, { favoritesOnly: true })
        : getAllRecipes(user.uid)

    request
      .then((data) => {
        if (!cancelled) setRecipes(data)
      })
      .catch((err) => {
        if (!cancelled) {
          requestFailedRef.current = true
          setError(err.message)
        }
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })

    return () => {
      cancelled = true
    }
  }, [user, appliedKey, retryNonce])

  function handleSearch(e) {
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
    const next = listFilterSearch({
      ingredients: terms,
      combine: combineMode,
      active: true,
    })
    if (next === appliedKey) {
      if (requestFailedRef.current) setRetryNonce((n) => n + 1)
      return
    }
    setSearchParams(new URLSearchParams(next))
  }

  async function handleClearFilter() {
    const draftDirty = ingredientInput.trim() !== '' || combineMode !== 'All'
    const hintShowing = searchHint !== ''
    setIngredientInput('')
    setCombineMode('All')
    setSearchHint('')
    if (applied.active) {
      setSearchParams(new URLSearchParams())
      return
    }
    if (!draftDirty && !hintShowing) return
    if (!user) return
    setLoading(true)
    setError('')
    try {
      const data = favoritesOnlyRef.current
        ? await getAllRecipes(user.uid, { favoritesOnly: true })
        : await getAllRecipes(user.uid)
      setRecipes(data)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  async function handleDelete(recipeId) {
    if (!window.confirm('Delete this recipe?')) return
    await deleteRecipe(user.uid, recipeId)
    setRecipes((r) => r.filter((x) => x.recipeId !== recipeId))
  }

  async function handleFavoritesOnlyChange(checked) {
    setFavoritesOnly(checked)
    if (filterActive || !user) return
    if (checked) setRecipes((rows) => rows.filter((r) => r.isFavorite))
    setLoading(true)
    setError('')
    try {
      const data = checked
        ? await getAllRecipes(user.uid, { favoritesOnly: true })
        : await getAllRecipes(user.uid)
      setRecipes(data)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  // Favorite toggles wait for the API, then refetch so favorites-first order
  // stays server-authoritative. Ingredient-search rows are patched in memory
  // (favorites-only is a client filter on that result set) and re-sorted
  // favorites-first, then lastUpdated descending. A failed toggle leaves the
  // previous list and uses the error banner.
  async function handleToggleFavorite(recipe) {
    const next = !recipe.isFavorite
    setError('')
    try {
      if (next) await setFavorite(user.uid, recipe.recipeId)
      else await clearFavorite(user.uid, recipe.recipeId)
      if (filterActive) {
        setRecipes((rows) =>
          orderFavoritesFirst(
            rows.map((row) =>
              row.recipeId === recipe.recipeId ? { ...row, isFavorite: next } : row,
            ),
          ),
        )
        return
      }
      const data = favoritesOnly
        ? await getAllRecipes(user.uid, { favoritesOnly: true })
        : await getAllRecipes(user.uid)
      setRecipes(data)
    } catch (err) {
      setError(err.message)
    }
  }

  const visibleRecipes =
    filterActive && favoritesOnly ? recipes.filter((r) => r.isFavorite) : recipes

  function emptyMessage() {
    if (filterActive && favoritesOnly) return 'No favorite recipes match your ingredients.'
    if (filterActive) return 'No recipes match your ingredients.'
    if (favoritesOnly) return 'No favorite recipes yet.'
    return 'No recipes yet — create your first one!'
  }

  if (loading && recipes.length === 0 && !filterActive && !favoritesOnly) {
    return <p className="text-center mt-8 text-gray-500 dark:text-gray-400">Loading...</p>
  }

  return (
    <div className="max-w-5xl mx-auto mt-8 px-4">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-gray-100">My Recipes</h1>
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
          className="mb-4 flex flex-wrap items-center justify-between gap-2 rounded-lg border border-red-300 dark:border-red-800 bg-red-50 dark:bg-red-950 px-4 py-3 text-sm text-red-700 dark:text-red-300"
        >
          <span>{error}</span>
          <button
            type="button"
            className="shrink-0 rounded font-medium text-red-700 dark:text-red-300 underline hover:text-red-900 dark:hover:text-red-200"
            onClick={() => setError('')}
          >
            Dismiss
          </button>
        </div>
      ) : null}

      <form
        onSubmit={handleSearch}
        className="mb-4 flex flex-wrap items-end gap-3 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 p-4 shadow-sm"
        aria-label="Filter recipes by ingredients"
      >
        <div className="flex min-w-[12rem] flex-1 flex-col gap-1">
          <label htmlFor="recipe-ingredient-filter" className="text-xs font-medium text-gray-600 dark:text-gray-300">
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
            className="rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 px-3 py-2 text-sm text-gray-900 dark:text-gray-100"
            autoComplete="off"
            aria-describedby={searchHint ? 'recipe-search-hint' : undefined}
          />
          {searchHint ? (
            <p id="recipe-search-hint" className="text-xs text-amber-700 dark:text-amber-300" role="status">
              {searchHint}
            </p>
          ) : null}
        </div>
        <div className="flex flex-col gap-1">
          <label htmlFor="recipe-ingredient-combine" className="text-xs font-medium text-gray-600 dark:text-gray-300">
            Match
          </label>
          <select
            id="recipe-ingredient-combine"
            value={combineMode}
            onChange={(e) => setCombineMode(e.target.value)}
            className="rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-900 px-3 py-2 text-sm text-gray-900 dark:text-gray-100"
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
          className="rounded-lg border border-gray-300 dark:border-gray-600 px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-200 hover:bg-gray-50 dark:hover:bg-gray-800"
        >
          Clear
        </button>
      </form>

      <label className="mb-4 flex items-center gap-2 text-sm text-gray-700 dark:text-gray-200">
        <input
          type="checkbox"
          checked={favoritesOnly}
          onChange={(e) => handleFavoritesOnlyChange(e.target.checked)}
        />
        Show favorites only
      </label>

      {loading && filterActive ? (
        <p className="mb-4 text-center text-sm text-gray-500 dark:text-gray-400">Searching…</p>
      ) : null}

      <div className="w-full bg-white dark:bg-gray-900 shadow rounded-xl overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-gray-50 dark:bg-gray-800 text-gray-600 dark:text-gray-300 text-xs uppercase tracking-wide">
              <th className="text-left px-4 py-3 font-medium w-16"></th>
              <th className="text-left px-4 py-3 font-medium">Name</th>
              <th className="text-left px-4 py-3 font-medium">Prep Time (mins)</th>
              <th className="text-left px-4 py-3 font-medium">Servings</th>
              <th className="text-left px-4 py-3 font-medium">Cook Time (mins)</th>
              <th className="text-left px-4 py-3 font-medium">Last Updated</th>
              <th className="px-4 py-3"></th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
            {visibleRecipes.length === 0 ? (
              loading ? null : (
                <tr>
                  <td colSpan="7" className="px-4 py-12 text-center text-gray-500 dark:text-gray-400">
                    {emptyMessage()}
                  </td>
                </tr>
              )
            ) : (
              visibleRecipes.map((r) => {
                const query = listFilterSearch(applied)
                const viewTo = query ? `/recipes/${r.recipeId}?${query}` : `/recipes/${r.recipeId}`
                return (
                  <tr key={r.recipeId} className="hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors">
                    <td className="px-4 py-3 w-16">
                      <Link
                        to={viewTo}
                        aria-label={`View ${r.name}`}
                        className="inline-flex rounded focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-700"
                      >
                        {r.completedImageUrl ? (
                          <img
                            src={r.completedImageUrl}
                            alt=""
                            className="h-10 w-10 rounded object-cover border border-gray-200 dark:border-gray-700"
                            loading="lazy"
                          />
                        ) : (
                          <span className="inline-block h-10 w-10 rounded bg-gray-100 dark:bg-gray-800 border border-gray-100 dark:border-gray-700" aria-hidden="true" />
                        )}
                      </Link>
                    </td>
                    <td className="px-4 py-3">
                      <Link
                        to={viewTo}
                        className="text-gray-900 dark:text-gray-100 font-medium hover:underline focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-700"
                      >
                        {r.name}
                      </Link>
                    </td>
                    <td className="px-4 py-3 text-gray-600 dark:text-gray-300">{r.prepTimeMins ?? r.prepTime}</td>
                    <td className="px-4 py-3 text-gray-600 dark:text-gray-300">{r.servings}</td>
                    <td className="px-4 py-3 text-gray-600 dark:text-gray-300">{r.cookTimeMins ?? r.cookTime}</td>
                    <td className="px-4 py-3 text-gray-600 dark:text-gray-300">
                      {r.lastUpdated || r.lastUpdateTime
                        ? new Date(r.lastUpdated || r.lastUpdateTime).toLocaleDateString()
                        : ''}
                    </td>
                    <td className="px-4 py-3 flex gap-3 items-center">
                      <FavoriteButton
                        isFavorite={r.isFavorite}
                        onToggle={() => handleToggleFavorite(r)}
                      />
                      <Link to={`/recipes/${r.recipeId}/edit`} className="text-blue-600 dark:text-blue-400 hover:underline">
                        Edit
                      </Link>
                      <Link to={viewTo} className="text-gray-500 dark:text-gray-400 hover:underline">
                        Details
                      </Link>
                      <button
                        className="text-red-600 dark:text-red-400 hover:underline"
                        onClick={() => handleDelete(r.recipeId)}
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                )
              })
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
