'use client';

import { useState, useEffect } from 'react';
import { useAuth } from '@/contexts/AuthContext';
import { transactionService } from '@/services/api';
import { Transaction, PagedResult } from '@/types';
import FileUpload from '@/components/FileUpload';
import TransactionTable from '@/components/TransactionTable';
import toast from 'react-hot-toast';
import PrivateRoute from '@/components/PrivateRoute';
            
export default function DashboardPage() {
  const { user, logout } = useAuth();
  const handleLogout = () => {
    logout();
    window.location.href = '/login';
  };
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [loading, setLoading] = useState(true);
  const [pagination, setPagination] = useState<PagedResult<Transaction>>({
    items: [],
    pageNumber: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 1
  });

  const fetchTransactions = async (pageNumber: number = 1, pageSize: number = 10) => {
    setLoading(true);
    try {
      const result = await transactionService.getAll(pageNumber, pageSize);
      setTransactions(result.items);
      setPagination(result);
    } catch (error) {
      toast.error('Failed to fetch transactions');
      console.error('Error fetching transactions:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTransactions();
  }, []);

  const handleUploadSuccess = () => {
    fetchTransactions(1, pagination.pageSize);
  };

  const handleUpdate = async (id: number, data: any): Promise<boolean> => {
    try {
      await transactionService.update(id, data);
      toast.success('Transaction updated successfully');
      fetchTransactions(pagination.pageNumber, pagination.pageSize);
      return true;
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to update transaction');
      return false;
    }
  };

  const handleDelete = async (id: number): Promise<void> => {
    if (!confirm('Are you sure you want to delete this transaction?')) {
      return;
    }

    try {
      await transactionService.delete(id);
      toast.success('Transaction deleted successfully');
      fetchTransactions(pagination.pageNumber, pagination.pageSize);
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to delete transaction');
    }
  };

  const handlePageChange = (newPage: number) => {
    if (newPage >= 1 && newPage <= pagination.totalPages) {
      fetchTransactions(newPage, pagination.pageSize);
    }
  };

  return (
    <PrivateRoute>
      <div className="min-h-screen bg-gradient-to-br from-blue-500 via-blue-400 to-indigo-500 flex items-center justify-center">
        <div className="w-full max-w-5xl bg-white bg-opacity-90 rounded-2xl shadow-xl p-8 mx-4">
          <div className="mb-8">
            <h1 className="text-4xl font-extrabold text-gray-900 text-center">Financial Transactions Dashboard</h1>
            <div className="mt-2 flex items-center justify-center gap-4">
              <p className="text-lg text-gray-700">
                Welcome back, <span className="font-semibold text-blue-600">{user?.email}</span>
              </p>
              <button
                onClick={handleLogout}
                className="ml-4 px-4 py-2 bg-red-500 hover:bg-red-600 text-white rounded-md font-semibold shadow-sm transition"
              >
                Logout
              </button>
            </div>
          </div>

          <div className="flex flex-col items-center mb-8">
            <div className="w-full max-w-md">
              <FileUpload onUploadSuccess={handleUploadSuccess} />
            </div>
          </div>

          <div className="mt-8">
            <div className="bg-white rounded-xl shadow-lg p-6">
              <TransactionTable
                transactions={transactions}
                loading={loading}
                pagination={pagination}
                onPageChange={handlePageChange}
                onUpdate={handleUpdate}
                onDelete={handleDelete}
              />
            </div>
          </div>
        </div>
      </div>
    </PrivateRoute>
  );
}