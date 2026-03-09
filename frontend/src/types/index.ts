export interface Transaction {
  id: number
  transactionTime: string
  amount: number
  description: string
  transactionId: string
  createdAt: string
  updatedAt?: string
}

export interface TransactionUploadResult {
  success: boolean
  processedRows: number
  errors: ValidationError[]
  message: string
}

export interface ValidationError {
  rowNumber: number
  column: string
  error: string
}

export interface PagedResult<T> {
  items: T[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface UpdateTransactionDto {
  transactionTime: string
  amount: number
  description: string
}

export interface AuthResult {
  success: boolean
  token: string
  message: string
  email: string
  role: string
}

export interface User {
  email: string
  role: string
}