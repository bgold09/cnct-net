# NuGet.org Publishing Setup Instructions

## Required GitHub Secret Configuration

To enable publishing to NuGet.org, you need to set up a NuGet API key as a GitHub repository secret.

### Steps to Configure NuGet.org API Key

1. **Create a NuGet.org Account** (if you don't have one)
   - Go to https://www.nuget.org/
   - Sign up for an account or sign in

2. **Generate an API Key**
   - Go to https://www.nuget.org/account/apikeys
   - Click "Create" to generate a new API key
   - Configure the key with:
     - **Key Name**: Choose a descriptive name (e.g., "GitHub Actions - cnct-net")
     - **Package Owner**: Select your account
     - **Scopes**: Select "Push new packages and package versions"
     - **Packages**: Select "All packages" or specify "Cnct" if you want to limit it
     - **Glob Pattern**: Use `*` for all packages or `Cnct*` for this specific package

3. **Add the API Key to GitHub Secrets**
   - Go to your GitHub repository: https://github.com/bgold09/cnct-net
   - Navigate to Settings → Secrets and variables → Actions
   - Click "New repository secret"
   - Set:
     - **Name**: `NUGET_API_KEY`
     - **Secret**: Paste the API key you generated from NuGet.org
   - Click "Add secret"

### Verification

Once the secret is configured, your GitHub Actions workflow will be able to publish packages to NuGet.org when:
- Code is pushed to the `main` branch
- The build and tests pass successfully
- The workflow runs on `ubuntu-latest`

The package will be available at: https://www.nuget.org/packages/Cnct/

### Installation Command

Users will be able to install your tool using:
```bash
dotnet tool install -g Cnct
```

## Security Notes

- Keep your NuGet API key secure and never commit it to your repository
- The API key should only be stored in GitHub Secrets
- Consider using a dedicated API key for CI/CD that has minimal required permissions
- You can revoke and regenerate API keys at any time from your NuGet.org account
