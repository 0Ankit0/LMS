import React from 'react';
import ReactDOM from 'react-dom/client';
import { AppRouter } from '@/router';
import { initializeTheme } from '@/lib/initialize-theme';
import '@/app/globals.css';

initializeTheme();

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <AppRouter />
  </React.StrictMode>
);