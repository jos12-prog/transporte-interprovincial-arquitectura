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
          900: '#0F172A',
          600: '#475569',
          200: '#E2E8F0',
          50: '#F8FAFC',
        },
        success: { 600: '#16A34A' },
        danger: { 600: '#DC2626' },
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
        sm: '0 1px 2px rgba(15, 23, 42, 0.06)',
        md: '0 8px 24px rgba(15, 23, 42, 0.08)',
        lg: '0 24px 48px rgba(6, 63, 54, 0.18)',
      },
    },
  },
  plugins: [],
} satisfies Config;
