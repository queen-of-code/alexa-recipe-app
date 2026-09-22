import { render, act } from '@testing-library/react'
import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest'
import { ThemeProvider, useTheme } from './ThemeContext'
import { THEME_STORAGE_KEY } from './themeStorage'

const useAuthMock = vi.fn()

vi.mock('../auth/AuthContext', () => ({
  useAuth: () => useAuthMock(),
}))

function Probe() {
  const { resolved, toggleTheme } = useTheme()
  return (
    <div>
      <span data-testid="resolved">{resolved}</span>
      <button type="button" onClick={toggleTheme}>
        toggle
      </button>
    </div>
  )
}

describe('ThemeProvider', () => {
  beforeEach(() => {
    localStorage.clear()
    document.documentElement.classList.remove('dark')
    useAuthMock.mockReturnValue(null)
    vi.stubGlobal(
      'matchMedia',
      vi.fn().mockImplementation((query) => ({
        matches: query === '(prefers-color-scheme: dark)' ? false : false,
        media: query,
        addEventListener: vi.fn(),
        removeEventListener: vi.fn(),
      }))
    )
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('does not add dark class when signed out', () => {
    localStorage.setItem(THEME_STORAGE_KEY, 'dark')
    render(
      <ThemeProvider>
        <Probe />
      </ThemeProvider>
    )
    expect(document.documentElement.classList.contains('dark')).toBe(false)
  })

  it('adds dark class when signed in with stored dark preference', () => {
    localStorage.setItem(THEME_STORAGE_KEY, 'dark')
    useAuthMock.mockReturnValue({ uid: 'u1', email: 'a@b.com' })
    render(
      <ThemeProvider>
        <Probe />
      </ThemeProvider>
    )
    expect(document.documentElement.classList.contains('dark')).toBe(true)
  })

  it('removes dark class on sign-out', () => {
    localStorage.setItem(THEME_STORAGE_KEY, 'dark')
    useAuthMock.mockReturnValue({ uid: 'u1', email: 'a@b.com' })
    const { rerender } = render(
      <ThemeProvider>
        <Probe />
      </ThemeProvider>
    )
    expect(document.documentElement.classList.contains('dark')).toBe(true)

    useAuthMock.mockReturnValue(null)
    rerender(
      <ThemeProvider>
        <Probe />
      </ThemeProvider>
    )
    expect(document.documentElement.classList.contains('dark')).toBe(false)
  })

  it('toggle persists explicit preference', async () => {
    useAuthMock.mockReturnValue({ uid: 'u1', email: 'a@b.com' })
    const { getByRole, getByTestId } = render(
      <ThemeProvider>
        <Probe />
      </ThemeProvider>
    )
    expect(getByTestId('resolved').textContent).toBe('light')

    await act(async () => {
      getByRole('button', { name: 'toggle' }).click()
    })

    expect(localStorage.getItem(THEME_STORAGE_KEY)).toBe('dark')
    expect(document.documentElement.classList.contains('dark')).toBe(true)
  })
})
