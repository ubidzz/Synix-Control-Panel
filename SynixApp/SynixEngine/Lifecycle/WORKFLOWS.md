# Server workflow safety

## Before starting

A requested startup or scheduled-maintenance backup must finish successfully before an update or server launch can proceed. A requested update must also succeed before launch. Failures leave the server stopped and explain which step needs attention. Crash recovery retains its existing policy of skipping pre-start backups.

Backup creation and server update/validation return success explicitly. Callers must not treat a completed asynchronous call as proof that its operation succeeded. Manual backup failure is shown in both the dashboard and Minecraft Control Center.

## Backup destinations and retention

An unavailable custom backup folder is an error, not permission to silently switch to the default location. Reconnect the selected destination or choose another folder in settings.

The new archive and integrity receipt are published before retention runs. Cleanup never selects the just-created archive, even if an older archive has a future timestamp. A locked old archive produces a warning; it does not remove the new backup. Incomplete creation still removes partial files and preserves existing backups.

## Restore completion

All full-restore entry points use the same completion path. After activating the backup files, Synix synchronizes shared server settings using the game's existing configuration mappings. Minecraft also refreshes its recorded runtime and change history. Unknown keys are not guessed, and reading restored settings does not rewrite the configuration files.

The restore journal records whether settings synchronization remains pending. Until synchronization succeeds, the previous installation and recovery record are retained. A settings failure is reported as a partial completion: the backup files are restored, but Synix's settings are not yet synchronized. This is not an automatic rollback of the restored files.

After correcting the reported issue, retrying Start retries synchronization before launch validation, updates, or startup. Restarting Synix preserves pending settings completion rather than silently deleting its recovery files. An interrupted file-activation phase still follows the existing original-folder recovery path.

Regression coverage includes startup ordering, blocked/failed backups, locked retention files, unavailable custom destinations, both ARK configuration mappings, Minecraft player-limit refresh, failed settings persistence, and recovery retry after an interruption.
