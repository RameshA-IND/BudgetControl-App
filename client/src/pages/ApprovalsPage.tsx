import { useState, useEffect } from 'react';
import { Clock, CheckCircle2, XCircle, MoreVertical } from 'lucide-react';
import { expenseApi } from '../api';
import { Expense } from '../types';
import { formatCurrency, formatDate } from '../utils/formatters';

export default function ApprovalsPage() {
    const [pending, setPending] = useState<Expense[]>([]);
    const [processed, setProcessed] = useState<Expense[]>([]);
    const [loading, setLoading] = useState(true);
    const [processingId, setProcessingId] = useState<number | null>(null);

    useEffect(() => {
        loadData();
    }, []);

    const loadData = async () => {
        try {
            const [p, all] = await Promise.all([
                expenseApi.getPending(),
                expenseApi.getAll()
            ]);
            setPending(p.data);
            setProcessed(all.data.filter((e: Expense) => e.status === 'Approved' || e.status === 'Rejected').slice(0, 5));
        } catch (e) { console.error(e); }
        finally { setLoading(false); }
    };

    const handleAction = async (id: number, action: 'Approved' | 'Rejected') => {
        if (processingId) return;
        setProcessingId(id);
        try {
            await expenseApi.updateStatus(id, { action });
            await loadData();
        } catch (err: any) {
            alert(err.response?.data?.message || 'Action failed');
        } finally {
            setProcessingId(null);
        }
    };

    const approvedToday = processed.filter(e =>
        e.approvals?.some(a => new Date(a.actionDate).toDateString() === new Date().toDateString() && a.action === 'Approved')
    ).length;

    const rejectedCount = processed.filter(e => e.status === 'Rejected').length;

    const getAvatarColor = (name: string) => {
        const colors = ['#06b6d4', '#8b5cf6', '#f59e0b', '#10b981', '#ef4444', '#ec4899'];
        const idx = name.charCodeAt(0) % colors.length;
        return colors[idx];
    };

    if (loading) return <div className="loading-spinner"><div className="spinner" /></div>;

    return (
        <>
            <div className="page-header">
                <div className="breadcrumb"><span>BudgetQ</span> / <span>Approval Workflow</span></div>
                <div>
                    <h1 className="page-title">Approval Workflow</h1>
                    <p className="page-subtitle">Review and process expense approval requests from departments</p>
                </div>
            </div>
            <div className="page-content">
                {/* Stat Cards */}
                <div className="stat-cards" style={{ gridTemplateColumns: 'repeat(3, 1fr)' }}>
                    <div className="stat-card">
                        <div>
                            <div className="stat-label">Pending Review</div>
                            <div className="stat-value">{pending.length}</div>
                        </div>
                        <div className="stat-icon orange"><Clock size={20} /></div>
                    </div>
                    <div className="stat-card">
                        <div>
                            <div className="stat-label">Approved Today</div>
                            <div className="stat-value">{approvedToday}</div>
                        </div>
                        <div className="stat-icon green"><CheckCircle2 size={20} /></div>
                    </div>
                    <div className="stat-card">
                        <div>
                            <div className="stat-label">Rejected</div>
                            <div className="stat-value">{rejectedCount}</div>
                        </div>
                        <div className="stat-icon red"><XCircle size={20} /></div>
                    </div>
                </div>

                {/* Pending Approvals */}
                <div className="card" style={{ marginBottom: 24 }}>
                    <div className="card-header">
                        <div className="card-title">Pending Approvals</div>
                        <span className="badge badge-warning">{pending.length} pending review</span>
                    </div>
                    {pending.length === 0 ? (
                        <div className="empty-state"><h3>No pending approvals</h3><p>All expenses have been processed</p></div>
                    ) : (
                        pending.map(exp => (
                            <div key={exp.id} className="approval-item">
                                <div className="approval-avatar" style={{ background: getAvatarColor(exp.submittedByName) }}>
                                    {exp.submittedByName.split(' ').map(n => n[0]).join('').slice(0, 2)}
                                </div>
                                <div className="approval-info" style={{ flex: 1 }}>
                                    <div className="approval-title">{exp.title}</div>
                                    <div className="approval-meta">
                                        <span>{exp.submittedByName}</span>
                                        <span>•</span>
                                        <span>{exp.departmentName}</span>
                                        <span>•</span>
                                        <span>{formatDate(exp.submittedAt)}</span>
                                    </div>
                                </div>
                                <div className="approval-actions">
                                    <span className="badge badge-neutral" style={{ marginRight: 8 }}>{exp.category}</span>
                                    <span className="approval-amount">{formatCurrency(exp.amount)}</span>
                                    <button
                                        className="btn btn-sm btn-danger"
                                        onClick={() => handleAction(exp.id, 'Rejected')}
                                        disabled={processingId === exp.id}
                                    >
                                        {processingId === exp.id ? '...' : 'Reject'}
                                    </button>
                                    <button
                                        className="btn btn-sm btn-success"
                                        onClick={() => handleAction(exp.id, 'Approved')}
                                        disabled={processingId === exp.id}
                                    >
                                        {processingId === exp.id ? '...' : 'Approve'}
                                    </button>
                                    <button className="btn btn-icon btn-ghost btn-sm"><MoreVertical size={14} /></button>
                                </div>
                            </div>
                        ))
                    )}
                </div>

                {/* Recently Processed */}
                <div className="card">
                    <div className="card-header">
                        <div className="card-title">Recently Processed</div>
                    </div>
                    {processed.length === 0 ? (
                        <div className="empty-state"><h3>No processed expenses yet</h3></div>
                    ) : (
                        processed.map(exp => (
                            <div key={exp.id} className="approval-item">
                                <div className="approval-avatar" style={{ background: getAvatarColor(exp.submittedByName) }}>
                                    {exp.submittedByName.split(' ').map(n => n[0]).join('').slice(0, 2)}
                                </div>
                                <div className="approval-info" style={{ flex: 1 }}>
                                    <div className="approval-title">{exp.title}</div>
                                    <div className="approval-meta">
                                        <span>{exp.submittedByName}</span>
                                        <span>•</span>
                                        <span>{exp.departmentName}</span>
                                        <span>•</span>
                                        <span>Approved by {exp.approvals?.[exp.approvals.length - 1]?.approverName || 'Unknown'}</span>
                                    </div>
                                </div>
                                <div className="approval-actions">
                                    <span className="approval-amount">{formatCurrency(exp.amount)}</span>
                                    <span className={`badge ${exp.status === 'Approved' ? 'badge-success' : 'badge-critical'}`}>
                                        {exp.status === 'Approved' ? 'Approved' : 'Rejected'}
                                    </span>
                                </div>
                            </div>
                        ))
                    )}
                </div>
            </div>
        </>
    );
}
