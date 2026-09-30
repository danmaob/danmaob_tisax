import { createBrowserRouter, Navigate } from 'react-router';
import { PlatformHomePage } from '../platform/PlatformHomePage';
import { PlatformLayout } from '../platform/PlatformLayout';
import { PlatformLoginPage } from '../platform/PlatformLoginPage';
import { RequirePlatformSession } from '../platform/RequirePlatformSession';

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
        ],
      },
    ],
  },
  {
    path: '*',
    element: <Navigate to="/platform" replace />,
  },
]);
