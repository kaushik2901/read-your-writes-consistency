import { Outlet } from 'react-router-dom';
import { Header } from './components/Header';
import { useUrlSync } from './hooks/useUrlSync';

function App() {
  useUrlSync();

  return (
    <div className="min-h-screen flex flex-col bg-background">
      <Header />
      <main className="container mx-auto max-w-6xl px-4 sm:px-6 md:px-8 py-6 flex-1">
        <Outlet />
      </main>
    </div>
  );
}

export default App;
