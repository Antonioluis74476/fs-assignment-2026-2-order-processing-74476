import axios from "axios";

const api = axios.create({
  baseURL: "https://localhost:7150/api",
});

export async function getOrders(status) {
  const url = status ? `/orders?status=${encodeURIComponent(status)}` : "/orders";
  const response = await api.get(url);
  return response.data;
}

export async function getOrderDetails(orderId) {
  const response = await api.get(`/orders/${orderId}/details`);
  return response.data;
}

export async function getOrderStatus(orderId) {
  const response = await api.get(`/orders/${orderId}/status`);
  return response.data;
}