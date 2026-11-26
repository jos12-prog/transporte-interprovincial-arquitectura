import { fireEvent, render, screen } from '@testing-library/react';
import { ConfirmDialog, RowActions, StatusBadge, Toast } from '@/components/admin/RowActions';

describe('acciones compartidas de filas', () => {
  it('muestra acciones visibles con iconos, nombres accesibles y color de peligro', () => {
    const onEdit = jest.fn();
    render(<RowActions name="vehículo AEX-PUQ-H01" actions={[
      { id: 'edit', label: 'Editar', onSelect: onEdit },
      { id: 'deactivate', label: 'Desactivar', onSelect: jest.fn() },
    ]} />);
    const edit = screen.getByRole('button', { name: 'Editar vehículo AEX-PUQ-H01' });
    const deactivate = screen.getByRole('button', { name: 'Desactivar vehículo AEX-PUQ-H01' });
    expect(edit.querySelector('svg')).toBeInTheDocument();
    expect(edit).toHaveClass('border-primary-600/30', 'text-primary-700');
    expect(deactivate.querySelector('svg')).toBeInTheDocument();
    expect(deactivate).toHaveClass('border-danger-600/30', 'text-danger-700');
    expect(deactivate).toHaveAttribute('title', 'Desactivar vehículo AEX-PUQ-H01');
    expect(screen.queryByRole('menu')).not.toBeInTheDocument();
    expect(onEdit).not.toHaveBeenCalled();
  });

  it.each([
    ['active', 'Activo', 'text-[#15803D]'], ['inactive', 'Inactivo', 'text-neutral-600'],
    ['scheduled', 'Programado', 'text-[#15803D]'], ['cancelled', 'Cancelado', 'text-[#B91C1C]'],
  ] as const)('muestra el badge %s con icono', (status, label, color) => {
    render(<StatusBadge status={status} />);
    const badge = screen.getByText(label).closest('span')!;
    expect(badge).toHaveClass(color);
    expect(badge).toHaveClass('plain');
    expect(badge.querySelector('svg')).toBeInTheDocument();
  });

  it.each(['success', 'error'] as const)('presenta y permite cerrar el toast %s', tone => {
    render(<Toast message="Resultado" tone={tone} onClose={jest.fn()} />);
    expect(screen.getByRole('status')).toHaveTextContent('Resultado');
    expect(screen.getByRole('button', { name: 'Cerrar notificación' })).toBeInTheDocument();
  });

  it('invoca la acción destructiva solo al confirmar', () => {
    const onConfirm = jest.fn();
    render(<ConfirmDialog open title="¿Desactivar vehículo AEX-PUQ-H01?" consequence="No podrá asignarse a nuevos viajes." confirmLabel="Desactivar" onConfirm={onConfirm} onCancel={jest.fn()} />);
    expect(screen.getByRole('alertdialog')).toHaveTextContent('No podrá asignarse a nuevos viajes.');
    expect(onConfirm).not.toHaveBeenCalled();
    const confirm = screen.getByRole('button', { name: 'Desactivar' });
    expect(confirm.querySelector('svg')).toHaveClass('lucide-ban');
    fireEvent.click(confirm);
    expect(onConfirm).toHaveBeenCalledTimes(1);
  });

  it('usa CircleX al confirmar la cancelación de un viaje', () => {
    render(<ConfirmDialog open title="¿Cancelar viaje Ayacucho → Lima?" consequence="Se liberarán los asientos retenidos." confirmLabel="Cancelar viaje" onConfirm={jest.fn()} onCancel={jest.fn()} />);
    expect(screen.getByRole('button', { name: 'Cancelar viaje' }).querySelector('svg')).toHaveClass('lucide-circle-x');
  });
});
