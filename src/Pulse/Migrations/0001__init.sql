DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'task_status') THEN
        CREATE TYPE task_status AS ENUM ('New', 'InProgress', 'Completed', 'Overdue');
    END IF;
END$$;

CREATE TABLE IF NOT EXISTS tasks
(
    id              uuid           NOT NULL PRIMARY KEY,
    title           text           NOT NULL,
    description     text           NOT NULL,
    due_date_utc    timestamptz    NOT NULL,
    status          task_status    NOT NULL DEFAULT 'New',
    created_at_utc  timestamptz    NOT NULL,
    updated_at_utc  timestamptz    NOT NULL
    );

CREATE INDEX IF NOT EXISTS ix_tasks_status_due_date ON tasks (status, due_date_utc);
CREATE INDEX IF NOT EXISTS ix_tasks_due_date        ON tasks (due_date_utc);
CREATE INDEX IF NOT EXISTS ix_tasks_created_at      ON tasks (created_at_utc);
CREATE INDEX IF NOT EXISTS ix_tasks_updated_at      ON tasks (updated_at_utc);