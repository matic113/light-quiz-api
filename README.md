# Light Quiz API

A modern .NET 9 REST API for quiz management with AI-powered auto-grading capabilities.

## ✨ Features

- **🤖 AI-Powered Grading** - Automatic quiz grading using Google Gemini API
- **👥 User Management** - JWT authentication with role-based access (Admin/Teacher/Student)
- **📊 Group Analytics** - Comprehensive quiz performance tracking and reporting
- **🔔 Push Notifications** - Real-time notifications via Firebase
- **📁 File Management** - Image upload and storage with Azure Blob Storage
- **📊 Excel Reports** - Generate and export quiz reports using MiniExcel
- **⚡ Background Jobs** - Async processing with Hangfire dashboard
- **🐳 Docker Ready** - Containerized deployment support
- **📚 API Documentation** - Interactive Swagger/OpenAPI docs

## 🚀 Quick Start

### Prerequisites
- .NET 9 SDK
- PostgreSQL database
- Google Gemini API key

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/matic113/light-quiz-api.git
   cd light-quiz-api
   ```

2. **Configure environment variables**
   ```bash
   cd src
   # Create appsettings.json or set environment variables (see Environment Setup below)
   ```

3. **Run the application**
   ```bash
   dotnet run
   ```

4. **Access the API**
   - API: `https://localhost:5001`
   - Swagger UI: `https://localhost:5001/swagger`
   - Hangfire Dashboard: `https://localhost:5001/hangfire`

## ⚙️ Environment Setup

Configure the following environment variables in your `appsettings.json` or as system environment variables:

### Required Configuration

```json
{
  "JWT": {
    "Key": "your-secret-key-here-minimum-32-characters",
    "Issuer": "https://your-domain.com",
    "Audience": "https://your-domain.com",
    "ExpiresInDays": 7
  },
  "Gemini": {
    "ApiKey": "your-gemini-api-key"
  },
  "ConnectionStrings": {
    "DevConnection": "Host=localhost;Database=lightquiz_dev;Username=postgres;Password=yourpassword",
    "DefaultConnection": "Host=localhost;Database=lightquiz;Username=postgres;Password=yourpassword",
    "AzureStorage": "DefaultEndpointsProtocol=https;AccountName=youraccount;AccountKey=yourkey"
  },
  "Hangfire": {
    "DashboardPassword": "your-hangfire-admin-password"
  }
}
```

### Firebase Service Key Setup

**Option 1: Service Key File (Recommended for development)**
1. Download your Firebase service account key from the Firebase Console
2. Save it as `serviceKey.json` in the `src/` directory
3. Ensure the file is included in your `.gitignore` for security

**Option 2: Environment Variable (Recommended for production)**
```bash
export SERVICE_KEY_JSON='{"type":"service_account","project_id":"your-project",...}'
```

> **Note**: When using Docker, the SERVICE_KEY_JSON environment variable is automatically written to `/app/serviceKey.json` during container startup.

## 🔗 Related Projects

- [Light Quiz Frontend](https://github.com/matic113/light-quiz) - Angular-based web application

## 📄 License

MIT License - see [LICENSE](LICENSE) file for details.
