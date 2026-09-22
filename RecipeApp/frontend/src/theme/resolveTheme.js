/**
 * @param {'light' | 'dark' | null} stored explicit preference or null for system
 * @param {boolean} prefersDark from matchMedia
 * @returns {'light' | 'dark'}
 */
export function resolveTheme(stored, prefersDark) {
  if (stored === 'light') return 'light'
  if (stored === 'dark') return 'dark'
  return prefersDark ? 'dark' : 'light'
}
