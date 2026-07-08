import { useState, useEffect, useCallback } from 'react';
import {
  Box, Button, List, ListItem, ListItemText, IconButton, Dialog, DialogTitle,
  DialogContent, DialogActions, TextField, Alert, CircularProgress
} from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import AddIcon from '@mui/icons-material/Add';
import dayjs from 'dayjs';
import { destinationApi } from '../../api/destinationApi';
import { Destination } from '../../models/Destination';

export function DestinationsTab({ planId }) {
  const [destinations, setDestinations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState(null); // null = kreiranje, objekat = izmena

  const [name, setName] = useState('');
  const [location, setLocation] = useState('');
  const [arrivalDate, setArrivalDate] = useState(dayjs());
  const [departureDate, setDepartureDate] = useState(dayjs().add(2, 'day'));
  const [notes, setNotes] = useState('');
  const [error, setError] = useState('');

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const res = await destinationApi.getAll(planId);
      setDestinations(res.data.map((d) => new Destination(d)));
    } finally {
      setLoading(false);
    }
  }, [planId]);

  useEffect(() => { load(); }, [load]);

  const openCreate = () => {
    setEditing(null);
    setName(''); setLocation(''); setNotes('');
    setArrivalDate(dayjs()); setDepartureDate(dayjs().add(2, 'day'));
    setError('');
    setDialogOpen(true);
  };

  const openEdit = (dest) => {
    setEditing(dest);
    setName(dest.name); setLocation(dest.location); setNotes(dest.notes || '');
    setArrivalDate(dayjs(dest.arrivalDate)); setDepartureDate(dayjs(dest.departureDate));
    setError('');
    setDialogOpen(true);
  };

  const handleSubmit = async () => {
    if (departureDate.isBefore(arrivalDate)) {
      setError('Datum odlaska ne može biti pre datuma dolaska.');
      return;
    }

    const payload = {
      name, location, notes,
      arrivalDate: arrivalDate.toISOString(),
      departureDate: departureDate.toISOString(),
    };

    try {
      if (editing) {
        await destinationApi.update(planId, editing.id, payload);
      } else {
        await destinationApi.create(planId, payload);
      }
      setDialogOpen(false);
      load();
    } catch (err) {
      setError(err.response?.data?.message || 'Greška pri čuvanju destinacije.');
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Obrisati ovu destinaciju?')) return;
    await destinationApi.delete(planId, id);
    load();
  };

  if (loading) return <CircularProgress />;

  return (
    <Box>
      <Button startIcon={<AddIcon />} variant="contained" onClick={openCreate} sx={{ mb: 2 }}>
        Dodaj destinaciju
      </Button>

      <List>
        {destinations.map((dest) => (
          <ListItem
            key={dest.id}
            secondaryAction={
              <>
                <IconButton onClick={() => openEdit(dest)}><EditIcon /></IconButton>
                <IconButton onClick={() => handleDelete(dest.id)}><DeleteIcon /></IconButton>
              </>
            }
          >
            <ListItemText
              primary={`${dest.name} — ${dest.location}`}
              secondary={`${dayjs(dest.arrivalDate).format('DD.MM.YYYY')} - ${dayjs(dest.departureDate).format('DD.MM.YYYY')}${dest.notes ? ' | ' + dest.notes : ''}`}
            />
          </ListItem>
        ))}
      </List>

      {destinations.length === 0 && <Box color="text.secondary">Nema unetih destinacija.</Box>}

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editing ? 'Izmena destinacije' : 'Nova destinacija'}</DialogTitle>
        <DialogContent>
          {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
          <TextField label="Naziv" fullWidth required margin="normal" value={name} onChange={(e) => setName(e.target.value)} />
          <TextField label="Lokacija" fullWidth required margin="normal" value={location} onChange={(e) => setLocation(e.target.value)} />
          <DatePicker label="Datum dolaska" value={arrivalDate} onChange={setArrivalDate} sx={{ width: '100%', mt: 2 }} />
          <DatePicker label="Datum odlaska" value={departureDate} onChange={setDepartureDate} sx={{ width: '100%', mt: 2 }} />
          <TextField label="Napomene" fullWidth multiline rows={2} margin="normal" value={notes} onChange={(e) => setNotes(e.target.value)} />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Otkaži</Button>
          <Button variant="contained" onClick={handleSubmit} disabled={!name || !location}>Sačuvaj</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}