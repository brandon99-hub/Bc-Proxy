# Running BcProxy as a Windows Service

To ensure the API runs continuously and automatically starts on server reboot, you have several options:

## Option 1: Using NSSM (Recommended - Easiest)

NSSM (Non-Sucking Service Manager) is the easiest way to run a .NET application as a Windows Service.

### Steps:

1. **Download NSSM:**
   - Download from: https://nssm.cc/download
   - Extract to a folder (e.g., `C:\Tools\nssm`)

2. **Install the Service:**
   ```powershell
   # Open PowerShell as Administrator
   cd "C:\Users\USERR\PythonProject\BC proxy\BcProxy"
   
   # Install the service (replace path with your NSSM location)
   C:\Tools\nssm\win64\nssm.exe install BcProxyService
   ```

3. **Configure the Service:**
   - **Path:** `C:\Program Files\dotnet\dotnet.exe` (or your dotnet path)
   - **Startup directory:** `C:\Users\USERR\PythonProject\BC proxy\BcProxy`
   - **Arguments:** `run --urls "http://localhost:5000;https://localhost:8093"`
   - **Service name:** `BcProxyService`
   - **Display name:** `Business Central Proxy API`
   - **Description:** `Proxy API for Business Central OData endpoints`

   Or use command line:
   ```powershell
   C:\Tools\nssm\win64\nssm.exe set BcProxyService Application "C:\Program Files\dotnet\dotnet.exe"
   C:\Tools\nssm\win64\nssm.exe set BcProxyService AppDirectory "C:\Users\USERR\PythonProject\BC proxy\BcProxy"
   C:\Tools\nssm\win64\nssm.exe set BcProxyService AppParameters "run --urls \"http://localhost:5000;https://localhost:8093\""
   C:\Tools\nssm\win64\nssm.exe set BcProxyService DisplayName "Business Central Proxy API"
   C:\Tools\nssm\win64\nssm.exe set BcProxyService Description "Proxy API for Business Central OData endpoints"
   C:\Tools\nssm\win64\nssm.exe set BcProxyService Start SERVICE_AUTO_START
   ```

4. **Start the Service:**
   ```powershell
   C:\Tools\nssm\win64\nssm.exe start BcProxyService
   ```

5. **Manage the Service:**
   ```powershell
   # Stop
   C:\Tools\nssm\win64\nssm.exe stop BcProxyService
   
   # Restart
   C:\Tools\nssm\win64\nssm.exe restart BcProxyService
   
   # Remove
   C:\Tools\nssm\win64\nssm.exe remove BcProxyService confirm
   ```

## Option 2: Using Windows Service Control Manager (sc.exe)

### Steps:

1. **Publish the Application:**
   ```powershell
   cd "C:\Users\USERR\PythonProject\BC proxy\BcProxy"
   dotnet publish -c Release -o "C:\Services\BcProxy"
   ```

2. **Create a Batch File to Run the Service:**
   Create `C:\Services\BcProxy\run.bat`:
   ```batch
   @echo off
   cd /d "C:\Services\BcProxy"
   dotnet BcProxy.dll --urls "http://localhost:5000;https://localhost:8093"
   ```

3. **Install as Service using NSSM or sc.exe:**
   ```powershell
   # Using sc.exe (requires a service wrapper - NSSM is easier)
   # Or use NSSM as shown in Option 1
   ```

## Option 3: Using Windows Task Scheduler (Alternative)

If you prefer not to use a service:

1. **Open Task Scheduler** (taskschd.msc)

2. **Create Basic Task:**
   - Name: `BcProxy API`
   - Trigger: `When the computer starts`
   - Action: `Start a program`
   - Program: `C:\Program Files\dotnet\dotnet.exe`
   - Arguments: `run --urls "http://localhost:5000;https://localhost:8093"`
   - Start in: `C:\Users\USERR\PythonProject\BC proxy\BcProxy`
   - Check: `Run whether user is logged on or not`
   - Check: `Run with highest privileges`

## Option 4: Publish and Run as Standalone (Production)

For production, publish as a self-contained executable:

```powershell
cd "C:\Users\USERR\PythonProject\BC proxy\BcProxy"
dotnet publish -c Release -r win-x64 --self-contained true -o "C:\Services\BcProxy"
```

Then use NSSM to run `C:\Services\BcProxy\BcProxy.exe` directly.

## Verifying the Service is Running

```powershell
# Check service status
Get-Service BcProxyService

# View service logs (if using NSSM)
C:\Tools\nssm\win64\nssm.exe status BcProxyService

# Test the API
curl http://localhost:5000/items
```

## Security Configuration

### API Key Authentication

The API uses API key authentication to protect endpoints. Configure it in `appsettings.json`:

```json
{
  "ApiKey": "your-secret-api-key-here"
}
```

**Important:** 
- If `ApiKey` is empty or not set, authentication is **disabled** (development mode)
- If `ApiKey` is set, all requests must include the header: `X-API-Key: your-secret-api-key-here`

### Using the API with Authentication

```powershell
# With API key
curl -H "X-API-Key: your-secret-api-key-here" http://localhost:5000/items

# Without API key (will fail if ApiKey is configured)
curl http://localhost:5000/items
```

### Exposing to External Network

To allow external access (not just localhost):

1. **Update the URL binding** in your service configuration:
   ```powershell
   # Change from localhost to 0.0.0.0 (all interfaces) or specific IP
   --urls "http://0.0.0.0:5000;https://0.0.0.0:8093"
   ```

2. **Configure Firewall:**
   ```powershell
   # Allow port 5000 in Windows Firewall
   New-NetFirewallRule -DisplayName "BcProxy API" -Direction Inbound -LocalPort 5000 -Protocol TCP -Action Allow
   ```

3. **Set API Key** in `appsettings.json` (REQUIRED before exposing):
   ```json
   {
     "ApiKey": "your-strong-secret-key-here"
   }
   ```

4. **Share the URL:**
   - Internal network: `http://YOUR-SERVER-IP:5000/items`
   - External: `http://YOUR-PUBLIC-IP:5000/items` (if port-forwarded)

## Firewall Configuration

If accessing from other machines, allow the port in Windows Firewall:

```powershell
New-NetFirewallRule -DisplayName "BcProxy API" -Direction Inbound -LocalPort 5000 -Protocol TCP -Action Allow
```

## Recommended: Use NSSM

NSSM is the simplest and most reliable option. It handles:
- Automatic restarts on failure
- Logging to files
- Easy configuration via GUI or command line
- Proper Windows Service integration

