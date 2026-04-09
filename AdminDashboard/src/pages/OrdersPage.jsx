import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { getOrders } from "../api/ordersApi";

export default function OrdersPage() {
  const [orders, setOrders] = useState([]);
  const [statusFilter, setStatusFilter] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  async function loadOrders(filter = "") {
    try {
      setLoading(true);
      setError("");
      const data = await getOrders(filter || undefined);
      setOrders(data);
    } catch (err) {
      setError("Failed to load orders.");
      console.error(err);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadOrders(statusFilter);
  }, [statusFilter]);

  return (
    <div className="page">
      <h1>Orders Dashboard</h1>

      <div className="toolbar">
        <label>Status Filter:</label>
        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
        >
          <option value="">All</option>
          <option value="Submitted">Submitted</option>
          <option value="InventoryConfirmed">InventoryConfirmed</option>
          <option value="PaymentApproved">PaymentApproved</option>
          <option value="PaymentFailed">PaymentFailed</option>
          <option value="Completed">Completed</option>
        </select>
      </div>

      {loading && <p>Loading orders...</p>}
      {error && <p className="error">{error}</p>}

      {!loading && !error && (
        <table className="orders-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Customer</th>
              <th>Status</th>
              <th>Total</th>
              <th>Payment</th>
              <th>Created</th>
              <th>Details</th>
            </tr>
          </thead>
          <tbody>
            {orders.map((order) => (
              <tr key={order.id}>
                <td>{order.id}</td>
                <td>{order.customerName}</td>
                <td>{order.status}</td>
                <td>{order.totalAmount}</td>
                <td>{order.paymentStatus ?? "-"}</td>
                <td>{new Date(order.createdAtUtc).toLocaleString()}</td>
                <td>
                  <Link to={`/orders/${order.id}`}>View</Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}