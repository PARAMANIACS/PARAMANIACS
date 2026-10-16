# Contributing to PARAMANIACS

Thanks for wanting to make architects smile! 🎉
This project is for fun, so contributing should be fun too.

## Ways to contribute

- 💡 **Suggest a crazy idea** in [Discussions](../../discussions) or open a *New component idea* issue.
- 🐛 **Report a bug** using the bug report template.
- 🧩 **Build a component** (game, random event, weird command…).
- 📝 **Improve documentation** or fix typos.

## How the workflow works

Nobody pushes directly to `main`. All changes go through a **Pull Request (PR)** that is reviewed by at least one maintainer.

1. **Fork** the repository to your own GitHub account.
2. **Clone** your fork:
   ```bash
   git clone https://github.com/<your-username>/paramaniacs.git
   cd paramaniacs
   ```
3. **Create a branch** with a short, descriptive name:
   ```bash
   git checkout -b feature/tic-tac-toe-component
   ```
4. **Make your changes** and test them inside Rhino/Grasshopper.
5. **Commit** with a clear message (see below) and **push** to your fork.
6. Open a **Pull Request** against `main` and fill in the template.
7. A maintainer will review it. Please be patient and respond to comments — reviews are a conversation, not a judgment.
8. Once approved, a maintainer will **squash and merge** it.

Keep PRs small and focused. One component or one fix per PR is ideal.

## Commit messages

We loosely follow [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: add Tic-Tac-Toe two-player component
fix: prevent crash when canvas is empty
docs: clarify installation steps
chore: update .gitignore
```

## Safety rules

PARAMANIACS must be **fun and harmless**. Users must be able to trust every component. A PR that breaks any of these rules will not be merged.

1. **Stay inside Rhino/Grasshopper.** Components may affect the Rhino viewport, the Grasshopper canvas, and their own windows. They must **not** change Windows settings, the registry, other applications, or system files, and must not kill or start other processes.
2. **Never destroy the user's work.** Don't delete or modify existing geometry, definitions or files unless the user explicitly asked. Anything that changes the document must be undoable (use Rhino's undo records).
3. **No hidden network access.** No telemetry, no downloading or uploading data, no calls home. If a feature truly needs the network (e.g. online multiplayer), it must be discussed in an issue first, clearly disclosed in the component description, and opt-in.
4. **Opt-in only.** Surprises happen only when the user places or runs the component. Nothing runs automatically at startup or in the background.
5. **Always stoppable.** Games, loops and random events must have an obvious way to stop (a toggle, a button or the Escape key).
6. **Readable code only.** No obfuscated code, no compiled binaries or `.dll` files committed to the repository, no encoded blobs.
7. **Review new dependencies.** Don't add NuGet packages or external libraries without discussing it in an issue first.
8. **Be kind.** No offensive, discriminatory or shocking content, even as a joke.

If your component is a prank or a "random event", add the label `needs-safety-review` and describe exactly what it does in the PR.

## Code guidelines

- Language: **C#**, targeting the .NET version required by the supported Rhino version.
- Use **RhinoCommon** and the **Grasshopper SDK**.
- One component per file, with a clear name and a short description visible in Grasshopper's tooltip.
- Add a short comment at the top of each component explaining what it does.
- Prefer simple, readable code over clever code.
- Keep the code free of compiler warnings where possible.

(Detailed project structure and build instructions will be added once the codebase is set up.)

## Licensing of contributions

By submitting a contribution, you agree that it is licensed under the project's [MIT License](LICENSE), and that you have the right to submit it (it is your own work, or you have permission to share it). Don't submit code you copied from somewhere with an incompatible license.

## Code review guidelines (for maintainers)

- Check the [safety rules](#safety-rules) first.
- Build and try the component in Rhino before approving.
- Be friendly and constructive, especially with first-time contributors.
- Authors may not approve their own PRs.

## Need help?

Ask in [Discussions](../../discussions). No question is too small.
