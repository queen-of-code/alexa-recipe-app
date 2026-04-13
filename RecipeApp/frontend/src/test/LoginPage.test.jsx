import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import LoginPage from '../auth/LoginPage'

const mockNavigate = vi.fn()
const mockUseAuth = vi.fn(() => null)

vi.mock('../firebase', () => ({ auth: {} }))

vi.mock('firebase/auth', () => ({
  signInWithEmailAndPassword: vi.fn(),
  createUserWithEmailAndPassword: vi.fn(),
}))

vi.mock('react-router-dom', async (importOriginal) => {
  const actual = await importOriginal()
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  }
})

vi.mock('../auth/AuthContext', () => ({
  useAuth: () => mockUseAuth(),
}))

import { signInWithEmailAndPassword, createUserWithEmailAndPassword } from 'firebase/auth'

describe('LoginPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockUseAuth.mockReturnValue(null)
  })

  function renderPage() {
    return render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    )
  }

  it('renders "Sign In" heading by default', () => {
    renderPage()
    expect(screen.getByRole('heading', { name: /Sign In/i })).toBeInTheDocument()
  })

  it('toggles to "Create Account" when register link clicked', () => {
    renderPage()
    const toggleLink = screen.getByText(/Need an account\? Register/i)
    fireEvent.click(toggleLink)
    expect(screen.getByRole('heading', { name: /Create Account/i })).toBeInTheDocument()
  })

  it('calls signInWithEmailAndPassword on submit in login mode', async () => {
    signInWithEmailAndPassword.mockResolvedValue({ user: { uid: 'test-uid' } })
    renderPage()

    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: 'test@example.com' } })
    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'password123' } })
    fireEvent.click(screen.getByRole('button', { name: /sign in/i }))

    await waitFor(() => {
      expect(signInWithEmailAndPassword).toHaveBeenCalledWith({}, 'test@example.com', 'password123')
    })
  })

  it('calls createUserWithEmailAndPassword on submit in register mode', async () => {
    createUserWithEmailAndPassword.mockResolvedValue({ user: { uid: 'test-uid' } })
    renderPage()

    fireEvent.click(screen.getByText(/Need an account\? Register/i))
    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: 'test@example.com' } })
    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'password123' } })
    fireEvent.click(screen.getByRole('button', { name: /create account/i }))

    await waitFor(() => {
      expect(createUserWithEmailAndPassword).toHaveBeenCalledWith({}, 'test@example.com', 'password123')
    })
  })

  it('navigates to /recipes on successful login', async () => {
    signInWithEmailAndPassword.mockResolvedValue({ user: { uid: 'test-uid' } })
    // Simulate useAuth returning a user after sign-in resolves
    mockUseAuth.mockReturnValueOnce(null).mockReturnValue({ uid: 'test-uid', email: 'test@example.com' })
    renderPage()

    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: 'test@example.com' } })
    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'password123' } })
    fireEvent.click(screen.getByRole('button', { name: /sign in/i }))

    await waitFor(() => {
      expect(mockNavigate).toHaveBeenCalledWith('/recipes', { replace: true })
    })
  })

  it('shows error message on auth failure', async () => {
    signInWithEmailAndPassword.mockRejectedValue(new Error('Invalid credentials'))
    renderPage()

    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: 'bad@example.com' } })
    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'wrong' } })
    fireEvent.click(screen.getByRole('button', { name: /sign in/i }))

    await waitFor(() => {
      expect(screen.getByText(/Invalid credentials/i)).toBeInTheDocument()
    })
  })
})
