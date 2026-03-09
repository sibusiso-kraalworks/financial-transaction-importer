import Link from 'next/link'
import { CurrencyDollarIcon } from '@heroicons/react/24/outline'

export default function Home() {
  return (
    <div className="min-h-screen bg-gradient-to-b from-blue-50 to-white">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <nav className="flex justify-between items-center mb-16">
          <div className="flex items-center space-x-2">
            <CurrencyDollarIcon className="h-8 w-8 text-blue-600" />
            <span className="font-bold text-xl text-gray-900">Finance Importer</span>
          </div>
          <div className="space-x-4">
            <Link href="/login" className="text-gray-600 hover:text-gray-900">
              Login
            </Link>
            <Link 
              href="/register" 
              className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700"
            >
              Get Started
            </Link>
          </div>
        </nav>

        <div className="text-center mb-16">
          <h1 className="text-5xl font-bold text-gray-900 mb-4">
            Financial Transaction Importer
          </h1>
          <p className="text-xl text-gray-600 max-w-3xl mx-auto">
            Easily import, validate, and manage your financial transactions with our powerful CSV processing tool.
          </p>
        </div>

        <div className="grid md:grid-cols-3 gap-8 max-w-5xl mx-auto">
          <div className="bg-white p-6 rounded-xl shadow-md">
            <div className="text-blue-600 text-2xl mb-4">📤</div>
            <h3 className="text-lg font-semibold mb-2">Upload CSV</h3>
            <p className="text-gray-600">Upload your transaction files with automatic validation</p>
          </div>
          <div className="bg-white p-6 rounded-xl shadow-md">
            <div className="text-blue-600 text-2xl mb-4">✓</div>
            <h3 className="text-lg font-semibold mb-2">Validate Data</h3>
            <p className="text-gray-600">Automatic validation of formats, dates, and amounts</p>
          </div>
          <div className="bg-white p-6 rounded-xl shadow-md">
            <div className="text-blue-600 text-2xl mb-4">📊</div>
            <h3 className="text-lg font-semibold mb-2">Manage Transactions</h3>
            <p className="text-gray-600">View, edit, and delete your transactions easily</p>
          </div>
        </div>
      </div>
    </div>
  )
}