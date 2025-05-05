# ZenFlow Frontend

Modern web application frontend for the ZenFlow workflow automation platform, built with Next.js, React, and Tailwind CSS.

## 📂 Folder Structure

```
src/
├── app/               # Next.js App Router pages and layouts
│   ├── api/           # API route handlers
│   ├── auth/          # Authentication pages (login, register, etc.)
│   ├── dashboard/     # Dashboard and app features
│   ├── globals.css    # Global styles
│   └── layout.tsx     # Root layout with providers
├── components/        # React components
│   ├── global/        # Global components (navbar, footer, etc.)
│   ├── layouts/       # Layout components (dashboard, etc.)
│   └── ui/            # Reusable UI components (buttons, inputs, etc.)
├── lib/               # Utility functions and shared code
│   ├── api/           # API client and services
│   ├── auth/          # Authentication utilities
│   └── utils.ts       # General utility functions
├── models/            # TypeScript interfaces and type definitions
├── store/             # Global state management (Zustand)
└── utils/             # Additional utilities
```

## 🧩 Architecture Overview

The project follows a modular architecture with the following key concepts:

### 1. App Router (Next.js)

- Based on file-system routing with app directory
- Each folder represents a route segment
- Layout components for shared UI across routes
- Server and client components appropriately used

### 2. Component Structure

- **Global Components**: App-wide components like headers and footers
- **Layout Components**: Structural components for page layouts
- **UI Components**: Reusable UI elements like buttons, inputs, etc.

### 3. Data Flow

- **API Services**: Centralized API interaction via lib/api
- **Authentication**: NextAuth.js for secure authentication
- **State Management**: Zustand for global state

### 4. Styling

- **Tailwind CSS**: Utility-first CSS framework
- **Theme Switching**: Dark/light mode with next-themes
- **Component Variants**: Class variance authority (cva) for component variants

## 🚀 Getting Started

### Prerequisites

- Node.js 18.17 or later
- npm or yarn

### Installation

```bash
# Install dependencies
npm install

# Start development server
npm run dev
```

### Build

```bash
# Create production build
npm run build

# Start production server
npm start
```

## 🛠️ Development Guidelines

### Component Creation

- Create components in the appropriate directory based on their scope
- Use the "use client" directive for client components
- Follow the existing component patterns

### API Integration

- Add new API services in `lib/api/` directory
- Use the existing API client for consistent error handling

### Styling

- Use Tailwind CSS for styling
- Use the `cn()` utility for conditional class names
- Follow the existing UI component patterns

### State Management

- Use Zustand stores in the `store/` directory
- Follow the existing store patterns

## 📜 License

This project is proprietary and confidential. All rights reserved.
