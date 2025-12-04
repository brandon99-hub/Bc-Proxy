# BcProxy - Business Central Proxy API

A .NET 8 Web API that acts as a middle-layer proxy between Business Central on-prem server and external clients.

## Features

- Windows Authentication (UseDefaultCredentials)
- Clean JSON responses (OData metadata stripped)
- Strong error handling (timeouts, connection failures, non-200 responses)
- Minimal logging (Console + Debug)
- RESTful endpoints for Business Central Items

## Configuration

Update `appsettings.json` with your Business Central base URL:

```json
{
  "BusinessCentral": {
    "BaseUrl": "http://brandon:7048/BC240/ODataV4/Company('CRONUS%20International%20Ltd.')"
  }
}
```

## Running the Application

### Prerequisites
- .NET 8 SDK installed
- Windows Server (same network as Business Central)
- Windows Authentication configured

### Run Commands

```bash
# Navigate to project directory
cd BcProxy

# Restore packages
dotnet restore

# Run the application
dotnet run

# Or specify URL and port explicitly
dotnet run --urls "http://localhost:5000;https://localhost:5001"
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI (Development): `http://localhost:5000/swagger`

## API Endpoints

### GET /items
Retrieves all items from Business Central.

**Response:** Array of Item objects

### GET /items/{no}
Retrieves a specific item by number.

**Parameters:**
- `no` (path): Item number

**Response:** Item object or 404 if not found

## Example Requests

```bash
# Get all items
curl http://localhost:5000/items

# Get specific item
curl http://localhost:5000/items/1000
```

## Error Handling

The API handles various error scenarios:
- **502 Bad Gateway**: Connection failure to Business Central
- **504 Gateway Timeout**: Request timeout (30 seconds)
- **404 Not Found**: Item not found
- **500 Internal Server Error**: Unexpected errors

## Development

The application runs in development mode with:
- Swagger UI enabled
- Debug logging
- No authentication required

