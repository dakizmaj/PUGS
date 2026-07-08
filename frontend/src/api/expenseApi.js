import axiosInstance from './axiosInstance';

export const expenseApi = {
  getAll: (planId) => axiosInstance.get(`/travel-plans/${planId}/expenses`),
  getSummary: (planId) => axiosInstance.get(`/travel-plans/${planId}/expenses/summary`),
  create: (planId, data) => axiosInstance.post(`/travel-plans/${planId}/expenses`, data),
  update: (planId, id, data) => axiosInstance.put(`/travel-plans/${planId}/expenses/${id}`, data),
  delete: (planId, id) => axiosInstance.delete(`/travel-plans/${planId}/expenses/${id}`),
};