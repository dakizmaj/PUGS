import { useState, useEffect, useCallback } from 'react';
import {
  Box, Button, Dialog, DialogTitle, DialogContent, DialogActions, TextField,
  MenuItem, Alert, CircularProgress, Card, CardContent, Typography, IconButton, Chip
} from '@mui/material';
import { DatePicker, TimePicker } from '@mui/x-date-pickers';
import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import AddIcon from '@mui/icons-material/Add';
import dayjs from 'dayjs';
import { activityApi } from '../../api/activityApi';
import { ACTIVITY_STATUSES, ACTIVITY_STATUS_LABELS } from '../../models/Activity';

const STATUS_COLORS = {
  Planned: 'default',
  Reserved: 'info',
  Completed: 'success',
  Cancelled: 'error',
};

export function ActivitiesTab({ planId }) {
  const [groupedActivities, setGroupedActivities] = useState([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState(null);

  const [name, setName] = useState('');
  const [date, setDate] = useState(dayjs());
  const [time, setTime] = useState(dayjs().hour(10).minute(0));
  const [location, setLocation] = useState('');
  const [description, setDescription] = useState('');
  const [estimatedCost, setEstimatedCost] = useState('');
  const [status, setStatus] = useState('Planned');
  const [error, setError] = useState('');

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const res = await activityApi.getCalendar(planId);
      setGroupedActivities(res.data);
    } finally {
      setLoading(false);
    }
  }, [planId]);

  useEffect(() => { load(); }, [load]);

  const openCreate = () => {
    setEditing(null);
    setName(''); setLocation(''); setDescription(''); setEstimatedCost(''); setStatus('Planned');
    setDate(dayjs()); setTime(dayjs().hour(10).minute(0));
    setError('');
    setDialogOpen(true);
  };

  const openEdit = (activity) => {
    setEditing(activity);
    setName(activity.name); setLocation(activity.location || ''); setDescription(activity.description || '');
    setEstimatedCost(String(activity.estimatedCost)); setStatus(activity.status);
    setDate(dayjs(activity.date));
    setTime(activity.time ? dayjs(`2000-01-01T${activity.time}`) : dayjs().hour(10).minute(0));
    setError('');
    setDialogOpen(true);
  };

  const handleSubmit = async () => {
    if (Number(estimatedCost) < 0) {
      setError('Procenjeni trošak ne može biti negativan.');
      return;
    }

    const payload = {
      name, location, description, status,
      date: date.toISOString(),
      time: time.format('HH:mm:ss'),
      estimatedCost: Number(estimatedCost) || 0,
    };

    try {
      if (editing) {
        await activityApi.update(planId, editing.id, payload);
      } else {
        await activityApi.create(planId, payload);
      }
      setDialogOpen(false);
      load();
    } catch (err) {
      setError(err.response?.data?.message || 'Greška pri čuvanju aktivnosti.');
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Obrisati ovu aktivnost?')) return;
    await activityApi.delete(planId, id);
    load();
  };

  if (loading) return <CircularProgress />;

  return (
    <Box>
      <Button startIcon={<AddIcon />} variant="contained" onClick={openCreate} sx={{ mb: 2 }}>
        Dodaj aktivnost
      </Button>

      {groupedActivities.length === 0 && <Box color="text.secondary">Nema unetih aktivnosti.</Box>}

      {groupedActivities.map((group) => (
        <Box key={group.date} sx={{ mb: 3 }}>
          <Typography variant="h6" color="primary" sx={{ mb: 1 }}>
            {dayjs(group.date).format('dddd, DD.MM.YYYY')}
          </Typography>

          {group.activities.map((activity) => (
            <Card key={activity.id} sx={{ mb: 1 }}>
              <CardContent sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Box>
                  <Typography variant="subtitle1">
                    {activity.time && `${activity.time.substring(0, 5)} — `}{activity.name}
                  </Typography>
                  {activity.location && <Typography variant="body2" color="text.secondary">{activity.location}</Typography>}
                  <Box sx={{ display: 'flex', gap: 1, mt: 0.5 }}>
                    <Chip label={ACTIVITY_STATUS_LABELS[activity.status]} size="small" color={STATUS_COLORS[activity.status]} />
                    <Chip label={`${activity.estimatedCost} RSD`} size="small" variant="outlined" />
                  </Box>
                </Box>
                <Box>
                  <IconButton onClick={() => openEdit(activity)}><EditIcon /></IconButton>
                  <IconButton onClick={() => handleDelete(activity.id)}><DeleteIcon /></IconButton>
                </Box>
              </CardContent>
            </Card>
          ))}
        </Box>
      ))}

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editing ? 'Izmena aktivnosti' : 'Nova aktivnost'}</DialogTitle>
        <DialogContent>
          {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
          <TextField label="Naziv" fullWidth required margin="normal" value={name} onChange={(e) => setName(e.target.value)} />
          <DatePicker label="Datum" value={date} onChange={setDate} sx={{ width: '100%', mt: 2 }} />
          <TimePicker label="Vreme" value={time} onChange={setTime} sx={{ width: '100%', mt: 2 }} />
          <TextField label="Lokacija" fullWidth margin="normal" value={location} onChange={(e) => setLocation(e.target.value)} />
          <TextField label="Opis" fullWidth multiline rows={2} margin="normal" value={description} onChange={(e) => setDescription(e.target.value)} />
          <TextField
            label="Procenjeni trošak (RSD)"
            type="number"
            fullWidth
            margin="normal"
            value={estimatedCost}
            onChange={(e) => setEstimatedCost(e.target.value)}
          />
          <TextField
            select
            label="Status"
            fullWidth
            margin="normal"
            value={status}
            onChange={(e) => setStatus(e.target.value)}
          >
            {ACTIVITY_STATUSES.map((s) => (
              <MenuItem key={s} value={s}>{ACTIVITY_STATUS_LABELS[s]}</MenuItem>
            ))}
          </TextField>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Otkaži</Button>
          <Button variant="contained" onClick={handleSubmit} disabled={!name}>Sačuvaj</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}