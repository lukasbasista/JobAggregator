import React, { useEffect, useState, useContext } from 'react';
import axios from 'axios';
import { AuthContext } from '../services/AuthContext';
import { TextField, Button, Container, Typography, Box, CircularProgress } from '@mui/material';
import { Navigate } from 'react-router-dom';

const ProfilePage: React.FC = () => {
  const authContext = useContext(AuthContext);
  const [profile, setProfile] = useState<any>(null);
  const [isFetching, setIsFetching] = useState(true);
  const [errorMessage, setErrorMessage] = useState('');

  const isAuthenticated = authContext?.isAuthenticated ?? false;
  const isLoading = authContext?.isLoading ?? false;

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const response = await axios.get('/account/profile');
        setProfile(response.data);
      } catch (error) {
        console.error('Error fetching profile', error);
        setErrorMessage('Nepodařilo se načíst profil');
      } finally {
        setIsFetching(false);
      }
    };
    if (isAuthenticated) {
      fetchProfile();
    } else {
      setIsFetching(false);
    }
  }, [isAuthenticated]);

  if (isLoading || isFetching) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
        <CircularProgress />
      </Box>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" />;
  }

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await axios.put('/account/profile', profile);
      setErrorMessage('Profil byl úspěšně aktualizován');
    } catch (error) {
      console.error('Profile update failed', error);
      setErrorMessage('Nepodařilo se aktualizovat profil');
    }
  };

  return (
    <Container maxWidth="sm">
      <Box mt={8}>
        <Typography variant="h4" align="center" gutterBottom>
          Profil
        </Typography>
        {errorMessage && (
          <Typography
            color={errorMessage.includes('úspěšně') ? 'primary' : 'error'}
            variant="body1"
          >
            {errorMessage}
          </Typography>
        )}
        {profile && (
          <form onSubmit={handleUpdate}>
            <TextField
              label="Jméno"
              variant="outlined"
              margin="normal"
              fullWidth
              value={profile.firstName || ''}
              onChange={(e) => setProfile({ ...profile, firstName: e.target.value })}
            />
            <TextField
              label="Příjmení"
              variant="outlined"
              margin="normal"
              fullWidth
              value={profile.lastName || ''}
              onChange={(e) => setProfile({ ...profile, lastName: e.target.value })}
            />
            <TextField
              label="Email"
              variant="outlined"
              margin="normal"
              fullWidth
              value={profile.email || ''}
              onChange={(e) => setProfile({ ...profile, email: e.target.value })}
            />
            <Button type="submit" variant="contained" color="primary" fullWidth sx={{ mt: 2 }}>
              Aktualizovat profil
            </Button>
          </form>
        )}
      </Box>
    </Container>
  );
};

export default ProfilePage;
