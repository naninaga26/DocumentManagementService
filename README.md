# Microservices Solution - .NET Core 8

A modular solution containing Document, Auth, and Ingestion services with Docker Compose integration.

## 🚀 Features

- **Auth Service**: JWT-based authentication & authorization
- **Document Service**: File storage and management
- **Ingestion Service**: Data processing and Ingestion Service
- **Dockerized**: Ready-to-run containerized environment
- **API Documentation**: Integrated Swagger UI

## 📋 Prerequisites

- .NET Core 8 SDK
- Docker Desktop (with Compose)
- Git
- AWS

## 🛠️ Setup & Running

### Clone the repository
```bash
git clone https://github.com/yourusername/your-repo.git
cd your-repo
```

---

## 📁 Project Structure

```
.
├── docker-compose.yml
├── services/
│   ├── UserService/
│   ├── OrderService/
│   └── InventoryService/
└── README.md
```

---

## 🐳 Running the Full Solution with Docker Compose

To run all services together using Docker Compose:

```bash
docker-compose up --build
```

This will:

- Build all services
- Start containers for each service
- Use any defined networks, volumes, or dependencies in `docker-compose.yml`

> Ensure Docker is installed and running on your machine.

---

## 🔧 Running Services Individually

To run a specific service on your local machine:

```bash
cd path/to/service-name
dotnet run
```

For example:

```bash
cd services/Document.Services.AuthAPI
dotnet run
```

> Make sure any dependencies (e.g., databases, other services) are running if required.
---
