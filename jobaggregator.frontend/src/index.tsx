import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import App from './App';
import './styles/global.css';
import { Box, CssBaseline, GlobalStyles } from '@mui/material';
import axios from 'axios';
import { BrowserRouter } from 'react-router-dom';

const root = ReactDOM.createRoot(document.getElementById('root') as HTMLElement);
axios.defaults.baseURL = process.env.REACT_APP_API_URL || 'http://localhost:5000'; // Ensure a default baseURL

root.render(
  <React.StrictMode>
    <BrowserRouter>
      <CssBaseline />
      <GlobalStyles
        styles={{
          body: { margin: 0, padding: 0 },
        }}
      />
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          minHeight: '100vh',
        }}
      >
        <App />
      </Box>
    </BrowserRouter>
  </React.StrictMode>
);
