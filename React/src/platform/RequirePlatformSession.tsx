import { Navigate, Outlet } from 'react-router';
import { usePlatformSession } from '../session/usePlatformSession';

export function RequirePlatformSession() {
  const session = usePlatformSession();

  if (session === null) {
    return <Navigate to="/platform/login" replace />;
  }

  return <Outlet />;
}
