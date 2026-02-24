import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { AlertTriangle, AlertOctagon, CheckCircle, Bell } from 'lucide-react';
import { alertApi } from '../api';
import { Alert } from '../types';
import { formatDateTime, formatCurrency } from '../utils/formatters';
import { useAuth } from '../contexts/AuthContext';

export default function AlertsPage() {
    const { isFinanceAdmin } = useAuth();
    const navigate = useNavigate();
    const [alerts, setAlerts] = useState<Alert[]>([]);
    const [loading, setLoading] = useState(true);
    const [filter, setFilter] = useState<'all' | 'active' | 'acknowledged'>('all');

    useEffect(() => {
        loadAlerts();
    }, []);

    const loadAlerts = async () => {
        try {
            const res = await alertApi.getAll();
            setAlerts(res.data);
        } catch (e) { console.error(e); }
        finally { setLoading(false); }
    };

    const handleAcknowledge = async (id: number) => {
        try {
            await alertApi.markAsRead(id);
            loadAlerts();
        } catch (e) { console.error(e); }
    };

    const filteredAlerts = alerts.filter(a => {
        if (filter === 'active') return !a.isRead;
        if (filter === 'acknowledged') return a.isRead;
        return true;
    });

    const criticalCount = alerts.filter(a => a.severity === 'Critical' && !a.isRead).length;
    const warningCount = alerts.filter(a => a.severity === 'Warning' && !a.isRead).length;
    const acknowledgedCount = alerts.filter(a => a.isRead).length;

    if (loading) return <div className="loading-spinner"><div className="spinner" /></div>;

    return (
        <>
            <div className="page-header">
                <div className="breadcrumb"><span>BudgetQ</span> / <span>Alerts & Monitoring</span></div>

                {/* Tabs */}
                <div className="tab-bar" style={{ marginTop: 12 }}>
                    <button className="tab-item active" onClick={() => navigate('/alerts')}>
                        <Bell size={14} style={{ marginRight: 6 }} /> Alerts
                    </button>
                    {isFinanceAdmin && (
                        <button className="tab-item" onClick={() => navigate('/reports')}>
                            Reports
                        </button>
                    )}
                </div>

                <div>
                    <h1 className="page-title">Budget Alerts</h1>
                    <p className="page-subtitle">Threshold-based alerts for departments nearing budget exhaustion</p>
                </div>
            </div>
            <div className="page-content">
                {/* Stat Cards */}
                <div className="stat-cards" style={{ gridTemplateColumns: 'repeat(3, 1fr)' }}>
                    <div className="stat-card">
                        <div>
                            <div className="stat-label">Critical Alerts</div>
                            <div className="stat-value">{criticalCount}</div>
                        </div>
                        <div className="stat-icon red"><AlertOctagon size={20} /></div>
                    </div>
                    <div className="stat-card">
                        <div>
                            <div className="stat-label">Warning Alerts</div>
                            <div className="stat-value">{warningCount}</div>
                        </div>
                        <div className="stat-icon orange"><AlertTriangle size={20} /></div>
                    </div>
                    <div className="stat-card">
                        <div>
                            <div className="stat-label">Acknowledged</div>
                            <div className="stat-value">{acknowledgedCount}</div>
                        </div>
                        <div className="stat-icon green"><CheckCircle size={20} /></div>
                    </div>
                </div>

                {/* Filter Chips */}
                <div className="filter-row">
                    <button className={`filter-chip ${filter === 'all' ? 'active' : ''}`} onClick={() => setFilter('all')}>All</button>
                    <button className={`filter-chip ${filter === 'active' ? 'active' : ''}`} onClick={() => setFilter('active')}>Active</button>
                    <button className={`filter-chip ${filter === 'acknowledged' ? 'active' : ''}`} onClick={() => setFilter('acknowledged')}>Acknowledged</button>
                </div>

                {/* Alert Cards */}
                {filteredAlerts.length === 0 ? (
                    <div className="empty-state"><h3>No alerts found</h3><p>All departments are within budget</p></div>
                ) : (
                    filteredAlerts.map(alert => (
                        <div key={alert.id} className={`alert-card ${alert.severity.toLowerCase()} ${alert.isRead ? 'acknowledged' : ''}`}>
                            <div className={`alert-indicator ${alert.severity.toLowerCase()}`} />
                            <div style={{ flex: 1 }}>
                                <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 6 }}>
                                    <span style={{ fontWeight: 700, fontSize: 14, color: 'var(--text-heading)' }}>
                                        {alert.departmentName}
                                    </span>
                                    <span className={`badge badge-${alert.severity === 'Critical' ? 'critical' : 'warning'}`}>
                                        {alert.severity}
                                    </span>
                                    {alert.isRead && <span className="badge badge-neutral">Acknowledged</span>}
                                </div>
                                <div style={{ fontSize: 13, color: 'var(--text-secondary)', marginBottom: 6 }}>
                                    {alert.message}
                                </div>
                                <div style={{ fontSize: 11, color: 'var(--text-muted)', display: 'flex', gap: 16 }}>
                                    <span>▸ Threshold: {alert.utilizationPercent}%</span>
                                    <span>▸ Current: {formatCurrency(alert.spentAmount)} of {formatCurrency(alert.allocatedAmount)}</span>
                                    <span>▸ {formatDateTime(alert.createdAt)}</span>
                                </div>
                            </div>
                            {!alert.isRead && (
                                <button className="btn btn-sm btn-ghost" onClick={() => handleAcknowledge(alert.id)}>
                                    ✓ Acknowledge
                                </button>
                            )}
                        </div>
                    ))
                )}
            </div>
        </>
    );
}
