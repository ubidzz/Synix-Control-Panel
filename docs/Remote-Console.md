# Remote game-console commands

1. Update Synix on the paired hosting PC.
2. Enable **Show server console window** in Synix settings.
3. Start or restart a supported game from Synix. Keep its Synix game-console window open.
4. On [Synix Manager](https://manager.synix.info/), choose the computer and server, then **Unlock commands**.
5. Enter the current account password and complete the required authenticator or passkey verification. If the account requires both methods, complete both.
6. Enter one game command and choose **Send command**.

Approval lasts three minutes for that browser, computer, server and game run. Stop, restart, a closed local console, a disabled console setting or a lost process invalidates access. The site also locks when disconnected or hidden. Use **Lock now** to end approval early. Start, Stop, Restart, Back up, Restore backup and Update server do not gain another verification prompt.

The initial supported inputs are Synix-generated Minecraft Java launchers (including supported Forge/NeoForge launchers) and the direct Minecraft Bedrock executable. Synix opens its own console window and starts the game directly with a private input pipe. The window remains usable locally. Arbitrary batch launchers, already-running processes recovered after an app restart, elevated launchers, and games without supported interactive input remain unavailable. Restart an eligible server through the paired Synix app to establish a new pipe.

The stable server ID selects the saved server. An independent random run ID selects one live input connection. PIDs, executable paths and process creation times remain local and are checked against the process that owns the pipe. Neither an incoming PID nor a shared console input buffer can be used as a fallback. An exited game's pipe cannot become a Windows command prompt.

The service binds approval to the current account policy and checks ownership and authorization again when it queues and delivers each command. Pending text is encrypted in storage, expires after 30 seconds and is cleared when delivery is authorized. Delivery is not retried after a missing acknowledgement. Recent actions show only the action and fixed result, never the entered command. Responses remain in the local game console. Game commands, including commands supplied by installed plugins, retain the permissions of that game; this feature does not make untrusted game code safe.
