import { useState, useEffect, useCallback } from 'react';
import {
  Box, Button, List, ListItem, ListItemIcon, ListItemText, Checkbox,
  IconButton, TextField, CircularProgress
} from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import { checklistApi } from '../../api/checklistApi';
import { ChecklistItem } from '../../models/ChecklistItem';

export function ChecklistTab({ planId }) {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [newTitle, setNewTitle] = useState('');

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const res = await checklistApi.getAll(planId);
      setItems(res.data.map((i) => new ChecklistItem(i)));
    } finally {
      setLoading(false);
    }
  }, [planId]);

  useEffect(() => { load(); }, [load]);

  const handleAdd = async () => {
    if (!newTitle.trim()) return;
    await checklistApi.create(planId, { title: newTitle });
    setNewTitle('');
    load();
  };

  const handleToggle = async (item) => {
    // Opticno azuriranje - odmah promeni UI, pa posalji zahtev
    setItems((prev) => prev.map((i) => i.id === item.id ? { ...i, isCompleted: !i.isCompleted } : i));
    await checklistApi.toggle(planId, item.id);
  };

  const handleDelete = async (id) => {
    await checklistApi.delete(planId, id);
    load();
  };

  if (loading) return <CircularProgress />;

  return (
    <Box>
      <Box sx={{ display: 'flex', gap: 1, mb: 2 }}>
        <TextField
          placeholder="Nova stavka (npr. Pasoš, Punjač...)"
          fullWidth
          value={newTitle}
          onChange={(e) => setNewTitle(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && handleAdd()}
        />
        <Button variant="contained" onClick={handleAdd}>Dodaj</Button>
      </Box>

      <List>
        {items.map((item) => (
          <ListItem
            key={item.id}
            secondaryAction={
              <IconButton onClick={() => handleDelete(item.id)}><DeleteIcon /></IconButton>
            }
          >
            <ListItemIcon>
              <Checkbox checked={item.isCompleted} onChange={() => handleToggle(item)} />
            </ListItemIcon>
            <ListItemText
              primary={item.title}
              sx={item.isCompleted ? { textDecoration: 'line-through', color: 'text.disabled' } : {}}
            />
          </ListItem>
        ))}
      </List>

      {items.length === 0 && <Box color="text.secondary">Nema stavki na checklisti.</Box>}
    </Box>
  );
}