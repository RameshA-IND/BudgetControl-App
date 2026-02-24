import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import {
    LayoutDashboard, Wallet, Receipt, CheckCircle2,
    AlertTriangle, FileBarChart, Building2, LogOut, Sun, Moon
} from 'lucide-react';
import { useTheme } from '../../contexts/ThemeContext';
import { useState, useEffect } from 'react';
import { alertApi } from '../../api';

export default function Layout() {
    const { user, logout, isFinanceAdmin, isDepartmentManager } = useAuth();
    const { theme, toggleTheme } = useTheme();
    const navigate = useNavigate();
    const [alertCount, setAlertCount] = useState(0);

    useEffect(() => {
        alertApi.getCount().then(res => setAlertCount(res.data)).catch(() => { });
        const interval = setInterval(() => {
            alertApi.getCount().then(res => setAlertCount(res.data)).catch(() => { });
        }, 30000);
        return () => clearInterval(interval);
    }, []);

    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    const initials = user?.fullName
        ?.split(' ')
        .map(n => n[0])
        .join('')
        .toUpperCase()
        .slice(0, 2) || 'U';

    return (
        <div className="app-layout">
            <aside className="sidebar">
                <div className="sidebar-logo">
                    <div className="logo-icon">B</div>
                    <div>
                        <div className="logo-text">Budget<span>Q</span></div>
                        <div className="logo-subtitle">Finance Platform</div>
                    </div>
                    <button
                        className="btn btn-icon btn-ghost"
                        onClick={toggleTheme}
                        style={{ marginLeft: 'auto', border: 'none' }}
                        title={`Switch to ${theme === 'dark' ? 'Light' : 'Dark'} Mode`}
                    >
                        {theme === 'dark' ? <Sun size={18} /> : <Moon size={18} />}
                    </button>
                </div>

                <nav className="sidebar-nav">
                    <div className="sidebar-section">Navigation</div>

                    <NavLink to="/" end className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}>
                        <LayoutDashboard size={18} />
                        Dashboard
                    </NavLink>

                    <NavLink to="/budgets" className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}>
                        <Wallet size={18} />
                        Budgets
                    </NavLink>

                    <NavLink to="/expenses" className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}>
                        <Receipt size={18} />
                        Expenses
                    </NavLink>

                    {(isFinanceAdmin || isDepartmentManager) && (
                        <NavLink to="/approvals" className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}>
                            <CheckCircle2 size={18} />
                            Approvals
                        </NavLink>
                    )}

                    <NavLink to="/alerts" className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}>
                        <AlertTriangle size={18} />
                        Alerts
                        {alertCount > 0 && <span className="badge">{alertCount}</span>}
                    </NavLink>

                    {isFinanceAdmin && (
                        <>
                            <NavLink to="/departments" className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}>
                                <Building2 size={18} />
                                Departments
                            </NavLink>
                            <NavLink to="/reports" className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}>
                                <FileBarChart size={18} />
                                Reports
                            </NavLink>
                        </>
                    )}
                </nav>

                <div className="sidebar-user">
                    <div className="avatar">{initials}</div>
                    <div className="user-info">
                        <div className="user-name">{user?.fullName}</div>
                        <div className="user-role">{user?.role?.replace(/([A-Z])/g, ' $1').trim()}</div>
                    </div>
                    <button className="btn btn-icon btn-ghost" onClick={handleLogout} title="Logout">
                        <LogOut size={16} />
                    </button>
                </div>
            </aside>

            <main className="main-content">
                <Outlet />
            </main>
        </div>
    );
}
