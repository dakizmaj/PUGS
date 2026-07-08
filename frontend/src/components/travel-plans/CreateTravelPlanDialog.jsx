import { useState } from 'react';
import {
  Dialog, DialogTitle, DialogContent, DialogActions,
  TextField, Button, Alert
} from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import dayjs from 'dayjs';
import { travelPlanApi } from '../../api/travelPlanApi';

export function CreateTravelPlanDialog({ open, onClose, onCreated }) {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [startDate, setStartDate] = useState(dayjs());
  const [endDate, setEndDate] = useState(dayjs().add(3, 'day'));
  const [budget, setBudget] = useState('');
  const [notes, setNotes] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const resetForm = () => {
    setName(''); setDescription(''); setBudget(''); setNotes('');
    setStartDate(dayjs()); setEndDate(dayjs().add(3, 'day')); setError('');
  };

  const handleSubmit = async () => {
    setError('');

    if (endDate.isBefore(startDate)) {
      setError('Krajnji datum ne može biti pre početnog.');
      return;
    }
    if (Number(budget) < 0) {
      setError('Budžet ne može biti negativan.');
      return;
    }

    setLoading(true);
    try {
      await travelPlanApi.create({
        name,
        description,
        startDate: startDate.toISOString(),
        endDate: endDate.toISOString(),
        budget: Number(budget) || 0,
        notes,
      });
      resetForm();
      onCreated();
      onClose();
    } catch (err) {
      setError(err.response?.data?.message || 'Greška pri kreiranju plana.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Novi plan putovanja</DialogTitle>
      <DialogContent>
        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

        <TextField
          label="Naziv putovanja"
          fullWidth
          required
          margin="normal"
          value={name}
          onChange={(e) => setName(e.target.value)}
        />
        <TextField
          label="Opis"
          fullWidth
          multiline
          rows={2}
          margin="normal"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
        />

        <DatePicker
          label="Početni datum"
          value={startDate}
          onChange={setStartDate}
          sx={{ width: '100%', mt: 2 }}
        />
        <DatePicker
          label="Krajnji datum"
          value={endDate}
          onChange={setEndDate}
          sx={{ width: '100%', mt: 2 }}
        />

        <TextField
          label="Planirani budžet (RSD)"
          type="number"
          fullWidth
          margin="normal"
          value={budget}
          onChange={(e) => setBudget(e.target.value)}
        />
        <TextField
          label="Napomene"
          fullWidth
          multiline
          rows={2}
          margin="normal"
          value={notes}
          onChange={(e) => setNotes(e.target.value)}
        />
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Otkaži</Button>
        <Button variant="contained" onClick={handleSubmit} disabled={loading || !name}>
          {loading ? 'Kreiranje...' : 'Kreiraj'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}