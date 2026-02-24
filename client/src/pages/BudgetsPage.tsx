import { useState, useEffect } from 'react';
import { Plus, Search, Building2 } from 'lucide-react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend, Cell } from 'recharts';
import { budgetApi, departmentApi } from '../api';
import { Budget, Department } from '../types';
import { formatCurrency } from '../utils/formatters';
import { useAuth } from '../contexts/AuthContext';

export default function BudgetsPage() {
    const { isFinanceAdmin } = useAuth();
    const [budgets, setBudgets] = useState<Budget[]>([]);
    const [departments, setDepartments] = useState<Department[]>([]);
    const [search, setSearch] = useState('');
    const [showModal, setShowModal] = useState(false);
    const [loading, setLoading] = useState(true);
    const [form, setForm] = useState({
        departmentId: '', fiscalYear: '2026', periodStart: '2026-01-01',
        periodEnd: '2026-12-31', allocatedAmount: '', warningThresholdPct: '75', criticalThresholdPct: '90'
    });

    useEffect(() => {
        loadData();
    }, []);

    const loadData = async () => {
        try {
            const [b, d] = await Promise.all([budgetApi.getAll(), departmentApi.getAll()]);
            setBudgets(b.data);
            setDepartments(d.data);
        } catch (e) { console.error(e); }
        finally { setLoading(false); }
    };

    const handleCreate = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            await budgetApi.create({
                ...form,
                departmentId: parseInt(form.departmentId),
                allocatedAmount: parseFloat(form.allocatedAmount),
                warningThresholdPct: parseFloat(form.warningThresholdPct),
                criticalThresholdPct: parseFloat(form.criticalThresholdPct)
            });
            setShowModal(false);
            setForm({ departmentId: '', fiscalYear: '2026', periodStart: '2026-01-01', periodEnd: '2026-12-31', allocatedAmount: '', warningThresholdPct: '75', criticalThresholdPct: '90' });
            loadData();
        } catch (err: any) {
            alert(err.response?.data?.message || 'Failed to create budget');
        }
    };

    const filtered = budgets.filter(b =>
        b.departmentName.toLowerCase().includes(search.toLowerCase())
    );

    const chartData = filtered.map(b => ({
        dept: b.departmentName,
        Allocated: b.allocatedAmount,
        Spent: b.spentAmount
    }));

    const getBarColor = (b: Budget) => {
        if (b.healthStatus === 'Critical') return '#ef4444';
        if (b.healthStatus === 'Warning') return '#f59e0b';
        return '#10b981';
    };

    if (loading) return <div className="loading-spinner"><div className="spinner" /></div>;

    return (
        <>
            <div className="page-header">
                <div className="breadcrumb"><span>BudgetQ</span> / <span>Budget Management</span></div>
                <div className="page-title-row">
                    <div>
                        <h1 className="page-title">Budget Management</h1>
                        <p className="page-subtitle">Create and manage departmental budgets with configurable thresholds</p>
                    </div>
                    {isFinanceAdmin && (
                        <button className="btn btn-primary" onClick={() => setShowModal(true)}>
                            <Plus size={16} /> Create Budget
                        </button>
                    )}
                </div>
            </div>
            <div className="page-content">
                {/* Budget Allocation Chart */}
                <div className="card" style={{ marginBottom: 24 }}>
                    <div className="card-header">
                        <div>
                            <div className="card-title">Budget Allocation vs Utilization</div>
                            <div className="card-subtitle">Compare allocated budget vs actual spending by department</div>
                        </div>
                    </div>
                    <div className="card-body" style={{ height: 260 }}>
                        <ResponsiveContainer width="100%" height="100%">
                            <BarChart data={chartData} barGap={4}>
                                <CartesianGrid strokeDasharray="3 3" stroke="#1e2d4a" />
                                <XAxis dataKey="dept" stroke="#64748b" fontSize={11} />
                                <YAxis stroke="#64748b" fontSize={11} tickFormatter={(v: any) => `₹${v / 1000}k`} />
                                <Tooltip
                                    contentStyle={{ background: '#151d2e', border: '1px solid #253552', borderRadius: 8, color: '#f1f5f9' }}
                                    formatter={(v: any) => [formatCurrency(v as number)]}
                                />
                                <Legend wrapperStyle={{ fontSize: 12 }} />
                                <Bar dataKey="Allocated" fill="#64748b" radius={[4, 4, 0, 0] as any} />
                                <Bar dataKey="Spent" fill="#10b981" radius={[4, 4, 0, 0] as any}>
                                    {chartData.map((_, i) => {
                                        const b = filtered[i];
                                        return <Cell key={i} fill={b ? getBarColor(b) : '#10b981'} />;
                                    })}
                                </Bar>
                            </BarChart>
                        </ResponsiveContainer>
                    </div>
                </div>

                {/* Search */}
                <div className="search-bar">
                    <Search size={16} color="var(--text-muted)" />
                    <input placeholder="Search departments..." value={search} onChange={e => setSearch(e.target.value)} />
                </div>

                {/* Budget Cards Grid */}
                <div className="grid-3">
                    {filtered.map((b) => (
                        <div key={b.id} className="budget-card">
                            <div className="budget-card-header">
                                <div>
                                    <div className="budget-dept-name">
                                        <Building2 size={16} color="var(--color-primary)" /> {b.departmentName}
                                    </div>
                                    <div className="budget-dept-meta">
                                        FY {b.fiscalYear} · Managed by {b.managerName || 'N/A'}
                                    </div>
                                </div>
                                <span className={`badge badge-${b.healthStatus === 'Critical' ? 'critical' : b.healthStatus === 'Warning' ? 'warning' : 'success'}`}>
                                    {b.healthStatus}
                                </span>
                            </div>

                            <div className="budget-amount">{formatCurrency(b.allocatedAmount)}</div>
                            <div style={{ display: 'flex', gap: 12, fontSize: 12, color: 'var(--text-muted)' }}>
                                <span style={{ color: getBarColor(b) }}>▲ {formatCurrency(b.spentAmount)}</span>
                                <span>{b.utilizationPercent}%</span>
                            </div>

                            <div className="budget-progress">
                                <div className="progress-bar">
                                    <div
                                        className={`progress-fill ${b.healthStatus.toLowerCase()}`}
                                        style={{ width: `${Math.min(b.utilizationPercent, 100)}%` }}
                                    />
                                </div>
                            </div>

                            <div className="budget-footer">
                                <span>Remaining: {formatCurrency(b.remainingAmount)}</span>
                                <span>⚡ {b.warningThresholdPct}% / {b.criticalThresholdPct}%</span>
                            </div>
                        </div>
                    ))}
                </div>

                {filtered.length === 0 && (
                    <div className="empty-state"><h3>No budgets found</h3></div>
                )}
            </div>

            {/* Create Budget Modal */}
            {showModal && (
                <div className="modal-overlay" onClick={() => setShowModal(false)}>
                    <div className="modal" onClick={e => e.stopPropagation()}>
                        <div className="modal-header">
                            <div className="modal-title">Create Budget</div>
                            <button className="btn btn-icon btn-ghost" onClick={() => setShowModal(false)}>✕</button>
                        </div>
                        <form onSubmit={handleCreate}>
                            <div className="modal-body">
                                <div className="form-group">
                                    <label className="form-label">Department</label>
                                    <select className="form-select" value={form.departmentId} onChange={e => setForm({ ...form, departmentId: e.target.value })} required>
                                        <option value="">Select Department</option>
                                        {departments.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
                                    </select>
                                </div>
                                <div className="form-row">
                                    <div className="form-group">
                                        <label className="form-label">Fiscal Year</label>
                                        <input className="form-input" value={form.fiscalYear} onChange={e => setForm({ ...form, fiscalYear: e.target.value })} required />
                                    </div>
                                    <div className="form-group">
                                        <label className="form-label">Allocated Amount (₹)</label>
                                        <input className="form-input" type="number" placeholder="100000" value={form.allocatedAmount} onChange={e => setForm({ ...form, allocatedAmount: e.target.value })} required />
                                    </div>
                                </div>
                                <div className="form-row">
                                    <div className="form-group">
                                        <label className="form-label">Period Start</label>
                                        <input className="form-input" type="date" value={form.periodStart} onChange={e => setForm({ ...form, periodStart: e.target.value })} required />
                                    </div>
                                    <div className="form-group">
                                        <label className="form-label">Period End</label>
                                        <input className="form-input" type="date" value={form.periodEnd} onChange={e => setForm({ ...form, periodEnd: e.target.value })} required />
                                    </div>
                                </div>
                                <div className="form-row">
                                    <div className="form-group">
                                        <label className="form-label">Warning Threshold (%)</label>
                                        <input className="form-input" type="number" value={form.warningThresholdPct} onChange={e => setForm({ ...form, warningThresholdPct: e.target.value })} />
                                    </div>
                                    <div className="form-group">
                                        <label className="form-label">Critical Threshold (%)</label>
                                        <input className="form-input" type="number" value={form.criticalThresholdPct} onChange={e => setForm({ ...form, criticalThresholdPct: e.target.value })} />
                                    </div>
                                </div>
                            </div>
                            <div className="modal-footer">
                                <button type="button" className="btn btn-ghost" onClick={() => setShowModal(false)}>Cancel</button>
                                <button type="submit" className="btn btn-primary">Create Budget</button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </>
    );
}
