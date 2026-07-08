import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { Container, Paper, TextField, Button, Typography, Box, Alert, Tabs, Tab } from '@mui/material';
import { authApi } from '../api/authApi';
import { useAuth } from '../context/AuthContext';

export function LoginPage() {
  const [tab, setTab] = useState(0); // 0 = obicna prijava, 1 = LDAP
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [username, setUsername] = useState('');
  const [ldapPassword, setLdapPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = tab === 0
        ? await authApi.login({ email, password })
        : await authApi.ldapLogin({ username, password: ldapPassword });

      login(response.data.token, response.data.user);
      navigate('/');
    } catch (err) {
      const message = err.response?.data?.message || 'Greška pri prijavi. Proverite podatke.';
      setError(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container maxWidth="xs" sx={{ mt: 8 }}>
      <Paper elevation={3} sx={{ p: 4 }}>
        <Typography variant="h5" align="center" gutterBottom>
          Prijava
        </Typography>

        <Tabs value={tab} onChange={(e, val) => setTab(val)} centered sx={{ mb: 2 }}>
          <Tab label="Obična prijava" />
          <Tab label="LDAP prijava" />
        </Tabs>

        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

        <Box component="form" onSubmit={handleSubmit}>
          {tab === 0 ? (
            <>
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
                value={password}
                onChange={(e) => setPassword(e.target.value)}
              />
            </>
          ) : (
            <>
              <TextField
                label="Korisničko ime (LDAP)"
                fullWidth
                required
                margin="normal"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
              />
              <TextField
                label="Lozinka"
                type="password"
                fullWidth
                required
                margin="normal"
                value={ldapPassword}
                onChange={(e) => setLdapPassword(e.target.value)}
              />
            </>
          )}

          <Button type="submit" variant="contained" fullWidth sx={{ mt: 2 }} disabled={loading}>
            {loading ? 'Prijavljivanje...' : 'Prijavi se'}
          </Button>
        </Box>

        <Typography align="center" sx={{ mt: 2 }}>
          Nemate nalog? <Link to="/register">Registrujte se</Link>
        </Typography>
      </Paper>
    </Container>
  );
}