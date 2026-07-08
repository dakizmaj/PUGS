import { useParams } from 'react-router-dom';
import { Container, Typography } from '@mui/material';

export function TravelPlanDetailPage() {
  const { id } = useParams();

  return (
    <Container sx={{ mt: 4 }}>
      <Typography variant="h4">Detalji plana</Typography>
      <Typography color="text.secondary">ID plana: {id}</Typography>
    </Container>
  );
}