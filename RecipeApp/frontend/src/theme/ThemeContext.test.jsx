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

function stubMatchMedia(prefersDark) {
  vi.stubGlobal(
    'matchMedia',
    vi.fn().mockImplementation((query) => ({
      matches: query === '(prefers-color-scheme: dark)' ? prefersDark : false,
      media: query,
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
    }))
  )
}

describe('ThemeProvider', () => {
  beforeEach(() => {
    localStorage.clear()
    document.documentElement.classList.remove('dark')
    useAuthMock.mockReturnValue(null)
    stubMatchMedia(false)
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('does not add dark class when signed out even if storage and OS are dark', () => {
    localStorage.setItem(THEME_STORAGE_KEY, 'dark')
    stubMatchMedia(true)
    render(
      <ThemeProvider>
        <Probe />
      </ThemeProvider>
    )
    expect(document.documentElement.classList.contains('dark')).toBe(false)
  })

  it('adds dark class when signed in with no stored key and OS dark', () => {
    stubMatchMedia(true)
    useAuthMock.mockReturnValue({ email: 'a@b.com' })
    render(
      <ThemeProvider>
        <Probe />
      </ThemeProvider>
    )
    expect(document.documentElement.classList.contains('dark')).toBe(true)
  })

  it('stays light when signed in with stored light and OS dark', () => {
    localStorage.setItem(THEME_STORAGE_KEY, 'light')
    stubMatchMedia(true)
    useAuthMock.mockReturnValue({ email: 'a@b.com' })
    render(
      <ThemeProvider>
        <Probe />
      </ThemeProvider>
    )
    expect(document.documentElement.classList.contains('dark')).toBe(false)
  })

  it('removes dark class on sign-out and keeps the storage key', () => {
    localStorage.setItem(THEME_STORAGE_KEY, 'dark')
    useAuthMock.mockReturnValue({ email: 'a@b.com' })
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
    expect(localStorage.getItem(THEME_STORAGE_KEY)).toBe('dark')
  })

  it('toggle from resolved light writes dark and adds the class', async () => {
    useAuthMock.mockReturnValue({ email: 'a@b.com' })
    const { getByRole } = render(
      <ThemeProvider>
        <Probe />
      </ThemeProvider>
    )

    await act(async () => {
      getByRole('button', { name: 'toggle' }).click()
    })

    expect(localStorage.getItem(THEME_STORAGE_KEY)).toBe('dark')
    expect(document.documentElement.classList.contains('dark')).toBe(true)
  })
})
