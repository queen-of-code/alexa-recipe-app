import { render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import RecipeDetail from '../recipes/RecipeDetail'

vi.mock('../firebase', () => ({ auth: {} }))
vi.mock('../auth/AuthContext', () => ({
  useAuth: () => ({ uid: 'test-uid', email: 'test@example.com' }),
}))

const mockGetRecipe = vi.fn()
vi.mock('../api/recipeApi', () => ({
  getRecipe: (...args) => mockGetRecipe(...args),
}))

const sampleRecipe = {
  recipeId: 'abc123',
  name: 'Test Pasta',
  prepTime: 10,
  servings: 4,
  cookTime: 20,
  ingredients: ['pasta', 'sauce', 'cheese'],
  steps: ['Boil water', 'Cook pasta', 'Add sauce'],
}

describe('RecipeDetail', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockGetRecipe.mockResolvedValue(sampleRecipe)
  })

  function renderPage(initialEntry = '/recipes/abc123') {
    return render(
      <MemoryRouter initialEntries={[initialEntry]}>
        <Routes>
          <Route path="/recipes/:recipeId" element={<RecipeDetail />} />
        </Routes>
      </MemoryRouter>
    )
  }

  it('renders recipe name', async () => {
    renderPage()
    await waitFor(() => {
      expect(screen.getByText('Test Pasta')).toBeInTheDocument()
    })
  })

  it('renders steps as numbered list', async () => {
    renderPage()
    await waitFor(() => {
      expect(screen.getByText('Boil water')).toBeInTheDocument()
      expect(screen.getByText('Cook pasta')).toBeInTheDocument()
      expect(screen.getByText('Add sauce')).toBeInTheDocument()
    })
    const ol = document.querySelector('ol')
    expect(ol).toBeInTheDocument()
  })

  it('renders ingredients as list', async () => {
    renderPage()
    await waitFor(() => {
      expect(screen.getByText('pasta')).toBeInTheDocument()
      expect(screen.getByText('sauce')).toBeInTheDocument()
      expect(screen.getByText('cheese')).toBeInTheDocument()
    })
    const ul = document.querySelector('ul')
    expect(ul).toBeInTheDocument()
  })

  it('points Back to List at the list with the same filter query', async () => {
    renderPage('/recipes/abc123?ingredients=tomato&combine=Any')
    const back = await screen.findByRole('link', { name: /back to list/i })
    const href = new URL(back.getAttribute('href'), 'http://localhost')
    expect(href.pathname).toBe('/recipes')
    expect(href.searchParams.get('ingredients')).toBe('tomato')
    expect(href.searchParams.get('combine')).toBe('Any')
  })

  it('points Back to List at /recipes when the detail URL has no filter', async () => {
    renderPage()
    const back = await screen.findByRole('link', { name: /back to list/i })
    expect(back).toHaveAttribute('href', '/recipes')
  })
})
