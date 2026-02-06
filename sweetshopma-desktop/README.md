# SweetShopMa Desktop Application

A multi-branch offline-first Point of Sale system built with Django and PyWebView.

## Features

- ✅ **Offline-First** - Works without internet connection
- ✅ **Auto-Sync** - Syncs to central server when online
- ✅ **Encrypted Database** - SQLCipher for data protection
- ✅ **Hardware-Based Licensing** - Tied to specific machines
- ✅ **Multi-Branch Support** - Manage multiple locations
- ✅ **Real-Time Dashboard** - Owner visibility across all branches

## Project Structure

```
sweetshopma-desktop/
├── backend/              # Django backend (REST API)
│   ├── manage.py
│   ├── sweetshop/       # Django project settings
│   └── api/             # API app (models, views, serializers)
├── frontend/            # PyWebView desktop frontend
│   ├── main.py          # Application entry point
│   ├── api.py           # API bridge to Django
│   ├── utils.py         # Utility functions
│   ├── templates/       # HTML templates
│   └── static/          # CSS, JS, images
├── security/            # Security modules
│   ├── hardware_id.py   # Hardware ID generation
│   ├── license_manager.py  # License validation
│   ├── key_manager.py   # SQLCipher key management
│   └── anti_debug.py    # Anti-debugging protection
├── sync/                # Sync service
│   ├── sync_service.py  # Background sync thread
│   └── sync_api.py      # Sync API client
├── config/              # Configuration files
│   ├── settings.py      # Application settings
│   └── build_config.py  # Build configuration
├── scripts/             # Build and utility scripts
│   ├── build.py         # Build executable
│   ├── generate_license.py  # License generator
│   └── obfuscate.py     # Code obfuscation
├── requirements.txt     # Python dependencies
├── sweetshopma.spec     # PyInstaller spec file
└── README.md           # This file
```

## Installation

### Development Setup

```bash
# Clone or navigate to project directory
cd sweetshopma-desktop

# Create virtual environment
python -m venv venv

# Activate virtual environment
# Windows:
venv\Scripts\activate
# Mac/Linux:
source venv/bin/activate

# Install dependencies
pip install -r requirements.txt
```

### Running in Development Mode

```bash
# Start Django backend (in one terminal)
cd backend
python manage.py runserver

# Start PyWebView frontend (in another terminal)
cd frontend
python main.py
```

## Building Executable

### Prerequisites

- Python 3.9+
- All dependencies installed
- PyArmor (for obfuscation)
- PyInstaller (for packaging)

### Build Steps

```bash
# 1. Obfuscate code (optional but recommended)
python scripts/obfuscate.py

# 2. Build executable
python scripts/build.py

# 3. Find executable in dist/SweetShopMa.exe
```

## Deployment

### For Each Branch

1. **Copy executable to branch PC**
   ```
   Copy dist/SweetShopMa.exe to the target computer
   ```

2. **Run executable to get Hardware ID**
   ```
   SweetShopMa.exe
   # Will display Hardware ID
   ```

3. **Generate license key**
   ```bash
   python scripts/generate_license.py
   # Enter the Hardware ID from step 2
   # Enter branch name
   # Enter license duration (days)
   ```

4. **Distribute license file**
   ```
   Copy the generated .key file to the branch PC
   Place it next to SweetShopMa.exe
   ```

5. **Run application**
   ```
   SweetShopMa.exe
   # Should now launch successfully
   ```

## Central Server Setup

### Required Components

1. **Django REST API** - Receives sync data from branches
2. **PostgreSQL Database** - Stores all branch data
3. **Owner Dashboard** - Web interface for viewing all branches

### Deployment Options

- **Heroku** - $5-25/month
- **Railway** - $5/month
- **DigitalOcean** - $4-8/month
- **AWS/Azure** - Enterprise options

## Security Features

### Multi-Layer Protection

1. **SQLCipher Encryption** - AES-256 database encryption
2. **Hardware-Based Licensing** - Tied to machine hardware
3. **Code Obfuscation** - PyArmor protection
4. **Anti-Debugging** - Blocks debugging attempts
5. **Server Validation** - Detects tampering during sync

### Key Management

- Encryption keys derived from hardware ID + license + install time
- Keys are obfuscated and embedded in code
- Database cannot be opened on another machine

## Sync Service

### How It Works

1. Application runs normally (offline)
2. Background thread checks for internet connection
3. When online, uploads local changes to central server
4. Central server validates and stores data
5. Owner can view all branch data in real-time

### Sync Configuration

```python
# In config/settings.py
CENTRAL_SERVER_URL = "https://your-server.com/api"
SYNC_INTERVAL = 300  # 5 minutes
```

## Troubleshooting

### Common Issues

**"License not valid for this machine"**
- Verify Hardware ID matches
- Regenerate license with correct Hardware ID

**"Database cannot be opened"**
- Check if database file exists
- Verify encryption key is correct
- Check SQLCipher installation

**"Sync not working"**
- Check internet connection
- Verify CENTRAL_SERVER_URL is correct
- Check server logs for errors

## Development

### Adding New Features

1. Add models to `backend/api/models.py`
2. Create serializers in `backend/api/serializers.py`
3. Add views in `backend/api/views.py`
4. Update URLs in `backend/api/urls.py`
5. Create frontend API methods in `frontend/api.py`
6. Add UI templates in `frontend/templates/`

### Testing

```bash
# Run Django tests
cd backend
python manage.py test

# Test executable on clean machine
# Copy dist/SweetShopMa.exe to a VM or clean PC
# Test all features offline and online
```

## Maintenance

### Updates

1. Update code in repository
2. Increment version in `config/settings.py`
3. Rebuild executable
4. Distribute new version to branches

### Backups

- Automatic database backups to `backups/` directory
- Keeps last 10 backups
- Manual backup: Copy `sweetshopma.db` file

## Support

For issues or questions:
- Check documentation in `plans/` directory
- Review logs in `logs/` directory
- Contact support with Hardware ID and error details

## License

Proprietary - All rights reserved

## Version History

- **1.0.0** - Initial release
  - Offline-first POS system
  - Multi-branch support
  - Encrypted database
  - Hardware-based licensing
  - Auto-sync to central server
