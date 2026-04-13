import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import RecipeForm from '../recipes/RecipeForm'

vi.mock('../firebase', () => ({ auth: {} }))
vi.mock('../auth/AuthContext', () => ({
  useAuth: () => ({ uid: 'test-uid', email: 'test@example.com' }),
}))

const mockGetRecipe = vi.fn()
const mockCreateRecipe = vi.fn()
const mockUpdateRecipe = vi.fn()

vi.mock('../api/recipeApi', () => ({
  getRecipe: (...args) => mockGetRecipe(...args),
  createRecipe: (...args) => mockCreateRecipe(...args),
  updateRecipe: (...args) => mockUpdateRecipe(...args),
}))

const mockNavigate = vi.fn()
vi.mock('react-router-dom', async (importOriginal) => {
  const actual = await importOriginal()
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  }
})

const sampleRecipe = {
  recipeId: 'abc123',
  name: 'Test Pasta',
  prepTime: 10,
  servings: 4,
  cookTime: 20,
  ingredients: ['pasta', 'sauce'],
  steps: ['Boil water', 'Cook pasta'],
}

describe('RecipeForm', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  function renderNew() {
    return render(
      <MemoryRouter initialEntries={['/recipes/new']}>
        <Routes>
          <Route path="/recipes/new" element={<RecipeForm />} />
        </Routes>
      </MemoryRouter>
    )
  }

  function renderEdit() {
    mockGetRecipe.mockResolvedValue(sampleRecipe)
    return render(
      <MemoryRouter initialEntries={['/recipes/abc123/edit']}>
        <Routes>
          <Route path="/recipes/:recipeId/edit" element={<RecipeForm />} />
        </Routes>
      </MemoryRouter>
    )
  }

  it('renders Name, Prep Time, Servings, Cook Time, Steps fields', () => {
    renderNew()
    expect(screen.getByLabelText(/name/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/prep time/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/servings/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/cook time/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/steps/i)).toBeInTheDocument()
  })

  it('pre-populates fields when editing', async () => {
    renderEdit()
    await waitFor(() => {
      expect(screen.getByDisplayValue('Test Pasta')).toBeInTheDocument()
      expect(screen.getByDisplayValue('10')).toBeInTheDocument()
      expect(screen.getByDisplayValue('4')).toBeInTheDocument()
      expect(screen.getByDisplayValue('20')).toBeInTheDocument()
    })
  })

  it('calls updateRecipe on save when editing', async () => {
    mockUpdateRecipe.mockResolvedValue()
    renderEdit()

    await waitFor(() => {
      expect(screen.getByDisplayValue('Test Pasta')).toBeInTheDocument()
    })

    fireEvent.click(screen.getByRole('button', { name: /save/i }))

    await waitFor(() => {
      expect(mockUpdateRecipe).toHaveBeenCalled()
    })
  })

  it('calls createRecipe on save when creating new', async () => {
    mockCreateRecipe.mockResolvedValue()
    renderNew()

    fireEvent.change(screen.getByLabelText(/name/i), { target: { value: 'New Recipe' } })
    fireEvent.click(screen.getByRole('button', { name: /create/i }))

    await waitFor(() => {
      expect(mockCreateRecipe).toHaveBeenCalled()
    })
  })
})
