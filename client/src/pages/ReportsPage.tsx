import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { FileSpreadsheet, FileText, Bell } from 'lucide-react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, ResponsiveContainer, BarChart, Bar, Cell, Legend } from 'recharts';
import { reportApi, dashboardApi } from '../api';
import { downloadBlob, formatCurrency } from '../utils/formatters';
import { DepartmentSpending, CategorySpending, MonthlyTrend } from '../types';

const CHART_COLORS = ['#06b6d4', '#8b5cf6', '#f59e0b', '#10b981', '#ef4444', '#ec4899'];

export default function ReportsPage() {
    const navigate = useNavigate();
    const [loadingExcel, setLoadingExcel] = useState(false);
    const [loadingPdf, setLoadingPdf] = useState(false);
    const [loadingData, setLoadingData] = useState(true);

    const [deptSpending, setDeptSpending] = useState<DepartmentSpending[]>([]);
    const [categorySpending, setCategorySpending] = useState<CategorySpending[]>([]);
    const [trends, setTrends] = useState<MonthlyTrend[]>([]);

    useEffect(() => {
        Promise.all([
            dashboardApi.getDepartmentSpending(),
            dashboardApi.getCategorySpending(),
            dashboardApi.getMonthlyTrends()
        ]).then(([d, c, t]) => {
            setDeptSpending(d.data);
            setCategorySpending(c.data);
            setTrends(t.data);
        }).catch(console.error).finally(() => setLoadingData(false));
    }, []);

    const handleExportExcel = async () => {
        setLoadingExcel(true);
        try {
            const res = await reportApi.exportExcel();
            downloadBlob(new Blob([res.data]), `expense_report_${new Date().toISOString().slice(0, 10)}.xlsx`);
        } catch (e) {
            alert('Failed to export Excel');
        } finally { setLoadingExcel(false); }
    };

    const handleExportPdf = async () => {
        setLoadingPdf(true);
        try {
            const res = await reportApi.exportPdf();
            downloadBlob(new Blob([res.data]), `expense_report_${new Date().toISOString().slice(0, 10)}.pdf`);
        } catch (e) {
            alert('Failed to export PDF');
        } finally { setLoadingPdf(false); }
    };

    if (loadingData) return <div className="loading-spinner"><div className="spinner" /></div>;

    const totalAllocated = deptSpending.reduce((sum, d) => sum + d.allocatedAmount, 0);
    const totalSpent = deptSpending.reduce((sum, d) => sum + d.spentAmount, 0);
    const totalRemaining = deptSpending.reduce((sum, d) => sum + d.remainingAmount, 0);
    const totalUtilized = totalAllocated > 0 ? ((totalSpent / totalAllocated) * 100).toFixed(1) : 0;

    return (
        <>
            <div className="page-header">
                <div className="breadcrumb"><span>BudgetQ</span> / <span>Reports & Export</span></div>

                {/* Tabs */}
                <div className="tab-bar" style={{ marginTop: 12 }}>
                    <button className="tab-item" onClick={() => navigate('/alerts')}>
                        <Bell size={14} style={{ marginRight: 6 }} /> Alerts
                    </button>
                    <button className="tab-item active" onClick={() => navigate('/reports')}>
                        Reports
                    </button>
                </div>

                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginTop: 24 }}>
                    <div>
                        <h1 className="page-title">Financial Reports</h1>
                        <p className="page-subtitle">Generate and export departmental budget and expense reports</p>
                    </div>
                    <div style={{ display: 'flex', gap: 12 }}>
                        <button className="btn btn-secondary" onClick={handleExportExcel} disabled={loadingExcel}>
                            <FileSpreadsheet size={16} /> {loadingExcel ? 'Exporting...' : 'Export Excel'}
                        </button>
                        <button className="btn btn-secondary" onClick={handleExportPdf} disabled={loadingPdf}>
                            <FileText size={16} /> {loadingPdf ? 'Exporting...' : 'Export PDF'}
                        </button>
                    </div>
                </div>
            </div>

            <div className="page-content">

                {/* Monthly Spending Trend */}
                <div className="card" style={{ marginBottom: 24 }}>
                    <div className="card-header">
                        <div>
                            <div className="card-title">Monthly Spending Trend</div>
                            <div className="card-subtitle">6-month spending trend with budget baseline</div>
                        </div>
                        <div className="filter-dropdown">
                            <select className="form-input" style={{ width: 140, padding: '6px 12px' }}>
                                <option>Last 6 months</option>
                                <option>This Year</option>
                            </select>
                        </div>
                    </div>
                    <div className="card-body" style={{ height: 350 }}>
                        <ResponsiveContainer width="100%" height="100%">
                            <LineChart data={trends} margin={{ top: 20, right: 30, left: 20, bottom: 5 }}>
                                <CartesianGrid strokeDasharray="3 3" stroke="#1e2d4a" vertical={false} />
                                <XAxis dataKey="month" stroke="#64748b" fontSize={11} tickLine={false} axisLine={false} />
                                <YAxis stroke="#64748b" fontSize={11} tickFormatter={(v: any) => `₹${v / 1000}k`} tickLine={false} axisLine={false} />
                                <RechartsTooltip
                                    contentStyle={{ background: '#151d2e', border: '1px solid #253552', borderRadius: 8, color: '#f1f5f9', fontSize: 12 }}
                                    itemStyle={{ color: '#f1f5f9' }}
                                    labelStyle={{ color: '#94a3b8', marginBottom: 4 }}
                                    formatter={(value: any, name: any) => [formatCurrency(value as number), name]}
                                />
                                <Legend wrapperStyle={{ fontSize: 12, paddingTop: 20 }} iconType="circle" iconSize={8} />
                                <Line type="monotone" dataKey="budgetAmount" name="Budget" stroke="#06b6d4" strokeWidth={2} strokeDasharray="5 5" dot={false} activeDot={{ r: 6 }} />
                                <Line type="monotone" dataKey="spentAmount" name="Actual" stroke="#10b981" strokeWidth={2} dot={{ strokeWidth: 2, r: 4 }} activeDot={{ r: 6 }} />
                            </LineChart>
                        </ResponsiveContainer>
                    </div>
                </div>

                {/* Department Budget Summary Table */}
                <div className="card" style={{ marginBottom: 24 }}>
                    <div className="card-header">
                        <div>
                            <div className="card-title">Department Budget Summary</div>
                            <div className="card-subtitle">Comprehensive breakdown of budget allocation and utilization</div>
                        </div>
                    </div>
                    <table className="data-table">
                        <thead>
                            <tr>
                                <th>Department</th>
                                <th>Manager</th>
                                <th style={{ textAlign: 'right' }}>Allocated</th>
                                <th style={{ textAlign: 'right' }}>Spent</th>
                                <th style={{ textAlign: 'right' }}>Remaining</th>
                                <th>Utilization</th>
                                <th>Status</th>
                            </tr>
                        </thead>
                        <tbody>
                            {deptSpending.map((d) => (
                                <tr key={d.departmentId}>
                                    <td className="cell-title" style={{ fontWeight: 600 }}>{d.departmentName}</td>
                                    <td style={{ color: 'var(--text-secondary)' }}>Department Manager</td>
                                    <td style={{ textAlign: 'right' }}>{formatCurrency(d.allocatedAmount)}</td>
                                    <td style={{ textAlign: 'right' }}>{formatCurrency(d.spentAmount)}</td>
                                    <td style={{ textAlign: 'right' }}>{formatCurrency(d.remainingAmount)}</td>
                                    <td>
                                        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                                            <span style={{ fontSize: 12, fontWeight: 600, minWidth: 40 }}>
                                                {d.utilizationPercent}%
                                            </span>
                                            <div className="progress-bar" style={{ width: 100 }}>
                                                <div
                                                    className={`progress-fill ${d.healthStatus.toLowerCase()}`}
                                                    style={{ width: `${Math.min(d.utilizationPercent, 100)}%` }}
                                                />
                                            </div>
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
                        <tfoot style={{ borderTop: '2px solid var(--border-color)', fontWeight: 700 }}>
                            <tr>
                                <td>Total</td>
                                <td>-</td>
                                <td style={{ textAlign: 'right' }}>{formatCurrency(totalAllocated)}</td>
                                <td style={{ textAlign: 'right' }}>{formatCurrency(totalSpent)}</td>
                                <td style={{ textAlign: 'right' }}>{formatCurrency(totalRemaining)}</td>
                                <td>{totalUtilized}%</td>
                                <td>-</td>
                            </tr>
                        </tfoot>
                    </table>
                </div>

                {/* Category-wise Spending Breakdown */}
                <div className="card">
                    <div className="card-header">
                        <div>
                            <div className="card-title">Category-wise Spending Breakdown</div>
                            <div className="card-subtitle">Total spending segmented by expense category</div>
                        </div>
                    </div>
                    <div className="card-body" style={{ height: 400 }}>
                        <ResponsiveContainer width="100%" height="100%">
                            <BarChart data={categorySpending} layout="vertical" margin={{ top: 20, right: 30, left: 100, bottom: 5 }}>
                                <CartesianGrid strokeDasharray="3 3" stroke="#1e2d4a" horizontal={false} />
                                <XAxis type="number" stroke="#64748b" fontSize={11} tickFormatter={(v: any) => `₹${v / 1000}k`} tickLine={false} axisLine={false} />
                                <YAxis dataKey="category" type="category" stroke="#64748b" fontSize={11} tickLine={false} axisLine={false} width={120} />
                                <RechartsTooltip
                                    contentStyle={{ background: '#151d2e', border: '1px solid #253552', borderRadius: 8, color: '#f1f5f9', fontSize: 12 }}
                                    itemStyle={{ color: '#f1f5f9' }}
                                    labelStyle={{ color: '#94a3b8', marginBottom: 4 }}
                                    formatter={(value: any) => [formatCurrency(value as number)]}
                                    cursor={{ fill: '#1e2d4a', opacity: 0.4 }}
                                />
                                <Bar dataKey="totalAmount" fill="#0369a1" radius={[0, 4, 4, 0]} barSize={24}>
                                    {categorySpending.map((_, index) => (
                                        <Cell key={`cell-${index}`} fill={CHART_COLORS[0]} /> // Always using the blue color like in the screenshot
                                    ))}
                                </Bar>
                            </BarChart>
                        </ResponsiveContainer>
                    </div>
                </div>

            </div>
        </>
    );
}
