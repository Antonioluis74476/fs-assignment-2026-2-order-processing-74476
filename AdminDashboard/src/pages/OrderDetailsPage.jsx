import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { getOrderDetails } from "../api/ordersApi";

export default function OrderDetailsPage() {
  const { id } = useParams();
  const [details, setDetails] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    async function loadDetails() {
      try {
        setLoading(true);
        setError("");
        const data = await getOrderDetails(id);
        setDetails(data);
      } catch (err) {
        setError("Failed to load order details.");
        console.error(err);
      } finally {
        setLoading(false);
      }
    }

    loadDetails();
  }, [id]);

  if (loading) return <div className="page"><p>Loading order details...</p></div>;
  if (error) return <div className="page"><p className="error">{error}</p></div>;
  if (!details) return <div className="page"><p>No details found.</p></div>;

  return (
    <div className="page">
      <h1>Order Details</h1>

      <div className="card">
        <p><strong>Order ID:</strong> {details.order.id}</p>
        <p><strong>Customer:</strong> {details.order.customerName}</p>
        <p><strong>Status:</strong> {details.order.status}</p>
        <p><strong>Total:</strong> {details.order.totalAmount}</p>
        <p><strong>Payment:</strong> {details.order.paymentStatus ?? "-"}</p>
        <p><strong>Correlation ID:</strong> {details.order.correlationId}</p>
      </div>

      <h2>Items</h2>
      <ul>
        {details.items.map((item, index) => (
          <li key={index}>
            {item.productName} - Qty: {item.quantity} - Price: {item.unitPrice}
          </li>
        ))}
      </ul>

      <h2>Inventory Records</h2>
      <pre>{JSON.stringify(details.inventoryRecords, null, 2)}</pre>

      <h2>Payment Records</h2>
      <pre>{JSON.stringify(details.paymentRecords, null, 2)}</pre>

      <h2>Shipment Records</h2>
      <pre>{JSON.stringify(details.shipmentRecords, null, 2)}</pre>

      <Link to="/">Back to Orders</Link>
    </div>
  );
}