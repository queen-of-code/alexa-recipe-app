import { describe, expect, it } from 'vitest'
import { resolveTheme } from './resolveTheme'

describe('resolveTheme', () => {
  it('returns light when stored is null and the OS is light', () => {
    expect(resolveTheme(null, false)).toBe('light')
  })

  it('returns dark when stored is null and the OS is dark', () => {
    expect(resolveTheme(null, true)).toBe('dark')
  })

  it('returns light when stored light even if the OS is dark', () => {
    expect(resolveTheme('light', true)).toBe('light')
  })

  it('returns dark when stored dark even if the OS is light', () => {
    expect(resolveTheme('dark', false)).toBe('dark')
  })

  it('treats an unrecognized stored value as system', () => {
    expect(resolveTheme('nope', true)).toBe('dark')
  })
})
