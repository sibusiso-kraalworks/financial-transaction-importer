'use client';

import { useState } from 'react';
import { Transaction, PagedResult, UpdateTransactionDto } from '@/types';
import { PencilIcon, TrashIcon, CheckIcon, XMarkIcon } from '@heroicons/react/24/outline';
import { format } from 'date-fns';

interface TransactionTableProps {
  transactions: Transaction[];
  loading: boolean;
  pagination: PagedResult<Transaction>;
  onPageChange: (page: number) => void;
  onUpdate: (id: number, data: UpdateTransactionDto) => Promise<boolean>;
  onDelete: (id: number) => Promise<void>;
}

export default function TransactionTable({
  transactions,
  loading,
  pagination,
  onPageChange,
  onUpdate,
  onDelete,
}: TransactionTableProps) {
  const [editingId, setEditingId] = useState<number | null>(null);
  const [editForm, setEditForm] = useState<UpdateTransactionDto>({
    transactionTime: '',
    amount: 0,
    description: ''
  });

  const handleEdit = (transaction: Transaction) => {
    setEditingId(transaction.id);
    setEditForm({
      transactionTime: format(new Date(transaction.transactionTime), "yyyy-MM-dd'T'HH:mm"),
      amount: transaction.amount,
      description: transaction.description
    });
  };

  const handleSave = async (id: number) => {
    const success = await onUpdate(id, editForm);
    if (success) {
      setEditingId(null);
    }
  };

  const handleCancel = () => {
    setEditingId(null);
    setEditForm({ transactionTime: '', amount: 0, description: '' });
  };

  const formatDate = (dateString: string) => {
    return format(new Date(dateString), 'MMM dd, yyyy HH:mm');
  };

  const formatAmount = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center py-12">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
      </div>
    );
  }

  return (
    <div className="card">
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="table-header">ID</th>
              <th className="table-header">Transaction Time</th>
              <th className="table-header">Amount</th>
              <th className="table-header">Description</th>
              <th className="table-header">Transaction ID</th>
              <th className="table-header">Actions</th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {transactions.length === 0 ? (
              <tr>
                <td colSpan={6} className="px-6 py-12 text-center text-gray-500">
                  No transactions found. Upload a CSV file to get started.
                </td>
              </tr>
            ) : (
              transactions.map((transaction) => (
                <tr key={transaction.id} className="hover:bg-gray-50">
                  <td className="table-cell">{transaction.id}</td>
                  <td className="table-cell">
                    {editingId === transaction.id ? (
                      <input
                        type="datetime-local"
                        className="input-field text-sm"
                        value={editForm.transactionTime}
                        onChange={(e) => setEditForm({ ...editForm, transactionTime: e.target.value })}
                      />
                    ) : (
                      formatDate(transaction.transactionTime)
                    )}
                  </td>
                  <td className="table-cell">
                    {editingId === transaction.id ? (
                      <input
                        type="number"
                        step="0.01"
                        className="input-field text-sm"
                        value={editForm.amount}
                        onChange={(e) => setEditForm({ ...editForm, amount: parseFloat(e.target.value) })}
                      />
                    ) : (
                      <span className="font-medium text-green-600">
                        {formatAmount(transaction.amount)}
                      </span>
                    )}
                  </td>
                  <td className="table-cell">
                    {editingId === transaction.id ? (
                      <input
                        type="text"
                        className="input-field text-sm"
                        value={editForm.description}
                        onChange={(e) => setEditForm({ ...editForm, description: e.target.value })}
                      />
                    ) : (
                      transaction.description
                    )}
                  </td>
                  <td className="table-cell font-mono text-sm">
                    {transaction.transactionId}
                  </td>
                  <td className="table-cell">
                    {editingId === transaction.id ? (
                      <div className="flex space-x-2">
                        <button
                          onClick={() => handleSave(transaction.id)}
                          className="text-green-600 hover:text-green-900"
                          title="Save"
                        >
                          <CheckIcon className="h-5 w-5" />
                        </button>
                        <button
                          onClick={handleCancel}
                          className="text-gray-600 hover:text-gray-900"
                          title="Cancel"
                        >
                          <XMarkIcon className="h-5 w-5" />
                        </button>
                      </div>
                    ) : (
                      <div className="flex space-x-3">
                        <button
                          onClick={() => handleEdit(transaction)}
                          className="text-primary-600 hover:text-primary-900"
                          title="Edit"
                        >
                          <PencilIcon className="h-5 w-5" />
                        </button>
                        <button
                          onClick={() => onDelete(transaction.id)}
                          className="text-red-600 hover:text-red-900"
                          title="Delete"
                        >
                          <TrashIcon className="h-5 w-5" />
                        </button>
                      </div>
                    )}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      {pagination.totalPages > 1 && (
        <div className="flex items-center justify-between border-t border-gray-200 bg-white px-4 py-3 sm:px-6 mt-4">
          <div className="flex flex-1 justify-between sm:hidden">
            <button
              onClick={() => onPageChange(pagination.pageNumber - 1)}
              disabled={pagination.pageNumber === 1}
              className="relative inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Previous
            </button>
            <button
              onClick={() => onPageChange(pagination.pageNumber + 1)}
              disabled={pagination.pageNumber === pagination.totalPages}
              className="relative ml-3 inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Next
            </button>
          </div>
          <div className="hidden sm:flex sm:flex-1 sm:items-center sm:justify-between">
            <div>
              <p className="text-sm text-gray-700">
                Showing{' '}
                <span className="font-medium">
                  {(pagination.pageNumber - 1) * pagination.pageSize + 1}
                </span>{' '}
                to{' '}
                <span className="font-medium">
                  {Math.min(pagination.pageNumber * pagination.pageSize, pagination.totalCount)}
                </span>{' '}
                of <span className="font-medium">{pagination.totalCount}</span> results
              </p>
            </div>
            <div>
              <nav className="isolate inline-flex -space-x-px rounded-md shadow-sm" aria-label="Pagination">
  {/* Previous button */}
  <button
    onClick={() => onPageChange(pagination.pageNumber - 1)}
    disabled={pagination.pageNumber === 1}
    className="relative inline-flex items-center rounded-l-md px-2 py-2 text-gray-400 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 focus:z-20 focus:outline-offset-0 disabled:opacity-50 disabled:cursor-not-allowed"
  >
    <span className="sr-only">Previous</span>
    <svg className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
      <path fillRule="evenodd" d="M12.79 5.23a.75.75 0 01-.02 1.06L8.832 10l3.938 3.71a.75.75 0 11-1.04 1.08l-4.5-4.25a.75.75 0 010-1.08l4.5-4.25a.75.75 0 011.06.02z" clipRule="evenodd" />
    </svg>
  </button>
  {/* Next 3 pages */}
  {(() => {
    const { pageNumber, totalPages } = pagination;
    const nextPages = [];
    for (let i = pageNumber + 1; i <= Math.min(pageNumber + 3, totalPages); i++) {
      nextPages.push(i);
    }
    return nextPages.map((p) => (
      <button
        key={p}
        onClick={() => onPageChange(p)}
        className={`relative inline-flex items-center px-4 py-2 text-sm font-semibold ${
          pagination.pageNumber === p
            ? 'z-10 bg-primary-600 text-white focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary-600'
            : 'text-gray-900 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 focus:z-20 focus:outline-offset-0'
        }`}
      >
        {p}
      </button>
    ));
  })()}
  {/* Ellipsis if gap before last 3 pages */}
  {(() => {
    const { pageNumber, totalPages } = pagination;
    const last3Start = Math.max(totalPages - 2, pageNumber + 4);
    if (last3Start > pageNumber + 4) {
      return <span className="px-2 py-2 text-gray-500">...</span>;
    }
    return null;
  })()}
  {/* Last 3 pages */}
  {(() => {
    const { pageNumber, totalPages } = pagination;
    const lastPages = [];
    for (let i = Math.max(totalPages - 2, pageNumber + 4); i <= totalPages; i++) {
      if (i > pageNumber + 3) {
        lastPages.push(i);
      }
    }
    return lastPages.map((p) => (
      <button
        key={p}
        onClick={() => onPageChange(p)}
        className={`relative inline-flex items-center px-4 py-2 text-sm font-semibold ${
          pagination.pageNumber === p
            ? 'z-10 bg-primary-600 text-white focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary-600'
            : 'text-gray-900 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 focus:z-20 focus:outline-offset-0'
        }`}
      >
        {p}
      </button>
    ));
  })()}
  {/* Next button */}
  <button
    onClick={() => onPageChange(pagination.pageNumber + 1)}
    disabled={pagination.pageNumber === pagination.totalPages}
    className="relative inline-flex items-center rounded-r-md px-2 py-2 text-gray-400 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 focus:z-20 focus:outline-offset-0 disabled:opacity-50 disabled:cursor-not-allowed"
  >
    <span className="sr-only">Next</span>
    <svg className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
      <path fillRule="evenodd" d="M7.21 14.77a.75.75 0 01.02-1.06L11.168 10 7.23 6.29a.75.75 0 111.04-1.08l4.5 4.25a.75.75 0 010 1.08l-4.5 4.25a.75.75 0 01-1.06-.02z" clipRule="evenodd" />
    </svg>
  </button>
</nav>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}