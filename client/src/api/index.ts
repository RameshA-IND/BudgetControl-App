import api from './axiosConfig';

export const authApi = {
    login: (email: string, password: string) =>
        api.post('/auth/login', { email, password }),
    register: (data: { fullName: string; email: string; password: string; role: string; departmentId?: number }) =>
        api.post('/auth/register', data),
    getProfile: () => api.get('/auth/me')
};

export const departmentApi = {
    getAll: () => api.get('/departments'),
    getById: (id: number) => api.get(`/departments/${id}`),
    create: (data: any) => api.post('/departments', data),
    update: (id: number, data: any) => api.put(`/departments/${id}`, data)
};

export const budgetApi = {
    getAll: () => api.get('/budgets'),
    getById: (id: number) => api.get(`/budgets/${id}`),
    getByDepartment: (deptId: number) => api.get(`/budgets/department/${deptId}`),
    create: (data: any) => api.post('/budgets', data),
    update: (id: number, data: any) => api.put(`/budgets/${id}`, data)
};

export const expenseApi = {
    getAll: (params?: { status?: string; category?: string; departmentId?: number }) =>
        api.get('/expenses', { params }),
    getById: (id: number) => api.get(`/expenses/${id}`),
    getMy: () => api.get('/expenses/my'),
    getPending: (departmentId?: number) =>
        api.get('/expenses/pending', { params: { departmentId } }),
    submit: (data: any) => api.post('/expenses', data),
    updateStatus: (id: number, data: { action: string; comments?: string }) =>
        api.put(`/expenses/${id}/status`, data)
};

export const alertApi = {
    getAll: () => api.get('/alerts'),
    getUnread: () => api.get('/alerts/unread'),
    getCount: () => api.get('/alerts/count'),
    markAsRead: (id: number) => api.put(`/alerts/${id}/read`)
};

export const dashboardApi = {
    getSummary: () => api.get('/dashboard/summary'),
    getDepartmentSpending: () => api.get('/dashboard/department-spending'),
    getCategorySpending: () => api.get('/dashboard/category-spending'),
    getMonthlyTrends: () => api.get('/dashboard/monthly-trends'),
    getPendingApprovals: () => api.get('/dashboard/pending-approvals')
};

export const reportApi = {
    exportExcel: (params?: { departmentId?: number; fiscalYear?: string }) =>
        api.get('/reports/export/excel', { params, responseType: 'blob' }),
    exportPdf: (params?: { departmentId?: number; fiscalYear?: string }) =>
        api.get('/reports/export/pdf', { params, responseType: 'blob' })
};
