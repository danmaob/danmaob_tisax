import { notifications } from '@mantine/notifications';

export function showSuccess(message: string): void {
  notifications.show({ color: 'green', message });
}

export function showError(message: string): void {
  notifications.show({ color: 'red', message });
}
