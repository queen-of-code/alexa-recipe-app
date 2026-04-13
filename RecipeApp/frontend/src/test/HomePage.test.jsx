import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import HomePage from '../pages/HomePage'

vi.mock('../firebase', () => ({ auth: {} }))
vi.mock('../auth/AuthContext', () => ({
  useAuth: () => ({ uid: 'test-uid', email: 'test@example.com' }),
}))

describe('HomePage', () => {
  function renderPage() {
    return render(
      <MemoryRouter>
        <HomePage />
      </MemoryRouter>
    )
  }

  it('renders "Alexa Recipe App" heading', () => {
    renderPage()
    expect(screen.getByText(/Alexa Recipe App/i)).toBeInTheDocument()
  })

  it('renders "Store & Retrieve Recipes" section', () => {
    renderPage()
    expect(screen.getByText(/Store & Retrieve Recipes/i)).toBeInTheDocument()
  })

  it('renders "Backup to the Cloud" section', () => {
    renderPage()
    expect(screen.getByText(/Backup to the Cloud/i)).toBeInTheDocument()
  })

  it('renders "Interact With Alexa" section', () => {
    renderPage()
    expect(screen.getByText(/Interact With Alexa/i)).toBeInTheDocument()
  })

  it('renders "Plan Weekly Meals" section', () => {
    renderPage()
    expect(screen.getByText(/Plan Weekly Meals/i)).toBeInTheDocument()
  })
})
