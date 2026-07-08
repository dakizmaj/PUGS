import axiosInstance from './axiosInstance';

export const adminApi = {
  getAllUsers: () => axiosInstance.get('/admin/users'),
  changeRole: (id, role) => axiosInstance.patch(`/admin/users/${id}/role`, { role }),
  deleteUser: (id) => axiosInstance.delete(`/admin/users/${id}`),
};