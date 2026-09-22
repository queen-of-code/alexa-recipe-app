import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, useLocation } from 'react-router-dom'
import { deleteRecipe } from '../api/recipeApi'
import RecipeList from '../recipes/RecipeList'

function LocationProbe() {
  const location = useLocation()
  return <div data-testid="location">{`${location.pathname}${location.search}`}</div>
}

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

  function renderPage(initialEntry = '/recipes') {
    return render(
      <MemoryRouter initialEntries={[initialEntry]}>
        <RecipeList />
        <LocationProbe />
      </MemoryRouter>
    )
  }

  const pasta = {
    recipeId: '1',
    name: 'Pasta',
    prepTime: 10,
    servings: 4,
    cookTime: 20,
    lastUpdated: '2024-01-01T00:00:00Z',
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
    expect(screen.getByTestId('location').textContent).toBe('/recipes')
    expect(mockSearchRecipes).not.toHaveBeenCalled()
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

  it('links the recipe name to its detail view when the list has no filter', async () => {
    mockGetAllRecipes.mockResolvedValue([pasta])
    renderPage()
    const nameLink = await screen.findByRole('link', { name: 'Pasta' })
    expect(nameLink).toHaveAttribute('href', '/recipes/1')
  })

  it('names the thumbnail link View plus the recipe name and uses the same href', async () => {
    mockGetAllRecipes.mockResolvedValue([pasta])
    renderPage()
    const nameLink = await screen.findByRole('link', { name: 'Pasta' })
    const thumbLink = screen.getByRole('link', { name: 'View Pasta' })
    expect(thumbLink).toHaveAttribute('href', nameLink.getAttribute('href'))
  })

  it('loads the filter from the URL and puts that query on the name link', async () => {
    mockSearchRecipes.mockResolvedValue([pasta])
    renderPage('/recipes?ingredients=tomato,cheese&combine=Any')
    await waitFor(() => {
      expect(mockSearchRecipes).toHaveBeenCalledWith('test-uid', {
        ingredients: ['tomato', 'cheese'],
        combine: 'Any',
      })
    })
    expect(screen.getByLabelText(/^ingredients$/i)).toHaveValue('tomato, cheese')
    expect(screen.getByLabelText(/^match$/i)).toHaveValue('Any')
    const nameLink = await screen.findByRole('link', { name: 'Pasta' })
    const href = new URL(nameLink.getAttribute('href'), 'http://localhost')
    expect(href.pathname).toBe('/recipes/1')
    expect(href.searchParams.get('ingredients')).toBe('tomato,cheese')
    expect(href.searchParams.get('combine')).toBe('Any')
    expect(mockGetAllRecipes).not.toHaveBeenCalled()
  })

  it('writes the applied filter to the URL and searches from that URL', async () => {
    const user = userEvent.setup()
    mockGetAllRecipes.mockResolvedValue([pasta])
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
    await screen.findByText('Pasta')
    mockSearchRecipes.mockClear()

    await user.type(screen.getByPlaceholderText(/tomato, cheddar/i), 'tomato, cheese')
    await user.selectOptions(screen.getByLabelText(/^match$/i), 'Any')
    await user.click(screen.getByRole('button', { name: /^search$/i }))

    await waitFor(() => {
      expect(screen.getByTestId('location').textContent).toContain('combine=Any')
    })
    const location = screen.getByTestId('location').textContent
    expect(new URL(location, 'http://localhost').searchParams.get('ingredients')).toBe('tomato,cheese')
    expect(mockSearchRecipes).toHaveBeenCalledTimes(1)
    expect(mockSearchRecipes).toHaveBeenCalledWith('test-uid', {
      ingredients: ['tomato', 'cheese'],
      combine: 'Any',
    })
  })

  it('removes the query and loads every recipe when Clear is clicked', async () => {
    const user = userEvent.setup()
    mockSearchRecipes.mockResolvedValue([pasta])
    mockGetAllRecipes.mockResolvedValue([pasta])
    renderPage('/recipes?ingredients=tomato,cheese&combine=Any')
    await waitFor(() => expect(mockSearchRecipes).toHaveBeenCalled())
    mockGetAllRecipes.mockClear()

    await user.click(screen.getByRole('button', { name: /^clear$/i }))

    await waitFor(() => expect(mockGetAllRecipes).toHaveBeenCalledWith('test-uid'))
    expect(screen.getByTestId('location').textContent).toBe('/recipes')
    expect(screen.getByLabelText(/^ingredients$/i)).toHaveValue('')
    expect(screen.getByLabelText(/^match$/i)).toHaveValue('All')
  })

  it('keeps Edit on the edit route without the filter query', async () => {
    mockSearchRecipes.mockResolvedValue([pasta])
    renderPage('/recipes?ingredients=tomato&combine=Any')
    const edit = await screen.findByRole('link', { name: 'Edit' })
    expect(edit).toHaveAttribute('href', '/recipes/1/edit')
  })

  it('confirms before deleteRecipe', async () => {
    const user = userEvent.setup()
    vi.spyOn(window, 'confirm').mockReturnValue(false)
    mockGetAllRecipes.mockResolvedValue([pasta])
    renderPage()
    await screen.findByRole('button', { name: /^delete$/i })
    await user.click(screen.getByRole('button', { name: /^delete$/i }))
    expect(window.confirm).toHaveBeenCalledWith('Delete this recipe?')
    expect(deleteRecipe).not.toHaveBeenCalled()
  })
})
