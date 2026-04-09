import { BrowserRouter, Link, Route, Routes } from "react-router-dom";
import OrdersPage from "./pages/OrdersPage";
import FailedOrdersPage from "./pages/FailedOrdersPage";
import OrderDetailsPage from "./pages/OrderDetailsPage";

export default function App() {
  return (
    <BrowserRouter>
      <div className="layout">
        <nav className="sidebar">
          <h2>Admin Dashboard</h2>
          <Link to="/">Orders</Link>
          <Link to="/failed">Failed Orders</Link>
        </nav>

        <main className="content">
          <Routes>
            <Route path="/" element={<OrdersPage />} />
            <Route path="/failed" element={<FailedOrdersPage />} />
            <Route path="/orders/:id" element={<OrderDetailsPage />} />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  );
}