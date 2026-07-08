import { useAuth } from './context/AuthContext';

function App() {
  const { user, loading } = useAuth();

  if (loading) return <div>Učitavanje...</div>;

  return (
    <div>
      <h1>PUGS Frontend Setup OK</h1>
      <p>Trenutno ulogovan korisnik: {user ? user.name : 'niko'}</p>
    </div>
  );
}

export default App;