import type { Config } from 'tailwindcss';

export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        primary: {
          900: '#063F36',
          700: '#0B5C4E',
          600: '#0E6B5C',
          100: '#DDF2EC',
        },
        accent: {
          500: '#F2A93B',
          100: '#FDF1DC',
        },
        neutral: {
          900: '#111B19',
          700: '#34413E',
          600: '#4A5754',
          500: '#62706C',
          400: '#8D9995',
          200: '#E0E6E4',
          50: '#F6F8F7',
        },
        success: { 600: '#16A34A' },
        danger: { 700: '#B91C1C', 600: '#DC2626', 100: '#FEE2E2', 50: '#FEF2F2' },
        warning: { 500: '#F59E0B' },
      },
      fontFamily: {
        display: ['Plus Jakarta Sans', 'Inter', 'sans-serif'],
        sans: ['Inter', 'system-ui', 'sans-serif'],
      },
      spacing: {
        grid: '8px',
        'page-gutter': '16px',
      },
      maxWidth: {
        container: '1200px',
      },
      borderRadius: {
        button: '12px',
        card: '16px',
        modal: '24px',
        pill: '999px',
      },
      boxShadow: {
        sm: '0 1px 2px rgba(17,27,25,.05), 0 1px 1px rgba(17,27,25,.03)',
        md: '0 1px 2px rgba(17,27,25,.04), 0 8px 24px -6px rgba(17,27,25,.10)',
        lg: '0 2px 6px rgba(6,63,54,.06), 0 24px 48px -12px rgba(6,63,54,.22)',
        layered: '0 1px 2px rgba(17,27,25,.04), 0 8px 24px -6px rgba(17,27,25,.10)',
        auth: '0 0 0 1px rgba(0,0,0,.35), inset 0 1px 0 rgba(255,255,255,.08), 0 24px 64px -24px rgba(0,0,0,.70)',
      },
      transitionTimingFunction: {
        'brand-out': 'cubic-bezier(.23,1,.32,1)',
        'brand-in-out': 'cubic-bezier(.77,0,.175,1)',
        drawer: 'cubic-bezier(.32,.72,0,1)',
      },
      keyframes: {
        'auth-enter': { from: { opacity: '0', transform: 'translateY(14px) scale(.985)', filter: 'blur(6px)' }, to: { opacity: '1', transform: 'translateY(0) scale(1)', filter: 'blur(0)' } },
        'auth-fade': { from: { opacity: '0' }, to: { opacity: '1' } },
        'menu-in': { from: { opacity: '0', transform: 'scale(.98)' }, to: { opacity: '1', transform: 'scale(1)' } },
      },
      animation: {
        'auth-enter': 'auth-enter 560ms cubic-bezier(.23,1,.32,1) both',
        'auth-fade': 'auth-fade 320ms ease-out both',
        'menu-in': 'menu-in 150ms cubic-bezier(.23,1,.32,1) both',
      },
    },
  },
  plugins: [],
} satisfies Config;
