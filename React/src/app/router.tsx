import { createBrowserRouter, Navigate } from 'react-router';
import { PlatformHomePage } from '../platform/PlatformHomePage';
import { PlatformLayout } from '../platform/PlatformLayout';
import { PlatformLoginPage } from '../platform/PlatformLoginPage';
import { RequirePlatformSession } from '../platform/RequirePlatformSession';
import { TenantsPage } from '../platform/tenants/TenantsPage';

export const router = createBrowserRouter([
  {
    path: '/',
    element: <Navigate to="/platform" replace />,
  },
  {
    path: '/platform/login',
    element: <PlatformLoginPage />,
  },
  {
    path: '/platform',
    element: <RequirePlatformSession />,
    children: [
      {
        element: <PlatformLayout />,
        children: [
           {
             index: true,
             element: <PlatformHomePage />,
           },
           {
             path: 'tenants',
             element: <TenantsPage />,
           },
        ],
      },
    ],
  },
  {
    path: '*',
    element: <Navigate to="/platform" replace />,
  },
]);
