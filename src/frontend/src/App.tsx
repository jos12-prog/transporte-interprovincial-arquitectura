import { Navigate, Route, Routes } from 'react-router-dom';
import { PassengerLayout } from '@/layouts/PassengerLayout';
import { StaffShell } from '@/layouts/StaffShell';
import { RoleLanding } from '@/pages/RoleLanding';
import { TripResultsPage } from '@/pages/TripResultsPage';
import { TripSearchPage } from '@/pages/TripSearchPage';
import { SeatSelectionPage } from '@/pages/SeatSelectionPage';

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/pasajero" replace />} />
      <Route path="/pasajero" element={<PassengerLayout />}>
        <Route index element={<TripSearchPage />} />
        <Route path="resultados" element={<TripResultsPage />} />
        <Route path="viajes/:tripId/asientos" element={<SeatSelectionPage />} />
        <Route path="*" element={<RoleLanding title="Pasajero" />} />
      </Route>
      <Route path="/agente" element={<StaffShell role="Agente" />}>
        <Route index element={<RoleLanding title="Agente" />} />
        <Route path="*" element={<RoleLanding title="Agente" />} />
      </Route>
      <Route path="/admin-agencia" element={<StaffShell role="Administrador de agencia" />}>
        <Route index element={<RoleLanding title="Administrador de agencia" />} />
        <Route path="*" element={<RoleLanding title="Administrador de agencia" />} />
      </Route>
      <Route path="/superadmin" element={<StaffShell role="Superadministrador" />}>
        <Route index element={<RoleLanding title="Superadministrador" />} />
        <Route path="*" element={<RoleLanding title="Superadministrador" />} />
      </Route>
      <Route path="*" element={<RoleLanding title="Página no encontrada" />} />
    </Routes>
  );
}
