import { Alert, Box, Button, Center, Group, Image, Paper, PasswordInput, Stack, Text, TextInput, Title } from '@mantine/core';
import { useMutation } from '@tanstack/react-query';
import { useState } from 'react';
import type { FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router';
import { ApiError } from '../api/ApiError';
import { loginPlatformAdmin } from '../api/platformAuth';
import { LanguageSwitcher } from '../i18n/LanguageSwitcher';
import { getLastEndReason, startSession } from '../session/sessionStore';

export function PlatformLoginPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const loginMutation = useMutation({ mutationFn: (credentials: { email: string; password: string }) => loginPlatformAdmin(credentials.email, credentials.password), onSuccess: (accessToken, credentials) => { startSession(accessToken, credentials.email); void navigate('/platform', { replace: true }); } });
  const handleSubmit = (event: FormEvent<HTMLFormElement>) => { event.preventDefault(); loginMutation.mutate({ email: email.trim(), password }); };
  let errorMessage: string | null = null;
  if (loginMutation.error instanceof ApiError && loginMutation.error.status === 401) { errorMessage = t('login.errors.invalidCredentials'); }
  else if (loginMutation.error instanceof ApiError && loginMutation.error.status === 400) { errorMessage = t('login.errors.invalidInput'); }
  else if (loginMutation.error !== null) { errorMessage = t('login.errors.unavailable'); }
  const sessionExpired = getLastEndReason() === 'expired' && loginMutation.isPending === false && loginMutation.isSuccess === false;
  return (
    <Box mih="100vh" bg="gray.0">
      <Group justify="flex-end" p="md">
        <LanguageSwitcher />
      </Group>
      <Center px="md" pb="xl">
        <Paper withBorder radius="md" p="xl" w="100%" maw={420}>
          <form onSubmit={handleSubmit}>
            <Stack gap="md">
              <Image src="/brand/danmaob_logo_principal.png" alt={t('brand.logoAlt')} h={56} w="auto" fit="contain" mx="auto" />
              <Title order={2} ta="center" c="danmaob.6">{t('login.title')}</Title>
              <Text ta="center" c="dimmed" size="sm">{t('login.subtitle')}</Text>
              {sessionExpired && <Alert color="orange">{t('login.sessionExpired')}</Alert>}
              {errorMessage !== null && <Alert color="red">{errorMessage}</Alert>}
              <TextInput label={t('login.email')} type="email" autoComplete="username" value={email} onChange={(event) => setEmail(event.currentTarget.value)} />
              <PasswordInput label={t('login.password')} autoComplete="current-password" value={password} onChange={(event) => setPassword(event.currentTarget.value)} />
              <Button type="submit" fullWidth loading={loginMutation.isPending} disabled={email.trim().length === 0 || password.length === 0}>{t('login.submit')}</Button>
            </Stack>
          </form>
        </Paper>
      </Center>
    </Box>
  );
}
