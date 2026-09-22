import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom'
import { signOut } from 'firebase/auth'
import { auth } from '../firebase'
import { useAuth } from '../auth/AuthContext'
import ThemeToggle from './ThemeToggle'

function navLinkClass({ isActive }) {
  return isActive
    ? 'text-violet-700 dark:text-violet-400 font-medium text-sm'
    : 'text-gray-600 dark:text-gray-300 hover:text-violet-700 dark:hover:text-violet-400 text-sm'
}

export default function Layout() {
  const user = useAuth()
  const navigate = useNavigate()

  async function handleLogout() {
    await signOut(auth)
    navigate('/')
  }

  return (
    <>
      <nav className="sticky top-0 z-50 bg-white dark:bg-gray-900 shadow-sm dark:border-b dark:border-gray-800">
        <div className="max-w-7xl mx-auto px-4 flex items-center justify-between h-14">
          <Link to="/">
            <img src="/logo.png" alt="Queen of Code" width="50" height="35" />
          </Link>

          <div className="flex items-center gap-6">
            <NavLink to="/" className={navLinkClass}>
              Home
            </NavLink>
            <NavLink to="/about" className={navLinkClass}>
              About
            </NavLink>
            <NavLink to="/contact" className={navLinkClass}>
              Contact
            </NavLink>

            {user && (
              <NavLink to="/recipes" className={navLinkClass}>
                My Recipes
              </NavLink>
            )}

            {!user ? (
              <>
                <NavLink
                  to="/login"
                  className="text-gray-600 dark:text-gray-300 hover:text-violet-700 dark:hover:text-violet-400 text-sm"
                >
                  Register
                </NavLink>
                <NavLink
                  to="/login"
                  className="bg-violet-700 hover:bg-violet-800 text-white text-sm font-medium px-4 py-1.5 rounded-lg transition-colors"
                >
                  Login
                </NavLink>
              </>
            ) : (
              <>
                <ThemeToggle />
                <span className="text-sm text-gray-600 dark:text-gray-300">Hello {user.email}</span>
                <button
                  type="button"
                  className="text-sm text-gray-600 dark:text-gray-300 hover:text-violet-700 dark:hover:text-violet-400"
                  onClick={handleLogout}
                >
                  Logout
                </button>
              </>
            )}
          </div>
        </div>
      </nav>

      <main className="min-h-screen bg-gray-50 dark:bg-gray-950">
        <Outlet />
      </main>

      <footer className="bg-gray-100 dark:bg-gray-900 py-4">
        <div className="max-w-7xl mx-auto px-4 text-center text-sm text-gray-500 dark:text-gray-400">
          &copy; 2019 - Zeebee Technologies
        </div>
      </footer>
    </>
  )
}
