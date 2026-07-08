import axiosInstance from './axiosInstance';

export const sharedPlanApi = {
  getShared: (token) => axiosInstance.get(`/travel-plans/shared/${token}`),
  updateShared: (token, data) => axiosInstance.put(`/travel-plans/shared/${token}`, data),
};