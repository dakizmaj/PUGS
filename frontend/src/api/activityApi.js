import axiosInstance from './axiosInstance';

export const activityApi = {
  getAll: (planId) => axiosInstance.get(`/travel-plans/${planId}/activities`),
  getCalendar: (planId) => axiosInstance.get(`/travel-plans/${planId}/activities/calendar`),
  create: (planId, data) => axiosInstance.post(`/travel-plans/${planId}/activities`, data),
  update: (planId, id, data) => axiosInstance.put(`/travel-plans/${planId}/activities/${id}`, data),
  delete: (planId, id) => axiosInstance.delete(`/travel-plans/${planId}/activities/${id}`),
};