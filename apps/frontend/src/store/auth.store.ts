import create from 'zustand'

interface AuthStore {
  isAuthenticated: boolean
  user: User | null
  login: (credentials: Credentials) => Promise<void>
  logout: () => void
}

export const useAuthStore = create<AuthStore>((set) => ({
  isAuthenticated: false,
  user: null,
  login: async (credentials) => {
    // Implementation
  },
  logout: () => set({ isAuthenticated: false, user: null })
}))