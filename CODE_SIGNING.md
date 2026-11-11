# Code signing policy

## Current status

BASIC EDU is unsigned. No signing service has approved this project and no certificate
has been assigned. A successful build or CI artifact is not a signed release.
Windows Smart App Control can block unsigned EXE and DLL files.

We intend to apply to [SignPath Foundation](https://signpath.org/) after publishing
the source, documentation and an initial release. Acceptance requires a verifiable
project reputation and is discretionary, not guaranteed.

## Proposed responsibilities

Petr, the project initiator, is the proposed maintainer, reviewer and release approver.
His public GitHub identity and repository address must be confirmed before publication.
These are proposed roles, not an existing SignPath configuration. Contributions from
other authors, including build-script changes, require maintainer review.
Every signing request needs explicit maintainer approval. Repository and signing
accounts must use multi-factor authentication before applying.

## Future signing scope

Only our files built by an auditable workflow from reviewed source are candidates:
BasicEdu.exe, BasicEdu.dll and BasicEdu.Core.dll. Runtime and upstream libraries,
including AvalonEdit, retain their original signatures and licenses and must not
be signed under this project's SignPath subscription. Product metadata must identify
BASIC EDU; first-party files must share a version. Any blocked upstream DLL needs
its own supported signing solution; signing our files does not guarantee all dependencies run.

After approval, this page and release notes will include the required SignPath attribution
and confirmed public roles. We do not claim active sponsorship today.
The workflow currently builds unsigned artifacts and never publishes releases automatically.

## Privacy

The application does not transmit information to networked systems on its own.
See [PRIVACY.md](PRIVACY.md). Build services have separate privacy policies.

Reference: https://signpath.org/terms.html (reviewed 2026-11-11).
