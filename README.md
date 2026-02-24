## 📊 Departmental Budget Control & Expense Monitoring Platform

### Overview

This application is a finance operations platform designed for small to medium-sized IT organizations to manage departmental budgets, track expenses in real time, and proactively prevent budget overruns.

It replaces spreadsheet- and email-based expense tracking with a centralized, role-based system that improves visibility, control, and decision-making.

---

## Business Context

Organizations typically manage departmental expenses through manual processes, resulting in delayed financial insights and late detection of budget overruns.

This platform provides a centralized solution tailored to IT organizations, enabling finance teams and managers to monitor budgets and expenses in near real time.

---

## Problem Statement

There is no centralized system to monitor departmental expenses, control approval workflows, and proactively alert stakeholders when budgets are at risk.

---

## Objective

To build a finance operations platform that:

* Tracks departmental budgets in real time
* Controls expenses through approval workflows
* Triggers threshold-based alerts
* Enables proactive financial control

---

## User Personas

* **Finance Admin** – Creates budgets, monitors spending, manages approvals
* **Department Manager** – Reviews and approves department expenses
* **Employee** – Submits expenses

---

## Functional Scope (MVP)

### Budget Module

* Create departmental budgets for a defined period
* Total organization budget calculated as sum of all department budgets
* Configure warning and critical thresholds
* Real-time budget tracking based on approved expenses
* View utilization by amount and percentage
* Department-wise and category-wise utilization

### Expense Module

* Expense submission with department, amount, and category
* Mandatory category selection
* Predefined categories:

  * Food
  * Travel
  * Infrastructure
  * Hardware
  * Software Licenses
  * Learning
* Optional receipt upload
* Expense history with approval status

### Approval Workflow

* Department manager approval
* Optional finance approval layer
* Real-time approval status visibility

### Alerts

* Warning alerts when warning threshold is crossed
* Critical alerts when critical threshold is crossed
* Alerts visible on finance dashboard with severity indicators

### Dashboards

* Budget vs Actual overview
* Department-wise spending summary
* Top spending categories
* Pending approvals overview

### Reporting

* Export reports in Excel and PDF formats
* Budget vs Actual reports
* Department-wise expense reports

---

## Non-Functional Requirements

* Role-based access control
* Near real-time updates
* Secure receipt storage
* Responsive user interface

---

## Out of Scope (Demo)

* ERP or accounting integrations
* Tax and statutory calculations
* Multi-currency support

---

## Tech Stack

| Layer            | Technology             |
| ---------------- | ---------------------- |
| Frontend         | React (Vite)           |
| Backend          | ASP.NET Core 8 Web API |
| Database         | PostgreSQL (Supabase)  |
| Frontend Hosting | Vercel                 |
| Backend Hosting  | Render                 |
| Authentication   | JWT                    |

---

## Deployment

* Frontend deployed on **Vercel**
* Backend deployed on **Render**
* Database hosted on **Supabase**
* Configuration managed via environment variables
* Source code managed in GitHub

---

## Project Status

* PRD-aligned (v1.1)
* MVP / Demo scope
* Portfolio and interview ready
* Extensible for future enhancements

---

## Conclusion

This project implements the BA-defined PRD with a clean, scalable architecture and modern open-source tooling, delivering a real-world finance management solution suitable for demo and MVP use.