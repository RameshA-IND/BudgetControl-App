Build started...
Build succeeded.
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

CREATE TABLE expense_categories (
    id uuid NOT NULL,
    name character varying(100) NOT NULL,
    description text,
    is_active boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    CONSTRAINT "PK_expense_categories" PRIMARY KEY (id)
);

CREATE TABLE alerts (
    id uuid NOT NULL,
    budget_id uuid NOT NULL,
    department_id uuid NOT NULL,
    severity character varying(20) NOT NULL,
    message text NOT NULL,
    utilization_percent numeric(5,2) NOT NULL,
    is_read boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    CONSTRAINT "PK_alerts" PRIMARY KEY (id)
);

CREATE TABLE budgets (
    id uuid NOT NULL,
    department_id uuid NOT NULL,
    fiscal_year character varying(10) NOT NULL,
    period_start timestamp with time zone NOT NULL,
    period_end timestamp with time zone NOT NULL,
    allocated_amount numeric(18,2) NOT NULL,
    spent_amount numeric(18,2) NOT NULL,
    warning_threshold_pct numeric(5,2) NOT NULL,
    critical_threshold_pct numeric(5,2) NOT NULL,
    status character varying(20) NOT NULL,
    created_by_id uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT "PK_budgets" PRIMARY KEY (id)
);

CREATE TABLE departments (
    id uuid NOT NULL,
    name character varying(100) NOT NULL,
    code character varying(20) NOT NULL,
    description text,
    manager_id uuid,
    is_active boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT "PK_departments" PRIMARY KEY (id)
);

CREATE TABLE users (
    id uuid NOT NULL,
    full_name character varying(150) NOT NULL,
    email character varying(255) NOT NULL,
    password_hash text NOT NULL,
    role character varying(30) NOT NULL,
    department_id uuid,
    is_active boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT "PK_users" PRIMARY KEY (id),
    CONSTRAINT "FK_users_departments_department_id" FOREIGN KEY (department_id) REFERENCES departments (id) ON DELETE SET NULL
);

CREATE TABLE expenses (
    id uuid NOT NULL,
    title character varying(200) NOT NULL,
    description text,
    amount numeric(18,2) NOT NULL,
    category character varying(100) NOT NULL,
    department_id uuid NOT NULL,
    budget_id uuid,
    submitted_by_id uuid NOT NULL,
    status character varying(30) NOT NULL,
    receipt_url text,
    submitted_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT "PK_expenses" PRIMARY KEY (id),
    CONSTRAINT "FK_expenses_budgets_budget_id" FOREIGN KEY (budget_id) REFERENCES budgets (id) ON DELETE SET NULL,
    CONSTRAINT "FK_expenses_departments_department_id" FOREIGN KEY (department_id) REFERENCES departments (id) ON DELETE CASCADE,
    CONSTRAINT "FK_expenses_users_submitted_by_id" FOREIGN KEY (submitted_by_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE TABLE expense_approvals (
    id uuid NOT NULL,
    expense_id uuid NOT NULL,
    approver_id uuid NOT NULL,
    approver_role character varying(30) NOT NULL,
    action character varying(20) NOT NULL,
    comments text,
    action_date timestamp with time zone NOT NULL,
    CONSTRAINT "PK_expense_approvals" PRIMARY KEY (id),
    CONSTRAINT "FK_expense_approvals_expenses_expense_id" FOREIGN KEY (expense_id) REFERENCES expenses (id) ON DELETE CASCADE,
    CONSTRAINT "FK_expense_approvals_users_approver_id" FOREIGN KEY (approver_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE INDEX "IX_alerts_budget_id" ON alerts (budget_id);

CREATE INDEX "IX_alerts_department_id" ON alerts (department_id);

CREATE INDEX "IX_budgets_created_by_id" ON budgets (created_by_id);

CREATE UNIQUE INDEX "IX_budgets_department_id_fiscal_year" ON budgets (department_id, fiscal_year);

CREATE UNIQUE INDEX "IX_departments_code" ON departments (code);

CREATE INDEX "IX_departments_manager_id" ON departments (manager_id);

CREATE UNIQUE INDEX "IX_departments_name" ON departments (name);

CREATE INDEX "IX_expense_approvals_approver_id" ON expense_approvals (approver_id);

CREATE INDEX "IX_expense_approvals_expense_id" ON expense_approvals (expense_id);

CREATE UNIQUE INDEX "IX_expense_categories_name" ON expense_categories (name);

CREATE INDEX "IX_expenses_budget_id" ON expenses (budget_id);

CREATE INDEX "IX_expenses_department_id" ON expenses (department_id);

CREATE INDEX "IX_expenses_submitted_by_id" ON expenses (submitted_by_id);

CREATE INDEX "IX_users_department_id" ON users (department_id);

CREATE UNIQUE INDEX "IX_users_email" ON users (email);

ALTER TABLE alerts ADD CONSTRAINT "FK_alerts_budgets_budget_id" FOREIGN KEY (budget_id) REFERENCES budgets (id) ON DELETE CASCADE;

ALTER TABLE alerts ADD CONSTRAINT "FK_alerts_departments_department_id" FOREIGN KEY (department_id) REFERENCES departments (id) ON DELETE CASCADE;

ALTER TABLE budgets ADD CONSTRAINT "FK_budgets_departments_department_id" FOREIGN KEY (department_id) REFERENCES departments (id) ON DELETE CASCADE;

ALTER TABLE budgets ADD CONSTRAINT "FK_budgets_users_created_by_id" FOREIGN KEY (created_by_id) REFERENCES users (id) ON DELETE CASCADE;

ALTER TABLE departments ADD CONSTRAINT "FK_departments_users_manager_id" FOREIGN KEY (manager_id) REFERENCES users (id) ON DELETE SET NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260224123915_InitialCreate', '8.0.11');

COMMIT;

START TRANSACTION;

ALTER TABLE users ALTER COLUMN updated_at TYPE timestamp without time zone;

ALTER TABLE users ALTER COLUMN department_id TYPE integer;

ALTER TABLE users ALTER COLUMN created_at TYPE timestamp without time zone;

ALTER TABLE users ALTER COLUMN id TYPE integer;
ALTER TABLE users ALTER COLUMN id DROP DEFAULT;
ALTER TABLE users ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY;

ALTER TABLE expenses ALTER COLUMN updated_at TYPE timestamp without time zone;

ALTER TABLE expenses ALTER COLUMN title TYPE character varying(150);

ALTER TABLE expenses ALTER COLUMN submitted_by_id TYPE integer;

ALTER TABLE expenses ALTER COLUMN submitted_at TYPE timestamp without time zone;

ALTER TABLE expenses ALTER COLUMN department_id TYPE integer;

ALTER TABLE expenses ALTER COLUMN category TYPE character varying(50);

ALTER TABLE expenses ALTER COLUMN budget_id TYPE integer;

ALTER TABLE expenses ALTER COLUMN id TYPE integer;
ALTER TABLE expenses ALTER COLUMN id DROP DEFAULT;
ALTER TABLE expenses ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY;

ALTER TABLE expense_categories ALTER COLUMN created_at TYPE timestamp without time zone;

ALTER TABLE expense_categories ALTER COLUMN id TYPE integer;
ALTER TABLE expense_categories ALTER COLUMN id DROP DEFAULT;
ALTER TABLE expense_categories ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY;

ALTER TABLE expense_approvals ALTER COLUMN expense_id TYPE integer;

ALTER TABLE expense_approvals ALTER COLUMN approver_id TYPE integer;

ALTER TABLE expense_approvals ALTER COLUMN action_date TYPE timestamp without time zone;

ALTER TABLE expense_approvals ALTER COLUMN id TYPE integer;
ALTER TABLE expense_approvals ALTER COLUMN id DROP DEFAULT;
ALTER TABLE expense_approvals ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY;

ALTER TABLE departments ALTER COLUMN updated_at TYPE timestamp without time zone;

ALTER TABLE departments ALTER COLUMN manager_id TYPE integer;

ALTER TABLE departments ALTER COLUMN created_at TYPE timestamp without time zone;

ALTER TABLE departments ALTER COLUMN id TYPE integer;
ALTER TABLE departments ALTER COLUMN id DROP DEFAULT;
ALTER TABLE departments ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY;

ALTER TABLE budgets ALTER COLUMN updated_at TYPE timestamp without time zone;

ALTER TABLE budgets ALTER COLUMN period_start TYPE timestamp without time zone;

ALTER TABLE budgets ALTER COLUMN period_end TYPE timestamp without time zone;

ALTER TABLE budgets ALTER COLUMN department_id TYPE integer;

ALTER TABLE budgets ALTER COLUMN created_by_id TYPE integer;

ALTER TABLE budgets ALTER COLUMN created_at TYPE timestamp without time zone;

ALTER TABLE budgets ALTER COLUMN id TYPE integer;
ALTER TABLE budgets ALTER COLUMN id DROP DEFAULT;
ALTER TABLE budgets ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY;

ALTER TABLE alerts ALTER COLUMN department_id TYPE integer;

ALTER TABLE alerts ALTER COLUMN created_at TYPE timestamp without time zone;

ALTER TABLE alerts ALTER COLUMN budget_id TYPE integer;

ALTER TABLE alerts ALTER COLUMN id TYPE integer;
ALTER TABLE alerts ALTER COLUMN id DROP DEFAULT;
ALTER TABLE alerts ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260224150631_ChangeIdTypesToInt_Final', '8.0.11');

COMMIT;

START TRANSACTION;

CREATE TABLE expense_categories (
    id integer GENERATED BY DEFAULT AS IDENTITY,
    name character varying(100) NOT NULL,
    description text,
    is_active boolean NOT NULL,
    created_at timestamp without time zone NOT NULL,
    CONSTRAINT "PK_expense_categories" PRIMARY KEY (id)
);

CREATE TABLE alerts (
    id integer GENERATED BY DEFAULT AS IDENTITY,
    budget_id integer NOT NULL,
    department_id integer NOT NULL,
    severity character varying(20) NOT NULL,
    message text NOT NULL,
    utilization_percent numeric(5,2) NOT NULL,
    is_read boolean NOT NULL,
    created_at timestamp without time zone NOT NULL,
    CONSTRAINT "PK_alerts" PRIMARY KEY (id)
);

CREATE TABLE budgets (
    id integer GENERATED BY DEFAULT AS IDENTITY,
    department_id integer NOT NULL,
    fiscal_year character varying(10) NOT NULL,
    period_start timestamp without time zone NOT NULL,
    period_end timestamp without time zone NOT NULL,
    allocated_amount numeric(18,2) NOT NULL,
    spent_amount numeric(18,2) NOT NULL,
    warning_threshold_pct numeric(5,2) NOT NULL,
    critical_threshold_pct numeric(5,2) NOT NULL,
    status character varying(20) NOT NULL,
    created_by_id integer NOT NULL,
    created_at timestamp without time zone NOT NULL,
    updated_at timestamp without time zone NOT NULL,
    CONSTRAINT "PK_budgets" PRIMARY KEY (id)
);

CREATE TABLE departments (
    id integer GENERATED BY DEFAULT AS IDENTITY,
    name character varying(100) NOT NULL,
    code character varying(20) NOT NULL,
    description text,
    manager_id integer,
    is_active boolean NOT NULL,
    created_at timestamp without time zone NOT NULL,
    updated_at timestamp without time zone NOT NULL,
    CONSTRAINT "PK_departments" PRIMARY KEY (id)
);

CREATE TABLE users (
    id integer GENERATED BY DEFAULT AS IDENTITY,
    full_name character varying(150) NOT NULL,
    email character varying(255) NOT NULL,
    password_hash text NOT NULL,
    role character varying(30) NOT NULL,
    department_id integer,
    is_active boolean NOT NULL,
    created_at timestamp without time zone NOT NULL,
    updated_at timestamp without time zone NOT NULL,
    CONSTRAINT "PK_users" PRIMARY KEY (id),
    CONSTRAINT "FK_users_departments_department_id" FOREIGN KEY (department_id) REFERENCES departments (id) ON DELETE SET NULL
);

CREATE TABLE expenses (
    id integer GENERATED BY DEFAULT AS IDENTITY,
    title character varying(150) NOT NULL,
    description text,
    amount numeric(18,2) NOT NULL,
    category character varying(50) NOT NULL,
    department_id integer NOT NULL,
    budget_id integer,
    submitted_by_id integer NOT NULL,
    status character varying(30) NOT NULL,
    receipt_url text,
    submitted_at timestamp without time zone NOT NULL,
    updated_at timestamp without time zone NOT NULL,
    CONSTRAINT "PK_expenses" PRIMARY KEY (id),
    CONSTRAINT "FK_expenses_budgets_budget_id" FOREIGN KEY (budget_id) REFERENCES budgets (id) ON DELETE SET NULL,
    CONSTRAINT "FK_expenses_departments_department_id" FOREIGN KEY (department_id) REFERENCES departments (id) ON DELETE CASCADE,
    CONSTRAINT "FK_expenses_users_submitted_by_id" FOREIGN KEY (submitted_by_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE TABLE expense_approvals (
    id integer GENERATED BY DEFAULT AS IDENTITY,
    expense_id integer NOT NULL,
    approver_id integer NOT NULL,
    approver_role character varying(30) NOT NULL,
    action character varying(20) NOT NULL,
    comments text,
    action_date timestamp without time zone NOT NULL,
    CONSTRAINT "PK_expense_approvals" PRIMARY KEY (id),
    CONSTRAINT "FK_expense_approvals_expenses_expense_id" FOREIGN KEY (expense_id) REFERENCES expenses (id) ON DELETE CASCADE,
    CONSTRAINT "FK_expense_approvals_users_approver_id" FOREIGN KEY (approver_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE INDEX "IX_alerts_budget_id" ON alerts (budget_id);

CREATE INDEX "IX_alerts_department_id" ON alerts (department_id);

CREATE INDEX "IX_budgets_created_by_id" ON budgets (created_by_id);

CREATE UNIQUE INDEX "IX_budgets_department_id_fiscal_year" ON budgets (department_id, fiscal_year);

CREATE UNIQUE INDEX "IX_departments_code" ON departments (code);

CREATE INDEX "IX_departments_manager_id" ON departments (manager_id);

CREATE UNIQUE INDEX "IX_departments_name" ON departments (name);

CREATE INDEX "IX_expense_approvals_approver_id" ON expense_approvals (approver_id);

CREATE INDEX "IX_expense_approvals_expense_id" ON expense_approvals (expense_id);

CREATE UNIQUE INDEX "IX_expense_categories_name" ON expense_categories (name);

CREATE INDEX "IX_expenses_budget_id" ON expenses (budget_id);

CREATE INDEX "IX_expenses_department_id" ON expenses (department_id);

CREATE INDEX "IX_expenses_submitted_by_id" ON expenses (submitted_by_id);

CREATE INDEX "IX_users_department_id" ON users (department_id);

CREATE UNIQUE INDEX "IX_users_email" ON users (email);

ALTER TABLE alerts ADD CONSTRAINT "FK_alerts_budgets_budget_id" FOREIGN KEY (budget_id) REFERENCES budgets (id) ON DELETE CASCADE;

ALTER TABLE alerts ADD CONSTRAINT "FK_alerts_departments_department_id" FOREIGN KEY (department_id) REFERENCES departments (id) ON DELETE CASCADE;

ALTER TABLE budgets ADD CONSTRAINT "FK_budgets_departments_department_id" FOREIGN KEY (department_id) REFERENCES departments (id) ON DELETE CASCADE;

ALTER TABLE budgets ADD CONSTRAINT "FK_budgets_users_created_by_id" FOREIGN KEY (created_by_id) REFERENCES users (id) ON DELETE CASCADE;

ALTER TABLE departments ADD CONSTRAINT "FK_departments_users_manager_id" FOREIGN KEY (manager_id) REFERENCES users (id) ON DELETE SET NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260224151045_InitialIntMigration', '8.0.11');

COMMIT;


