# Departmental Budget Control & Expense Monitoring Platform

## Architecture Overview

### Tech Stack
| Layer      | Technology              | Hosting     |
|------------|------------------------|-------------|
| Frontend   | React (Vite) + TypeScript | Vercel      |
| Backend    | ASP.NET Core 8 Web API | Render      |
| Database   | PostgreSQL 17          | Supabase    |
| Auth       | JWT + Role-based       | Backend     |
| Storage    | Supabase Storage       | Supabase    |

---

## ER Diagram (Entity Relationships)

```
┌──────────────────┐       ┌──────────────────────┐
│     Users         │       │    Departments        │
├──────────────────┤       ├──────────────────────┤
│ Id (PK)          │──┐    │ Id (PK)              │
│ FullName         │  │    │ Name                 │
│ Email (unique)   │  │    │ Code                 │
│ PasswordHash     │  │    │ Description          │
│ Role             │  │    │ ManagerId (FK→Users) │
│ DepartmentId(FK) │  │    │ IsActive             │
│ IsActive         │  │    │ CreatedAt            │
│ CreatedAt        │  │    │ UpdatedAt            │
│ UpdatedAt        │  │    └──────────────────────┘
└──────────────────┘  │             │
        │             │             │
        │             │    ┌──────────────────────┐
        │             │    │      Budgets          │
        │             │    ├──────────────────────┤
        │             └───▶│ Id (PK)              │
        │                  │ DepartmentId (FK)    │
        │                  │ FiscalYear           │
        │                  │ PeriodStart          │
        │                  │ PeriodEnd            │
        │                  │ AllocatedAmount      │
        │                  │ SpentAmount          │
        │                  │ WarningThreshold (%) │
        │                  │ CriticalThreshold (%)|
        │                  │ CreatedById (FK)     │
        │                  │ Status               │
        │                  │ CreatedAt            │
        │                  │ UpdatedAt            │
        │                  └──────────────────────┘
        │                           │
        │                  ┌──────────────────────┐
        │                  │     Expenses          │
        │                  ├──────────────────────┤
        └─────────────────▶│ Id (PK)              │
                           │ Title                │
                           │ Description          │
                           │ Amount               │
                           │ Category             │
                           │ DepartmentId (FK)    │
                           │ BudgetId (FK)        │
                           │ SubmittedById (FK)   │
                           │ Status               │
                           │ ReceiptUrl           │
                           │ SubmittedAt          │
                           │ UpdatedAt            │
                           └──────────────────────┘
                                    │
                           ┌──────────────────────┐
                           │  ExpenseApprovals     │
                           ├──────────────────────┤
                           │ Id (PK)              │
                           │ ExpenseId (FK)       │
                           │ ApproverId (FK→Users)│
                           │ ApproverRole         │
                           │ Action (Approve/Rej) │
                           │ Comments             │
                           │ ActionDate           │
                           └──────────────────────┘

                           ┌──────────────────────┐
                           │      Alerts           │
                           ├──────────────────────┤
                           │ Id (PK)              │
                           │ BudgetId (FK)        │
                           │ DepartmentId (FK)    │
                           │ Severity (Warn/Crit) │
                           │ Message              │
                           │ UtilizationPercent   │
                           │ IsRead               │
                           │ CreatedAt            │
                           └──────────────────────┘

                           ┌──────────────────────┐
                           │  ExpenseCategories    │
                           ├──────────────────────┤
                           │ Id (PK)              │
                           │ Name                 │
                           │ Description          │
                           │ IsActive             │
                           │ CreatedAt            │
                           └──────────────────────┘
```

---

## Entity Relationships

| Relationship | Type | Description |
|---|---|---|
| Users → Department | Many-to-One | Each user belongs to one department |
| Department → Users (Manager) | One-to-One | Each department has one manager |
| Budget → Department | Many-to-One | Budgets are department-specific |
| Expense → Department | Many-to-One | Expenses belong to a department |
| Expense → Budget | Many-to-One | Expenses are linked to a budget period |
| Expense → User (Submitter) | Many-to-One | Expenses are submitted by users |
| ExpenseApproval → Expense | Many-to-One | Multiple approvals per expense |
| ExpenseApproval → User (Approver) | Many-to-One | Approvals are done by users |
| Alert → Budget | Many-to-One | Alerts are tied to budgets |
| Alert → Department | Many-to-One | Alerts are tied to departments |

---

## Backend Folder Structure (Clean Architecture)

```
BudgetControl.API/
├── Controllers/
│   ├── AuthController.cs
│   ├── BudgetsController.cs
│   ├── DepartmentsController.cs
│   ├── ExpensesController.cs
│   ├── AlertsController.cs
│   ├── DashboardController.cs
│   └── ReportsController.cs
├── DTOs/
│   ├── Auth/
│   │   ├── LoginDto.cs
│   │   ├── RegisterDto.cs
│   │   └── AuthResponseDto.cs
│   ├── Budget/
│   │   ├── CreateBudgetDto.cs
│   │   ├── UpdateBudgetDto.cs
│   │   └── BudgetResponseDto.cs
│   ├── Department/
│   │   ├── CreateDepartmentDto.cs
│   │   └── DepartmentResponseDto.cs
│   ├── Expense/
│   │   ├── SubmitExpenseDto.cs
│   │   ├── UpdateExpenseStatusDto.cs
│   │   └── ExpenseResponseDto.cs
│   ├── Alert/
│   │   └── AlertResponseDto.cs
│   └── Dashboard/
│       ├── DashboardSummaryDto.cs
│       ├── DepartmentSpendingDto.cs
│       └── CategorySpendingDto.cs
├── Models/
│   ├── User.cs
│   ├── Department.cs
│   ├── Budget.cs
│   ├── Expense.cs
│   ├── ExpenseApproval.cs
│   ├── Alert.cs
│   └── ExpenseCategory.cs
├── Services/
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── IBudgetService.cs
│   │   ├── IDepartmentService.cs
│   │   ├── IExpenseService.cs
│   │   ├── IAlertService.cs
│   │   ├── IDashboardService.cs
│   │   └── IReportService.cs
│   └── Implementations/
│       ├── AuthService.cs
│       ├── BudgetService.cs
│       ├── DepartmentService.cs
│       ├── ExpenseService.cs
│       ├── AlertService.cs
│       ├── DashboardService.cs
│       └── ReportService.cs
├── Data/
│   ├── AppDbContext.cs
│   └── Migrations/
├── Middleware/
│   └── ExceptionMiddleware.cs
├── Helpers/
│   ├── JwtHelper.cs
│   └── ThresholdCalculator.cs
├── Program.cs
├── appsettings.json
└── appsettings.Development.json
```

---

## API Endpoint List

### Authentication
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | /api/auth/register | Register new user | No |
| POST | /api/auth/login | Login and get JWT | No |
| GET | /api/auth/me | Get current user profile | Yes |

### Departments
| Method | Endpoint | Description | Auth/Role |
|--------|----------|-------------|-----------|
| GET | /api/departments | List all departments | All |
| GET | /api/departments/{id} | Get department details | All |
| POST | /api/departments | Create department | FinanceAdmin |
| PUT | /api/departments/{id} | Update department | FinanceAdmin |

### Budgets
| Method | Endpoint | Description | Auth/Role |
|--------|----------|-------------|-----------|
| GET | /api/budgets | List all budgets | All |
| GET | /api/budgets/{id} | Get budget details | All |
| POST | /api/budgets | Create budget | FinanceAdmin |
| PUT | /api/budgets/{id} | Update budget | FinanceAdmin |
| GET | /api/budgets/department/{deptId} | Get budgets by dept | All |

### Expenses
| Method | Endpoint | Description | Auth/Role |
|--------|----------|-------------|-----------|
| GET | /api/expenses | List expenses (filtered) | All |
| GET | /api/expenses/{id} | Get expense details | All |
| POST | /api/expenses | Submit new expense | Employee+ |
| PUT | /api/expenses/{id}/status | Approve/Reject expense | Manager/Admin |
| GET | /api/expenses/my | Get current user's expenses | All |
| GET | /api/expenses/pending | Get pending approvals | Manager/Admin |

### Alerts
| Method | Endpoint | Description | Auth/Role |
|--------|----------|-------------|-----------|
| GET | /api/alerts | Get all alerts | FinanceAdmin |
| GET | /api/alerts/unread | Get unread alerts | FinanceAdmin |
| PUT | /api/alerts/{id}/read | Mark alert as read | FinanceAdmin |

### Dashboard
| Method | Endpoint | Description | Auth/Role |
|--------|----------|-------------|-----------|
| GET | /api/dashboard/summary | Organization overview | All |
| GET | /api/dashboard/department-spending | Department-wise spending | All |
| GET | /api/dashboard/category-spending | Category-wise spending | All |
| GET | /api/dashboard/monthly-trends | Monthly spending trends | All |

### Reports
| Method | Endpoint | Description | Auth/Role |
|--------|----------|-------------|-----------|
| GET | /api/reports/export/excel | Export report as Excel | FinanceAdmin |
| GET | /api/reports/export/pdf | Export report as PDF | FinanceAdmin |

---

## Threshold Alert Calculation Logic

```
On every expense approval:
  1. Fetch the related budget
  2. Calculate: utilization% = (SpentAmount / AllocatedAmount) × 100
  3. If utilization% >= CriticalThreshold AND no existing Critical alert:
     → Create Alert(Severity: Critical)
  4. Else If utilization% >= WarningThreshold AND no existing Warning alert:
     → Create Alert(Severity: Warning)
  5. Update Budget.SpentAmount += expense.Amount
```

---

## React Frontend Folder Structure

```
client/
├── public/
│   └── favicon.ico
├── src/
│   ├── api/
│   │   ├── axiosConfig.ts
│   │   ├── authApi.ts
│   │   ├── budgetApi.ts
│   │   ├── departmentApi.ts
│   │   ├── expenseApi.ts
│   │   ├── alertApi.ts
│   │   ├── dashboardApi.ts
│   │   └── reportApi.ts
│   ├── components/
│   │   ├── Layout/
│   │   │   ├── Sidebar.tsx
│   │   │   ├── Header.tsx
│   │   │   └── Layout.tsx
│   │   ├── Dashboard/
│   │   │   ├── BudgetOverview.tsx
│   │   │   ├── DepartmentSpending.tsx
│   │   │   ├── CategoryBreakdown.tsx
│   │   │   ├── MonthlyTrends.tsx
│   │   │   └── AlertsPanel.tsx
│   │   ├── Budget/
│   │   │   ├── BudgetList.tsx
│   │   │   ├── BudgetForm.tsx
│   │   │   └── BudgetCard.tsx
│   │   ├── Expense/
│   │   │   ├── ExpenseList.tsx
│   │   │   ├── ExpenseForm.tsx
│   │   │   ├── ExpenseCard.tsx
│   │   │   └── ApprovalPanel.tsx
│   │   ├── Department/
│   │   │   ├── DepartmentList.tsx
│   │   │   └── DepartmentForm.tsx
│   │   └── Common/
│   │       ├── ProtectedRoute.tsx
│   │       ├── StatusBadge.tsx
│   │       ├── LoadingSpinner.tsx
│   │       └── AlertBanner.tsx
│   ├── contexts/
│   │   └── AuthContext.tsx
│   ├── hooks/
│   │   ├── useAuth.ts
│   │   └── useDashboard.ts
│   ├── pages/
│   │   ├── LoginPage.tsx
│   │   ├── RegisterPage.tsx
│   │   ├── DashboardPage.tsx
│   │   ├── BudgetsPage.tsx
│   │   ├── ExpensesPage.tsx
│   │   ├── ApprovalsPage.tsx
│   │   ├── DepartmentsPage.tsx
│   │   ├── AlertsPage.tsx
│   │   └── ReportsPage.tsx
│   ├── types/
│   │   └── index.ts
│   ├── utils/
│   │   ├── formatters.ts
│   │   └── constants.ts
│   ├── App.tsx
│   ├── App.css
│   ├── index.css
│   └── main.tsx
├── index.html
├── package.json
├── tsconfig.json
└── vite.config.ts
```

---

## Deployment Steps

### 1. Database (Supabase)
1. Create a Supabase project
2. Run migration scripts to create tables
3. Configure RLS policies
4. Note the connection string for backend

### 2. Backend (Render)
1. Push code to GitHub
2. Create a new Web Service on Render
3. Set environment variables:
   - `ConnectionStrings__DefaultConnection`
   - `JwtSettings__SecretKey`
   - `JwtSettings__Issuer`
   - `JwtSettings__Audience`
4. Set build command: `dotnet publish -c Release -o out`
5. Set start command: `dotnet out/BudgetControl.API.dll`

### 3. Frontend (Vercel)
1. Push client code to GitHub
2. Import project in Vercel
3. Set environment variables:
   - `VITE_API_URL` = Render backend URL
4. Deploy with default Vite config
