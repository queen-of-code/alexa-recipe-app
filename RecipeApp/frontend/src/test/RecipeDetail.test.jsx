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

  function renderPage() {
    return render(
      <MemoryRouter initialEntries={['/recipes/abc123']}>
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
})
