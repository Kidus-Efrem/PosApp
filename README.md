# 🛒 .NET MAUI Point-of-Sale Application

A cross-platform **Point-of-Sale (POS) application** built with **.NET MAUI**, designed for cashier-facing tablet and desktop environments.

The application provides a complete local-first checkout workflow—from product discovery and cart management to payment selection, order persistence, receipt generation, and order history. It also includes a modernized implementation of a legacy Xamarin.Forms codebase, addressing several issues in the original architecture and business logic.

---

## ✨ Highlights

* 🛍️ **Product Catalog** — Search, filter by category, and sort products by name or price.
* 🛒 **Smart Cart** — Add items, adjust quantities, remove products, and enforce stock limits.
* 💰 **Accurate Checkout Calculations** — Handles subtotals, percentage/flat discounts, 8.5% sales tax, and grand totals.
* 💳 **Mock Payments** — Supports Cash and Card payment methods.
* 💾 **Local-First Persistence** — Stores products and completed orders using SQLite.
* 🧾 **Digital Receipts** — Interactive receipt view with PDF export using QuestPDF.
* 📋 **Order History** — Displays previously completed orders, newest first.
* 🧪 **Automated Testing** — xUnit tests covering calculations, discounts, and boundary conditions.
* 🖥️📱 **Cross-Platform** — Built and tested for Windows and Android.
* 🔄 **Modernized Legacy Code** — Refactors problematic Xamarin.Forms patterns into a reactive MVVM architecture.

---

## 📚 Table of Contents

* [✨ Highlights](#-highlights)
* [🏗️ Architecture](#️-architecture)
* [🛠️ Tech Stack](#️-tech-stack)
* [🛒 Core Functionality](#-core-functionality)

  * [Product Catalog](#product-catalog)
  * [Cart & Order Management](#cart--order-management)
  * [Checkout & Payments](#checkout--payments)
  * [Receipts & PDF Export](#receipts--pdf-export)
  * [Order History](#order-history)
* [📝 Legacy Xamarin.Forms Analysis](#-legacy-xamarinforms-analysis)

  * [Identified Issues](#identified-issues)
  * [Modernized Solution](#modernized-solution)
* [🧪 Testing](#-testing)
* [📂 Project Structure](#-project-structure)
* [🚀 Getting Started](#-getting-started)
* [💡 Assumptions & Future Improvements](#-assumptions--future-improvements)
* [🤖 AI Assistance Disclosure](#-ai-assistance-disclosure)
* [📱 Platform Support](#-platform-support)

---

## 🏗️ Architecture

The application follows the **Model-View-ViewModel (MVVM)** pattern, using `CommunityToolkit.Mvvm` to keep UI state and business logic cleanly separated.

```text
┌─────────────────────────────────────┐
│             .NET MAUI UI            │
│           XAML + Data Binding       │
└──────────────────┬──────────────────┘
                   │
                   ▼
┌─────────────────────────────────────┐
│           ViewModels (MVVM)         │
│                                     │
│  Cart • Products • Checkout • Orders│
└──────────────────┬──────────────────┘
                   │
          ┌────────┴────────┐
          ▼                 ▼
┌──────────────────┐  ┌──────────────────┐
│  SQLite Database │  │ Business Logic   │
│                  │  │                  │
│ Products / Orders│  │ Discounts / Tax  │
└──────────────────┘  └──────────────────┘
          │
          ▼
┌─────────────────────────────────────┐
│         External Functionality      │
│                                     │
│      QuestPDF • Payment Mocking     │
└─────────────────────────────────────┘
```

### Design Goals

The application was designed around three main principles:

* **Separation of concerns** — UI, state management, persistence, and business calculations are kept separate.
* **Offline-first operation** — Core POS functionality does not depend on a network connection.
* **Cross-platform compatibility** — The same application codebase targets both Windows and Android.

---

## 🛠️ Tech Stack

| Category           | Technology             |
| ------------------ | ---------------------- |
| **Framework**      | .NET 9.0 / .NET MAUI   |
| **Language**       | C#                     |
| **Architecture**   | MVVM                   |
| **MVVM Toolkit**   | CommunityToolkit.Mvvm  |
| **Database**       | SQLite                 |
| **SQLite Library** | sqlite-net-pcl         |
| **Testing**        | xUnit                  |
| **Test SDK**       | Microsoft.NET.Test.Sdk |
| **PDF Generation** | QuestPDF               |
| **CI/CD**          | GitHub Actions         |
| **Platforms**      | Windows, Android       |

---

## 🛒 Core Functionality

### Product Catalog

Products are seeded into a local SQLite database and can be explored through:

* Dynamic text search
* Category filtering
* Name sorting
* Price sorting
* Stock availability

The local database allows the catalog to remain available without requiring a network connection.

---

### Cart & Order Management

The cart provides a complete item management workflow:

* Add products to the cart
* Increase or decrease quantities
* Remove products
* Validate available stock
* Prevent quantities from exceeding inventory
* Automatically update financial calculations

Cart state is managed through the MVVM layer, allowing the UI to react immediately to changes.

---

### Checkout & Payments

The checkout system calculates:

1. Subtotal
2. Discount
3. Tax
4. Grand total

Supported promotional codes include:

| Code     | Type                | Value |
| -------- | ------------------- | ----: |
| `SAVE10` | Percentage discount |   10% |
| `FLAT5`  | Flat discount       |     5 |

The application applies an exact **8.5% sales tax** after the discount.

Mock payment methods are provided for:

* 💵 Cash
* 💳 Card

Once checkout is completed, the order is persisted locally and the corresponding product inventory is decremented.

---

### Receipts & PDF Export

After a successful checkout, the application provides an interactive receipt containing:

* Purchased items
* Quantities
* Subtotal
* Applied discount
* Tax
* Grand total
* Payment method

Receipts can also be exported as PDF documents using **QuestPDF**.

---

### Order History

Completed orders are persisted in SQLite and displayed in chronological order, with the newest orders appearing first.

This allows the cashier to review previous transactions even when operating offline.

---

## 📝 Legacy Xamarin.Forms Analysis

As part of the assessment, a legacy Xamarin.Forms `CartViewModel` implementation was analyzed and modernized.

The original implementation contained several architectural and business-logic issues that could lead to incorrect calculations or unreliable UI updates.

### Identified Issues

#### 1. Missing Property Notification

The legacy implementation did not properly implement `INotifyPropertyChanged`.

As a result, changes to properties such as `DiscountPercent` could fail to propagate to UI elements bound to those properties.

---

#### 2. Ambiguous Percentage Representation

Representing a percentage as a raw `double` introduces ambiguity.

For example:

```text
0.1  → Could mean 10%
10   → Could also mean 10%
```

Without explicit conventions and validation, this can easily result in incorrect discount calculations.

---

#### 3. Destructive / Compounding Calculations

Applying discounts directly to an already-modified total can cause calculations to compound incorrectly if the discount operation is triggered multiple times.

A safer approach is to always calculate from a stable subtotal:

```text
Items
  │
  ▼
Subtotal
  │
  ▼
Discount
  │
  ▼
Tax
  │
  ▼
Grand Total
```

This ensures recalculations remain deterministic.

---

#### 4. Tight Coupling

The legacy implementation mixed state management and business logic in ways that made the code harder to maintain and test.

The modernized implementation separates these responsibilities using MVVM and dedicated calculation logic.

---

## 🔧 Modernized Solution

The modern MAUI implementation refactors the legacy behavior into a reactive calculation flow.

The calculation process is:

```text
Cart Changes
     │
     ▼
Recalculate Subtotal
     │
     ▼
Validate Promo Code
     │
     ▼
Apply Discount
     │
     ▼
Calculate 8.5% Tax
     │
     ▼
Calculate Grand Total
     │
     ▼
Notify UI
```

Promo codes are explicitly validated, negative values and discounts exceeding the subtotal are handled safely, and calculations are always performed from the original subtotal rather than repeatedly modifying the previous result.

`CommunityToolkit.Mvvm` property notifications ensure that changes are reflected immediately in the UI.

---

## 🧪 Testing

The solution includes a dedicated test project:

```text
PosApp.Tests
```

The xUnit test suite covers the core business logic, including:

* Subtotal calculations with multiple quantities
* Percentage discount calculations
* Flat discount calculations
* Promo code validation
* Discounts exceeding the subtotal
* Cart calculation boundary conditions

Run the test suite with:

```bash
dotnet test PosApp.Tests/PosApp.Tests.csproj
```

---

## 📂 Project Structure

```text
PosApp/
├── PosApp/
│   ├── Models/              # Product, Order, Cart and related models
│   ├── ViewModels/          # MVVM view models
│   ├── Views/               # XAML UI pages
│   ├── Services/            # Database, PDF and application services
│   ├── Resources/           # Fonts, images and application resources
│   └── PosApp.csproj        # MAUI project configuration
│
├── PosApp.Tests/
│   ├── CartTests.cs         # Cart and calculation tests
│   └── PosApp.Tests.csproj  # Test project
│
└── README.md
```

---

## 🚀 Getting Started

### Prerequisites

* **.NET 9.0 SDK**
* .NET MAUI workload
* Visual Studio 2022 with the .NET MAUI workload, or VS Code with the appropriate .NET MAUI tooling

You can verify your .NET installation with:

```bash
dotnet --version
```

### 1. Clone the Repository

```bash
git clone https://github.com/Kidus-Efrem/PosApp
cd PosApp
```

### 2. Build & Run on Windows

```bash
dotnet build PosApp/PosApp.csproj -t:Run -f net9.0-windows10.0.19041.0
```

### 3. Run the Unit Tests

```bash
dotnet test PosApp.Tests/PosApp.Tests.csproj
```

### 4. Run on Android

The project can also be launched using an Android emulator or a connected Android device through Visual Studio or the .NET MAUI CLI.

---

## 💡 Assumptions & Future Improvements

### Current Assumptions

* Payment processing is mocked because a real payment gateway is outside the scope of the application.
* SQLite provides sufficient persistence for the local-first POS workflow.
* Inventory is managed locally and does not currently synchronize with a central server.
* Barcode scanning is represented through the existing product/SKU interaction rather than physical scanner hardware.

### With More Time

#### ☁️ Cloud Synchronization

Implement an offline-first synchronization service using a queue-based architecture.

```text
Offline Transaction
       │
       ▼
   SQLite Queue
       │
       │ Network Available
       ▼
   REST API
       │
       ▼
 Cloud Database
```

This would allow transactions to continue being created offline and automatically synchronize once connectivity is restored.

#### 📦 Barcode Scanner Integration

Integrate physical barcode scanners through a platform abstraction/dependency service so that scanning a product automatically adds the corresponding SKU to the cart.

#### 💳 Real Payment Processing

Replace the mock Cash/Card flow with a secure payment gateway integration suitable for the target deployment environment.

#### 🔐 Authentication & User Roles

For a production POS system, add authentication and role-based permissions for cashiers, managers, and administrators.

---

## 🤖 AI Assistance Disclosure

AI coding assistants, were used during development to accelerate:

* XAML UI boilerplate
* xUnit test structure
* GitHub Actions workflow configuration
* Debugging and implementation exploration

All AI-assisted code was **reviewed, tested, modified, and understood manually** before being integrated into the project.

---

## 📱 Platform Support

| Platform               | Status               |
| ---------------------- | -------------------- |
| 🪟 Windows 11 Desktop  | ✅ Tested             |
| 🤖 Android 14 Emulator | ✅ Tested             |
| Android minimum OS     | Android 21           |
| Windows minimum OS     | Windows 10.0.17763.0 |

---

## 📄 License

This project was developed as part of a technical assessment and is intended for educational and portfolio purposes.
