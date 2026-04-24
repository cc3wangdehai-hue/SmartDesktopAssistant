# Smart Desktop Assistant - GitHub Actions CI/CD Setup

## Quick Start

1. **Create GitHub Repository**
   ```bash
   # Initialize git if not already
   git init
   
   # Add remote
   git remote add origin https://github.com/YOUR_USERNAME/SmartDesktopAssistant.git
   ```

2. **Push to GitHub**
   ```bash
   git add .
   git commit -m "Add project with CI/CD"
   git branch -M main
   git push -u origin main
   ```

3. **View Build Results**
   - Go to your repository on GitHub
   - Click "Actions" tab
   - View the build workflow results

## Workflow Features

- **Auto-trigger**: Runs on every push to main/master branch
- **Manual trigger**: Can be manually triggered from Actions tab
- **Windows environment**: Uses windows-latest runner
- **Artifacts**: Build outputs are saved for 7 days

## Build Output

After successful build:
1. Go to Actions → Select the workflow run
2. Scroll down to "Artifacts" section
3. Download `SmartDesktopAssistant-build.zip`
4. Extract and run on your Windows machine

## Local Testing

Before pushing, you can test locally:
```bash
dotnet build SmartDesktopAssistant.sln --configuration Release
```

## Troubleshooting

If build fails:
1. Check the Actions log for error details
2. Ensure all NuGet packages are properly referenced
3. Verify .NET 8.0 SDK compatibility
