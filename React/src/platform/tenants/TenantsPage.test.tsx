import { screen } from '@testing-library/react'
import { beforeEach, describe, expect, it } from 'vitest'
import i18n from '../../i18n/i18n'
import { renderWithProviders } from '../../test/renderWithProviders'
import { TenantsPage } from './TenantsPage'

describe('TenantsPage', () => {
  beforeEach(async () => {
    await i18n.changeLanguage('es')
  })

  it('shows the title and the create button', () => {
    renderWithProviders(<TenantsPage />)

    expect(screen.getByRole('heading', { level: 2, name: 'Tenants' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Nuevo tenant' })).toBeInTheDocument()
  })
})
