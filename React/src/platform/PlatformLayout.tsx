import { AppShell, Burger, Button, Group, Image, NavLink, Stack, Text } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { useTranslation } from 'react-i18next';
import { Link, Outlet, useLocation, useNavigate } from 'react-router';
import { LanguageSwitcher } from '../i18n/LanguageSwitcher';
import { endSession } from '../session/sessionStore';
import { usePlatformSession } from '../session/usePlatformSession';

export function PlatformLayout() {
  const { t } = useTranslation();
  const [opened, { toggle, close }] = useDisclosure();
  const session = usePlatformSession();
  const location = useLocation();
  const navigate = useNavigate();

  const handleLogout = () => {
    endSession('logout');
    void navigate('/platform/login', { replace: true });
  };

  return (
    <AppShell
      header={{ height: 60 }}
      navbar={{ width: 240, breakpoint: 'sm', collapsed: { mobile: !opened } }}
      padding="md"
    >
      <AppShell.Header>
        <Group h="100%" px="md" justify="space-between" wrap="nowrap">
          <Group gap="sm" wrap="nowrap">
            <Burger opened={opened} onClick={toggle} hiddenFrom="sm" size="sm" aria-label={t('platform.menu')} />
            <Image src="/brand/danmaob_isotipo.png" alt={t('brand.logoAlt')} h={32} w="auto" fit="contain" />
            <Text fw={700} c="danmaob.6">{t('platform.title')}</Text>
          </Group>
          <Group gap="sm" wrap="nowrap" visibleFrom="sm">
            <Text size="sm" c="dimmed">{session?.email}</Text>
            <LanguageSwitcher />
            <Button variant="light" onClick={handleLogout}>{t('platform.logout')}</Button>
          </Group>
        </Group>
      </AppShell.Header>
      <AppShell.Navbar p="md">
        <NavLink component={Link} to="/platform" label={t('platform.nav.home')} active={location.pathname === '/platform'} onClick={close} />
        <NavLink component={Link} to="/platform/tenants" label={t('platform.nav.tenants')} active={location.pathname.startsWith('/platform/tenants')} onClick={close} />
        <Stack gap="sm" mt="xl" hiddenFrom="sm">
          <Text size="sm" c="dimmed">{session?.email}</Text>
          <LanguageSwitcher />
          <Button variant="light" onClick={handleLogout}>{t('platform.logout')}</Button>
        </Stack>
      </AppShell.Navbar>
      <AppShell.Main bg="gray.0">
        <Outlet />
      </AppShell.Main>
    </AppShell>
  );
}
