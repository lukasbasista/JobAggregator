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
axios.defaults.baseURL = process.env.REACT_APP_API_URL;
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

