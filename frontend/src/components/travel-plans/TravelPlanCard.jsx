import { Card, CardContent, CardActions, Typography, Button, Chip, Box } from '@mui/material';
import { useNavigate } from 'react-router-dom';

export function TravelPlanCard({ plan, onDelete }) {
  const navigate = useNavigate();

  return (
    <Card sx={{ mb: 2 }}>
      <CardContent>
        <Typography variant="h6">{plan.name}</Typography>
        <Typography color="text.secondary" gutterBottom>
          {plan.formattedDateRange}
        </Typography>
        {plan.description && (
          <Typography variant="body2" sx={{ mb: 1 }}>{plan.description}</Typography>
        )}
        <Box sx={{ display: 'flex', gap: 1, mt: 1 }}>
          <Chip label={`Budžet: ${plan.budget} RSD`} size="small" />
          <Chip label={`${plan.destinationsCount} destinacija`} size="small" />
          <Chip label={`${plan.activitiesCount} aktivnosti`} size="small" />
        </Box>
      </CardContent>
      <CardActions>
        <Button size="small" onClick={() => navigate(`/plans/${plan.id}`)}>
          Detalji
        </Button>
        <Button size="small" color="error" onClick={() => onDelete(plan.id)}>
          Obriši
        </Button>
      </CardActions>
    </Card>
  );
}