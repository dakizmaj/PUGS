import { createTheme } from '@mui/material/styles';

export const theme = createTheme({
    palette: {
        mode: 'light',
        primary: {
            main: '#1565C0',      // tamnija, jasnija plava (umesto default MUI plave koja bledi)
            light: '#5E92F3',
            dark: '#003C8F',
            contrastText: '#FFFFFF',
        },
        secondary: {
            main: '#00897B',      // tirkizno-zelena, jasno se razlikuje od primarne
            contrastText: '#FFFFFF',
        },
        success: {
            main: '#2E7D32',
        },
        error: {
            main: '#C62828',
        },
        warning: {
            main: '#EF6C00',
        },
        info: {
            main: '#0277BD',
        },
        background: {
            default: '#F4F6F8',   // svetlo siva pozadina - jasnije odvaja kartice od pozadine
            paper: '#FFFFFF',
        },
        text: {
            primary: '#1A1A1A',
            secondary: '#5F6368',
        },
    },
    shape: {
        borderRadius: 8,
    },
    typography: {
        fontFamily: '"Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif',
        h4: { fontWeight: 600 },
        h5: { fontWeight: 600 },
        h6: { fontWeight: 600 },
    },
    components: {
  MuiCard: {
    styleOverrides: {
      root: {
        boxShadow: '0 1px 4px rgba(0,0,0,0.08)',
        border: '1px solid #E0E0E0',
      },
    },
  },
  MuiChip: {
    styleOverrides: {
      root: {
        fontWeight: 500,
      },
    },
  },
  MuiAppBar: {
    styleOverrides: {
      root: {
        boxShadow: '0 1px 3px rgba(0,0,0,0.15)',
      },
    },
  },
  MuiDialogTitle: {
    styleOverrides: {
      root: {
        fontWeight: 600,
        color: '#1A1A1A',
        fontSize: '1.25rem',
      },
    },
  },
},
});