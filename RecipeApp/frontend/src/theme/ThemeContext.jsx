import { createContext, useCallback, useContext, useLayoutEffect, useMemo, useState } from 'react'
import { useAuth } from '../auth/AuthContext'
import { getStoredTheme, setStoredTheme } from './themeStorage'
import { resolveTheme } from './resolveTheme'

const ThemeContext = createContext(null)

function readPrefersDark() {
  if (typeof window === 'undefined' || typeof window.matchMedia !== 'function') return false
  return window.matchMedia('(prefers-color-scheme: dark)').matches
}

function toPreference(stored) {
  if (stored === 'light' || stored === 'dark') return stored
  return 'system'
}

export function ThemeProvider({ children }) {
  const user = useAuth()
  const signedOut = user == null
  const [prefersDark, setPrefersDark] = useState(readPrefersDark)
  const [stored, setStored] = useState(() => getStoredTheme())

  const preference = toPreference(stored)
  const systemActive = !signedOut && preference === 'system'

  useLayoutEffect(() => {
    if (!systemActive) return undefined
    if (typeof window.matchMedia !== 'function') return undefined
    const mq = window.matchMedia('(prefers-color-scheme: dark)')
    setPrefersDark(mq.matches)
    const onChange = (event) => setPrefersDark(event.matches)
    mq.addEventListener('change', onChange)
    return () => mq.removeEventListener('change', onChange)
  }, [systemActive])

  const resolved = signedOut ? 'light' : resolveTheme(stored, prefersDark)

  useLayoutEffect(() => {
    const root = document.documentElement
    if (signedOut) {
      root.classList.remove('dark')
    } else if (resolved === 'dark') {
      root.classList.add('dark')
    } else {
      root.classList.remove('dark')
    }
  }, [signedOut, resolved])

  const toggleTheme = useCallback(() => {
    if (user == null) return
    const next = resolved === 'dark' ? 'light' : 'dark'
    setStoredTheme(next)
    setStored(next)
  }, [user, resolved])

  const value = useMemo(
    () => ({
      resolved,
      preference,
      toggleTheme,
    }),
    [resolved, preference, toggleTheme]
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
