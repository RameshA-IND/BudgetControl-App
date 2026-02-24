import { useState, useEffect } from 'react';
import { Plus, Building2, Users } from 'lucide-react';
import { departmentApi } from '../api';
import { Department } from '../types';

export default function DepartmentsPage() {
    const [departments, setDepartments] = useState<Department[]>([]);
    const [showModal, setShowModal] = useState(false);
    const [loading, setLoading] = useState(true);
    const [form, setForm] = useState({ name: '', code: '', description: '' });

    useEffect(() => {
        loadData();
    }, []);

    const loadData = async () => {
        try {
            const res = await departmentApi.getAll();
            setDepartments(res.data);
        } catch (e) { console.error(e); }
        finally { setLoading(false); }
    };

    const handleCreate = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            await departmentApi.create(form);
            setShowModal(false);
            setForm({ name: '', code: '', description: '' });
            loadData();
        } catch (err: any) {
            alert(err.response?.data?.message || 'Failed to create department');
        }
    };

    if (loading) return <div className="loading-spinner"><div className="spinner" /></div>;

    return (
        <>
            <div className="page-header">
                <div className="breadcrumb"><span>BudgetQ</span> / <span>Departments</span></div>
                <div className="page-title-row">
                    <div>
                        <h1 className="page-title">Departments</h1>
                        <p className="page-subtitle">Manage organizational departments</p>
                    </div>
                    <button className="btn btn-primary" onClick={() => setShowModal(true)}>
                        <Plus size={16} /> Add Department
                    </button>
                </div>
            </div>
            <div className="page-content">
                <div className="grid-3">
                    {departments.map(dept => (
                        <div key={dept.id} className="budget-card">
                            <div className="budget-card-header">
                                <div className="budget-dept-name">
                                    <Building2 size={16} color="var(--color-primary)" /> {dept.name}
                                </div>
                                <span className="badge badge-info">{dept.code}</span>
                            </div>
                            <p style={{ fontSize: 13, color: 'var(--text-muted)', margin: '8px 0 14px' }}>
                                {dept.description || 'No description'}
                            </p>
                            <div className="budget-footer">
                                <span><Users size={14} style={{ verticalAlign: -2 }} /> {dept.employeeCount} employees</span>
                                <span>Manager: {dept.managerName || 'Unassigned'}</span>
                            </div>
                        </div>
                    ))}
                </div>
            </div>

            {showModal && (
                <div className="modal-overlay" onClick={() => setShowModal(false)}>
                    <div className="modal" onClick={e => e.stopPropagation()}>
                        <div className="modal-header">
                            <div className="modal-title">Add Department</div>
                            <button className="btn btn-icon btn-ghost" onClick={() => setShowModal(false)}>✕</button>
                        </div>
                        <form onSubmit={handleCreate}>
                            <div className="modal-body">
                                <div className="form-group">
                                    <label className="form-label">Department Name</label>
                                    <input className="form-input" placeholder="e.g., Engineering" value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} required />
                                </div>
                                <div className="form-group">
                                    <label className="form-label">Code</label>
                                    <input className="form-input" placeholder="e.g., ENG" value={form.code} onChange={e => setForm({ ...form, code: e.target.value })} required />
                                </div>
                                <div className="form-group">
                                    <label className="form-label">Description</label>
                                    <textarea className="form-textarea" placeholder="Department description..." value={form.description} onChange={e => setForm({ ...form, description: e.target.value })} />
                                </div>
                            </div>
                            <div className="modal-footer">
                                <button type="button" className="btn btn-ghost" onClick={() => setShowModal(false)}>Cancel</button>
                                <button type="submit" className="btn btn-primary">Create Department</button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </>
    );
}
