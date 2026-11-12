# Recovering a Synix manager password

Use any computer whose pairing to your account was already approved in Synix. You do not need to sign in at https://manager.synix.info or enable a separate recovery setting first.

If you forget the website password:

1. On that computer, open **Synix → Settings → Privacy & Security → Remote manager → Recover website password**.
2. Create a temporary password. The app displays the account that owns the pairing; it cannot choose another account.
3. Enter that username and temporary password on the manager website. Complete the existing authenticator or passkey verification. Accounts requiring both still need both.
4. Choose a new permanent password. Your server manager opens immediately without another sign-in.

The temporary password works once and expires within ten minutes. The original password remains valid until recovery completes. Completing recovery signs out other browser sessions and opens one fresh session in the browser where recovery finished. Computer pairings, verification methods and game servers are preserved. The app can cancel the temporary password. Unpairing, changing account security, or signing out everywhere also invalidates pending recovery.

The computer must already be paired to your account. Recovery does not recover missing authenticators/passkeys or replace them. If no approved paired computer or required verification method is available, this flow cannot recover the account.

## Computer identity and boundaries

Requests use the pairing's signing key and bind the destination, computer identity, action, body, timestamp and one-use nonce. The server obtains the account from its saved pairing and checks it again when a password reset completes. Recovery tokens are stored as hashes, cannot authorize server actions and are never placed in URLs.

The website accepts only version 2 computer proofs on its official manager address. Requests from browser contexts, unsupported routes, modified URLs, reused proofs and excessive request traffic are rejected. Registration accepts a canonical public RSA key only, so alternative encodings cannot register the same computer identity twice.

Updated app builds restrict wake connections to Synix's exact relay address and prove possession of the pairing key during the WebSocket handshake. Connection credentials can be used once, within one minute; an established connection can last up to one hour. Unpairing invalidates unused relay credentials. The wake channel carries only connection notifications; server commands still require separate signed requests to the manager. Older app builds retain a short-lived, single-use bearer handshake until updated.

Deploy the updated relay and manager website before distributing the updated app. The app requires the website's key-bound relay response and checks explicit command approval against the command ID, server, action, deadline, backup and console run before execution. A missing or mismatched approval fails closed.

Synix prefers a non-exportable TPM key when Windows supports it and otherwise uses a Windows-protected software key. Recovery supports approved PCs and VMs without mandatory TPM attestation. The displayed key-provider status is not remotely verified hardware attestation. A compromised computer can misuse its own pairing; this design does not guarantee protection against a compromised operating system.
