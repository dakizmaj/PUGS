import axiosInstance from './axiosInstance';

export const authApi = {
  register: (data) => axiosInstance.post('/auth/register', data),
  login: (data) => axiosInstance.post('/auth/login', data),
  ldapLogin: (data) => axiosInstance.post('/auth/ldap-login', data),
  getCurrentUser: () => axiosInstance.get('/auth/me'),
};