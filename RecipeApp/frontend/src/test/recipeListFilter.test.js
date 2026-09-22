import { listFilterSearch, parseListFilter } from '../recipes/recipeListFilter'

describe('parseListFilter', () => {
  it('drops empty ingredient tokens', () => {
    const params = new URLSearchParams('ingredients=tomato,, cheese ,')
    expect(parseListFilter(params)).toEqual({
      ingredients: ['tomato', 'cheese'],
      combine: 'All',
      active: true,
    })
  })

  it('treats a missing ingredient list as unfiltered', () => {
    expect(parseListFilter(new URLSearchParams())).toEqual({
      ingredients: [],
      combine: 'All',
      active: false,
    })
  })

  it('coerces combine other than Any to All', () => {
    expect(parseListFilter(new URLSearchParams('ingredients=tomato&combine=all')).combine).toBe('All')
    expect(parseListFilter(new URLSearchParams('ingredients=tomato&combine=Any')).combine).toBe('Any')
  })
})

describe('listFilterSearch', () => {
  it('returns a canonical query string for an active filter', () => {
    expect(
      listFilterSearch({
        ingredients: ['tomato', 'cheese'],
        combine: 'Any',
        active: true,
      }),
    ).toBe('ingredients=tomato%2Ccheese&combine=Any')
  })

  it('returns an empty string when the filter is inactive', () => {
    expect(listFilterSearch({ ingredients: ['tomato'], combine: 'Any', active: false })).toBe('')
  })
})
