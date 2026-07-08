import { useState, useEffect, useCallback } from 'react';
import {
    Box, Button, List, ListItem, ListItemText, IconButton, Dialog, DialogTitle,
    DialogContent, DialogActions, TextField, MenuItem, Alert, CircularProgress,
    Card, CardContent, Typography, Chip
} from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import DeleteIcon from '@mui/icons-material/Delete';
import AddIcon from '@mui/icons-material/Add';
import dayjs from 'dayjs';
import { expenseApi } from '../../api/expenseApi';
import { Expense, EXPENSE_CATEGORIES, EXPENSE_CATEGORY_LABELS } from '../../models/Expense';

export function BudgetTab({ planId }) {
    const [expenses, setExpenses] = useState([]);
    const [summary, setSummary] = useState(null);
    const [loading, setLoading] = useState(true);
    const [dialogOpen, setDialogOpen] = useState(false);

    const [name, setName] = useState('');
    const [category, setCategory] = useState('Other');
    const [amount, setAmount] = useState('');
    const [date, setDate] = useState(dayjs());
    const [description, setDescription] = useState('');
    const [error, setError] = useState('');

    const load = useCallback(async () => {
        setLoading(true);
        try {
            const [expensesRes, summaryRes] = await Promise.all([
                expenseApi.getAll(planId),
                expenseApi.getSummary(planId),
            ]);
            setExpenses(expensesRes.data.map((e) => new Expense(e)));
            setSummary(summaryRes.data);
        } finally {
            setLoading(false);
        }
    }, [planId]);

    useEffect(() => { load(); }, [load]);

    const openCreate = () => {
        setName(''); setCategory('Other'); setAmount(''); setDate(dayjs()); setDescription('');
        setError('');
        setDialogOpen(true);
    };

    const handleSubmit = async () => {
        if (Number(amount) < 0) {
            setError('Iznos ne može biti negativan.');
            return;
        }

        try {
            await expenseApi.create(planId, {
                travelPlanId: planId,
                name, category,
                amount: Number(amount) || 0,
                date: date.toISOString(),
                description,
            });
            setDialogOpen(false);
            load();
        } catch (err) {
            setError(err.response?.data?.message || 'Greška pri dodavanju troška.');
        }
    };

    const handleDelete = async (expense) => {
        if (expense.isFromActivity) {
            alert('Ovaj trošak je automatski generisan iz aktivnosti. Izmenite ili obrišite aktivnost umesto ovoga.');
            return;
        }
        if (!window.confirm('Obrisati ovaj trošak?')) return;
        await expenseApi.delete(planId, expense.id);
        load();
    };

    if (loading) return <CircularProgress />;

    return (
        <Box>
            {summary && (
                <Card sx={{ mb: 3, bgcolor: 'grey.50' }}>
                    <CardContent>
                        <Box sx={{ display: 'flex', gap: 4, flexWrap: 'wrap' }}>
                            <Box sx={{ flex: 1, minWidth: 150 }}>
                                <Typography variant="body2" color="text.secondary">Planirani budžet</Typography>
                                <Typography variant="h6">{summary.plannedBudget} RSD</Typography>
                            </Box>
                            <Box sx={{ flex: 1, minWidth: 150 }}>
                                <Typography variant="body2" color="text.secondary">Ukupno potrošeno</Typography>
                                <Typography variant="h6">{summary.totalSpent} RSD</Typography>
                            </Box>
                            <Box sx={{ flex: 1, minWidth: 150 }}>
                                <Typography variant="body2" color="text.secondary">Preostalo</Typography>
                                <Typography
                                    variant="h6"
                                    color={summary.remainingBudget < 0 ? 'error.main' : 'success.main'}
                                >
                                    {summary.remainingBudget} RSD
                                </Typography>
                            </Box>
                        </Box>

                        {summary.byCategory.length > 0 && (
                            <Box sx={{ mt: 2, display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                                {summary.byCategory.map((cat) => (
                                    <Chip
                                        key={cat.category}
                                        label={`${EXPENSE_CATEGORY_LABELS[cat.category] || cat.category}: ${cat.total} RSD`}
                                        size="small"
                                    />
                                ))}
                            </Box>
                        )}
                    </CardContent>
                </Card>
            )}

            <Button startIcon={<AddIcon />} variant="contained" onClick={openCreate} sx={{ mb: 2 }}>
                Dodaj trošak
            </Button>

            <List>
                {expenses.map((expense) => (
                    <ListItem
                        key={expense.id}
                        secondaryAction={
                            !expense.isFromActivity && (
                                <IconButton onClick={() => handleDelete(expense)}><DeleteIcon /></IconButton>
                            )
                        }
                    >
                        <ListItemText
                            primary={`${expense.name} — ${expense.amount} RSD ${expense.isFromActivity ? '(iz aktivnosti)' : ''}`}
                            secondary={`${EXPENSE_CATEGORY_LABELS[expense.category] || expense.category} | ${dayjs(expense.date).format('DD.MM.YYYY')}${expense.description ? ' | ' + expense.description : ''}`}
                        />
                    </ListItem>
                ))}
            </List>

            {expenses.length === 0 && <Box color="text.secondary">Nema evidentiranih troškova.</Box>}

            <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
                <DialogTitle>Novi trošak</DialogTitle>
                <DialogContent>
                    {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
                    <TextField label="Naziv" fullWidth required margin="normal" value={name} onChange={(e) => setName(e.target.value)} />
                    <TextField select label="Kategorija" fullWidth margin="normal" value={category} onChange={(e) => setCategory(e.target.value)}>
                        {EXPENSE_CATEGORIES.map((c) => (
                            <MenuItem key={c} value={c}>{EXPENSE_CATEGORY_LABELS[c]}</MenuItem>
                        ))}
                    </TextField>
                    <TextField
                        label="Iznos (RSD)"
                        type="number"
                        fullWidth
                        required
                        margin="normal"
                        value={amount}
                        onChange={(e) => setAmount(e.target.value)}
                    />
                    <DatePicker label="Datum" value={date} onChange={setDate} sx={{ width: '100%', mt: 2 }} />
                    <TextField label="Opis" fullWidth multiline rows={2} margin="normal" value={description} onChange={(e) => setDescription(e.target.value)} />
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setDialogOpen(false)}>Otkaži</Button>
                    <Button variant="contained" onClick={handleSubmit} disabled={!name || !amount}>Sačuvaj</Button>
                </DialogActions>
            </Dialog>
        </Box>
    );
}