import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { roleHome, type TechnicalRole } from '@/api/identity';
import { useAuth } from '@/auth/AuthContext';

export function RequireRole({ allowed }: { allowed: TechnicalRole[] }) {
  const { session } = useAuth();
  const location = useLocation();
  if (!session) return <Navigate to={location.pathname.startsWith('/pasajero') ? '/pasajero/login' : '/personal/login'} replace state={{ from: location }} />;
  if (!allowed.includes(session.user.role)) return <Navigate to={roleHome(session.user.role)} replace />;
  return <Outlet />;
}
