import { useState, useEffect } from 'react';
import { IndianRupee, TrendingUp, AlertTriangle, Wallet } from 'lucide-react';
import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell, Legend } from 'recharts';
import { dashboardApi, alertApi } from '../api';
import { DashboardSummary, DepartmentSpending, CategorySpending, MonthlyTrend, PendingApproval, Alert } from '../types';
import { formatCurrency } from '../utils/formatters';

const CHART_COLORS = ['#06b6d4', '#8b5cf6', '#f59e0b', '#10b981', '#ef4444', '#ec4899'];

export default function DashboardPage() {
    const [summary, setSummary] = useState<DashboardSummary | null>(null);
    const [deptSpending, setDeptSpending] = useState<DepartmentSpending[]>([]);
    const [categorySpending, setCategorySpending] = useState<CategorySpending[]>([]);
    const [trends, setTrends] = useState<MonthlyTrend[]>([]);
    const [pendingApprovals, setPendingApprovals] = useState<PendingApproval[]>([]);
    const [alerts, setAlerts] = useState<Alert[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        Promise.all([
            dashboardApi.getSummary(),
            dashboardApi.getDepartmentSpending(),
            dashboardApi.getCategorySpending(),
            dashboardApi.getMonthlyTrends(),
            dashboardApi.getPendingApprovals(),
            alertApi.getUnread()
        ]).then(([s, d, c, t, p, a]) => {
            setSummary(s.data);
            setDeptSpending(d.data);
            setCategorySpending(c.data);
            setTrends(t.data);
            setPendingApprovals(p.data);
            setAlerts(a.data);
        }).catch(console.error).finally(() => setLoading(false));
    }, []);

    if (loading) return <div className="loading-spinner"><div className="spinner" /></div>;

    return (
        <>
            <div className="page-header">
                <div className="breadcrumb">
                    <span>BudgetQ</span> / <span>Dashboard</span>
                </div>
            </div>
            <div className="page-content">
                {/* Stat Cards */}
                <div className="stat-cards">
                    <div className="stat-card">
                        <div>
                            <div className="stat-label">Total Budget</div>
                            <div className="stat-value">{formatCurrency(summary?.totalBudget || 0)}</div>
                            <div className="stat-sub">Across all departments</div>
                        </div>
                        <div className="stat-icon blue"><IndianRupee size={20} /></div>
                    </div>
                    <div className="stat-card">
                        <div>
                            <div className="stat-label">Total Spent</div>
                            <div className="stat-value">{formatCurrency(summary?.totalSpent || 0)}</div>
                            <div className="stat-sub">{summary?.overallUtilization || 0}% utilized</div>
                        </div>
                        <div className="stat-icon purple"><TrendingUp size={20} /></div>
                    </div>
                    <div className="stat-card">
                        <div>
                            <div className="stat-label">Remaining</div>
                            <div className="stat-value">{formatCurrency(summary?.totalRemaining || 0)}</div>
                            <div className="stat-sub">{summary?.totalDepartments || 0} departments</div>
                        </div>
                        <div className="stat-icon green"><Wallet size={20} /></div>
                    </div>
                    <div className="stat-card">
                        <div>
                            <div className="stat-label">Active Alerts</div>
                            <div className="stat-value">{summary?.activeAlerts || 0}</div>
                            <div className="stat-sub">{summary?.pendingApprovals || 0} pending approvals</div>
                        </div>
                        <div className="stat-icon orange"><AlertTriangle size={20} /></div>
                    </div>
                </div>

                {/* Charts Row */}
                <div className="grid-60-40" style={{ marginBottom: 24 }}>
                    {/* Budget vs Actual Chart */}
                    <div className="card">
                        <div className="card-header">
                            <div>
                                <div className="card-title">Budget vs Actual Spending</div>
                                <div className="card-subtitle">Monthly comparison (current year)</div>
                            </div>
                        </div>
                        <div className="card-body" style={{ height: 280 }}>
                            <ResponsiveContainer width="100%" height="100%">
                                <AreaChart data={trends}>
                                    <defs>
                                        <linearGradient id="budgetGrad" x1="0" y1="0" x2="0" y2="1">
                                            <stop offset="5%" stopColor="#06b6d4" stopOpacity={0.3} />
                                            <stop offset="95%" stopColor="#06b6d4" stopOpacity={0} />
                                        </linearGradient>
                                        <linearGradient id="spentGrad" x1="0" y1="0" x2="0" y2="1">
                                            <stop offset="5%" stopColor="#8b5cf6" stopOpacity={0.3} />
                                            <stop offset="95%" stopColor="#8b5cf6" stopOpacity={0} />
                                        </linearGradient>
                                    </defs>
                                    <CartesianGrid strokeDasharray="3 3" stroke="#1e2d4a" />
                                    <XAxis dataKey="month" stroke="#64748b" fontSize={11} />
                                    <YAxis stroke="#64748b" fontSize={11} tickFormatter={(v: any) => `₹${(v / 1000)}k`} />
                                    <Tooltip
                                        contentStyle={{ background: '#151d2e', border: '1px solid #253552', borderRadius: 8, color: '#f1f5f9', fontSize: 12 }}
                                        itemStyle={{ color: '#f1f5f9' }}
                                        formatter={(value: any, name: any) => [formatCurrency(value as number), name]}
                                    />
                                    <Area type="monotone" dataKey="budgetAmount" stroke="#06b6d4" fill="url(#budgetGrad)" strokeWidth={2} name="Budget" />
                                    <Area type="monotone" dataKey="spentAmount" stroke="#8b5cf6" fill="url(#spentGrad)" strokeWidth={2} name="Spent" />
                                </AreaChart>
                            </ResponsiveContainer>
                        </div>
                    </div>

                    {/* Spending by Category */}
                    <div className="card">
                        <div className="card-header">
                            <div>
                                <div className="card-title">Spending by Category</div>
                                <div className="card-subtitle">Top spending categories</div>
                            </div>
                        </div>
                        <div className="card-body" style={{ height: 280 }}>
                            <ResponsiveContainer width="100%" height="100%">
                                <PieChart>
                                    <Pie
                                        data={categorySpending}
                                        cx="50%" cy="50%"
                                        innerRadius={60} outerRadius={95}
                                        dataKey="totalAmount"
                                        nameKey="category"
                                        paddingAngle={3}
                                    >
                                        {categorySpending.map((_, i) => (
                                            <Cell key={i} fill={CHART_COLORS[i % CHART_COLORS.length]} />
                                        ))}
                                    </Pie>
                                    <Legend
                                        wrapperStyle={{ fontSize: 11, color: '#94a3b8' }}
                                        iconType="circle"
                                        iconSize={8}
                                    />
                                    <Tooltip
                                        contentStyle={{ background: '#151d2e', border: '1px solid #253552', borderRadius: 8, color: '#f1f5f9', fontSize: 12 }}
                                        itemStyle={{ color: '#f1f5f9' }}
                                        formatter={(value: any, name: any) => [formatCurrency(value as number), name]}
                                    />
                                </PieChart>
                            </ResponsiveContainer>
                        </div>
                    </div>
                </div>

                {/* Department Budget Utilization Table */}
                <div className="card" style={{ marginBottom: 24 }}>
                    <div className="card-header">
                        <div>
                            <div className="card-title">Department Budget Utilization</div>
                            <div className="card-subtitle">Real-time budget tracking across departments</div>
                        </div>
                    </div>
                    <table className="data-table">
                        <thead>
                            <tr>
                                <th>Department</th>
                                <th style={{ textAlign: 'right' }}>Allocated</th>
                                <th style={{ textAlign: 'right' }}>Spent</th>
                                <th style={{ width: 200 }}>Utilization</th>
                                <th>Status</th>
                            </tr>
                        </thead>
                        <tbody>
                            {deptSpending.map((d) => (
                                <tr key={d.departmentId}>
                                    <td className="cell-title">{d.departmentName}</td>
                                    <td style={{ textAlign: 'right' }}>{formatCurrency(d.allocatedAmount)}</td>
                                    <td style={{ textAlign: 'right' }}>{formatCurrency(d.spentAmount)}</td>
                                    <td>
                                        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                                            <div className="progress-bar" style={{ flex: 1 }}>
                                                <div
                                                    className={`progress-fill ${d.healthStatus.toLowerCase()}`}
                                                    style={{ width: `${Math.min(d.utilizationPercent, 100)}%` }}
                                                />
                                            </div>
                                            <span style={{ fontSize: 12, fontWeight: 600, minWidth: 45, textAlign: 'right' }}>
                                                {d.utilizationPercent}%
                                            </span>
                                        </div>
                                    </td>
                                    <td>
                                        <span className={`badge badge-${d.healthStatus === 'Critical' ? 'critical' : d.healthStatus === 'Warning' ? 'warning' : 'success'}`}>
                                            {d.healthStatus}
                                        </span>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>

                {/* Bottom Row: Pending Approvals + Active Alerts */}
                <div className="grid-2">
                    {/* Pending Approvals */}
                    <div className="card">
                        <div className="card-header">
                            <div className="card-title">Pending Approvals</div>
                            <span className="badge badge-info">{pendingApprovals.length} pending</span>
                        </div>
                        <div>
                            {pendingApprovals.length === 0 ? (
                                <div className="empty-state"><h3>No pending approvals</h3></div>
                            ) : (
                                pendingApprovals.map((item) => (
                                    <div key={item.id} className="approval-item">
                                        <div className="approval-avatar" style={{ background: `linear-gradient(135deg, ${CHART_COLORS[Math.floor(Math.random() * 6)]}, #0891b2)` }}>
                                            {item.submittedBy.split(' ').map(n => n[0]).join('').slice(0, 2)}
                                        </div>
                                        <div className="approval-info">
                                            <div className="approval-title">{item.title}</div>
                                            <div className="approval-meta">
                                                <span>{item.submittedBy}</span>
                                                <span>•</span>
                                                <span>{item.department}</span>
                                            </div>
                                        </div>
                                        <div className="approval-amount">{formatCurrency(item.amount)}</div>
                                    </div>
                                ))
                            )}
                        </div>
                    </div>

                    {/* Active Alerts */}
                    <div className="card">
                        <div className="card-header">
                            <div className="card-title">Active Alerts</div>
                            <span className="badge badge-critical">{alerts.length} alerts</span>
                        </div>
                        <div className="card-body">
                            {alerts.length === 0 ? (
                                <div className="empty-state"><h3>No active alerts</h3></div>
                            ) : (
                                alerts.slice(0, 4).map((alert) => (
                                    <div key={alert.id} className={`alert-card ${alert.severity.toLowerCase()}`}>
                                        <div className={`alert-indicator ${alert.severity.toLowerCase()}`} />
                                        <div style={{ flex: 1 }}>
                                            <div style={{ fontWeight: 600, fontSize: 13, color: 'var(--text-heading)', marginBottom: 4 }}>
                                                {alert.departmentName}
                                            </div>
                                            <div style={{ fontSize: 12, color: 'var(--text-secondary)' }}>
                                                {alert.message}
                                            </div>
                                        </div>
                                        <span className={`badge badge-${alert.severity === 'Critical' ? 'critical' : 'warning'}`}>
                                            {alert.severity}
                                        </span>
                                    </div>
                                ))
                            )}
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
}
