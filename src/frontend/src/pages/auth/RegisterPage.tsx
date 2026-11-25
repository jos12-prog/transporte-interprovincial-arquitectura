import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { registerPassenger } from '@/api/identity';
import { AuthCard } from '@/components/auth/AuthCard';
import { AuthField } from '@/components/auth/AuthField';
import { AuthLayout } from '@/components/auth/AuthLayout';
import { SubmitButton, type SubmitState } from '@/components/auth/SubmitButton';

export function RegisterPage() {
  const [state, setState] = useState<SubmitState>('idle');
  const [error, setError] = useState('');
  const navigate = useNavigate();

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (state === 'loading') return;
    setError('');
    setState('loading');
    const form = new FormData(event.currentTarget);
    try {
      await registerPassenger({
        fullName: String(form.get('fullName')),
        dni: String(form.get('dni')),
        email: String(form.get('email')),
        phoneNumber: String(form.get('phoneNumber')),
        password: String(form.get('password')),
      });
      setState('done');
      navigate('/pasajero/login', { replace: true, state: { registered: true } });
    } catch (reason) {
      setState('idle');
      setError(reason instanceof Error ? reason.message : 'No se pudo crear la cuenta.');
    }
  }

  return (
    <AuthLayout eyebrow="Crea tu cuenta">
      <AuthCard title="Regístrate como pasajero" description="Completa tus datos para comenzar a planificar tus viajes.">
        <form className="space-y-4" onSubmit={submit}>
          <AuthField id="fullName" name="fullName" label="Nombre completo" autoComplete="name" required />
          <div className="grid gap-4 sm:grid-cols-2">
            <AuthField id="dni" name="dni" label="DNI" inputMode="numeric" pattern="[0-9]{8}" minLength={8} maxLength={8} autoComplete="off" required />
            <AuthField id="phoneNumber" name="phoneNumber" label="Celular" type="tel" autoComplete="tel" required />
          </div>
          <AuthField id="email" name="email" label="Correo electrónico" type="email" autoComplete="email" required />
          <AuthField id="password" name="password" label="Contraseña" type="password" minLength={8} autoComplete="new-password" required />
          {error ? <p role="alert" className="rounded-button bg-danger-50 px-3 py-2 text-sm text-danger-700">{error}</p> : null}
          <SubmitButton state={state}>Crear cuenta</SubmitButton>
        </form>
        <p className="mt-6 text-center text-sm text-white/70">¿Ya tienes cuenta? <Link className="font-semibold text-accent-500 underline-offset-4 hover:underline" to="/pasajero/login">Iniciar sesión</Link></p>
      </AuthCard>
    </AuthLayout>
  );
}
