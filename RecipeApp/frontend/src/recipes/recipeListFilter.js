export function parseListFilter(searchParams) {
  const ingredients = (searchParams.get('ingredients') ?? '')
    .split(',')
    .map((s) => s.trim())
    .filter(Boolean)
  const combine = searchParams.get('combine') === 'Any' ? 'Any' : 'All'
  return { ingredients, combine, active: ingredients.length > 0 }
}

export function listFilterSearch({ ingredients, combine, active }) {
  if (!active) return ''
  const params = new URLSearchParams()
  params.set('ingredients', ingredients.join(','))
  params.set('combine', combine === 'Any' ? 'Any' : 'All')
  return params.toString()
}
