import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import App from './App';
import i18n from './i18n/i18n';
import { renderWithProviders } from './test/renderWithProviders';

describe('App', () => {
  it('renders the product name and the ready message in Spanish', async () => {
    await i18n.changeLanguage('es');
    renderWithProviders(<App />);
    expect(screen.getByRole('heading', { level: 1, name: 'DANMAOB TISAX Compliance Manager' })).toBeInTheDocument();
    expect(screen.getByText('La base del frontend está lista.')).toBeInTheDocument();
  });

  it('shows the texts in English after switching language', async () => {
    await i18n.changeLanguage('es');
    const user = userEvent.setup();
    renderWithProviders(<App />);
    await user.click(screen.getByText('EN'));
    expect(await screen.findByText('The frontend foundation is ready.')).toBeInTheDocument();
  });
});
