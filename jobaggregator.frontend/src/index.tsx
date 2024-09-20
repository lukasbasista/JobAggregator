import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import App from './App';
import './styles/global.css';
import { CssBaseline, GlobalStyles } from '@mui/material';
import axios from 'axios';

const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement
);
axios.defaults.baseURL = 'https://localhost:7139/api';
root.render(
  <React.StrictMode>
    <CssBaseline />
    <GlobalStyles
      styles={{
        body: { margin: 0, padding: 0 },
      }}
    />
    <App />
  </React.StrictMode>
);

