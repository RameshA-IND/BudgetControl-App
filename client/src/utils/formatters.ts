export const formatCurrency = (amount: number): string => {
    return new Intl.NumberFormat('en-IN', {
        style: 'currency',
        currency: 'INR',
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
    }).format(amount);
};

export const formatDate = (dateStr: string): string => {
    return new Date(dateStr).toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric'
    });
};

export const formatDateTime = (dateStr: string): string => {
    return new Date(dateStr).toLocaleString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
};

export const getHealthColor = (health: string): string => {
    switch (health) {
        case 'Critical': return 'var(--color-critical)';
        case 'Warning': return 'var(--color-warning)';
        default: return 'var(--color-success)';
    }
};

export const getStatusColor = (status: string): string => {
    switch (status) {
        case 'Approved': return 'var(--color-success)';
        case 'Rejected': return 'var(--color-critical)';
        case 'DepartmentApproved': return 'var(--color-info)';
        default: return 'var(--color-warning)';
    }
};

export const downloadBlob = (blob: Blob, filename: string) => {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
};
