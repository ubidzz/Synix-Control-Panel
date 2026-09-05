# Synix server data migrations

Synix keeps `servers.json` as a JSON array so existing transfer packages and
older installations remain compatible. Each saved server record carries its
own `DataSchemaVersion`.

Migration rules live in `SynixEngine/ServerDataMigrator.cs` and run in order.
They are append-only: do not rewrite or remove an older migration after a
release has shipped. Add a new method, advance `CurrentVersion`, and add a new
`switch` case instead.

Before Synix saves upgraded records, it preserves the original file beside the
live data as `servers.json.before-data-v{version}.bak`. The backup is created
only once for that target version, so a later retry cannot overwrite the last
known legacy data.

Every new migration must include tests for:

- upgrading from the oldest affected schema;
- upgrading through every intermediate version;
- running again without changing current data;
- rejecting data written by a newer unsupported Synix version;
- preserving the pre-migration backup.
