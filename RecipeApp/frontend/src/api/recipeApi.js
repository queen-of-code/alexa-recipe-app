import { auth } from '../firebase'

const API_BASE = import.meta.env.VITE_API_URL

async function getAuthHeaders() {
  const token = await auth.currentUser.getIdToken()
  return {
    Authorization: `Bearer ${token}`,
    'Content-Type': 'application/json',
  }
}

export async function getAllRecipes(userId, options = {}) {
  const headers = await getAuthHeaders()
  const params = new URLSearchParams()
  if (options.favoritesOnly) params.set('favoritesOnly', 'true')
  const qs = params.toString()
  const url = `${API_BASE}/api/values/${userId}${qs ? `?${qs}` : ''}`
  const res = await fetch(url, { headers })
  if (!res.ok) throw new Error('Failed to fetch recipes')
  return res.json()
}

/** @param {{ ingredients?: string[], combine?: string, query?: string }} body */
export async function searchRecipes(userId, body) {
  const headers = await getAuthHeaders()
  const res = await fetch(`${API_BASE}/api/values/${userId}/search`, {
    method: 'POST',
    headers,
    body: JSON.stringify(body),
  })
  if (!res.ok) throw new Error('Search failed')
  return res.json()
}

export async function getRecipe(userId, recipeId) {
  const headers = await getAuthHeaders()
  const res = await fetch(`${API_BASE}/api/values/${userId}/${recipeId}`, { headers })
  if (!res.ok) throw new Error('Failed to fetch recipe')
  return res.json()
}

export async function createRecipe(userId, recipe) {
  const headers = await getAuthHeaders()
  const res = await fetch(`${API_BASE}/api/values/${userId}`, {
    method: 'POST',
    headers,
    body: JSON.stringify({ ...recipe, userId }),
  })
  if (!res.ok) throw new Error('Failed to create recipe')
  if (res.status === 204) return null
  return res.json()
}

export async function updateRecipe(userId, recipeId, recipe) {
  const headers = await getAuthHeaders()
  const res = await fetch(`${API_BASE}/api/values/${userId}/${recipeId}`, {
    method: 'PUT',
    headers,
    body: JSON.stringify({ ...recipe, userId, recipeId }),
  })
  if (!res.ok) throw new Error('Failed to update recipe')
}

export async function deleteRecipe(userId, recipeId) {
  const headers = await getAuthHeaders()
  const res = await fetch(`${API_BASE}/api/values/${userId}/${recipeId}`, {
    method: 'DELETE',
    headers,
  })
  if (!res.ok) throw new Error('Failed to delete recipe')
}

export async function setFavorite(userId, recipeId) {
  const headers = await getAuthHeaders()
  const res = await fetch(`${API_BASE}/api/values/${userId}/${recipeId}/favorite`, {
    method: 'PUT',
    headers,
    body: JSON.stringify({ favorite: true }),
  })
  if (!res.ok) throw new Error('Failed to favorite recipe')
  return res.json()
}

export async function clearFavorite(userId, recipeId) {
  const headers = await getAuthHeaders()
  const res = await fetch(`${API_BASE}/api/values/${userId}/${recipeId}/favorite`, {
    method: 'DELETE',
    headers,
  })
  if (!res.ok) throw new Error('Failed to unfavorite recipe')
  return res.json()
}
