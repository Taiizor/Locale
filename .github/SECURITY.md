# Security Policy

## Supported Versions

The following versions of Locale are currently supported with security updates:

| Version | Supported          |
| ------- | ------------------ |
| 0.0.x   | :white_check_mark: |

## Reporting a Vulnerability

We take security vulnerabilities seriously. If you discover a security issue, please report it responsibly.

### How to Report

1. **DO NOT** create a public GitHub issue for security vulnerabilities
2. Email the security issue to the maintainers directly
3. Include as much detail as possible:
   - Description of the vulnerability
   - Steps to reproduce
   - Potential impact
   - Suggested fix (if any)

### What to Expect

- **Initial Response**: Within 48 hours of your report
- **Status Update**: Within 7 days with our assessment
- **Resolution**: Security patches will be prioritized and released as soon as possible

### Scope

This security policy applies to:

- `Locale` NuGet package
- `Locale.CLI` .NET tool
- Source code in the official repository

### Out of Scope

The following are not considered security vulnerabilities:

- Third-party translation API security (Google, DeepL, OpenAI, etc.)
- Self-hosted LibreTranslate or Ollama instances
- Issues in dependencies (report these to the respective maintainers)

## Security Best Practices

When using Locale.CLI with translation providers:

1. **API Keys**: Never commit API keys to source control
2. **Environment Variables**: Use environment variables for sensitive data
3. **Rate Limiting**: Use the `--delay` option to avoid API rate limits
4. **Local Translation**: Consider using Ollama for sensitive content

```bash
# Good: Use environment variable
export OPENAI_API_KEY="your-key"
locale translate tr --from en --in ./locales --provider openai --api-key $OPENAI_API_KEY

# Good: Use local LLM for sensitive data
locale translate tr --from en --in ./locales --provider ollama --model llama3.2
```

## Acknowledgments

We appreciate security researchers who help keep Locale safe. Contributors who report valid security issues will be acknowledged (with permission) in our release notes.