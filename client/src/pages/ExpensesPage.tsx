import { useState, useEffect } from 'react';
import { Plus, Search, Filter, Receipt, CheckCircle, Clock } from 'lucide-react';
import { expenseApi, departmentApi } from '../api';
import { Expense, Department, EXPENSE_CATEGORIES } from '../types';
import { formatCurrency, formatDate } from '../utils/formatters';
import { useAuth } from '../contexts/AuthContext';

export default function ExpensesPage() {
    const { user } = useAuth();
    const [expenses, setExpenses] = useState<Expense[]>([]);
    const [departments, setDepartments] = useState<Department[]>([]);
    const [search, setSearch] = useState('');
    const [statusFilter, setStatusFilter] = useState('');
    const [categoryFilter, setCategoryFilter] = useState('');
    const [showModal, setShowModal] = useState(false);
    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);
    const [form, setForm] = useState({ title: '', description: '', amount: '', category: '', departmentId: '' });

    useEffect(() => {
        loadData();
    }, [statusFilter, categoryFilter]);

    const loadData = async () => {
        try {
            const params: any = {};
            if (statusFilter) params.status = statusFilter;
            if (categoryFilter) params.category = categoryFilter;
            const [e, d] = await Promise.all([expenseApi.getAll(params), departmentApi.getAll()]);
            setExpenses(e.data);
            setDepartments(d.data);
        } catch (err) { console.error(err); }
        finally { setLoading(false); }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (submitting) return;
        setSubmitting(true);
        try {
            await expenseApi.submit({
                ...form,
                amount: parseFloat(form.amount),
                departmentId: form.departmentId ? parseInt(form.departmentId) : user?.departmentId
            });
            setShowModal(false);
            setForm({ title: '', description: '', amount: '', category: '', departmentId: '' });
            await loadData();
        } catch (err: any) {
            alert(err.response?.data?.message || 'Failed to submit expense');
        } finally {
            setSubmitting(false);
        }
    };

    const filtered = expenses.filter(e =>
        e.title.toLowerCase().includes(search.toLowerCase()) ||
        e.submittedByName.toLowerCase().includes(search.toLowerCase())
    );

    const totalExpenses = filtered.reduce((s, e) => s + e.amount, 0);
    const totalApproved = filtered.filter(e => e.status === 'Approved').reduce((s, e) => s + e.amount, 0);
    const pendingCount = filtered.filter(e => e.status === 'Pending' || e.status === 'DepartmentApproved').length;

    const getStatusBadge = (status: string) => {
        switch (status) {
            case 'Approved': return 'badge-success';
            case 'Rejected': return 'badge-critical';
            case 'DepartmentApproved': return 'badge-info';
            default: return 'badge-warning';
        }
    };

    const getCategoryBadge = (cat: string) => {
        const colors: Record<string, string> = {
            'Food': '#10b981', 'Travel': '#3b82f6', 'Infrastructure': '#f59e0b',
            'Hardware': '#ef4444', 'Software Licenses': '#8b5cf6', 'Learning': '#06b6d4'
        };
        return colors[cat] || '#64748b';
    };

    if (loading) return <div className="loading-spinner"><div className="spinner" /></div>;

    return (
        <>
            <div className="page-header">
                <div className="breadcrumb"><span>BudgetQ</span> / <span>Expense Management</span></div>
                <div className="page-title-row">
                    <div>
                        <h1 className="page-title">Expense Management</h1>
                        <p className="page-subtitle">Submit, track, and manage expenses across departments</p>
                    </div>
                    <button className="btn btn-primary" onClick={() => setShowModal(true)}>
                        <Plus size={16} /> Submit Expense
                    </button>
                </div>
            </div>
            <div className="page-content">
                {/* Stat Cards */}
                <div className="stat-cards" style={{ gridTemplateColumns: 'repeat(3, 1fr)' }}>
                    <div className="stat-card">
                        <div>
                            <div className="stat-label">Total Expenses</div>
                            <div className="stat-value">{formatCurrency(totalExpenses)}</div>
                        </div>
                        <div className="stat-icon blue"><Receipt size={20} /></div>
                    </div>
                    <div className="stat-card">
                        <div>
                            <div className="stat-label">Total Approved</div>
                            <div className="stat-value">{formatCurrency(totalApproved)}</div>
                        </div>
                        <div className="stat-icon green"><CheckCircle size={20} /></div>
                    </div>
                    <div className="stat-card">
                        <div>
                            <div className="stat-label">Pending Review</div>
                            <div className="stat-value">{pendingCount} <span style={{ fontSize: 14, fontWeight: 400, color: 'var(--text-muted)' }}>expenses</span></div>
                        </div>
                        <div className="stat-icon orange"><Clock size={20} /></div>
                    </div>
                </div>

                {/* Search & Filters */}
                <div style={{ display: 'flex', gap: 12, marginBottom: 20 }}>
                    <div className="search-bar" style={{ flex: 1, marginBottom: 0 }}>
                        <Search size={16} color="var(--text-muted)" />
                        <input placeholder="Search expenses or submitters..." value={search} onChange={e => setSearch(e.target.value)} />
                    </div>
                    <select className="form-select" style={{ width: 140 }} value={statusFilter} onChange={e => setStatusFilter(e.target.value)}>
                        <option value="">All Status</option>
                        <option value="Pending">Pending</option>
                        <option value="Approved">Approved</option>
                        <option value="Rejected">Rejected</option>
                    </select>
                    <select className="form-select" style={{ width: 160 }} value={categoryFilter} onChange={e => setCategoryFilter(e.target.value)}>
                        <option value="">All Categories</option>
                        {EXPENSE_CATEGORIES.map(c => <option key={c} value={c}>{c}</option>)}
                    </select>
                </div>

                {/* Expense Table */}
                <div className="card">
                    <table className="data-table">
                        <thead>
                            <tr>
                                <th>Expense</th>
                                <th>Category</th>
                                <th>Department</th>
                                <th>Submitted By</th>
                                <th>Date</th>
                                <th style={{ textAlign: 'right' }}>Amount</th>
                                <th>Status</th>
                            </tr>
                        </thead>
                        <tbody>
                            {filtered.map(exp => (
                                <tr key={exp.id}>
                                    <td>
                                        <div className="cell-title">{exp.title}</div>
                                        <div className="cell-sub">{exp.description?.slice(0, 50) || '—'}</div>
                                    </td>
                                    <td>
                                        <span className="badge" style={{ background: `${getCategoryBadge(exp.category)}20`, color: getCategoryBadge(exp.category) }}>
                                            {exp.category}
                                        </span>
                                    </td>
                                    <td>{exp.departmentName}</td>
                                    <td>{exp.submittedByName}</td>
                                    <td style={{ fontSize: 12, color: 'var(--text-muted)' }}>{formatDate(exp.submittedAt)}</td>
                                    <td style={{ textAlign: 'right', fontWeight: 700 }}>{formatCurrency(exp.amount)}</td>
                                    <td><span className={`badge ${getStatusBadge(exp.status)}`}>{exp.status}</span></td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    {filtered.length === 0 && <div className="empty-state"><h3>No expenses found</h3></div>}
                </div>
            </div>

            {/* Submit Expense Modal */}
            {showModal && (
                <div className="modal-overlay" onClick={() => setShowModal(false)}>
                    <div className="modal" onClick={e => e.stopPropagation()}>
                        <div className="modal-header">
                            <div className="modal-title">Submit Expense</div>
                            <button className="btn btn-icon btn-ghost" onClick={() => setShowModal(false)}>✕</button>
                        </div>
                        <form onSubmit={handleSubmit}>
                            <div className="modal-body">
                                <div className="form-group">
                                    <label className="form-label">Expense Title</label>
                                    <input className="form-input" placeholder="e.g., Conference Travel" value={form.title} onChange={e => setForm({ ...form, title: e.target.value })} required />
                                </div>
                                <div className="form-group">
                                    <label className="form-label">Description</label>
                                    <textarea className="form-textarea" placeholder="Add details..." value={form.description} onChange={e => setForm({ ...form, description: e.target.value })} />
                                </div>
                                <div className="form-row">
                                    <div className="form-group">
                                        <label className="form-label">Amount (₹)</label>
                                        <input className="form-input" type="number" step="0.01" placeholder="0.00" value={form.amount} onChange={e => setForm({ ...form, amount: e.target.value })} required />
                                    </div>
                                    <div className="form-group">
                                        <label className="form-label">Category</label>
                                        <select className="form-select" value={form.category} onChange={e => setForm({ ...form, category: e.target.value })} required>
                                            <option value="">Select Category</option>
                                            {EXPENSE_CATEGORIES.map(c => <option key={c} value={c}>{c}</option>)}
                                        </select>
                                    </div>
                                </div>
                                <div className="form-group">
                                    <label className="form-label">Department</label>
                                    <select className="form-select" value={form.departmentId} onChange={e => setForm({ ...form, departmentId: e.target.value })} required>
                                        <option value="">Select Department</option>
                                        {departments.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
                                    </select>
                                </div>
                            </div>
                            <div className="modal-footer">
                                <button type="button" className="btn btn-ghost" onClick={() => setShowModal(false)} disabled={submitting}>Cancel</button>
                                <button type="submit" className="btn btn-primary" disabled={submitting}>
                                    {submitting ? 'Submitting...' : 'Submit Expense'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </>
    );
}
