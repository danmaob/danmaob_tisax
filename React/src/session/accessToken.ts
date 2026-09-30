import { getSession } from './sessionStore'

export function requireAccessToken(): string {
  const session = getSession()
  if (!session) throw new Error('No platform session.')
  return session.accessToken
}
