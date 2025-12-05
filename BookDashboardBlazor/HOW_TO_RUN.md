# How to Run the Book Dashboard Blazor Application

## Prerequisites
This Blazor WebAssembly application requires a backend .NET Web API to be running.

## Required Components

### 1. Backend API Server
The Blazor app is configured to connect to a backend API at: **`http://localhost:5069/`**

You need to:
1. Locate your .NET Web API project (the backend with Controllers for Books, Authors, Genres, etc.)
2. Navigate to the API project directory
3. Run the API server:
   ```powershell
   dotnet run
   ```
4. Verify the API is running on port 5069 (check the console output)

### 2. Blazor Frontend
Once the backend API is running:

1. Open a **new terminal window**
2. Navigate to the Blazor project:
   ```powershell
   cd C:\Users\Mohamed\.gemini\antigravity\scratch\BookDashboardBlazor
   ```
3. Run the Blazor app:
   ```powershell
   dotnet run
   ```
4. Open your browser to the URL shown in the console (typically `http://localhost:5000` or `https://localhost:5001`)

## Running Both Projects Simultaneously

You have two options:

### Option A: Two Terminal Windows
1. **Terminal 1**: Run the backend API
   ```powershell
   cd <path-to-api-project>
   dotnet run
   ```

2. **Terminal 2**: Run the Blazor frontend
   ```powershell
   cd C:\Users\Mohamed\.gemini\antigravity\scratch\BookDashboardBlazor
   dotnet run
   ```

### Option B: Using dotnet watch (Recommended for Development)
This automatically reloads when you make changes:

1. **Terminal 1**: Backend API
   ```powershell
   cd <path-to-api-project>
   dotnet watch run
   ```

2. **Terminal 2**: Blazor Frontend
   ```powershell
   cd C:\Users\Mohamed\.gemini\antigravity\scratch\BookDashboardBlazor
   dotnet watch run
   ```

## Troubleshooting

### Error: "fail: Microsoft.Extensions.Hosting.Internal.Host[11]"
This error occurs when the Blazor app cannot connect to the backend API. Make sure:
- The backend API is running on `http://localhost:5069/`
- The API endpoints are accessible (test with browser or Postman)
- No firewall is blocking the connection

### Changing the API URL
If your backend API runs on a different port, update [`Program.cs`](file:///C:/Users/Mohamed/.gemini/antigravity/scratch/BookDashboardBlazor/Program.cs) line 12:
```csharp
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:YOUR_PORT/") });
```

## API Endpoints Required
The Blazor app expects these API endpoints to be available:
- `/api/Books` - GET, POST, PUT, DELETE
- `/api/Authors` - GET, POST, PUT, DELETE
- `/api/Genres` - GET, POST, PUT, DELETE
- `/api/Reviews` - GET, POST, PUT, DELETE
- `/api/Comments` - GET, POST, PUT, DELETE
- `/api/Goals` - GET, POST, PUT, DELETE
- `/api/Auth/login` - POST (for authentication)
- `/api/Auth/register` - POST (for registration)

## Next Steps
1. Locate your backend API project
2. Start the backend API first
3. Then start the Blazor frontend
4. Navigate to the Blazor app URL in your browser
