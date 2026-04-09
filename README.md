# Distributed Order Processing System (SportsStore)

## Overview

This project implements a distributed, event-driven order processing system using ASP.NET Core, RabbitMQ, and React.

The system simulates a real-world e-commerce workflow where customer orders are processed asynchronously across multiple independent services, including inventory validation, payment processing, and shipping.

Two frontends are provided:

* A Blazor Server application (SportsStore) for customers
* A React-based Admin Dashboard for monitoring and management

---

## Architecture

The system follows a microservices-inspired architecture based on asynchronous messaging.

### Main Components

**SportsStore (Blazor Server)**
Customer-facing frontend used to browse products, place orders, and track order status.

**OrderManagement.API**
Central API responsible for:

* receiving checkout requests
* storing orders in the database
* publishing events to RabbitMQ

**InventoryService**
Processes inventory validation and determines whether items are available.

**PaymentService**
Simulates payment processing and determines whether a payment is approved or rejected.

**ShippingService**
Creates shipment records and marks orders as completed after successful payment.

**RabbitMQ**
Acts as the message broker, enabling communication between services.

**React Admin Dashboard**
Provides an interface for administrators to monitor orders, filter by status, and inspect processing details.

**SQL Server (LocalDB)**
Used by each service for data persistence.

**Serilog and Seq**
Provide structured logging and centralized log visualization.

---

## Event Flow

The system processes orders through a sequence of asynchronous events:

```
Checkout (SportsStore)
        ↓
OrderManagement.API
        ↓
OrderSubmitted (RabbitMQ)
        ↓
InventoryService
        ↓
InventoryConfirmed / InventoryFailed
        ↓
PaymentService
        ↓
PaymentApproved / PaymentRejected
        ↓
ShippingService
        ↓
ShippingCreated
        ↓
Order Completed
```

Each event includes a CorrelationId, which allows tracking a single order across all services.

---

## Customer Features (SportsStore)

* Browse products
* Add items to cart
* Submit orders through checkout
* Track order status
* View correlation ID for debugging and traceability

---

## Admin Dashboard (React)

The admin dashboard is available at:

```
http://localhost:5173
```

Features include:

* Viewing all orders
* Filtering orders by status (Submitted, InventoryConfirmed, PaymentApproved, PaymentFailed, Completed)
* Viewing failed orders
* Accessing detailed order information, including:

  * order items
  * inventory processing records
  * payment records
  * shipment records

---

## Logging and Observability

The system uses Serilog for structured logging and Seq for visualization.

Each log entry includes:

* OrderId
* CustomerId
* EventType
* ServiceName
* CorrelationId

This allows full traceability of each order across all services.

Seq can be accessed at:

```
http://localhost:5341
```

---

## Payment Simulation

The PaymentService simulates payment decisions using a simple rule:

* Orders with total amount less than or equal to 1000 are approved
* Orders with total amount greater than 1000 are rejected

This approach allows consistent testing of both successful and failed payment scenarios without external dependencies.

---

## Testing

The solution includes:

**SportsStore.Tests**

* Unit tests for cart and order logic

**DistributedServices.Tests**

* API checkout test
* Inventory processing test
* Payment processing test
* Shipping completion test

All tests are executed through GitHub Actions.

---

## How to Run the Project

### 1. Start RabbitMQ using Docker

```
docker run -d -p 5672:5672 -p 15672:15672 rabbitmq:management
```

RabbitMQ management interface:

```
http://localhost:15672
```

---

### 2. Run Backend Services

Start the following projects in Visual Studio:

* OrderManagement.API
* InventoryService
* PaymentService
* ShippingService

---

### 3. Run SportsStore (Customer Frontend)

* Set SportsStore as the startup project
* Run the application
* Open:

```
http://localhost:5000
```

---

### 4. Run React Admin Dashboard

From the AdminDashboard folder:

```
npm install
npm run dev
```

Open:

```
http://localhost:5173
```

---

## Docker Support

RabbitMQ is executed using Docker.
The system has been designed to support full containerization if extended with Docker Compose.

---

## Design Decisions

**Event-driven architecture**

* Improves scalability
* Decouples services
* Enables asynchronous processing
* Allows independent service evolution

**Simulated payment service**

* Avoids reliance on third-party providers
* Ensures predictable and testable behavior
* Aligns with assignment requirements

**CorrelationId usage**

* Enables tracking across services
* Simplifies debugging in a distributed environment

---

## Future Improvements

* Full Docker Compose configuration for all services
* Real-time updates using SignalR
* Improved user interface for the admin dashboard
* Integration with real payment providers
* Retry mechanisms for failed operations

---

## Technologies Used

* ASP.NET Core (.NET 8)
* Entity Framework Core
* RabbitMQ with MassTransit
* React (Vite)
* Blazor Server
* SQL Server (LocalDB)
* Serilog and Seq
* xUnit and Moq

---

## Repository

The repository is public and accessible.
The project supervisor has been added as a collaborator.

---

## Conclusion

This project demonstrates a distributed system that uses asynchronous communication to process orders across multiple services. It combines backend services, messaging infrastructure, and both customer and administrative frontends to simulate a realistic and scalable order processing platform.
