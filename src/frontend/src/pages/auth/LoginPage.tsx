import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { AuthCard } from '@/components/auth/AuthCard';
import { AuthField } from '@/components/auth/AuthField';
import { AuthLayout } from '@/components/auth/AuthLayout';
import { SubmitButton, type SubmitState } from '@/components/auth/SubmitButton';
import { login, roleHome } from '@/api/identity';
import { useAuth } from '@/auth/AuthContext';

export function LoginPage({ staff = false }: { staff?: boolean }) {
  const [state, setState] = useState<SubmitState>('idle');
  const [error, setError] = useState('');
  const { setSession } = useAuth();
  const navigate = useNavigate();

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (state === 'loading') return;
    setError('');
    setState('loading');
    const form = new FormData(event.currentTarget);
    try {
      const session = await login(String(form.get('identifier')), String(form.get('password')));
      setSession(session);
      setState('done');
      navigate(roleHome(session.user.role), { replace: true });
    } catch (reason) {
      setState('idle');
      setError(reason instanceof Error ? reason.message : 'No se pudo iniciar sesión.');
    }
  }

  return (
    <AuthLayout eyebrow={staff ? 'Acceso del personal' : 'Acceso de pasajero'}>
      <AuthCard title={staff ? 'Bienvenido al portal' : 'Inicia sesión'} description={staff ? 'Ingrese con su correo corporativo.' : 'Accede para continuar con tus viajes.'}>
        <form className="space-y-5" onSubmit={submit}>
          <AuthField id="identifier" name="identifier" label={staff ? 'Correo corporativo' : 'Correo o DNI'} type={staff ? 'email' : 'text'} autoComplete="username" required />
          <AuthField id="password" name="password" label="Contraseña" type="password" autoComplete="current-password" required />
          {error ? <p role="alert" className="rounded-button bg-danger-50 px-3 py-2 text-sm text-danger-700">{error}</p> : null}
          <SubmitButton state={state}>{staff ? 'Entrar al portal' : 'Iniciar sesión'}</SubmitButton>
        </form>
        {!staff ? <p className="mt-6 text-center text-sm text-neutral-600">¿Aún no tienes cuenta? <Link className="font-semibold text-primary-700 underline-offset-4 hover:underline" to="/pasajero/registro">Crear cuenta</Link></p> : null}
        <p className="mt-5 text-center text-sm text-white/65"><Link to="/pasajero" className="underline-offset-4 hover:underline">Volver a buscar viajes</Link></p>
      </AuthCard>
    </AuthLayout>
  );
}
