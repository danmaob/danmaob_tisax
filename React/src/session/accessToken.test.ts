import { afterEach, describe, expect, it } from 'vitest'
import { endSession, startSession } from './sessionStore'
import { requireAccessToken } from './accessToken'

const token = 'header.' + btoa(JSON.stringify({ exp: Math.floor(Date.now() / 1000) + 600 })).replace(/=+$/, '') + '.signature'

describe('requireAccessToken', () => {
  afterEach(() => {
    endSession('logout')
  })

  it('returns the token of the active session', () => {
    startSession(token, 'admin@example.com')
    expect(requireAccessToken()).toBe(token)
  })

  it('throws when there is no session', () => {
    endSession('logout')
    expect(() => requireAccessToken()).toThrow('No platform session.')
  })
})
