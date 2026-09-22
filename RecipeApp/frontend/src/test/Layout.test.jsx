import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import Layout from '../components/Layout'
import { ThemeProvider } from '../theme/ThemeContext'

vi.mock('../firebase', () => ({ auth: {} }))
vi.mock('firebase/auth', () => ({ signOut: vi.fn() }))

const useAuthMock = vi.fn()

vi.mock('../auth/AuthContext', () => ({
  useAuth: () => useAuthMock(),
}))

function renderLayout() {
  return render(
    <MemoryRouter>
      <ThemeProvider>
        <Layout />
      </ThemeProvider>
    </MemoryRouter>
  )
}

describe('Layout theme toggle', () => {
  it('does not show theme toggle when signed out', () => {
    useAuthMock.mockReturnValue(null)
    renderLayout()
    expect(screen.queryByRole('button', { name: /switch to (dark|light) mode/i })).not.toBeInTheDocument()
  })

  it('shows theme toggle before logout when signed in', () => {
    useAuthMock.mockReturnValue({ uid: 'u1', email: 'chef@example.com' })
    renderLayout()
    const toggle = screen.getByRole('button', { name: /switch to dark mode/i })
    expect(toggle).toBeInTheDocument()
    const logout = screen.getByRole('button', { name: 'Logout' })
    expect(toggle.compareDocumentPosition(logout) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy()
  })
})
