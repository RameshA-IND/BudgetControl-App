import { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { authApi, departmentApi } from '../api';
import { Department } from '../types';
import { Sun, Moon } from 'lucide-react';
import { useTheme } from '../contexts/ThemeContext';

export default function RegisterPage() {
    const [form, setForm] = useState({ fullName: '', email: '', password: '', role: 'Employee', departmentId: '' });
    const [departments, setDepartments] = useState<Department[]>([]);
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const { login } = useAuth();
    const { theme, toggleTheme } = useTheme();
    const navigate = useNavigate();

    useEffect(() => {
        departmentApi.getAll().then(r => setDepartments(r.data)).catch(() => { });
    }, []);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');
        setLoading(true);
        try {
            const payload = {
                ...form,
                departmentId: form.departmentId ? parseInt(form.departmentId) : undefined
            };
            const res = await authApi.register(payload);
            login(res.data.token, res.data.user);
            navigate('/');
        } catch (err: any) {
            setError(err.response?.data?.message || 'Registration failed');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="login-page">
            <div style={{ position: 'absolute', top: 20, right: 20 }}>
                <button className="btn btn-icon btn-ghost" onClick={toggleTheme} title="Toggle Theme">
                    {theme === 'dark' ? <Sun size={20} /> : <Moon size={20} />}
                </button>
            </div>
            <div className="login-card" style={{ maxWidth: 480 }}>
                <div className="login-logo">
                    <div className="logo-icon">B</div>
                    <h1>Create Account</h1>
                    <p>Join BudgetQ Platform</p>
                </div>
                {error && <div className="login-error">{error}</div>}
                <form onSubmit={handleSubmit}>
                    <div className="form-group">
                        <label className="form-label">Full Name</label>
                        <input className="form-input" placeholder="John Doe" value={form.fullName}
                            onChange={e => setForm({ ...form, fullName: e.target.value })} required />
                    </div>
                    <div className="form-group">
                        <label className="form-label">Email</label>
                        <input className="form-input" type="email" placeholder="name@company.com" value={form.email}
                            onChange={e => setForm({ ...form, email: e.target.value })} required />
                    </div>
                    <div className="form-group">
                        <label className="form-label">Password</label>
                        <input className="form-input" type="password" placeholder="Min 6 characters" value={form.password}
                            onChange={e => setForm({ ...form, password: e.target.value })} required minLength={6} />
                    </div>
                    <div className="form-row">
                        <div className="form-group">
                            <label className="form-label">Role</label>
                            <select className="form-select" value={form.role} onChange={e => setForm({ ...form, role: e.target.value })}>
                                <option value="Employee">Employee</option>
                                <option value="DepartmentManager">Department Manager</option>
                                <option value="FinanceAdmin">Finance Admin</option>
                            </select>
                        </div>
                        <div className="form-group">
                            <label className="form-label">Department</label>
                            <select className="form-select" value={form.departmentId} onChange={e => setForm({ ...form, departmentId: e.target.value })}>
                                <option value="">Select Department</option>
                                {departments.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
                            </select>
                        </div>
                    </div>
                    <button type="submit" className="btn btn-primary" style={{ width: '100%', padding: '12px' }} disabled={loading}>
                        {loading ? 'Creating Account...' : 'Create Account'}
                    </button>
                </form>
                <div className="login-footer">
                    Already have an account? <Link to="/login">Sign In</Link>
                </div>
            </div>
        </div>
    );
}
