import { useState, useEffect, useCallback } from 'react';
import { useParams } from 'react-router-dom';
import { Container, Typography, Tabs, Tab, Box, CircularProgress } from '@mui/material';
import { travelPlanApi } from '../api/travelPlanApi';
import { DestinationsTab } from '../components/destinations/DestinationsTab';
import { ActivitiesTab } from '../components/activities/ActivitiesTab';
import { Button } from '@mui/material';
import DownloadIcon from '@mui/icons-material/Download';
import { BudgetTab } from '../components/budget/BudgetTab';
import { ChecklistTab } from '../components/checklist/ChecklistTab';

export function TravelPlanDetailPage() {
  const { id } = useParams();
  const [plan, setPlan] = useState(null);
  const [loading, setLoading] = useState(true);
  const [tab, setTab] = useState(0);

  const loadPlan = useCallback(async () => {
    setLoading(true);
    try {
      const res = await travelPlanApi.getById(id);
      setPlan(res.data);
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => { loadPlan(); }, [loadPlan]);

  if (loading) return <Container sx={{ mt: 4 }}><CircularProgress /></Container>;
  if (!plan) return <Container sx={{ mt: 4 }}><Typography>Plan nije pronađen.</Typography></Container>;

  const handleDownloadReport = async () => {
    try {
        const response = await travelPlanApi.getReport(id);
        const blob = new Blob([response.data], { type: 'application/pdf' });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Izvestaj_${plan.name}.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
    } catch (err) {
        alert('Greška pri preuzimanju izveštaja.');
    }
    };

    return (
    <Container sx={{ mt: 4, mb: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
        <Box>
            <Typography variant="h4">{plan.name}</Typography>
            <Typography color="text.secondary" gutterBottom>{plan.description}</Typography>
        </Box>
        <Button variant="outlined" startIcon={<DownloadIcon />} onClick={handleDownloadReport}>
            Preuzmi PDF izveštaj
        </Button>
        </Box>

        <Tabs value={tab} onChange={(e, val) => setTab(val)} sx={{ mt: 2, mb: 2 }}>
        <Tab label="Destinacije" />
        <Tab label="Aktivnosti" />
        <Tab label="Budžet" />
        <Tab label="Checklist" />
        </Tabs>

        <Box>
        {tab === 0 && <DestinationsTab planId={id} />}
        {tab === 1 && <ActivitiesTab planId={id} />}
        {tab === 2 && <BudgetTab planId={id} />}
        {tab === 3 && <ChecklistTab planId={id} />}
        </Box>
    </Container>
    );
}