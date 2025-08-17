CREATE TABLE tasks
(
    id              uuid         NOT NULL PRIMARY KEY,
    title           text         NOT NULL,
    description     text         NOT NULL,
    due_date_utc    timestamptz  NOT NULL,
    status          text         NOT NULL DEFAULT 'New',
    created_at_utc  timestamptz  NOT NULL,
    updated_at_utc  timestamptz  NOT NULL,
    CONSTRAINT tasks_status_chk CHECK (status IN ('New','InProgress','Completed','Overdue'))
);

CREATE INDEX IF NOT EXISTS ix_tasks_status_due_date ON tasks (status, due_date_utc);
CREATE INDEX IF NOT EXISTS ix_tasks_due_date        ON tasks (due_date_utc);
CREATE INDEX IF NOT EXISTS ix_tasks_created_at      ON tasks (created_at_utc);
CREATE INDEX IF NOT EXISTS ix_tasks_updated_at      ON tasks (updated_at_utc);