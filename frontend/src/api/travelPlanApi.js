import axiosInstance from './axiosInstance';

export const travelPlanApi = {
  getAll: () => axiosInstance.get('/travel-plans'),
  getById: (id) => axiosInstance.get(`/travel-plans/${id}`),
  create: (data) => axiosInstance.post('/travel-plans', data),
  update: (id, data) => axiosInstance.put(`/travel-plans/${id}`, data),
  delete: (id) => axiosInstance.delete(`/travel-plans/${id}`),
  getReport: (id) => axiosInstance.get(`/travel-plans/${id}/report`, { responseType: 'blob' }),
};