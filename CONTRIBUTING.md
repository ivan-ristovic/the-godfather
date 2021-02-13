# Contributing to the-godfather
TheGodfather is made for the community and contributions are always welcome! However, here are a few guidelines when contributing.

# Proper titles
When opening issues, make sure the title is brief and that it reflects the purpose of the issue or the pull request. Further description belongs inside the issue or PR.

# Proper base
When opening a PR, please make sure your branch is even with the target branch.

# Descriptive changes
We require the commits describe the change made. It can be a short description. If you fixed or resolved an open issue, 
please reference it by using the # notation.

# Code style
Code style used in this repository is not standard, however it is easy to pick up. Please consult `.editorconfig` file for detailed information. You should auto-format the code based on `.editorconfig` before commiting.

# Code changes
Every commit must build successfully and that is verified by CI. When you open a pull request, CI will start a build and you can view its summary.

PRs that do not build will not be accepted until the issues are resolved.

# Non-code changes
Documentation is automatically generated, so `.md` files should never be changed manually. Strings are located inside the `Translations` directory. If you do make a change that is only affecting the documentation and not how the code works, then tag your commit with `[ci-skip]`, so that the CI will not run the build.