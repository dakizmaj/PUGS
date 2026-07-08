import axiosInstance from './axiosInstance';

export const shareApi = {
  create: (data) => axiosInstance.post('/share', data),
  validate: (token) => axiosInstance.get(`/share/${token}/validate`),
  getForPlan: (planId) => axiosInstance.get(`/share/plan/${planId}`),
  revoke: (id) => axiosInstance.delete(`/share/${id}`),
};