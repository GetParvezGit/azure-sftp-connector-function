
Here's the improved `README.md` file, incorporating the new content while maintaining the existing structure and information:


# Azure SFTP Connector

This repository contains an Azure Functions solution that provides SFTP connector functionality. It includes reusable SFTP helpers (SSH key and password-based) and Azure Functions HTTP endpoints to interact with SFTP servers (upload, download, delete, list files). The solution targets .NET 8 and is developed to run locally with Visual Studio 2022 and in Azure Functions.

## Projects

- `AzureSftpHelper` - Class library with SFTP helper services using SSH.NET and WinSCP. Contains connection configuration models, services for SSH-key and password authentication, and extension utilities.
- `AzureSftpConnectorFunction` - .NET isolated Azure Functions project exposing HTTP-triggered endpoints that call into `AzureSftpHelper` to perform SFTP operations.

## Prerequisites

- Visual Studio 2022 (latest updates)
- .NET 8 SDK
- Azure Functions Core Tools (for local function host, optional when using Visual Studio)
- (Optional) WinSCP installed if you intend to use/inspect WinSCP scripting features locally

## Configuration

This solution uses `local.settings.json` (for local development) and environment variables in production. Sensitive values MUST NOT be checked into source control.

Configuration keys follow a naming convention to support multiple SFTP systems and authentication types:

- **SSH key-based connections:**
  - `SftpHost_SshKey_{SystemName}`
  - `SftpPort_SshKey_{SystemName}`
  - `SftpUsername_SshKey_{SystemName}`
  - `SftpSSHKey_SshKey_{SystemName}` (Base64-encoded private key)
  - `SftpPassPhrase_SshKey_{SystemName}` (optional)

- **Password-based connections (WinSCP or SSH.NET):**
  - `SftpHost_WinSCP_{SystemName}` / `SftpHost_SshNet_{SystemName}`
  - `SftpPort_WinSCP_{SystemName}` / `SftpPort_SshNet_{SystemName}`
  - `SftpUsername_WinSCP_{SystemName}` / `SftpUsername_SshNet_{SystemName}`
  - `SftpPassword_WinSCP_{SystemName}` / `SftpPassword_SshNet_{SystemName}`

Example (local.settings.json - DO NOT commit secrets):

```
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",

    "SftpHost_SshKey_MyMicros": "example.sftp.host",
    "SftpPort_SshKey_MyMicros": "22",
    "SftpUsername_SshKey_MyMicros": "user",
    "SftpSSHKey_SshKey_MyMicros": "<Base64-encoded-private-key>",

    "SftpHost_WinSCP_Other": "1.2.3.4",
    "SftpPort_WinSCP_Other": "22",
    "SftpUsername_WinSCP_Other": "user",
    "SftpPassword_WinSCP_Other": "<password>"
  }
}
```

## Build and run locally

1. Open the solution in Visual Studio 2022.
2. Set `AzureSftpConnectorFunction` as the startup project.
3. Ensure `local.settings.json` contains your local configuration (copy from `local.settings.json.example` or create one).
4. Run the project (F5) — the Azure Functions host will start and expose HTTP endpoints.

Alternatively, use the Azure Functions Core Tools:

- From the project folder: `func start --script-root bin/Debug/net8.0` (adjust path for your build configuration)

## Endpoints and usage

The Azure Functions expose HTTP endpoints for common SFTP actions. Each endpoint accepts parameters such as `fileName`, `systemName`, and `remoteDirectory`. Consult the function route definitions in the `AzureSftpConnectorFunction` project for exact routes and HTTP methods.

Examples:
- GET /api/sftp/download?systemName=MyMicros&remoteDirectory=/inbound&fileName=test.txt
- POST /api/sftp/upload (multipart/form-data or request body depending on function implementation)

## Dependencies

- **SSH.NET (Renci.SshNet)** — used for SSH-key and password authentication SFTP operations.
- **WinSCP** — used for some WinSCP-specific transfer flows and scripting.

Refer to each project .csproj for exact package versions.

## Security

- Never commit `local.settings.json` with secrets. Use user secrets or environment variables in CI/CD and production.
- Private keys are expected to be provided as Base64-encoded strings in configuration. Protect those values using your secret store (Azure Key Vault, GitHub Secrets, etc.).

## Coding standards and contribution

This repository contains an `.editorconfig` and `CONTRIBUTING.md`. All code must follow those rules exactly. Before creating PRs, run code formatting and follow the contribution checklist described in `CONTRIBUTING.md`.

## Extending the solution

- To add a new SFTP system, follow the naming convention described under Configuration and add the appropriate keys to your environment or `local.settings.json`.
- Implement new behaviors in `AzureSftpHelper` and add corresponding Function endpoints in `AzureSftpConnectorFunction`.

## Troubleshooting

- Inspect the Function host logs in the console output when running locally.
- Use `AzureSftpHelper.Extensions.ExtensionMethods.GetMessageForLogging` to format exceptions for detailed logging.

## License

MIT License. See `LICENSE` file for details.
