import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import RecipeList from '../recipes/RecipeList'

vi.mock('../firebase', () => ({ auth: {} }))
vi.mock('../auth/AuthContext', () => {
  const user = { uid: 'test-uid', email: 'test@example.com' }
  return { useAuth: () => user }
})

const mockGetAllRecipes = vi.fn()
const mockSearchRecipes = vi.fn()
vi.mock('../api/recipeApi', () => ({
  getAllRecipes: (...args) => mockGetAllRecipes(...args),
  searchRecipes: (...args) => mockSearchRecipes(...args),
  deleteRecipe: vi.fn().mockResolvedValue(),
}))

describe('RecipeList', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  function renderPage() {
    return render(
      <MemoryRouter>
        <RecipeList />
      </MemoryRouter>
    )
  }

  it('renders table with Name, Prep Time, Servings, Cook Time, Last Updated headers', async () => {
    mockGetAllRecipes.mockResolvedValue([])
    renderPage()
    await waitFor(() => {
      expect(screen.getByText('Name')).toBeInTheDocument()
      expect(screen.getByText(/Prep Time/i)).toBeInTheDocument()
      expect(screen.getByText(/Servings/i)).toBeInTheDocument()
      expect(screen.getByText(/Cook Time/i)).toBeInTheDocument()
      expect(screen.getByText(/Last Updated/i)).toBeInTheDocument()
    })
  })

  it('renders recipe rows with data', async () => {
    mockGetAllRecipes.mockResolvedValue([
      {
        recipeId: '1',
        name: 'Pasta',
        prepTime: 10,
        servings: 4,
        cookTime: 20,
        lastUpdated: '2024-01-01T00:00:00Z',
      },
    ])
    renderPage()
    await waitFor(() => {
      expect(screen.getByText('Pasta')).toBeInTheDocument()
      expect(screen.getByText('10')).toBeInTheDocument()
      expect(screen.getByText('4')).toBeInTheDocument()
      expect(screen.getByText('20')).toBeInTheDocument()
    })
  })

  it('shows empty state when no recipes', async () => {
    mockGetAllRecipes.mockResolvedValue([])
    renderPage()
    await waitFor(() => {
      expect(screen.getByText(/no recipes/i)).toBeInTheDocument()
    })
  })

  it('shows loading state initially', () => {
    mockGetAllRecipes.mockReturnValue(new Promise(() => {}))
    renderPage()
    expect(screen.getByText(/loading/i)).toBeInTheDocument()
  })

  it('search posts ingredients and combine mode', async () => {
    const user = userEvent.setup()
    mockGetAllRecipes.mockResolvedValue([
      {
        recipeId: '1',
        name: 'Pasta',
        prepTimeMins: 10,
        servings: 4,
        cookTimeMins: 20,
        lastUpdated: '2024-01-01T00:00:00Z',
      },
    ])
    mockSearchRecipes.mockResolvedValue([
      {
        recipeId: '2',
        name: 'Filtered',
        prepTimeMins: 5,
        servings: 2,
        cookTimeMins: 15,
        lastUpdated: '2024-01-02T00:00:00Z',
      },
    ])
    renderPage()
    await waitFor(() => expect(screen.getByText('Pasta')).toBeInTheDocument())

    await user.type(screen.getByPlaceholderText(/tomato, cheddar/i), 'tomato, cheese')
    await user.selectOptions(screen.getByLabelText(/^match$/i), 'Any')
    await user.click(screen.getByRole('button', { name: /^search$/i }))

    await waitFor(() => {
      expect(mockSearchRecipes).toHaveBeenCalledWith('test-uid', {
        ingredients: ['tomato', 'cheese'],
        combine: 'Any',
      })
    })
    await waitFor(() => expect(screen.getByText('Filtered')).toBeInTheDocument())
  })
})
