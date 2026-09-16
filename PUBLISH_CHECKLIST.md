# GitHub Publication Checklist

## Repository review

- [ ] Confirm the repository name, description, and visibility.
- [ ] Review `README.md` and make sure every feature listed is present in the current build.
- [ ] Add a gameplay video link and, if available, a downloadable build link.
- [ ] Add two or three in-game screenshots to the README.
- [ ] Complete `ASSET_AUDIT.md`.
- [ ] Confirm that team contributions and your own responsibilities are described accurately.
- [ ] Decide whether the public repository should include the visual assets.
- [ ] Keep the repository without an open-source license unless the whole team agrees to one.

## Technical checks

- [ ] Open the project from the title screen with Unity 6000.2.9f1.
- [ ] Run the Play Mode test suite.
- [ ] Check the Console for compile errors and missing serialized references.
- [ ] Create a clean clone and verify that the documented setup works without local-only files.
- [ ] Confirm that `Library`, `Logs`, `UserSettings`, local IDE files, and the audio pack directory are ignored.
- [ ] Confirm that no file contains credentials, tokens, private URLs, or personal data.
- [ ] Confirm that no regular Git object exceeds GitHub's 100 MiB limit.

## Publishing commands

After completing the checks:

```powershell
git add .
git commit -m "Initial portfolio release"
git remote add origin https://github.com/<your-username>/echoes-inside.git
git push -u origin main
```

If a remote named `origin` already exists, update it instead:

```powershell
git remote set-url origin https://github.com/<your-username>/echoes-inside.git
```

Do not rewrite or fabricate earlier development history. Continue with clear commits for real changes from this point forward.
