# Steam dedicated-server download audit

Reviewed: 2026-09-29. Scope: all 227 original built-in profiles and 217 distinct positive Steam AppIDs. The corrected catalog contains **220 profiles** (218 Steam-based and two non-Steam definitions).

This is a download-metadata audit, not a claim that every game has been installed and played. SteamDB app identity was checked for each Steam entry, with public depot/branch contents inspected where available. Some pages expose incomplete or cached metadata. Steam login, ownership, discontinued services and runtime compatibility still require installation testing. Existing historical verification records have not been relabeled as current verification.

The machine-readable [complete audit](steam-download-audit.json) records every original profile, old/current ID, decision and evidence links. Regression tests require an audit entry for every active profile and check that removed IDs do not reappear accidentally.

## Corrected downloads and selectors

- **Terraria:** 105610 → **105600**. The old server Tool is a 2012 package. The current dedicated executable, `TerrariaServer.exe`, is bundled with the owned game download. Steam login/ownership is required. [Old package](https://steamdb.info/app/105610/depots/), [current package](https://steamdb.info/app/105600/depots/), [official server guide](https://terraria.wiki.gg/wiki/Guide:Setting_up_a_Terraria_server).
- **Stormworks:** 1247090 → **573090**, requiring Steam login/ownership. The separate Tool's public content consists of two 13 KiB launchers; the developer documents running `server64.exe` from the game installation. [Tool content](https://steamdb.info/depot/1247091/), [current game package](https://steamdb.info/app/573090/depots/), [developer guide](https://steamcommunity.com/sharedfiles/filedetails/?id=1995371855).
- **The Isle:** both profiles correctly use **412680**. Legacy now explicitly selects `public`; Evrima selects `evrima`. Legacy's map list no longer offers Evrima maps. [SteamDB branches](https://steamdb.info/app/412680/depots/).
- **GoldSrc:** all eight profiles correctly use **90**. Each supplies its game-specific `90 mod ...` setting before the update. Half-Life now explicitly selects `valve` rather than inheriting a previously used game's setting. [SteamDB shared game depots](https://steamdb.info/app/90/depots/).

A Steam AppID identifies the download package, not a unique Synix game. Do not replace a shared server ID with a retail client ID simply to make the column unique. Bundled dedicated executables are an explicitly documented exception.

## Removed from the Windows installer catalog

These removals do **not** mean the games have no dedicated servers on any platform. They mean the configured public download does not provide the Windows dedicated-server installation offered by this profile. Restricted provider downloads and Linux-only servers are not supported by this Windows installer. No installed folders, saved servers, worlds or backups are deleted. Removed source definitions remain recoverable in Git history.

| Removed profile | Previous download | Reason |
| --- | --- | --- |
| Battle Splash 2.0 | [761080](https://steamdb.info/app/761080/depots/) | The public server content is Linux-only (depots 761082/761083); Windows redistributables are not a Windows game server. |
| BattleBit Remastered | [689410](https://steamdb.info/app/689410/depots/) | App 689410 currently references an old Playtest depot, not the configured BattleBitDedicated.exe. A working public dedicated-server download for this profile was not established. |
| Deadside | [1275530](https://steamdb.info/app/1275530/depots/) | The public server depot contains only a 6-byte placeholder, with server.txt configured as the launch file. |
| DUCKSIDE | [2690320](https://steamdb.info/app/2690320/depots/) | The public Windows server depot contains only an 18-byte placeholder; restricted provider builds are not a supported public download. |
| Pavlov VR | [622970](https://steamdb.info/app/622970/depots/) | The dedicated-server content depot 622971 is Linux-only; the configured Windows server executable is not supplied. |
| Quake Live | [349090](https://steamdb.info/app/349090/depots/) | The dedicated-server content depot 349091 is Linux-only; the configured Windows server executable is not supplied. |
| Risk of Rain 2 | [1180760](https://steamdb.info/app/1180760/depots/) | The public Windows dedicated-server depot 1180761 is empty (0 bytes). |

## Distribution and test limits

- BeamMP and Minecraft remain because they have official dedicated servers outside Steam. AppID `0` is a non-Steam marker, not a SteamCMD download request.
- A `Tool` label alone is insufficient evidence; a `Game` label alone does not rule out a bundled dedicated server.
- No historical manifest is pinned and no private branch or entitlement is bypassed.
- Catalog removals preserve saved server records, but a removed profile is no longer available for creating/installing a server and its previous built-in management behavior is not guaranteed.
- These changes require a newly built Synix executable. The already-published v1.0.25 installer is not changed by editing the source or wiki.
