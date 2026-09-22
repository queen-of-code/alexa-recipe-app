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
const mockSetFavorite = vi.fn()
const mockClearFavorite = vi.fn()
vi.mock('../api/recipeApi', () => ({
  getAllRecipes: (...args) => mockGetAllRecipes(...args),
  searchRecipes: (...args) => mockSearchRecipes(...args),
  deleteRecipe: vi.fn().mockResolvedValue(),
  setFavorite: (...args) => mockSetFavorite(...args),
  clearFavorite: (...args) => mockClearFavorite(...args),
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

  it('shows hint when Search submitted with empty ingredients', async () => {
    const user = userEvent.setup()
    mockGetAllRecipes.mockResolvedValue([])
    renderPage()
    await waitFor(() => expect(screen.getByText('My Recipes')).toBeInTheDocument())

    await user.click(screen.getByRole('button', { name: /^search$/i }))
    expect(
      screen.getByText(/enter at least one ingredient/i),
    ).toBeInTheDocument()
  })

  it('favorites-only toggle requests the favoritesOnly query', async () => {
    const user = userEvent.setup()
    mockGetAllRecipes.mockResolvedValue([])
    renderPage()
    await waitFor(() => expect(screen.getByText(/no recipes yet/i)).toBeInTheDocument())

    await user.click(screen.getByRole('checkbox', { name: /show favorites only/i }))

    await waitFor(() => {
      expect(mockGetAllRecipes).toHaveBeenCalledWith('test-uid', { favoritesOnly: true })
    })
    expect(screen.getByText(/no favorite recipes yet/i)).toBeInTheDocument()
  })

  it('favorite button saves and shows the pressed state after refresh', async () => {
    const user = userEvent.setup()
    const plain = {
      recipeId: '1',
      name: 'Pasta',
      prepTimeMins: 10,
      servings: 4,
      cookTimeMins: 20,
      isFavorite: false,
    }
    mockGetAllRecipes.mockResolvedValueOnce([plain]).mockResolvedValue([{ ...plain, isFavorite: true }])
    mockSetFavorite.mockResolvedValue({ ...plain, isFavorite: true })
    renderPage()
    await waitFor(() => expect(screen.getByText('Pasta')).toBeInTheDocument())

    await user.click(screen.getByRole('button', { name: /add to favorites/i }))

    await waitFor(() => expect(mockSetFavorite).toHaveBeenCalledWith('test-uid', '1'))
    expect(screen.getByRole('button', { name: /remove from favorites/i })).toHaveAttribute(
      'aria-pressed',
      'true',
    )
  })

  it('favorites-only filters ingredient search results in memory', async () => {
    const user = userEvent.setup()
    mockGetAllRecipes.mockResolvedValue([])
    mockSearchRecipes.mockResolvedValue([
      { recipeId: '1', name: 'Fav Soup', isFavorite: true, prepTimeMins: 1, servings: 1, cookTimeMins: 1 },
      { recipeId: '2', name: 'Weeknight Chili', isFavorite: false, prepTimeMins: 1, servings: 1, cookTimeMins: 1 },
    ])
    renderPage()
    await waitFor(() => expect(screen.getByText(/no recipes yet/i)).toBeInTheDocument())

    await user.type(screen.getByPlaceholderText(/tomato, cheddar/i), 'tomato')
    await user.click(screen.getByRole('button', { name: /^search$/i }))
    await waitFor(() => expect(screen.getByText('Weeknight Chili')).toBeInTheDocument())

    await user.click(screen.getByRole('checkbox', { name: /show favorites only/i }))

    expect(screen.getByText('Fav Soup')).toBeInTheDocument()
    expect(screen.queryByText('Weeknight Chili')).not.toBeInTheDocument()
    expect(mockGetAllRecipes).toHaveBeenCalledTimes(1)
  })

  it('shows dismissible error banner without hiding the table', async () => {
    const user = userEvent.setup()
    mockGetAllRecipes.mockRejectedValue(new Error('Network down'))
    renderPage()
    await waitFor(() => expect(screen.getByText(/network down/i)).toBeInTheDocument())
    expect(screen.getByText('My Recipes')).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: /^dismiss$/i }))
    expect(screen.queryByText(/network down/i)).not.toBeInTheDocument()
  })
})
