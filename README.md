# 🛒 eShop Application

Welcome to the **eShop Application**! This is a modern, scalable, and feature-rich e-commerce backend built with **.NET 8**. It leverages a robust microservices architecture, integrated with cutting-edge tools and services to deliver a seamless shopping experience.

---

## 🚀 Features

- **⚙️ .NET 8 REST API**: Core backend built using Clean Architecture and Vertical Slice with MediatR and CQRS.
- **🔐 Authentication**: Secure user management with Keycloak.
- **💳 Payment Processing**: Integrated Stripe API for smooth transactions.
- **🔍 Search**: Full-text search powered by Elasticsearch.
- **⚡ Caching and Storage**: Redis used for caching and shopping cart storage.
- **📬 Messaging**: RabbitMQ with MassTransit for asynchronous messaging.
- **📊 Logging and Monitoring**: Seq and Serilog for comprehensive logging.
- **🗃️ Database**: Microsoft SQL Server as the primary database.
- **🧪 Unit Testing**: Comprehensive test suite using xUnit, Fluent Assertions, and SQLite for DB faking.
- **📁 Static File Storage**: Supabase integration for handling images and static files.
- **🐳 Containerized Services**: Docker and Docker Compose for easy deployment.

---

## 🛠️ Tech Stack

| Technology               | Purpose                                |
|---------------------------|----------------------------------------|
| ![.NET 8](https://img.shields.io/badge/.NET-8.0-blueviolet) | Backend framework                     |
| ![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-red) | Relational database                   |
| ![Elasticsearch](https://img.shields.io/badge/Elasticsearch-8.15.1-yellowgreen) | Full-text search                      |
| ![Redis](https://img.shields.io/badge/Redis-In%20Memory-red)      | Caching and session storage           |
| ![RabbitMQ](https://img.shields.io/badge/RabbitMQ-Message%20Broker-orange) | Message broker                        |
| ![Seq](https://img.shields.io/badge/Seq-Logging%20Dashboard-blue) | Centralized logging                   |
| ![Keycloak](https://img.shields.io/badge/Keycloak-Authentication-orange) | Identity and access management        |
| ![Stripe](https://img.shields.io/badge/Stripe-Payment%20Integration-lightblue) | Payment integration                   |
| ![xUnit](https://img.shields.io/badge/xUnit-Testing-brightgreen)  | Unit testing framework                |
| ![Fluent Assertions](https://img.shields.io/badge/Fluent%20Assertions-Testing-yellowgreen) | Assertions for unit tests             |
| ![SQLite](https://img.shields.io/badge/SQLite-In%20Memory-lightgrey) | Lightweight in-memory database        |
| ![Docker](https://img.shields.io/badge/Docker-Containerization-blue) | Containerization                      |
| ![Supabase](https://img.shields.io/badge/Supabase-Static%20Files-green) | Static file storage                   |

---

## 🔧 Docker Compose Setup

This project uses Docker Compose to manage its services.

### Available Services

- **API** (`eshop.api`) - The main backend service.
- **Database** (`eshop.db`) - SQL Server for data persistence.
- **Redis** (`eshop.redis`) - In-memory caching.
- **RabbitMQ** (`eshop.rabbitmq`) - Messaging service with a management UI.
- **Elasticsearch** (`eshop.elasticsearch`) - Search engine.
- **Seq** (`eshop.seq`) - Logging dashboard.
- **Keycloak** (`eshop.keycloak`) - Authentication and authorization.

### Quick Start

#### Prerequisites

- Install **[Docker](https://www.docker.com/)** and **[Docker Compose](https://docs.docker.com/compose/)**.
- Clone the repository:
  ```bash
  git clone https://github.com/your-repo/eshop.git
  cd eshop
  ```
