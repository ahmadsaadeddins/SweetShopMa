# SweetShopMa Frontend (Next.js)

Next.js 16 frontend for the SweetShopMa Point of Sale system.

## Setup

1. **Install dependencies:**
```bash
npm install
```

2. **Set environment variables:**
Create a `.env.local` file:
```
NEXT_PUBLIC_API_URL=http://localhost:8000/api
```

3. **Run development server:**
```bash
npm run dev
```

The app will be available at `http://localhost:3000`

## Features

- ✅ User authentication (login/logout)
- ✅ Shop interface (product browsing, cart, checkout)
- ✅ Admin panel (user management, reports)
- ✅ Multi-language support (English/Arabic with RTL)
- ✅ Responsive design with Tailwind CSS

## Project Structure

```
frontend/
├── app/              # Next.js app directory (pages)
├── components/       # React components
├── lib/             # Utilities, API client, contexts
└── public/          # Static assets
```

## Pages

- `/` - Home (redirects to login or shop)
- `/login` - Login page
- `/shop` - Main shop interface
- `/admin` - Admin panel

## Development

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run start` - Start production server
- `npm run lint` - Run ESLint

## Notes

- Uses JWT tokens stored in cookies for authentication
- API client automatically handles token refresh
- Localization context manages language state
- Auth context manages user authentication state

