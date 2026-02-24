export interface User {
  id: number;
  fullName: string;
  email: string;
  role: 'Employee' | 'DepartmentManager' | 'FinanceAdmin';
  departmentId?: number;
  departmentName?: string;
}

export interface AuthResponse {
  token: string;
  user: User;
}

export interface Department {
  id: number;
  name: string;
  code: string;
  description?: string;
  managerId?: number;
  managerName?: string;
  isActive: boolean;
  employeeCount: number;
  createdAt: string;
}

export interface Budget {
  id: number;
  departmentId: number;
  departmentName: string;
  departmentCode: string;
  managerName?: string;
  fiscalYear: string;
  periodStart: string;
  periodEnd: string;
  allocatedAmount: number;
  spentAmount: number;
  remainingAmount: number;
  utilizationPercent: number;
  warningThresholdPct: number;
  criticalThresholdPct: number;
  status: string;
  healthStatus: 'Healthy' | 'Warning' | 'Critical';
  createdAt: string;
}

export interface Expense {
  id: number;
  title: string;
  description?: string;
  amount: number;
  category: string;
  departmentId: number;
  departmentName: string;
  budgetId?: number;
  submittedById: number;
  submittedByName: string;
  status: 'Pending' | 'DepartmentApproved' | 'Approved' | 'Rejected';
  receiptUrl?: string;
  submittedAt: string;
  updatedAt: string;
  approvals: ApprovalRecord[];
}

export interface ApprovalRecord {
  id: number;
  approverName: string;
  approverRole: string;
  action: string;
  comments?: string;
  actionDate: string;
}

export interface Alert {
  id: number;
  budgetId: number;
  departmentId: number;
  departmentName: string;
  severity: 'Warning' | 'Critical';
  message: string;
  utilizationPercent: number;
  allocatedAmount: number;
  spentAmount: number;
  isRead: boolean;
  createdAt: string;
}

export interface DashboardSummary {
  totalBudget: number;
  totalSpent: number;
  totalRemaining: number;
  activeAlerts: number;
  pendingApprovals: number;
  totalDepartments: number;
  overallUtilization: number;
}

export interface DepartmentSpending {
  departmentId: number;
  departmentName: string;
  allocatedAmount: number;
  spentAmount: number;
  remainingAmount: number;
  utilizationPercent: number;
  healthStatus: string;
}

export interface CategorySpending {
  category: string;
  totalAmount: number;
  percentage: number;
  expenseCount: number;
}

export interface MonthlyTrend {
  month: string;
  budgetAmount: number;
  spentAmount: number;
}

export interface PendingApproval {
  id: number;
  title: string;
  submittedBy: string;
  department: string;
  amount: number;
  category: string;
  submittedAt: string;
}

export const EXPENSE_CATEGORIES = [
  'Food',
  'Travel',
  'Infrastructure',
  'Hardware',
  'Software Licenses',
  'Learning'
] as const;
