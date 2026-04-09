import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { getOrders } from "../api/ordersApi";

export default function FailedOrdersPage() {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    async function loadFailedOrders() {
      try {
        setLoading(true);
        setError("");

        const paymentFailed = await getOrders("PaymentFailed");
        const inventoryFailed = await getOrders("InventoryFailed");

        const combined = [...paymentFailed, ...inventoryFailed];
        const unique = Array.from(new Map(combined.map(o => [o.id, o])).values());

        setOrders(unique);
      } catch (err) {
        setError("Failed to load failed orders.");
        console.error(err);
      } finally {
        setLoading(false);
      }
    }

    loadFailedOrders();
  }, []);

  return (
    <div className="page">
      <h1>Failed Orders</h1>

      {loading && <p>Loading failed orders...</p>}
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