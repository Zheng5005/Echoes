# Echoes

A full-stack real-time chat application built with React, Node.js, and Socket.IO.

## Features

- **Real-time messaging** -- Instant message delivery powered by Socket.IO
- **Authentication** -- Secure signup, login, and logout with JWT stored in HTTP-only cookies
- **User management** -- Browse users, view profiles, and upload profile pictures via Cloudinary
- **Chat interface** -- Two-pane layout with user sidebar and message history, supporting text and image messages
- **Theme support** -- Light/dark theme switching with DaisyUI
- **Protected routes** -- Server-side JWT middleware and client-side route guards

## Tech Stack

### Frontend

| Technology | Purpose |
|---|---|
| React 18 | UI library |
| Vite 6 | Build tool |
| React Router DOM v7 | Client-side routing |
| Zustand v5 | State management |
| Tailwind CSS v3 + DaisyUI v4 | Styling and components |
| Socket.IO Client v4 | Real-time communication |
| Axios | HTTP client |
| Lucide React | Icons |
| React Hot Toast | Notifications |

### Backend

| Technology | Purpose |
|---|---|
| Node.js + Express v4 | Server runtime and framework |
| MongoDB + Mongoose v8 | Database and ODM |
| Socket.IO v4 | Real-time communication |
| JWT + bcryptjs | Authentication and password hashing |
| Cloudinary v2 | Image upload and storage |

## Project Structure

```
Echoes/
├── Client/                 # React frontend
│   ├── src/
│   │   ├── components/     # Reusable UI components
│   │   ├── pages/          # Route pages (Home, Login, Signup, Profile, Settings)
│   │   ├── store/          # Zustand stores (auth, chat, theme)
│   │   ├── lib/            # Axios instance and utilities
│   │   └── constants/      # Application constants
│   └── vite.config.js
├── Server/                 # Express backend
│   ├── src/
│   │   ├── controllers/    # Request handlers
│   │   ├── models/         # Mongoose schemas
│   │   ├── routes/         # API route definitions
│   │   ├── middlewares/    # JWT authentication middleware
│   │   ├── lib/            # Database, Socket.IO, Cloudinary, utilities
│   │   └── index.js        # Server entry point
│   └── package.json
└── package.json            # Root workspace scripts
```

## Getting Started

### Prerequisites

- Node.js 18+
- MongoDB instance (local or Atlas)
- Cloudinary account (for image uploads)

### Environment Variables

Create a `.env` file in the `Server/` directory:

```env
MONGO_URI=your_mongodb_connection_string
JWT_SECRET=your_jwt_secret
CLOUDINARY_CLOUD_NAME=your_cloudinary_cloud_name
CLOUDINARY_API_KEY=your_cloudinary_api_key
CLOUDINARY_API_SECRET=your_cloudinary_api_secret
PORT=3000
```

### Installation

1. Clone the repository:

```bash
git clone https://github.com/Zheng5005/Echoes.git
cd Echoes
```

2. Install dependencies and build:

```bash
npm run build
```

3. Start the server:

```bash
npm run start
```

The application will be available at `http://localhost:3000`.

### Development

Run the frontend and backend separately in development mode:

```bash
# Start the backend (from Server/)
cd Server && npm run dev

# Start the frontend (from Client/)
cd Client && npm run dev
```

The Vite dev server runs on `http://localhost:5173` and proxies API requests to the backend.

## API Endpoints

### Authentication

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/signup` | Register a new user |
| POST | `/api/auth/login` | Authenticate user |
| POST | `/api/auth/logout` | Clear authentication cookie |
| POST | `/api/auth/update-profile` | Update profile picture |
| GET | `/api/auth/check` | Verify authentication status |

### Messages

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/messages/users` | Get all users for sidebar |
| GET | `/api/messages/:id` | Get conversation messages with a user |
| POST | `/api/messages/send/:id` | Send a message (text or image) |

## Scripts

| Command | Description |
|---|---|
| `npm run build` | Install dependencies and build the frontend |
| `npm run start` | Start the production server |

## License

ISC
