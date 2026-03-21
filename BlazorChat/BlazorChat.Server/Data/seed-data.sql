-- Development seed data for Postgres (development only).
-- Safe to run multiple times: uses ON CONFLICT DO NOTHING.
-- Run AFTER EF Core migrations have been applied. Do NOT use in production.

BEGIN;

-- 1. Users (Status: 0=Offline, 1=Online, 2=Idle, 3=DoNotDisturb)
INSERT INTO "Users" ("Id", "Email", "Username", "PasswordHash", "Status") VALUES
  (1, 'alice@example.com', 'alice', 'devpassword', 1),
  (2, 'bob@example.com',   'bob',   'devpassword', 1),
  (3, 'carol@example.com', 'carol', 'devpassword', 0)
ON CONFLICT ("Id") DO NOTHING;

SELECT setval(pg_get_serial_sequence('"Users"', 'Id'), GREATEST((SELECT COALESCE(MAX("Id"), 0) FROM "Users"), 3));

-- 2. Friendships
INSERT INTO "Friendships" ("RequesterId", "ReceiverId", "Status", "CreatedAt") VALUES
  (1, 2, 1, now()),
  (1, 3, 0, now())
ON CONFLICT ("RequesterId", "ReceiverId") DO NOTHING;

-- 3. Servers
INSERT INTO "Servers" ("Id", "Name", "OwnerId") VALUES
  (1, 'Dev Server', 1)
ON CONFLICT ("Id") DO NOTHING;

SELECT setval(pg_get_serial_sequence('"Servers"', 'Id'), GREATEST((SELECT COALESCE(MAX("Id"), 0) FROM "Servers"), 1));

-- 4. ServerMemberships (Role: 0=Owner, 1=Admin, 2=Member)
INSERT INTO "ServerMemberships" ("ServerId", "UserId", "Role", "JoinedAt") VALUES
  (1, 1, 0, now()),
  (1, 2, 2, now())
ON CONFLICT ("ServerId", "UserId") DO NOTHING;

-- 5. Channels (Type: 0=ServerText, 1=DirectMessage)
INSERT INTO "Channels" ("Id", "Name", "Type", "SortOrder", "ServerId", "CreatedAt") VALUES
  (1, 'general', 0, 0, 1, now()),       -- Server Channel (Belongs to Server 1)
  (2, 'random',  0, 1, 1, now()),       -- Server Channel (Belongs to Server 1)
  (3, NULL,      1, 0, NULL, now())     -- DM Channel (No Name, No Server!)
ON CONFLICT ("Id") DO NOTHING;

SELECT setval(pg_get_serial_sequence('"Channels"', 'Id'), GREATEST((SELECT COALESCE(MAX("Id"), 0) FROM "Channels"), 3));

-- 6. ChannelMembers (This is the EF Core Join Table for DM participants!)
INSERT INTO "ChannelMembers" ("ChannelsId", "MembersId") VALUES
  (3, 1), -- Alice is in the DM (Channel 3)
  (3, 2)  -- Bob is in the DM (Channel 3)
ON CONFLICT ("ChannelsId", "MembersId") DO NOTHING;

-- 7. Messages (Type: 0=Text, 1=System)
INSERT INTO "Messages" ("Id", "Content", "Type", "CreatedAt", "AuthorId", "ChannelId") VALUES
  (1, 'Welcome to the dev server!',    0, now() - interval '1 hour', 1, 1),
  (2, 'Hi Alice — glad to be here!',   0, now() - interval '30 min', 2, 1),
  (3, 'Hey Bob, DM works via SQL too!', 0, now(), 1, 3)
ON CONFLICT ("Id") DO NOTHING;

SELECT setval(pg_get_serial_sequence('"Messages"', 'Id'), GREATEST((SELECT COALESCE(MAX("Id"), 0) FROM "Messages"), 3));

COMMIT;