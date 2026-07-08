import { useState, useEffect, useCallback } from 'react';
import {
  Container, Typography, Table, TableBody, TableCell, TableContainer,
  TableHead, TableRow, Paper, Chip, Select, MenuItem, IconButton,
  CircularProgress, Alert, Box
} from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import { adminApi } from '../api/adminApi';
import { useAuth } from '../context/AuthContext';

export function AdminPage() {
  const { user: currentUser } = useAuth();
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const load = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const res = await adminApi.getAllUsers();
      setUsers(res.data);
    } catch (err) {
      setError('Greška pri učitavanju korisnika.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  const handleRoleChange = async (id, newRole) => {
    try {
      await adminApi.changeRole(id, newRole);
      setUsers((prev) => prev.map((u) => u.id === id ? { ...u, role: newRole } : u));
    } catch (err) {
      alert('Greška pri promeni role.');
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Obrisati ovog korisnika? Svi njegovi planovi putovanja biće takođe obrisani.')) return;

    try {
      await adminApi.deleteUser(id);
      setUsers((prev) => prev.filter((u) => u.id !== id));
    } catch (err) {
      alert(err.response?.data?.message || 'Greška pri brisanju korisnika.');
    }
  };

  if (loading) return <Container sx={{ mt: 4 }}><CircularProgress /></Container>;

  return (
    <Container sx={{ mt: 4, mb: 4 }}>
      <Typography variant="h4" gutterBottom>Admin panel — Korisnici</Typography>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Ime</TableCell>
              <TableCell>Email</TableCell>
              <TableCell>Tip naloga</TableCell>
              <TableCell>Rola</TableCell>
              <TableCell>Kreiran</TableCell>
              <TableCell align="right">Akcije</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {users.map((u) => (
              <TableRow key={u.id}>
                <TableCell>{u.name}</TableCell>
                <TableCell>{u.email}</TableCell>
                <TableCell>
                  <Chip
                    label={u.isLdapUser ? 'LDAP' : 'Lokalni'}
                    size="small"
                    color={u.isLdapUser ? 'info' : 'default'}
                  />
                </TableCell>
                <TableCell>
                  <Select
                    value={u.role}
                    size="small"
                    disabled={u.id === currentUser.id}
                    onChange={(e) => handleRoleChange(u.id, e.target.value)}
                  >
                    <MenuItem value="User">User</MenuItem>
                    <MenuItem value="Admin">Admin</MenuItem>
                  </Select>
                </TableCell>
                <TableCell>{new Date(u.createdAt).toLocaleDateString('sr-RS')}</TableCell>
                <TableCell align="right">
                  <IconButton
                    color="error"
                    disabled={u.id === currentUser.id}
                    onClick={() => handleDelete(u.id)}
                  >
                    <DeleteIcon />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {users.length === 0 && (
        <Box sx={{ mt: 2 }} color="text.secondary">Nema korisnika.</Box>
      )}
    </Container>
  );
}