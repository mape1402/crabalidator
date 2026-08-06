# Release Process

Crabalidator releases are published from dedicated release branches.

## Production Release

1. Start from an updated `main` branch.

```bash
git checkout main
git pull --ff-only origin main
```

2. Ensure `CHANGELOG.md` contains a versioned section for the release tag.

For version `1.0.1`, the section must start with:

```markdown
## [v1.0.1] - YYYY-MM-DD
```

3. Create and push the release branch.

```bash
git checkout -b releases/v1.0.1
git push -u origin releases/v1.0.1
```

4. Ensure repository publishing configuration exists.

- GitHub repository variable `NUGET_USER` must match the nuget.org user configured in the Trusted Publishing policy.
- nuget.org Trusted Publishing must point to this repository and the release workflow.

5. Run the `Release to NuGet` workflow manually from the release branch.

6. Verify the result.

- GitHub release was created for the tag.
- `.nupkg` and `.snupkg` files are attached to the GitHub release.
- NuGet lists the expected `Crabalidator` package version.

## Notes

- Do not put release-branch or Trusted Publishing instructions in the public NuGet README.
- Keep public package documentation focused on users of the library.
- Keep internal release mechanics in this file.
