const axios = require('axios');
axios.post('http://localhost:5000/api/auth/login', { email: 'admin@budgetq.com', password: 'AdminPassword123!' })
    .then(res => {
        return axios.get('http://localhost:5000/api/dashboard/monthly-trends', {
            headers: { Authorization: 'Bearer ' + res.data.token }
        });
    })
    .then(res => console.log(JSON.stringify(res.data, null, 2)))
    .catch(console.error);
