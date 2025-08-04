import './App.css';
import { Routes, Route, Navigate } from 'react-router-dom';
import { AccountList } from './components/accountList';
import { AccountForm } from './components/acountForm';

export const App = () => {
  return (
    <Routes>
      <Route path="/accounts" element={<AccountList />} />
      <Route path="/accounts/new" element={<AccountForm />} />
      <Route path="/accounts/:id" element={<AccountForm />} />
      <Route path="*" element={<Navigate to="/accounts" />} />
    </Routes>

  );
};
