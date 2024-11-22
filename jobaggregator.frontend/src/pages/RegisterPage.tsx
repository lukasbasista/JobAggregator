import React, { useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import {
  TextField,
  Button,
  Container,
  Typography,
  Box,
  Alert,
} from '@mui/material';

const RegisterPage: React.FC = () => {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    username: '',
    email: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: '',
  });
  const [errorMessages, setErrorMessages] = useState<string[]>([]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessages([]);

    if (formData.password !== formData.confirmPassword) {
      setErrorMessages(['Hesla se neshodují']);
      return;
    }

    try {
      await axios.post('/account/register', formData);
      navigate('/login');
    } catch (error: any) {
      console.error('Registrace selhala', error);

      if (error.response && error.response.data) {
        const data = error.response.data;

        if (Array.isArray(data)) {
          const messages = data.map((item: any) => item.description || 'Došlo k chybě');
          setErrorMessages(messages);
        } else if (data.errors) {
          const messages = Object.values(data.errors).flat();
          setErrorMessages(messages as string[]);
        } else {
          setErrorMessages([data.message || 'Registrace selhala']);
        }
      } else {
        setErrorMessages(['Registrace selhala']);
      }
    }
  };

  return (
    <Container maxWidth="xs">
      <Box mt={8}>
        <Typography variant="h4" align="center" gutterBottom>
          Registrace
        </Typography>
        {errorMessages.length > 0 && (
          <Box mt={2}>
            {errorMessages.map((message, index) => (
              <Alert key={index} severity="error" sx={{ mb: 1 }}>
                {message}
              </Alert>
            ))}
          </Box>
        )}
        <form onSubmit={handleSubmit}>
          <TextField
            label="Uživatelské jméno"
            variant="outlined"
            margin="normal"
            required
            fullWidth
            value={formData.username}
            onChange={(e) => setFormData({ ...formData, username: e.target.value })}
          />
          <TextField
            label="Email"
            variant="outlined"
            margin="normal"
            required
            fullWidth
            type="email"
            value={formData.email}
            onChange={(e) => setFormData({ ...formData, email: e.target.value })}
          />
          <TextField
            label="Jméno"
            variant="outlined"
            margin="normal"
            required
            fullWidth
            value={formData.firstName}
            onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
          />
          <TextField
            label="Příjmení"
            variant="outlined"
            margin="normal"
            required
            fullWidth
            value={formData.lastName}
            onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
          />
          <TextField
            label="Heslo"
            variant="outlined"
            margin="normal"
            required
            fullWidth
            type="password"
            value={formData.password}
            onChange={(e) => setFormData({ ...formData, password: e.target.value })}
          />
          <TextField
            label="Potvrzení hesla"
            variant="outlined"
            margin="normal"
            required
            fullWidth
            type="password"
            value={formData.confirmPassword}
            onChange={(e) => setFormData({ ...formData, confirmPassword: e.target.value })}
          />
          <Button type="submit" variant="contained" color="primary" fullWidth sx={{ mt: 2 }}>
            Registrovat
          </Button>
        </form>
      </Box>
    </Container>
  );
};

export default RegisterPage;
