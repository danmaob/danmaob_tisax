import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import '@mantine/core/styles.css';
import '@mantine/notifications/styles.css';
import '@fontsource/inter/400.css';
import '@fontsource/inter/500.css';
import '@fontsource/inter/700.css';
import App from './App';
import { AppProviders } from './app/AppProviders';
import i18n from './i18n/i18n';
import { loadConfig } from './app/config';

const rootElement = document.getElementById('root');
if (rootElement === null) {
  throw new Error('Root element not found.');
}

void loadConfig().then(() => {
  createRoot(rootElement).render(
    <StrictMode>
      <AppProviders>
        <App />
      </AppProviders>
    </StrictMode>,
  );
}).catch(() => {
  rootElement.textContent = i18n.t('app.configError');
});
