import { useState, useEffect, useCallback } from 'react';
import {
  Dialog, DialogTitle, DialogContent, DialogActions, Button, TextField,
  MenuItem, Box, Typography, IconButton, List, ListItem, ListItemText, Divider, Alert
} from '@mui/material';
import ContentCopyIcon from '@mui/icons-material/ContentCopy';
import DeleteIcon from '@mui/icons-material/Delete';
import { shareApi } from '../../api/shareApi';

export function ShareDialog({ open, onClose, planId }) {
  const [accessLevel, setAccessLevel] = useState('View');
  const [expiresInDays, setExpiresInDays] = useState('');
  const [creating, setCreating] = useState(false);
  const [newLink, setNewLink] = useState(null);
  const [existingLinks, setExistingLinks] = useState([]);
  const [error, setError] = useState('');
  const [copied, setCopied] = useState(false);

  const loadExisting = useCallback(async () => {
    try {
      const res = await shareApi.getForPlan(planId);
      setExistingLinks(res.data);
    } catch (err) {
      // tiho ignorisemo ako ne uspe (npr. servis privremeno nedostupan)
    }
  }, [planId]);

  useEffect(() => {
    if (open) {
      loadExisting();
      setNewLink(null);
      setError('');
    }
  }, [open, loadExisting]);

  const handleCreate = async () => {
    setError('');
    setCreating(true);
    try {
      const res = await shareApi.create({
        travelPlanId: planId,
        accessLevel,
        expiresInDays: expiresInDays ? Number(expiresInDays) : null,
      });
      setNewLink(res.data);
      loadExisting();
    } catch (err) {
      setError(err.response?.data?.message || 'Greška pri kreiranju linka za deljenje.');
    } finally {
      setCreating(false);
    }
  };

  const handleCopy = (link) => {
    navigator.clipboard.writeText(link);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const handleRevoke = async (id) => {
    if (!window.confirm('Opozvati ovaj link? Više neće raditi.')) return;
    await shareApi.revoke(id);
    loadExisting();
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Podeli plan putovanja</DialogTitle>
      <DialogContent>
        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
        {copied && <Alert severity="success" sx={{ mb: 2 }}>Link kopiran!</Alert>}

        <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
          <TextField
            select
            label="Nivo pristupa"
            value={accessLevel}
            onChange={(e) => setAccessLevel(e.target.value)}
            sx={{ flex: 1 }}
          >
            <MenuItem value="View">Samo pregled</MenuItem>
            <MenuItem value="Edit">Pregled i uređivanje</MenuItem>
          </TextField>
          <TextField
            label="Ističe za (dana, opciono)"
            type="number"
            value={expiresInDays}
            onChange={(e) => setExpiresInDays(e.target.value)}
            sx={{ flex: 1 }}
          />
        </Box>

        <Button variant="contained" onClick={handleCreate} disabled={creating} fullWidth>
          {creating ? 'Kreiranje...' : 'Kreiraj novi link'}
        </Button>

        {newLink && (
          <Box sx={{ mt: 3, textAlign: 'center' }}>
            <img
              src={`data:image/png;base64,${newLink.qrCodeBase64}`}
              alt="QR kod"
              style={{ width: 200, height: 200 }}
            />
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 2 }}>
              <TextField value={newLink.shareLink} fullWidth size="small" InputProps={{ readOnly: true }} />
              <IconButton onClick={() => handleCopy(newLink.shareLink)}>
                <ContentCopyIcon />
              </IconButton>
            </Box>
          </Box>
        )}

        {existingLinks.length > 0 && (
          <>
            <Divider sx={{ my: 3 }} />
            <Typography variant="subtitle1" gutterBottom>Aktivni linkovi</Typography>
            <List dense>
              {existingLinks.map((link) => (
                <ListItem
                  key={link.id}
                  secondaryAction={
                    <IconButton onClick={() => handleRevoke(link.id)}><DeleteIcon /></IconButton>
                  }
                >
                  <ListItemText
                    primary={link.accessLevel === 'Edit' ? 'Pregled i uređivanje' : 'Samo pregled'}
                    secondary={link.expiresAt ? `Ističe: ${new Date(link.expiresAt).toLocaleDateString('sr-RS')}` : 'Bez isteka'}
                  />
                </ListItem>
              ))}
            </List>
          </>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Zatvori</Button>
      </DialogActions>
    </Dialog>
  );
}