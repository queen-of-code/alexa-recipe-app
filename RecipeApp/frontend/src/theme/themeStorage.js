export const THEME_STORAGE_KEY = 'alexa-recipe-theme'

/** @returns {'light' | 'dark' | null} null = use system preference */
export function getStoredTheme() {
  try {
    const value = localStorage.getItem(THEME_STORAGE_KEY)
    if (value === 'light' || value === 'dark') return value
  } catch {
    /* private mode / blocked storage */
  }
  return null
}

/** @param {'light' | 'dark'} theme */
export function setStoredTheme(theme) {
  try {
    localStorage.setItem(THEME_STORAGE_KEY, theme)
  } catch {
    /* ignore */
  }
}
