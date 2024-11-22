import React, { useState, useContext } from 'react';
import { AuthContext } from '../services/AuthContext';
import axios from 'axios';
import { useNavigate, Navigate } from 'react-router-dom';
import { TextField, Button, Container, Typography, Box, CircularProgress } from '@mui/material';

const LoginPage: React.FC = () => {
  const authContext = useContext(AuthContext);
  const navigate = useNavigate();
  const [credentials, setCredentials] = useState({ username: '', password: '' });
  const [errorMessage, setErrorMessage] = useState('');

  if (!authContext) {
    return null;
  }

  const { login, isAuthenticated, isLoading } = authContext;

  if (isLoading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
        <CircularProgress />
      </Box>
    );
  }

  if (isAuthenticated) {
    return <Navigate to="/" />;
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage('');
    try {
      const response = await axios.post('/account/login', credentials);
      login(response.data.token);
      navigate('/');
    } catch (error: any) {
      console.error('Přihlášení selhalo', error);
      if (error.response && error.response.data) {
        setErrorMessage(error.response.data.message || 'Přihlášení selhalo');
      } else {
        setErrorMessage('Přihlášení selhalo');
      }
    }
  };

  return (
    <Container maxWidth="xs">
      <Box mt={8}>
        <Typography variant="h4" align="center" gutterBottom>
          Přihlášení
        </Typography>
        {errorMessage && (
          <Typography color="error" variant="body1">
            {errorMessage}
          </Typography>
        )}
        <form onSubmit={handleSubmit}>
          <TextField
            label="Uživatelské jméno"
            variant="outlined"
            margin="normal"
            required
            fullWidth
            value={credentials.username}
            onChange={(e) => setCredentials({ ...credentials, username: e.target.value })}
          />
          <TextField
            label="Heslo"
            variant="outlined"
            margin="normal"
            required
            fullWidth
            type="password"
            value={credentials.password}
            onChange={(e) => setCredentials({ ...credentials, password: e.target.value })}
          />
          <Button type="submit" variant="contained" color="primary" fullWidth sx={{ mt: 2 }}>
            Přihlásit se
          </Button>
        </form>
      </Box>
    </Container>
  );
};

export default LoginPage;
