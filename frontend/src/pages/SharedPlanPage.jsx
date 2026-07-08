import { useState, useEffect, useCallback } from 'react';
import { useParams } from 'react-router-dom';
import {
  Container, Typography, Box, CircularProgress, Alert, Card, CardContent,
  List, ListItem, ListItemText, TextField, Button, Chip
} from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import dayjs from 'dayjs';
import { shareApi } from '../api/shareApi';
import { sharedPlanApi } from '../api/sharedPlanApi';
import { useAuth } from '../context/AuthContext';

export function SharedPlanPage() {
  const { token } = useParams();
  const { user } = useAuth();

  const [plan, setPlan] = useState(null);
  const [accessLevel, setAccessLevel] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [editMode, setEditMode] = useState(false);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [startDate, setStartDate] = useState(dayjs());
  const [endDate, setEndDate] = useState(dayjs());
  const [budget, setBudget] = useState('');
  const [notes, setNotes] = useState('');
  const [saving, setSaving] = useState(false);
  const [saveError, setSaveError] = useState('');

  const load = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const validation = await shareApi.validate(token);
      if (!validation.data.isValid) {
        setError(validation.data.reason || 'Nevažeći link za deljenje.');
        setLoading(false);
        return;
      }
      setAccessLevel(validation.data.accessLevel);

      const planRes = await sharedPlanApi.getShared(token);
      setPlan(planRes.data);
      setName(planRes.data.name);
      setDescription(planRes.data.description || '');
      setStartDate(dayjs(planRes.data.startDate));
      setEndDate(dayjs(planRes.data.endDate));
      setBudget(String(planRes.data.budget));
      setNotes(planRes.data.notes || '');
    } catch (err) {
      setError('Plan nije pronađen ili link više ne važi.');
    } finally {
      setLoading(false);
    }
  }, [token]);

  useEffect(() => { load(); }, [load]);

  const handleSave = async () => {
    setSaveError('');
    if (endDate.isBefore(startDate)) {
      setSaveError('Krajnji datum ne može biti pre početnog.');
      return;
    }

    setSaving(true);
    try {
      await sharedPlanApi.updateShared(token, {
        name, description,
        startDate: startDate.toISOString(),
        endDate: endDate.toISOString(),
        budget: Number(budget) || 0,
        notes,
      });
      setEditMode(false);
      load();
    } catch (err) {
      setSaveError(err.response?.data?.message || 'Greška pri čuvanju izmena.');
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <Container sx={{ mt: 4 }}><CircularProgress /></Container>;
  if (error) return <Container sx={{ mt: 4 }}><Alert severity="error">{error}</Alert></Container>;
  if (!plan) return null;

  const canEdit = accessLevel === 'Edit';

  return (
    <Container sx={{ mt: 4, mb: 4 }}>
      <Chip label={canEdit ? 'Pristup: Pregled i uređivanje' : 'Pristup: Samo pregled'} sx={{ mb: 2 }} />

      {canEdit && !user && (
        <Alert severity="info" sx={{ mb: 2 }}>
          Ovaj link dozvoljava uređivanje, ali morate biti prijavljeni da biste sačuvali izmene.
        </Alert>
      )}

      {editMode ? (
        <Card>
          <CardContent>
            {saveError && <Alert severity="error" sx={{ mb: 2 }}>{saveError}</Alert>}
            <TextField label="Naziv" fullWidth margin="normal" value={name} onChange={(e) => setName(e.target.value)} />
            <TextField label="Opis" fullWidth multiline rows={2} margin="normal" value={description} onChange={(e) => setDescription(e.target.value)} />
            <DatePicker label="Početni datum" value={startDate} onChange={setStartDate} sx={{ width: '100%', mt: 2 }} />
            <DatePicker label="Krajnji datum" value={endDate} onChange={setEndDate} sx={{ width: '100%', mt: 2 }} />
            <TextField label="Budžet (RSD)" type="number" fullWidth margin="normal" value={budget} onChange={(e) => setBudget(e.target.value)} />
            <TextField label="Napomene" fullWidth multiline rows={2} margin="normal" value={notes} onChange={(e) => setNotes(e.target.value)} />
            <Box sx={{ display: 'flex', gap: 1, mt: 2 }}>
              <Button variant="contained" onClick={handleSave} disabled={saving}>
                {saving ? 'Čuvanje...' : 'Sačuvaj'}
              </Button>
              <Button onClick={() => setEditMode(false)}>Otkaži</Button>
            </Box>
          </CardContent>
        </Card>
      ) : (
        <>
          <Typography variant="h4">{plan.name}</Typography>
          <Typography color="text.secondary" gutterBottom>{plan.description}</Typography>
          <Typography>
            {dayjs(plan.startDate).format('DD.MM.YYYY')} - {dayjs(plan.endDate).format('DD.MM.YYYY')}
          </Typography>
          <Typography sx={{ mt: 1 }}>Budžet: {plan.budget} RSD</Typography>
          {plan.notes && <Typography sx={{ mt: 1 }}>Napomene: {plan.notes}</Typography>}

          {canEdit && user && (
            <Button variant="contained" sx={{ mt: 2 }} onClick={() => setEditMode(true)}>
              Uredi plan
            </Button>
          )}

          <Typography variant="h6" sx={{ mt: 4 }}>Destinacije</Typography>
          <List>
            {plan.destinations?.map((d) => (
              <ListItem key={d.id}>
                <ListItemText primary={`${d.name} — ${d.location}`} secondary={`${dayjs(d.arrivalDate).format('DD.MM.YYYY')} - ${dayjs(d.departureDate).format('DD.MM.YYYY')}`} />
              </ListItem>
            ))}
          </List>

          <Typography variant="h6" sx={{ mt: 2 }}>Aktivnosti</Typography>
          <List>
            {plan.activities?.map((a) => (
              <ListItem key={a.id}>
                <ListItemText primary={a.name} secondary={`${dayjs(a.date).format('DD.MM.YYYY')} — ${a.estimatedCost} RSD`} />
              </ListItem>
            ))}
          </List>

          <Typography variant="h6" sx={{ mt: 2 }}>Checklist</Typography>
          <List>
            {plan.checklistItems?.map((c) => (
              <ListItem key={c.id}>
                <ListItemText primary={c.title} sx={c.isCompleted ? { textDecoration: 'line-through' } : {}} />
              </ListItem>
            ))}
          </List>
        </>
      )}
    </Container>
  );
}