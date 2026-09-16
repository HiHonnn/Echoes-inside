# Asset Publication Audit

Complete this audit before making the repository public.

| Area | Approximate local size | Repository status | Required action |
|---|---:|---|---|
| `Assets/_EchoesInside/Music` | 1.7 GB | Excluded by `.gitignore` | Verify the license and attribution requirements for every audio pack before redistributing any file. |
| `Assets/_EchoesInside/Art` | 86 MB | Included | Confirm that the team created each image or has permission to publish it in a public source repository. |
| `Assets/_EchoesInside/Fonts` | Less than 1 MB | Included | Record the font source and license. |
| `Assets/TextMesh Pro` | About 4 MB | Included | Keep bundled license notices, including the Liberation Sans OFL notice. |
| Source code | About 3 MB | Included | Confirm which team members contributed and describe personal ownership accurately in the README or CV. |

## Audio files kept locally

The local project contains compressed source packs and extracted WAV, AIF, OGG, and MP3 files. Several individual archives exceed GitHub's normal 100 MiB file limit. They are excluded both to keep the portfolio repository small and to avoid publishing third-party media without confirmed permission.

If an audio license explicitly permits redistribution:

1. Add only the clips used by the game.
2. Add the required attribution to this document.
3. Remove the narrowest applicable ignore rule.
4. Track large audio files through Git LFS.
5. Verify the repository from a clean clone.

## Attribution record

Fill this table before publishing any third-party asset.

| Asset or pack | Creator/source | License | Changes made | Included in public repository |
|---|---|---|---|---|
| | | | | |
