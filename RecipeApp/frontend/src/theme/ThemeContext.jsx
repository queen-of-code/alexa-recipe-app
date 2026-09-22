import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react'
import { useAuth } from '../auth/AuthContext'
import { getStoredTheme, setStoredTheme } from './themeStorage'
import { resolveTheme } from './resolveTheme'

const ThemeContext = createContext(null)

function readPrefersDark() {
  if (typeof window === 'undefined' || typeof window.matchMedia !== 'function') return false
  return window.matchMedia('(prefers-color-scheme: dark)').matches
}

export function ThemeProvider({ children }) {
  const user = useAuth()
  const signedIn = Boolean(user)
  const [prefersDark, setPrefersDark] = useState(readPrefersDark)
  const [preferenceTick, setPreferenceTick] = useState(0)

  useEffect(() => {
    if (typeof window.matchMedia !== 'function') return undefined
    const mq = window.matchMedia('(prefers-color-scheme: dark)')
    const onChange = (event) => setPrefersDark(event.matches)
    mq.addEventListener('change', onChange)
    return () => mq.removeEventListener('change', onChange)
  }, [])

  const resolved = useMemo(() => {
    if (!signedIn) return 'light'
    void preferenceTick
    return resolveTheme(getStoredTheme(), prefersDark)
  }, [signedIn, prefersDark, preferenceTick, user?.uid])

  useEffect(() => {
    const root = document.documentElement
    if (signedIn && resolved === 'dark') {
      root.classList.add('dark')
    } else {
      root.classList.remove('dark')
    }
  }, [signedIn, resolved])

  const toggleTheme = useCallback(() => {
    const current = resolveTheme(getStoredTheme(), prefersDark)
    const next = current === 'dark' ? 'light' : 'dark'
    setStoredTheme(next)
    setPreferenceTick((t) => t + 1)
  }, [prefersDark])

  const value = useMemo(
    () => ({
      resolved,
      preference: signedIn ? getStoredTheme() : null,
      toggleTheme,
    }),
    [resolved, signedIn, toggleTheme, preferenceTick]
  )

  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>
}

export function useTheme() {
  const ctx = useContext(ThemeContext)
  if (!ctx) {
    throw new Error('useTheme must be used within ThemeProvider')
  }
  return ctx
}
