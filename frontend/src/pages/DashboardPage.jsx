import { useState, useEffect, useCallback } from 'react';
import { Container, Typography, Button, Box, CircularProgress, Alert } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { travelPlanApi } from '../api/travelPlanApi';
import { TravelPlan } from '../models/TravelPlan';
import { TravelPlanCard } from '../components/travel-plans/TravelPlanCard';
import { CreateTravelPlanDialog } from '../components/travel-plans/CreateTravelPlanDialog';
import { useAuth } from '../context/AuthContext';

export function DashboardPage() {
  const { user } = useAuth();
  const [plans, setPlans] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);

  const loadPlans = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const response = await travelPlanApi.getAll();
      setPlans(response.data.map((p) => new TravelPlan(p)));
    } catch (err) {
      setError('Greška pri učitavanju planova.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadPlans();
  }, [loadPlans]);

  const handleDelete = async (id) => {
    if (!window.confirm('Da li ste sigurni da želite da obrišete ovaj plan?')) return;

    try {
      await travelPlanApi.delete(id);
      setPlans((prev) => prev.filter((p) => p.id !== id));
    } catch (err) {
      alert('Greška pri brisanju plana.');
    }
  };

  return (
    <Container sx={{ mt: 4, mb: 4 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">
          {user?.isAdmin() ? 'Svi planovi putovanja' : 'Moji planovi putovanja'}
        </Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => setDialogOpen(true)}>
          Novi plan
        </Button>
      </Box>

      {loading && <CircularProgress />}
      {error && <Alert severity="error">{error}</Alert>}

      {!loading && plans.length === 0 && (
        <Typography color="text.secondary">Nema kreiranih planova. Napravite prvi!</Typography>
      )}

      {plans.map((plan) => (
        <TravelPlanCard key={plan.id} plan={plan} onDelete={handleDelete} />
      ))}

      <CreateTravelPlanDialog
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        onCreated={loadPlans}
      />
    </Container>
  );
}