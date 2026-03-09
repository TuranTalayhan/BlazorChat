-- Development seed data for Postgres (development only).
-- Safe to run multiple times: uses ON CONFLICT DO NOTHING.
-- Run AFTER EF Core migrations have been applied. Do NOT use in production.

BEGIN;

-- Users (Status: 0=Offline, 1=Online, 2=Idle, 3=DoNotDisturb)
INSERT INTO "Users" ("Id", "Email", "Username", "PasswordHash", "Status") VALUES
  (1, 'alice@example.com', 'alice', 'devpassword', 1),
  (2, 'bob@example.com',   'bob',   'devpassword', 1),
  (3, 'carol@example.com', 'carol', 'devpassword', 0)
ON CONFLICT ("Id") DO NOTHING;

SELECT setval(pg_get_serial_sequence('"Users"', 'Id'), GREATEST((SELECT MAX("Id") FROM "Users"), 3));

-- Friendships
INSERT INTO "Friendships" ("RequesterId", "ReceiverId", "Status", "CreatedAt") VALUES
  (1, 2, 1, now()),
  (1, 3, 0, now())
ON CONFLICT ("RequesterId", "ReceiverId") DO NOTHING;

-- Messages
INSERT INTO "Messages" ("Id", "Content", "CreatedAt", "AuthorId", "ChannelId") VALUES
  (1, 'Welcome to the dev server!',  now(), 1, 1),
  (2, 'Hi Alice — glad to be here!', now(), 2, 1)
ON CONFLICT ("Id") DO NOTHING;

SELECT setval(pg_get_serial_sequence('"Messages"', 'Id'), GREATEST((SELECT MAX("Id") FROM "Messages"), 2));

COMMIT;
