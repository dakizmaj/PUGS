import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { Container, Paper, TextField, Button, Typography, Box, Alert } from '@mui/material';
import { authApi } from '../api/authApi';
import { useAuth } from '../context/AuthContext';

export function RegisterPage() {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (password.length < 6) {
      setError('Lozinka mora imati najmanje 6 karaktera.');
      return;
    }

    setLoading(true);
    try {
      const response = await authApi.register({ name, email, password });
      login(response.data.token, response.data.user);
      navigate('/');
    } catch (err) {
      const message = err.response?.data?.message || 'Greška pri registraciji.';
      setError(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container maxWidth="xs" sx={{ mt: 8 }}>
      <Paper elevation={3} sx={{ p: 4 }}>
        <Typography variant="h5" align="center" gutterBottom>
          Registracija
        </Typography>

        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

        <Box component="form" onSubmit={handleSubmit}>
          <TextField
            label="Ime i prezime"
            fullWidth
            required
            margin="normal"
            value={name}
            onChange={(e) => setName(e.target.value)}
          />
          <TextField
            label="Email"
            type="email"
            fullWidth
            required
            margin="normal"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
          <TextField
            label="Lozinka"
            type="password"
            fullWidth
            required
            margin="normal"
            helperText="Najmanje 6 karaktera"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />

          <Button type="submit" variant="contained" fullWidth sx={{ mt: 2 }} disabled={loading}>
            {loading ? 'Registrovanje...' : 'Registruj se'}
          </Button>
        </Box>

        <Typography align="center" sx={{ mt: 2 }}>
          Već imate nalog? <Link to="/login">Prijavite se</Link>
        </Typography>
      </Paper>
    </Container>
  );
}