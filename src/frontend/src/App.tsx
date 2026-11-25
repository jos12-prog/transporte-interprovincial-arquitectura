import { Navigate, Route, Routes } from 'react-router-dom';
import { PassengerLayout } from '@/layouts/PassengerLayout';
import { StaffShell } from '@/layouts/StaffShell';
import { RoleLanding } from '@/pages/RoleLanding';
import { TripResultsPage } from '@/pages/TripResultsPage';
import { TripSearchPage } from '@/pages/TripSearchPage';
import { SeatSelectionPage } from '@/pages/SeatSelectionPage';
import { LoginPage } from '@/pages/auth/LoginPage';
import { RegisterPage } from '@/pages/auth/RegisterPage';
import { RequireRole } from '@/auth/RequireRole';

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/pasajero" replace />} />
      <Route path="/pasajero/login" element={<LoginPage />} />
      <Route path="/pasajero/registro" element={<RegisterPage />} />
      <Route path="/personal/login" element={<LoginPage staff />} />
      <Route path="/pasajero" element={<PassengerLayout />}>
        <Route index element={<TripSearchPage />} />
        <Route path="resultados" element={<TripResultsPage />} />
        <Route path="viajes/:tripId/asientos" element={<SeatSelectionPage />} />
        <Route path="*" element={<RoleLanding title="Pasajero" />} />
      </Route>
      <Route element={<RequireRole allowed={['agent']} />}>
        <Route path="/agente" element={<StaffShell role="Agente/Vendedor" />}>
          <Route index element={<RoleLanding title="Agente/Vendedor" />} />
          <Route path="*" element={<RoleLanding title="Agente/Vendedor" />} />
        </Route>
      </Route>
      <Route element={<RequireRole allowed={['agency_admin']} />}>
        <Route path="/admin-agencia" element={<StaffShell role="Administrador de Agencia" />}>
          <Route index element={<RoleLanding title="Administrador de Agencia" />} />
          <Route path="*" element={<RoleLanding title="Administrador de Agencia" />} />
        </Route>
      </Route>
      <Route element={<RequireRole allowed={['super_admin']} />}>
        <Route path="/superadmin" element={<StaffShell role="Superadministrador" />}>
          <Route index element={<RoleLanding title="Superadministrador" />} />
          <Route path="*" element={<RoleLanding title="Superadministrador" />} />
        </Route>
      </Route>
      <Route path="*" element={<RoleLanding title="Página no encontrada" />} />
    </Routes>
  );
}
