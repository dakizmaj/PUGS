import axiosInstance from './axiosInstance';

export const checklistApi = {
  getAll: (planId) => axiosInstance.get(`/travel-plans/${planId}/checklist-items`),
  create: (planId, data) => axiosInstance.post(`/travel-plans/${planId}/checklist-items`, data),
  toggle: (planId, id) => axiosInstance.patch(`/travel-plans/${planId}/checklist-items/${id}/toggle`),
  delete: (planId, id) => axiosInstance.delete(`/travel-plans/${planId}/checklist-items/${id}`),
};