# CI/CD Integration Examples

This folder contains examples for integrating Locale into your CI/CD pipelines.

## GitHub Actions

### Example 1: Validate Translations on PR

Create `.github/workflows/locale-check.yml`:

```yaml
name: Validate Translations

on:
  pull_request:
    branches: [main, develop]
    paths:
      - 'locales/**'
      - 'src/**/*.resx'

jobs:
  validate:
    name: Check Translation Quality
    runs-on: ubuntu-latest
    
    steps:
      - name: 📥 Checkout
        uses: actions/checkout@v4
      
      - name: 📦 Install Locale CLI
        run: npm install -g @taiizor/locale-cli
      
      - name: ✅ Check for empty values
        run: |
          locale check ./locales \
            --rules no-empty-values,no-duplicate-keys \
            --ci
      
      - name: 🔍 Scan for missing translations
        run: |
          locale scan ./locales \
            --base en \
            --targets tr,de,fr \
            --output scan-report.json
      
      - name: 📊 Upload scan report
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: translation-scan-report
          path: scan-report.json
      
      - name: 💬 Comment on PR
        if: failure()
        uses: actions/github-script@v7
        with:
          script: |
            const fs = require('fs');
            const report = JSON.parse(fs.readFileSync('scan-report.json', 'utf8'));
            
            let comment = '## ⚠️ Translation Issues Detected\\n\\n';
            
            for (const result of report.results) {
              if (result.missingKeys.length > 0) {
                comment += `### ${result.targetCulture}\\n`;
                comment += `- Missing keys: ${result.missingKeys.length}\\n`;
                comment += `- Orphan keys: ${result.orphanKeys.length}\\n`;
                comment += `- Empty values: ${result.emptyValues.length}\\n\\n`;
              }
            }
            
            github.rest.issues.createComment({
              issue_number: context.issue.number,
              owner: context.repo.owner,
              repo: context.repo.repo,
              body: comment
            });
```

### Example 2: Auto-translate Missing Keys

Create `.github/workflows/auto-translate.yml`:

```yaml
name: Auto-translate Missing Keys

on:
  workflow_dispatch:
    inputs:
      target_culture:
        description: 'Target culture to translate (e.g., tr, de)'
        required: true
      provider:
        description: 'Translation provider'
        required: true
        default: 'google'
        type: choice
        options:
          - google
          - deepl
          - openai

jobs:
  translate:
    name: Auto-translate
    runs-on: ubuntu-latest
    
    steps:
      - name: 📥 Checkout
        uses: actions/checkout@v4
      
      - name: 📦 Install Locale CLI
        run: npm install -g @taiizor/locale-cli
      
      - name: 🌐 Translate
        env:
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
          DEEPL_API_KEY: ${{ secrets.DEEPL_API_KEY }}
        run: |
          # Determine provider and API key
          # Default to Google (free, no API key) if secrets are not available
          PROVIDER="${{ inputs.provider }}"
          API_KEY=""
          
          case "${PROVIDER}" in
            openai)
              if [ -z "${OPENAI_API_KEY}" ]; then
                echo "⚠️ OpenAI API key not found, falling back to Google Translate (free)"
                PROVIDER="google"
              else
                API_KEY="${OPENAI_API_KEY}"
              fi
              ;;
            deepl)
              if [ -z "${DEEPL_API_KEY}" ]; then
                echo "⚠️ DeepL API key not found, falling back to Google Translate (free)"
                PROVIDER="google"
              else
                API_KEY="${DEEPL_API_KEY}"
              fi
              ;;
            google)
              # Google Translate is free and doesn't require an API key
              ;;
            *)
              echo "⚠️ Provider ${PROVIDER} requires API key configuration"
              ;;
          esac
          
          # Build command with or without API key
          if [ -n "${API_KEY}" ]; then
            locale translate ${{ inputs.target_culture }} \
              --from en \
              --in ./locales \
              --provider "${PROVIDER}" \
              --api-key "${API_KEY}" \
              --parallel 5 \
              --delay 500
          else
            locale translate ${{ inputs.target_culture }} \
              --from en \
              --in ./locales \
              --provider "${PROVIDER}" \
              --parallel 5 \
              --delay 500
          fi
      
      - name: 📝 Create Pull Request
        uses: peter-evans/create-pull-request@v6
        with:
          commit-message: "chore: auto-translate ${{ inputs.target_culture }} using ${{ inputs.provider }}"
          title: "Auto-translate ${{ inputs.target_culture }}"
          body: |
            Automated translation for ${{ inputs.target_culture }} using ${{ inputs.provider }}.
            
            **⚠️ Please review translations before merging!**
            
            Generated by GitHub Actions workflow.
          branch: auto-translate/${{ inputs.target_culture }}
          labels: translation, automated
```

## Azure Pipelines

### Example: Translation Validation Pipeline

Create `azure-pipelines.yml`:

```yaml
trigger:
  branches:
    include:
      - main
      - develop
  paths:
    include:
      - locales/**

pool:
  vmImage: 'ubuntu-latest'

steps:
  - task: NodeTool@0
    inputs:
      versionSpec: '20.x'
    displayName: 'Install Node.js'

  - script: |
      npm install -g @taiizor/locale-cli
    displayName: 'Install Locale CLI'

  - script: |
      locale scan ./locales --base en --targets tr,de --output $(Build.ArtifactStagingDirectory)/scan-report.json
    displayName: 'Scan translations'

  - script: |
      locale check ./locales --rules no-empty-values,consistent-placeholders --ci
    displayName: 'Validate translation quality'
    continueOnError: false

  - task: PublishBuildArtifacts@1
    condition: always()
    inputs:
      PathtoPublish: '$(Build.ArtifactStagingDirectory)'
      ArtifactName: 'translation-reports'
    displayName: 'Publish reports'
```

## GitLab CI

### Example: Translation Pipeline

Create `.gitlab-ci.yml`:

```yaml
stages:
  - validate
  - report

variables:
  NODE_VERSION: "20"

validate-translations:
  stage: validate
  image: node:${NODE_VERSION}
  before_script:
    - npm install -g @taiizor/locale-cli
  script:
    - locale check ./locales --rules no-empty-values,no-duplicate-keys --ci
    - locale scan ./locales --base en --targets tr,de --output scan-report.json
  artifacts:
    when: always
    paths:
      - scan-report.json
    reports:
      junit: scan-report.json
  rules:
    - if: '$CI_PIPELINE_SOURCE == "merge_request_event"'
    - changes:
        - locales/**

generate-report:
  stage: report
  image: node:${NODE_VERSION}
  needs: [validate-translations]
  before_script:
    - npm install -g @taiizor/locale-cli
  script:
    - echo "Translation validation completed"
    - cat scan-report.json
  only:
    - merge_requests
```

## Pre-commit Hook

### Example: Local Validation

Create `.husky/pre-commit` (using Husky):

```bash
#!/bin/sh
. "$(dirname "$0")/_/husky.sh"

# Check if translation files were modified
CHANGED_LOCALES=$(git diff --cached --name-only | grep -E "^locales/.*\.(json|yaml|resx|po)$")

if [ -n "$CHANGED_LOCALES" ]; then
  echo "🔍 Validating translation files..."
  
  # Run locale check
  npx @taiizor/locale-cli check ./locales \
    --rules no-empty-values,no-duplicate-keys \
    || (echo "❌ Translation validation failed!" && exit 1)
  
  echo "✅ Translation validation passed!"
fi
```

Or using Git hooks directly (`.git/hooks/pre-commit`):

```bash
#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}🔍 Checking translation files...${NC}"

# Check if locale CLI is installed
if ! command -v locale &> /dev/null; then
    echo -e "${YELLOW}⚠️  Locale CLI not found. Installing...${NC}"
    npm install -g @taiizor/locale-cli
fi

# Get staged locale files
STAGED_FILES=$(git diff --cached --name-only --diff-filter=ACM | grep -E "\.(json|yaml|resx|po)$" | grep "locales/")

if [ -z "$STAGED_FILES" ]; then
    echo -e "${GREEN}✅ No translation files to validate${NC}"
    exit 0
fi

# Run validation
locale check ./locales --rules no-empty-values,no-duplicate-keys --ci

if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Translation validation failed!${NC}"
    echo -e "${YELLOW}Fix the issues above before committing.${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Translation validation passed!${NC}"
exit 0
```

## CircleCI

### Example: Translation Workflow

Create `.circleci/config.yml`:

```yaml
version: 2.1

orbs:
  node: circleci/node@5.1.0

jobs:
  validate-translations:
    executor:
      name: node/default
      tag: '20'
    steps:
      - checkout
      - run:
          name: Install Locale CLI
          command: npm install -g @taiizor/locale-cli
      - run:
          name: Check translations
          command: |
            locale check ./locales \
              --rules no-empty-values,no-duplicate-keys,consistent-placeholders \
              --ci
      - run:
          name: Scan for gaps
          command: |
            locale scan ./locales \
              --base en \
              --targets tr,de \
              --output scan-report.json
      - store_artifacts:
          path: scan-report.json
          destination: translation-reports

workflows:
  version: 2
  validate:
    jobs:
      - validate-translations:
          filters:
            branches:
              only:
                - main
                - develop
```

## Docker Integration

### Example: Validation Container

Create `Dockerfile.locale-check`:

```dockerfile
FROM node:20-alpine

# Install Locale CLI
RUN npm install -g @taiizor/locale-cli

# Set working directory
WORKDIR /app

# Default command
ENTRYPOINT ["locale"]
CMD ["--help"]
```

Usage:

```bash
# Build
docker build -t locale-check -f Dockerfile.locale-check .

# Run validation
docker run --rm -v $(pwd)/locales:/app/locales locale-check \
  check /app/locales --rules no-empty-values --ci
```

## Tips for CI/CD Integration

1. **Cache Dependencies**: Cache npm packages or .NET tools to speed up builds
2. **Parallel Execution**: Use `--parallel` flag for translation to speed up large projects
3. **Fail Fast**: Use `--ci` flag to return non-zero exit codes on violations
4. **Artifacts**: Always upload reports as artifacts for debugging
5. **Selective Runs**: Only run validation when translation files change
6. **Security**: Store API keys in secure environment variables/secrets