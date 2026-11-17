# Security Policy

## Security Architecture & Model

AXIS operates directly on Windows system configurations, kernel scheduling parameters, and registry keys. Because configuring low-level operating system parameters requires elevated administrative privileges, AXIS adheres to a strict defensive engineering standard:

1. **Explicit Elevation**: AXIS requests standard Windows UAC elevation only when necessary and validates administrative tokens via `WindowsIdentity.GetCurrent()`.
2. **Defensive Registry Operations**: All registry modifications use isolated transactions and snapshot recording before write operations.
3. **No Network Telemetry / Zero External Dependencies**: AXIS does not transmit any system data, hardware IDs, or tweak configurations over the network. Network operations are strictly limited to user-requested ICMP ping diagnostics to user-selected target hosts.
4. **Preservation of Core Security Controls**: AXIS by default excludes high-risk modifications to Windows Defender real-time protection, Windows Firewall, and User Account Control (UAC).

## Supported Versions

| Version | Supported | Security Updates |
| :--- | :---: | :--- |
| **2.0.x** | Yes | Active maintenance and security patches |
| < 2.0 | No | End of life; upgrade to v2.0+ recommended |

## Reporting a Security Vulnerability

If you discover a potential security issue (such as privilege escalation, unsanitized script execution, or registry integrity corruption):

1. **Do not create a public GitHub issue.**
2. Send an email to the security maintainers or open a [Private Vulnerability Advisory](https://github.com/Vektor010/axis/security/advisories/new) via GitHub.
3. Provide detailed steps to reproduce the issue, your Windows OS build, and the affected tweak ID or subsystem.

We will review your submission and release a patch in an expedited maintenance release.
