# Security Policy

## Supported Versions

The `main` branch is the only currently supported version.

## Reporting a Vulnerability

Please do not open a public issue for sensitive security problems.

Report vulnerabilities privately through GitHub security advisories when available, or contact the repository owner through GitHub with:

- a short description of the issue;
- steps to reproduce;
- potential impact;
- any suggested mitigation.

## Secrets

Never commit production secrets, database passwords, Basic Auth hashes, private keys, tokens, or local `.env` files.

Use environment variables, GitHub secrets, Docker secrets, or server-local configuration for sensitive values.
