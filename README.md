# Archiver Solution

This solution contains three projects:

1.  **Archiver.WebApp**: A Blazor Web App that serves as the frontend.
2.  **Archiver.ApiGateway**: An API Gateway built with ASP.NET Core and YARP (Yet Another Reverse Proxy) to route requests to backend services.
3.  **Archiver.Services**: A services layer containing REST APIs.

## Project Structure

- `Archiver.WebApp/`: The Blazor frontend application.
- `Archiver.ApiGateway/`: The API Gateway, configured to forward requests to `Archiver.Services`.
- `Archiver.Services/`: The backend services API.
- `tests/Archiver.WebApp.UITests/`: Playwright UI tests for the Blazor web application.
- `diagrams/`: Contains PlantUML diagrams describing the solution architecture.

## Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (Version 8.0 or later)
- [Node.js and npm](https://nodejs.org/en/download/) (for Playwright UI tests)

### Running the Application

1.  **Open the solution:** Open `ArchiverSolution.sln` in Visual Studio or your preferred IDE.
2.  **Run all projects:** Configure the solution to start multiple projects:
    - Right-click on the solution in Solution Explorer -> `Set Startup Projects...`
    - Select `Multiple startup projects`.
    - Set `Archiver.WebApp`, `Archiver.ApiGateway`, and `Archiver.Services` to `Start`.
    - Click `OK`.
3.  **Start debugging:** Press `F5` or click the `Start` button in your IDE.

Alternatively, you can run each project from the command line:

```bash
dotnet run --project Archiver.Services
dotnet run --project Archiver.ApiGateway
dotnet run --project Archiver.WebApp
```

Make sure to run them in the order: `Services`, `ApiGateway`, then `WebApp`.

### Playwright Test Setup

1.  **Navigate to the Playwright test directory:**
    ```bash
    cd tests/Archiver.WebApp.UITests
    ```
2.  **Install Node.js dependencies:**
    ```bash
    npm install
    ```
3.  **Install Playwright browsers:**
    ```bash
    npx playwright install --with-deps
    ```

### Running UI Tests (Playwright)

1.  **Ensure the `Archiver.WebApp` is running.** The Playwright tests expect the web application to be accessible at `https://localhost:7170`. You can start it using the instructions in "Running the Application" section.
2.  **Navigate to the Playwright test directory (if not already there):**
    ```bash
    cd tests/Archiver.WebApp.UITests
    ```
3.  **Run the tests:**
    ```bash
    npx playwright test
    ```
    To open the Playwright UI Test Runner:
    ```bash
    npx playwright test --ui
    ```

## Diagrams

PlantUML diagrams describing the solution architecture are located in the `diagrams/` folder:

- `Archiver.WebApp.puml`: Class hierarchy and object model for the Blazor Web App.
- `Archiver.ApiGateway.puml`: Class hierarchy and object model for the API Gateway.
- `Archiver.Services.puml`: Class hierarchy and object model for the Services layer.
- `ComponentDiagram.puml`: Component-level interrelation of the UI, API Gateway, and Services.
- `SequenceDiagram_WebApp_to_Services.puml`: Sequence diagram illustrating a typical request flow through the stack.

To view these diagrams, you can use a PlantUML viewer or extension in your IDE.

## Configuration

- **Archiver.ApiGateway**: Configured to proxy requests to `Archiver.Services`. For local development, it defaults to `http://localhost:5112` (configured in `Archiver.ApiGateway/appsettings.Development.json`). In other environments, the service address should be provided via environment variables or other configuration providers, overriding the placeholder in `Archiver.ApiGateway/appsettings.json`.
- **Archiver.WebApp**: Configured to make API calls to `Archiver.ApiGateway`. For local development, it defaults to `https://localhost:7005` (configured in `Archiver.WebApp/appsettings.Development.json`). In other environments, the API Gateway URL should be provided via environment variables or other configuration providers, overriding the placeholder in `Archiver.WebApp/appsettings.json`.
- **Playwright Tests**: Configured to use `https://localhost:7170` as the `baseURL`. This can be adjusted in `tests/Archiver.WebApp.UITests/playwright.config.ts`.