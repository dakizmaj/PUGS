import { Container, Typography } from '@mui/material';

export function AdminPage() {
  return (
    <Container sx={{ mt: 4 }}>
      <Typography variant="h4">Admin panel</Typography>
      <Typography color="text.secondary">Ovde će biti upravljanje korisnicima.</Typography>
    </Container>
  );
}