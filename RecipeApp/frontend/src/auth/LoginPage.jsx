import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { signInWithEmailAndPassword, createUserWithEmailAndPassword } from 'firebase/auth'
import { auth } from '../firebase'

export default function LoginPage() {
  const [isRegister, setIsRegister] = useState(false)
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const navigate = useNavigate()

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')
    try {
      if (isRegister) {
        await createUserWithEmailAndPassword(auth, email, password)
      } else {
        await signInWithEmailAndPassword(auth, email, password)
      }
      navigate('/recipes')
    } catch (err) {
      setError(err.message)
    }
  }

  return (
    <div className="container mt-5">
      <div className="row justify-content-center">
        <div className="col-md-6">
          <h1>Recipe App</h1>
          <h2>{isRegister ? 'Create Account' : 'Sign In'}</h2>

          {error && (
            <div className="alert alert-danger" role="alert">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label htmlFor="email">Email</label>
              <input
                id="email"
                type="email"
                className="form-control"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>
            <div className="form-group">
              <label htmlFor="password">Password</label>
              <input
                id="password"
                type="password"
                className="form-control"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
            </div>
            <button type="submit" className="btn btn-primary">
              {isRegister ? 'Create Account' : 'Sign In'}
            </button>
          </form>

          <p className="mt-3">
            {isRegister ? (
              <a
                href="#"
                onClick={(e) => {
                  e.preventDefault()
                  setIsRegister(false)
                }}
              >
                Already have an account? Sign In
              </a>
            ) : (
              <a
                href="#"
                onClick={(e) => {
                  e.preventDefault()
                  setIsRegister(true)
                }}
              >
                Need an account? Register
              </a>
            )}
          </p>
        </div>
      </div>
    </div>
  )
}
