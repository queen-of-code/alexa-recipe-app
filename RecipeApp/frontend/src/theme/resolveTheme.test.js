import { describe, expect, it } from 'vitest'
import { resolveTheme } from './resolveTheme'

describe('resolveTheme', () => {
  it('returns explicit light when stored light', () => {
    expect(resolveTheme('light', true)).toBe('light')
    expect(resolveTheme('light', false)).toBe('light')
  })

  it('returns explicit dark when stored dark', () => {
    expect(resolveTheme('dark', false)).toBe('dark')
  })

  it('follows system when stored is null', () => {
    expect(resolveTheme(null, true)).toBe('dark')
    expect(resolveTheme(null, false)).toBe('light')
  })
})
