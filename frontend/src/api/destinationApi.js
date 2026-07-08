import axiosInstance from './axiosInstance';

export const destinationApi = {
  getAll: (planId) => axiosInstance.get(`/travel-plans/${planId}/destinations`),
  create: (planId, data) => axiosInstance.post(`/travel-plans/${planId}/destinations`, data),
  update: (planId, id, data) => axiosInstance.put(`/travel-plans/${planId}/destinations/${id}`, data),
  delete: (planId, id) => axiosInstance.delete(`/travel-plans/${planId}/destinations/${id}`),
};