---
description: Setup the Next.js frontend for the web application
---

# Setup Next.js Frontend

Setup and run the Next.js frontend for the SweetShopMa web application.

## Prerequisites
- Node.js 18+ installed
- Backend server running on port 8000 (see /setup-backend)

## Initial Setup

1. Navigate to frontend directory:
```bash
cd c:\Users\AMA\Documents\myhub\SweetShopMa\web-app\frontend
```

// turbo
2. Install dependencies:
```bash
npm install
```

3. Create environment file `.env.local` with:
```
NEXT_PUBLIC_API_URL=http://localhost:8000/api
```

## Run Development Server

// turbo
4. Start Next.js development server:
```bash
npm run dev
```

Frontend will run on `http://localhost:3000`

## Build for Production

// turbo
5. Build production bundle:
```bash
npm run build
```

// turbo
6. Start production server:
```bash
npm start
```

## Linting and Formatting

// turbo
7. Run ESLint:
```bash
npm run lint
```

## Features Available
- User authentication (login/logout)
- Shop interface (product browsing, barcode search, cart, checkout)
- Admin panel (reports, user management, product management)
- Multi-language support (English/Arabic with RTL)
- Responsive design
